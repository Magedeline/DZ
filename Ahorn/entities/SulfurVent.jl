module SulfurVent

using ..Ahorn, Maple

@mapdef Entity "DZ/SulfurVent" SulfurVent(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(SulfurVent)
)

function Ahorn.selection(entity::SulfurVent)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SulfurVent, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
