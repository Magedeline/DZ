module TesseractPortalTorch

using ..Ahorn, Maple

@mapdef Entity "DZ/TesseractPortalTorch" TesseractPortalTorch(x::Integer, y::Integer, activationFlag::String="", isLit::Bool=true, lightRadius::Number=80.0, particleEffect::Bool=true, portalActive::Bool=false, portalColor::String="8844ff", portalDestination::String="", requiresActivation::Bool=false)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(TesseractPortalTorch),
    "active_portal" => Ahorn.EntityPlacement(TesseractPortalTorch),
    "activation_required" => Ahorn.EntityPlacement(TesseractPortalTorch)
)

function Ahorn.selection(entity::TesseractPortalTorch)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TesseractPortalTorch, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/IngesteHelper/tesseract_portal_torch", entity.x, entity.y)
end

end
