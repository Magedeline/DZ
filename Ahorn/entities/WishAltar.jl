module WishAltar

using ..Ahorn, Maple

@mapdef Entity "DZ/WishAltar" WishAltar(x::Integer, y::Integer, canInteract::Bool=true, dialoguePrefix::String="WISH_ALTAR", requiredHearts::Integer=0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(WishAltar)
)

function Ahorn.selection(entity::WishAltar)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WishAltar, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
