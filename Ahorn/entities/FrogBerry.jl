module FrogHelperDiagonalWingedBerry

using ..Ahorn, Maple

@mapdef Entity "FrogHelper/FrogBerry" FrogBerry(
    x::Integer,
    y::Integer
)

const placements = Ahorn.PlacementDict(
    "Frog Berry (Frogeline Helper)" => Ahorn.EntityPlacement(
        FrogBerry,
    )
)

const sprite = "objects/FrogHelper/frogBerry/normal00"

function Ahorn.selection(entity::FrogBerry)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 9, y - 8, 18, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FrogBerry, room::Maple.Room)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end