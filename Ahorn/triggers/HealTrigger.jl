module HealTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/HealTrigger" HealTrigger(x::Integer, y::Integer, fullHeal::Bool=false, healAmount::Integer=1, height::Integer=16, onlyOnce::Bool=true, removeAfterUse::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "Heal Trigger" => Ahorn.EntityPlacement(HealTrigger),
    "Full Heal Trigger" => Ahorn.EntityPlacement(HealTrigger)
)

function Ahorn.selection(entity::HealTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HealTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
