module TrainingDummy

using ..Ahorn, Maple

@mapdef Entity "DZ/TrainingDummy" TrainingDummy(x::Integer, y::Integer, maxHealth::Integer=10, showDamage::Bool=true)

const placements = Ahorn.PlacementDict(
    "TrainingDummy" => Ahorn.EntityPlacement(TrainingDummy)
)

function Ahorn.selection(entity::TrainingDummy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TrainingDummy, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/Kglobal::Player/idle00", entity.x, entity.y)
end

end
