module InteractiveSign

using ..Ahorn, Maple

@mapdef Entity "DZ/InteractiveSign" InteractiveSign(x::Integer, y::Integer, dialogKey::String="SIGN_DEFAULT", isActive::Bool=true, showPrompt::Bool=true, signType::String="wooden")

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(InteractiveSign),
    "stone" => Ahorn.EntityPlacement(InteractiveSign),
    "magical" => Ahorn.EntityPlacement(InteractiveSign)
)

function Ahorn.selection(entity::InteractiveSign)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::InteractiveSign, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/memorial/memorial_text", entity.x, entity.y)
end

end
