using System;
using System.Collections.Generic;

namespace Convention.Experimental.Modules
{
    public class ConfigManager : PublicType.GameModule
    {
        public readonly ProjectConfig Config = new();
        public bool IsSavePropertiesWhenShutdown = false;

        internal override void Shutdown()
        {
            if (IsSavePropertiesWhenShutdown)
                Config.SaveProperties();
        }
    }
}
