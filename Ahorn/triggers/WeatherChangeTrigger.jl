module WeatherChangeTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/WeatherChangeTrigger" WeatherChangeTrigger(x::Integer, y::Integer, height::Integer=16, intensity::Number=0.5, weatherType::String="Rain", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "rain" => Ahorn.EntityPlacement(WeatherChangeTrigger),
    "snow" => Ahorn.EntityPlacement(WeatherChangeTrigger),
    "storm" => Ahorn.EntityPlacement(WeatherChangeTrigger)
)

function Ahorn.selection(entity::WeatherChangeTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WeatherChangeTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
