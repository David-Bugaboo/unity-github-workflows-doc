using Fusion.Samples.Stage;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Buga_PlayerController))]
public class BugaPlayerAnimation : MonoBehaviour
{
    [Header("---- Control ----")]
    public Buga_PlayerController controller;
    public Animator animator => controller.AvatarAnimator;
    public NavMeshAgent agent;
    public RuntimeAnimatorController animatorController;

    public AnimatorParamsNames animatorNames;


    [Header("---- Parameters ----")]
    [Tooltip("Maior = transicao mais rapida ao acelerar")]
    public float increaseSpeedSmooth = 8f;
    [Tooltip("Maior = transicao mais rapida ao desacelerar")]
    public float decreaseSpeedSmooth = 5f;

    [Header("---- Seating ----")]
    public StageHardwareRig hardwareRig;

    private NetworkMecanimAnimator _networkMecanimAnimator;

    private void Start()
    {
        if (controller == null) controller = GetComponent<Buga_PlayerController>();
    }

    private void Update()
    {
        if (animator != null)
        {
            float currentSpeed = animator.GetFloat(animatorNames.speedFloat);
            float newValue = Mathf.Lerp(currentSpeed, agent.velocity.magnitude, Time.deltaTime * (currentSpeed < agent.velocity.magnitude ? increaseSpeedSmooth : decreaseSpeedSmooth));

            animator.SetFloat(animatorNames.speedFloat, newValue);

            if (hardwareRig.seatStatus.seated)
            {
                animator.SetInteger(animatorNames.poseTypeInt, 1); // Pose sentado
            }
            else
            {
                animator.SetInteger(animatorNames.poseTypeInt, 0); // Pose de p�
            }
        }
    }

    public void ReactionPerform(int reactionType)
    {
        animator.SetInteger(animatorNames.reactionType, reactionType);
        // animator.SetTrigger(animatorNames.reactionTrigger);
        controller.MyNetworkPlayer.GetComponent<NetworkMecanimAnimator>().SetTrigger(animatorNames.reactionTrigger);
    }
}
