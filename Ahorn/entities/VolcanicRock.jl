module VolcanicRock

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/VolcanicRock" VolcanicRock(x::Integer, y::Integer, rockSpeed::Number=120.0, spawnInterval::Number=3.0)

const placements = Ahorn.PlacementDict(
    "VolcanicRock" => Ahorn.EntityPlacement(VolcanicRock),
    "VolcanicRock_intense" => Ahorn.EntityPlacement(VolcanicRock)
)

function Ahorn.selection(entity::VolcanicRock)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::VolcanicRock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
