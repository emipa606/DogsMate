using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Revolus.DogsMate;

[HarmonyPatch(typeof(Hediff_Pregnant), nameof(Hediff_Pregnant.DoBirthSpawn), typeof(Pawn), typeof(Pawn))]
public class Patch__Hediff_Pregnant__DoBirthSpawn
{
    private static int GetLitterCount(Pawn pawn)
    {
        var raceProps = pawn.RaceProps;
        if (raceProps.litterSizeCurve is null)
        {
            return 1;
        }

        var n = Mathf.RoundToInt(Rand.ByCurve(pawn.RaceProps.litterSizeCurve));
        return n > 1 ? n : 1;
    }

    [HarmonyPrefix]
    public static bool Replace_DoBirthSpawn(Pawn mother, Pawn father)
    {
        if (father is null || mother.kindDef == father.kindDef)
        {
            return true;
        }

        var litterCount = Math.Min(GetLitterCount(mother), GetLitterCount(father));

        List<(PawnKindDef p, List<HybridDef> h)> hybridKinds = null;
        if (
            DogsMateMod.TryGetCompatibleFemales(father.kindDef, out var dict) &&
            dict.TryGetValue(mother.kindDef, out var hybridDefs) &&
            hybridDefs.Count > 0
        )
        {
            hybridKinds = hybridDefs.Select(h => h.children.Where(c => c.IsUsable).Select(a => (a, h)))
                .SelectMany(x => x).Select(ah => ah.a.FoundPawnKinds.Where(p => p != null).Select(p => (p, ah.h)))
                .SelectMany(x => x).GroupBy(ph => ph.p)
                .Select(g => (g.Key, g.Select(ph => ph.h).Where(h => h.IsUsable).ToList())).ToList();
            DogsMateMod.Debug(
                $"father=<{father.kindDef.ToStringSafe()}> " +
                $"mother=<{mother.kindDef.ToStringSafe()}> " +
                $"hybrids=<{hybridKinds.Select(ph => ph.p.label).ToCommaList()}>"
            );
        }

        Pawn child = null;
        for (var childIndex = 0; childIndex < litterCount; ++childIndex)
        {
            PawnKindDef childKind;
            HybridDef hybridDef = null;
            if (hybridKinds != null)
            {
                hybridKinds.TryGetRandomElement(out var ph);
                ph.h.TryGetRandomElement(out hybridDef);
                childKind = ph.p;
            }
            else if (Rand.Value > 0.5f)
            {
                childKind = mother.kindDef;
            }
            else
            {
                childKind = father.kindDef;
            }

            bool newChildIsGood;
            var newChild = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                childKind,
                mother.Faction,
                forceGenerateNewPawn: false,
                developmentalStages: DevelopmentalStage.Newborn
                //newborn: true
            ));
            if (PawnUtility.TrySpawnHatchedOrBornPawn(newChild, mother))
            {
                if (newChild.playerSettings != null && mother.playerSettings != null)
                {
                    newChild.playerSettings.AreaRestriction = mother.playerSettings.AreaRestriction;
                }

                if (newChild.RaceProps.IsFlesh)
                {
                    newChild.relations.AddDirectRelation(PawnRelationDefOf.Parent, mother);
                    newChild.relations.AddDirectRelation(PawnRelationDefOf.Parent, father);
                }

                if (hybridDef != null)
                {
                    AddHediffs(newChild, hybridDef.childrenHediffs);
                    switch (newChild.gender)
                    {
                        case Gender.Male:
                            AddHediffs(newChild, hybridDef.maleChildrenHediffs);
                            break;
                        case Gender.Female:
                            AddHediffs(newChild, hybridDef.femaleChildrenHediffs);
                            break;
                    }
                }

                newChildIsGood = true;
            }
            else
            {
                Find.WorldPawns.PassToWorld(newChild, PawnDiscardDecideMode.Discard);
                newChildIsGood = false;
            }

            TaleRecorder.RecordTale(TaleDefOf.GaveBirth, mother, newChild);

            if (newChildIsGood)
            {
                child = newChild;
            }
        }

        if (!mother.Spawned)
        {
            return false;
        }

        FilthMaker.TryMakeFilth(mother.Position, mother.Map, ThingDefOf.Filth_AmnioticFluid,
            mother.LabelIndefinite(), 5);
        mother.caller?.DoCall();
        child?.caller?.DoCall();

        return false;
    }

    private static void AddHediffs(Pawn newChild, List<HybridHediff> list)
    {
        if (list is null)
        {
            return;
        }

        foreach (var h in list)
        {
            BodyPartRecord bodyPart = null;
            if (h.bodyPartDef != null)
            {
                foreach (var p in newChild.health.hediffSet.GetNotMissingParts())
                {
                    if (p.def == h.bodyPartDef)
                    {
                        bodyPart = p;
                    }
                }
            }

            var hediff = HediffMaker.MakeHediff(h.hediffDef, newChild, bodyPart);
            if (h.severityCurve.TryGetRandomValue(out var severity))
            {
                hediff.Severity = severity;
            }

            if (hediff.Severity > 0f)
            {
                newChild.health.AddHediff(hediff);
            }
        }
    }
}