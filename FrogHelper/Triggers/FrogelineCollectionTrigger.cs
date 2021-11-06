using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using static Celeste.SummitCheckpoint;

namespace FrogHelper.Triggers {

	[CustomEntity("FrogHelper/FrogelineCollectionTrigger")]
	class FrogelineCollectionTrigger : Trigger {

		public FrogelineCollectionTrigger(EntityData data, Vector2 offset) : base(data, offset) {
		}

		public override void OnEnter(Player player) {
			base.OnEnter(player);
			Level level = Scene as Level;
			string sid = level.Session.Area.SID;
			if(!FrogHelperModule.Instance.SaveData.LevelsWithFrogelineCollected.Contains(sid)) {
				level.Displacement.AddBurst(Position, 0.5f, 4f, 24f, 0.5f);
				level.Add(new ConfettiRenderer(BottomCenter));
				Audio.Play("event:/game/07_summit/checkpoint_confetti", Position);
				FrogHelperModule.Instance.SaveData.LevelsWithFrogelineCollected.Add(sid);
			}
		}
	}
}
