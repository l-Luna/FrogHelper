using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace FrogHelper.Entities {

    [CustomEntity("FrogHelper/StylegroundsPanel")]
    [Tracked]
    public class StylegroundsPanel : Entity {

        public new int Width = 4, Height = 4;

        public bool Foreground = false;

        public float ScrollX = 1f, ScrollY = 1f, Opacity = 1f;

        public string Room;

        public StylegroundsPanel(EntityData data, Vector2 pos) : base(data.Position + pos){
            Width = data.Width;
            Height = data.Height;
            Foreground = data.Bool("foreground", false);
            ScrollX = data.Float("scrollX", 1);
            ScrollY = data.Float("scrollY", 1);
            Opacity = data.Float("opacity", 1);
            Room = data.Attr("room");
        }
    }
}