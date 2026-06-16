module DXSpikeBallChain

using ..Ahorn, Maple

@mapdef Entity "DZ/DXSpikeBallChain" DXSpikeBallChain(x::Integer, y::Integer, ballRadius::Number=12.0, chainLength::Number=80.0, swingAngle::Number=1.5708, swingSpeed::Number=2.0)

const placements = Ahorn.PlacementDict(
    "DXSpikeBallChain" => Ahorn.EntityPlacement(DXSpikeBallChain),
    "DXSpikeBallChain_Long" => Ahorn.EntityPlacement(DXSpikeBallChain),
    "DXSpikeBallChain_Fast" => Ahorn.EntityPlacement(DXSpikeBallChain)
)

function Ahorn.selection(entity::DXSpikeBallChain)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DXSpikeBallChain, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
