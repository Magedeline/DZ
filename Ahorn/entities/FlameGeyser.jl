module FlameGeyser

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/FlameGeyser" FlameGeyser(x::Integer, y::Integer, damageRadius::Integer=30, eruptDuration::Number=1.0, eruptInterval::Number=4.0, flameHeight::Integer=200, warningTime::Number=1.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(FlameGeyser)
)

function Ahorn.selection(entity::FlameGeyser)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FlameGeyser, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
