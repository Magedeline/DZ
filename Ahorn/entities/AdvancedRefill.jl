module AdvancedRefill

using ..Ahorn, Maple

@mapdef Entity "DZ/AdvancedRefill" AdvancedRefill(x::Integer, y::Integer, dashCount::Integer=1, oneUse::Bool=false)

const placements = Ahorn.PlacementDict(
    "one_dash" => Ahorn.EntityPlacement(AdvancedRefill),
    "two_dashes" => Ahorn.EntityPlacement(AdvancedRefill),
    "three_dashes" => Ahorn.EntityPlacement(AdvancedRefill),
    "four_dashes" => Ahorn.EntityPlacement(AdvancedRefill),
    "five_dashes" => Ahorn.EntityPlacement(AdvancedRefill),
    "ten_dashes" => Ahorn.EntityPlacement(AdvancedRefill)
)

function Ahorn.selection(entity::AdvancedRefill)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AdvancedRefill, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
