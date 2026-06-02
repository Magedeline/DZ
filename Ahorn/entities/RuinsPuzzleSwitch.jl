module RuinsPuzzleSwitch

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/RuinsPuzzleSwitch" RuinsPuzzleSwitch(x::Integer, y::Integer, gateId::String="", holdTime::Number=1.0, sequenceOrder::Integer=0, switchType::String="Simple", timerDuration::Number=3.0)

const placements = Ahorn.PlacementDict(
    "simple" => Ahorn.EntityPlacement(RuinsPuzzleSwitch),
    "hold" => Ahorn.EntityPlacement(RuinsPuzzleSwitch),
    "timed" => Ahorn.EntityPlacement(RuinsPuzzleSwitch)
)

function Ahorn.selection(entity::RuinsPuzzleSwitch)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RuinsPuzzleSwitch, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
