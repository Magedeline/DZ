module KirbyNPC

using ..Ahorn, Maple

@mapdef Entity "DZ/KirbyNPC" KirbyNPC(x::Integer, y::Integer, behavior::Integer=0, canGiveItem::Bool=false, character::Integer=0, dialogId::String="", followDistance::Integer=48, giveItemId::String="", moveSpeed::Integer=40)

const placements = Ahorn.PlacementDict(
    "Bandana Waddle Dee" => Ahorn.EntityPlacement(KirbyNPC),
    "King Dedede (Friendly)" => Ahorn.EntityPlacement(KirbyNPC),
    "Meta Knight (Friendly)" => Ahorn.EntityPlacement(KirbyNPC),
    "Magolor" => Ahorn.EntityPlacement(KirbyNPC),
    "Shop Keeper" => Ahorn.EntityPlacement(KirbyNPC),
    "Companion (Follows Kglobal::Player)" => Ahorn.EntityPlacement(KirbyNPC),
    "Patrol NPC (With Nodes)" => Ahorn.EntityPlacement(KirbyNPC)
)

function Ahorn.selection(entity::KirbyNPC)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyNPC, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
