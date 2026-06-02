module KirbyDreamBlock

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/KirbyDreamBlock" KirbyDreamBlock(x::Integer, y::Integer, below::Bool=false, fastMoving::Bool=false, height::Integer=16, oneUse::Bool=false, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "MaggyHelper/KirbyDreamBlock" => Ahorn.EntityPlacement(KirbyDreamBlock)
)

function Ahorn.selection(entity::KirbyDreamBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyDreamBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (1.0, 0.41, 0.71, 0.35), (1.0, 0.08, 0.58, 1.0))
end

end
