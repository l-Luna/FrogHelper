using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace FrogHelper.Entities {

    /// <summary>
    /// A strawberry that only appears when you collect all frogelines in a lobby's level set.
    /// </summary>
    [CustomEntity("FrogHelper/FrogBerry")]
    [RegisterStrawberry(tracked: false, blocksCollection: false)]
    public class FrogBerry : Strawberry {
        //Modified vanilla code
        private class FrogularText : Entity {
            private readonly bool isGhost;
            private readonly Sprite sprite;
            private readonly VertexLight light;
            private readonly BloomPoint bloom;
            private DisplacementRenderer.Burst burst;

            public FrogularText(Vector2 pos, bool ghost) : base(pos) {
                Add(sprite = GFX.SpriteBank.Create("FrogHelper_frogularText"));
                Add(light = new VertexLight(Color.White, 1f, 16, 24));
                Add(bloom = new BloomPoint(1f, 12f));
                Depth = Depths.FormationSequences - 100;
                Tag = Tags.Persistent | Tags.TransitionUpdate | Tags.FrozenUpdate;
                isGhost = ghost;
            }

            public override void Added(Scene scene) {
                base.Added(scene);

                sprite.Play("fade", false, false);
                sprite.OnFinish = _ => RemoveSelf();

                burst = SceneAs<Level>().Displacement.AddBurst(Position, 0.3f, 16f, 24f, 0.3f, null, null);
            }

            public override void Update() {
                Level level = SceneAs<Level>();
                if(level.Frozen) {
                    if(burst != null) {
                        burst.AlphaFrom = burst.AlphaTo = 0f;
                        burst.Percent = burst.Duration;
                    }
                    return;
                }

                base.Update();

                Camera camera = level.Camera;
                Y -= 8f * Engine.DeltaTime;
                X = Calc.Clamp(X, camera.Left + 8f, camera.Right - 8f);
                Y = Calc.Clamp(Y, camera.Top + 8f, camera.Bottom - 8f);

                bloom.Alpha = light.Alpha = Calc.Approach(light.Alpha, 0f, Engine.DeltaTime * 4f);

                ParticleType ptype = isGhost ? Strawberry.P_GhostGlow : Strawberry.P_MoonGlow;

                if(Scene.OnInterval(0.05f)) {
                    if(sprite.Color == ptype.Color2) sprite.Color = ptype.Color;
                    else sprite.Color = ptype.Color2;
                }

                if(Scene.OnInterval(0.06f) && sprite.CurrentAnimationFrame > 11) {
                    level.ParticlesFG.Emit(ptype, 1, Position + Vector2.UnitY * -2f, new Vector2(8f, 4f));
                }
            }
        }

        private Sprite sprite;
        private int numShardsRequired;

        public FrogBerry(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid) => numShardsRequired = data.Int("numShardsRequired");

        public override void Added(Scene scene) {
            base.Added(scene);

            //Replace the sprite
            sprite = new DynData<Strawberry>(this).Get<Sprite>("sprite");
            sprite = GFX.SpriteBank.CreateOn(sprite, FrogHelperModule.Instance.SaveData.LevelsWithFrogBerryCollected.Contains(SceneAs<Level>().Session.Area.SID) ? "FrogHelper_ghostFrogBerry" : "FrogHelper_frogBerry");
            sprite.OnFrameChange = OnAnimate;
            sprite.Play("idle");
        }

        public override void Awake(Scene scene) {
            //Check if enough frog shards have been collected in the same levelset
            if(FrogHelperModule.Instance.SaveData.CountCollectedFrogShards(SceneAs<Level>().Session.Area) < numShardsRequired) {
                RemoveSelf();
                return;
            }

            base.Awake(scene);
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

        //Modified vanilla code
        private IEnumerator CollectRoutine(int collectIdx) {
            FrogHelperModule.Instance.SaveData.LevelsWithFrogBerryCollected.Add(SceneAs<Level>().Session.Area.SID);

            Tag = Tags.TransitionUpdate;
            Depth = Depths.FormationSequences - 10;

            Audio.Play("event:/game/general/strawberry_get", Position, "colour", 3, "count", collectIdx);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

            sprite.Play("collect", false, false);
            while(sprite.Animating) yield return null;

            Scene.Add(new FrogularText(Position, new DynData<Strawberry>(this).Get<bool>("isGhostBerry")));
            RemoveSelf();
        }

        public static void Load() {
            IL.Celeste.Strawberry.ctor += ctorModifier;
            On.Celeste.Strawberry.CollectRoutine += CollectRoutineHook;
        }

        public static void Unload() {
            IL.Celeste.Strawberry.ctor -= ctorModifier;
            On.Celeste.Strawberry.CollectRoutine -= CollectRoutineHook;
        }

        private static void ctorModifier(ILContext ctx) {
            //Patch SaveData.Instance.CheckStrawberry calls
            FieldInfo idField = typeof(Strawberry).GetField(nameof(Strawberry.ID));

            ILCursor cursor = new ILCursor(ctx);
            while(cursor.TryGotoNext(i => i.MatchCallOrCallvirt(typeof(SaveData), nameof(SaveData.CheckStrawberry)))) {
                ILLabel origLabel = cursor.DefineLabel(), endLabel = cursor.DefineLabel();

                //Check if the strawberry is a shard, and the ID is the shard's
                cursor.MoveAfterLabels();
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Isinst, typeof(FrogBerryShard));
                cursor.Emit(OpCodes.Brfalse, origLabel);

                cursor.Emit(OpCodes.Dup);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, idField);
                cursor.EmitDelegate<Func<EntityID, EntityID, bool>>((a, b) => a.Level == b.Level && a.ID == b.ID);
                cursor.Emit(OpCodes.Brfalse, origLabel);

                //If yes: check if the frog berry has been collected
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate<Func<bool>>(() => FrogHelperModule.Instance.SaveData.LevelsWithFrogBerryCollected.Contains(SaveData.Instance.CurrentSession_Safe.Area.SID));
                cursor.Emit(OpCodes.Br, endLabel);

                //If no: execute regular code, then go to end
                cursor.MarkLabel(origLabel);
                cursor.Index++;

                cursor.MarkLabel(endLabel);
            }
        }

        private static IEnumerator CollectRoutineHook(On.Celeste.Strawberry.orig_CollectRoutine orig, Strawberry self, int collectIndex) {
            if(self is FrogBerry frogBerry) return frogBerry.CollectRoutine(collectIndex);
            return orig(self, collectIndex);
        }
    }
}
