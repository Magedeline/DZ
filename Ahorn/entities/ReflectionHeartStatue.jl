module ReflectionHeartStatue

using ..Ahorn, Maple

@mapdef Entity "DZ/ReflectionHeartStatue" ReflectionHeartStatue(x::Integer, y::Integer, dashSound::String="event:/game/06_reflection/supersecret_dashflavour", flagPrefix::String="heartTorch_", gemSprite::String="objects/reflectionHeart/gem", heartAppearSound::String="event:/game/06_reflection/supersecret_heartappear", heartSprite::String="collectables/heartgem/white00", hintSprite::String="objects/reflectionHeart/hint", statueSprite::String="objects/reflectionHeart/statue", torchSoundPrefix::String="event:/game/06_reflection/supersecret_torch_", torchSprite::String="objects/reflectionHeart/torch")

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(ReflectionHeartStatue)
)

function Ahorn.selection(entity::ReflectionHeartStatue)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ReflectionHeartStatue, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=5, max=5
# Basic node rendering not implemented in auto-generated plugin

end
