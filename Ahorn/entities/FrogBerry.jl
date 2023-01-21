module FrogHelperDiagonalWingedBerry

using ..Ahorn, Maple

@mapdef Entity "FrogHelper/FrogBerry" FrogBerry(
    x::Integer,
    y::Integer,
    numShardsRequired::Integer=0
)

@mapdef Entity "FrogHelper/FrogBerryShard" FrogBerryShard(
    x::Integer,
    y::Integer,
    shardLevelSet::String=""
)

const placements = Ahorn.PlacementDict(
    "Frog Berry (Frogeline Helper)" => Ahorn.EntityPlacement(
        FrogBerry,
    ),
    "Frog Berry Shard (Frogeline Helper)" => Ahorn.EntityPlacement(
        FrogBerryShard,
    ),
)

function Ahorn.selection(entity::FrogBerry)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 9, y - 8, 18, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FrogBerry, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/FrogHelper/frogBerry/normal00", 0, 0)
end

function Ahorn.selection(entity::FrogBerryShard)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 9, y - 8, 18, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FrogBerryShard, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/FrogHelper/frogBerryShard/normal00", 0, 0)
end

end