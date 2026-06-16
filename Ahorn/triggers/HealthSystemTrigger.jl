module HealthSystemTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/HealthSystemTrigger" HealthSystemTrigger(x::Integer, y::Integer, displayMode::Integer=0, healAmount::Integer=0, healOnEnter::Bool=false, height::Integer=16, kirbyMode::Bool=false, maxHP::Integer=6, persistent::Bool=true, showUI::Bool=true, trackBosses::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "Health System Trigger" => Ahorn.EntityPlacement(HealthSystemTrigger),
    "Health System Trigger (Kirby Mode)" => Ahorn.EntityPlacement(HealthSystemTrigger)
)

function Ahorn.selection(entity::HealthSystemTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HealthSystemTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
