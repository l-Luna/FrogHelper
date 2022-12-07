using Celeste.Mod;
using System.Collections.Generic;

namespace FrogHelper {
	public class FrogHelperSaveData : EverestModuleSaveData {
		public HashSet<string> LevelsWithFrogelineCollected = new HashSet<string>();
	}
}
