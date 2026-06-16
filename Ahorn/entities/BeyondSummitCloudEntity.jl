module BeyondSummitCloudEntity

using ..Ahorn, Maple

@mapdef Entity "DZ/BeyondSummitCloudEntity" BeyondSummitCloudEntity(x::Integer, y::Integer, color::String="b64a86", dark::Bool=false, highlightColor::String="d988b7", particleCount::Integer=16, speedMultiplier::Number=1.0)

const placements = Ahorn.PlacementDict(
    "BeyondSummitCloud (Light)" => Ahorn.EntityPlacement(BeyondSummitCloudEntity),
    "BeyondSummitCloud (Dark)" => Ahorn.EntityPlacement(BeyondSummitCloudEntity)
)

function Ahorn.selection(entity::BeyondSummitCloudEntity)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BeyondSummitCloudEntity, room::Maple.Room)
    Ahorn.drawSprite(ctx, "@Internal@/summit_background_manager", entity.x, entity.y)
end

end
