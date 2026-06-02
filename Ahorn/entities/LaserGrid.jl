module LaserGrid

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/LaserGrid" LaserGrid(x::Integer, y::Integer, height::Integer=64, offTime::Number=2.0, onTime::Number=2.0, startOn::Bool=true, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "lazpewpewnormal" => Ahorn.EntityPlacement(LaserGrid),
    "lazpewpewalternating" => Ahorn.EntityPlacement(LaserGrid)
)

function Ahorn.selection(entity::LaserGrid)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::LaserGrid, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
