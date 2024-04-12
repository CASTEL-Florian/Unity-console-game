using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UnityConsole
{
    public class BeepPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        private float frequency;
        private readonly float sampleRate = 44100;
        private readonly float waveLengthInSeconds = 1f;

        private int timeIndex = 0;

        private void Start()
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0;
            audioSource.Stop();
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            for (int i = 0; i < data.Length; i += channels)
            {
                data[i] = CreateSine(timeIndex, frequency, sampleRate);

                if (channels == 2)
                    data[i + 1] = CreateSine(timeIndex, frequency, sampleRate);

                timeIndex++;

                //if timeIndex gets too big, reset it to 0
                if (timeIndex >= (sampleRate * waveLengthInSeconds))
                {
                    timeIndex = 0;
                }
            }
        }

        private float CreateSine(int timeIndex, float frequency, float sampleRate)
        {
            return Mathf.Sin(2 * Mathf.PI * timeIndex * frequency / sampleRate);
        }

        public async UniTask Beep(float frequency, float duration, CancellationToken cancellationToken = default)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Debug.LogWarning("Beep is not supported on WebGL");
            }

            this.frequency = frequency;
            timeIndex = 0; //resets timer before playing sound
            audioSource.Play();
            await UniTask.Delay((int)duration, cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            audioSource.Stop();
        }
    }
}