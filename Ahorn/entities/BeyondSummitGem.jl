module BeyondSummitGem

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/BeyondSummitGem" BeyondSummitGem(x::Integer, y::Integer, sprite::String="")

const placements = Ahorn.PlacementDict(
    "BeyondSummitGem (" => Ahorn.EntityPlacement(BeyondSummitGem)
)

function Ahorn.selection(entity::BeyondSummitGem)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BeyondSummitGem, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
