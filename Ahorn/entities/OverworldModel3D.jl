module OverworldModel3D

using ..Ahorn, Maple

@mapdef Entity "DZ/OverworldModel3D" OverworldModel3D(x::Integer, y::Integer, animationSpeed::Number=1.0, animationType::String="idle", collisionEnabled::Bool=true, interactable::Bool=false, lightingEnabled::Bool=true, mapEditorVisible::Bool=true, materialType::String="stone", modelScale::Number=1.0, modelType::String="tower", overworldVisible::Bool=true, positionX::Number=0.0, positionY::Number=0.0, positionZ::Number=0.0, renderPriority::Integer=0, rotationX::Number=0.0, rotationY::Number=0.0, rotationZ::Number=0.0, shadowsEnabled::Bool=true)

const placements = Ahorn.PlacementDict(
    "tower_model" => Ahorn.EntityPlacement(OverworldModel3D),
    "building_model" => Ahorn.EntityPlacement(OverworldModel3D),
    "mountain_model" => Ahorn.EntityPlacement(OverworldModel3D),
    "crystal_model" => Ahorn.EntityPlacement(OverworldModel3D),
    "floating_platform" => Ahorn.EntityPlacement(OverworldModel3D)
)

function Ahorn.selection(entity::OverworldModel3D)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::OverworldModel3D, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/IngesteHelper/overworld3d/base", entity.x, entity.y)
end

end
