module CS09_FakeSaveStageBTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/CS09_FakeSaveStageBTrigger" CS09_FakeSaveStageBTrigger(x::Integer, y::Integer, height::Integer=16, playerOnly::Bool=true, triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "CS09_FakeSaveStageBTrigger" => Ahorn.EntityPlacement(CS09_FakeSaveStageBTrigger)
)

function Ahorn.selection(entity::CS09_FakeSaveStageBTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CS09_FakeSaveStageBTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
