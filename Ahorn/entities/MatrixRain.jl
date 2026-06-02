module MatrixRain

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/MatrixRain" MatrixRain(x::Integer, y::Integer, density::Number=0.5, dropSpeed::Integer=150, intensity::String="Normal", rainHeight::Integer=400, rainWidth::Integer=200)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(MatrixRain)
)

function Ahorn.selection(entity::MatrixRain)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MatrixRain, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
