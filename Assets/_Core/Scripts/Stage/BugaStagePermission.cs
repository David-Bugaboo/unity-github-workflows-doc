using UnityEngine;
using UnityEngine.AI;

public class BugaStagePermission : MonoBehaviour
{
    [Tooltip("Arraste aqui o componente NavMesh Obstacle do próprio objeto.")]
    [SerializeField] private NavMeshObstacle navMeshObstacle;
    [SerializeField] private GameObject cube;
    
    void Start()
    {
        if (navMeshObstacle == null)
        {
            navMeshObstacle = GetComponent<NavMeshObstacle>();
        }

        // if (APIHandler.Instance.Role == "Admin")
        // {
        //     navMeshObstacle.gameObject.SetActive(false);
        //     cube.SetActive(true);
        // }
        // else
        // {
        //     navMeshObstacle.gameObject.SetActive(true);
        //     cube.SetActive(false);
        // }
    }
}