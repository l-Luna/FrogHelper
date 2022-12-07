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
        private Sprite sprite;

        public FrogBerryShard(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid) {}

        public override void Added(Scene scene) {
            base.Added(scene);

            //Replace the sprite
            sprite = new DynData<Strawberry>(this).Get<Sprite>("sprite");
            sprite = GFX.SpriteBank.CreateOn(sprite, SaveData.Instance.CheckStrawberry(ID) ? "FrogHelper_ghostFrogBerryShard" : "FrogHelper_frogBerryShard");
            sprite.OnFrameChange = OnAnimate;
            sprite.Play("idle");
        }

        //Modified vanilla code
        private IEnumerator CollectRoutine(int collectIdx) {
            FrogHelperModule.Instance.SaveData.LevelsWithFrogelineCollected.Add(SceneAs<Level>().Session.Area.SID);

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
                if(dynDat.Get<bool>("uncollected") && (CollideCheck<FakeWall>() || CollideCheck<Solid>())) {
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
        }

        public static void Unload() {
            On.Celeste.Strawberry.CollectRoutine -= CollectRoutineHook;
            updateHook?.Dispose();
            updateHook = null;
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
    }
}
