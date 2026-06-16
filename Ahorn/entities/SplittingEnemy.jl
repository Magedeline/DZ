module SplittingEnemy

using ..Ahorn, Maple

@mapdef Entity "DZ/SplittingEnemy" SplittingEnemy(x::Integer, y::Integer, health::Integer=2, isSmall::Bool=false, splitCount::Integer=2)

const placements = Ahorn.PlacementDict(
    "SplittingEnemy" => Ahorn.EntityPlacement(SplittingEnemy),
    "SplittingEnemy_triple_split" => Ahorn.EntityPlacement(SplittingEnemy)
)

function Ahorn.selection(entity::SplittingEnemy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SplittingEnemy, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
