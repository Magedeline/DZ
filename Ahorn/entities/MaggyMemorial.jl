module MaggyMemorial

using ..Ahorn, Maple

@mapdef Entity "DZ/MaggyMemorial" MaggyMemorial(x::Integer, y::Integer, dialogKey::String="MAGGY_MEMORIAL_DEFAULT", dreamy::Bool=false, spritePath::String="scenery/memorial/memorial")

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(MaggyMemorial),
    "dreamy" => Ahorn.EntityPlacement(MaggyMemorial)
)

function Ahorn.selection(entity::MaggyMemorial)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MaggyMemorial, room::Maple.Room)
    Ahorn.drawSprite(ctx, "scenery/memorial/memorial", entity.x, entity.y)
end

end
