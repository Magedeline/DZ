module FlingBirdIntroCutscene

using ..Ahorn, Maple

@mapdef Entity "DZ/FlingBirdIntroCutscene" FlingBirdIntroCutscene(x::Integer, y::Integer, crashes::Bool=true)

const placements = Ahorn.PlacementDict(
    "FlingBirdIntroCutscene_Crash" => Ahorn.EntityPlacement(FlingBirdIntroCutscene),
    "FlingBirdIntroCutscene_Miss" => Ahorn.EntityPlacement(FlingBirdIntroCutscene)
)

function Ahorn.selection(entity::FlingBirdIntroCutscene)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FlingBirdIntroCutscene, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/bird/hover00", entity.x, entity.y)
end

end
