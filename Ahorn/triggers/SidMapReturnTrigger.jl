module SidMapReturnTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/SidMapReturnTrigger" SidMapReturnTrigger(x::Integer, y::Integer, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "Return to Lobby" => Ahorn.EntityPlacement(SidMapReturnTrigger)
)

function Ahorn.selection(entity::SidMapReturnTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SidMapReturnTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
