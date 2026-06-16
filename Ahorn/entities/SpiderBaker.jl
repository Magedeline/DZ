module SpiderBaker

using ..Ahorn, Maple

@mapdef Entity "DZ/SpiderBaker" SpiderBaker(x::Integer, y::Integer, detectionRange::Integer=100, health::Integer=2, startFriendly::Bool=false, webY::Integer=-80)

const placements = Ahorn.PlacementDict(
    "neutral" => Ahorn.EntityPlacement(SpiderBaker),
    "friendly" => Ahorn.EntityPlacement(SpiderBaker)
)

function Ahorn.selection(entity::SpiderBaker)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpiderBaker, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
