module NPC10_Badeline

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC10_Badeline" NPC10_Badeline(x::Integer, y::Integer, dialogKey::String="CH2_BADELINE_GRIEFA", flagName::String="badeline_met")

const placements = Ahorn.PlacementDict(
    "NPC10_Badeline" => Ahorn.EntityPlacement(NPC10_Badeline)
)

function Ahorn.selection(entity::NPC10_Badeline)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC10_Badeline, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/badeline/idle00", entity.x, entity.y)
end

end
