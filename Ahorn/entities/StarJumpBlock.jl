module StarJumpBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/StarJumpBlock" StarJumpBlock(x::Integer, y::Integer, height::Integer=8, sinks::Bool=true, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "star_jump_block" => Ahorn.EntityPlacement(StarJumpBlock)
)

function Ahorn.selection(entity::StarJumpBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::StarJumpBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
