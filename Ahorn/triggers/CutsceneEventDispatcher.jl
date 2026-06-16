module CutsceneEventDispatcher

using ..Ahorn, Maple

@mapdef Trigger "DZ/CutsceneEventDispatcher" CutsceneEventDispatcher(x::Integer, y::Integer, event::String="", height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(CutsceneEventDispatcher),
    "chapter_intro" => Ahorn.EntityPlacement(CutsceneEventDispatcher),
    "boss_intro" => Ahorn.EntityPlacement(CutsceneEventDispatcher),
    "boss_mid" => Ahorn.EntityPlacement(CutsceneEventDispatcher),
    "asriel_angel_boss_intro" => Ahorn.EntityPlacement(CutsceneEventDispatcher),
    "boss_end" => Ahorn.EntityPlacement(CutsceneEventDispatcher),
    "ending" => Ahorn.EntityPlacement(CutsceneEventDispatcher),
    "credits" => Ahorn.EntityPlacement(CutsceneEventDispatcher)
)

function Ahorn.selection(entity::CutsceneEventDispatcher)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CutsceneEventDispatcher, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
