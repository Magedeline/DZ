module OneHitTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/OneHitTrigger" OneHitTrigger(x::Integer, y::Integer, height::Integer=32, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "OneHitTrigger" => Ahorn.EntityPlacement(OneHitTrigger)
)

function Ahorn.selection(entity::OneHitTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::OneHitTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
