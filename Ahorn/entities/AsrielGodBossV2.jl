module AsrielGodBossV2

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/AsrielGodBossV2" AsrielGodBossV2(x::Integer, y::Integer, attackSequence::String="", cameraLockY::Bool=true, cameraPastY::Number=120.0, dialog::Bool=true, patternIndex::Integer=0, startHit::Bool=false)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(AsrielGodBossV2),
    "phase_1_basic" => Ahorn.EntityPlacement(AsrielGodBossV2),
    "phase_2_spread" => Ahorn.EntityPlacement(AsrielGodBossV2),
    "phase_3_blades" => Ahorn.EntityPlacement(AsrielGodBossV2),
    "phase_4_blackhole" => Ahorn.EntityPlacement(AsrielGodBossV2),
    "phase_5_hypergoner" => Ahorn.EntityPlacement(AsrielGodBossV2)
)

function Ahorn.selection(entity::AsrielGodBossV2)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AsrielGodBossV2, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/asrielgodboss/boss00", entity.x, entity.y)
end

end
