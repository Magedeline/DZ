module Heartgemmod

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/Heartgemmod" Heartgemmod(x::Integer, y::Integer, fake::Bool=false, fakeHeartDialog::String="CH19_WRONG_HEART", keepGoingDialog::String="CH19_KEEP_GOING_KIRBY", removeCameraTriggers::Bool=false)

const placements = Ahorn.PlacementDict(
    "real_crystal_heart" => Ahorn.EntityPlacement(Heartgemmod)
)

function Ahorn.selection(entity::Heartgemmod)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Heartgemmod, room::Maple.Room)
    Ahorn.drawSprite(ctx, "collectables/heartGem/0/00", entity.x, entity.y)
end

end
