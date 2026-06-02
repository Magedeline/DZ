module MadelineBandageDummy

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/MadelineBandageDummy" MadelineBandageDummy(x::Integer, y::Integer, alpha::Number=1.0, animation::String="idle", facing::Integer=1, isVisible::Bool=true, playAnimationOnSpawn::Bool=true, scale::Number=1.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(MadelineBandageDummy)
)

function Ahorn.selection(entity::MadelineBandageDummy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MadelineBandageDummy, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/madeline_bandage/idle00", entity.x, entity.y)
end

end
