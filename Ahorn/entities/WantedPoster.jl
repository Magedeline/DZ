module WantedPoster

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/WantedPoster" WantedPoster(x::Integer, y::Integer, bountyName::String="OUTLAW", bountyReward::Integer=100, enemyCount::Integer=3, enemyType::String="MaggyHelper/BanditoRoller")

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(WantedPoster)
)

function Ahorn.selection(entity::WantedPoster)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WantedPoster, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
