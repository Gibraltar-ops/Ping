using UnityEngine;

public class AudioDetection : MonoBehaviour
{
    public int sampleWindow = 64;
    private AudioClip microphoneClip;

    void Start()
    {
        MicrophoneToAudioClip();
    }

    public void MicrophoneToAudioClip()
    {

        //Get first microphone in devices list
        string microphoneName = Microphone.devices[0];
        microphoneClip = Microphone.Start(microphoneName, true, 20, AudioSettings.outputSampleRate);
    }
    
    public float GetLoudnessFromMicrophone()
    {
        return GetLoudness(Microphone.GetPosition(Microphone.devices[0]), microphoneClip);
    }

    public float GetLoudness(int clipPosition, AudioClip clip)
    {
        int startPosition = clipPosition - sampleWindow;

        if (startPosition < 0)
            return 0;


        float[] waveData = new float[sampleWindow];
        clip.GetData(waveData, startPosition);

        // Compute loudness

        float totalLoudness = 0;

        for (int i = 0; i < sampleWindow; i++)
        {
            totalLoudness += Mathf.Abs(waveData[i]);
        }

        return totalLoudness / sampleWindow;
    }
}
