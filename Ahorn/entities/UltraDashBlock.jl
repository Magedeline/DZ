module UltraDashBlock

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/UltraDashBlock" UltraDashBlock(x::Integer, y::Integer, breakableBySeeker::Bool=true, environment::String="General", height::Integer=16, permanent::Bool=false, tiletype::String="m", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(UltraDashBlock),
    "permanent" => Ahorn.EntityPlacement(UltraDashBlock)
)

function Ahorn.selection(entity::UltraDashBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::UltraDashBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
