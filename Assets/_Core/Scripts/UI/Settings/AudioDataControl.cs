using UnityEngine;

public class AudioDataControl : MonoBehaviour
{
    static AudioDataControl instance;
    
    [SerializeField] AudioSystemController audioSys;
    public static AudioSystemController AudioController => instance.audioSys;
    private void Start() { instance = this; audioSys.LoadConfig(); }
    private void OnDestroy() => audioSys.SaveConfig();
}
