module CollectibleGem

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/CollectibleGem" CollectibleGem(x::Integer, y::Integer, gemType::String="blue", isCollected::Bool=false, sparkleEffect::Bool=true, value::Integer=1)

const placements = Ahorn.PlacementDict(
    "blue" => Ahorn.EntityPlacement(CollectibleGem),
    "red" => Ahorn.EntityPlacement(CollectibleGem),
    "purple" => Ahorn.EntityPlacement(CollectibleGem),
    "golden" => Ahorn.EntityPlacement(CollectibleGem)
)

function Ahorn.selection(entity::CollectibleGem)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CollectibleGem, room::Maple.Room)
    Ahorn.drawSprite(ctx, "collectables/strawberry/normal00", entity.x, entity.y)
end

end
