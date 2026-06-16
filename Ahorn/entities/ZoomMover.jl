module ZoomMover

using ..Ahorn, Maple

@mapdef Entity "DZ/ZoomMover" ZoomMover(x::Integer, y::Integer, height::Integer=16, moveSpeed::Number=300.0, permanent::Bool=false, theme::String="Normal", timed::Bool=false, waits::Bool=false, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(ZoomMover),
    "moon" => Ahorn.EntityPlacement(ZoomMover),
    "foundlevels" => Ahorn.EntityPlacement(ZoomMover),
    "finallevels" => Ahorn.EntityPlacement(ZoomMover)
)

function Ahorn.selection(entity::ZoomMover)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ZoomMover, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=1, max=1
# Basic node rendering not implemented in auto-generated plugin

end
