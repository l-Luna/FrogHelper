using System;
using System.Linq;
using Celeste.Mod;
using YamlDotNet.Serialization;

namespace FrogHelper {
	public class FrogHelperSettings : EverestModuleSettings {
		[YamlMember]
		private int frogShardCountPos = -1;

		[YamlIgnore]
		[SettingRange(1, 100)]
		public int CollectableCountPosition {
			get {
				if(frogShardCountPos >= 0) return frogShardCountPos;

				//Determine default value
				return Everest.Modules.Any(m => m.GetType().FullName.Equals("CelesteDeathTracker.DeathTrackerModule", StringComparison.OrdinalIgnoreCase)) ? 2 : 1;
			}
			set => frogShardCountPos = value;
		}
	}
}
