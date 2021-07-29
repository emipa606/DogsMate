using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Revolus.DogsMate
{
    [HarmonyPatch(typeof(PawnUtility), nameof(PawnUtility.Mated), typeof(Pawn), typeof(Pawn))]
    public class Patch__PawnUtility__Mated
    {
        [HarmonyPrefix]
        public static bool Guard_Hediff_AnimalFertilityReduced(Pawn male, Pawn female)
        {
            if (
                HybridDef.TryGetHybrids(male.kindDef, female.kindDef, out var hybridList) &&
                hybridList.TryGetRandomElement(out var hybrid) &&
                hybrid.fertilizationFailesIfGreaterThanZeroCurve.TryGetRandomValue(
                    out var fertilizationFailesIfGreaterThanZero) &&
                fertilizationFailesIfGreaterThanZero >= 0f
            )
            {
                DogsMateMod.Debug(
                    $"fertilizationFailesIfGreaterThanZero: {fertilizationFailesIfGreaterThanZero:0.000}, " +
                    "failed"
                );
                return true;
            }

            var maleHediff = SeverityOf(male);
            var femaleHediff = SeverityOf(female);
            if (maleHediff <= 0f && femaleHediff <= 0f)
            {
                return true;
            }

            if (maleHediff >= 1f || femaleHediff >= 1f)
            {
                DogsMateMod.Debug(
                    $"male fertility: {(1f - maleHediff) * 100:0.0}%, " +
                    $"female fertility: {(1f - femaleHediff) * 100:0.0}%, " +
                    "can't fertilize"
                );
                return false;
            }

            var threshold =
                Mathf.Sqrt((1f - maleHediff) * (1f - femaleHediff) * (1f - Mathf.Max(maleHediff, femaleHediff)));
            DogsMateMod.Debug(
                $"male fertility: {(1f - maleHediff) * 100:0.0}%, " +
                $"female fertility: {(1f - femaleHediff) * 100:0.0}%, " +
                $"fertilation chance:  {threshold * 50:0.0}%" // RimWorld lets every other fertilation fail.
            );
            var value = Rand.Value;
            return value <= threshold;
        }

        private static float SeverityOf(Pawn pawn)
        {
            return pawn?.health?.hediffSet.hediffs.Where(h => h.def is AnimalFertilityReduced)
                       .Aggregate(0f, (v, h) => Math.Max(v, h.Severity))
                   ?? 0;
        }
    }
}