module TumbleweedCluster

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/TumbleweedCluster" TumbleweedCluster(x::Integer, y::Integer, bounceChance::Number=0.3, pushForce::Integer=100, rollSpeed::Integer=180, tumbleweedCount::Integer=3)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(TumbleweedCluster)
)

function Ahorn.selection(entity::TumbleweedCluster)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TumbleweedCluster, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
