using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

namespace Convention
{
    public abstract class BasicAudioSystem : MonoBehaviour
    {
        public abstract void LoadAudioClip(AudioClip clip);
        public abstract AudioClip GetAudioClip();
        public abstract float GetClockTime();

        public AudioClip CurrentClip
        {
            get => GetAudioClip();
            set => LoadAudioClip(value);
        }
        public float CurrentTime => GetClockTime();

        public abstract bool IsPlaying();

        public abstract void Stop();
        public abstract void Pause();
        public abstract void Play();

        public abstract void SetPitch(float pitch);
        public abstract void SetSpeed(float speed);
        public abstract void SetVolume(float volume);

        public void PrepareToOtherScene()
        {
            StartCoroutine(ClockOnJump());
            IEnumerator ClockOnJump()
            {
                transform.parent = null;
                DontDestroyOnLoad(gameObject);
                for (float now = 0; now < 1; now += UnityEngine.Time.deltaTime)
                {
                    this.SetVolume(1 - now);
                    yield return new WaitForEndOfFrame();
                }
                Destroy(gameObject);
            }
        }

        private void OnValidate()
        {
            if (samples.Length != spectrumLength) samples = new float[spectrumLength];
            if (bands.Length != BandCount) bands = new float[BandCount];
            if (freqBands.Length != BandCount) freqBands = new float[BandCount];
            if (bandBuffers.Length != BandCount) bandBuffers = new float[BandCount];
            if (bufferDecrease.Length != BandCount) bufferDecrease = new float[BandCount];
            if (bandHighest.Length != BandCount) bandHighest = new float[BandCount];
            if (normalizedBands.Length != BandCount) normalizedBands = new float[BandCount];
            if (normalizedBandBuffers.Length != BandCount) normalizedBandBuffers = new float[BandCount];
            if (sampleCount.Length != BandCount) sampleCount = new int[BandCount];
        }

        public void Sampling()
        {
            if (samples.Length != spectrumLength) samples = new float[spectrumLength];
            if (bands.Length != BandCount) bands = new float[BandCount];
            if (freqBands.Length != BandCount) freqBands = new float[BandCount];
            if (bandBuffers.Length != BandCount) bandBuffers = new float[BandCount];
            if (bufferDecrease.Length != BandCount) bufferDecrease = new float[BandCount];
            if (bandHighest.Length != BandCount) bandHighest = new float[BandCount];
            if (normalizedBands.Length != BandCount) normalizedBands = new float[BandCount];
            if (normalizedBandBuffers.Length != BandCount) normalizedBandBuffers = new float[BandCount];
            if (sampleCount.Length != BandCount) sampleCount = new int[BandCount];

            InjectSampling();
        }

        protected abstract void InjectSampling();

        public static AudioType GetAudioType(string path)
        {
            return Path.GetExtension(path) switch
            {
                "wav" => AudioType.WAV,
                "mp3" => AudioType.MPEG,
                "ogg" => AudioType.OGGVORBIS,
                _ => AudioType.UNKNOWN
            };
        }

        public void LoadOnResource(string path)
        {
            LoadAudioClip(Resources.Load<AudioClip>(path));
        }
        public void LoadOnUrl(string url)
        {
            LoadOnUrl(url, GetAudioType(url));
        }
        public void LoadOnUrl(string url, AudioType audioType)
        {
            StartCoroutine(LoadAudio(url, audioType));
        }
        public IEnumerator LoadAudio(string path, AudioType audioType)
        {
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);
                CurrentClip = audioClip;
            }
            else Debug.LogError(request.result);
        }

        public enum SpectrumLength
        {
            Spectrum64, Spectrum128, Spectrum256, Spectrum512, Spectrum1024, Spectrum2048, Spectrum4096, Spectrum8192
        }

        public enum BufferDecreasingType
        {
            Jump, Slide, Falling
        }

        public enum BufferIncreasingType
        {
            Jump, Slide
        }
        [Header("MusicSampler")]
        public SpectrumLength SpectrumCount = SpectrumLength.Spectrum256;
        protected int spectrumLength => (int)Mathf.Pow(2, ((int)SpectrumCount + 6));
        public float[] samples = new float[64];
        protected int[] sampleCount = new int[8];
        public uint BandCount = 8;
        public BufferDecreasingType decreasingType = BufferDecreasingType.Jump;
        public float decreasing = 0.003f;
        public float DecreaseAcceleration = 0.2f;
        public BufferIncreasingType increasingType = BufferIncreasingType.Jump;
        public float increasing = 0.003f;
        public float[] bands = new float[8];
        protected float[] freqBands = new float[8];
        protected float[] bandBuffers = new float[8];
        protected float[] bufferDecrease = new float[8];
        public float average
        {
            get
            {
                float average = 0;
                for (int i = 0; i < BandCount; i++)
                {
                    average += normalizedBands[i];
                }
                average /= BandCount;
                return average;
            }
        }

        protected float[] bandHighest = new float[8];
        public float[] normalizedBands = new float[8];
        protected float[] normalizedBandBuffers = new float[8];

        protected void GetSampleCount()
        {
            float acc = (((float)((int)SpectrumCount + 6)) / BandCount);
            int sum = 0;
            int last = 0;
            for (int i = 0; i < BandCount - 1; i++)
            {
                int pow = (int)Mathf.Pow(2, acc * (i));
                sampleCount[i] = pow - sum;
                if (sampleCount[i] < last) sampleCount[i] = last;
                sum += sampleCount[i];
                last = sampleCount[i];
            }
            sampleCount[BandCount - 1] = samples.Length - sum;
        }
    }

    public class AudioSystem : BasicAudioSystem
    {
        [SerializeField] private AudioSource source;

        public AudioSource Source
        {
            get
            {
                if (source == null)
                    source = this.SeekComponent<AudioSource>();
                if (source == null)
                    source = this.gameObject.AddComponent<AudioSource>();
                return source;
            }
            set
            {
                Stop();
                source = value;
            }
        }

        public AudioMixer Mixer = null;

        public override AudioClip GetAudioClip()
        {
            return Source.clip;
        }
        public override float GetClockTime()
        {
            return (float)Source.timeSamples / (float)Source.clip.frequency;
        }

        public override bool IsPlaying()
        {
            return Source.isPlaying;
        }

        public override void LoadAudioClip(AudioClip clip)
        {
            Source.clip = clip;
        }

        public override void Stop()
        {
            Source.Stop();
        }
        public override void Pause()
        {
            Source.Pause();
        }
        public override void Play()
        {
            Source.Play();
        }

        public override void SetPitch(float pitch)
        {
            Source.pitch = pitch;
        }
        public override void SetSpeed(float speed)
        {
            SetSpeed(speed, 1.0f, "Master", "MasterPitch", "PitchShifterPitch", true);
        }
        public override void SetVolume(float volume)
        {
            Source.volume = volume;
        }

        public void SetSpeed(float speed,
                             float TargetPitchValue,
                             string TargetGroupName,
                             string TargetPitch_Attribute_Name,
                             string TargetPitchshifterPitch_Attribute_Name,
                             bool IsIgnoreWarning = false)
        {
            if (Mixer != null)
            {
                if (TargetPitchValue > 0)
                {
                    Source.pitch = 1;
                    Mixer.SetFloat(TargetPitch_Attribute_Name, TargetPitchValue);
                    float TargetPitchshifterPitchValue = 1.0f / TargetPitchValue;
                    Mixer.SetFloat(TargetPitchshifterPitch_Attribute_Name, TargetPitchshifterPitchValue);
                    Source.outputAudioMixerGroup = Mixer.FindMatchingGroups(TargetGroupName)[0];
                }
                else
                {
                    Source.pitch = -1;
                    Mixer.SetFloat(TargetPitch_Attribute_Name, -TargetPitchValue);
                    float TargetPitchshifterPitchValue = -1.0f / TargetPitchValue;
                    Mixer.SetFloat(TargetPitchshifterPitch_Attribute_Name, TargetPitchshifterPitchValue);
                    Source.outputAudioMixerGroup = Mixer.FindMatchingGroups(TargetGroupName)[0];
                }
            }
            else if (IsIgnoreWarning == false)
            {
                Debug.LogWarning("you try to change an Audio's speed without AudioMixer, which will cause it to change its pitch");
                SetPitch(speed);
            }
        }

        private void GetSpectrums()
        {
            Source.GetSpectrumData(samples, 0, FFTWindow.Blackman);
        }


        private void GetFrequencyBands()
        {
            int counter = 0;
            for (int i = 0; i < BandCount; i++)
            {
                float average = 0;
                for (int j = 0; j < sampleCount[i]; j++)
                {
                    average += samples[counter] * (counter + 1);
                    counter++;
                }
                average /= sampleCount[i];
                freqBands[i] = average * 10;
            }
        }

        private void GetNormalizedBands()
        {
            for (int i = 0; i < BandCount; i++)
            {
                if (freqBands[i] > bandHighest[i])
                {
                    bandHighest[i] = freqBands[i];
                }
            }
        }

        private void GetBandBuffers(BufferIncreasingType increasingType, BufferDecreasingType decreasingType)
        {
            for (int i = 0; i < BandCount; i++)
            {
                if (freqBands[i] > bandBuffers[i])
                {
                    switch (increasingType)
                    {
                        case BufferIncreasingType.Jump:
                            bandBuffers[i] = freqBands[i];
                            bufferDecrease[i] = decreasing;
                            break;
                        case BufferIncreasingType.Slide:
                            bufferDecrease[i] = decreasing;
                            bandBuffers[i] += increasing;
                            break;
                    }
                    if (freqBands[i] < bandBuffers[i]) bandBuffers[i] = freqBands[i];
                }
                if (freqBands[i] < bandBuffers[i])
                {
                    switch (decreasingType)
                    {
                        case BufferDecreasingType.Jump:
                            bandBuffers[i] = freqBands[i];
                            break;
                        case BufferDecreasingType.Falling:
                            bandBuffers[i] -= decreasing;
                            break;
                        case BufferDecreasingType.Slide:
                            bandBuffers[i] -= bufferDecrease[i];
                            bufferDecrease[i] *= 1 + DecreaseAcceleration;
                            break;
                    }
                    if (freqBands[i] > bandBuffers[i]) bandBuffers[i] = freqBands[i]; ;
                }
                bands[i] = bandBuffers[i];
                if (bandHighest[i] == 0) continue;
                normalizedBands[i] = (freqBands[i] / bandHighest[i]);
                normalizedBandBuffers[i] = (bandBuffers[i] / bandHighest[i]);
                if (normalizedBands[i] > normalizedBandBuffers[i])
                {
                    switch (increasingType)
                    {
                        case BufferIncreasingType.Jump:
                            normalizedBandBuffers[i] = normalizedBands[i];
                            bufferDecrease[i] = decreasing;
                            break;
                        case BufferIncreasingType.Slide:
                            bufferDecrease[i] = decreasing;
                            normalizedBandBuffers[i] += increasing;
                            break;
                    }
                    if (normalizedBands[i] < normalizedBandBuffers[i]) normalizedBandBuffers[i] = normalizedBands[i];
                }
                if (normalizedBands[i] < normalizedBandBuffers[i])
                {
                    switch (decreasingType)
                    {
                        case BufferDecreasingType.Jump:
                            normalizedBandBuffers[i] = normalizedBands[i];
                            break;
                        case BufferDecreasingType.Falling:
                            normalizedBandBuffers[i] -= decreasing;
                            break;
                        case BufferDecreasingType.Slide:
                            normalizedBandBuffers[i] -= bufferDecrease[i];
                            bufferDecrease[i] *= 1 + DecreaseAcceleration;
                            break;
                    }
                    if (normalizedBands[i] > normalizedBandBuffers[i]) normalizedBandBuffers[i] = normalizedBands[i];
                }
                normalizedBands[i] = normalizedBandBuffers[i];
            }
        }

        private void BandNegativeCheck()
        {
            for (int i = 0; i < BandCount; i++)
            {
                if (bands[i] < 0)
                {
                    bands[i] = 0;
                }
                if (normalizedBands[i] < 0)
                {
                    normalizedBands[i] = 0;
                }
            }
        }

        protected override void InjectSampling()
        {
            GetSpectrums();
            GetFrequencyBands();
            GetNormalizedBands();
            GetBandBuffers(increasingType, decreasingType);
            BandNegativeCheck();

        }
        void Update()
        {
            float[] spectrum = new float[256];
            AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
            for (int i = 1; i < spectrum.Length - 1; i++)
            {
                Debug.DrawLine(new Vector3(i - 1, spectrum[i] + 10, 0), new Vector3(i, spectrum[i + 1] + 10, 0), Color.red);
                Debug.DrawLine(new Vector3(i - 1, Mathf.Log(spectrum[i - 1]) + 10, 2), new Vector3(i, Mathf.Log(spectrum[i]) + 10, 2), Color.cyan);
                Debug.DrawLine(new Vector3(Mathf.Log(i - 1), spectrum[i - 1] - 10, 1), new Vector3(Mathf.Log(i), spectrum[i] - 10, 1), Color.green);
                Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(spectrum[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log(spectrum[i]), 3), Color.blue);
            }
        }

    }
}
