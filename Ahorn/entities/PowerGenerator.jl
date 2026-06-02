module PowerGenerator

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/PowerGenerator" PowerGenerator(x::Integer, y::Integer, canTeleport::Bool=false, flag::Bool=false, flipX::Bool=false, health::Integer=5, music::String="", music_progress::Integer=-1, music_session::Bool=false)

const placements = Ahorn.PlacementDict(
    "Power_Generator" => Ahorn.EntityPlacement(PowerGenerator)
)

function Ahorn.selection(entity::PowerGenerator)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PowerGenerator, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Maggy/DesoloZantas/powergen/Idle00", entity.x, entity.y)
end

# Nodes: min=0, max=1
# Basic node rendering not implemented in auto-generated plugin

end
