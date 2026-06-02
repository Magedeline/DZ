module DialogTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/DialogTrigger" DialogTrigger(x::Integer, y::Integer, dialogKey::String="DIALOG_DEFAULT", height::Integer=16, npcName::String="", requireInteraction::Bool=false, triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "DialogTrigger" => Ahorn.EntityPlacement(DialogTrigger),
    "interaction" => Ahorn.EntityPlacement(DialogTrigger)
)

function Ahorn.selection(entity::DialogTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DialogTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
