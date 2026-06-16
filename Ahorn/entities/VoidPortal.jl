module VoidPortal

using ..Ahorn, Maple

@mapdef Entity "DZ/VoidPortal" VoidPortal(x::Integer, y::Integer, destinationId::String="", isFinalPortal::Bool=false)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(VoidPortal),
    "final" => Ahorn.EntityPlacement(VoidPortal)
)

function Ahorn.selection(entity::VoidPortal)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::VoidPortal, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
