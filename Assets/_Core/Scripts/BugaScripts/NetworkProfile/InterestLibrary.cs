using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Interest Library", menuName = "Bugaboo/Mundo IEL/Interest Library")]
public class InterestLibrary : ScriptableObject
{
    public InterestConfig[] interests;
    public InterestConfig fallback;

    public InterestConfig FindInterest(string id)
    {
        InterestConfig found = interests.First(interest => interest.id == id);
        if(found != null) return found;
        return fallback;
    }
}