module FrostHelperIntegration

using ..Ahorn, Maple

@mapdef Entity "DZ/FrostHelperIntegration" FrostHelperIntegration(x::Integer, y::Integer, compatibilityMode::Bool=true, debugOutput::Bool=false, integrationMode::String="Auto", shareParticles::Bool=true, shareUtilities::Bool=true)

const placements = Ahorn.PlacementDict(
    "frosthelper" => Ahorn.EntityPlacement(FrostHelperIntegration),
    "debug_mode" => Ahorn.EntityPlacement(FrostHelperIntegration),
    "force_enable" => Ahorn.EntityPlacement(FrostHelperIntegration)
)

function Ahorn.selection(entity::FrostHelperIntegration)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FrostHelperIntegration, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/IngesteHelper/frost_integration", entity.x, entity.y)
end

# Nodes: min=0, max=inf
# Basic node rendering not implemented in auto-generated plugin

end
