module NPC_Phone

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/NPC_Phone" NPC_Phone(x::Integer, y::Integer, dialogKey::String="KIRBY_PAYPHONE_AWAKE_END", phoneType::String="mom")

const placements = Ahorn.PlacementDict(
    "Phone - Mom Call" => Ahorn.EntityPlacement(NPC_Phone),
    "Phone - Ex Call" => Ahorn.EntityPlacement(NPC_Phone)
)

function Ahorn.selection(entity::NPC_Phone)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC_Phone, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/payphone/payphone00", entity.x, entity.y)
end

end
