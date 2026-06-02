module KirbyKnightTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/KirbyKnightTrigger" KirbyKnightTrigger(x::Integer, y::Integer, autoTransform::Bool=false, height::Integer=16, mode::String="Enable", onlyOnce::Bool=true, playEffects::Bool=true, requiredFlag::String="", transformDelay::Number=0.0, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "knight_enable" => Ahorn.EntityPlacement(KirbyKnightTrigger),
    "knight_disable" => Ahorn.EntityPlacement(KirbyKnightTrigger),
    "knight_toggle" => Ahorn.EntityPlacement(KirbyKnightTrigger),
    "knight_final_run" => Ahorn.EntityPlacement(KirbyKnightTrigger),
    "knight_last_push" => Ahorn.EntityPlacement(KirbyKnightTrigger),
    "knight_unlock" => Ahorn.EntityPlacement(KirbyKnightTrigger),
    "knight_delayed_enable" => Ahorn.EntityPlacement(KirbyKnightTrigger)
)

function Ahorn.selection(entity::KirbyKnightTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyKnightTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
