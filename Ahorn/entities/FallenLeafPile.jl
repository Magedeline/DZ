module FallenLeafPile

using ..Ahorn, Maple

@mapdef Entity "DZ/FallenLeafPile" FallenLeafPile(x::Integer, y::Integer, collectibleType::String="", detectionRange::Integer=40, enemyType::String="DZ/RuinsSentinel", hiddenContent::String="Nothing")

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(FallenLeafPile),
    "spikes" => Ahorn.EntityPlacement(FallenLeafPile),
    "enemy" => Ahorn.EntityPlacement(FallenLeafPile),
    "collectible" => Ahorn.EntityPlacement(FallenLeafPile)
)

function Ahorn.selection(entity::FallenLeafPile)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FallenLeafPile, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
