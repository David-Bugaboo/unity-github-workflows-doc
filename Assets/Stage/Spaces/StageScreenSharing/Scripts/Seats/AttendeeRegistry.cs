using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Samples.Stage
{
    public enum AttendeeStatus
    {
        Spectator,
        VoiceRequestingSpectator,
        MutedSpectator,
        RejectedVoiceSpectator,
        VoicedSpectator
    }

    public interface IAttendee
    {
        public NetworkBehaviourId Id { get; }
        public AttendeeStatus AttendeeStatus { get; set; }
        public PlayerRef AttendeePlayer { get; }
        public string AttendeeName { get; }
        public void ChangeAttendeeStatus(AttendeeStatus status);
    }

    public interface IAttendeeRegistryListener {
        public void OnAttendeeUpdate(AttendeeRegistry registry, IAttendee attendee);
        public void OnAttendeeRegister(AttendeeRegistry registry, IAttendee attendee);
        public void OnAttendeeUnregister(AttendeeRegistry registry, IAttendee attendee);
    }

    /**
     * Store a list of IATtendee, and broadcast to IAttendeeRegistryListener any AttendeeStatus change received
     * Note that this component is not synchornized over the network: the OnAttendeeUpdate call has to be made on each clients
     */
    public class AttendeeRegistry : NetworkBehaviour
    {
        public List<IAttendee> attendees = new List<IAttendee>();
        public List<IAttendeeRegistryListener> listeners = new List<IAttendeeRegistryListener>();

        #region Registration
        public void RegisterListener(IAttendeeRegistryListener listener)
        {
            if (!listeners.Contains(listener)) listeners.Add(listener);
        }
        public void UnRegisterListener(IAttendeeRegistryListener listener)
        {
            if (listeners.Contains(listener)) listeners.Remove(listener);
        }

        public void RegisterAttendee(IAttendee attendee)
        {
            if (!attendees.Contains(attendee))
            {
                attendees.Add(attendee);
                foreach (var listener in listeners)
                {
                    listener.OnAttendeeRegister(this, attendee);
                }
            }
        }
        public void UnRegisterAttendee(IAttendee attendee)
        {
            if (attendees.Contains(attendee))
            {
                attendees.Remove(attendee);
                foreach (var listener in listeners)
                {
                    listener.OnAttendeeUnregister(this, attendee);
                }
            }
        }
        #endregion

        public void OnAttendeeUpdate(IAttendee attendee)
        {
            Debug.Log($"OnAttendeeUpdate {attendee.AttendeePlayer} {attendee.Id} {attendee.AttendeeStatus}");
            foreach (var listener in listeners)
            {
                listener.OnAttendeeUpdate(this, attendee);
            }
        }
    }
}
