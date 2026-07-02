using System;
using System.Collections.Generic;
using Celeste.Entities;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ;

/// <summary>
/// Separated chapter data and registration helper to reduce AreaMapData file size.
/// This factory method handles all chapter definitions so AreaMapData can focus on management.
/// </summary>
internal static class ChapterRegistry
{
    /// <summary>Register all chapters into the provided list</summary>
    public static void RegisterAllChapters(List<AreaMapData.ChapterDef> chapters)
    {
        // Prologue (Chapter 0)
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 0, SID = AreaModeExtender.Build0SID("00_Prologue"), Name = "Prologue",
            Icon = "areas/DZ/prolouge", Has1 = false, Has2 = false, Has2 = false, HasDXSide = false,
        });

        // Chapter 1: Forbidden Metropolis
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 1, SID = AreaModeExtender.Build0SID("01_City"), Name = "Forbidden Metropolis",
            Icon = "areas/DZ/city", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 2: Veil of Shadows
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 2, SID = AreaModeExtender.Build0SID("02_Nightmare"), Name = "Veil of Shadows",
            Icon = "areas/DZ/nightmare", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 3: Arrival
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 3, SID = AreaModeExtender.Build0SID("03_Stars"), Name = "Arrival",
            Icon = "areas/DZ/star", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 4: Chronicles of Destiny
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 4, SID = AreaModeExtender.Build0SID("04_Legend"), Name = "Chronicles of Destiny",
            Icon = "areas/DZ/legend", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 5: Fractured Memories
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 5, SID = AreaModeExtender.Build0SID("05_Fractured"), Name = "Fractured Memories",
            Icon = "areas/DZ/resort", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 6: Fortress of Solitude
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 6, SID = AreaModeExtender.Build0SID("06_Stronghold"), Name = "Fortress of Solitude",
            Icon = "areas/DZ/stronghold", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 7: Infernal Reflections
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 7, SID = AreaModeExtender.Build0SID("07_Inferno"), Name = "Infernal Reflections",
            Icon = "areas/DZ/hell", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 8: Revelation's Edge
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 8, SID = AreaModeExtender.Build0SID("08_Truth"), Name = "Revelation's Edge",
            Icon = "areas/DZ/truth", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 9: Apex of Reality (Summit)
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 9, SID = AreaModeExtender.Build0SID("09_Summit"), Name = "Apex of Reality",
            Icon = "areas/DZ/summit", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 10: Echoes of the Past
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 10, SID = AreaModeExtender.Build0SID("10_Ruins"), Name = "Echoes of the Past",
            Icon = "areas/DZ/ruins", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 11: Frozen Sanctuary
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 11, SID = AreaModeExtender.Build0SID("11_Snow"), Name = "Frozen Sanctuary",
            Icon = "areas/DZ/snow", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 12: Cascading Depths
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 12, SID = AreaModeExtender.Build0SID("12_Water"), Name = "Cascading Depths",
            Icon = "areas/DZ/water", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 13: Blazing Territories
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 13, SID = AreaModeExtender.Build0SID("13_Fire"), Name = "Blazing Territories",
            Icon = "areas/DZ/fire", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 14: Cyber Nexus
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 14, SID = AreaModeExtender.Build0SID("14_Digital"), Name = "Cyber Nexus",
            Icon = "areas/DZ/digital", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 15: Ethereal Citadel
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 15, SID = AreaModeExtender.Build0SID("15_Castle"), Name = "Ethereal Citadel",
            Icon = "areas/DZ/castle", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 16: Organ Garden of Despair
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 16, SID = AreaModeExtender.Build0SID("16_Corruption"), Name = "Organ Garden of Despair",
            Icon = "areas/DZ/corruption", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 17: Epilogue (A-Side only)
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 17, SID = AreaModeExtender.Build0SID("17_Epilogue"), Name = "Epilogue",
            Icon = "areas/DZ/postepilogue", Has1 = false, Has2 = false, Has2 = false, HasDXSide = false,
        });

        // Chapter 18: Core of Existence
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 18, SID = AreaModeExtender.Build0SID("18_Heart"), Name = "Core of Existence",
            Icon = "areas/DZ/heart", Has1 = true, Has2 = true, Has2 = true, HasDXSide = false,
        });

        // Chapter 19: Farewell to Stars (A-Side only)
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 19, SID = AreaModeExtender.Build0SID("19_Space"), Name = "Farewell to Stars",
            Icon = "areas/DZ/farewell", Has1 = false, Has2 = false, Has2 = false, HasDXSide = false,
        });

        // Chapter 20: The Last Push (A-Side only)
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 20, SID = AreaModeExtender.Build0SID("20_TheEnd"), Name = "The Last Push",
            Icon = "areas/DZ/theend", Has1 = false, Has2 = false, Has2 = false, HasDXSide = false,
        });

        // Chapter 21: True Finale (A-Side only)
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 21, SID = AreaModeExtender.Build0SID("21_LastLevel"), Name = "True Finale",
            Icon = "areas/DZ/lastlevel", Has1 = false, Has2 = false, Has2 = false, HasDXSide = false,
        });

        Logger.Log(LogLevel.Info, "DZ",
            $"Registered {chapters.Count} chapters in AreaMapData");
    }

    private static void Register(List<AreaMapData.ChapterDef> chapters, AreaMapData.ChapterDef chapter)
    {
        chapters.Add(chapter);
    }

    private static Func<Session, Scene> CreateFinalVignette(Func<Session, Scene> finalevignette)
    {
        return finalevignette;
    }

    private static Func<Session, Scene> CreatePostcardVignette(Func<Session, Scene> postcards)
    {
        return postcards;
    }
}
