module ElementalPillar

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/ElementalPillar" ElementalPillar(x::Integer, y::Integer, element::String="Fire")

const placements = Ahorn.PlacementDict(
    "fire" => Ahorn.EntityPlacement(ElementalPillar),
    "ice" => Ahorn.EntityPlacement(ElementalPillar),
    "electric" => Ahorn.EntityPlacement(ElementalPillar),
    "wind" => Ahorn.EntityPlacement(ElementalPillar)
)

function Ahorn.selection(entity::ElementalPillar)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ElementalPillar, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
