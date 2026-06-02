module DefeatSwitchGate

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/DefeatSwitchGate" DefeatSwitchGate(x::Integer, y::Integer, flag::String="", height::Integer=48, persistent::Bool=false, requiredBossDefeats::Integer=0, requiredEnemyDefeats::Integer=5, useGlobalCounts::Bool=false, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "enemy_gate" => Ahorn.EntityPlacement(DefeatSwitchGate),
    "boss_gate" => Ahorn.EntityPlacement(DefeatSwitchGate),
    "combined_gate" => Ahorn.EntityPlacement(DefeatSwitchGate)
)

function Ahorn.selection(entity::DefeatSwitchGate)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DefeatSwitchGate, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
