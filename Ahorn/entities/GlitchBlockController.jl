module GlitchBlockController

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/GlitchBlockController" GlitchBlockController(x::Integer, y::Integer, syncInterval::Number=2.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(GlitchBlockController)
)

function Ahorn.selection(entity::GlitchBlockController)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GlitchBlockController, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
