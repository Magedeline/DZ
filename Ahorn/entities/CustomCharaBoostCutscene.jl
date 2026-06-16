module CustomCharaBoostCutscene

using ..Ahorn, Maple

@mapdef Entity "DZ/CustomCharaBoostCutscene" CustomCharaBoostCutscene(x::Integer, y::Integer, canSkip::Bool=true, cutsceneDash::Bool=true, oneUse::Bool=false)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(CustomCharaBoostCutscene)
)

function Ahorn.selection(entity::CustomCharaBoostCutscene)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomCharaBoostCutscene, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/charaboost/idle00", entity.x, entity.y)
end

end
