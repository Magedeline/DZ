module ElectricEnemy

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/ElectricEnemy" ElectricEnemy(x::Integer, y::Integer, chargeTime::Number=3.0, health::Integer=2, shockRadius::Number=60.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(ElectricEnemy),
    "fast_charge" => Ahorn.EntityPlacement(ElectricEnemy)
)

function Ahorn.selection(entity::ElectricEnemy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ElectricEnemy, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
