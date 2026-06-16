module DreamPowerRefillBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/DreamPowerRefillBlock" DreamPowerRefillBlock(x::Integer, y::Integer, grantPower::String="None", oneUse::Bool=false, refillDash::Bool=true)

const placements = Ahorn.PlacementDict(
    "DZ/DreamPowerRefillBlock" => Ahorn.EntityPlacement(DreamPowerRefillBlock),
    "Dream Power Refill (one-use)" => Ahorn.EntityPlacement(DreamPowerRefillBlock)
)

function Ahorn.selection(entity::DreamPowerRefillBlock)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DreamPowerRefillBlock, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Maggy/DesoloZantas/refill/idle00", entity.x, entity.y)
end

end
