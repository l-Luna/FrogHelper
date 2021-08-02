module FrogHelperWingedSilver

using ..Ahorn, Maple

@mapdef Entity "FrogHelper/WingedSilver" WingedSilver(
    x::Integer,
    y::Integer,
    condition::String="dashless",
)

const placements = Ahorn.PlacementDict(
    "Winged Silver (Frogeline Helper)" => Ahorn.EntityPlacement(
        WingedSilver,
    )
)

const sprite = "objects/FrogHelper/wingedSilver/idle00"

function Ahorn.selection(entity::WingedSilver)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 9, y - 8, 18, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WingedSilver, room::Maple.Room)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end