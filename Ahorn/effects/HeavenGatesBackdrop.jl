module HeavenGatesBackdrop

using ..Ahorn, Maple

@mapdef Effect "MaggyHelper/HeavenGatesBackdrop" HeavenGatesBackdrop(x::Integer, y::Integer, astralBirthScale::Number=1.0, gateHeight::Number=100.0, gateWidth::Number=40.0, glowIntensity::Number=1.0, intensity::Number=1.0, only::String="end-saved", speed::Number=1.0, voidRadius::Number=35.0)

const placements = Ahorn.PlacementDict(
    "heaven_gates_backdrop" => Ahorn.EntityPlacement(HeavenGatesBackdrop),
    "heaven_gates_large_void" => Ahorn.EntityPlacement(HeavenGatesBackdrop)
)

function Ahorn.selection(entity::HeavenGatesBackdrop)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HeavenGatesBackdrop, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
