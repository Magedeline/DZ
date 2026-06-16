module StarBubble

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/StarBubble" StarBubble(x::Integer, y::Integer, duration::Number=5.0, floatSpeed::Number=80.0, immuneToHazards::Bool=true, respawnTime::Number=3.0)

const placements = Ahorn.PlacementDict(
    "Star Bubble" => Ahorn.EntityPlacement(StarBubble),
    "Star Bubble (Long Duration)" => Ahorn.EntityPlacement(StarBubble),
    "Star Bubble (Fast)" => Ahorn.EntityPlacement(StarBubble)
)

function Ahorn.selection(entity::StarBubble)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::StarBubble, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
