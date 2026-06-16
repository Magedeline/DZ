module CS09_FakeSaveStageDTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/CS09_FakeSaveStageDTrigger" CS09_FakeSaveStageDTrigger(x::Integer, y::Integer, height::Integer=16, playerOnly::Bool=true, triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "CS09_FakeSaveStageDTrigger" => Ahorn.EntityPlacement(CS09_FakeSaveStageDTrigger)
)

function Ahorn.selection(entity::CS09_FakeSaveStageDTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CS09_FakeSaveStageDTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
