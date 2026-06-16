module DashCodeGate

using ..Ahorn, Maple

@mapdef Entity "DZ/DashCodeGate" DashCodeGate(x::Integer, y::Integer, columns::Integer=1, height::Integer=16, iconOrientation::String="Auto", persistenceFlag::String="", sprite::String="stars", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(DashCodeGate)
)

function Ahorn.selection(entity::DashCodeGate)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DashCodeGate, room::Maple.Room)
    Ahorn.drawSprite(ctx, "stars", entity.x, entity.y)
end

# Nodes: min=1, max=1
# Basic node rendering not implemented in auto-generated plugin

end
