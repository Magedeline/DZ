local CreditsTriggerPart2 = {}

CreditsTriggerPart2.name = "MaggyHelper/CreditsTriggerPart2"
CreditsTriggerPart2.canResize = {true, true}
CreditsTriggerPart2.placements = {
    {
        name = "wait_jump_dash",
        data = {
            width = 16,
            height = 16,
            event = "WaitJumpDash"
        }
    },
    {
        name = "wait_jump_double_dash",
        data = {
            width = 16,
            height = 16,
            event = "WaitJumpDoubleDash"
        }
    },
    {
        name = "wait_jump_quintriple_dash",
        data = {
            width = 16,
            height = 16,
            event = "WaitJumpQuintripleDash"
        }
    },
    {
        name = "climb_down",
        data = {
            width = 16,
            height = 16,
            event = "ClimbDown"
        }
    },
    {
        name = "wait",
        data = {
            width = 16,
            height = 16,
            event = "Wait"
        }
    },
    {
        name = "badeline_offset",
        data = {
            width = 16,
            height = 16,
            event = "BadelineOffset"
        }
    },
    {
        name = "chara_offset",
        data = {
            width = 16,
            height = 16,
            event = "CharaOffset"
        }
    },
    {
        name = "kirby_offset",
        data = {
            width = 16,
            height = 16,
            event = "KirbyOffset"
        }
    },
    {
        name = "ralsei_offset",
        data = {
            width = 16,
            height = 16,
            event = "RalseiOffset"
        }
    },
    {
        name = "oshiro_marker",
        data = {
            width = 16,
            height = 16,
            event = "Oshiro"
        }
    }
}

CreditsTriggerPart2.fieldInformation = {
    event = {
        fieldType = "string",
        options = {
            "WaitJumpDash",
            "WaitJumpDoubleDash",
            "WaitJumpQuintripleDash",
            "ClimbDown",
            "Wait",
            "BadelineOffset",
            "CharaOffset",
            "KirbyOffset",
            "RalseiOffset",
            "Oshiro"
        },
        editable = false
    }
}

CreditsTriggerPart2.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
    "event"
}

function CreditsTriggerPart2.selection(room, entity)
    return {
        entity.x,
        entity.y,
        entity.width or 16,
        entity.height or 16
    }
end

return CreditsTriggerPart2
