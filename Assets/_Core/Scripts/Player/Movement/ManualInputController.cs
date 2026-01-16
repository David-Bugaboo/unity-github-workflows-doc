using System;
using Unity.Cinemachine;
using UnityEngine;
using Object = UnityEngine.Object;

public class ManualInputController : InputAxisControllerBase<ManualInputController.ManualReader>
{
    // NOVO: O valor de input agora é uma variável pública no Controller principal.
    // O HideInInspector é para não poluir o Inspector, já que ele é controlado por script.
    [HideInInspector]
    public Vector2 LookInput;

    // A função Update continua sendo necessária para chamar o método da classe base.
    protected void Update()
    {
        if (Application.isPlaying)
            UpdateControllers();
    }
    
    /// <summary>
    /// Função pública para que outros scripts possam "injetar" o valor de rotação.
    /// Agora, ela simplesmente atualiza a variável local.
    /// </summary>
    public void SetLookInput(Vector2 lookValue)
    {
        this.LookInput = lookValue;
    }

    /// <summary>
    /// O "Reader" agora é mais simples. Ele não armazena mais nenhum valor,
    /// apenas sabe como pegar o valor do seu Controller pai.
    /// </summary>
    [Serializable]
    public class ManualReader : IInputAxisReader
    {
        /// <summary>
        /// Esta é a função principal. O Cinemachine a chama para cada eixo.
        /// </summary>
        /// <param name="context">O objeto que está pedindo o valor (neste caso, nosso ManualInputController).</param>
        /// <param name="hint">A dica que nos diz se o Cinemachine está pedindo pelo eixo X, Y ou Z.</param>
        public float GetValue(Object context, IInputAxisOwner.AxisDescriptor.Hints hint)
        {
            // 1. Converte o 'context' para o tipo do nosso Controller pai.
            var controller = context as ManualInputController;
            if (controller == null)
                return 0; // Se não conseguir encontrar o controller, retorna 0 por segurança.

            // 2. Pega o valor diretamente da variável pública do controller.
            Vector2 lookValue = controller.LookInput;

            // 3. Retorna o componente correto (x ou y) com base na dica do Cinemachine.
            if (hint == IInputAxisOwner.AxisDescriptor.Hints.X)
                return lookValue.x;
            
            if (hint == IInputAxisOwner.AxisDescriptor.Hints.Y)
                return lookValue.y;

            return 0;
        }
    }
}
