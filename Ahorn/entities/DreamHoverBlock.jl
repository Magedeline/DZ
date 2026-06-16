module DreamHoverBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/DreamHoverBlock" DreamHoverBlock(x::Integer, y::Integer, height::Integer=16, requireKirbyMode::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "DZ/DreamHoverBlock" => Ahorn.EntityPlacement(DreamHoverBlock)
)

function Ahorn.selection(entity::DreamHoverBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DreamHoverBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.85, 0.08, 0.85, 0.35), (1.0, 0.08, 1.0, 1.0))
end

end
