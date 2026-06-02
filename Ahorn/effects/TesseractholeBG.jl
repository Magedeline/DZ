module TesseractholeBG

using ..Ahorn, Maple

@mapdef Effect "MaggyHelper/TesseractholeBG" TesseractholeBG(x::Integer, y::Integer, alpha::Number=1.0, direction::Number=1.0, scale::Number=1.0, strength::String="Mild")

const placements = Ahorn.PlacementDict(
    "MaggyHelper/TesseractholeBG" => Ahorn.EntityPlacement(TesseractholeBG)
)

function Ahorn.selection(entity::TesseractholeBG)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TesseractholeBG, room::Maple.Room)
    Ahorn.drawSprite(ctx, "bgs/10/blackhole/particle", entity.x, entity.y)
end

end
