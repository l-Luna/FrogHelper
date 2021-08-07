using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace FrogHelper.Triggers {

	/// <summary>
	/// A trigger that prevents watchtowers from zooming out.
	/// </summary>
	[CustomEntity("FrogHelper/LookoutZoomFixTrigger")]
	class LookoutZoomFixTrigger : Trigger {

		// we don't actually need any parameters
		public LookoutZoomFixTrigger(EntityData data, Vector2 offset) : base(data, offset) {
			Depth = -9000;
		}

		public override void Update() {
			base.Update();
			if(PlayerIsInside)
				SceneAs<Level>().ScreenPadding = 0;
		}
	}
}