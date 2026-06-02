module FlingAsrielGod

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/FlingAsrielGod" FlingAsrielGod(x::Integer, y::Integer, waiting::Bool=false)

const placements = Ahorn.PlacementDict(
    "fling_asriel_god" => Ahorn.EntityPlacement(FlingAsrielGod)
)

function Ahorn.selection(entity::FlingAsrielGod)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FlingAsrielGod, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/asrielgodboss/idle00", entity.x, entity.y)
end

end
