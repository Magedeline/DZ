module DXFloweyOmegaBoss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/DXFloweyOmegaBoss" DXFloweyOmegaBoss(x::Integer, y::Integer, autoStart::Bool=true, bossMusic::String="", corruptionLevel::Number=0.0, enableAbyssalCathedral::Bool=true, enableArsenalOverdrive::Bool=true, enableCorruptedGarden::Bool=true, enableCorruptionOverload::Bool=true, enableNightmareNexus::Bool=true, enableSoulHarvest::Bool=true, maxCorruption::Number=100.0, maxHealth::Integer=1500, showHealthBar::Bool=true)

const placements = Ahorn.PlacementDict(
    "DXFloweyOmega" => Ahorn.EntityPlacement(DXFloweyOmegaBoss),
    "DXFloweyOmega_Nightmare" => Ahorn.EntityPlacement(DXFloweyOmegaBoss)
)

function Ahorn.selection(entity::DXFloweyOmegaBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DXFloweyOmegaBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=0, max=inf
# Basic node rendering not implemented in auto-generated plugin

end
