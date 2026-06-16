module MagicalAura

using ..Ahorn, Maple

@mapdef Effect "DZ/MagicalAura" MagicalAura(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "MagicalAura" => Ahorn.EntityPlacement(MagicalAura)
)

function Ahorn.selection(entity::MagicalAura)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MagicalAura, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
