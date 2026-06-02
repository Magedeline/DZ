module TesseractMirrorPortal

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/TesseractMirrorPortal" TesseractMirrorPortal(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "TesseractMirrorPortal" => Ahorn.EntityPlacement(TesseractMirrorPortal)
)

function Ahorn.selection(entity::TesseractMirrorPortal)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TesseractMirrorPortal, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
