using System.Collections.Generic;
using Celeste;
using Monocle;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FrogHelper.Entities {

    // Thanks communal helper, very cool
    public class StylegroundsPanelRenderer{

        private static VirtualRenderTarget MaskRenderTarget;
        private static VirtualRenderTarget StylegroundsRenderTarget;

        private static BlendState alphaMaskBlendState = new BlendState() {
            ColorSourceBlend = Blend.Zero,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.Zero,
        };

        private static Backdrop Test1 = new FinalBossStarfield();
        private static Backdrop Test2 = new WindSnowFG();

        public static void RenderStylegroundsPanels(bool fg, Scene level, BackdropRenderer renderer){
            if (MaskRenderTarget == null)
                MaskRenderTarget = VirtualContent.CreateRenderTarget("frog-helper-stylegrounds-mask", 320, 180);
            if (StylegroundsRenderTarget == null)
                StylegroundsRenderTarget = VirtualContent.CreateRenderTarget("frog-helper-stylegrounds-target", 320, 180);
            
            Camera camera = (level as Level).Camera;
            // find all of the panels we want to fill
            List<IGrouping<string, StylegroundsPanel>> toRender = level.Entities
                .FindAll<StylegroundsPanel>()
                .Where(it => it.Foreground == fg)
                .GroupBy(it => it.Room)
                .ToList();

            foreach (var item in toRender){
                var room = item.Key;
                foreach(Backdrop bg in renderer.Backdrops)
                    if(IsVisible(bg, level as Level, room)){
                        bool wasVisible = bg.Visible, wasForceVisible = bg.ForceVisible;
                        bg.Visible = true;
                        bg.ForceVisible = true;
                        bg.Update(level);
                        bg.BeforeRender(level);
                        bg.Visible = wasVisible;
                        bg.ForceVisible = wasForceVisible;
                    }
                // get our styleground mask
                Engine.Graphics.GraphicsDevice.SetRenderTarget(MaskRenderTarget);
                Engine.Graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));
                Draw.SpriteBatch.Begin();
                foreach (var panel in item)
                    Draw.Rect((panel.X - camera.Left - (320 / 2)) * panel.ScrollX + (320 / 2),
                              (panel.Y - camera.Top - (180 / 2)) * panel.ScrollY + (180 / 2),
                               panel.Width, panel.Height, Color.White * panel.Opacity);
                Draw.SpriteBatch.End();
                // render some styleground
                Engine.Graphics.GraphicsDevice.SetRenderTarget(StylegroundsRenderTarget);
                Engine.Graphics.GraphicsDevice.Clear(new Color(0, 0, 0, 0));
                RenderStylegroundsForRoom(room, renderer, level as Level);
                // apply the mask to the styleground
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, alphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect);
                Draw.SpriteBatch.Draw(MaskRenderTarget, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();
                // render the styleground
                Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);
                GameplayRenderer.Begin();
                Draw.SpriteBatch.Draw(StylegroundsRenderTarget, camera.Position, Color.White);
                GameplayRenderer.End();
            }
        }

        public static void RenderStylegroundsForRoom(string room, BackdropRenderer renderer, Level level){
            BlendState blendState = BlendState.AlphaBlend;
            bool usingSpritebatch = false;
            foreach(Backdrop backdrop in renderer.Backdrops){
                if (IsVisible(backdrop, level, room)){
                    if (backdrop is Parallax && (backdrop as Parallax).BlendState != blendState){
						if(usingSpritebatch)
				            Draw.SpriteBatch.End();
                        usingSpritebatch = false;
						blendState = (backdrop as Parallax).BlendState;
					}
					if (backdrop.UseSpritebatch && !usingSpritebatch){
						Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, blendState, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
                        usingSpritebatch = true;
					}
					if (!backdrop.UseSpritebatch && usingSpritebatch){
						if(usingSpritebatch)
				            Draw.SpriteBatch.End();
                        usingSpritebatch = false;
					}
					backdrop.Render(level);
                }
            }
            if(usingSpritebatch)
				Draw.SpriteBatch.End();
            usingSpritebatch = false;
        }

        public static void Cleanup(){
            if(MaskRenderTarget != null)
                MaskRenderTarget.Dispose();
            if(StylegroundsRenderTarget != null)
                StylegroundsRenderTarget.Dispose();
            MaskRenderTarget = StylegroundsRenderTarget = null;
        }

        public static void Load(){
            On.Celeste.BackdropRenderer.Render += OnBackdropRendererRender;
            On.Celeste.BackdropRenderer.Ended += OnBackdropRendererEnded;
        }

        public static void Unload(){
            On.Celeste.BackdropRenderer.Render -= OnBackdropRendererRender;
            On.Celeste.BackdropRenderer.Ended -= OnBackdropRendererEnded;
        }

        public static void OnBackdropRendererRender(On.Celeste.BackdropRenderer.orig_Render orig, BackdropRenderer self, Scene scene){
            orig(self, scene);
            if(scene is Level level)
                if(self == level.Foreground)
                    RenderStylegroundsPanels(true, level, self);
                else if(self == level.Background)
                    RenderStylegroundsPanels(false, level, self);
        }

        public static void OnBackdropRendererEnded(On.Celeste.BackdropRenderer.orig_Ended orig, BackdropRenderer self, Scene scene){
            orig(self, scene);
            Cleanup();
        }

        private static bool IsVisible(Backdrop styleground, Level level, string room)
		{
			if (styleground.ForceVisible)
			{
				return true;
			}
			if (!string.IsNullOrEmpty(styleground.OnlyIfNotFlag) && level.Session.GetFlag(styleground.OnlyIfNotFlag))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(styleground.AlsoIfFlag) && level.Session.GetFlag(styleground.AlsoIfFlag))
			{
				return true;
			}
			if (styleground.Dreaming.HasValue && styleground.Dreaming.Value != level.Session.Dreaming)
			{
				return false;
			}
			if (!string.IsNullOrEmpty(styleground.OnlyIfFlag) && !level.Session.GetFlag(styleground.OnlyIfFlag))
			{
				return false;
			}
			if (styleground.ExcludeFrom != null && styleground.ExcludeFrom.Contains(room))
			{
				return false;
			}
			if (styleground.OnlyIn != null && !styleground.OnlyIn.Contains(room))
			{
				return false;
			}
			return true;
		}
    }
}