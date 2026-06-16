module StrawberryRemix

using ..Ahorn, Maple

@mapdef Entity "DZ/StrawberryRemix" StrawberryRemix(x::Integer, y::Integer, bobAmplitude::Number=2.0, bobSpeed::Number=4.0, collectDelay::Number=0.15, glowInterval::Number=0.08, golden::Bool=false, moon::Bool=false, pink::Bool=false, popstar::Bool=false, winged::Bool=false)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(StrawberryRemix),
    "winged" => Ahorn.EntityPlacement(StrawberryRemix),
    "golden" => Ahorn.EntityPlacement(StrawberryRemix),
    "pink" => Ahorn.EntityPlacement(StrawberryRemix),
    "moon" => Ahorn.EntityPlacement(StrawberryRemix),
    "popstar" => Ahorn.EntityPlacement(StrawberryRemix)
)

function Ahorn.selection(entity::StrawberryRemix)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::StrawberryRemix, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
