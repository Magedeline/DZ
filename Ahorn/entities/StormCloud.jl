module StormCloud

using ..Ahorn, Maple

@mapdef Entity "DZ/StormCloud" StormCloud(x::Integer, y::Integer, strikeInterval::Number=4.0, strikeRadius::Number=32.0)

const placements = Ahorn.PlacementDict(
    "StormCloud" => Ahorn.EntityPlacement(StormCloud),
    "StormCloud_frequent" => Ahorn.EntityPlacement(StormCloud)
)

function Ahorn.selection(entity::StormCloud)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::StormCloud, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
