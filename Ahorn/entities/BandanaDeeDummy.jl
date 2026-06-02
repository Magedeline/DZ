module BandanaDeeDummy

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/BandanaDeeDummy" BandanaDeeDummy(x::Integer, y::Integer, alpha::Number=1.0, animation::String="idle", autoFollow::Bool=true, facing::Integer=1, followDelay::Number=0.42, isVisible::Bool=true, playAnimationOnSpawn::Bool=true, scale::Number=1.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(BandanaDeeDummy)
)

function Ahorn.selection(entity::BandanaDeeDummy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BandanaDeeDummy, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/kirby/Maggy/DesoloZantas/bandana_dee/idle00", entity.x, entity.y)
end

end
