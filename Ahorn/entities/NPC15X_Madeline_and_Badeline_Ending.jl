module NPC15X_Madeline_and_Badeline_Ending

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC15X_Madeline_and_Badeline_Ending" NPC15X_Madeline_and_Badeline_Ending(x::Integer, y::Integer, ch15EasterEgg::Bool=false)

const placements = Ahorn.PlacementDict(
    "NPC15X_Madeline_and_Badeline_Ending" => Ahorn.EntityPlacement(NPC15X_Madeline_and_Badeline_Ending)
)

function Ahorn.selection(entity::NPC15X_Madeline_and_Badeline_Ending)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC15X_Madeline_and_Badeline_Ending, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/madeline/idle00", entity.x, entity.y)
end

end
