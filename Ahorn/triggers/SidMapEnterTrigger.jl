module SidMapEnterTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/SidMapEnterTrigger" SidMapEnterTrigger(x::Integer, y::Integer, height::Integer=80, lockedDialogKey::String="RUINS_SM_LOCKED", requiredFlag::String="", targetRoom::String="lvl_sm1_start", targetSid::String="Maggy/SmallMaps/10_Ruins_SM1", width::Integer=40)

const placements = Ahorn.PlacementDict(
    "SM1 - Fragment I" => Ahorn.EntityPlacement(SidMapEnterTrigger),
    "SM2 - Fragment II" => Ahorn.EntityPlacement(SidMapEnterTrigger),
    "SM3 - Fragment III" => Ahorn.EntityPlacement(SidMapEnterTrigger),
    "SM4 - Fragment IV" => Ahorn.EntityPlacement(SidMapEnterTrigger),
    "SM5 - Fragment V" => Ahorn.EntityPlacement(SidMapEnterTrigger),
    "SM6 - Fragment VI" => Ahorn.EntityPlacement(SidMapEnterTrigger),
    "EX - Hidden Relic" => Ahorn.EntityPlacement(SidMapEnterTrigger),
    "Boss - Ancient Warden" => Ahorn.EntityPlacement(SidMapEnterTrigger),
    "Generic (custom SID)" => Ahorn.EntityPlacement(SidMapEnterTrigger)
)

function Ahorn.selection(entity::SidMapEnterTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SidMapEnterTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end
