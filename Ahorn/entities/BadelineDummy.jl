module BadelineDummy

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/BadelineDummy" BadelineDummy(x::Integer, y::Integer, alpha::Number=1.0, animation::String="idle", facing::Integer=1, isVisible::Bool=true, playAnimationOnSpawn::Bool=true, scale::Number=1.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(BadelineDummy),
    "campfire" => Ahorn.EntityPlacement(BadelineDummy),
    "starjump" => Ahorn.EntityPlacement(BadelineDummy)
)

function Ahorn.selection(entity::BadelineDummy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BadelineDummy, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/badeline/idle00", entity.x, entity.y)
end

end
