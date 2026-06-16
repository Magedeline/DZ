module WaveFazeSlammerTutorialMachine

using ..Ahorn, Maple

@mapdef Entity "DZ/WaveFazeSlammerTutorialMachine" WaveFazeSlammerTutorialMachine(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "WaveFazeSlammerTutorialMachine" => Ahorn.EntityPlacement(WaveFazeSlammerTutorialMachine)
)

function Ahorn.selection(entity::WaveFazeSlammerTutorialMachine)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WaveFazeSlammerTutorialMachine, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
