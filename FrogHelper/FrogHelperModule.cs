using Celeste;
using Celeste.Mod;
using FrogHelper.Entities;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrogHelper {
	public class FrogHelperModule : EverestModule {

		public static FrogHelperModule Instance;

		public override Type SettingsType => typeof(FrogHelperSettings);
		public FrogHelperSettings Settings => _Settings as FrogHelperSettings;

		public override Type SessionType => typeof(FrogHelperSession);
		public FrogHelperSession Session => _Session as FrogHelperSession;

		public override Type SaveDataType => typeof(FrogHelperSaveData);
		public FrogHelperSaveData SaveData => _SaveData as FrogHelperSaveData;

		public static List<Hook> OptionalHooks = new List<Hook>();

		public FrogHelperModule() {
			Instance = this;
		}

		public override void Load() {
			WingedSilver.Load();
			StylegroundsPanelRenderer.Load();
			FrogBerry.Load();
			FrogBerryShard.Load();

			On.Celeste.OuiChapterPanel.Render += AddFrogToChapterPanel;
		}

		public override void Unload() {
			WingedSilver.Unload();
			StylegroundsPanelRenderer.Unload();
			FrogBerry.Unload();
			FrogBerryShard.Unload();

			On.Celeste.OuiChapterPanel.Render -= AddFrogToChapterPanel;

			foreach(var item in OptionalHooks)
				item.Dispose();
			OptionalHooks.Clear();
		}

		public override void Initialize() {
			WingedSilver.Initialize();
		}

		private void AddFrogToChapterPanel(On.Celeste.OuiChapterPanel.orig_Render orig, OuiChapterPanel self) {
			orig(self);
			string sid = self.Area.GetSID();
			AreaModeStats areaModeStats = self.RealStats.Modes[(int)self.Area.Mode];
			if(SaveData.LevelsWithFrogShardCollected.Any(kv => kv.Value.Contains(sid))) {
				GFX.Gui["FrogHelper/frog"].Draw(self.Position + new Vector2(70, areaModeStats.Completed ? 250 : 220), Vector2.Zero, Color.White * 0.75f, areaModeStats.Completed ? 3 : 2.5f, (float)(-Math.PI / 8));
			}
		}
	}
}
