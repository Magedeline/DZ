module ClutterSwitch

using ..Ahorn, Maple

@mapdef Entity "DZ/ClutterSwitch" ClutterSwitch(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "ClutterSwitch" => Ahorn.EntityPlacement(ClutterSwitch)
)

function Ahorn.selection(entity::ClutterSwitch)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ClutterSwitch, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.4, 0.4, 0.4, 0.8), (1.0, 1.0, 1.0, 1.0))
end

end
