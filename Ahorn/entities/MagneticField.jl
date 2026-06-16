module MagneticField

using ..Ahorn, Maple

@mapdef Entity "DZ/MagneticField" MagneticField(x::Integer, y::Integer, height::Integer=64, polarity::String="Attract", width::Integer=64)

const placements = Ahorn.PlacementDict(
    "attract" => Ahorn.EntityPlacement(MagneticField),
    "repel" => Ahorn.EntityPlacement(MagneticField)
)

function Ahorn.selection(entity::MagneticField)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MagneticField, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
