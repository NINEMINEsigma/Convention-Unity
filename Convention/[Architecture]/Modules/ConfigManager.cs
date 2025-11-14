using System;
using System.Collections.Generic;

namespace Convention.Experimental.Modules
{
    public class ConfigManager : PublicType.GameModule
    {
        public readonly ProjectConfig m_ProjectConfig = new();
        public bool IsSavePropertiesWhenShutdown = false;

        internal override void Shutdown()
        {
            if (IsSavePropertiesWhenShutdown)
                m_ProjectConfig.SaveProperties();
        }
    }
}
