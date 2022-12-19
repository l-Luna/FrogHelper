using System;
using System.Collections;
using System.Reflection;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace FrogHelper.Entities {

    /// <summary>
    /// A strawberry that only appears when you collect all frogelines in a lobby's level set.
    /// </summary>
    [CustomEntity("FrogHelper/FrogBerryShard")]
    [RegisterStrawberry(tracked: false, blocksCollection: false)]
    public class FrogBerryShard : Strawberry {
        //Modified vanilla code
        private class ShardCounter : Component {
            public static readonly Color Color = Color.White, FlashColor = Calc.HexToColor("FF5E76");
            public const float Scale = 1f, Stroke = 2f;
            private const int IconWidth = 60;

            public Vector2 Position;
            private int amount;
            private Wiggler wiggler;
            private float flashTimer;
            private MTexture xTex;

            public int Amount {
                get => amount;
                set {
                    if(amount == value) return;
                    amount = value;

                    Audio.Play("event:/ui/game/increment_strawberry");
                    wiggler.Start();
                    flashTimer = 0.5f;
                }
            }

            public ShardCounter(int amount) : base(true, true) {
                this.amount = amount;

                //Create wiggler
                wiggler = Wiggler.Create(0.5f, 3f);
                wiggler.StartZero = true;
                wiggler.UseRawDeltaTime = true;

                xTex = GFX.Gui["x"];
            }

            public override void Update() {
                base.Update();

                //Update wiggler
                if(wiggler.Active) wiggler.Update();

                //Decrease flash timer
                if(flashTimer > 0f) flashTimer -= Engine.RawDeltaTime;
            }

            public override void Render() {
                //Determine render position and color
                Vector2 renderPos = (Position + (Entity?.Position ?? Vector2.Zero)).Round();
                Color color = (flashTimer > 0f && (Scene?.BetweenRawInterval(0.05f) ?? false)) ? FlashColor : Color;

                //Render the counter
                float off = 0;

                GFX.Gui["FrogHelper/frogBerryShard"].DrawCentered(renderPos + new Vector2(off + IconWidth * 0.5f, 0) * Scale, Color.White, Scale);
                off += IconWidth + 2;

                xTex.DrawCentered(renderPos + new Vector2(off + xTex.Width * 0.5f, 2f) * Scale, color, Scale);
                off += xTex.Width + 2;

                float amountTextSz = ActiveFont.Measure(amount.ToString()).X;
                ActiveFont.DrawOutline(amount.ToString(), renderPos + new Vector2(off + amountTextSz * 0.5f, wiggler.Value * 18f) * Scale, Vector2.One / 2, Vector2.One * Scale, color, Stroke, Color.Black);
                off += amountTextSz;
            }
        }

        //Modified vanilla code
        private class TotalShardDisplay : Entity {
            private const float LerpInSpeed = 1.2f, LerpOutSpeed = 2f;
            private const float NumberUpdateDelay = 0.4f, ComboUpdateDelay = 0.3f;
            private const float AfterUpdateDelay = 2f;

            private MTexture bg;
            public float DrawLerp;
            private float updateTimer;
            private float waitTimer;
            private ShardCounter counter;

            public TotalShardDisplay(AreaKey area) {
                Y = 96f;
                Depth = Depths.Pickups - 1;
                Tag = Tags.HUD | Tags.Global | Tags.PauseUpdate | Tags.TransitionUpdate;
                bg = GFX.Gui["strawberryCountBG"];
                Add(counter = new ShardCounter(FrogHelperModule.Instance.SaveData.CountCollectedFrogShards(area)));
            }

            public override void Update() {
                base.Update();

                Level level = SceneAs<Level>();
                int numShards = FrogHelperModule.Instance.SaveData.CountCollectedFrogShards(level.Session.Area);

                //The player must have at least one shard
                if(numShards <= 0) {
                    Visible = false;
                    DrawLerp = updateTimer = waitTimer = 0;
                    return;
                }

                //Start update timer if new shards have been collected
                if(numShards > counter.Amount && updateTimer <= 0f) updateTimer = NumberUpdateDelay;

                //Update draw lerp
                if(numShards > counter.Amount || updateTimer > 0f || waitTimer > 0f || (level.Paused && level.PauseMainMenuOpen)) {
                    DrawLerp = Calc.Approach(DrawLerp, 1f, LerpInSpeed * Engine.RawDeltaTime);
                } else {
                    DrawLerp = Calc.Approach(DrawLerp, 0f, LerpOutSpeed * Engine.RawDeltaTime);
                }

                //Decrease wait timer
                if(waitTimer > 0f) waitTimer -= Engine.RawDeltaTime;

                //Decrease the update timer
                if(updateTimer > 0f && DrawLerp == 1f) {
                    updateTimer -= Engine.RawDeltaTime;
                    if(updateTimer <= 0f) {
                        //Increment the shard count
                        if(counter.Amount < numShards) counter.Amount++;

                        //Restart timers
                        waitTimer = AfterUpdateDelay;
                        if(counter.Amount < numShards) updateTimer = ComboUpdateDelay;
                    }
                }

                //Update display Y coordinate
                if(Visible) {
                    float displayY = 96f;
                    if(!level.TimerHidden) {
                        if(Settings.Instance.SpeedrunClock == SpeedrunType.Chapter) {
                            displayY += 58f;
                        } else if(Settings.Instance.SpeedrunClock == SpeedrunType.File) {
                            displayY += 78f;
                        }
                    }
                    displayY += 78f;
                    Y = Calc.Approach(Y, displayY, Engine.DeltaTime * 800f);
                }
                Visible = DrawLerp > 0f;
            }

            public override void Render() {
                Vector2 pos = Vector2.Lerp(new Vector2(-bg.Width, Y), new Vector2(32f, Y), Ease.CubeOut(this.DrawLerp)).Round();
                bg.DrawJustified(pos + new Vector2(-96f, 12f), new Vector2(0f, 0.5f));
                counter.Position = pos + new Vector2(0f, -Y);
                counter.Render();
            }
        }

        private Sprite sprite;

        public FrogBerryShard(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid) {}

        public override void Added(Scene scene) {
            base.Added(scene);

            bool isGhost = FrogHelperModule.Instance.SaveData.LevelsWithFrogShardCollected.Contains(SceneAs<Level>().Session.Area.SID);

            //Replace the sprite
            sprite = new DynData<Strawberry>(this).Get<Sprite>("sprite");
            sprite = GFX.SpriteBank.CreateOn(sprite, isGhost ? "FrogHelper_ghostFrogBerryShard" : "FrogHelper_frogBerryShard");
            sprite.OnFrameChange = OnAnimate;
            sprite.Play("idle");
        }

        //Modified vanilla code
        private IEnumerator CollectRoutine(int collectIdx) {
            FrogHelperModule.Instance.SaveData.LevelsWithFrogShardCollected.Add(SceneAs<Level>().Session.Area.SID);

            Tag = Tags.TransitionUpdate;
            Depth = Depths.FormationSequences - 10;

            Audio.Play("event:/game/general/strawberry_get", Position, "colour", 3, "count", collectIdx);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

            sprite.Play("collect", false, false);
            while(sprite.Animating) yield return null;

            Scene.Add(new StrawberryPoints(Position, new DynData<Strawberry>(this).Get<bool>("isGhostBerry"), collectIdx, false));
            RemoveSelf();
        }

        private void OnAnimate(string id) {
            DynData<Strawberry> dynDat = new DynData<Strawberry>(this);
            if(sprite.CurrentAnimationFrame == 36) {
                dynDat.Get<Tween>("lightTween").Start();
                if(!dynDat.Get<bool>("collected") && (CollideCheck<FakeWall>() || CollideCheck<Solid>())) {
                    Audio.Play("event:/game/general/strawberry_pulse", Position);
                    SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.1f);
                } else {
                    Audio.Play("event:/game/general/strawberry_pulse", Position);
                    SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
                }
            }
        }

        private static ILHook updateHook;

        public static void Load() {
            On.Celeste.Strawberry.CollectRoutine += CollectRoutineHook;
            updateHook = new ILHook(typeof(Strawberry).GetMethod(nameof(Strawberry.orig_Update)), UpdateModifier);

            IL.Celeste.LevelLoader.LoadingThread += LoadingThreadHook;
            IL.Celeste.Level.Reload += DrawLerpHook;
            IL.Celeste.Level.SkipCutsceneRoutine += DrawLerpHook;
        }

        public static void Unload() {
            On.Celeste.Strawberry.CollectRoutine -= CollectRoutineHook;
            updateHook?.Dispose();
            updateHook = null;

            IL.Celeste.LevelLoader.LoadingThread -= LoadingThreadHook;
            IL.Celeste.Level.Reload -= DrawLerpHook;
            IL.Celeste.Level.SkipCutsceneRoutine -= DrawLerpHook;
        }

        private static IEnumerator CollectRoutineHook(On.Celeste.Strawberry.orig_CollectRoutine orig, Strawberry self, int collectIndex) {
            if(self is FrogBerryShard frogBerryShard) return frogBerryShard.CollectRoutine(collectIndex);
            return orig(self, collectIndex);
        }

        private static void UpdateModifier(ILContext ctx) {
            ILCursor cursor = new ILCursor(ctx);

            //Patch wiggle amount
            ILLabel regWobble = cursor.DefineLabel();
            cursor.GotoNext(MoveType.After, i => i.MatchCallOrCallvirt(typeof(Math).GetMethod(nameof(Math.Sin), BindingFlags.Public | BindingFlags.Static)));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Isinst, typeof(FrogBerryShard));
            cursor.Emit(OpCodes.Brfalse, regWobble);
            cursor.Emit(OpCodes.Ldc_R4, 0.4f);
            cursor.Emit(OpCodes.Mul);
            cursor.MarkLabel(regWobble);
        }

        private static void LoadingThreadHook(ILContext ctx) {
            ILCursor cursor = new ILCursor(ctx);
            cursor.GotoNext(i => i.MatchNewobj<TotalStrawberriesDisplay>());
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<LevelLoader>>(loader => loader.Level.Add(new TotalShardDisplay(loader.Level.Session.Area)));
        }

        private static void DrawLerpHook(ILContext ctx) {
            ILCursor cursor = new ILCursor(ctx);
            while(cursor.TryGotoNext(i => i.MatchStfld(typeof(TotalShardDisplay), nameof(TotalShardDisplay.DrawLerp)))) {
                cursor.Emit(OpCodes.Dup);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<float, Level>>((lerp, lvl) => lvl.Tracker.GetEntity<TotalShardDisplay>().DrawLerp = lerp);
                cursor.Index++;
            }
        }
    }
}
