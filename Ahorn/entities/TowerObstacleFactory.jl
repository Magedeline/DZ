module TowerObstacleFactory

using ..Ahorn, Maple

@mapdef Entity "DZ/TowerObstacleFactory" TowerObstacleFactory(x::Integer, y::Integer, activationDelay::Number=0.0, autoPositionAroundTower::Bool=true, backgroundStyle::String="Default", createBackground::Bool=true, createObstacles::Bool=true, obstacleCount::Integer=15, obstaclePattern::String="Rings", obstacleSetType::String="Beginner", patternRotation::Number=0.0, towerRadius::Number=120.0, verticalSpacing::Number=150.0)

const placements = Ahorn.PlacementDict(
    "beginner_set" => Ahorn.EntityPlacement(TowerObstacleFactory),
    "intermediate_set" => Ahorn.EntityPlacement(TowerObstacleFactory),
    "advanced_set" => Ahorn.EntityPlacement(TowerObstacleFactory),
    "expert_set" => Ahorn.EntityPlacement(TowerObstacleFactory),
    "random_set" => Ahorn.EntityPlacement(TowerObstacleFactory),
    "pattern_only" => Ahorn.EntityPlacement(TowerObstacleFactory),
    "background_only" => Ahorn.EntityPlacement(TowerObstacleFactory),
    "complete_setup" => Ahorn.EntityPlacement(TowerObstacleFactory)
)

function Ahorn.selection(entity::TowerObstacleFactory)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TowerObstacleFactory, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
