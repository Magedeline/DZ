module CrownBearing

using ..Ahorn, Maple

@mapdef Entity "DZ/CrownBearing" CrownBearing(x::Integer, y::Integer, gravityRadius::Integer=150, gravityStrength::Integer=200, gravityType::String="Pull", isActive::Bool=true)

const placements = Ahorn.PlacementDict(
    "pull" => Ahorn.EntityPlacement(CrownBearing),
    "push" => Ahorn.EntityPlacement(CrownBearing)
)

function Ahorn.selection(entity::CrownBearing)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CrownBearing, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
