module SplitScreenTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/SplitScreenTrigger" SplitScreenTrigger(x::Integer, y::Integer, height::Integer=16, splitDirection::String="Horizontal", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "horizontal" => Ahorn.EntityPlacement(SplitScreenTrigger),
    "vertical" => Ahorn.EntityPlacement(SplitScreenTrigger)
)

function Ahorn.selection(entity::SplitScreenTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SplitScreenTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
