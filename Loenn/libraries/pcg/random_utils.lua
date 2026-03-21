-- PCG Random Utilities
-- Provides shared random helpers for PCG modules.

local randomUtils = {}

--- Shuffle a list in-place using random_shuffle-style semantics.
-- Equivalent to C++ std::random_shuffle:
-- - default variant uses global RNG
-- - custom variant accepts rand(n) returning an integer in [0, n)
-- @param list table   Array-like table to shuffle in-place
-- @param rand function|nil Optional rand(n) -> [0, n)
-- @return table       The same table instance, shuffled
function randomUtils.random_shuffle(list, rand)
    assert(type(list) == "table", "random_shuffle expects a table")

    for i = #list, 2, -1 do
        local j

        if rand then
            local r = rand(i)
            assert(type(r) == "number", "rand(n) must return a number")

            -- C++ random_shuffle(rand) expects rand(n) in [0, n)
            j = math.floor(r) + 1
            assert(j >= 1 and j <= i, "rand(n) must return integer in [0, n)")
        else
            j = math.random(1, i)
        end

        list[i], list[j] = list[j], list[i]
    end

    return list
end

return randomUtils