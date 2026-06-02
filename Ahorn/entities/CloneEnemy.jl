module CloneEnemy

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/CloneEnemy" CloneEnemy(x::Integer, y::Integer, delaySeconds::Number=2.0, health::Integer=1)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(CloneEnemy),
    "short_delay" => Ahorn.EntityPlacement(CloneEnemy)
)

function Ahorn.selection(entity::CloneEnemy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CloneEnemy, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
