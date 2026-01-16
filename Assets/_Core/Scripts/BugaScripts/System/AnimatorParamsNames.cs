using UnityEngine;

[CreateAssetMenu(fileName = "Animator Controller Names", menuName = "Bugaboo/Animator Controller Names")]
public class AnimatorParamsNames : ScriptableObject
{
    [Header("---- Body ----")]
    [Tooltip("Tipo de corpo do avatar:\n0 - Masculino\n1 - Feminino")]
    public string bodyTipeInt = "body/type";

    [Header("---- Motion ----")]
    public string speedFloat = "motion/speed";

    [Header("---- Reactions ----")]
    public string reactionTrigger = "reaction/trigger";
    [Tooltip("Tipo de anima��o de reac��o:\n1 - Like\n2 - Aplauso\n3 - Dislike")]
    public string reactionType = "reaction/type";

    [Header("---- Pose ----")]
    [Tooltip("Tipo de pose geral do avatar:\n0 - De p�\n1 - Sentado")]
    public string poseTypeInt = "pose/main_type";
    [Tooltip("Tipo de anima��o idle")]
    public string poseIdleTypeInt = "pose/main_type";
}
