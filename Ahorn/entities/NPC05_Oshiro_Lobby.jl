module NPC05_Oshiro_Lobby

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC05_Oshiro_Lobby" NPC05_Oshiro_Lobby(x::Integer, y::Integer, dialogKey::String="ingeste_oshiro_05_lobby", flagName::String="oshiro_05_lobby", spriteId::String="oshiro")

const placements = Ahorn.PlacementDict(
    "NPC05_Oshiro_Lobby" => Ahorn.EntityPlacement(NPC05_Oshiro_Lobby)
)

function Ahorn.selection(entity::NPC05_Oshiro_Lobby)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC05_Oshiro_Lobby, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/oshiro/oshiro25", entity.x, entity.y)
end

end
