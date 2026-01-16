using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventSignal : Multiton<EventSignal>
{
    [SerializeField] private List<Signal> signals;
    private Dictionary<string, UnityEvent> events;

    protected override void Awake()
    {
        base.Awake();
        events = new Dictionary<string, UnityEvent>();
        foreach (var signal in signals)
        {
            events.Add(signal.Name, signal.Event);
        }
    }

    public static void SendSignal(string name)
    {
        foreach (var instance in _instances)
        {
            if (instance.events.TryGetValue(name, out var sig))
            {
                sig?.Invoke();
            }
        }
    }
    
    [Serializable]
    public class Signal
    {
        public string Name;
        public UnityEvent Event;
    }
}

