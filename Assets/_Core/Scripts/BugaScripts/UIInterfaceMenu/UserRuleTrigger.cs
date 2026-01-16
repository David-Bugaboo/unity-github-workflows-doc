using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class UserRuleTrigger : MonoBehaviour
{

    public string[] rules;
    public UnityEvent onRuleMatch;
    public UnityEvent onRuleMiss;

    // Start is called before the first frame update
    void Start()
    {
        // if(rules.Contains(APIHandler.Instance.Role)){
        //     onRuleMatch.Invoke();
        // }else{
        //     onRuleMiss.Invoke();
        // }
    }
}
