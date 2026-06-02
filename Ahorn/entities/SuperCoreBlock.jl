module SuperCoreBlock

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/SuperCoreBlock" SuperCoreBlock(x::Integer, y::Integer, coldColor::String="00BFFF", hotColor::String="FF4500", launchRange::Number=400.0, requiresCoreMode::Bool=false, speedMultiplier::Number=3.0, superColor::String="FFD700")

const placements = Ahorn.PlacementDict(
    "MaggyHelper/SuperCoreBlock" => Ahorn.EntityPlacement(SuperCoreBlock)
)

function Ahorn.selection(entity::SuperCoreBlock)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SuperCoreBlock, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Ingeste/superCoreBlock/super_idle00", entity.x, entity.y)
end

end
