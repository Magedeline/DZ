module CubeDreamBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/CubeDreamBlock" CubeDreamBlock(x::Integer, y::Integer, below::Bool=false, cubeColor::String="4B0082", dreamColor::String="FF69B4", fastMoving::Bool=false, height::Integer=16, oneUse::Bool=false, requiredCutsceneFlag::String="chara_mirror_cutscene_completed", requiresCutscene::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "DZ/CubeDreamBlock" => Ahorn.EntityPlacement(CubeDreamBlock)
)

function Ahorn.selection(entity::CubeDreamBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CubeDreamBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.3, 0.0, 0.5, 0.4), (1.0, 0.4, 0.9, 1.0))
end

end
