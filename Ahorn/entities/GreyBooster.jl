module GreyBooster

using ..Ahorn, Maple

@mapdef Entity "DesoloZatnas/GreyBooster" GreyBooster(x::Integer, y::Integer, red::Bool=false)

const placements = Ahorn.PlacementDict(
    "DesoloZatnas/GreyBooster" => Ahorn.EntityPlacement(GreyBooster),
    "grey_booster_red" => Ahorn.EntityPlacement(GreyBooster)
)

function Ahorn.selection(entity::GreyBooster)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GreyBooster, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Maggy/DesoloZantas/greybooster/gasterbooster00", entity.x, entity.y)
end

end
