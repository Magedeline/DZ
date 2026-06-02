module CardShark

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/CardShark" CardShark(x::Integer, y::Integer, cardsPerThrow::Integer=3, detectionRange::Integer=180, health::Integer=2, patrolDistance::Integer=80, throwInterval::Number=1.5)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(CardShark)
)

function Ahorn.selection(entity::CardShark)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CardShark, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
