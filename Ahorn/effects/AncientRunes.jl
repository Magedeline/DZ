module AncientRunes

using ..Ahorn, Maple

@mapdef Effect "MaggyHelper/AncientRunes" AncientRunes(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "AncientRunes" => Ahorn.EntityPlacement(AncientRunes)
)

function Ahorn.selection(entity::AncientRunes)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AncientRunes, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
