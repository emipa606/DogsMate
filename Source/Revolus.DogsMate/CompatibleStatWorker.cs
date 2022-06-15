using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Revolus.DogsMate;

public class CompatibleStatWorker : StatWorker
{
    public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense,
        float finalVal)
    {
        if (AnimalGroupDef.TryGetGroups(req, out var groups))
        {
            return string.Join("\n\n",
                groups.Where(g => g.canMate && g.FoundPawnKinds.Count >= 2 && !g.description.NullOrEmpty())
                    .OrderBy(g => g.label, StringComparer.InvariantCultureIgnoreCase).Select(g => g.description));
        }

        return "";
    }

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        if (AnimalGroupDef.TryGetGroups(req, out var groups))
        {
            return groups.Where(g => g.FoundPawnKinds.Count >= 2).Select(m => (string)m.LabelCap)
                .OrderBy(m => m, StringComparer.InvariantCultureIgnoreCase).ToCommaList(true);
        }

        return "";
    }

    public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest req)
    {
        if (AnimalGroupDef.TryGetGroups(req, out var group))
        {
            return group.Where(g => g.FoundPawnKinds.Count >= 2).Select(g => g.FoundPawnKinds).SelectMany(x => x)
                .Select(m => (Def)DefDatabase<ThingDef>.GetNamedSilentFail(m.defName) ?? m)
                .OrderBy(x => x.label, StringComparer.InvariantCultureIgnoreCase)
                .Select(m => new Dialog_InfoCard.Hyperlink(m));
        }

        return Array.Empty<Dialog_InfoCard.Hyperlink>();
    }

    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        return 0;
    }

    public override bool ShouldShowFor(StatRequest req)
    {
        return AnimalGroupDef.TryGetGroups(req, out var groupDefs) &&
               groupDefs.Any(g => g.canMate && g.FoundPawnKinds.Count >= 2);
    }

    public override string GetStatDrawEntryLabel(StatDef statDef, float value, ToStringNumberSense numberSense,
        StatRequest optionalReq, bool finalized = true)
    {
        if (AnimalGroupDef.TryGetGroups(optionalReq, out var groups))
        {
            return groups.Where(g => g.canMate && g.FoundPawnKinds.Count >= 2).Select(g => (string)g.LabelCap)
                .ToCommaList();
        }

        return "";
    }
}