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
        public bool DesquishDamageEnabled = false;
        public bool RandomDamageEnabled = false;
        public bool ZeroDamageEnabled = false;
        public bool CrazyDamageEnabled = false;

        [NonSerialized]
        private DalamudPluginInterface pluginInterface;

        [NonSerialized]
        private MemeFlytextPlugin flytextPlugin;

        public void Initialize(DalamudPluginInterface pluginInterface, MemeFlytextPlugin flytextPlugin)
        {
            this.pluginInterface = pluginInterface;
            this.flytextPlugin = flytextPlugin;
        }

        public void Save()
        {
            pluginInterface.SavePluginConfig(this);
        }
    }
}
