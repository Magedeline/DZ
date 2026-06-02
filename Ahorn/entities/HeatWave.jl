module HeatWave

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/HeatWave" HeatWave(x::Integer, y::Integer, expansionSpeed::Integer=100, interval::Number=5.0, isActive::Bool=true, maxRadius::Integer=150, pushForce::Integer=150)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(HeatWave)
)

function Ahorn.selection(entity::HeatWave)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HeatWave, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
