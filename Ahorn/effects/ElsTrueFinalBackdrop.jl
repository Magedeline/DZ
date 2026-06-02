module ElsTrueFinalBackdrop

using ..Ahorn, Maple

@mapdef Effect "MaggyHelper/ElsTrueFinalBackdrop" ElsTrueFinalBackdrop(x::Integer, y::Integer, alpha::Number=1.0, color::String="FFFFFF", corruptionSpeed::Number=0.8, exclude::String="", fadeIn::Bool=false, fadeOut::Bool=false, fadeX::String="", fadeY::String="", flag::String="", flipX::Bool=false, flipY::Bool=false, gridExpansionSpeed::Number=0.4, instantIn::Bool=false, instantOut::Bool=false, intensity::Number=1.0, loopX::Bool=true, loopY::Bool=true, notFlag::String="", only::String="*", rainbowEdgeIntensity::Number=1.0, rainbowSpeed::Number=1.5, scrollX::Number=1.0, scrollY::Number=1.0, speed::Number=1.0, speedX::Number=0.0, speedY::Number=0.0, tag::String="", voidRadius::Number=60.0)

const placements = Ahorn.PlacementDict(
    "els_true_final_backdrop" => Ahorn.EntityPlacement(ElsTrueFinalBackdrop),
    "els_true_final_backdrop_intense" => Ahorn.EntityPlacement(ElsTrueFinalBackdrop),
    "els_true_final_backdrop_void" => Ahorn.EntityPlacement(ElsTrueFinalBackdrop),
    "els_true_final_backdrop_small_void" => Ahorn.EntityPlacement(ElsTrueFinalBackdrop)
)

function Ahorn.selection(entity::ElsTrueFinalBackdrop)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ElsTrueFinalBackdrop, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
