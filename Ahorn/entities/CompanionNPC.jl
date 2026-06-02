module CompanionNPC

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/CompanionNPC" CompanionNPC(x::Integer, y::Integer, canFight::Bool=true, companionType::String="Bandana_Dee", followDistance::Number=24.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(CompanionNPC)
)

function Ahorn.selection(entity::CompanionNPC)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CompanionNPC, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/kirby/idle00", entity.x, entity.y)
end

end
