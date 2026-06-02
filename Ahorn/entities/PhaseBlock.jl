module PhaseBlock

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/PhaseBlock" PhaseBlock(x::Integer, y::Integer, height::Integer=16, phaseSpeed::Number=1.0, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "PhaseBlock" => Ahorn.EntityPlacement(PhaseBlock),
    "fast" => Ahorn.EntityPlacement(PhaseBlock),
    "offset" => Ahorn.EntityPlacement(PhaseBlock)
)

function Ahorn.selection(entity::PhaseBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PhaseBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.3, 0.7, 0.4), (0.7, 0.4, 1.0, 0.6))
end

end
