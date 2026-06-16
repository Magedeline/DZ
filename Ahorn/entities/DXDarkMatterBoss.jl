module DXDarkMatterBoss

using ..Ahorn, Maple

@mapdef Entity "DZ/DXDarkMatterBoss" DXDarkMatterBoss(x::Integer, y::Integer, autoStart::Bool=true, bossMusic::String="", echoAsriel::Bool=true, echoDedede::Bool=true, echoFlowey::Bool=true, enableDarkConstruct::Bool=true, enableEventHorizon::Bool=true, enableSingularity::Bool=true, enableVoidHeart::Bool=true, maxHealth::Integer=2000, showHealthBar::Bool=true)

const placements = Ahorn.PlacementDict(
    "DXDarkMatter" => Ahorn.EntityPlacement(DXDarkMatterBoss),
    "DXDarkMatter_Void" => Ahorn.EntityPlacement(DXDarkMatterBoss)
)

function Ahorn.selection(entity::DXDarkMatterBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DXDarkMatterBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=0, max=inf
# Basic node rendering not implemented in auto-generated plugin

end
