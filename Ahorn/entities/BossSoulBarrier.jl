module BossSoulBarrier

using ..Ahorn, Maple

@mapdef Entity "DZ/BossSoulBarrier" BossSoulBarrier(
    x::Integer,
    y::Integer,
    width::Integer=8,
    height::Integer=32,
    bossType::Integer=0,
    barrierId::String="",
    breakAfterCutscene::Bool=true
)

const placements = Ahorn.PlacementDict(
    "Boss Soul Barrier (Titan King)" => Ahorn.EntityPlacement(BossSoulBarrier, Dict{String, Any}("bossType" => 0, "barrierId" => "titan_king")),
    "Boss Soul Barrier (Guardian Titan)" => Ahorn.EntityPlacement(BossSoulBarrier, Dict{String, Any}("bossType" => 1, "barrierId" => "guardian_titan")),
    "Boss Soul Barrier (Chapter 16 Els)" => Ahorn.EntityPlacement(BossSoulBarrier, Dict{String, Any}("bossType" => 2, "barrierId" => "ch16_els")),
    "Boss Soul Barrier (Asriel Angel of Death)" => Ahorn.EntityPlacement(BossSoulBarrier, Dict{String, Any}("bossType" => 3, "barrierId" => "asriel_angel")),
    "Boss Soul Barrier (Els True Final)" => Ahorn.EntityPlacement(BossSoulBarrier, Dict{String, Any}("bossType" => 4, "barrierId" => "els_true_final")),
    "Boss Soul Barrier (Asriel Break Giygas)" => Ahorn.EntityPlacement(BossSoulBarrier, Dict{String, Any}("bossType" => 5, "barrierId" => "asriel_giygas"))
)

const colors = Dict{Integer, Tuple{Float64, Float64, Float64, Float64}}(
    0 => (1.0, 0.5, 0.0, 0.8),
    1 => (0.5, 0.5, 0.5, 0.8),
    2 => (0.5, 0.0, 0.0, 0.8),
    3 => (1.0, 0.8, 0.0, 0.8),
    4 => (0.5, 0.0, 0.5, 0.8),
    5 => (0.1, 0.1, 0.1, 0.8)
)

function Ahorn.selection(entity::BossSoulBarrier)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BossSoulBarrier, room::Maple.Room)
    local color = get(colors, get(entity, "bossType", 0), (0.5, 0.5, 0.5, 0.8))
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), color, (0.0, 0.0, 0.0, 1.0))
end

end
