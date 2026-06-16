module KirbyTutorialBirdTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/KirbyTutorialBirdTrigger" KirbyTutorialBirdTrigger(x::Integer, y::Integer, birdId::String="", conditionFunction::String="", height::Integer=8, tutorialIndex::Integer=0, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "tutorial_bird_trigger" => Ahorn.EntityPlacement(KirbyTutorialBirdTrigger),
    "show_when_aqua_hook_fixed" => Ahorn.EntityPlacement(KirbyTutorialBirdTrigger),
    "show_when_kirby_aqua_swinging" => Ahorn.EntityPlacement(KirbyTutorialBirdTrigger),
    "close_when_aqua_attracted" => Ahorn.EntityPlacement(KirbyTutorialBirdTrigger)
)

function Ahorn.selection(entity::KirbyTutorialBirdTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyTutorialBirdTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
