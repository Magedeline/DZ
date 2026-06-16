module BlackholeSandwich

using ..Ahorn, Maple

@mapdef Entity "DZ/BlackholeSandwich" BlackholeSandwich(x::Integer, y::Integer, canSwitch::Bool=true, glitchy::Bool=true, height::Integer=128, mode::String="Hot", speed::Number=80.0, switchFlag::String="", width::Integer=64)

const placements = Ahorn.PlacementDict(
    "hot" => Ahorn.EntityPlacement(BlackholeSandwich),
    "cold" => Ahorn.EntityPlacement(BlackholeSandwich)
)

function Ahorn.selection(entity::BlackholeSandwich)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BlackholeSandwich, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/IngesteHelper/blackhole_sandwich_space", entity.x, entity.y)
end

end
