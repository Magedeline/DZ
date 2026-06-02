module Payphone

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/Payphone" Payphone(x::Integer, y::Integer, dialogId::String="CH2_DREAM_PHONECALL_TRAP", flagToSet::String="", onlyOnce::Bool=true)

const placements = Ahorn.PlacementDict(
    "payphone" => Ahorn.EntityPlacement(Payphone)
)

function Ahorn.selection(entity::Payphone)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Payphone, room::Maple.Room)
    Ahorn.drawSprite(ctx, "scenery/payphone", entity.x, entity.y)
end

end
