module RainbowBridge

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/RainbowBridge" RainbowBridge(x::Integer, y::Integer, height::Integer=8, width::Integer=64)

const placements = Ahorn.PlacementDict(
    "RainbowBridge" => Ahorn.EntityPlacement(RainbowBridge)
)

function Ahorn.selection(entity::RainbowBridge)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RainbowBridge, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (1.0, 0.5, 0.5, 0.3), (1.0, 0.3, 0.3, 0.6))
end

end
