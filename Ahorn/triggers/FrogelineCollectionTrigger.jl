module FrogHelperFrogelineCollectionTrigger

using ..Ahorn, Maple

@mapdef Trigger "FrogHelper/FrogelineCollectionTrigger" FrogelineCollectionTrigger(
    x::Integer,
    y::Integer,
    width::Integer=Maple.defaultTriggerWidth,
    height::Integer=Maple.defaultTriggerHeight,
)

const placements = Ahorn.PlacementDict(
    "Frogeline Collection Trigger (Frog Helper)" => Ahorn.EntityPlacement(
        FrogelineCollectionTrigger, 
        "rectangle"
    ),
)

end
