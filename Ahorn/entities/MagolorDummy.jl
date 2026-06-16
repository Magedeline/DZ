module MagolorDummy

using ..Ahorn, Maple

@mapdef Entity "DZ/MagolorDummy" MagolorDummy(x::Integer, y::Integer, alpha::Number=1.0, animation::String="idle", autoFollow::Bool=true, facing::Integer=1, followDelay::Number=0.55, isVisible::Bool=true, playAnimationOnSpawn::Bool=true, scale::Number=1.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(MagolorDummy)
)

function Ahorn.selection(entity::MagolorDummy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MagolorDummy, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/kirby/Maggy/DesoloZantas/magolor/idle00", entity.x, entity.y)
end

end
