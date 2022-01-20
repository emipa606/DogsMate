using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Revolus.DogsMate;

public class AnimalGroupDef : Def
{
    private static IReadOnlyDictionary<PawnKindDef, IReadOnlyCollection<AnimalGroupDef>> _dict;
    public readonly bool canMate = true;
    public readonly List<PawnKindDef> pawnKinds = new List<PawnKindDef>();

    private bool? _isUsable;

    public bool IsUsable
    {
        get
        {
            _isUsable ??= pawnKinds != null && pawnKinds.Any(p => p != null);
            return _isUsable.Value;
        }
    }

    private static IReadOnlyDictionary<PawnKindDef, IReadOnlyCollection<AnimalGroupDef>> LookupDict
    {
        get
        {
            if (_dict is null)
            {
                _dict = DefDatabase<AnimalGroupDef>.AllDefsListForReading.Where(def => def.IsUsable)
                    .Select(def => def.pawnKinds.Where(p => p != null).Select(pawnKind => (pawnKind, def)))
                    .SelectMany(kv => kv).GroupBy(kv => kv.pawnKind).ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.def).Distinct().ToArray().AsReadOnlyArray()
                    ).AsReadOnlyDict();
            }

            return _dict;
        }
    }

    public static bool TryGetGroups(PawnKindDef kindDef, out IReadOnlyCollection<AnimalGroupDef> groups)
    {
        if (kindDef != null)
        {
            return LookupDict.TryGetValue(kindDef, out groups);
        }

        groups = default;
        return false;
    }

    private static bool TryGetGroups(ThingDef thingDef, out IReadOnlyCollection<AnimalGroupDef> groups)
    {
        return TryGetGroups(thingDef?.race?.AnyPawnKind, out groups);
    }

    private static bool TryGetGroups(Pawn pawn, out IReadOnlyCollection<AnimalGroupDef> groups)
    {
        return TryGetGroups(pawn?.kindDef, out groups);
    }

    private static bool TryGetGroups(Thing thing, out IReadOnlyCollection<AnimalGroupDef> groups)
    {
        return TryGetGroups(thing as Pawn, out groups);
    }

    private static bool TryGetGroups(Def def, out IReadOnlyCollection<AnimalGroupDef> groups)
    {
        if (def is PawnKindDef kindDef)
        {
            return TryGetGroups(kindDef, out groups);
        }

        if (def is ThingDef thingDef)
        {
            return TryGetGroups(thingDef, out groups);
        }

        if (def is AnimalGroupDef animalGroupDef)
        {
            groups = new[] { animalGroupDef };
            return true;
        }

        groups = default;
        return false;
    }

    public static bool TryGetGroups(StatRequest req, out IReadOnlyCollection<AnimalGroupDef> groups)
    {
        if (req.Pawn != null)
        {
            return TryGetGroups(req.Pawn, out groups);
        }

        if (req.Thing != null)
        {
            return TryGetGroups(req.Thing, out groups);
        }

        if (req.Def != null)
        {
            return TryGetGroups(req.Def, out groups);
        }

        groups = default;
        return false;
    }
}