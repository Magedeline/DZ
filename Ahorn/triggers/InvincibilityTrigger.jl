module InvincibilityTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/InvincibilityTrigger" InvincibilityTrigger(x::Integer, y::Integer, duration::Number=5.0, height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "InvincibilityTrigger" => Ahorn.EntityPlacement(InvincibilityTrigger),
    "star_power" => Ahorn.EntityPlacement(InvincibilityTrigger)
)

function Ahorn.selection(entity::InvincibilityTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::InvincibilityTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
