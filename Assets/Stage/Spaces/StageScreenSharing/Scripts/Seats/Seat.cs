using Fusion.Addons.Hover;
using Fusion.XR.Shared.Rig;
using System.Collections;
using UnityEngine;

namespace Fusion.Samples.Stage
{
    /**
     * Room seat: triggers seating when a beam release on it occurs
     */
    public class Seat : MonoBehaviour
    {
        public string id = "";
        public Collider seatCollider;
        public BeamHoverable beamHoverable;

        public RigInfo rigInfo;
        StageHardwareRig rig;
        private void Awake()
        {
            seatCollider = GetComponentInChildren<Collider>();
            beamHoverable = GetComponentInChildren<BeamHoverable>();
            beamHoverable.onBeamRelease.AddListener(OnBeamRelease);
            CreateId();
        }

        void CreateId()
        {

            var idPart = transform;
            id = name;
            while (idPart.parent)
            {
                idPart = idPart.parent;
                id = idPart.name + "/" + id;
            }
            id = id.Replace("CinemaLevel/Seats/", "");
            id = id.Replace("Level", "L");
            id = id.Replace("Seat", "S");
            id = id.Replace("(", "");
            id = id.Replace(")", "");
            id = id.Replace(" ", "");
        }

        private void OnBeamRelease()
        {
            Debug.Log("Seat '" + gameObject.name + "' recebeu OnBeamRelease. Tentando sentar o rig local.");
            if (rig == null)
            {
                rig = (StageHardwareRig)rigInfo.localHardwareRig;
            }
            StartCoroutine(SeatCoroutine());
        }

        IEnumerator SeatCoroutine()
        {
            if (rig.headset.fader) yield return rig.headset.fader.FadeIn();
            rig.Teleport(transform.position);
            Debug.Log("Seating after a beam release");
            rig.SeatWatchingInDirection(this);
            if (rig.headset.fader) yield return rig.headset.fader.WaitBlinkDuration();
            if (rig.headset.fader) yield return rig.headset.fader.FadeOut();

        }
    }

}
