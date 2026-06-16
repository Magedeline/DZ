module SpiralStaircase

using ..Ahorn, Maple

@mapdef Entity "DZ/SpiralStaircase" SpiralStaircase(x::Integer, y::Integer, clockwise::Bool=true, maxSpeed::Number=2.0, platformCount::Integer=8, radius::Integer=100, rotationSpeed::Number=0.5)

const placements = Ahorn.PlacementDict(
    "clockwise" => Ahorn.EntityPlacement(SpiralStaircase),
    "counterclockwise" => Ahorn.EntityPlacement(SpiralStaircase)
)

function Ahorn.selection(entity::SpiralStaircase)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpiralStaircase, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
