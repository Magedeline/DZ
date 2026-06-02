module Tower3DTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/Tower3DTrigger" Tower3DTrigger(x::Integer, y::Integer, climbingSpeed::Number=100.0, enableClimbing::Bool=true, flagToSet::String="", height::Integer=16, oneUse::Bool=true, requirePlayerOnGround::Bool=false, requiredFlag::String="", rotationSpeed::Number=1.0, towerHeight::Number=1000.0, triggerDelay::Number=0.0, triggerType::String="Activate", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "activate_tower" => Ahorn.EntityPlacement(Tower3DTrigger),
    "deactivate_tower" => Ahorn.EntityPlacement(Tower3DTrigger),
    "modify_tower" => Ahorn.EntityPlacement(Tower3DTrigger),
    "tower_checkpoint" => Ahorn.EntityPlacement(Tower3DTrigger)
)

function Ahorn.selection(entity::Tower3DTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Tower3DTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
