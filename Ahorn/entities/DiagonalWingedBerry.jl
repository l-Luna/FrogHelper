module FrogHelperDiagonalWingedBerry

using ..Ahorn, Maple

@mapdef Entity "FrogHelper/DiagonalWingedBerry" DiagonalWingedBerry(
    x::Integer,
    y::Integer,
)

const placements = Ahorn.PlacementDict(
    "Diagonal Winged Berry (Frogeline Helper)" => Ahorn.EntityPlacement(
        DiagonalWingedBerry,
    )
)

const sprite = "collectables/strawberry/wings01"

function Ahorn.selection(entity::DiagonalWingedBerry)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 9, y - 8, 18, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DiagonalWingedBerry, room::Maple.Room)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end