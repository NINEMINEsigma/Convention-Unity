using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;


namespace Convention
{
    public sealed partial class ToolFile
    {
        public static AudioType GetAudioType(string path)
        {
            return Path.GetExtension(path) switch
            {
                "ogg" => AudioType.OGGVORBIS,
                "mp2" => AudioType.MPEG,
                "mp3" => AudioType.MPEG,
                "mod" => AudioType.MOD,
                "wav" => AudioType.WAV,
                "aif" => AudioType.IT,
                _ => AudioType.UNKNOWN
            };
        }

        public bool IsAssetBundle => ExtensionIs("ab", nameof(AssetBundle), nameof(AssetBundle).ToLower());

        #region Load

        public IEnumerator LoadAsImage([In] Action<Texture2D> callback)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(OriginPath);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(DownloadHandlerTexture.GetContent(request));
            }
            else callback(null);
        }
        public AudioClip LoadAsAudio()
        {
            return ES3Plugin.LoadAudio(OriginPath, GetAudioType(OriginPath));
        }
        public IEnumerator LoadAsAudio([In] Action<AudioClip> callback)
        {
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(OriginPath, GetAudioType(OriginPath));
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(DownloadHandlerAudioClip.GetContent(request));
            }
            else callback(null);
        }
        public AssetBundle LoadAsAssetBundle()
        {
            return AssetBundle.LoadFromFile(OriginPath);
        }
        public IEnumerator LoadAsAssetBundle([In] Action<AssetBundle> callback)
        {
            AssetBundleCreateRequest result = AssetBundle.LoadFromFileAsync(OriginPath);
            yield return result;
            callback(result.assetBundle);
            yield return null;
        }
        public IEnumerator LoadAsAssetBundle([In] Action<float> progress, [In] Action<AssetBundle> callback)
        {
            AssetBundleCreateRequest result = AssetBundle.LoadFromFileAsync(OriginPath);
            while (result.isDone == false)
            {
                progress(result.progress);
                yield return null;
            }
            yield return result;
            callback(result.assetBundle);
            yield return null;
        }


        #endregion

        #region Save

        #endregion
    }
}
