using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Multiton<T> : MonoBehaviour where T : Multiton<T>
{
    public static HashSet<T> _instances;
    
    protected virtual void Awake()
    {
        _instances ??= new();
        _instances.Add((T)this);
    }

    protected void Start()
    {
        if(_instances.All(c => c != (T)this)) _instances.Add((T)this);
    }

    private void OnDestroy()
    {
        _instances.Remove((T)this);
    }
}