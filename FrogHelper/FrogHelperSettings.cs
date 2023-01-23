using System;
using System.Linq;
using Celeste.Mod;
using YamlDotNet.Serialization;

namespace FrogHelper {
	public class FrogHelperSettings : EverestModuleSettings {
		[SettingIgnore]
		public int FrogShardCountPos { get; set; } = -1;

		[YamlIgnore]
		[SettingRange(1, 100)]
		public int CollectableCountPosition {
			get {
				if(FrogShardCountPos >= 0) return FrogShardCountPos;

				//Determine default value
				return Everest.Modules.Any(m => m.GetType().FullName.Equals("CelesteDeathTracker.DeathTrackerModule", StringComparison.OrdinalIgnoreCase)) ? 2 : 1;
			}
			set => FrogShardCountPos = value;
		}
	}
}
