using UnityEngine;
using UnityEngine.UI;

public class UI_AudioController : MonoBehaviour
{
    [SerializeField] Slider mainAudio, sfxAudio;
    private void Awake() {
        mainAudio.value = AudioDataControl.AudioController.MusicAudioLevel;
        sfxAudio.value = AudioDataControl.AudioController.SFXAudioLevel;
    }
}
