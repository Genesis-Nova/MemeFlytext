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

			ImGui.SetNextWindowSize(new Vector2(300, 400), ImGuiCond.FirstUseEver);
			if (ImGui.Begin("Meme Flytext Config", ref settingsVisible, ImGuiWindowFlags.AlwaysVerticalScrollbar))
			{

				// local copies of config properties
				var DesquishDamageConfigValue = configuration.DesquishDamageEnabled;
				var ZeroDamageConfigValue = configuration.ZeroDamageEnabled;
				var CrazyDamageConfigValue = configuration.CrazyDamageEnabled;

				if (ImGui.Checkbox("Enable Desquished Damage", ref DesquishDamageConfigValue))
				{
					memeFlytextPlugin.Desquish = DesquishDamageConfigValue;
					configuration.DesquishDamageEnabled = DesquishDamageConfigValue;
					configuration.Save();
				}
				if (ImGui.Checkbox("Enable All Damage is Zero", ref ZeroDamageConfigValue))
				{
					memeFlytextPlugin.Zero = ZeroDamageConfigValue;
					configuration.ZeroDamageEnabled = ZeroDamageConfigValue;
					configuration.Save();
				}
				if (ImGui.Checkbox("Enable Crazy Random Damage", ref CrazyDamageConfigValue))
				{
					memeFlytextPlugin.Crazy = CrazyDamageConfigValue;
					configuration.CrazyDamageEnabled = CrazyDamageConfigValue;
					configuration.Save();
				}
				ImGui.End();
			}
		}
	}
}
