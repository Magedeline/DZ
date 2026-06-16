module InfernoMirrorVortex

using ..Ahorn, Maple

@mapdef Entity "DZ/InfernoMirrorVortex" InfernoMirrorVortex(x::Integer, y::Integer, colorFrom::String="ff3333", colorTo::String="330000", distortion::Number=0.25, flag::String="", height::Integer=64, width::Integer=64)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(InfernoMirrorVortex)
)

function Ahorn.selection(entity::InfernoMirrorVortex)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::InfernoMirrorVortex, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
