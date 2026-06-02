module BombWaddleDee

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/BombWaddleDee" BombWaddleDee(x::Integer, y::Integer, health::Integer=1, throwInterval::Number=2.0, throwRange::Number=120.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(BombWaddleDee),
    "rapid" => Ahorn.EntityPlacement(BombWaddleDee)
)

function Ahorn.selection(entity::BombWaddleDee)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BombWaddleDee, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
