using System;
using System.Collections;
using UnityEngine;
using Unity.Collections;

namespace Convention
{
    /// <summary>
    /// 频谱图渲染器 - 横轴时间，纵轴频率，颜色深度表示声音强度
    /// </summary>
    public class SpectrogramRenderer : MonoBehaviour
    {
        [Header("Audio Source")]
        public AudioClip clip;
        public int Step = 100;
        
        [Header("Render Texture")]
        public RenderTexture spectrogramTexture;

        [Header("Display Settings")]
        public Color lowIntensityColor = Color.black;
        public Color midIntensityColor = Color.red;
        public Color highIntensityColor = Color.white;
        public float PowMode = 5f;

        private Texture2D bufferTexture;

        void Start()
        {
            InitializeSpectrogramTexture();
            if (clip != null)
            {
                StartCoroutine(ProcessAudioClip());
            }
        }

        void InitializeSpectrogramTexture()
        {
            bufferTexture = new Texture2D(spectrogramTexture.width, spectrogramTexture.height, TextureFormat.ARGB32, false);
            
            // 初始化为黑色
            Color[] pixels = new Color[spectrogramTexture.width * spectrogramTexture.height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.black;
            }
            bufferTexture.SetPixels(pixels);
            bufferTexture.Apply();
        }

        IEnumerator ProcessAudioClip()
        {
            var samplesCount = clip.samples;
            Debug.Log("ProcessAudioClip Start", this);

            var data = new float[spectrogramTexture.width + 10][];
            for (int i = 0; i < spectrogramTexture.width + 10; i++)
            {
                data[i] = new float[spectrogramTexture.height];
                for (int j = 0; j < spectrogramTexture.height; j++)
                    data[i][j] = -1;
            }

            for (int i = 0; i < samplesCount; )
            {
                var current = new float[spectrogramTexture.height];
                int x = Mathf.FloorToInt(i * (spectrogramTexture.width / (float)samplesCount));
                clip.GetData(current, i);
                for (int j = 0; j < spectrogramTexture.height; j++)
                {
                    data[x][j] = Mathf.Max(data[x][j], current[j]);
                }
                for (int y = 0; y < spectrogramTexture.height; y++)
                {
                    var last = bufferTexture.GetPixel(x, y);
                    var t = (data[x][y] + 1f) * 0.5f;
                    Color pixelColor = Color.white;
                    if (t < 0.5f)
                        pixelColor = Color.Lerp(lowIntensityColor, midIntensityColor, Mathf.Log(t*100, PowMode)*0.01f);
                    else
                        pixelColor = Color.Lerp(midIntensityColor, highIntensityColor, Mathf.Log(t*100, PowMode)*0.01f);
                    pixelColor = new(Mathf.Max(last.r, pixelColor.r), Mathf.Max(last.g, pixelColor.g),
                        Mathf.Max(last.b, pixelColor.b), Mathf.Max(last.a, pixelColor.a));
                    bufferTexture.SetPixel(x, y, pixelColor);
                }
                if (x % 100 == 0)
                {
                    bufferTexture.Apply();
                    Debug.Log($"ProcessAudioClip: {i}-{i * 100 / samplesCount}%");
                    yield return null;
                }
                i += Step;
            }
            //for (int x = 0; x < spectrogramTexture.width; x++)
            //{
            //    for (int y = 0; y < spectrogramTexture.height; y++)
            //    {
            //        var last = bufferTexture.GetPixel(x, y);
            //        var t = (data[x][y] + 1f) * 0.5f;
            //        Color pixelColor = Color.Lerp(lowIntensityColor, highIntensityColor, Mathf.Pow(t, PowMode));
            //        pixelColor = new(Mathf.Max(last.r, pixelColor.r), Mathf.Max(last.g, pixelColor.g),
            //            Mathf.Max(last.b, pixelColor.b), Mathf.Max(last.a, pixelColor.a));
            //        bufferTexture.SetPixel(x, y, pixelColor);
            //    }
            //    if (x % 100 == 0)
            //    {
            //        Debug.Log($"ProcessAudioClip Render: {x}-{x * 100 / spectrogramTexture.width}%");
            //        yield return null;
            //    }
            //}

            // 最终更新渲染纹理
            bufferTexture.Apply();
            Graphics.CopyTexture(bufferTexture, spectrogramTexture);
            Debug.Log("频谱图生成完成！");
        }

        void OnDestroy()
        {
            if (bufferTexture != null)
            {
                DestroyImmediate(bufferTexture);
            }
        }

    }
}
