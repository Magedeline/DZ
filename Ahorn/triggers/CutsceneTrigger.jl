module CutsceneTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/CutsceneTrigger" CutsceneTrigger(x::Integer, y::Integer, autoStart::Bool=false, cutsceneId::String="ingeste_intro", height::Integer=16, playerOnly::Bool=true, triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "CutsceneTrigger" => Ahorn.EntityPlacement(CutsceneTrigger),
    "auto_start" => Ahorn.EntityPlacement(CutsceneTrigger)
)

function Ahorn.selection(entity::CutsceneTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CutsceneTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
