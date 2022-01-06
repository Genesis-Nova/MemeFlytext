using ImGuiNET;
using System;
using System.Numerics;

namespace MemeFlytext
{
	class PluginUI : IDisposable
	{
		private Configuration configuration;
		private MemeFlytextPlugin memeFlytextPlugin;

		private bool visible = false;
		public bool Visible
		{
			get => visible;
			set => visible = value;
		}

		private bool settingsVisible = false;
		public bool SettingsVisible
		{
			get => settingsVisible;
			set => settingsVisible = value;
		}

		public PluginUI(Configuration configuration, MemeFlytextPlugin memeFlytextPlugin)
		{
			this.configuration = configuration;
			this.memeFlytextPlugin = memeFlytextPlugin;
		}

		public void Dispose()
		{

		}

		public void Draw()
		{
			DrawSettingsWindow();
		}

		private void DrawSettingsWindow()
		{
			if (!SettingsVisible) return;

			ImGui.SetNextWindowSize(new Vector2(400, 500), ImGuiCond.FirstUseEver);
			if (ImGui.Begin("Meme Flytext Config", ref settingsVisible, ImGuiWindowFlags.AlwaysVerticalScrollbar))
			{

				// local copies of config properties
				var debugLogConfigValue = configuration.DebugLogEnabled;
				var CrazyDamageConfigValue = configuration.CrazyDamageEnabled;

				if (ImGui.Checkbox("Enable Crazy Random Damage", ref CrazyDamageConfigValue))
				{
					configuration.CrazyDamageEnabled = CrazyDamageConfigValue;
					configuration.Save();
				}

				if (ImGui.CollapsingHeader("Debug"))
					{
						if (ImGui.Checkbox("Enable Debug Logging", ref debugLogConfigValue))
						{
							configuration.DebugLogEnabled = debugLogConfigValue;
							configuration.Save();
						}
					}
				ImGui.End();
			}
		}
	}
}
