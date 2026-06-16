module SafeCrystalSpinner

using ..Ahorn, Maple

@mapdef Entity "DZ/SafeCrystalSpinner" SafeCrystalSpinner(x::Integer, y::Integer, attachToSolid::Bool=false, color::String="Blue")

const placements = Ahorn.PlacementDict(
    "blue" => Ahorn.EntityPlacement(SafeCrystalSpinner),
    "red" => Ahorn.EntityPlacement(SafeCrystalSpinner),
    "purple" => Ahorn.EntityPlacement(SafeCrystalSpinner),
    "rainbow" => Ahorn.EntityPlacement(SafeCrystalSpinner)
)

function Ahorn.selection(entity::SafeCrystalSpinner)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SafeCrystalSpinner, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
