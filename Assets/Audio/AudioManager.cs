using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }
    

    public void PlaySFX(AudioClip audioClip, float volume = 1f)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = audioClip;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.Play();
        Destroy(source, audioClip.length + 0.1f);
    }

    public void PlaySFXAtPoint(AudioClip audioClip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        GameObject tempGO = new GameObject("TempAudio_Point");
        tempGO.transform.position = position;
        AudioSource source = Configure3DSource(tempGO, audioClip, volume, pitch);
        source.Play();
        Destroy(tempGO, audioClip.length + 0.1f);
    }
    public void PlaySFXOnTransform(AudioClip audioClip, Transform target, float volume = 1f, float pitch = 1f)
    {
        GameObject tempGO = new GameObject("TempAudio_Follow");
        tempGO.transform.SetParent(target);
        tempGO.transform.localPosition = Vector3.zero;

        AudioSource source = Configure3DSource(tempGO, audioClip, volume, pitch);
        source.Play();
        Destroy(tempGO, audioClip.length + 0.1f);
    }
    
    private AudioSource Configure3DSource(GameObject go, AudioClip clip, float vol, float p)
    {
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = vol;
        source.pitch = p;
        source.spatialBlend = 1.0f; // 3D
        source.minDistance = 1f;
        source.maxDistance = 25f;
        source.rolloffMode = AudioRolloffMode.Linear;
        return source;
    }

}