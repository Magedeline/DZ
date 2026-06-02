module AscendManagerBeyond

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/AscendManagerBeyond" AscendManagerBeyond(x::Integer, y::Integer, ambience::String="", arrivial::Bool=false, cutscene::String="", dark::Bool=false, height::Integer=32, index::Integer=0, intro_launch::Bool=false, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "AscendManagerBeyond" => Ahorn.EntityPlacement(AscendManagerBeyond),
    "AscendManagerBeyond_dark" => Ahorn.EntityPlacement(AscendManagerBeyond),
    "AscendManagerBeyond_ch19_ending" => Ahorn.EntityPlacement(AscendManagerBeyond)
)

function Ahorn.selection(entity::AscendManagerBeyond)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AscendManagerBeyond, room::Maple.Room)
    Ahorn.drawSprite(ctx, "@Internal@/summit_background_manager", entity.x, entity.y)
end

end
