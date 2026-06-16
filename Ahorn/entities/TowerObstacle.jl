module TowerObstacle

using ..Ahorn, Maple

@mapdef Entity "DZ/TowerObstacle" TowerObstacle(x::Integer, y::Integer, activationDelay::Number=0.0, damageRadius::Number=16.0, detectionRange::Number=100.0, height::Number=0.0, moveSpeed::Number=50.0, movementPattern::String="Static", obstacleType::String="Spikes", rotationSpeed::Number=1.0)

const placements = Ahorn.PlacementDict(
    "static_spikes" => Ahorn.EntityPlacement(TowerObstacle),
    "rotating_spinner" => Ahorn.EntityPlacement(TowerObstacle),
    "moving_platform" => Ahorn.EntityPlacement(TowerObstacle),
    "falling_block" => Ahorn.EntityPlacement(TowerObstacle),
    "laser_beam" => Ahorn.EntityPlacement(TowerObstacle),
    "wind_tunnel" => Ahorn.EntityPlacement(TowerObstacle),
    "portal" => Ahorn.EntityPlacement(TowerObstacle),
    "ping_pong_platform" => Ahorn.EntityPlacement(TowerObstacle),
    "following_spinner" => Ahorn.EntityPlacement(TowerObstacle),
    "vertical_mover" => Ahorn.EntityPlacement(TowerObstacle)
)

function Ahorn.selection(entity::TowerObstacle)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TowerObstacle, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
