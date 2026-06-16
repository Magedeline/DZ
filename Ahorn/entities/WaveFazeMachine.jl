module WaveFazeMachine

using ..Ahorn, Maple

@mapdef Entity "DZ/WaveFazeMachine" WaveFazeMachine(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "WaveFazeMachine" => Ahorn.EntityPlacement(WaveFazeMachine)
)

function Ahorn.selection(entity::WaveFazeMachine)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WaveFazeMachine, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
