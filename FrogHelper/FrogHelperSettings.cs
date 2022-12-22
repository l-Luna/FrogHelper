using Celeste.Mod;

namespace FrogHelper {
	public class FrogHelperSettings : EverestModuleSettings {
		[SettingRange(1, 100)]
		public int CollectableCountPosition { get; set; } = 1;
	}
}
