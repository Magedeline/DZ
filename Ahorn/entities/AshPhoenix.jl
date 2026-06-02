module AshPhoenix

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/AshPhoenix" AshPhoenix(x::Integer, y::Integer, detectionRange::Integer=250, flySpeed::Integer=100, health::Integer=3)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(AshPhoenix)
)

function Ahorn.selection(entity::AshPhoenix)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AshPhoenix, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
