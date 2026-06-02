module NPC_Theo

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/NPC_Theo" NPC_Theo(x::Integer, y::Integer, dialogKey::String="CH0_THEO_A", enabledByDefault::Bool=true, flagName::String="theo_prologue_met")

const placements = Ahorn.PlacementDict(
    "NPC_Theo_Prologue" => Ahorn.EntityPlacement(NPC_Theo),
    "NPC_Theo_About_Magolor" => Ahorn.EntityPlacement(NPC_Theo),
    "NPC_Theo_With_Magolor" => Ahorn.EntityPlacement(NPC_Theo)
)

function Ahorn.selection(entity::NPC_Theo)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC_Theo, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/theo/idle00", entity.x, entity.y)
end

end
