module TitanThrone

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/TitanThrone" TitanThrone(x::Integer, y::Integer, activationRadius::Integer=100, autoActivate::Bool=false, bossEntity::String="MaggyHelper/KingTitanBoss", cutsceneId::String="CH15_ROARING_TITAN_KING_BATTLE")

const placements = Ahorn.PlacementDict(
    "manual" => Ahorn.EntityPlacement(TitanThrone),
    "auto" => Ahorn.EntityPlacement(TitanThrone)
)

function Ahorn.selection(entity::TitanThrone)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TitanThrone, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
