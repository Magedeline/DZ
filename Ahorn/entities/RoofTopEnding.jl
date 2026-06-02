module RoofTopEnding

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/RoofTopEnding" RoofTopEnding(x::Integer, y::Integer, width::Integer=24)

const placements = Ahorn.PlacementDict(
    "resort_roof_ending" => Ahorn.EntityPlacement(RoofTopEnding)
)

function Ahorn.selection(entity::RoofTopEnding)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RoofTopEnding, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
