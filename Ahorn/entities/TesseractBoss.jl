module TesseractBoss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/TesseractBoss" TesseractBoss(x::Integer, y::Integer, health::Integer=1350, maxHealth::Integer=1350)

const placements = Ahorn.PlacementDict(
    "tesseract_boss" => Ahorn.EntityPlacement(TesseractBoss)
)

function Ahorn.selection(entity::TesseractBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TesseractBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
