module DataStream

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/DataStream" DataStream(x::Integer, y::Integer, direction::String="Right", flowSpeed::Integer=150, streamLength::Integer=200, streamWidth::Integer=32)

const placements = Ahorn.PlacementDict(
    "right" => Ahorn.EntityPlacement(DataStream),
    "left" => Ahorn.EntityPlacement(DataStream),
    "up" => Ahorn.EntityPlacement(DataStream),
    "down" => Ahorn.EntityPlacement(DataStream)
)

function Ahorn.selection(entity::DataStream)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DataStream, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
