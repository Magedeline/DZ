local kirbyTutorialBird = {}

kirbyTutorialBird.name = "MaggyHelper/KirbyTutorialBird"
kirbyTutorialBird.depth = -1000000
kirbyTutorialBird.nodeLineRenderType = "line"
kirbyTutorialBird.justification = {0.5, 1.0}
kirbyTutorialBird.texture = "characters/bird/crow00"
kirbyTutorialBird.nodeLimits = {0, -1}
kirbyTutorialBird.fieldInformation = {
    startupIndex = {
        fieldType = "integer"
    },
    triggerOnce = {
        fieldType = "boolean"
    },
    faceLeft = {
        fieldType = "boolean"
    },
    caw = {
        fieldType = "boolean"
    },
    onlyOnce = {
        fieldType = "boolean"
    }
}

kirbyTutorialBird.placements = {
    {
        name = "tutorial_bird",
        data = {
            birdId = "",
            dialogs = "",
            controls = "",
            startupIndex = 0,
            triggerOnce = true,
            faceLeft = true,
            caw = true,
            onlyOnce = false
        }
    },
    {
        name = "aqua_hook_intro",
        data = {
            birdId = "kirby_aqua_intro",
            dialogs = "tutorial_dash;tutorial_dreamjump",
            controls = "mod:Aqua/ThrowHook,PLUS,UpRight;HOLD,Grab,PLUS,tinyarrow,Jump",
            startupIndex = 0,
            triggerOnce = true,
            faceLeft = true,
            caw = true,
            onlyOnce = false
        }
    },
    {
        name = "aqua_hook_swing",
        data = {
            birdId = "kirby_aqua_swing",
            dialogs = "tutorial_climb;tutorial_dreamjump",
            controls = "HOLD,Grab,PLUS,Left;HOLD,Grab,PLUS,tinyarrow,Jump",
            startupIndex = -1,
            triggerOnce = true,
            faceLeft = false,
            caw = true,
            onlyOnce = false
        }
    },
    {
        name = "method_btn_dash_jump",
        data = {
            birdId = "kirby_method_btn",
            dialogs = "tutorial_dash;tutorial_dreamjump",
            controls = "btn:Dash,PLUS,btn:Jump;prompt:Grab,THEN,btn:Jump",
            startupIndex = 0,
            triggerOnce = true,
            faceLeft = true,
            caw = true,
            onlyOnce = false
        }
    },
    {
        name = "method_compound_tokens",
        data = {
            birdId = "kirby_compound",
            dialogs = "tutorial_dash;tutorial_climb",
            controls = "PressPLUSClimb;HoldGrabPLUSUpRight",
            startupIndex = 0,
            triggerOnce = true,
            faceLeft = true,
            caw = true,
            onlyOnce = false
        }
    },
    {
        name = "method_mod_binding",
        data = {
            birdId = "kirby_mod_binding",
            dialogs = "tutorial_dash;tutorial_dreamjump",
            controls = "mod:Aqua/ThrowHook,PLUS,UpRight;HOLD,Grab,PLUS,tinyarrow,Jump",
            startupIndex = -1,
            triggerOnce = true,
            faceLeft = true,
            caw = true,
            onlyOnce = false
        }
    },
    {
        name = "controls_method_reference",
        data = {
            birdId = "kirby_controls_reference",
            dialogs = "tutorial_dash;tutorial_climb;tutorial_dreamjump;tutorial_hold",
            controls = "btn:Dash,PLUS,btn:Jump;prompt:Grab,THEN,UpRight;PressPLUSClimb;method:Talk,PLUS,tinyarrow",
            startupIndex = 0,
            triggerOnce = false,
            faceLeft = true,
            caw = false,
            onlyOnce = false
        }
    }
}

function kirbyTutorialBird.scale(room, entity)
    return (entity.faceLeft and -1 or 1), 1
end

return kirbyTutorialBird
