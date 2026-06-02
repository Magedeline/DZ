module StaminaModTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/StaminaModTrigger" StaminaModTrigger(x::Integer, y::Integer, height::Integer=32, staminaMultiplier::Number=999.0, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "infinite" => Ahorn.EntityPlacement(StaminaModTrigger),
    "half_stamina" => Ahorn.EntityPlacement(StaminaModTrigger)
)

function Ahorn.selection(entity::StaminaModTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::StaminaModTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
