using Celeste;
using Celeste.Mod;
using System.Collections.Generic;
using System.Linq;

namespace FrogHelper {
	public class FrogHelperSaveData : EverestModuleSaveData {
		public HashSet<string> LevelsWithFrogShardCollected = new HashSet<string>();

        public int CountCollectedFrogShards(AreaKey area) => LevelsWithFrogShardCollected.Count(sid => AreaData.Get(sid).LevelSet == area.LevelSet);
    }
}
