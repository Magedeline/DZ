module CS09_FakeSaveStageCTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/CS09_FakeSaveStageCTrigger" CS09_FakeSaveStageCTrigger(x::Integer, y::Integer, height::Integer=16, playerOnly::Bool=true, triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "CS09_FakeSaveStageCTrigger" => Ahorn.EntityPlacement(CS09_FakeSaveStageCTrigger)
)

function Ahorn.selection(entity::CS09_FakeSaveStageCTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CS09_FakeSaveStageCTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
