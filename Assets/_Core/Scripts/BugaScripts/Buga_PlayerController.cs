using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using Fusion;
using UnityEngine.Events;
using Fusion.XR.Shared.Locomotion;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

[System.Serializable]
public class ControlState
{
    [Tooltip("Se esse componente deve controlar a movimenta��o")] public bool canMove = true;
    [Tooltip("Se esse componente deve controlar a c�mera")] public bool canLook = true;
    [Tooltip("Se esse componente deve controlar o Mecanim do avar a ele associado")] public bool controlMecanim = true;
    [Tooltip("Se esse componente deve controlar a movimenta��o do NetworkObject associado a ele")] public bool placeNetwork = true;
}

public class Buga_PlayerController : SimulationBehaviour, ILocomotionValidator, ILocomotionValidationHandler
{

    [SerializeField] PlayerInput mainInput;

    [Header("---- Control State ----")]
    public ControlState controlState;// completly USELESS

    [Header("---- Local References ----")]
    public CinemachineCamera cinemachineCamera;
    public BugaPlayerInput playerInput;
    public NavMeshAgent agent;

    [Header("---- Scene References ----")]
    
    public Camera referenceCamera;

    [Header("---- Networked ----")]
    [Tooltip("Animator do avatar (deve ser atualizado atrav�s do m�todo SetupAvatar)")]
    [SerializeField]
    private Animator _avatarAnimator;
    public Animator AvatarAnimator { get => _avatarAnimator; }

    [Tooltip("Transform do networkPlayer (deve ser atualizado atrav�s do m�todo SetupAvatar)")]
    [SerializeField]
    private NetworkObject _networkPlayer;
    public NetworkObject MyNetworkPlayer { get => _networkPlayer; }

    [Header("----- Parameters -----")]
    [Tooltip("Valor maior ou igual a 2 para a anima��o do StarterAssets")]
    public float walkSpeed = 3f;
    public float runSpeed = 3f;
    public float rotationSpeed = 10f;
    public Vector2 lookSensivity = Vector2.one;

    [Header("---- Events ----")]
    public UnityEvent<GameObject> onSetupAvatar;

    public CinemachineInputAxisController CinemachineInputAxisController;
    
    [Header("---- Câmera e Input ----")]
    [Tooltip("ARRASTE AQUI O COMPONENTE 'ManualInputController' da sua câmera.")]
    [SerializeField] private ManualInputController manualInputController;
    [Tooltip("Sensibilidade para o input de look.")]
    public Vector2 lookSensitivity = Vector2.one;

    void Start()
    {
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.acceleration = 100f;
            agent.angularSpeed = 0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (controlState.controlMecanim) SetupAgent();
        if(controlState.canMove) MovePlayer();
        if (controlState.placeNetwork && _networkPlayer != null)
        {
            _networkPlayer.transform.position = transform.position;
            _networkPlayer.transform.rotation = transform.rotation;
        }

    }

    void LateUpdate()
    {
        if (controlState.canLook)
        {
            Vector2 rawInput = playerInput.look;
            Vector2 finalLookValue = rawInput * lookSensitivity;
            manualInputController.SetLookInput(finalLookValue);
        }
    }

    public override void FixedUpdateNetwork()
    {
        Debug.Log("AQUI");
    }

    void SetupAgent()
    {
        agent.speed = playerInput.sprint ? runSpeed : walkSpeed;
    }

    void MovePlayer()
    {
        Vector3 finalDir = referenceCamera.transform.right * playerInput.move.x + referenceCamera.transform.forward * playerInput.move.y;
        finalDir.y = 0;
        finalDir = finalDir.normalized;

        if (playerInput.move.sqrMagnitude > 0.01f)
        {
            agent.velocity = finalDir * agent.speed;

            Quaternion targetRotation = Quaternion.LookRotation(finalDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            agent.velocity = Vector3.zero;
        }
    }

    private void RotateCamera()
    {
        foreach (var c in CinemachineInputAxisController.Controllers)
        {
            if (c.Name == "Look Orbit X") c.InputValue = playerInput.look.x;
            if (c.Name == "Look Orbit Y") c.InputValue = playerInput.look.y;
        }
            
        // if (playerInput.move.sqrMagnitude > 0.01f)
        // {
        //     var playerForward = transform.forward;
        //     var targetPosition = CinemachineCameraTarget.transform.position + playerForward * MoveSpeed * Time.deltaTime;
        //         
        //     CinemachineCameraTarget.transform.position = Vector3.Lerp(
        //         CinemachineCameraTarget.transform.position,
        //         targetPosition,
        //         0.1f * Time.deltaTime
        //     );
        // }
    }
    
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }


    public void AssignNetworkPlayer(NetworkObject networkObject)
    {
        _networkPlayer = networkObject;
    }

    internal void SetupAvatarController(GameObject avatar)
    {
        _avatarAnimator = avatar.GetComponentInChildren<Animator>();
        if (_avatarAnimator == null) { Debug.LogWarning($"no Animator component found on avatar.name.", avatar); }
        onSetupAvatar.Invoke(avatar);
    }

    public bool CanMoveHeadset(Vector3 headserNewPosition)
    {
        return true;
    }

    public void OnDidMove()
    {
        //Debug.Log("nsdlvndovdasvds");
    }

    public void OnDidMoveFadeFinished()
    {
    }

    public void EnableMove( bool enable ) => controlState.canMove = enable;
    public void DisableMove( bool enable ) => controlState.canMove = !enable;
}
