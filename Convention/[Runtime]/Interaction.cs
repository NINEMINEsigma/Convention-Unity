using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Convention
{
    /// <summary>
    /// 统一的文件交互类，支持本地文件、网络文件和localhost路径的自适应处理
    /// 使用UnityWebRequest实现跨平台兼容，支持WebGL和IL2CPP
    /// 
    /// <list type="bullet"><see cref="ToolFile"/>是依赖项</list>
    /// </summary>
    [Serializable]
    public sealed class Interaction
    {
        #region Fields and Properties

        private string originalPath;
        private string processedPath;
        private PathType pathType;
        private object cachedData;

        public string OriginalPath => originalPath;
        public string ProcessedPath => processedPath;
        public PathType Type => pathType;

        public enum PathType
        {
            LocalFile,      // 本地文件 (file://)
            NetworkHTTP,    // 网络HTTP (http://)
            NetworkHTTPS,   // 网络HTTPS (https://)
            LocalServer,    // 本地服务器 (localhost)
            StreamingAssets // StreamingAssets目录
        }

        #endregion

        public Interaction(string path)
        {
            SetPath(path);
        }

        #region Setup

        /// <summary>
        /// 设置并处理路径
        /// </summary>
        public Interaction SetPath(string path)
        {
            originalPath = path;
            processedPath = ProcessPath(path);
            pathType = DeterminePathType(path);
            return this;
        }

        /// <summary>
        /// 处理路径，转换为UnityWebRequest可识别的格式
        /// </summary>
        private string ProcessPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            // 网络路径直接返回
            if (path.StartsWith("http://") || path.StartsWith("https://"))
            {
                return path;
            }

            // localhost处理
            if (path.StartsWith("localhost"))
            {
                return path.StartsWith("localhost/") ? "http://" + path : "http://localhost/" + path;
            }

            // StreamingAssets路径处理
            if (path.StartsWith("StreamingAssets/") || path.StartsWith("StreamingAssets\\"))
            {
                return Path.Combine(Application.streamingAssetsPath, path.Substring(16)).Replace("\\", "/");
            }

            // 本地文件路径处理
            string fullPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
            
            // WebGL平台特殊处理
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // WebGL只能访问StreamingAssets，尝试构建StreamingAssets路径
                return Application.streamingAssetsPath + "/" + Path.GetFileName(fullPath);
            }

            // 其他平台使用file://协议
            return "file:///" + fullPath.Replace("\\", "/");
        }

        /// <summary>
        /// 确定路径类型
        /// </summary>
        private PathType DeterminePathType(string path)
        {
            if (string.IsNullOrEmpty(path))
                return PathType.LocalFile;

            if (path.StartsWith("https://"))
                return PathType.NetworkHTTPS;
            if (path.StartsWith("http://"))
                return PathType.NetworkHTTP;
            if (path.StartsWith("localhost"))
                return PathType.LocalServer;
            if (path.StartsWith("StreamingAssets"))
                return PathType.StreamingAssets;

            return PathType.LocalFile;
        }

        #endregion

        #region Load

        #region LoadAsync

        public IEnumerator LoadAsTextAsync(Action<string> onSuccess, Action<string> onError = null)
        {
            using UnityWebRequest request = UnityWebRequest.Get(processedPath);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                cachedData = request.downloadHandler.text;
                onSuccess?.Invoke((string)cachedData);
            }
            else
            {
                string errorMsg = $"Failed to load text from {originalPath}: {request.error}";
                onError?.Invoke(errorMsg);
            }
        }

        public IEnumerator LoadAsBinaryAsync(Action<byte[]> onSuccess, Action<string> onError = null)
        {
            using UnityWebRequest request = UnityWebRequest.Get(processedPath);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                cachedData = request.downloadHandler.data;
                onSuccess?.Invoke((byte[])cachedData);
            }
            else
            {
                string errorMsg = $"Failed to load binary from {originalPath}: {request.error}";
                onError?.Invoke(errorMsg);
            }
        }
        public IEnumerator LoadAsBinaryAsync(Action<float> progress, Action<byte[]> onSuccess, Action<string> onError = null)
        {
            using UnityWebRequest request = UnityWebRequest.Get(processedPath);
            var result = request.SendWebRequest();
            while (result.isDone == false)
            {
                progress(result.progress);
                yield return null;
            }
            if (request.result == UnityWebRequest.Result.Success)
            {
                cachedData = request.downloadHandler.data;
                onSuccess?.Invoke((byte[])cachedData);
            }
            else
            {
                string errorMsg = $"Failed to load binary from {originalPath}: {request.error}";
                onError?.Invoke(errorMsg);
            }
        }

        public IEnumerator LoadAsRawJsonAsync<T>(Action<T> onSuccess, Action<string> onError = null)
        {
            yield return LoadAsTextAsync(
                text =>
                {
                    try
                    {
                        T result = JsonUtility.FromJson<T>(text);
                        cachedData = result;
                        onSuccess?.Invoke(result);
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"Failed to parse JSON: {e.Message}");
                    }
                },
                onError
            );
        }

        public IEnumerator LoadAsJsonAsync<T>(Action<T> onSuccess, Action<string> onError = null)
        {
            yield return LoadAsTextAsync(
                text =>
                {
                    try
                    {
                        using StreamReader reader = new(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text)));
                        var jsonReader = new ES3Internal.ES3JSONReader(reader.BaseStream, new(originalPath));
                        onSuccess?.Invoke(jsonReader.Read<T>());
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke($"Failed to parse JSON: {e.Message}");
                    }
                },
                onError
            );
        }

        public IEnumerator LoadAsImageAsync(Action<Texture2D> onSuccess, Action<string> onError = null)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(processedPath))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    cachedData = texture;
                    onSuccess?.Invoke(texture);
                }
                else
                {
                    string errorMsg = $"Failed to load image from {originalPath}: {request.error}";
                    onError?.Invoke(errorMsg);
                }
            }
        }

        public IEnumerator LoadAsAudioAsync(Action<AudioClip> onSuccess, Action<string> onError = null, AudioType audioType = AudioType.UNKNOWN)
        {
            if (audioType == AudioType.UNKNOWN)
                audioType = GetAudioType(originalPath);

            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(processedPath, audioType))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);
                    cachedData = audioClip;
                    onSuccess?.Invoke(audioClip);
                }
                else
                {
                    string errorMsg = $"Failed to load audio from {originalPath}: {request.error}";
                    onError?.Invoke(errorMsg);
                }
            }
        }

        public IEnumerator LoadAsAssetBundle(Action<float> progress, Action<AssetBundle> onSuccess, Action<string> onError = null)
        {
            AssetBundleCreateRequest request = null;
            yield return LoadAsBinaryAsync(x => progress(x * 0.5f), data => request = AssetBundle.LoadFromMemoryAsync(data), onError);
            while (request.isDone == false)
            {
                progress(0.5f + request.progress * 0.5f);
                yield return null;
            }
            try
            {
                AssetBundle bundle = request.assetBundle;
                if (bundle != null)
                {
                    cachedData = bundle;
                    onSuccess?.Invoke(bundle);
                }
                else
                {
                    onError?.Invoke($"Failed to load AssetBundle from data.");
                }
            }
            catch (Exception e)
            {
                onError?.Invoke($"Failed to load AssetBundle: {e.Message}");
            }
        }

        #endregion

        #region Load Sync

        public string LoadAsText()
        {
            string buffer = null;
            bool isEnd = false;
            bool IsError = false;
            var it = LoadAsTextAsync(x =>
            {
                buffer = x;
                isEnd = true;
            }, e =>
            {
                IsError = true;
                throw new Exception(e);
            });
            try
            {
                while (!isEnd)
                {
                    it.MoveNext();
                }
            }
            catch
            {
                if(IsError)
                {
                    throw;
                }
            }
            return buffer;
        }

        public byte[] LoadAsBinary()
        {
            byte[] buffer = null;
            bool isEnd = false;
            bool IsError = false;
            var it = LoadAsBinaryAsync(x =>
            {
                buffer = x;
                isEnd = true;
            }, e =>
            {
                IsError = true;
                throw new Exception(e);
            });
            try
            {
                while (!isEnd)
                {
                    it.MoveNext();
                }
            }
            catch
            {
                if (IsError)
                {
                    throw;
                }
            }
            return buffer;
        }

        public T LoadAsRawJson<T>()
        {
            T buffer = default;
            bool isEnd = false;
            bool IsError = false;
            var it = LoadAsRawJsonAsync<T>(x =>
            {
                buffer = x;
                isEnd = true;
            }, e =>
            {
                IsError = true;
                throw new Exception(e);
            });
            try
            {
                while (!isEnd)
                {
                    it.MoveNext();
                }
            }
            catch
            {
                if (IsError)
                {
                    throw;
                }
            }
            return buffer;
        }

        public T LoadAsJson<T>()
        {
            T buffer = default;
            bool isEnd = false;
            bool IsError = false;
            var it = LoadAsJsonAsync<T>(x =>
            {
                buffer = x;
                isEnd = true;
            }, e =>
            {
                IsError = true;
                throw new Exception(e);
            });
            try
            {
                while (!isEnd)
                {
                    it.MoveNext();
                }
            }
            catch
            {
                if (IsError)
                {
                    throw;
                }
            }
            return buffer;
        }

        public Texture2D LoadAsImage()
        {
            Texture2D buffer = null;
            bool isEnd = false;
            bool IsError = false;
            var it = LoadAsImageAsync(x =>
            {
                buffer = x;
                isEnd = true;
            }, e =>
            {
                IsError = true;
                throw new Exception(e);
            });
            try
            {
                while (!isEnd)
                {
                    it.MoveNext();
                }
            }
            catch
            {
                if (IsError)
                {
                    throw;
                }
            }
            return buffer;
        }

        public AudioClip LoadAsAudio(AudioType audioType = AudioType.UNKNOWN)
        {
            AudioClip buffer = null;
            bool isEnd = false;
            bool IsError = false;
            var it = LoadAsAudioAsync(x =>
            {
                buffer = x;
                isEnd = true;
            }, e =>
            {
                IsError = true;
                throw new Exception(e);
            }, audioType);
            try
            {
                while (!isEnd)
                {
                    it.MoveNext();
                }
            }
            catch
            {
                if (IsError)
                {
                    throw;
                }
            }
            return buffer;
        }

        #endregion

        #endregion

        #region Save

        public Interaction SaveAsText(string content)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                throw new NotSupportedException("WebGL平台不支持文件保存。");
            }

            if (pathType != PathType.LocalFile)
            {
                throw new NotSupportedException("仅支持保存到本地文件路径。");
            }

            new ToolFile(originalPath).SaveAsText(content);
            return this;

        }

        public Interaction SaveAsBinary(byte[] data)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                throw new NotSupportedException("WebGL平台不支持文件保存。");
            }

            if (pathType != PathType.LocalFile)
            {
                throw new NotSupportedException("仅支持保存到本地文件路径。");
            }

            new ToolFile(originalPath).SaveAsBinary(data);
            return this;
        }

        public Interaction SaveAsRawJson<T>(T obj)
        {
            new ToolFile(originalPath).SaveAsRawJson(obj);
            return this;
        }

        public Interaction SaveAsJson<T>(T obj)
        {
            new ToolFile(originalPath).SaveAsJson(obj);
            return this;
        }

        #endregion

        #region Tools

        /// <summary>
        /// 获取本地文件路径
        /// </summary>
        private string GetLocalPath()
        {
            if (processedPath.StartsWith("file:///"))
            {
                return processedPath.Substring(8);
            }
            return originalPath;
        }

        /// <summary>
        /// 根据文件扩展名获取音频类型
        /// </summary>
        private AudioType GetAudioType(string path)
        {
            return BasicAudioSystem.GetAudioType(path);
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        public bool Exists()
        {
            if (pathType == PathType.LocalFile && Application.platform != RuntimePlatform.WebGLPlayer)
            {
                return File.Exists(GetLocalPath());
            }
            else
            {
                // TODO : 网络文件和WebGL平台需要通过请求检查
                // 当前默认存在
                return true;
            }
        }

        /// <summary>
        /// 异步检查文件是否存在
        /// </summary>
        public IEnumerator ExistsAsync(Action<bool> callback)
        {
            using UnityWebRequest request = UnityWebRequest.Head(processedPath);
            yield return request.SendWebRequest();
            callback?.Invoke(request.result == UnityWebRequest.Result.Success);
        }

        #endregion

        #region Operator

        public static implicit operator string(Interaction interaction) => interaction?.originalPath;

        public override string ToString() => originalPath;

        /// <summary>
        /// 路径连接操作符
        /// </summary>
        public static Interaction operator |(Interaction left, string rightPath)
        {
            if (left.pathType == PathType.NetworkHTTP || left.pathType == PathType.NetworkHTTPS || left.pathType == PathType.LocalServer)
            {
                string baseUrl = left.originalPath;
                string newUrl = baseUrl.EndsWith("/") ? baseUrl + rightPath : baseUrl + "/" + rightPath;
                return new Interaction(newUrl);
            }
            else
            {
                string newPath = Path.Combine(left.originalPath, rightPath);
                return new Interaction(newPath);
            }
        }

        #endregion

        #region Tools

        /// <summary>
        /// 获取文件名
        /// </summary>
        public string GetFilename()
        {
            if (pathType == PathType.NetworkHTTP || pathType == PathType.NetworkHTTPS || pathType == PathType.LocalServer)
            {
                var uri = new Uri(processedPath);
                return Path.GetFileName(uri.AbsolutePath);
            }
            return Path.GetFileName(originalPath);
        }

        /// <summary>
        /// 获取文件扩展名
        /// </summary>
        public string GetExtension()
        {
            return Path.GetExtension(GetFilename());
        }

        /// <summary>
        /// 检查扩展名
        /// </summary>
        public bool ExtensionIs(params string[] extensions)
        {
            string ext = GetExtension().ToLower();
            string extWithoutDot = ext.Length > 1 ? ext[1..] : "";
            
            foreach (string extension in extensions)
            {
                string checkExt = extension.ToLower();
                if (ext == checkExt || extWithoutDot == checkExt || ext == "." + checkExt)
                    return true;
            }
            return false;
        }

        #endregion

        #region Tools

        /// <summary>
        /// 创建一个新的Interaction实例
        /// </summary>
        public static Interaction Create(string path) => new(path);

        /// <summary>
        /// 从StreamingAssets创建
        /// </summary>
        public static Interaction FromStreamingAssets(string relativePath)
        {
            return new Interaction("StreamingAssets/" + relativePath);
        }

        /// <summary>
        /// 从本地服务器创建
        /// </summary>
        public static Interaction FromLocalhost(string path, int port = 80)
        {
            string url = port == 80 ? $"localhost/{path}" : $"localhost:{port}/{path}";
            return new Interaction(url);
        }

        #endregion
    }
}
