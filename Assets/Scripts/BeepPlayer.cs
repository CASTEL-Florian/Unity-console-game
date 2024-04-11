using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BeepPlayer : MonoBehaviour
{
    private float frequency;
    
    private float sampleRate = 44100;
    private float waveLengthInSeconds = 1f;
 
    [SerializeField] private AudioSource audioSource;
    int timeIndex = 0;
 
    float currentTime = 0;
    void Start()
    {
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0;
        audioSource.Stop();
    }
   
    void OnAudioFilterRead(float[] data, int channels)
    {
        for(int i = 0; i < data.Length; i+= channels)
        {          
            data[i] = CreateSine(timeIndex, frequency, sampleRate);
           
            if(channels == 2)
                data[i+1] = CreateSine(timeIndex, frequency, sampleRate);
           
            timeIndex++;
           
            //if timeIndex gets too big, reset it to 0
            if(timeIndex >= (sampleRate * waveLengthInSeconds))
            {
                timeIndex = 0;
            }
        }
    }
    
    public float CreateSine(int timeIndex, float frequency, float sampleRate)
    {
        return Mathf.Sin(2 * Mathf.PI * timeIndex * frequency / sampleRate);
    }
    
    public async UniTask Beep(float frequency, float duration)
    {
        Debug.Log("Beep: " + frequency + " " + duration);
        this.frequency = frequency;
        timeIndex = 0;  //resets timer before playing sound
        audioSource.Play();
        await UniTask.Delay((int)duration);
        audioSource.Stop();
    }
}
