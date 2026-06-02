module PowerSourceNumber

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/PowerSourceNumber" PowerSourceNumber(x::Integer, y::Integer, flag::String="disable_lightning", glowSprite::String="", gotCollectable::Bool=false, index::Integer=1, numberSprite::String="")

const placements = Ahorn.PlacementDict(
    "PowerSourceNumber" => Ahorn.EntityPlacement(PowerSourceNumber)
)

function Ahorn.selection(entity::PowerSourceNumber)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PowerSourceNumber, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/kevins_pc/pc_idle", entity.x, entity.y)
end

end
