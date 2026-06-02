module EmberWisp

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/EmberWisp" EmberWisp(x::Integer, y::Integer, burnDuration::Number=3.0, floatSpeed::Integer=40, health::Integer=1, igniteRadius::Integer=16)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(EmberWisp)
)

function Ahorn.selection(entity::EmberWisp)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::EmberWisp, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
