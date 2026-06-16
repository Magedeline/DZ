module KirbyAudioEntity

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/KirbyAudioEntity" KirbyAudioEntity(x::Integer, y::Integer, audioType::String="dialogue", autoPlay::Bool=false, cooldown::Integer=1000, triggerOnContact::Bool=true, volume::Number=1.0)

const placements = Ahorn.PlacementDict(
    "Kirby Voice Box" => Ahorn.EntityPlacement(KirbyAudioEntity)
)

function Ahorn.selection(entity::KirbyAudioEntity)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyAudioEntity, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
