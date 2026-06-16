module PinkGameboyColorGradeTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/PinkGameboyColorGradeTrigger" PinkGameboyColorGradeTrigger(x::Integer, y::Integer, colorGradeName::String="pinkgameboy", flagToSet::String="pink_gameboy_activated", playSound::Bool=true, transitionDuration::Number=0.5, triggerOnce::Bool=true)

const placements = Ahorn.PlacementDict(
    "pink_gameboy_colorgrade" => Ahorn.EntityPlacement(PinkGameboyColorGradeTrigger)
)

function Ahorn.selection(entity::PinkGameboyColorGradeTrigger)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PinkGameboyColorGradeTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
