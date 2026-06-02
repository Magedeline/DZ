module GravityWell

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/GravityWell" GravityWell(x::Integer, y::Integer, pullStrength::Number=100.0, radius::Number=80.0)

const placements = Ahorn.PlacementDict(
    "gravitywell" => Ahorn.EntityPlacement(GravityWell),
    "gravitywellstrong" => Ahorn.EntityPlacement(GravityWell)
)

function Ahorn.selection(entity::GravityWell)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GravityWell, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
