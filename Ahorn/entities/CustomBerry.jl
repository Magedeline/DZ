module CustomBerry

using ..Ahorn, Maple

@mapdef Entity "DZ/CustomBerry" CustomBerry(x::Integer, y::Integer, berryType::String="strawberry", checkpointID::Integer=-1, collectSound::String="Original", customCollectSound::String="", levelSet::String="", maps::String="", order::Integer=-1, requires::String="", winged::Bool=false)

const placements = Ahorn.PlacementDict(
    "strawberry" => Ahorn.EntityPlacement(CustomBerry),
    "strawberry_winged" => Ahorn.EntityPlacement(CustomBerry),
    "moonberry" => Ahorn.EntityPlacement(CustomBerry),
    "moonberry_winged" => Ahorn.EntityPlacement(CustomBerry),
    "voidstarberry" => Ahorn.EntityPlacement(CustomBerry),
    "popstarberry" => Ahorn.EntityPlacement(CustomBerry),
    "pinkplatinumberry" => Ahorn.EntityPlacement(CustomBerry)
)

function Ahorn.selection(entity::CustomBerry)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomBerry, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
