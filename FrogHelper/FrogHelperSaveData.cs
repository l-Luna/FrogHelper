using Celeste;
using Celeste.Mod;
using System.Collections.Generic;

namespace FrogHelper {
	public class FrogHelperSaveData : EverestModuleSaveData {
        public HashSet<string> LevelsWithFrogShardCollected = new HashSet<string>(), LevelsWithFrogBerryCollected = new HashSet<string>();
    }
}
