module NPC_Ralsei

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/NPC_Ralsei" NPC_Ralsei(x::Integer, y::Integer, dialogKey::String="RALSEI_FREE", enabledByDefault::Bool=false, flagName::String="ralsei_freed")

const placements = Ahorn.PlacementDict(
    "NPC_Ralsei_Trapped" => Ahorn.EntityPlacement(NPC_Ralsei),
    "NPC_Ralsei_Legend_Teller" => Ahorn.EntityPlacement(NPC_Ralsei),
    "NPC_Ralsei_Helper" => Ahorn.EntityPlacement(NPC_Ralsei)
)

function Ahorn.selection(entity::NPC_Ralsei)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC_Ralsei, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/ralsei/idle00", entity.x, entity.y)
end

end
