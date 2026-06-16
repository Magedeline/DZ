module TesseractMirror

using ..Ahorn, Maple

@mapdef Entity "DZ/TesseractMirror" TesseractMirror(x::Integer, y::Integer, height::Integer=16, reflectX::Number=0.0, reflectY::Number=0.0, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "tesseract_mirror" => Ahorn.EntityPlacement(TesseractMirror)
)

function Ahorn.selection(entity::TesseractMirror)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TesseractMirror, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
