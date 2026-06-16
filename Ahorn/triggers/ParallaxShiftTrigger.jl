module ParallaxShiftTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/ParallaxShiftTrigger" ParallaxShiftTrigger(x::Integer, y::Integer, height::Integer=16, parallaxX::Number=0.5, parallaxY::Number=0.5, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "ParallaxShiftTrigger" => Ahorn.EntityPlacement(ParallaxShiftTrigger)
)

function Ahorn.selection(entity::ParallaxShiftTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ParallaxShiftTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
