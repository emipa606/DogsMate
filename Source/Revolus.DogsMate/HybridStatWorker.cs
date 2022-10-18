using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Revolus.DogsMate;

public class HybridStatWorker : StatWorker
{
    public override bool ShouldShowFor(StatRequest req)
    {
        return AnimalGroupDef.TryGetGroups(req, out var groups) &&
               groups.Any(a => HybridDef.TryGetHybrids(a, out _));
    }

    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        return 0;
    }

    private static bool TryGetHybrids(StatRequest req, out IEnumerable<HybridDef> hybridDefs)
    {
        if (AnimalGroupDef.TryGetGroups(req, out var groups))
        {
            hybridDefs = groups.Select(a => HybridDef.TryGetHybrids(a, out var h) ? h : null).Where(h => h != null)
                .SelectMany(x => x).Select(h => h.Value).SelectMany(x => x).Distinct();
            return true;
        }

        hybridDefs = default;
        return false;
    }

    private static IEnumerable<AnimalGroupDef> GetAnimalsOf(IEnumerable<HybridDef> hybridDefs)
    {
        return hybridDefs.Select(h => h.children.Where(c => c.IsUsable)).SelectMany(x => x).Distinct();
    }

    private static bool TryGetHybridAnimals(StatRequest req, out IEnumerable<AnimalGroupDef> animalDefs)
    {
        if (TryGetHybrids(req, out var hybridDefs))
        {
            animalDefs = GetAnimalsOf(hybridDefs);
            return true;
        }

        animalDefs = default;
        return false;
    }

    public override string GetStatDrawEntryLabel(StatDef statDef, float value, ToStringNumberSense numberSense,
        StatRequest optionalReq, bool finalized = true)
    {
        return TryGetHybridAnimals(optionalReq, out var animalDefs)
            ? animalDefs.Select(h => (string)h.LabelCap).ToCommaList()
            : "";
    }

    public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest req)
    {
        if (TryGetHybridAnimals(req, out var animalDefs))
        {
            return animalDefs.Select(a => a.FoundPawnKinds.Where(p => p != null)).SelectMany(x => x)
                .Select(m => (Def)DefDatabase<ThingDef>.GetNamedSilentFail(m.defName) ?? m)
                .OrderBy(x => x.label, StringComparer.InvariantCultureIgnoreCase)
                .Select(m => new Dialog_InfoCard.Hyperlink(m));
        }

        return Array.Empty<Dialog_InfoCard.Hyperlink>();
    }

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        if (TryGetHybrids(req, out var hybridDefs))
        {
            return hybridDefs.Select(h =>
            {
                var children = h.children.Where(c => c.IsUsable)
                    .OrderBy(c => c.label, StringComparer.InvariantCultureIgnoreCase).ToList();
                return children.Count == 1
                    ? $"…+{h.label}={children[0].label}"
                    : $"…+{h.label}={string.Join("|", children.Select(c => c.label))}";
            }).OrderBy(x => x, StringComparer.InvariantCultureIgnoreCase).ToCommaList(true);
        }

        return "";
    }

    public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense,
        float finalVal)
    {
        if (TryGetHybridAnimals(req, out var animalDefs))
        {
            return string.Join("\n\n",
                animalDefs.OrderBy(g => g.label, StringComparer.InvariantCultureIgnoreCase)
                    .Select(g => g.description));
        }

        return "";
    }
}