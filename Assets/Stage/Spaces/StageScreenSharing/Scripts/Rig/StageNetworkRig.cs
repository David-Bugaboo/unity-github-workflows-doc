using Fusion.Addons.PerformanceTools;
using Fusion.Tools;
using Fusion.XR.Shared.Rig;
using UnityEngine;

namespace Fusion.Samples.Stage
{
    /**
     * Synchronize seat status 
     */
    // We ensure to run after the NetworkTransform or NetworkRigidbody, to be able to override the interpolation target behavior in Render()
    [DefaultExecutionOrder(NetworkRig.EXECUTION_ORDER)]
    public class StageNetworkRig : NetworkRig
    {
        [Networked]
        public SeatState SeatStatus { get; set; }

        StageHardwareRig stageHardwareRig;
        SeatsIndexer seatsIndexer;

        ChangeDetector renderChangeDetector;

        NetworkTransformRefreshThrottler rigThrottler;
        NetworkTransformRefreshThrottler headsetThrottler;
        NetworkTransformRefreshThrottler leftHandThrottler;
        NetworkTransformRefreshThrottler rightHandThrottler;

        protected override void Awake()
        {
            base.Awake();
            rigThrottler = networkTransform.GetComponent<NetworkTransformRefreshThrottler>();
            headsetThrottler = headset.GetComponent<NetworkTransformRefreshThrottler>();
            leftHandThrottler = leftHand.GetComponent<NetworkTransformRefreshThrottler>();
            rightHandThrottler = rightHand.GetComponent<NetworkTransformRefreshThrottler>();
        }

        bool TryDetectSeatChange(ChangeDetector changeDetector, out SeatState previousSeat, out SeatState currentSeat)
        {
            previousSeat = default;
            currentSeat = default;

            foreach (var changedNetworkedVarName in changeDetector.DetectChanges(this, out var previous, out var current))
            {
                if (changedNetworkedVarName == nameof(SeatStatus))
                {
                    var seatReader = GetPropertyReader<SeatState>(changedNetworkedVarName);
                    previousSeat = seatReader.Read(previous);
                    currentSeat = seatReader.Read(current);
                    return true;
                }
            }
            return false;
        }

        public override void Spawned()
        {
            base.Spawned();
            if (hardwareRig) stageHardwareRig = hardwareRig.GetComponent<StageHardwareRig>();
            seatsIndexer = FindObjectOfType<SeatsIndexer>();

            renderChangeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            LocalNetworkRigSeatStatusUpdate();

            if (Object.HasStateAuthority)
            {
                if(rigThrottler) rigThrottler.IsThrottled = SeatStatus.seated;
                if (leftHandThrottler) leftHandThrottler.IsThrottled = SeatStatus.seated;
                if (rightHandThrottler) rightHandThrottler.IsThrottled = SeatStatus.seated;
                if (headsetThrottler) headsetThrottler.IsThrottled = SeatStatus.seated;
            }
        }

        public override void Render()
        {
            base.Render();

            if (TryDetectSeatChange(renderChangeDetector, out var previousSeat, out var currentSeat))
            {
                if (previousSeat.seatId.ToString() != currentSeat.seatId.ToString())
                {
                    ChangeSeat(previousSeat, currentSeat);
                }
            }
        }

        void LocalNetworkRigSeatStatusUpdate()
        {
            if (IsLocalNetworkRig && stageHardwareRig && SeatStatus.Equals(stageHardwareRig.seatStatus) == false)
            {
                //Debug.LogError("Updating seat status");
                SeatStatus = stageHardwareRig.seatStatus;
            }
        }

        void ChangeSeat(SeatState previousStatus, SeatState newStatus)
        {
            if (previousStatus.seated)
            {
                if (seatsIndexer.seatsById.TryGetValue(previousStatus.seatId.ToString(), out var seat))
                {
                    seat.seatCollider.enabled = true;
                }
            }
            if (newStatus.seated)
            {
                if (seatsIndexer.seatsById.TryGetValue(newStatus.seatId.ToString(), out var seat))
                {
                    if (!IsLocalNetworkRig)
                    {
                        // We keep the collider activated for the local user, so that the SeatDetector can still detect when we leave the seat (it checks if the collider is still under the head)
                        seat.seatCollider.enabled = false;
                    }
                }
            }
        }
    }


}

