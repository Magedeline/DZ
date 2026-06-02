module GiygasGlitch

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/GiygasGlitch" GiygasGlitch(x::Integer, y::Integer, duration::String="Short", glitch::Bool=true, stay::Bool=false)

const placements = Ahorn.PlacementDict(
    "giygas_glitch_background" => Ahorn.EntityPlacement(GiygasGlitch)
)

function Ahorn.selection(entity::GiygasGlitch)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GiygasGlitch, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
