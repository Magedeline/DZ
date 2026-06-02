module IceBouncer

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/IceBouncer" IceBouncer(x::Integer, y::Integer, bounceStrength::Number=-180.0, dashesGranted::Integer=2, iceColor::String="87CEEB", requiresCoreMode::Bool=true)

const placements = Ahorn.PlacementDict(
    "MaggyHelper/IceBouncer" => Ahorn.EntityPlacement(IceBouncer)
)

function Ahorn.selection(entity::IceBouncer)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::IceBouncer, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Ingeste/iceBouncer/idle00", entity.x, entity.y)
end

end
