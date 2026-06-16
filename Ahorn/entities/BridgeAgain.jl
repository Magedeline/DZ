module BridgeAgain

using ..Ahorn, Maple

@mapdef Entity "DZ/BridgeAgain" BridgeAgain(x::Integer, y::Integer, getLevelFlag::String="", height::Integer=8, width::Integer=160)

const placements = Ahorn.PlacementDict(
    "BridgeAgain" => Ahorn.EntityPlacement(BridgeAgain)
)

function Ahorn.selection(entity::BridgeAgain)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BridgeAgain, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/bridge/bridge00", entity.x, entity.y)
end

# Nodes: min=2, max=2
# Basic node rendering not implemented in auto-generated plugin

end
