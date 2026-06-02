module FinalRemixBerryCutsceneTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/FinalRemixBerryCutsceneTrigger" FinalRemixBerryCutsceneTrigger(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "FinalRemixBerryCutsceneTrigger" => Ahorn.EntityPlacement(FinalRemixBerryCutsceneTrigger)
)

function Ahorn.selection(entity::FinalRemixBerryCutsceneTrigger)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FinalRemixBerryCutsceneTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
