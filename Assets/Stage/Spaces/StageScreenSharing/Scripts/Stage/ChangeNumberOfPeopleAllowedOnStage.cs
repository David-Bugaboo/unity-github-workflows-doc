using Fusion.Addons.AudioChatBubble;
using Fusion.XR.Shared;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Fusion.Samples.Stage
{
    /**
     * Synchronize the stage zone maxCapacity through a [Network] var containing its value
     * To be able to change this variable, and so to control the synchronized status, the player touching the control desk becomes the StateAuthority.
     */
    public class ChangeNumberOfPeopleAllowedOnStage : NetworkBehaviour
    {
        [SerializeField] private int maxPlayerOnStage = 10;
        [SerializeField] private ChatBubble stageChatBubble;
        [SerializeField] private TextMeshProUGUI numberOfPeopleAllowedOnStageTMP;
        private const string allowedOnStage = "Allowed on stage : ";

        [Networked]
        public int MaxCapacity { get; set; } = 0;

        ChangeDetector changeDetector;

        public override void Spawned()
        {
            base.Spawned();
            changeDetector = GetChangeDetector(ChangeDetector.Source.SnapshotFrom);
            OnMaxCapacityChanged();

            if (Object.StateAuthority == Runner.LocalPlayer)
            {
                MaxCapacity = stageChatBubble.capacity;
            }
            numberOfPeopleAllowedOnStageTMP.text = allowedOnStage + MaxCapacity.ToString();
        }

        public override void Render()
        {
            base.Render();
            foreach (var changedVar in changeDetector.DetectChanges(this))
            {
                if (changedVar == nameof(MaxCapacity))
                {
                    OnMaxCapacityChanged();
                }
            }
        }

        private async Task<bool> RequestAuthority()
        {
            if (!await Object.WaitForStateAuthority()) return false;
            return true;
        }


        private void OnMaxCapacityChanged()
        {
            stageChatBubble.capacity = MaxCapacity;
            numberOfPeopleAllowedOnStageTMP.text = allowedOnStage + MaxCapacity.ToString();
        }

        [ContextMenu("Increase number of allowed people")]
        public async void IncreaseNumberOfPeopleAllowedOnStage()
        {
            if (Object.StateAuthority != Runner.LocalPlayer)
            {
                if (!await RequestAuthority()) return;
            }

            if (MaxCapacity < maxPlayerOnStage)
            {
                MaxCapacity += 1;
            }
        }

        [ContextMenu("Decrease number of allowed people")]
        public async void DecreaseNumberOfPeopleAllowedOnStage()
        {
            if (Object.StateAuthority != Runner.LocalPlayer)
            {
                if (!await RequestAuthority()) return;
            }

            if (MaxCapacity > 1)
            {
                MaxCapacity -= 1;
            }
        }

    }
}
