module AsrielDummy

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/AsrielDummy" AsrielDummy(x::Integer, y::Integer, alpha::Number=1.0, animation::String="idle", facing::Integer=1, isVisible::Bool=true, playAnimationOnSpawn::Bool=false, scale::Number=1.0)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(AsrielDummy),
    "sad" => Ahorn.EntityPlacement(AsrielDummy),
    "happy" => Ahorn.EntityPlacement(AsrielDummy)
)

function Ahorn.selection(entity::AsrielDummy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AsrielDummy, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/asriel/idle00", entity.x, entity.y)
end

end
