module InfernoEndingMusicHandler

using ..Ahorn, Maple

@mapdef Entity "DZ/InfernoEndingMusicHandler" InfernoEndingMusicHandler(x::Integer, y::Integer, endLevel::String="e-09", fadeInLayer::Integer=5, fadeOutLayer::Integer=1, music::String="event:/music/lvl5/mirror", roomPattern::String="e-*", startLevel::String="e-01")

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(InfernoEndingMusicHandler)
)

function Ahorn.selection(entity::InfernoEndingMusicHandler)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::InfernoEndingMusicHandler, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
