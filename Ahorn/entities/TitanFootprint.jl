module TitanFootprint

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/TitanFootprint" TitanFootprint(x::Integer, y::Integer, cooldownTime::Number=3.0, crushDuration::Number=0.3, crushHeight::Integer=200, crushWidth::Integer=120, triggerDistance::Integer=80, warningDuration::Number=1.5)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(TitanFootprint)
)

function Ahorn.selection(entity::TitanFootprint)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TitanFootprint, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
