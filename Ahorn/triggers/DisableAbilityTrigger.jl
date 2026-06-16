module DisableAbilityTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/DisableAbilityTrigger" DisableAbilityTrigger(x::Integer, y::Integer, disableDash::Bool=true, disableGrab::Bool=false, height::Integer=32, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "no_dash" => Ahorn.EntityPlacement(DisableAbilityTrigger),
    "no_grab" => Ahorn.EntityPlacement(DisableAbilityTrigger)
)

function Ahorn.selection(entity::DisableAbilityTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DisableAbilityTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
