using System;
using DZ.Nez;

namespace DZ.Entities.Level;

/// <summary>
/// Marker component that holds a render-dust callback.
/// Ported from Celeste's DustEdge.cs.
///
/// Entities that have a <see cref="DustGraphic"/> component also get a
/// <see cref="DustEdge"/> added automatically.  The dust-edge renderer
/// iterates all <see cref="DustEdge"/> components in the scene and calls
/// <see cref="RenderDust"/> for each, compositing dust visuals at the correct
/// render layer.
///
/// This is a pure data/callback component – no <c>Update</c> logic.
/// </summary>
public class DustEdge : DZ.Nez.Component
{
    /// <summary>
    /// Callback invoked by the dust-edge renderer each frame.
    /// Should contain the rendering logic for this entity's dust graphic.
    /// </summary>
    public Action RenderDust { get; set; }

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public DustEdge() { }

    /// <param name="onRenderDust">Render callback (typically <c>DustGraphic.Render</c>).</param>
    public DustEdge(Action onRenderDust)
    {
        RenderDust = onRenderDust;
    }
}
