using UnityEngine;

namespace Fusion.Samples.Stage
{
    /**
     * Detect when the user teleport out of a seat
     * Also detect when a desktop user walks over a seat (it won't trigger on a regular - not a DesktopController keyboard move - teleport, as the seat collider prevent pointing at the ground under a seat)
     */
    public class SeatDetector : MonoBehaviour
    {
        public StageHardwareRig rig;
        public LayerMask seatLayerMask = 0;
        public GameObject seatGameObject;
        public Seat seat;

        bool allowSeatOnTeleport = true;

        private void Awake()
        {
            rig = GetComponent<StageHardwareRig>();
            rig.onTeleport.AddListener(OnTeleport);
            if (seatLayerMask == 0)
                Debug.LogError("SeatDetector: for seating to be possible, at least one layer has to be added to seatLayerMask, and used on seat surface colliders");
        }

        void OnTeleport(Vector3 previousPosition, Vector3 newPosition)
        {
            var ray = new Ray(rig.headset.transform.position, -rig.transform.up);
            RaycastHit hit;
            if (Physics.Raycast(rig.headset.transform.position, -rig.transform.up, out hit, maxDistance: 3, seatLayerMask))
            {
                // Over a seat
                if (allowSeatOnTeleport)
                {
                    seatGameObject = hit.collider.gameObject;
                    seat = seatGameObject.GetComponent<Seat>();
                    if (rig.seatStatus.seated && seat.id == rig.seatStatus.seatId)
                    {
                        // Same seat
                        return;
                    }

                    if (Vector3.Distance(previousPosition, newPosition) > 1f)
                    {
                        Debug.Log("Seating after a teleport (long distance)");
                        rig.SeatWatchingInDirection(seat);
                    }
                    else
                    {
                        // For small moves, we don't force to rotate
                        Debug.Log("Seating after a teleport (short distance)");
                        rig.Seat(seat);
                    }
                }
            } 
            else
            {
                if (rig.seatStatus.seated)
                {
                    rig.Unseat();
                    seat = null;
                }
            }
        }
    }
}
