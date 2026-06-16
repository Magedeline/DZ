module TesseractSwitch

using ..Ahorn, Maple

@mapdef Entity "DZ/TesseractSwitch" TesseractSwitch(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "TesseractSwitch" => Ahorn.EntityPlacement(TesseractSwitch)
)

function Ahorn.selection(entity::TesseractSwitch)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TesseractSwitch, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
