using UnityEngine;
using System.Collections;

[System.Serializable]
public class SoundBuga
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    [Range(0f, 10f)]
    public float delayTime = 0f;
}

public class SoundManagerBuga : MonoBehaviour
{
    public SoundBuga[] sounds;
    public AudioSource audioSource;
    private Coroutine currentSoundCoroutine;

    public void PlaySound(string name)
    {
        // Find the sound by name
        SoundBuga sound = System.Array.Find(sounds, s => s.name == name);
        if (sound != null)
        {
            // Stop the current sound coroutine if it's playing
            if (currentSoundCoroutine != null)
            {
                StopCoroutine(currentSoundCoroutine);
            }

            StartCoroutine(PlayDelayedSound(sound));
        }
        else
        {
            Debug.LogWarning("Sound with name " + name + " not found!");
        }
    }

    private IEnumerator PlayDelayedSound(SoundBuga sound)
    {
        yield return new WaitForSeconds(sound.delayTime);

        audioSource.clip = sound.clip;
        audioSource.volume = sound.volume;
        audioSource.pitch = sound.pitch;
        audioSource.Play();

        // Store the current sound coroutine
        currentSoundCoroutine = StartCoroutine(WaitForSoundCompletion());
    }

    private IEnumerator WaitForSoundCompletion()
    {
        // Wait until the current sound finishes playing
        while (audioSource.isPlaying)
        {
            yield return null;
        }

        // Reset the current sound coroutine
        currentSoundCoroutine = null;
    }
}