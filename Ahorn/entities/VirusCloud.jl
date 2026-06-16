module VirusCloud

using ..Ahorn, Maple

@mapdef Entity "DZ/VirusCloud" VirusCloud(x::Integer, y::Integer, damageRate::Number=0.5, health::Integer=3, moveSpeed::Integer=40, spreadRadius::Integer=100)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(VirusCloud)
)

function Ahorn.selection(entity::VirusCloud)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::VirusCloud, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
