module EyeBomb

using ..Ahorn, Maple

@mapdef Entity "DZ/EyeBomb" EyeBomb(x::Integer, y::Integer, detectionRadius::Number=64.0, explosionRadius::Number=48.0, fuseTime::Number=1.5)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(EyeBomb),
    "large_detection" => Ahorn.EntityPlacement(EyeBomb)
)

function Ahorn.selection(entity::EyeBomb)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::EyeBomb, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/eyebomb/eye", entity.x, entity.y)
end

end
