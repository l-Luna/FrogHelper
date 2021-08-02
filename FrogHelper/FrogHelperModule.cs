using Celeste.Mod;
using FrogHelper.Entities;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;

namespace FrogHelper {
	public class FrogHelperModule : EverestModule {

		public static FrogHelperModule Instance;

		public override Type SessionType => typeof(FrogHelperSession);
		public FrogHelperSession Session => _Session as FrogHelperSession;

		public static List<Hook> OptionalHooks = new List<Hook>();

		public FrogHelperModule() {
			Instance = this;
		}

		public override void Load() {
			FrogelineWingedSilver.Load();
		}

		public override void Unload() {
			FrogelineWingedSilver.Unload();

			foreach(var item in OptionalHooks)
				item.Dispose();
			OptionalHooks.Clear();
		}

		public override void Initialize() {
			FrogelineWingedSilver.Initialize();
		}
	}
}
