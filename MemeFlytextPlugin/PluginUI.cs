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

				if (ImGui.Checkbox("Enable Desquished flytext", ref configuration.DesquishDamageEnabled))
				{
					configuration.Save();
				}
				if (ImGui.Checkbox("Enable All flytext is Zero", ref configuration.ZeroDamageEnabled))
				{
					configuration.Save();
				}
				if (ImGui.Checkbox("Enable Crazy Random flytext", ref configuration.CrazyDamageEnabled))
				{
					configuration.Save();
				}
				ImGui.End();
			}
		}
	}
}