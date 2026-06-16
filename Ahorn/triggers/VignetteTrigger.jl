module VignetteTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/VignetteTrigger" VignetteTrigger(x::Integer, y::Integer, height::Integer=16, vignetteStrength::Number=0.5, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "VignetteTrigger" => Ahorn.EntityPlacement(VignetteTrigger),
    "heavy" => Ahorn.EntityPlacement(VignetteTrigger)
)

function Ahorn.selection(entity::VignetteTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::VignetteTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
