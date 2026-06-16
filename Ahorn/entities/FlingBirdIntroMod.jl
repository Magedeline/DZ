module FlingBirdIntroMod

using ..Ahorn, Maple

@mapdef Entity "DZ/FlingBirdIntroMod" FlingBirdIntroMod(x::Integer, y::Integer, crashes::Bool=false)

const placements = Ahorn.PlacementDict(
    "Intro_fling_bird" => Ahorn.EntityPlacement(FlingBirdIntroMod)
)

function Ahorn.selection(entity::FlingBirdIntroMod)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FlingBirdIntroMod, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/bird/Hover04", entity.x, entity.y)
end

end
