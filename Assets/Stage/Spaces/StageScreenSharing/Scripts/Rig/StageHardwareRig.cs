using Fusion.XR.Shared.Rig;
using UnityEngine;
using UnityEngine.Events;

namespace Fusion.Samples.Stage
{
    [System.Serializable]
    public struct SeatState : INetworkStruct
    {
        public NetworkBool seated;
        public NetworkString<_32> seatId;
    }

    /**
     * Subclass of network rig, handling use a seat in the room, and decrease the network refresh rate (by updating data less often) when someone is seated
     */
    public class StageHardwareRig : HardwareRig
    {
        public bool freezeForRemoteClientsWhenSeated = true;
        public SeatState seatStatus;
        public float nextSeatRefresh = -1;
        public UnityEvent<Seat> onSeat = new UnityEvent<Seat>();
        public UnityEvent onUnseat = new UnityEvent();

        [Header("Avatar Layer Settings")]
        [SerializeField] private Camera targetCamera;
        private int _avatarLayerMask;

        private void Start()
        {
            int avatarLayer = LayerMask.NameToLayer("Avatar");
            if (avatarLayer != -1)
            {
                _avatarLayerMask = 1 << avatarLayer;
            }

            if (targetCamera == null)
            {
                targetCamera = GetComponentInChildren<Camera>();
            }
        }

        [ContextMenu("Seat")]
        public void Seat(Seat seat)
        {
            Debug.Log("Seating "+ seat.id);
            seatStatus.seated = true;
            seatStatus.seatId = seat.id;
            if (onSeat != null) onSeat.Invoke(seat);

            SetAvatarLayerVisible(false);

            if (Application.isMobilePlatform)
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
            }
        }

        public void SeatWatchingInDirection(Seat seat)
        {
            var headsetForward = headset.transform.forward;
            headsetForward.y = 0;
            var angle = Vector3.SignedAngle(headsetForward, seat.transform.forward, transform.up);
            Rotate(angle);
            Seat(seat);
        }

        [ContextMenu("Unseat")]
        public void Unseat()
        {
            Debug.Log("Unseating");
            seatStatus.seated = false;
            seatStatus.seatId = "";
            if (onUnseat != null) onUnseat.Invoke();

            SetAvatarLayerVisible(true);

            if (Application.isMobilePlatform)
            {
                Screen.orientation = ScreenOrientation.Portrait;
            }
        }

        private void SetAvatarLayerVisible(bool visible)
        {
            if (targetCamera == null || _avatarLayerMask == 0) return;

            if (visible)
            {
                targetCamera.cullingMask |= _avatarLayerMask;
            }
            else
            {
                targetCamera.cullingMask &= ~_avatarLayerMask;
            }
        }
    }

}
