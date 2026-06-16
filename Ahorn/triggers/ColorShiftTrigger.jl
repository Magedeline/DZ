module ColorShiftTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/ColorShiftTrigger" ColorShiftTrigger(x::Integer, y::Integer, blendStrength::Number=0.5, height::Integer=16, targetColor::String="d2b48c", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "sepia" => Ahorn.EntityPlacement(ColorShiftTrigger),
    "blue_tint" => Ahorn.EntityPlacement(ColorShiftTrigger)
)

function Ahorn.selection(entity::ColorShiftTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ColorShiftTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
