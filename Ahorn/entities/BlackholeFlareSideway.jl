module BlackholeFlareSideway

using ..Ahorn, Maple

@mapdef Entity "DZ/BlackholeFlareSideway" BlackholeFlareSideway(x::Integer, y::Integer, direction::String="Right", glitchy::Bool=true, height::Integer=32, speed::Number=100.0, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "right" => Ahorn.EntityPlacement(BlackholeFlareSideway),
    "left" => Ahorn.EntityPlacement(BlackholeFlareSideway)
)

function Ahorn.selection(entity::BlackholeFlareSideway)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BlackholeFlareSideway, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/IngesteHelper/blackhole_flare", entity.x, entity.y)
end

end
