module WaterFlowPuzzle

using ..Ahorn, Maple

@mapdef Entity "DesoloZatnas/WaterFlowPuzzle" WaterFlowPuzzle(x::Integer, y::Integer, anvilWeight::Integer=20, currentVolume::Integer=0, dialogCorrect::String="CH11_GATEWAY_LIFTED", dialogIncorrect::String="CH11_WRONG_VOLUME_WATER", flowRate::Integer=10, height::Integer=32, leverCount::Integer=3, maxVolume::Integer=100, requiresAnvil::Bool=true, requiresBalance::Bool=true, targetVolume::Integer=50, tolerance::Integer=5, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "water_flow_puzzle" => Ahorn.EntityPlacement(WaterFlowPuzzle)
)

function Ahorn.selection(entity::WaterFlowPuzzle)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WaterFlowPuzzle, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
