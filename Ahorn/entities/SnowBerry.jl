module SnowBerry

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/SnowBerry" SnowBerry(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "SnowBerry" => Ahorn.EntityPlacement(SnowBerry)
)

function Ahorn.selection(entity::SnowBerry)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SnowBerry, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/snowberry", entity.x, entity.y)
end

end
