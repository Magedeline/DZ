module Tower3D

using ..Ahorn, Maple

@mapdef Entity "DZ/Tower3D" Tower3D(x::Integer, y::Integer, autoCreateObstacles::Bool=true, backgroundStyle::String="default", climbingEnabled::Bool=true, climbingSpeed::Integer=100, obstacleSetType::String="intermediate", overworldPosition::String="center", overworldScale::Number=1.0, overworldVisible::Bool=true, radius::Integer=120, renderIn3D::Bool=true, rotationSpeed::Number=1.0, segments::Integer=8, towerHeight::Integer=1000)

const placements = Ahorn.PlacementDict(
    "basic_tower" => Ahorn.EntityPlacement(Tower3D),
    "beginner_tower" => Ahorn.EntityPlacement(Tower3D),
    "expert_tower" => Ahorn.EntityPlacement(Tower3D),
    "overworld_model" => Ahorn.EntityPlacement(Tower3D)
)

function Ahorn.selection(entity::Tower3D)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Tower3D, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/IngesteHelper/tower3d/base", entity.x, entity.y)
end

end
