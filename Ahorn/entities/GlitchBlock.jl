module GlitchBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/GlitchBlock" GlitchBlock(x::Integer, y::Integer, glitchInterval::Number=3.0, height::Integer=16, invisibleTime::Number=1.0, isPattern::Bool=false, stability::Number=0.7, visibleTime::Number=2.0, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(GlitchBlock)
)

function Ahorn.selection(entity::GlitchBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GlitchBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
