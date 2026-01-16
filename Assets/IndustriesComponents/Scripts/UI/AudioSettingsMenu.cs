using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Voice.Unity;
using Photon.Voice;
using Photon.Voice.Fusion;
using Fusion.Addons.Touch.UI;

/**
 *
 * AudioSettingsMenu restores volume sliders with the audio setting manager values.
 * A listener is created for each slider in order to call the audio manager and save the new value.
 * It also creates a button for each microphone. The microphone button state is updated when it is selected by the user and then, saved in preference settings.
 * 
 **/

namespace Fusion.Samples.IndustriesComponents
{
    public class AudioSettingsMenu : VoiceComponent
    {
        public Slider masterVolume;
        public Slider voiceVolume;
        public Slider ambienceVolume;
        public Slider effectVolume;

        public RectTransform microphoneParent;
        public GameObject buttonPrefab;
        public GameObject labelPrefab;

        public FusionVoiceClient fusionVoiceClient;
        public Recorder recorder;
        public AudioSettingsManager audioSettingsManager;
        public Managers managers;

        private IDeviceEnumerator photonMicEnum;

        private void OnEnable()
        {
            if (managers == null) managers = Managers.FindInstance();
            if (fusionVoiceClient == null) fusionVoiceClient = managers.fusionVoiceClient;
            if (recorder == null) recorder = fusionVoiceClient.PrimaryRecorder;
            if (recorder == null) return;
            if (audioSettingsManager == null) audioSettingsManager = managers.audioSettingsManager;
            if (audioSettingsManager == null) { Debug.LogError("Audio Settings Manager not found"); return; }
            
            // A lógica de volume continua comentada como no seu original
            // ...

            // NOVO: Chama a rotina para definir o microfone inicial antes de criar os botões
            SetInitialMicrophone();
            CreateMicrophoneButtons();
        }

        /// <summary>
        /// NOVO: Este método define o microfone inicial a ser usado.
        /// Ele prioriza a escolha salva do usuário, mas usa o padrão do sistema como fallback.
        /// </summary>
        private void SetInitialMicrophone()
        {
            if (recorder == null) return;

#if !UNITY_WEBGL
            // Garante que o enumerador de dispositivos está pronto
            if (photonMicEnum == null)
            {
                photonMicEnum = Platform.CreateAudioInEnumerator(this.Logger);
            }
            photonMicEnum.Refresh();

            if (recorder.MicrophoneType == Recorder.MicType.Unity)
            {
                string savedMicName = PlayerPrefs.GetString("UnityMic");
                var devices = Microphone.devices;
                if (devices.Length == 0)
                {
                    Debug.LogWarning("Nenhum microfone da Unity encontrado.");
                    return;
                }

                // Tenta encontrar o microfone salvo
                if (!string.IsNullOrEmpty(savedMicName) && devices.Contains(savedMicName))
                {
                    recorder.MicrophoneDevice = new DeviceInfo(savedMicName);
                }
                else
                {
                    // Se não houver microfone salvo, usa o primeiro da lista da Unity como padrão
                    // (A API da Unity não expõe qual é o "padrão do sistema").
                    recorder.MicrophoneDevice = new DeviceInfo(devices[0]);
                }
            }
            else // Lógica para o microfone da Photon
            {
                int savedMicID = PlayerPrefs.GetInt("PhotonMic", -1);
                DeviceInfo? bestDevice = null; // <-- MUDANÇA 1: Adicionado '?'

                if (savedMicID != -1)
                {
                    bestDevice = photonMicEnum.FirstOrDefault(d => d.IDInt == savedMicID);
                }

                if (!bestDevice.HasValue) // <-- MUDANÇA 2: Checando com '.HasValue'
                {
                    bestDevice = photonMicEnum.FirstOrDefault(d => d.IsDefault);
                }
        
                if (!bestDevice.HasValue)
                {
                    bestDevice = photonMicEnum.FirstOrDefault();
                }

                if (bestDevice.HasValue)
                {
                    recorder.MicrophoneDevice = bestDevice.Value; // <-- MUDANÇA 3: Atribuindo com '.Value'
                }
                else
                {
                    Debug.LogWarning("Nenhum microfone da Photon encontrado.");
                }
            }
#endif
        }
        
        // MUDANÇA: Este método agora é mais simples. Ele apenas desenha a UI com base
        // no microfone que já foi definido em 'SetInitialMicrophone'.
        public void CreateMicrophoneButtons()
        {
            if (recorder == null) return;

            if (photonMicEnum != null)
            {
                photonMicEnum.Refresh();
            }
            DestroyAllChildren(microphoneParent);

#if (UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR
            return;
#endif

            List<DeviceInfo> devices = new List<DeviceInfo>();
#if !UNITY_WEBGL
            if (recorder.MicrophoneType == Recorder.MicType.Unity)
            {
                devices = Microphone.devices.Select(d => new DeviceInfo(d)).ToList();
            }
            else
            {
                if (photonMicEnum != null)
                {
                    devices = photonMicEnum.ToList();
                }
            }
#endif
            if (devices.Count == 0) return;

            // Encontra o índice do microfone que está atualmente selecionado no Recorder
            int selectedIndex = devices.FindIndex(d => d == recorder.MicrophoneDevice);

            // Instantiate microphone buttons
            for (int i = 0; i < devices.Count; ++i)
            {
                GameObject go;
                if (i == selectedIndex)
                {
                    go = Instantiate(labelPrefab, microphoneParent);
                }
                else
                {
                    int index = i;
                    go = Instantiate(buttonPrefab, microphoneParent);
                    var button = go.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.AddListener(() => OnInputDeviceChanged(index));
                    }
                }
                go.GetComponentInChildren<TextMeshProUGUI>().text = devices[i].Name;
            }
        }
        
        void OnInputDeviceChanged(int value)
        {
#if !UNITY_WEBGL
            if (recorder.MicrophoneType == Recorder.MicType.Unity)
            {
                var devices = Microphone.devices;
                if (value >= 0 && value < devices.Length)
                {
                    recorder.MicrophoneDevice = new DeviceInfo(devices[value]);
                    PlayerPrefs.SetString("UnityMic", recorder.MicrophoneDevice.Name);
                }
            }
            else
            {
                if (photonMicEnum != null)
                {
                    var device = photonMicEnum.ElementAtOrDefault(value);
                    if (device != null)
                    {
                        recorder.MicrophoneDevice = device;
                        PlayerPrefs.SetInt("PhotonMic", recorder.MicrophoneDevice.IDInt);
                    }
                }
            }

            if (recorder.RecordingEnabled)
            {
                recorder.RestartRecording();
            }
            CreateMicrophoneButtons();
#endif
        }
        
        public void DestroyAllChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; --i)
            {
                if(parent.GetChild(i) != null)
                {
                    Destroy(parent.GetChild(i).gameObject);
                }
            }
        }
    }
}