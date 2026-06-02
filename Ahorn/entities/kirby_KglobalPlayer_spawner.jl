module kirby_KglobalPlayer_spawner

using ..Ahorn, Maple

@mapdef Entity "kirby_Kglobal::Player_spawner" kirby_KglobalPlayer_spawner(x::Integer, y::Integer, enableKirbyMode::Bool=true, spawnCompanion::Bool=false, startingAbility::String="None")

const placements = Ahorn.PlacementDict(
    "kirby_Kglobal::Player_spawner" => Ahorn.EntityPlacement(kirby_KglobalPlayer_spawner),
    "kirby_Kglobal::Player_spawner_with_companion" => Ahorn.EntityPlacement(kirby_KglobalPlayer_spawner),
    "madeline_Kglobal::Player_spawner" => Ahorn.EntityPlacement(kirby_KglobalPlayer_spawner)
)

function Ahorn.selection(entity::kirby_KglobalPlayer_spawner)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::kirby_KglobalPlayer_spawner, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/kirby/idle00", entity.x, entity.y)
end

end
