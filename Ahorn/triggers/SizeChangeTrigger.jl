module SizeChangeTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/SizeChangeTrigger" SizeChangeTrigger(x::Integer, y::Integer, height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "shrink" => Ahorn.EntityPlacement(SizeChangeTrigger),
    "grow" => Ahorn.EntityPlacement(SizeChangeTrigger)
)

function Ahorn.selection(entity::SizeChangeTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SizeChangeTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
