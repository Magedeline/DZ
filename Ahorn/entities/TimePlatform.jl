module TimePlatform

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/TimePlatform" TimePlatform(x::Integer, y::Integer, height::Integer=8, timeEra::String="past", width::Integer=32)

const placements = Ahorn.PlacementDict(
    "past" => Ahorn.EntityPlacement(TimePlatform),
    "future" => Ahorn.EntityPlacement(TimePlatform)
)

function Ahorn.selection(entity::TimePlatform)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TimePlatform, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.3, 0.5, 0.7, 0.4), (0.4, 0.6, 0.9, 0.7))
end

end
