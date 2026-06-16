module EchoOrb

using ..Ahorn, Maple

@mapdef Entity "DZ/EchoOrb" EchoOrb(x::Integer, y::Integer, revealRadius::Number=80.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(EchoOrb)
)

function Ahorn.selection(entity::EchoOrb)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::EchoOrb, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
