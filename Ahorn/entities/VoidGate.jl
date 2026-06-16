module VoidGate

using ..Ahorn, Maple

@mapdef Entity "DZ/VoidGate" VoidGate(x::Integer, y::Integer, gateHeight::Integer=128, gateWidth::Integer=16, height::Integer=128, moveSpeed::Integer=100, triggerHeight::Integer=128, triggerWidth::Integer=96, width::Integer=160)

const placements = Ahorn.PlacementDict(
    "void_gate" => Ahorn.EntityPlacement(VoidGate)
)

function Ahorn.selection(entity::VoidGate)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::VoidGate, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end
