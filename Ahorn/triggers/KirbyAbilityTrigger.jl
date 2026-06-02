module KirbyAbilityTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/KirbyAbilityTrigger" KirbyAbilityTrigger(x::Integer, y::Integer, ability::String="Fire", action::String="Give", height::Integer=16, onlyOnce::Bool=false, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "give_ability" => Ahorn.EntityPlacement(KirbyAbilityTrigger),
    "remove_ability" => Ahorn.EntityPlacement(KirbyAbilityTrigger),
    "toggle_float" => Ahorn.EntityPlacement(KirbyAbilityTrigger),
    "toggle_inhale" => Ahorn.EntityPlacement(KirbyAbilityTrigger),
    "give_fire" => Ahorn.EntityPlacement(KirbyAbilityTrigger),
    "give_ice" => Ahorn.EntityPlacement(KirbyAbilityTrigger),
    "give_sword" => Ahorn.EntityPlacement(KirbyAbilityTrigger),
    "give_beam" => Ahorn.EntityPlacement(KirbyAbilityTrigger),
    "give_hammer" => Ahorn.EntityPlacement(KirbyAbilityTrigger),
    "give_knight" => Ahorn.EntityPlacement(KirbyAbilityTrigger)
)

function Ahorn.selection(entity::KirbyAbilityTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyAbilityTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
