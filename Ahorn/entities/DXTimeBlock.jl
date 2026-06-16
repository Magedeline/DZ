module DXTimeBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/DXTimeBlock" DXTimeBlock(x::Integer, y::Integer, color::String="9966CC", cycleTime::Number=3.0, groupId::Integer=0, height::Integer=16, phaseOffset::Number=0.0, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "DXTimeBlock_GroupA" => Ahorn.EntityPlacement(DXTimeBlock),
    "DXTimeBlock_GroupB" => Ahorn.EntityPlacement(DXTimeBlock)
)

function Ahorn.selection(entity::DXTimeBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DXTimeBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.3, 0.8, 0.5), (0.7, 0.4, 1.0, 0.8))
end

end
