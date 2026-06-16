module PixelEnemy

using ..Ahorn, Maple

@mapdef Entity "DZ/PixelEnemy" PixelEnemy(x::Integer, y::Integer, detectionRange::Integer=150, enemyType::String="Walker", gridSize::Integer=8, health::Integer=2, moveSpeed::Integer=60)

const placements = Ahorn.PlacementDict(
    "walker" => Ahorn.EntityPlacement(PixelEnemy),
    "shooter" => Ahorn.EntityPlacement(PixelEnemy),
    "dasher" => Ahorn.EntityPlacement(PixelEnemy)
)

function Ahorn.selection(entity::PixelEnemy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PixelEnemy, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
