using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Numerics;

namespace MemeFlytext
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool CrazyDamageEnabled { get; set; }
        
        [NonSerialized]
        private DalamudPluginInterface pluginInterface;

        [NonSerialized]
        private MemeFlytextPlugin dmgPlugin;

        public void Initialize(DalamudPluginInterface pluginInterface, MemeFlytextPlugin dmgPlugin)
        {
            this.pluginInterface = pluginInterface;
            this.dmgPlugin = dmgPlugin;
        }

        public void Save()
        {
            pluginInterface.SavePluginConfig(this);
        }
    }
}
