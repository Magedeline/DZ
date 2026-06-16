module UwUmper

using ..Ahorn, Maple

@mapdef Entity "DZ/UwUmper" UwUmper(x::Integer, y::Integer, fireMode::Bool=false)

const placements = Ahorn.PlacementDict(
    "UwUmper" => Ahorn.EntityPlacement(UwUmper),
    "fire_mode" => Ahorn.EntityPlacement(UwUmper)
)

function Ahorn.selection(entity::UwUmper)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::UwUmper, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/uwumper/Idle00", entity.x, entity.y)
end

end
