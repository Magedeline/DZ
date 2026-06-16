module KirbyKglobalPlayerCore

using ..Ahorn, Maple

@mapdef Entity "DZ/KirbyKglobal::PlayerCore" KirbyKglobalPlayerCore(x::Integer, y::Integer, introType::String="None", inventory::String="KirbyKglobal::Player", maxHealth::Integer=6, power::String="None", useSpawnPoints::Bool=true)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(KirbyKglobalPlayerCore)
)

function Ahorn.selection(entity::KirbyKglobalPlayerCore)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyKglobalPlayerCore, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/kirby/idle00", entity.x, entity.y)
end

end
