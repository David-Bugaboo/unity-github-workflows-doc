using Fusion.XR.Shared;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Samples.Stage
{
    /**
     * Synchornize the video player volume through a [Network] var containing the volume level
     * To be able to change this variable, and so to control the synchronized status, the player touching the control desk becomes the StateAuthority.
     */
    public class VideoVolume : NetworkBehaviour
    {
        [SerializeField] private AudioSource speaker;
        [SerializeField] private Slider videoVolumeSlider;
        private bool syncingSlider = false;

        [Networked]
        public float Volume { get; set; } = 0.8f;

        ChangeDetector changeDetector;

        private void Awake()
        {
            videoVolumeSlider.onValueChanged.AddListener(SliderPositionChanged);
        }

        public override void Spawned()
        {
            base.Spawned();
            changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            if (Object.StateAuthority == Runner.LocalPlayer)
            {
                Volume = speaker.volume;
            }
            videoVolumeSlider.value = Volume;
            UpdateSpeakerVolume();
        }


        public override void Render()
        {
            base.Render();

            foreach (var changedVarName in changeDetector.DetectChanges(this))
            {
                if (changedVarName == nameof(Volume))
                {
                    UpdateSpeakerVolume();
                }
            }
        }

        private void UpdateSpeakerVolume()
        {
            syncingSlider = true;
            speaker.volume = Volume;
            videoVolumeSlider.value = Volume;
            syncingSlider = false;
        }

        private async void SliderPositionChanged(float value)
        {
            if (!syncingSlider)
            {
                // slider updated by user using UI
                if (Object.StateAuthority != Runner.LocalPlayer)
                {
                    if (!await RequestAuthority()) return;
                }
                Volume = value;
            }
        }

        private async Task<bool> RequestAuthority()
        {
            if (!await Object.WaitForStateAuthority()) return false;
            return true;
        }
    }
}
