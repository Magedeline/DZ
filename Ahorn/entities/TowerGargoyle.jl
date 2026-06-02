module TowerGargoyle

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/TowerGargoyle" TowerGargoyle(x::Integer, y::Integer, detectionRange::Integer=150, glideSpeed::Integer=100, health::Integer=2, swoopSpeed::Integer=250)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(TowerGargoyle)
)

function Ahorn.selection(entity::TowerGargoyle)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TowerGargoyle, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
