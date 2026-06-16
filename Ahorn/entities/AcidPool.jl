module AcidPool

using ..Ahorn, Maple

@mapdef Entity "DZ/AcidPool" AcidPool(x::Integer, y::Integer, height::Integer=8, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(AcidPool),
    "rising" => Ahorn.EntityPlacement(AcidPool)
)

function Ahorn.selection(entity::AcidPool)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AcidPool, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
