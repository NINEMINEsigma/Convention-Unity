using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Convention
{
    [Serializable]
    public class GlobalConfig : IEnumerable<KeyValuePair<string, object>>
    {
        public static string ConstConfigFile = "config.easysave";

        public static void InitExtensionEnv()
        {
            ConstConfigFile = "config.json";
            ProjectConfig.InitExtensionEnv();
        }

        private int configLogging_tspace = "Property not found".Length;

        private ToolFile DataDir;
        [Content, SerializeField] private Dictionary<string, object> data_pair = new();

        public GlobalConfig(string dataDir, bool isTryCreateDataDir = false, bool isLoad = true)
            : this(new ToolFile(dataDir), isTryCreateDataDir, isLoad) { }
        public GlobalConfig(ToolFile dataDir, bool isTryCreateDataDir = false, bool isLoad = true)
        {
            // build up data folder
            dataDir ??= new ToolFile("./");
            this.DataDir = dataDir;
            if (this.DataDir.IsDir() == false)
                this.DataDir.BackToParentDir();
            if (this.DataDir.Exists() == false)
            {
                if (isTryCreateDataDir)
                    this.DataDir.MustExistsPath();
                else
                    throw new Exception($"Data dir not found: {this.DataDir}");
            }
            // build up init data file
            var configFile = this.ConfigFile;
            if (configFile.Exists() == false)
                SaveProperties();
            else if (isLoad)
                this.LoadProperties();
        }
        ~GlobalConfig()
        {

        }

        public ToolFile GetConfigFile() => DataDir | ConstConfigFile;
        public ToolFile ConfigFile => GetConfigFile();

        public ToolFile GetFile(string path)
        {
            return DataDir | path;
        }
        public bool EraseFile(string path)
        {
            var file = DataDir | path;
            if (file.Exists())
            {
                file.Delete();
                file.Create();
                return true;
            }
            return false;
        }
        public bool RemoveFile(string path)
        {
            var file = DataDir | path;
            if (file.Exists())
            {
                try
                {
                    file.Delete();
                    return true;
                }
                catch (Exception) { }
            }
            return false;
        }
        public bool CreateFile(string path)
        {
            var file = DataDir | path;
            if (file.Exists())
                return false;
            if (file.GetParentDir().Exists() == false)
                return false;
            file.Create();
            return true;
        }

        public object this[string key]
        {
            get
            {
                return data_pair[key];
            }
            set
            {
                data_pair[key] = value;
            }
        }
        public bool Contains(string key) => data_pair.ContainsKey(key);
        public bool Remove(string key)
        {
            if (data_pair.ContainsKey(key))
            {
                data_pair.Remove(key);
                return true;
            }
            return false;
        }
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)this.data_pair).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.data_pair).GetEnumerator();
        }
        public int DataSize() => data_pair.Count;

        [Serializable]
        public struct InternalProperty
        {
            public Dictionary<string, object> property;
            public Dictionary<string, object> find;
        }

        public GlobalConfig SaveProperties()
        {
            var configFile = this.ConfigFile;
            configFile.SaveAsJson(new InternalProperty()
            {
                property = data_pair,
                find = data_find
            });
            return this;
        }
        public GlobalConfig LoadProperties()
        {
            var configFile = this.ConfigFile;
            if (configFile.Exists() == false)
            {
                data_pair = new();
            }
            else
            {
                data_pair = configFile.LoadAsJson<InternalProperty>().property;
            }
            return this;
        }

        public ToolFile GetLogFile()
        {
            return this.GetFile(ConfigFile.GetName(true) + "_log.txt").MustExistsPath();
        }
        public ToolFile LogFile => GetLogFile();

        private Action<string> MyDefaultLogger;
        public Action<string> DefaultLogger
        {
            get
            {
                return MyDefaultLogger ?? Console.WriteLine;
            }
            set
            {
                MyDefaultLogger = value;
            }
        }

        public virtual void Log(string messageType, string message, Action<string> logger)
        {
            configLogging_tspace = Math.Max(configLogging_tspace, messageType.Length);
            (logger ?? DefaultLogger)($"[{Utility.NowFormat()}]{new string(' ', configLogging_tspace / 2)}{messageType}{new string(' ', configLogging_tspace - configLogging_tspace / 2)}: {message}");
        }
        public void Log(string messageType, string message) => Log(messageType, message, null);
        public void LogPropertyNotFound(string message, Action<string> logger, object @default = null)
        {
            if (@default != null)
            {
                message = $"{message} (default: {@default})";
            }
            Log("Property not found", message);
        }
        public void LogPropertyNotFound(string message, object @default = null)
        {
            if (@default != null)
            {
                message = $"{message} (default: {@default})";
            }
            Log("Property not found", message);
        }
        public void LogMessageOfPleaseCompleteConfiguration()
        {
            var message = "Please complete configuration";
            Log("Error", message);
        }

        private Dictionary<string, object> data_find = new();

        public object FindItem(string key, object @default = null)
        {
            if (Contains(key))
            {
                data_find.Remove(key);
                return this[key];
            }
            else
            {
                data_find[key] = @default;
                LogPropertyNotFound(key, @default);
                return @default;
            }
        }
    }

    public class ProjectConfig : GlobalConfig
    {
        private static string ProjectConfigFileFocus = "Assets/";

        public static new void InitExtensionEnv()
        {
            ProjectConfigFileFocus = "Assets/";
        }

        public ProjectConfig(bool isLoad = true) : base(ProjectConfigFileFocus, true, isLoad) { }

        public static void SetProjectConfigFileFocus(string path)
        {
            ProjectConfigFileFocus = path;
        }
        public static string GetProjectConfigFileFocus()
        {
            return ProjectConfigFileFocus;
        }
    }
}
