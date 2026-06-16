module ElementalEffectTrigger

using ..Ahorn, Maple

@mapdef Entity "DZ/ElementalEffectTrigger" ElementalEffectTrigger(x::Integer, y::Integer, duration::Number=1.0, effectType::String="fire_burst", elementType::String="Fire", height::Integer=16, intensity::Number=1.0, oneUse::Bool=false, radius::Number=32.0, triggerOnEnter::Bool=true, triggerOnExit::Bool=false, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "fire_effect" => Ahorn.EntityPlacement(ElementalEffectTrigger),
    "ice_effect" => Ahorn.EntityPlacement(ElementalEffectTrigger),
    "lightning_effect" => Ahorn.EntityPlacement(ElementalEffectTrigger),
    "earth_effect" => Ahorn.EntityPlacement(ElementalEffectTrigger)
)

function Ahorn.selection(entity::ElementalEffectTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ElementalEffectTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
