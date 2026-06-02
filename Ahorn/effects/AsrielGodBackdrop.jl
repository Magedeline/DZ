module AsrielGodBackdrop

using ..Ahorn, Maple

@mapdef Effect "MaggyHelper/AsrielGodBackdrop" AsrielGodBackdrop(x::Integer, y::Integer, alpha::Number=1.0, color::String="FFFFFF", exclude::String="", fadeIn::Bool=false, fadeOut::Bool=false, fadeX::String="", fadeY::String="", flag::String="", flipX::Bool=false, flipY::Bool=false, gridExpansionSpeed::Number=0.3, instantIn::Bool=false, instantOut::Bool=false, intensity::Number=1.0, loopX::Bool=true, loopY::Bool=true, notFlag::String="", only::String="*", rainbowSpeed::Number=2.0, scrollX::Number=1.0, scrollY::Number=1.0, speed::Number=1.0, speedX::Number=0.0, speedY::Number=0.0, starIntensity::Number=1.0, tag::String="")

const placements = Ahorn.PlacementDict(
    "asriel_god_backdrop" => Ahorn.EntityPlacement(AsrielGodBackdrop),
    "asriel_god_backdrop_intense" => Ahorn.EntityPlacement(AsrielGodBackdrop),
    "asriel_god_backdrop_cosmic" => Ahorn.EntityPlacement(AsrielGodBackdrop)
)

function Ahorn.selection(entity::AsrielGodBackdrop)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AsrielGodBackdrop, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
