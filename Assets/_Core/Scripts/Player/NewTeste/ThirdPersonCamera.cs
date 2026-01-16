using Unity.Cinemachine;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    private CinemachineCamera _virtualCamera;

    private void Awake()
    {
        _virtualCamera = FindFirstObjectByType<CinemachineCamera>();
        if (_virtualCamera == null)
        {
            Debug.LogError("Nenhuma Câmera Virtual da Cinemachine encontrada como filha!");
        }
    }

    public void SetTarget(Transform target)
    {
        if (_virtualCamera != null)
        {
            _virtualCamera.Follow = target;
            _virtualCamera.LookAt = target;
            Debug.Log($"Câmera agora está seguindo {target.name}");
        }
    }
}