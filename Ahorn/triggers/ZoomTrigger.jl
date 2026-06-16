module ZoomTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/ZoomTrigger" ZoomTrigger(x::Integer, y::Integer, height::Integer=16, targetZoom::Number=2.0, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "zoom_in" => Ahorn.EntityPlacement(ZoomTrigger),
    "zoom_out" => Ahorn.EntityPlacement(ZoomTrigger)
)

function Ahorn.selection(entity::ZoomTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ZoomTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
