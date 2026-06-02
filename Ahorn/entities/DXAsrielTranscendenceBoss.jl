module DXAsrielTranscendenceBoss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/DXAsrielTranscendenceBoss" DXAsrielTranscendenceBoss(x::Integer, y::Integer, autoStart::Bool=false, dialogAwakening::String="DXSIDE_ASRIEL_AWAKENING", dialogRedemption::String="DXSIDE_ASRIEL_REDEMPTION", enableAstralStorm::Bool=true, enableCosmicJudgment::Bool=true, enableDivineFury::Bool=true, enableSoulConvergence::Bool=true, enableTranscendenceRift::Bool=true, maxHealth::Integer=3500, maxTranscendence::Number=100.0, musicAwakening::String="", musicFury::String="", musicJudgment::String="", musicTranscendence::String="", showHealthBar::Bool=true)

const placements = Ahorn.PlacementDict(
    "DXAsrielTranscendence" => Ahorn.EntityPlacement(DXAsrielTranscendenceBoss),
    "DXAsrielTranscendence_Ultimate" => Ahorn.EntityPlacement(DXAsrielTranscendenceBoss)
)

function Ahorn.selection(entity::DXAsrielTranscendenceBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DXAsrielTranscendenceBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=0, max=4
# Basic node rendering not implemented in auto-generated plugin

end
