module IntroPresentsTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/IntroPresentsTrigger" IntroPresentsTrigger(x::Integer, y::Integer, height::Integer=32, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "IntroPresentsTrigger" => Ahorn.EntityPlacement(IntroPresentsTrigger)
)

function Ahorn.selection(entity::IntroPresentsTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::IntroPresentsTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
