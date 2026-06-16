module BeyondSummitCloud

using ..Ahorn, Maple

@mapdef Entity "DZ/BeyondSummitCloud" BeyondSummitCloud(x::Integer, y::Integer, ambience::String="", cutscene::String="", dark::Bool=false, index::Integer=0, intro_launch::Bool=false)

const placements = Ahorn.PlacementDict(
    "BeyondSummitCloud" => Ahorn.EntityPlacement(BeyondSummitCloud)
)

function Ahorn.selection(entity::BeyondSummitCloud)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BeyondSummitCloud, room::Maple.Room)
    Ahorn.drawSprite(ctx, "@Internal@/summit_background_manager", entity.x, entity.y)
end

end
