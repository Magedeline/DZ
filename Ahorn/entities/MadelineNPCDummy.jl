module MadelineNPCDummy

using ..Ahorn, Maple

@mapdef Entity "DZ/MadelineNPCDummy" MadelineNPCDummy(x::Integer, y::Integer, alpha::Number=1.0, animation::String="idle", facing::Integer=1, isVisible::Bool=true, playAnimationOnSpawn::Bool=true, scale::Number=1.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(MadelineNPCDummy)
)

function Ahorn.selection(entity::MadelineNPCDummy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MadelineNPCDummy, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/madeline/maddy00", entity.x, entity.y)
end

end
