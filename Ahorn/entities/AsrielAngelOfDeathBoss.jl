module AsrielAngelOfDeathBoss

using ..Ahorn, Maple

@mapdef Entity "DZ/AsrielAngelOfDeathBoss" AsrielAngelOfDeathBoss(x::Integer, y::Integer, barrierHeight::Integer=300, barrierWidth::Integer=400, dialogFlashback::String="CH20_ASRIEL_REMEMBER_A", dialogPhase1::String="CH20_ASRIEL_ZERO_RISE_KILL", dialogStruggle::String="CH20_ASRIEL_ZERO_STRUGGLE_START", dialogVoidAnswer::String="CH20_ASRIEL_ZERO_VOID_ANSWERS", enableBarrier::Bool=true, enableFinalBeam::Bool=true, enableFlashback::Bool=true, enableLostSouls::Bool=true, health::Integer=2500, maxHealth::Integer=2500, musicBurnInDespair::String="event:/pusheen/extra_content/music/lvl20/burn_in_despair", musicHisTheme01::String="event:/pusheen/extra_content/music/lvl20/his_theme01", musicHisTheme02::String="event:/pusheen/extra_content/music/lvl20/his_theme02", musicKirbyVsAsriel::String="event:/pusheen/extra_content/music/lvl20/kirby_vs_asriel_fight_02", riseSpeed::Number=3.0)

const placements = Ahorn.PlacementDict(
    "AsrielAngelOfDeathBoss" => Ahorn.EntityPlacement(AsrielAngelOfDeathBoss),
    "hard_mode" => Ahorn.EntityPlacement(AsrielAngelOfDeathBoss),
    "cutscene_only" => Ahorn.EntityPlacement(AsrielAngelOfDeathBoss)
)

function Ahorn.selection(entity::AsrielAngelOfDeathBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AsrielAngelOfDeathBoss, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/asrielangelofdeathboss/face/00", entity.x, entity.y)
end

# Nodes: min=0, max=4
# Basic node rendering not implemented in auto-generated plugin

end
