using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovementController : NetworkBehaviour, IPlayerLeft
{
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private float _moveSpeed = 6.0f;

    private Transform _cameraTransform;

    private void Start()
    {
        if (_navMeshAgent == null)
            _navMeshAgent = FindFirstObjectByType<NavMeshAgent>();
    }

    public override void Spawned()
    {
        // Desativamos o controle de posição e rotação do NavMeshAgent.
        // Faremos isso manualmente para ter um controle mais direto e responsivo.
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.updatePosition = true;

        if (Object.HasInputAuthority)
        {
            // Guarda a referência da câmera para o movimento ser relativo a ela
            _cameraTransform = Camera.main.transform;
            
            // Configura a câmera da Cinemachine para seguir este jogador
            FindObjectOfType<ThirdPersonCamera>()?.SetTarget(transform);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Só processa o input se tivermos autoridade sobre este objeto.
        if (GetInput(out NetworkInputData data))
        {
            // O movimento é aplicado diretamente no NavMeshAgent.
            // Como o NetworkTransform está sincronizando o transform, a posição será replicada.
            Vector3 moveDirection = data.direction.normalized;
            
            // Calcula a velocidade desejada
            _navMeshAgent.velocity = moveDirection * _moveSpeed;

            // Lógica de rotação: faz o personagem olhar para a direção em que está se movendo.
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Runner.DeltaTime * 10f);
            }
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (player == Object.InputAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}