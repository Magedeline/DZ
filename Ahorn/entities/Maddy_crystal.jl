module Maddy_crystal

using ..Ahorn, Maple

@mapdef Entity "DZ/Maddy_crystal" Maddy_crystal(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Maddy_crystal" => Ahorn.EntityPlacement(Maddy_crystal)
)

function Ahorn.selection(entity::Maddy_crystal)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Maddy_crystal, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
