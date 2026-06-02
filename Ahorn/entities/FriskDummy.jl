module FriskDummy

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/FriskDummy" FriskDummy(x::Integer, y::Integer, alpha::Number=1.0, animation::String="idle", facing::Integer=1, isVisible::Bool=true, playAnimationOnSpawn::Bool=false, scale::Number=1.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(FriskDummy),
    "determined" => Ahorn.EntityPlacement(FriskDummy)
)

function Ahorn.selection(entity::FriskDummy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FriskDummy, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/frisk/idle00", entity.x, entity.y)
end

end
