module CutsceneEventEntity

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/CutsceneEventEntity" CutsceneEventEntity(x::Integer, y::Integer, activationMode::String="interact", completionFlag::String="", eventId::String="", removeAfterTrigger::Bool=true, requireFlag::String="", showSprite::Bool=false, texturePath::String="objects/Ingeste/sampleEntity/idle00")

const placements = Ahorn.PlacementDict(
    "interact" => Ahorn.EntityPlacement(CutsceneEventEntity),
    "touch" => Ahorn.EntityPlacement(CutsceneEventEntity),
    "room_enter" => Ahorn.EntityPlacement(CutsceneEventEntity)
)

function Ahorn.selection(entity::CutsceneEventEntity)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CutsceneEventEntity, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Ingeste/sampleEntity/idle00", entity.x, entity.y)
end

end
