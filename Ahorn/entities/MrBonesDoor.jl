module MrBonesDoor

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/MrBonesDoor" MrBonesDoor(x::Integer, y::Integer, flagToSet::String="", height::Integer=48, requiredKeys::Integer=0)

const placements = Ahorn.PlacementDict(
    "MrBonesDoor" => Ahorn.EntityPlacement(MrBonesDoor),
    "key_locked" => Ahorn.EntityPlacement(MrBonesDoor)
)

function Ahorn.selection(entity::MrBonesDoor)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MrBonesDoor, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
