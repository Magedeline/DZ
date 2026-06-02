module EchoFlowerEntity

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/EchoFlowerEntity" EchoFlowerEntity(x::Integer, y::Integer, cooldownTime::Number=1.0, echoDelay::Number=0.5, echoSpeed::Integer=200, maxEchoes::Integer=3)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(EchoFlowerEntity)
)

function Ahorn.selection(entity::EchoFlowerEntity)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::EchoFlowerEntity, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
