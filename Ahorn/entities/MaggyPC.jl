module MaggyPC

using ..Ahorn, Maple

@mapdef Entity "DZ/MaggyPC" MaggyPC(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "MaggyPC" => Ahorn.EntityPlacement(MaggyPC)
)

function Ahorn.selection(entity::MaggyPC)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MaggyPC, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/maggypc/pc0", entity.x, entity.y)
end

end
