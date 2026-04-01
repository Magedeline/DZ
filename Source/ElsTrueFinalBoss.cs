using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace MaggyHelper.Entities
{
    /// <summary>
    /// Split root for ElsTrueFinalBoss. The heavy battle implementation lives in partial files;
    /// this part owns custom-sequence execution and summonable knight-clone support.
    /// </summary>
    public partial class ElsTrueFinalBoss
    {
        private readonly List<ElsKnightCloneBoss> summonedKnightClones = new List<ElsKnightCloneBoss>();
        private ElsPhase automaticCloneSummonPhase;
        private bool automaticCloneSummonPhaseInitialized;
        private float automaticCloneSummonCooldown;

        public IReadOnlyList<ElsKnightCloneBoss> SummonedKnightClones => summonedKnightClones;

        public GalactaKnightClone SummonGalactaKnightClone(Vector2 offset)
        {
            return SpawnKnightClone(new GalactaKnightClone(Position + offset));
        }

        public GalactaKnightClone SummonGalacticKnightClone(Vector2 offset)
        {
            return SummonGalactaKnightClone(offset);
        }

        public MorphoKnightClone SummonMorphoKnightClone(Vector2 offset)
        {
            return SpawnKnightClone(new MorphoKnightClone(Position + offset));
        }

        public void ClearSummonedKnightClones()
        {
            for (int i = summonedKnightClones.Count - 1; i >= 0; i--)
            {
                ElsKnightCloneBoss clone = summonedKnightClones[i];
                if (clone != null && clone.Scene != null)
                    clone.RemoveSelf();
            }

            summonedKnightClones.Clear();
        }

        private T SpawnKnightClone<T>(T clone) where T : ElsKnightCloneBoss
        {
            if (clone == null)
                return null;

            PruneSummonedKnightClones();
            summonedKnightClones.Add(clone);

            if (Scene != null)
                Scene.Add(clone);

            return clone;
        }

        private void PruneSummonedKnightClones()
        {
            for (int i = summonedKnightClones.Count - 1; i >= 0; i--)
            {
                ElsKnightCloneBoss clone = summonedKnightClones[i];
                if (clone == null || clone.Scene == null || clone.IsDefeated)
                    summonedKnightClones.RemoveAt(i);
            }

            while (summonedKnightClones.Count >= 4)
            {
                ElsKnightCloneBoss oldestClone = summonedKnightClones[0];
                if (oldestClone != null && oldestClone.Scene != null)
                    oldestClone.RemoveSelf();
                summonedKnightClones.RemoveAt(0);
            }
        }

        private bool HasKnightClone(ElsKnightCloneKind kind)
        {
            PruneSummonedKnightClones();

            for (int i = 0; i < summonedKnightClones.Count; i++)
            {
                ElsKnightCloneBoss clone = summonedKnightClones[i];
                if (clone != null && clone.Kind == kind && clone.Scene != null && !clone.IsDefeated)
                    return true;
            }

            return false;
        }

        private void UpdateAutomaticKnightCloneSummons()
        {
            if (Scene == null || Health <= 0 || introSequencePlaying || Moving || currentState == BossState.Transitioning)
                return;

            if (automaticCloneSummonCooldown > 0f)
                automaticCloneSummonCooldown -= Engine.DeltaTime;

            PruneSummonedKnightClones();

            if (!automaticCloneSummonPhaseInitialized || automaticCloneSummonPhase != currentElsPhase)
            {
                automaticCloneSummonPhaseInitialized = true;
                automaticCloneSummonPhase = currentElsPhase;
                ClearSummonedKnightClones();
                automaticCloneSummonCooldown = currentElsPhase == ElsPhase.SiamoZero ? 0.35f : 0.8f;
                return;
            }

            if (automaticCloneSummonCooldown > 0f)
                return;

            switch (currentElsPhase)
            {
                case ElsPhase.DoppiaElillca:
                    if ((totalHitsTaken >= 1 || Health <= MaxHealth * 0.85f) && !HasKnightClone(ElsKnightCloneKind.Galacta))
                    {
                        SummonGalactaKnightClone(new Vector2(-112f, -16f));
                        automaticCloneSummonCooldown = 9f;
                    }
                    break;

                case ElsPhase.PenumbraPhastasm:
                    bool spawnedPenumbraClone = false;

                    if (!HasKnightClone(ElsKnightCloneKind.Morpho))
                    {
                        SummonMorphoKnightClone(new Vector2(112f, -16f));
                        spawnedPenumbraClone = true;
                    }

                    if (isInVoidMode && !HasKnightClone(ElsKnightCloneKind.Galacta))
                    {
                        SummonGalactaKnightClone(new Vector2(-112f, -16f));
                        spawnedPenumbraClone = true;
                    }

                    if (spawnedPenumbraClone)
                        automaticCloneSummonCooldown = isInVoidMode ? 10f : 8.5f;
                    break;

                case ElsPhase.SiamoZero:
                    bool spawnedSiamoClone = false;

                    if (!HasKnightClone(ElsKnightCloneKind.Galacta))
                    {
                        SummonGalactaKnightClone(new Vector2(-128f, -18f));
                        spawnedSiamoClone = true;
                    }

                    if (!HasKnightClone(ElsKnightCloneKind.Morpho))
                    {
                        SummonMorphoKnightClone(new Vector2(128f, -18f));
                        spawnedSiamoClone = true;
                    }

                    if (spawnedSiamoClone)
                        automaticCloneSummonCooldown = 11f;
                    break;
            }
        }

        private bool TryExecuteNamedAttack(string attackName)
        {
            if (string.IsNullOrWhiteSpace(attackName))
                return false;

            string trimmed = attackName.Trim();
            if (TryExecuteSiamoAttack(trimmed))
                return true;

            switch (NormalizeAttackToken(trimmed))
            {
                case "doppiacloneassault":
                    doppiaCloneAssault();
                    return true;
                case "dualitywave":
                    dualityWave();
                    return true;
                case "shadowblast":
                    shadowBlast();
                    return true;
                case "mirrordimension":
                    mirrorDimension();
                    return true;
                case "dimensionaldefense":
                    dimensionalDefense();
                    return true;
                case "dualityheal":
                    dualityHeal();
                    return true;
                case "riftstrikecombo":
                    riftStrikeCombo();
                    return true;
                case "quickdashattack":
                    quickDashAttack();
                    return true;
                case "energyorbshot":
                    energyOrbShot();
                    return true;
                case "burstheal":
                    burstHeal();
                    return true;

                case "penumbravoidstorm":
                    penumbraVoidStorm();
                    return true;
                case "phantasmbarrage":
                    phantasmBarrage();
                    return true;
                case "voidcollapse":
                case "voidcollapseattack":
                    voidCollapseAttack();
                    return true;
                case "dimensionaltear":
                    dimensionalTear();
                    return true;
                case "ultimateannihilation":
                    ultimateAnnihilation();
                    return true;
                case "voidshield":
                    voidShield();
                    return true;
                case "penumbraregeneration":
                    penumbraRegeneration();
                    return true;
                case "dimensionalcataclysm":
                    dimensionalCataclysm();
                    return true;
                case "riftmaelstrom":
                    riftMaelstrom();
                    return true;
                case "apocalypticriftblast":
                    apocalypticRiftBlast();
                    return true;

                case "summongalactaknightclone":
                case "summongalacticknightclone":
                case "galactaknightclone":
                case "galacticknightclone":
                    SummonGalactaKnightClone(new Vector2(-112f, -16f));
                    return true;
                case "summonmorphoknightclone":
                case "morphoknightclone":
                    SummonMorphoKnightClone(new Vector2(112f, -16f));
                    return true;
                case "clearknightclones":
                case "dismissknightclones":
                    ClearSummonedKnightClones();
                    return true;
            }

            return false;
        }

        private IEnumerator RunCustomAttackSequence()
        {
            if (customAttackSteps == null || customAttackSteps.Count == 0)
                yield break;

            while (isAttacking && currentState != BossState.Defeated)
            {
                for (int i = 0; i < customAttackSteps.Count; i++)
                {
                    if (!isAttacking || currentState == BossState.Defeated || Moving)
                        yield break;

                    AttackStep step = customAttackSteps[i];
                    TryExecuteNamedAttack(step.Action);

                    float waitTime = step.Delay > 0f ? step.Delay : GetDefaultAttackDelay(step.Action);
                    yield return Math.Max(0.15f, waitTime);
                }
            }
        }

        private IEnumerator RunDefaultAttackSequence()
        {
            while (isAttacking && currentState != BossState.Defeated)
            {
                if (Moving || !Collidable)
                    yield break;

                ExecutePhasePatternAttack();
                yield return GetPhaseAttackDelay();
            }
        }

        private void ExecutePhasePatternAttack()
        {
            int attackSeed = Math.Abs(patternIndex + totalHitsTaken + currentPhase);

            switch (currentElsPhase)
            {
                case ElsPhase.DoppiaElillca:
                    ExecuteDoppiaAttack(attackSeed % 10);
                    break;

                case ElsPhase.PenumbraPhastasm:
                    ExecutePenumbraAttack((attackSeed + (int)dimensionRiftPower / 10) % 10);
                    break;

                case ElsPhase.SiamoZero:
                    if (!siamoZeroCombatActive)
                        ActivateSiamoZeroCombat();

                    ExecuteSiamoAttack((SiamoAttackType)(attackSeed % Enum.GetValues(typeof(SiamoAttackType)).Length));
                    break;
            }
        }

        private static string NormalizeAttackToken(string attackName)
        {
            return attackName
                .Replace("_", string.Empty)
                .Replace(" ", string.Empty)
                .Trim()
                .ToLowerInvariant();
        }

        private AttackStep ParseAttackSequenceEntry(string token)
        {
            string[] parts = token.Split(new[] { ':', '@' }, StringSplitOptions.RemoveEmptyEntries);
            string action = parts.Length > 0 ? parts[0].Trim() : string.Empty;
            float delay = GetDefaultAttackDelay(action);
            float arg = 0f;

            if (parts.Length > 1)
                float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out delay);

            if (parts.Length > 2)
                float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out arg);

            return new AttackStep(action, delay, arg);
        }

        private float GetDefaultAttackDelay(string action)
        {
            switch (NormalizeAttackToken(action))
            {
                case "doppiacloneassault":
                case "penumbravoidstorm":
                case "summongalactaknightclone":
                case "summongalacticknightclone":
                case "summonmorphoknightclone":
                    return 1.35f;

                case "dualitywave":
                case "voidcollapse":
                case "voidcollapseattack":
                case "ultimateannihilation":
                case "apocalypticriftblast":
                case "timebordercollapse":
                    return 1.6f;

                case "dimensionaldefense":
                case "dualityheal":
                case "burstheal":
                case "voidshield":
                case "penumbraregeneration":
                    return 1.15f;

                case "riftstrikecombo":
                case "dimensionalcataclysm":
                case "riftmaelstrom":
                case "vortexstrike":
                case "morphoemerge":
                    return 1.45f;

                case "quickdashattack":
                case "energyswordcombo":
                case "doublesideslash":
                    return 0.85f;

                default:
                    return 1f;
            }
        }

        private float GetPhaseAttackDelay()
        {
            switch (currentElsPhase)
            {
                case ElsPhase.PenumbraPhastasm:
                    return isInVoidMode ? 0.9f : 1.15f;

                case ElsPhase.SiamoZero:
                    return 0.85f;

                default:
                    return 1.2f;
            }
        }
    }
}