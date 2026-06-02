module MagicFountain

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/MagicFountain" MagicFountain(x::Integer, y::Integer, fountainType::String="healing", healAmount::Integer=1, isActive::Bool=true, particleCount::Integer=50, usesRemaining::Integer=-1)

const placements = Ahorn.PlacementDict(
    "MagicFountain" => Ahorn.EntityPlacement(MagicFountain),
    "power_fountain" => Ahorn.EntityPlacement(MagicFountain)
)

function Ahorn.selection(entity::MagicFountain)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MagicFountain, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/fountain/fountain_idle00", entity.x, entity.y)
end

end
