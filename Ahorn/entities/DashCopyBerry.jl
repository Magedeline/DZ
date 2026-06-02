module DashCopyBerry

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/DashCopyBerry" DashCopyBerry(x::Integer, y::Integer, dashRefill::Integer=1, power::String="Sword", refillOnlyWhenEmpty::Bool=false)

const placements = Ahorn.PlacementDict(
    "dash_copy_berry" => Ahorn.EntityPlacement(DashCopyBerry),
    "dash_copy_berry_fire" => Ahorn.EntityPlacement(DashCopyBerry),
    "dash_copy_berry_wing" => Ahorn.EntityPlacement(DashCopyBerry)
)

function Ahorn.selection(entity::DashCopyBerry)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DashCopyBerry, room::Maple.Room)
    Ahorn.drawSprite(ctx, "collectables/strawberry/normal00", entity.x, entity.y)
end

end
