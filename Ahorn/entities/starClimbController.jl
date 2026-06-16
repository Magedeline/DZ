module starClimbController

using ..Ahorn, Maple

@mapdef Entity "starClimbController" starClimbController(x::Integer, y::Integer, bgColor::String="293E4B", fgColor::String="A3FFFF")

const placements = Ahorn.PlacementDict(
    "controller" => Ahorn.EntityPlacement(starClimbController)
)

function Ahorn.selection(entity::starClimbController)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::starClimbController, room::Maple.Room)
    Ahorn.drawSprite(ctx, "@Internal@/northern_lights", entity.x, entity.y)
end

end
