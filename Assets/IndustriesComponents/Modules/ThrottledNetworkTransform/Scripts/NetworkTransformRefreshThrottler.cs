//#define DESACTIVATE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Tools;

#if DESACTIVATE
#else
namespace Fusion.Addons.PerformanceTools
{
    /**
     * Decrease the transmission rate of NT position/rotation when isThrottled is true, thus decreasing used bandwidth.
     * Provides a special interpolation for proxies (not for the state auth in this version) to compensate for the "on-purposely" corrupted data.
     * 
     * Handles position if the parent is a NetworkTransformRefreshThrottler too. Does not accept grandparent NetworkTransform (just one level is supported)
     * 
     * Does not work currently if the parent can change.
     * Does not work currently with simulated proxies.
     *  
     * In case of parented NetworkTransformRefreshThrottler components, IsThrottled should be active at the same time for all NetworkTransformRefreshThrottler components in the hierarchy
     * 
     * Important:
     *  Note some local extrapolation is required for the state authority, as the FUN data will be changed even for local interpolation
     */
    // We ensure to run after the NetworkTransform or NetworkRigidbody, to be able to override the interpolation target behavior in Render()
    [DefaultExecutionOrder(NetworkTransformRefreshThrottler.EXECUTION_ORDER)]
    public class NetworkTransformRefreshThrottler : NetworkBehaviour, IAfterTick
    {
        // We want to script to run late, to capture any change made other script before this one during FUN for the state auth
        public const int EXECUTION_ORDER = 100;

        [Networked]
        public NetworkBool IsThrottled { get; set; } = false;
        NetworkTransform networkTransform;
        NetworkTransformRefreshThrottler parentNTThrottler;

        float lastUpdateSentTime;
        [Header("Throttled transmission rate")]
        public float updateIntervalWhenThrottled = 0.5f;
        public float delayBeforeDecreasingrefreshRate = 1;

        [Header("Throttled interpolation")]
        public bool enableTimeBasedInterpolation = true;

        public bool debugThrottledInterpolation = false;
        Tick lastTickChangeWhenThrottled = -1;
        float lastThrottledInterpolationDataReceptionTime = -1;
        float lastNonThrottledRegularInterpolationTime = -1;
        Pose lastNonThrottledRegularInterpolationPose;
        Pose lastChangepose;
        Pose lastValidThrottledInterpolation;
        float interpolationMargin = -1;

        public struct PoseContainer : ICopiable<PoseContainer>
        {
            public Pose pose;

            public void CopyValuesFrom(PoseContainer source)
            {
                pose = source.pose;
            }
        }
        // Store the latest pose received while the player is throttled (and so does not send all expected data)
        public TimedRingbuffer<PoseContainer> throttledPoseRingBuffer = new TimedRingbuffer<PoseContainer>(2000);

        bool IsLocalUser => Object.HasStateAuthority;

        private void Awake()
        {
            networkTransform = GetComponent<NetworkTransform>();
            if (transform.parent)
            {
                parentNTThrottler = transform.parent.GetComponent<NetworkTransformRefreshThrottler>();
            }
        }

        public enum Status { 
            Undefined,
            // State auth
            NotThrottledStateAuthority,
            PendingThrottledStateAuthority,
            ThrottledStateAuthoritySendingData,
            ThrottledStateAuthorityReusingData,
            // Proxies
            NotThrottledProxy,
            PendingThrottledProxy,
            ThrottledProxy,
            ThrottledProxyInterpolated,
            ThrottledProxyPendingDataForInterpolation,
            ThrottledProxyMissingDataForInterpolation
        }

        public Status status = Status.Undefined;

        float nextThrottledRefresh = -1;
        Pose lastSharedLocalPose;
        float throttledStart = -1;

        [HideInInspector]
        public bool wasThrottledInterpolationAppliedThisFrame = false;
        [HideInInspector]
        public bool wasDataThrottlingInterpolationAppliedThisTick = false;

        public void AfterTick()
        {
            wasDataThrottlingInterpolationAppliedThisTick = false;
        }
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            HandleFUNDataThrottling();
        }

        public void HandleFUNDataThrottling()
        {
            if (wasDataThrottlingInterpolationAppliedThisTick) {
                return;
            }
            wasDataThrottlingInterpolationAppliedThisTick = true;

            // Make sure that the parent is properly reset at the expected position if it is throttled and reusing data
            if (parentNTThrottler)
            {
                parentNTThrottler.HandleFUNDataThrottling();
            }

            if (IsLocalUser && IsThrottled)
            {
                if (throttledStart == -1)
                {
                    throttledStart = Time.time;
                }
                if (nextThrottledRefresh == -1 || Time.time > nextThrottledRefresh)
                {
                    Pose localPose = new Pose { position = transform.position, rotation = transform.rotation };
                    if (transform.parent)
                    {
                        localPose.position = transform.parent.InverseTransformPoint(transform.position);
                        localPose.rotation = Quaternion.Inverse(transform.parent.rotation) * transform.rotation;
                    }
                    lastSharedLocalPose = localPose;
                    lastUpdateSentTime = Time.time;
                    if ((Time.time - throttledStart) > delayBeforeDecreasingrefreshRate)
                    {
                        // we should be throttled for long enough, the next refresh will have a decreased rate
                        nextThrottledRefresh = lastUpdateSentTime + updateIntervalWhenThrottled;
                        status = Status.ThrottledStateAuthoritySendingData;
                    }
                    else
                    {
                        status = Status.PendingThrottledStateAuthority;
                    }
                }
                else
                {
                    // We need to restore the last shared pose to prevent sending up to date data position on the network
                    Pose pose = new Pose { position = lastSharedLocalPose.position, rotation = lastSharedLocalPose.rotation };
                    if (transform.parent)
                    {
                        pose.position = transform.parent.TransformPoint(lastSharedLocalPose.position);
                        pose.rotation = transform.parent.rotation * lastSharedLocalPose.rotation;
                    }
                    status = Status.ThrottledStateAuthorityReusingData;
                    ApplyPose(pose);
                }
            }
            else
            {
                if (Object.HasStateAuthority)
                {
                    status = Status.NotThrottledStateAuthority;
                }
                nextThrottledRefresh = -1;
                throttledStart = -1;
            }
        }

        void ApplyPose(Pose pose)
        {
            transform.position = pose.position;
            transform.rotation = pose.rotation;
        }

        private void LateUpdate()
        {
            wasThrottledInterpolationAppliedThisFrame = false;
        }

        public override void Render()
        {
            base.Render();
            ApplyThrottledInterpolationAdaptation();
        }

        void ApplyThrottledInterpolationAdaptation()
        {
            if (wasThrottledInterpolationAppliedThisFrame)
            {
                return;
            }
            wasThrottledInterpolationAppliedThisFrame = true;

            bool prepareInterpolation = (IsLocalUser == false);
            if (prepareInterpolation)
            {
                if (parentNTThrottler)
                {
                    // Make sure the parent won't move again this frame, by moving it right now first
                    parentNTThrottler.ApplyThrottledInterpolationAdaptation();
                }

                var networkTransformsInterpolationPose = new Pose
                {
                    position = transform.position,
                    rotation = transform.rotation
                };
                if (lastThrottledInterpolationDataReceptionTime == -1)
                {
                    if (debugThrottledInterpolation) Debug.LogError("First render");
                    throttledPoseRingBuffer.Reset();
                    lastValidThrottledInterpolation = networkTransformsInterpolationPose;
                }
                if (IsThrottled)
                {
                    if (throttledStart == -1) throttledStart = Time.time;
                }
                else
                {
                    throttledStart = -1;
                }
                if (IsThrottled && (Time.time - throttledStart) > delayBeforeDecreasingrefreshRate)
                {
                    // Throttled interpolation
                    status = Status.ThrottledProxy;
                    ThrottledInterpolation();
                }
                else
                {
                    if (IsThrottled)
                    {
                        status = Status.NotThrottledProxy;
                    } 
                    else
                    {
                        status = Status.PendingThrottledProxy;

                    }
                    // Not throttled, or not for long enough yet: saving the received data in the ring buffer (to avoid starting later on the interpolation with an empty buffer)
                    var time = RenderPoseLastChangeTime(out var from, out var to);
                    if (debugThrottledInterpolation) Debug.LogError($"[Add:{time}||{from}-{to}] Store regular interpolation");
                    lastValidThrottledInterpolation = networkTransformsInterpolationPose;
                    lastThrottledInterpolationDataReceptionTime = time;
                    AddTimeBaseInterpolationEntry(networkTransformsInterpolationPose, time);
                    lastNonThrottledRegularInterpolationTime = time;
                    lastNonThrottledRegularInterpolationPose = networkTransformsInterpolationPose;
                    lastChangepose = networkTransformsInterpolationPose;
                }
            }
            else
            {
                // Local user will stutter, unless some extrapoaltion hass been applied in another component
            }
        }

        void AddTimeBaseInterpolationEntry(Pose pose, float time)
        {
            throttledPoseRingBuffer.Add(new PoseContainer { pose = pose }, time);
        }

        #region Throttled interpolation
        // Compute a new interpolation, based on time of insertion in the ring buffer, instead of the normal tick based interpolation
        void ThrottledInterpolation()
        {
            // If it is the first time we are Throttled and we never had a non throttled Render, we store the current data even if the user did not moved between from and to (we need a first reference data)
            bool forceChange = lastThrottledInterpolationDataReceptionTime == -1;

            bool toTickDataDidChange = PoseDidChangeInToTick(out var pose, out var fromTick, out var toTick, forceChange);
            float currentTime = Runner.RemoteRenderTime;
            if (toTickDataDidChange && (toTick != lastTickChangeWhenThrottled || forceChange))
            {
                // New data received (or force change)
                if (debugThrottledInterpolation) Debug.LogError($"[Add:{currentTime}||{fromTick}-{toTick}] New (forced:{forceChange}) interpolation data for {currentTime} (wait: {(forceChange ? 0 : (currentTime - lastThrottledInterpolationDataReceptionTime))})");
                AddTimeBaseInterpolationEntry(pose, currentTime);
                lastThrottledInterpolationDataReceptionTime = currentTime;
                lastTickChangeWhenThrottled = toTick;
                lastChangepose = pose;
            }

            if (interpolationMargin == -1) interpolationMargin = 2.5f * updateIntervalWhenThrottled;

            float delaySinceLastData = (currentTime - lastThrottledInterpolationDataReceptionTime);
            if (delaySinceLastData > (updateIntervalWhenThrottled * 1.5f) && lastThrottledInterpolationDataReceptionTime != -1)
            {
                // Time based interpolation starving: no data received (probably due to no movement). We emulate the reception of a "did not moved" state (not sent as not needed) by storing again the latest data
                AddTimeBaseInterpolationEntry(lastChangepose, lastThrottledInterpolationDataReceptionTime + updateIntervalWhenThrottled);
                if (debugThrottledInterpolation)
                {
                    Debug.LogError($"[Add:{lastThrottledInterpolationDataReceptionTime + updateIntervalWhenThrottled}||" +
                        $"{fromTick}-{toTick}] Starving interpolation: injecting for {lastThrottledInterpolationDataReceptionTime + updateIntervalWhenThrottled}" +
                        $"({lastThrottledInterpolationDataReceptionTime}+{updateIntervalWhenThrottled}) last data again delaySinceLastData={delaySinceLastData}");
                }
                lastThrottledInterpolationDataReceptionTime = currentTime;
            }

            if (enableTimeBasedInterpolation == false) return;

            float interpolationTime = currentTime - updateIntervalWhenThrottled - interpolationMargin;

            if (interpolationTime < lastNonThrottledRegularInterpolationTime)
            {
                // We just Throttled: we will wait that the Throttled interpolaiton catches up (which aims a bit in the past to compensate for the lack of received data) with the know regular interpoled time
                if (debugThrottledInterpolation)
                {
                    Debug.LogError($"=> {interpolationTime} < {lastNonThrottledRegularInterpolationTime} interpolationTime < lastNonThrottledRegularInterpolation");
                }
                ApplyPose(lastNonThrottledRegularInterpolationPose);
                status = Status.ThrottledProxyPendingDataForInterpolation;
            }
            else
            {
                var interpolation = throttledPoseRingBuffer.InterpolateInfo(interpolationTime);
                if (interpolation.status == InterpolationStatus.ValidFromTo)
                {
                    var fromPose = interpolation.from.pose;
                    var toPose = interpolation.to.pose;
                    var alpha = interpolation.alpha;
                    Pose interpolatedPose = fromPose.InterpolateTo(toPose, alpha);
                    ApplyPose(interpolatedPose);
                    lastValidThrottledInterpolation = interpolatedPose;
                    status = Status.ThrottledProxyInterpolated;
                }
                else
                {
                    if (debugThrottledInterpolation)
                    {
                        Debug.LogError($"Missing data for interpolation at {interpolationTime}" +
                            $"({currentTime} - {updateIntervalWhenThrottled} - {interpolationMargin}) " +
                            $"(last data: {delaySinceLastData}) => {interpolation.status} " +
                            $"from:{(interpolation.fromEntry != null ? interpolation.fromEntry?.time : "<missing>")} " +
                            $"to:{(interpolation.toEntry != null ? interpolation.toEntry?.time : "<missing>")}" +
                            $" [lastThrottledInterpolationDataReceptionTime:{lastThrottledInterpolationDataReceptionTime} toTick:{toTick} lastTickChangeWhenThrottled:{lastTickChangeWhenThrottled}]");
                    }
                    // No move after Throttled, so no new data to interpolate with
                    ApplyPose(lastValidThrottledInterpolation);
                    status = Status.ThrottledProxyMissingDataForInterpolation;
                }
            }

        }

        // Return the pose during the "to" tick, if any position changed between the from and to ticks
        bool PoseDidChangeInToTick(out Pose pose, out Tick changeFromTick, out Tick changeToTick, bool forceChange = false)
        {
            changeFromTick = default;
            changeToTick = default;
            bool poseDidChange = DidMove(networkTransform, out _, out var trspTo, out var rigChangeFromTick, out var rigChangeToTick);
            pose = default;

            if (forceChange || poseDidChange)
            {
                if (poseDidChange) { changeFromTick = rigChangeFromTick; changeToTick = rigChangeToTick; }
                pose = PoseFromToTRSPData(networkTransform, parentNTThrottler, trspTo);
                return true;
            }
            return false;
        }

        // Should be used during a Render phase
        // Return true if this NetworkTransform position or rotation changed between the "from" and "to" ticks used as references to compute rendering interpolation for this frame
        //  and set the NetworkTRSPData fromData and toData with the underlying NetworkTransform networked data
        bool DidMove(NetworkTransform nt, out NetworkTRSPData fromData, out NetworkTRSPData toData, out Tick fromTick, out Tick toTick)
        {
            bool didChange = false;
            fromData = default;
            toData = default;
            fromTick = default;
            toTick = default;
            if (nt.TryGetSnapshotsBuffers(out var from, out var to, out _))
            {
                fromData = from.ReinterpretState<NetworkTRSPData>();
                toData = to.ReinterpretState<NetworkTRSPData>();
                fromTick = from.Tick;
                toTick = to.Tick;
                var delta = Mathf.Abs((fromData.Position - toData.Position).magnitude);
                didChange = (fromData.Position != toData.Position && delta > 0.001f) || fromData.Rotation != toData.Rotation;
            }
            return didChange;
        }

        // The NetworkTRSPData contain local positions.
        // So to find the world positions, we have to compute the position in the parent referential, at the time of the to tick
        // To have the most accurate position, we use the parent position during the tick, instead of its current position (it is currently already an interpolation returned by its NetworkTrandsorm)
        // So to compute the world position, as we don't have a properly placed transform, we use a TRS matrix
        static Pose PoseFromToTRSPData(NetworkTransform nt, NetworkTransformRefreshThrottler parentThrottler, NetworkTRSPData trsp)
        {
            var pose = new Pose
            {
                position = trsp.Position,
                rotation = trsp.Rotation
            };

            // find parent position
            if (nt.transform.parent)
            {
                if (parentThrottler && parentThrottler.networkTransform.TryGetSnapshotsBuffers(out var from, out var to, out _))
                {
                    var toParentData = to.ReinterpretState<NetworkTRSPData>();
                    var parentPosition = toParentData.Position;
                    var parentRotation = toParentData.Rotation;
                    var parentScale = parentThrottler.transform.localScale;
                    if (parentThrottler.transform.parent)
                    {
                        parentPosition = parentThrottler.transform.parent.TransformPoint(parentPosition);
                        parentRotation = parentThrottler.transform.parent.rotation * parentRotation;
                    }
                    var parentMatrix = Matrix4x4.TRS(parentPosition, parentRotation, parentScale);
                    pose.position = parentMatrix.MultiplyPoint(pose.position);
                    pose.rotation = parentThrottler.transform.rotation * pose.rotation;
                }
                else
                {
                    pose.position = nt.transform.parent.TransformPoint(pose.position);
                    pose.rotation = nt.transform.parent.rotation * pose.rotation;
                }
            }
                
            return pose;
        }

        // Return the interpolation time of the latest pose change 
        float RenderPoseLastChangeTime(out Tick from, out Tick to)
        {
            float lastChangeTime = 0;
            from = default;
            to = default;
            if (networkTransform.TryGetSnapshotsBuffers(out var ntFrom, out var ntTo, out var alpha))
            {
                var time = ntFrom.Tick * Runner.DeltaTime + (ntFrom.Tick - ntTo.Tick) * Runner.DeltaTime * alpha;
                lastChangeTime = time;
                from = ntFrom.Tick;
                to = ntTo.Tick;
            }
            return lastChangeTime;
        }
        #endregion
    }

    public static class PoseInterpolationExtension
    {
        public static Pose InterpolateTo(this Pose fromPose, Pose toPose, float alpha)
        {
            Pose interpolatedPose = new Pose
            {
                position = Vector3.Lerp(fromPose.position, toPose.position, alpha),
                rotation = Quaternion.Slerp(fromPose.rotation, toPose.rotation, alpha)
            };
            return interpolatedPose;
        }
    }
}
#endif