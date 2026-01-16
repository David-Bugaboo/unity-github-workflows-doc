using Unity.Cinemachine;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform playerTransform; // Arraste o objeto do jogador aqui
    public Transform lookAtTransform;
    public CinemachineCamera playerCamera;
    public CinemachineOrbitalFollow freeLookCamera;
    public float movementThreshold = 0.1f;

    private Vector3 lastPosition;

    void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Transform do jogador não atribu�do!");
            enabled = false;
            return;
        }

        if (freeLookCamera == null)
        {
            Debug.LogError("Cinemachine FreeLookCamera não atribuída!");
            enabled = false;
            return;
        }
        
        playerCamera.LookAt = lookAtTransform;
        lastPosition = playerTransform.position;
    }

    void Update()
    {
        Vector3 currentPosition = playerTransform.position;
        float distance = Vector3.Distance(currentPosition, lastPosition);

        if (distance > movementThreshold)
        {
            // Ativa o recentering usando as novas propriedades aninhadas
            freeLookCamera.HorizontalAxis.Recentering.Enabled = true; 
            freeLookCamera.HorizontalAxis.Recentering.Enabled = true;
        }
        else
        {
            // Desativa o recentering
            freeLookCamera.HorizontalAxis.Recentering.Enabled = false;
            freeLookCamera.HorizontalAxis.Recentering.Enabled = false;
        }

        lastPosition = currentPosition;
    }
}
