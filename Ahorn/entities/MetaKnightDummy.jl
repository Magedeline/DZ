module MetaKnightDummy

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/MetaKnightDummy" MetaKnightDummy(x::Integer, y::Integer, alpha::Number=1.0, animation::String="idle", autoFollow::Bool=true, facing::Integer=1, followDelay::Number=0.72, isVisible::Bool=true, playAnimationOnSpawn::Bool=false, scale::Number=1.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(MetaKnightDummy)
)

function Ahorn.selection(entity::MetaKnightDummy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MetaKnightDummy, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/kirby/Maggy/DesoloZantas/meta_knight/idle00", entity.x, entity.y)
end

end
