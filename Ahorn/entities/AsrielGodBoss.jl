module AsrielGodBoss

using ..Ahorn, Maple

@mapdef Entity "DZ/AsrielGodBoss" AsrielGodBoss(x::Integer, y::Integer, attackSequence::String="", cameraLockY::Bool=true, cameraPastY::Number=120.0, dialog::Bool=true, patternIndex::Integer=0, startHit::Bool=false)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(AsrielGodBoss),
    "intro_cutscene" => Ahorn.EntityPlacement(AsrielGodBoss),
    "hard_mode" => Ahorn.EntityPlacement(AsrielGodBoss),
    "hypergoner_finale" => Ahorn.EntityPlacement(AsrielGodBoss)
)

function Ahorn.selection(entity::AsrielGodBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AsrielGodBoss, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/asrielgodboss/boss00", entity.x, entity.y)
end

end
