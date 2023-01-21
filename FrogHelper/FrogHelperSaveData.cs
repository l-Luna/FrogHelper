using Celeste;
using Celeste.Mod;
using System;
using System.Collections.Generic;

namespace FrogHelper {
	public class FrogHelperSaveData : EverestModuleSaveData {
		public Dictionary<string, HashSet<string>> LevelsWithFrogShardCollected = new Dictionary<string, HashSet<string>>();
        public HashSet<string> LevelsWithFrogBerryCollected = new HashSet<string>();

        public int CountCollectedFrogShards(AreaKey area) => LevelsWithFrogShardCollected.TryGetValue(area.LevelSet, out HashSet<string> sids) ? sids.Count : 0;
    }
}
