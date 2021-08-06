module FrogHelperLookoutZoomFixTrigger

using ..Ahorn, Maple

@mapdef Trigger "FrogHelper/LookoutZoomFixTrigger" LookoutZoomFixTrigger(
    x::Integer,
    y::Integer,
    width::Integer=Maple.defaultTriggerWidth,
    height::Integer=Maple.defaultTriggerHeight,
)

const placements = Ahorn.PlacementDict(
    "Lookout Zoom Fix Trigger (Frog Helper)" => Ahorn.EntityPlacement(
        LookoutZoomFixTrigger, 
        "rectangle"
    ),
)

end
