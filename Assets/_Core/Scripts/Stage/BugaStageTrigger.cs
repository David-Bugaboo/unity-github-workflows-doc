using System;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

public class BugaStageTrigger : MonoBehaviour
{
    public UnityEvent OnAdminEnteredStage;
    public UnityEvent OnAdminLeftStage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<PlayerInfo>();
            if (player.Object.HasInputAuthority)
            {
                Debug.Log("Admin local entrou na zona da UI. Ativando painel.");
                OnAdminEnteredStage?.Invoke();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<PlayerInfo>();
            if (player.Object.HasInputAuthority)
            {
                Debug.Log("Admin local saiu da zona da UI. Desativando painel.");
                OnAdminLeftStage?.Invoke();
            }
        }
    }
}
