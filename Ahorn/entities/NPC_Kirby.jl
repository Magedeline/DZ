module NPC_Kirby

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/NPC_Kirby" NPC_Kirby(x::Integer, y::Integer, canFloat::Bool=true, dialogKey::String="EXAMPLE_ADVANCED_CUTSCENE", enabledByDefault::Bool=true, flagName::String="kirby_tutorial_complete")

const placements = Ahorn.PlacementDict(
    "NPC_Kirby_Tutorial" => Ahorn.EntityPlacement(NPC_Kirby),
    "NPC_Kirby_Dream_Lever" => Ahorn.EntityPlacement(NPC_Kirby),
    "NPC_Kirby_Payphone" => Ahorn.EntityPlacement(NPC_Kirby)
)

function Ahorn.selection(entity::NPC_Kirby)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC_Kirby, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/kirby/idle00", entity.x, entity.y)
end

end
