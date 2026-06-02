module FinalTitanSummitBackgroundManager

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/FinalTitanSummitBackgroundManager" FinalTitanSummitBackgroundManager(x::Integer, y::Integer, ambience::String="", cloudStrengthMultiplier::Number=1.0, creatureStrengthMultiplier::Number=1.0, cutscene::String="", dark::Bool=false, debrisStrengthMultiplier::Number=1.0, giygasStrengthMultiplier::Number=1.0, index::Integer=0, intro_launch::Bool=false, thunderStrengthMultiplier::Number=1.0)

const placements = Ahorn.PlacementDict(
    "FinalTitanSummitBackgroundManager" => Ahorn.EntityPlacement(FinalTitanSummitBackgroundManager)
)

function Ahorn.selection(entity::FinalTitanSummitBackgroundManager)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FinalTitanSummitBackgroundManager, room::Maple.Room)
    Ahorn.drawSprite(ctx, "@Internal@/summit_background_manager", entity.x, entity.y)
end

end
