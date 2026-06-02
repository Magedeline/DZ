module AbilitySwapTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/AbilitySwapTrigger" AbilitySwapTrigger(x::Integer, y::Integer, abilityName::String="Sword", height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "AbilitySwapTrigger" => Ahorn.EntityPlacement(AbilitySwapTrigger)
)

function Ahorn.selection(entity::AbilitySwapTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AbilitySwapTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
