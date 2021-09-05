using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using Monocle;
using System.Collections;

namespace FrogHelper.Entities {

    /// <summary>
    /// A silver strawberry that appears only when certain conditions are met, and does not require deathless completion.
    /// </summary>
    [CustomEntity("FrogHelper/DiagonalWingedBerry")]
    [RegisterStrawberry(tracked: true, blocksCollection: false)]
    public class DiagonalWingedBerry : Strawberry {

        public DiagonalWingedBerry(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid) {
            new DynData<Strawberry>(this)["Winged"] = true;

            Add(new DashListener {
                OnDash = OnDash
            });
        }

        private void OnDash(Vector2 dir){
            var selfdata = new DynData<Strawberry>(this);
			if ((dir.X != 0) && (dir.Y != 0) && !selfdata.Get<bool>("flyingAway") && !WaitingOnSeeds){
				base.Depth = -1000000;
				Add(new Coroutine(FlyAwayRoutine()));
				selfdata["flyingAway"] = true;
			}
		}

        private IEnumerator FlyAwayRoutine(){
            var selfdata = new DynData<Strawberry>(this);
			selfdata.Get<Wiggler>("rotateWiggler").Start();
			selfdata["flapSpeed"] = -200f;
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.25f, start: true);
			tween.OnUpdate = delegate(Tween t){
				selfdata["flapSpeed"] = MathHelper.Lerp(-200f, 0f, t.Eased);
			};
			Add(tween);
			yield return 0.1f;
			Audio.Play("event:/game/general/strawberry_laugh", Position);
			yield return 0.2f;
			if (!Follower.HasLeader){
				Audio.Play("event:/game/general/strawberry_flyaway", Position);
			}
			tween = Tween.Create(Tween.TweenMode.Oneshot, null, 0.5f, start: true);
			tween.OnUpdate = delegate(Tween t){
				selfdata["flapSpeed"] = MathHelper.Lerp(0f, -200f, t.Eased);
			};
			Add(tween);
		}
    }
}