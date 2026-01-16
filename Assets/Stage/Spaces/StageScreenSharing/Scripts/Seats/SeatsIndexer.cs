using Fusion.XR.Shared.Rig;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Samples.Stage
{
    /**
     * Reference all seats in the room. Used by StageNetworkedRig when notified by a network var changed that an user is seated in a seat, described by its seat id
     */
    public class SeatsIndexer : MonoBehaviour
    {
        public Seat[] seats;
        public NetworkRunner runner;
        public Dictionary<string, Seat> seatsById = new Dictionary<string, Seat>();
        RigInfo rigInfo;
        
        private void Awake()
        {
            if (!runner) Debug.LogError("Missing runner");
            seats = GetComponentsInChildren<Seat>();
            rigInfo = RigInfo.FindRigInfo(runner);
            foreach (var seat in seats)
            {
                seat.rigInfo = rigInfo;
            }
        }

        private void Start()
        {
            foreach (var seat in seats)
            {
                seatsById[seat.id] = seat;
            }
        }

        public void DisabeInteraction()
        {
            foreach (var seat in seats)
            {
                seat.gameObject.SetActive(false);
                // seat.GetComponent<ClickableObject>().enabled = false;
            }
        }

        public void EnableInteraction()
        {
            foreach (var seat in seats)
            {
                seat.gameObject.SetActive(true);
                // seat.GetComponent<ClickableObject>().enabled = true;
            }
        }
    }

}
