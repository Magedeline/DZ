module MirrorPortal

using ..Ahorn, Maple

@mapdef Entity "DZ/MirrorPortal" MirrorPortal(x::Integer, y::Integer, linkedId::String="portal_b", portalId::String="portal_a", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "MirrorPortal" => Ahorn.EntityPlacement(MirrorPortal)
)

function Ahorn.selection(entity::MirrorPortal)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MirrorPortal, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
