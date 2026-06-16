module DarkerDarkMatterBoss

using ..Ahorn, Maple

@mapdef Entity "DZ/DarkerDarkMatterBoss" DarkerDarkMatterBoss(x::Integer, y::Integer, eyeAttackAnimationPath::String="attack", eyeDefeatAnimationPath::String="defeat", eyeDormantAnimationPath::String="dormant", eyeEnragedAnimationPath::String="enraged", eyeFormSpriteRoot::String="characters/darkmatter_boss_runtime", eyeIdleAnimationPath::String="idle", eyeTransformAnimationPath::String="transform", health::Integer=1600, maxHealth::Integer=1600, previewTexture::String="characters/darkmatter_boss_runtime/idle00", swordsmanDefeatAnimationPath::String="defeat", swordsmanFormSpriteRoot::String="characters/darkerdark_swordsman_runtime", swordsmanIdleAnimationPath::String="idle", swordsmanRainbowAnimationPath::String="rainbow", swordsmanReadyAnimationPath::String="ready", swordsmanSlashAnimationPath::String="slash")

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(DarkerDarkMatterBoss)
)

function Ahorn.selection(entity::DarkerDarkMatterBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DarkerDarkMatterBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
