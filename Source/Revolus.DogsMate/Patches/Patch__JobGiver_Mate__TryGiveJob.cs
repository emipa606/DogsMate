using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Revolus.DogsMate;

[HarmonyPatch(typeof(JobGiver_Mate), "TryGiveJob", typeof(Pawn))]
public class Patch__JobGiver_Mate__TryGiveJob
{
    [HarmonyPrefix]
    public static bool Replace_TryGiveJob(ref Job __result, Pawn pawn)
    {
        var malePawn = pawn;

        if (malePawn.GetComp<CompEggLayer>()?.props != null)
        {
            DogsMateMod.Debug(
                $"male=<{malePawn.ToStringSafe()}={malePawn.kindDef.ToStringSafe()}> is egg layer -> using default implementation");
            return true;
        }

        if (!DogsMateMod.TryGetCompatibleFemales(malePawn.kindDef, out var specialDict))
        {
            DogsMateMod.Debug(
                $"male=<{malePawn.ToStringSafe()}={malePawn.kindDef.ToStringSafe()}> is not special -> using default implementation");
            return true;
        }

        if (malePawn.gender != Gender.Male)
        {
            DogsMateMod.Debug(
                $"male=<{malePawn.ToStringSafe()}={malePawn.kindDef.ToStringSafe()}> is male -> fail");
            __result = null;
            return false;
        }

        if (!malePawn.ageTracker.CurLifeStage.reproductive)
        {
            DogsMateMod.Debug(
                $"male=<{malePawn.ToStringSafe()}={malePawn.kindDef.ToStringSafe()}> cannot reproduce in this life stage -> fail");
            __result = null;
            return false;
        }

        if (GenClosest.ClosestThingReachable(
                malePawn.Position,
                malePawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup
                    .Pawn), // original implementation tests for "ThingRequest.ForDef(malePawn.def)"
                PathEndMode.Touch,
                TraverseParms.For(malePawn, Danger.Some), // original implementation allows "Danger.Deadly"
                30f,
                femaleThing => IsValidFemale(femaleThing, malePawn, specialDict)
            ) is Pawn validFemalePawn)
        {
            DogsMateMod.Debug(
                $"male=<{malePawn.ToStringSafe()}={malePawn.kindDef.ToStringSafe()}> female=<{validFemalePawn.ToStringSafe()}={validFemalePawn.kindDef.ToStringSafe()}> will mate -> success"
            );
            __result = JobMaker.MakeJob(JobDefOf.Mate, validFemalePawn);
        }
        else
        {
            DogsMateMod.Debug(
                $"male=<{malePawn.ToStringSafe()}={malePawn.kindDef.ToStringSafe()}> no valid female found -> fail"
            );
            __result = null;
        }

        return false;
    }

    private static bool IsValidFemale(Thing femaleThing, Pawn malePawn,
        IReadOnlyDictionary<PawnKindDef, IReadOnlyCollection<HybridDef>> specialDict)
    {
        if (femaleThing is not Pawn femalePawn)
        {
            return false;
        }

        if (malePawn == femalePawn)
        {
            return false;
        }

        if (!specialDict.ContainsKey(femalePawn.kindDef))
        {
            DogsMateMod.Debug(
                $"male=<{malePawn.ToStringSafe()}={malePawn.kindDef.ToStringSafe()}> female=<{femalePawn.ToStringSafe()}={femalePawn.kindDef.ToStringSafe()}> not a valid mate"
            );
            return false;
        }

        if (femalePawn.Downed)
        {
            DogsMateMod.Debug(
                $"male=<{malePawn.ToStringSafe()}={malePawn.kindDef.ToStringSafe()}> female=<{femalePawn.ToStringSafe()}={femalePawn.kindDef.ToStringSafe()}> female downed"
            );
            return false;
        }

        if (!femalePawn.CanCasuallyInteractNow())
        {
            DogsMateMod.Debug(
                $"male=<{malePawn.ToStringSafe()}={malePawn.kindDef.ToStringSafe()}> female=<{femalePawn.ToStringSafe()}={femalePawn.kindDef.ToStringSafe()}> cannot interact casually with female"
            );
            return false;
        }

        if (femalePawn.IsForbidden(malePawn))
        {
            DogsMateMod.Debug(
                $"male=<{malePawn.ToStringSafe()}={malePawn.kindDef.ToStringSafe()}> female=<{femalePawn.ToStringSafe()}={femalePawn.kindDef.ToStringSafe()}> female is forbidden"
            );
            return false;
        }

        if (femalePawn.Faction != malePawn.Faction)
        {
            DogsMateMod.Debug(
                $"male=<{malePawn.ToStringSafe()}={malePawn.kindDef.ToStringSafe()}> female=<{femalePawn.ToStringSafe()}={femalePawn.kindDef.ToStringSafe()}> not same faction"
            );
            return false;
        }

        if (!PawnUtility.FertileMateTarget(malePawn, femalePawn))
        {
            DogsMateMod.Debug(
                $"male=<{malePawn.ToStringSafe()}={malePawn.kindDef.ToStringSafe()}> female=<{femalePawn.ToStringSafe()}={femalePawn.kindDef.ToStringSafe()}> female is not fertile"
            );
            return false;
        }

        DogsMateMod.Debug(
            $"male=<{malePawn.ToStringSafe()}={malePawn.kindDef.ToStringSafe()}> female=<{femalePawn.ToStringSafe()}={femalePawn.kindDef.ToStringSafe()}> is valid pair"
        );
        return true;
    }
}