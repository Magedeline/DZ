module KirbyActorKglobalPlayer

using ..Ahorn, Maple

@mapdef Entity "DZ/KirbyActorKglobal::Player" KirbyActorKglobalPlayer(x::Integer, y::Integer, faceLeft::Bool=false, showSweat::Bool=false, startAnimation::String="idle", sweatAnimation::String="idle")

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(KirbyActorKglobalPlayer),
    "sweat_danger" => Ahorn.EntityPlacement(KirbyActorKglobalPlayer)
)

function Ahorn.selection(entity::KirbyActorKglobalPlayer)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyActorKglobalPlayer, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/kirby/idle00", entity.x, entity.y)
end

end
