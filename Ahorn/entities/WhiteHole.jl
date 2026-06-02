module WhiteHole

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/WhiteHole" WhiteHole(x::Integer, y::Integer, ForceModifier::Number=0.8, SpeedModifier::Number=1.02)

const placements = Ahorn.PlacementDict(
    "WhiteHole" => Ahorn.EntityPlacement(WhiteHole)
)

function Ahorn.selection(entity::WhiteHole)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WhiteHole, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/WhiteHole/WhiteHole00", entity.x, entity.y)
end

end
