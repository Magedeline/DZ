module HeartGem

using ..Ahorn, Maple

@mapdef Entity "DZ/HeartGem" HeartGem(x::Integer, y::Integer, endLevelOnCollect::Bool=false, fake::Bool=false, removeCameraTriggers::Bool=false)

const placements = Ahorn.PlacementDict(
    "heartgem" => Ahorn.EntityPlacement(HeartGem),
    "fake" => Ahorn.EntityPlacement(HeartGem),
    "with_camera_removal" => Ahorn.EntityPlacement(HeartGem),
    "end_level" => Ahorn.EntityPlacement(HeartGem)
)

function Ahorn.selection(entity::HeartGem)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HeartGem, room::Maple.Room)
    Ahorn.drawSprite(ctx, "collectables/maggy/heartgem/0/00", entity.x, entity.y)
end

end
