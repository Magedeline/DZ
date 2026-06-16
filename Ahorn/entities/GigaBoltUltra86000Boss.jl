module GigaBoltUltra86000Boss

using ..Ahorn, Maple

@mapdef Entity "DZ/GigaBoltUltra86000Boss" GigaBoltUltra86000Boss(x::Integer, y::Integer, health::Integer=1100, maxHealth::Integer=1100)

const placements = Ahorn.PlacementDict(
    "giga_bolt_ultra_86000_boss" => Ahorn.EntityPlacement(GigaBoltUltra86000Boss)
)

function Ahorn.selection(entity::GigaBoltUltra86000Boss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GigaBoltUltra86000Boss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
