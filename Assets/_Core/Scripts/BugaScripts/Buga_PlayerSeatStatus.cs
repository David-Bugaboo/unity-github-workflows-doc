using Fusion.Samples.Stage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buga_PlayerSeatStatus : MonoBehaviour
{
    public Buga_PlayerController controller;
    public StageHardwareRig rig;

    public MonoBehaviour[] disableWhenSeated;
    public GameObject[] deactivateWhenSeated;

    private void Update()
    {
        if (controller != null && rig != null)
        {
            // controller.controlState.canMove = !rig.seatStatus.seated;
        }

        foreach (var component in disableWhenSeated)
        {
            component.enabled = !rig.seatStatus.seated;
        }

        foreach (var component in deactivateWhenSeated)
        {
            component.SetActive(!rig.seatStatus.seated);
        }

        ClickableObject.canUserInteract = !rig.seatStatus.seated;
    }
}
