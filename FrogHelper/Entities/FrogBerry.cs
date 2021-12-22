using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace FrogHelper.Entities {

	/// <summary>
	/// A strawberry that only appears when you collect all frogelines in a lobby's level set.
	/// </summary>
	[CustomEntity("FrogHelper/FrogBerry")]
	[RegisterStrawberry(tracked: true, blocksCollection: false)]
	class FrogBerry : Strawberry {

		public FrogBerry(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid) {

		}
	}
}
