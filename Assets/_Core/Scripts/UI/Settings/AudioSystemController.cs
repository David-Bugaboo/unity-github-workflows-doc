using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu( menuName = "IEL/Audio system", fileName = "Audio System" )]
public class AudioSystemController : ScriptableObject 
{
    const string MAIN_AUDIO = "Music Volume", EFFECT_AUDIO = "Effects Volume";
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] float minVal;
    
    public float MusicAudioLevel => _data.MusicVolume;
    
    public float SFXAudioLevel => _data.SFXVolume;
    
    AudioConfig _data;
    public void SetMainVolume( float val ) => audioMixer.SetFloat(MAIN_AUDIO, GetVolume( _data.MusicVolume = val ) );
    
    public void SetSFXVolume( float val ) => audioMixer.SetFloat( EFFECT_AUDIO, GetVolume( _data.SFXVolume = val ) );
    
    float GetVolume( float val ) => Mathf.Log( Mathf.Max( val, minVal ) ) * 20;
    
    public void SaveConfig() => PrefsControl.SetCustomData( nameof( AudioConfig ), JsonUtility.ToJson( _data ) );
    
    public void LoadConfig() 
    {
        _data = JsonUtility.FromJson<AudioConfig>( PrefsControl.GetCustomData( nameof( AudioConfig ) ) ) ?? new();
        SetMainVolume( _data.MusicVolume );
        SetSFXVolume( _data.SFXVolume );
    }
    
    public AudioMixerGroup GetGroup( string group ) 
    {
        var groups = audioMixer.FindMatchingGroups( group );
        return groups?[0];
    }

    public class AudioConfig 
    {
        public float MusicVolume = 1, SFXVolume = 1;
    }
}
