module MadelineGodDummy

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/MadelineGodDummy" MadelineGodDummy(x::Integer, y::Integer, alpha::Number=1.0, animation::String="boost", combatActive::Bool=true, facing::Integer=1, isVisible::Bool=true, playAnimationOnSpawn::Bool=true, scale::Number=1.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(MadelineGodDummy)
)

function Ahorn.selection(entity::MadelineGodDummy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MadelineGodDummy, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/madeline/maddy00", entity.x, entity.y)
end

end
