module NPC00_Theo

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC00_Theo" NPC00_Theo(x::Integer, y::Integer, dialogKey::String="ingeste_theo_00_house", flagName::String="theo_00_house", spriteId::String="theo")

const placements = Ahorn.PlacementDict(
    "NPC00_Theo" => Ahorn.EntityPlacement(NPC00_Theo)
)

function Ahorn.selection(entity::NPC00_Theo)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC00_Theo, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/theo/theo00", entity.x, entity.y)
end

end
