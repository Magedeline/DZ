module MagnetRail

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/MagnetRail" MagnetRail(x::Integer, y::Integer, speed::Number=120.0)

const placements = Ahorn.PlacementDict(
    "MagnetRail" => Ahorn.EntityPlacement(MagnetRail),
    "fast" => Ahorn.EntityPlacement(MagnetRail),
    "slow" => Ahorn.EntityPlacement(MagnetRail)
)

function Ahorn.selection(entity::MagnetRail)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MagnetRail, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
