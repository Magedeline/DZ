module HeartGemSattelite

using ..Ahorn, Maple

@mapdef Entity "DZ/HeartGemSattelite" HeartGemSattelite(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "HeartGemSattelite" => Ahorn.EntityPlacement(HeartGemSattelite)
)

function Ahorn.selection(entity::HeartGemSattelite)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HeartGemSattelite, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=2, max=2
# Basic node rendering not implemented in auto-generated plugin

end
