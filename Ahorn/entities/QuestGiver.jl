module QuestGiver

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/QuestGiver" QuestGiver(x::Integer, y::Integer, completionFlag::String="quest_1_done", dialogId::String="", questId::String="quest_1")

const placements = Ahorn.PlacementDict(
    "QuestGiver" => Ahorn.EntityPlacement(QuestGiver)
)

function Ahorn.selection(entity::QuestGiver)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::QuestGiver, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/oldlady/idle00", entity.x, entity.y)
end

end
