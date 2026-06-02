module KirbyKglobalPlayerSpawner

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/KirbyKglobal::PlayerSpawner" KirbyKglobalPlayerSpawner(x::Integer, y::Integer, enableKirbyMode::Bool=true, spawnCompanion::Bool=false, startingAbility::String="None")

const placements = Ahorn.PlacementDict(
    "kirby_Kglobal::Player_spawner" => Ahorn.EntityPlacement(KirbyKglobalPlayerSpawner),
    "kirby_Kglobal::Player_spawner_with_companion" => Ahorn.EntityPlacement(KirbyKglobalPlayerSpawner),
    "madeline_Kglobal::Player_spawner" => Ahorn.EntityPlacement(KirbyKglobalPlayerSpawner)
)

function Ahorn.selection(entity::KirbyKglobalPlayerSpawner)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyKglobalPlayerSpawner, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/kirby/idle00", entity.x, entity.y)
end

end
