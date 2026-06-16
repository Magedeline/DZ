module VineTrap

using ..Ahorn, Maple

@mapdef Entity "DZ/VineTrap" VineTrap(x::Integer, y::Integer, holdTime::Number=1.5, trapRadius::Number=24.0)

const placements = Ahorn.PlacementDict(
    "VineTrap" => Ahorn.EntityPlacement(VineTrap),
    "VineTrap_fast" => Ahorn.EntityPlacement(VineTrap)
)

function Ahorn.selection(entity::VineTrap)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::VineTrap, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
