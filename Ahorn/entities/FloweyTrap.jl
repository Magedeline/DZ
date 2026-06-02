module FloweyTrap

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/FloweyTrap" FloweyTrap(x::Integer, y::Integer, attackInterval::Number=1.5, attackPattern::String="Circular", detectionRange::Integer=120, health::Integer=2, pelletCount::Integer=5, pelletSpeed::Integer=150, retractRange::Integer=180)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(FloweyTrap)
)

function Ahorn.selection(entity::FloweyTrap)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FloweyTrap, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
