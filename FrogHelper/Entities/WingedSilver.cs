using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace FrogHelper.Entities {

    /// <summary>
    /// A silver strawberry that appears only when certain conditions are met, and does not require deathless completion.
    /// </summary>
    [CustomEntity("FrogHelper/WingedSilver")]
    [RegisterStrawberry(tracked: false, blocksCollection: false)]
    public class WingedSilver : Strawberry {

        /// <summary>
        /// One of: "dashless", "noExtraJumps"
        /// </summary>
        protected string Condition;
		protected Vector2 start;
		protected bool flyingAway = false;
        protected float flapSpeed = 0f;

        protected bool uncollected = true;

        public WingedSilver(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid) {
            new DynData<Strawberry>(this)["Golden"] = true;
            Condition = data.Attr("condition");

            start = data.Position + offset;

            Add(new PlayerCollider(OnPlayerSilver));
            Add(new DashListener {
                OnDash = OnDash
            });
        }

		public override void Added(Scene scene) {
			base.Added(scene);

            new DynData<Strawberry>(this).Get<Sprite>("sprite").Play("flap");

			if(InvalidForCollection(true))
                RemoveSelf();
		}

        protected virtual void OnDash(Vector2 dir) {
			if(!flyingAway && Condition.Equals("dashless") && uncollected && !WaitingOnSeeds) {
				Depth = -1000000;
                Add(new Coroutine(FlyAwayRoutine()));
                flyingAway = true;
            }
        }

        protected virtual bool InvalidForCollection(bool levelLoad) {
            if(!string.IsNullOrWhiteSpace(Condition)) {
                Level level = SceneAs<Level>();
                if(Condition.Equals("dashless"))
                    return level.Session.Dashes > 0;
                if(Condition.Equals("noExtraJumps"))
                    return FrogHelperModule.Instance.Session.ExtraJumped;
            }
            return false;
        }

        // vanilla copy
        protected virtual IEnumerator FlyAwayRoutine() {
            DynData<Strawberry> selfData = new DynData<Strawberry>(this);

            selfData.Get<Wiggler>("rotateWiggler").Start();
            flapSpeed = -200f;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.25f, start: true);
            tween.OnUpdate = delegate (Tween t) {
                flapSpeed = MathHelper.Lerp(-200f, 0f, t.Eased);
            };
            Add(tween);
            yield return 0.1f;
            Audio.Play("event:/game/general/strawberry_laugh", Position);
            yield return 0.2f;
            if(!Follower.HasLeader) {
                Audio.Play("event:/game/general/strawberry_flyaway", Position);
            }

            tween = Tween.Create(Tween.TweenMode.Oneshot, null, 0.5f, start: true);
            tween.OnUpdate = delegate (Tween t) {
                flapSpeed = MathHelper.Lerp(0f, -200f, t.Eased);
            };
            Add(tween);
        }

		public override void Update() {
			base.Update();
            if(uncollected) {
                if(!flyingAway && /*sprite.CurrentAnimationFrame % 9 == 4*/Scene.OnInterval(9 * 1 / 12f)) {
                    Audio.Play("event:/game/general/strawberry_wingflap", Position);
                    flapSpeed = -50f;
                }
                if(!flyingAway && InvalidForCollection(false)) {
                    Depth = -1000000;
                    Add(new Coroutine(FlyAwayRoutine()));
                    flyingAway = true;
                }
                Y += flapSpeed * Engine.DeltaTime;
                if(flyingAway) {
                    if(Y < (SceneAs<Level>().Bounds.Top - 16)) {
                        RemoveSelf();
                    }
                } else {
                    flapSpeed = Calc.Approach(flapSpeed, 20f, 170f * Engine.DeltaTime);
                    if(Y < start.Y - 5f) {
                        Y = start.Y - 5f;
                    } else if(Y > start.Y + 5f) {
                        Y = start.Y + 5f;
                    }
                }
            }
        }

        private void OnPlayerSilver(Player player) {
            if(uncollected) {
                Level level = SceneAs<Level>();
                uncollected = false;
                Sprite sprite = new DynData<Strawberry>(this).Get<Sprite>("sprite");
                sprite.Rate = 0f;
                Alarm.Set(this, Follower.FollowDelay, delegate {
                    sprite.Rate = 1f;
                    sprite.Play("idle");
                    level.Particles.Emit(P_WingsBurst, 8, Position + new Vector2(8f, 0f), new Vector2(4f, 2f));
                    level.Particles.Emit(P_WingsBurst, 8, Position - new Vector2(8f, 0f), new Vector2(4f, 2f));
                });
            }
        }

        // hooks

        static Type jumpCountType;

        public static void Load() {
            On.Celeste.Level.Reload += OnLevelReload;
            Everest.Events.Level.OnLoadLevel += OnLevelLoad;
        }

		public static void Initialize() {
            if(Everest.Loader.DependencyLoaded(new EverestModuleMetadata() { Name = "ExtendedVariantMode", Version = new Version(0, 21, 0) })) {
                Type extendedVariantsModule = Everest.Modules.Where(m => m.GetType().FullName == "ExtendedVariants.Module.ExtendedVariantsModule").First().GetType();
                jumpCountType = extendedVariantsModule.Assembly.GetType("ExtendedVariants.Variants.JumpCount");

                FrogHelperModule.OptionalHooks.Add(new Hook(
                    jumpCountType.GetMethod("canJump", BindingFlags.NonPublic | BindingFlags.Instance),
                    typeof(WingedSilver).GetMethod("CheckExtraJumped", BindingFlags.NonPublic | BindingFlags.Static)));
            }
        }

        public static void Unload() {
            On.Celeste.Level.Reload -= OnLevelReload;
        }

        private static void OnLevelReload(On.Celeste.Level.orig_Reload orig, Level self) {
            if(!self.Completed) {
                var session = FrogHelperModule.Instance.Session;
                session.ExtraJumped = session.ExtraJumpedAtLevelStart;
            }
            orig(self);
        }

        private static void OnLevelLoad(Level level, Player.IntroTypes playerIntro, bool isFromLoader) {
            var session = FrogHelperModule.Instance.Session;
            session.ExtraJumpedAtLevelStart = session.ExtraJumped;
        }

        private delegate float orig_JumpCount_canJump(object org, float initialJumpGraceTimer, Player self, bool canWallJumpRight, bool canWallJumpLeft);
        private static float CheckExtraJumped(orig_JumpCount_canJump orig, object org, float initialJumpGraceTimer, Player self, bool canWallJumpRight, bool canWallJumpLeft) {
            DynamicData data = new DynamicData(jumpCountType);
            int jumpCountPrev = data.Get<int>("jumpBuffer");
            var ret = orig(org, initialJumpGraceTimer, self, canWallJumpRight, canWallJumpLeft);
			if(data.Get<int>("jumpBuffer") < jumpCountPrev)
                FrogHelperModule.Instance.Session.ExtraJumped = true;
            return ret;
        }


    }
}
