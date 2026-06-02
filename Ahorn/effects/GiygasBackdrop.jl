module GiygasBackdrop

using ..Ahorn, Maple

@mapdef Effect "MaggyHelper/GiygasBackdrop" GiygasBackdrop(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "GiygasBackdrop" => Ahorn.EntityPlacement(GiygasBackdrop)
)

function Ahorn.selection(entity::GiygasBackdrop)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GiygasBackdrop, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
