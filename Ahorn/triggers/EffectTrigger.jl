module EffectTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/EffectTrigger" EffectTrigger(x::Integer, y::Integer, duration::Number=3.0, effectType::String="sparkles", height::Integer=16, intensity::Number=1.0, triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "particle" => Ahorn.EntityPlacement(EffectTrigger),
    "screen_shake" => Ahorn.EntityPlacement(EffectTrigger),
    "color_grade" => Ahorn.EntityPlacement(EffectTrigger)
)

function Ahorn.selection(entity::EffectTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::EffectTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
