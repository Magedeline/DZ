module KirbySpawnPoint

using ..Ahorn, Maple

@mapdef Entity "DZ/KirbySpawnPoint" KirbySpawnPoint(x::Integer, y::Integer, spawnAsKirby::Bool=true, startingAbility::String="None")

const placements = Ahorn.PlacementDict(
    "kirbyspawnpoint" => Ahorn.EntityPlacement(KirbySpawnPoint)
)

function Ahorn.selection(entity::KirbySpawnPoint)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbySpawnPoint, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
