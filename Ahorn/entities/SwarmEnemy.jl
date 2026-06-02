module SwarmEnemy

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/SwarmEnemy" SwarmEnemy(x::Integer, y::Integer, count::Integer=5)

const placements = Ahorn.PlacementDict(
    "small_swarm" => Ahorn.EntityPlacement(SwarmEnemy),
    "large_swarm" => Ahorn.EntityPlacement(SwarmEnemy)
)

function Ahorn.selection(entity::SwarmEnemy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SwarmEnemy, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
