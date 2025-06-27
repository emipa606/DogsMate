using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Revolus.DogsMate;

public class CrossBreedStatWorker : StatWorker
{
    private static List<ThingDef> possibleCrossbreeds(StatRequest req)
    {
        if (req.Pawn != null && DogsMate.ValidAnimals.Contains(req.Pawn.def))
        {
            return !req.Pawn.def.race.Animal ? [] : req.Pawn.def.race.canCrossBreedWith;
        }

        if (req.Thing != null && DogsMate.ValidAnimals.Contains(req.Thing.def))
        {
            return req.Thing.def.race?.Animal == false ? [] : req.Thing.def.race?.canCrossBreedWith;
        }

        if (req.Def == null)
        {
            return [];
        }

        switch (req.Def)
        {
            case ThingDef thingDef:
                return !DogsMate.ValidAnimals.Contains(thingDef) ? [] : thingDef.race.canCrossBreedWith;
            case PawnKindDef pawnKindDef:
                return !DogsMate.ValidAnimals.Contains(pawnKindDef.race) ? [] : pawnKindDef.race.race.canCrossBreedWith;
        }

        return [];
    }

    public override bool ShouldShowFor(StatRequest req)
    {
        return possibleCrossbreeds(req).Any();
    }

    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        return 0;
    }

    public override string GetStatDrawEntryLabel(StatDef statDef, float value, ToStringNumberSense numberSense,
        StatRequest optionalReq, bool finalized = true)
    {
        var animalDefs = possibleCrossbreeds(optionalReq);
        return animalDefs.Any()
            ? animalDefs.Select(h => (string)h.LabelCap).ToCommaList()
            : "";
    }

    public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest req)
    {
        var animalDefs = possibleCrossbreeds(req);
        if (animalDefs.Any())
        {
            return animalDefs.OrderBy(thingDef => thingDef.label, StringComparer.InvariantCultureIgnoreCase)
                .Select(thingDef => new Dialog_InfoCard.Hyperlink(thingDef));
        }

        return [];
    }

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        var animalDefs = possibleCrossbreeds(req);
        return animalDefs.Any()
            ? animalDefs.Select(h => (string)h.LabelCap).ToCommaList()
            : "";
    }

    public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense,
        float finalVal)
    {
        var animalDefs = possibleCrossbreeds(req);
        if (animalDefs.Any())
        {
            return string.Join("\n\n",
                animalDefs.OrderBy(g => g.label, StringComparer.InvariantCultureIgnoreCase)
                    .Select(g => g.description));
        }

        return "";
    }
}