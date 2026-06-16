module SpeedModifierTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/SpeedModifierTrigger" SpeedModifierTrigger(x::Integer, y::Integer, affectsX::Bool=true, height::Integer=32, speedMultiplier::Number=0.5, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "slow" => Ahorn.EntityPlacement(SpeedModifierTrigger),
    "fast" => Ahorn.EntityPlacement(SpeedModifierTrigger)
)

function Ahorn.selection(entity::SpeedModifierTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpeedModifierTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
