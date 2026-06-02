module BarrelBomber

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/BarrelBomber" BarrelBomber(x::Integer, y::Integer, detectionRange::Integer=80, explosionRadius::Integer=100, fuseTime::Number=1.5, health::Integer=1)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(BarrelBomber)
)

function Ahorn.selection(entity::BarrelBomber)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BarrelBomber, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
