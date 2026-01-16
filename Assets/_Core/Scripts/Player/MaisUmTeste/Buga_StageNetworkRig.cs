using Fusion;
using UnityEngine;

public class Buga_StageNetworkRig : NetworkBehaviour, IPlayerLeft
{
    [Header("Componentes")]
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _characterController;

    [Header("Configurações de Movimento")]
    [SerializeField] private float moveSpeed = 5.0f;

    // Propriedade de rede para sincronizar a velocidade da animação com outros jogadores
    [Networked]
    private float NetworkedSpeed { get; set; }

    private float _verticalVelocity = 0f; // Usado para aplicar gravidade

    private void Awake()
    {
        // Garante que temos as referências dos componentes
        if (_characterController == null) _characterController = GetComponent<CharacterController>();
        if (_animator == null) _animator = GetComponent<Animator>();
    }

    public override void Spawned()
    {
        // Este método é chamado quando o objeto é criado na rede.
        // Se este objeto pertence ao jogador local, configuramos a câmera para segui-lo.
        if (Object.HasInputAuthority)
        {
            FindObjectOfType<ThirdPersonCamera>()?.SetTarget(transform);
            Debug.Log("ThirdPersonNetworkCharacter Spawned e câmera configurada.");
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Só processa o input se tivermos autoridade sobre este objeto.
        if (HasInputAuthority == false)
        {
            return;
        }

        // Tenta obter o input para este jogador
        if (GetInput(out NetworkInputData data))
        {
            // Move o personagem
            Vector3 moveDirection = data.direction.normalized;
            _characterController.Move(moveDirection * moveSpeed * Runner.DeltaTime);

            // Aplica gravidade
            if (_characterController.isGrounded)
            {
                _verticalVelocity = -1f; // Força para baixo para manter no chão
            }
            else
            {
                _verticalVelocity += -9.81f * Runner.DeltaTime;
            }
            _characterController.Move(new Vector3(0, _verticalVelocity, 0) * Runner.DeltaTime);


            // Rotaciona o personagem para a direção do movimento
            if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }

            // Atualiza a velocidade para a animação
            float speed = new Vector3(_characterController.velocity.x, 0, _characterController.velocity.z).magnitude;
            NetworkedSpeed = speed; // Sincroniza a velocidade pela rede
        }
    }

    public override void Render()
    {
        // O Render é chamado a cada frame visual. É ideal para atualizar animações.
        // Atualizamos o Animator com a velocidade (local ou sincronizada pela rede).
        _animator.SetFloat("MoveSpeed", NetworkedSpeed);
    }
    
    // Método da interface IPlayerLeft para remover o personagem quando o jogador sai
    public void PlayerLeft(PlayerRef player)
    {
        if (player == Object.InputAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}