using UnityEngine;

[CreateAssetMenu(fileName = "EventsManager", menuName = "Scriptable Objects/EventsManager")]
public class EventsManager : ScriptableObject
{
    public static void SendSignal( string signal ) =>
        EventSignal.SendSignal( signal );
}
