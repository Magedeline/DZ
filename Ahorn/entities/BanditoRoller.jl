module BanditoRoller

using ..Ahorn, Maple

@mapdef Entity "DZ/BanditoRoller" BanditoRoller(x::Integer, y::Integer, bounceSpeed::Integer=200, detectionRange::Integer=200, health::Integer=2, rollSpeed::Integer=150)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(BanditoRoller)
)

function Ahorn.selection(entity::BanditoRoller)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BanditoRoller, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
