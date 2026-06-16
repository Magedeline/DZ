module DreamOrb

using ..Ahorn, Maple

@mapdef Entity "DZ/DreamOrb" DreamOrb(x::Integer, y::Integer, oneUse::Bool=false)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(DreamOrb),
    "one_use" => Ahorn.EntityPlacement(DreamOrb)
)

function Ahorn.selection(entity::DreamOrb)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DreamOrb, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/dreamorb/00", entity.x, entity.y)
end

end
