module DamageTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/DamageTrigger" DamageTrigger(x::Integer, y::Integer, cooldown::Number=1.0, damage::Integer=1, height::Integer=16, removeAfterHit::Bool=false, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "Damage Trigger" => Ahorn.EntityPlacement(DamageTrigger),
    "Damage Trigger (One Time)" => Ahorn.EntityPlacement(DamageTrigger)
)

function Ahorn.selection(entity::DamageTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DamageTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
