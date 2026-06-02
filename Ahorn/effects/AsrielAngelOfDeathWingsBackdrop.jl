module AsrielAngelOfDeathWingsBackdrop

using ..Ahorn, Maple

@mapdef Effect "MaggyHelper/AsrielAngelOfDeathWingsBackdrop" AsrielAngelOfDeathWingsBackdrop(x::Integer, y::Integer, bgAlpha::Number=1.0, exclude::String="", expansionSpeed::Number=18.0, intensity::Number=1.0, loop::Bool=true, only::String="*", speed::Number=1.0, wingAlpha::Number=1.0, wingScale::Number=1.0)

const placements = Ahorn.PlacementDict(
    "asriel_angel_wings_backdrop" => Ahorn.EntityPlacement(AsrielAngelOfDeathWingsBackdrop),
    "asriel_angel_wings_slow_reveal" => Ahorn.EntityPlacement(AsrielAngelOfDeathWingsBackdrop),
    "asriel_angel_wings_intense" => Ahorn.EntityPlacement(AsrielAngelOfDeathWingsBackdrop)
)

function Ahorn.selection(entity::AsrielAngelOfDeathWingsBackdrop)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AsrielAngelOfDeathWingsBackdrop, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
