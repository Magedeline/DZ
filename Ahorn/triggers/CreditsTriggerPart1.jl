module CreditsTriggerPart1

using ..Ahorn, Maple

@mapdef Trigger "DZ/CreditsTriggerPart1" CreditsTriggerPart1(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "CreditsTriggerPart1" => Ahorn.EntityPlacement(CreditsTriggerPart1)
)

function Ahorn.selection(entity::CreditsTriggerPart1)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CreditsTriggerPart1, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
