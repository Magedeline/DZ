module StrawberryExt

using ..Ahorn, Maple

@mapdef Entity "DZ/StrawberryExt" StrawberryExt(x::Integer, y::Integer, moon::Bool=false, popstar::Bool=false, winged::Bool=false)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(StrawberryExt),
    "winged" => Ahorn.EntityPlacement(StrawberryExt),
    "moon" => Ahorn.EntityPlacement(StrawberryExt),
    "popstar" => Ahorn.EntityPlacement(StrawberryExt)
)

function Ahorn.selection(entity::StrawberryExt)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::StrawberryExt, room::Maple.Room)
    Ahorn.drawSprite(ctx, "collectables/strawberry/normal00", entity.x, entity.y)
end

end
