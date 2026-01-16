using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Samples.Stage
{
    public enum ScreenShareStatus
    {
        NoScreenShareRequested,     // no request yet
        ScreenShareCanceled,        // request canceled on the recorder UI
        ScreenShareRequested,       // request done, waiting for answer
        ScreenShareInProgress,      // screensharing in progress
        ScreenShareStopped,         // screensharing stopped (by the speaker using the desk, or using the recorder UI)
        ScreenShareRejected         // screensharing rejected by the speaker
    }

    public interface IScreenShare
    {
        public NetworkBehaviourId Id { get; }
        public ScreenShareStatus ScreenShareStatus { get; set; }
        public PlayerRef ScreenSharePlayer { get; }
        public string ScreenShareName { get; }
        public void ChangeScreenShareStatus(ScreenShareStatus status);
    }

    public interface IScreenShareRegistryListener
    {
        public void OnScreenShareUpdate(ScreenShareRegistry registry, IScreenShare screenShare);
        public void OnScreenShareUnregister(ScreenShareRegistry registry, IScreenShare screenShare);
    }

    /**
     * 
     * Store a list of IScreenShare, and broadcast to IScreenShareRegistryListener any ScreenShareStatus change received
     * Note that this component is not synchronized over the network: the OnScreenShareUpdate call has to be made on each clients
     * 
     **/
    public class ScreenShareRegistry : NetworkBehaviour
    {
        //public Zone voiceZone;
        public List<IScreenShare> screenShares = new List<IScreenShare>();
        public List<IScreenShareRegistryListener> listeners = new List<IScreenShareRegistryListener>();

        #region Registration
        public void RegisterListener(IScreenShareRegistryListener listener)
        {
            if (!listeners.Contains(listener)) listeners.Add(listener);
        }
        public void UnRegisterListener(IScreenShareRegistryListener listener)
        {
            if (listeners.Contains(listener)) listeners.Remove(listener);
        }

        public void RegisterScreenShare(IScreenShare screenShare)
        {
            if (!screenShares.Contains(screenShare)) screenShares.Add(screenShare);
        }
        public void UnRegisterScreenShare(IScreenShare screenShare)
        {
            if (screenShares.Contains(screenShare))
            {
                screenShares.Remove(screenShare);
                foreach (var listener in listeners)
                {
                    listener.OnScreenShareUnregister(this, screenShare);
                }
            }
        }
        #endregion

        public void OnScreenShareUpdate(IScreenShare screenShare)
        {
            Debug.Log($"OnScreenShareUpdate {screenShare.ScreenSharePlayer} {screenShare.Id} {screenShare.ScreenShareStatus}");
            foreach (var listener in listeners)
            {
                listener.OnScreenShareUpdate(this, screenShare);
            }
        }
    }
}
