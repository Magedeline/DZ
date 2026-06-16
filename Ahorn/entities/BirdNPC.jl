module BirdNPC

using ..Ahorn, Maple

@mapdef Entity "DZ/BirdNPC" BirdNPC(x::Integer, y::Integer, PlayerLeft::Bool=false, autoFly::Bool=false, birdType::String="Default", disableFlapSfx::Bool=false, flyAwayUp::Bool=true, mode::String="Sleeping", onlyOnce::Bool=false, waitForLightningPostDelay::Number=0.0)

const placements = Ahorn.PlacementDict(
    "sleeping" => Ahorn.EntityPlacement(BirdNPC),
    "climbing_tutorial" => Ahorn.EntityPlacement(BirdNPC),
    "dashing_tutorial" => Ahorn.EntityPlacement(BirdNPC),
    "fly_away" => Ahorn.EntityPlacement(BirdNPC),
    "clover" => Ahorn.EntityPlacement(BirdNPC),
    "cody" => Ahorn.EntityPlacement(BirdNPC),
    "emily" => Ahorn.EntityPlacement(BirdNPC),
    "odin" => Ahorn.EntityPlacement(BirdNPC),
    "robin" => Ahorn.EntityPlacement(BirdNPC),
    "sabel" => Ahorn.EntityPlacement(BirdNPC)
)

function Ahorn.selection(entity::BirdNPC)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BirdNPC, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
