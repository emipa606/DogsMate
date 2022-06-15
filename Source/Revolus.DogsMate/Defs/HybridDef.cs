using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Revolus.DogsMate;

public class HybridDef : Def
{
    private static IReadOnlyDictionary<AnimalGroupDef, IReadOnlyDictionary<AnimalGroupDef, IReadOnlyList<HybridDef>>
    > _dict;

    private static readonly Dictionary<PawnKindDef, IReadOnlyDictionary<AnimalGroupDef, IReadOnlyList<HybridDef>>>
        kindToHybrids =
            new Dictionary<PawnKindDef, IReadOnlyDictionary<AnimalGroupDef, IReadOnlyList<HybridDef>>>();

    private static IReadOnlyDictionary<PawnKindDef, IReadOnlyList<PawnKindDef>> _hybridParents;

    public readonly List<HybridHediff> childrenHediffs = new List<HybridHediff>();
    public readonly List<HybridHediff> femaleChildrenHediffs = new List<HybridHediff>();
    public readonly List<HybridHediff> maleChildrenHediffs = new List<HybridHediff>();

    private bool? _isUsable;
    public List<AnimalGroupDef> children = new List<AnimalGroupDef>();

    public SimpleCurve fertilizationFailesIfGreaterThanZeroCurve;
    public List<AnimalGroupDef> parents = new List<AnimalGroupDef>();

    public bool IsUsable
    {
        get
        {
            _isUsable ??= parents != null && parents.Where(p => p.IsUsable && p.canMate).Distinct().Count() >= 2 &&
                          children != null && children.Any(p => p.IsUsable);
            return _isUsable.Value;
        }
    }

    private static
        IReadOnlyDictionary<AnimalGroupDef, IReadOnlyDictionary<AnimalGroupDef, IReadOnlyList<HybridDef>>>
        LookupDict
    {
        get
        {
            if (_dict is null)
            {
                _dict = DefDatabase<HybridDef>.AllDefsListForReading.Where(def => def.IsUsable)
                    .Select(h =>
                        h.parents.Where(a => a.IsUsable).Cross().Where(ab => ab.Item1 != ab.Item2)
                            .Select(ab => (a: ab.Item1, b: ab.Item2, h))).SelectMany(x => x).GroupBy(x => x.a)
                    .ToDictionary(g => g.Key,
                        g => g.GroupBy(x => x.b).ToDictionary(g2 => g2.Key,
                            g2 => g2.Select(x => x.h).Distinct().ToList().AsReadOnlyList()).AsReadOnlyDict())
                    .AsReadOnlyDict();
            }

            return _dict;
        }
    }

    private static IReadOnlyDictionary<PawnKindDef, IReadOnlyList<PawnKindDef>> HybridParents
    {
        get
        {
            if (_hybridParents is null)
            {
                _hybridParents = DefDatabase<HybridDef>.AllDefsListForReading.Where(h => h != null)
                    .Select(h =>
                        h.children.Where(a => a.IsUsable).Select(a => a.FoundPawnKinds.Where(c => c != null))
                            .SelectMany(x => x).Select(c => (c, a: h.parents.Where(a => a.IsUsable))))
                    .SelectMany(x => x).GroupBy(kv => kv.c).ToDictionary(g => g.Key,
                        g => g.Select(x => x.a).SelectMany(a => a).Select(a => a.FoundPawnKinds.Where(p => p != null))
                            .SelectMany(p => p).Distinct().ToList().AsReadOnlyList());
            }

            return _hybridParents;
        }
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var item in base.ConfigErrors())
        {
            yield return item;
        }

        if (parents.Count < 2)
        {
            yield return $"{nameof(HybridDef)}.{nameof(parents)} needs at least 2 items";
        }
        else if (parents.Distinct().Count() < parents.Count)
        {
            yield return $"{nameof(HybridDef)}.{nameof(parents)} must not have duplicated elements";
        }

        if (children.Count < 1)
        {
            yield return $"{nameof(HybridDef)}.{nameof(children)} needs at least 1 item";
        }
    }

    public static bool TryGetHybrids(AnimalGroupDef group,
        out IReadOnlyDictionary<AnimalGroupDef, IReadOnlyList<HybridDef>> hybrids)
    {
        if (group is not null)
        {
            return LookupDict.TryGetValue(group, out hybrids);
        }

        hybrids = default;
        return false;
    }

    public static bool TryGetHybrids(PawnKindDef kindDef,
        out IReadOnlyDictionary<AnimalGroupDef, IReadOnlyList<HybridDef>> hybrids)
    {
        if (kindToHybrids.TryGetValue(kindDef, out hybrids))
        {
            return hybrids != null && hybrids.Count > 0;
        }

        if (AnimalGroupDef.TryGetGroups(kindDef, out var groups))
        {
            hybrids = groups.Select(a => TryGetHybrids(a, out var d) ? d : null).Where(d => d != null)
                .SelectMany(x => x).GroupBy(kv => kv.Key).ToDictionary(g => g.Key,
                    g => g.Select(kv => kv.Value.Where(h => h.IsUsable)).SelectMany(x => x).Distinct().ToList()
                        .AsReadOnlyList()).AsReadOnlyDict();
        }

        kindToHybrids.Add(kindDef, hybrids);
        return hybrids != null && hybrids.Count > 0;
    }

    private static bool TryGetHybrids(ThingDef thingDef,
        out IReadOnlyDictionary<AnimalGroupDef, IReadOnlyList<HybridDef>> hybrids)
    {
        return TryGetHybrids(thingDef?.race?.AnyPawnKind, out hybrids);
    }

    private static bool TryGetHybrids(Pawn pawn,
        out IReadOnlyDictionary<AnimalGroupDef, IReadOnlyList<HybridDef>> hybrids)
    {
        return TryGetHybrids(pawn?.kindDef, out hybrids);
    }

    private static bool TryGetHybrids(Thing thing,
        out IReadOnlyDictionary<AnimalGroupDef, IReadOnlyList<HybridDef>> hybrids)
    {
        return TryGetHybrids(thing as Pawn, out hybrids);
    }

    private static bool TryGetHybrids(Def def,
        out IReadOnlyDictionary<AnimalGroupDef, IReadOnlyList<HybridDef>> hybrids)
    {
        if (def is PawnKindDef kindDef)
        {
            return TryGetHybrids(kindDef, out hybrids);
        }

        if (def is ThingDef thingDef)
        {
            return TryGetHybrids(thingDef, out hybrids);
        }

        hybrids = default;
        return false;
    }

    public static bool TryGetHybrids(StatRequest req,
        out IReadOnlyDictionary<AnimalGroupDef, IReadOnlyList<HybridDef>> hybrids)
    {
        if (req.Pawn != null)
        {
            return TryGetHybrids(req.Pawn, out hybrids);
        }

        if (req.Thing != null)
        {
            return TryGetHybrids(req.Thing, out hybrids);
        }

        if (req.Def != null)
        {
            return TryGetHybrids(req.Def, out hybrids);
        }

        hybrids = default;
        return false;
    }

    public static bool TryGetParents(PawnKindDef childDef, out IReadOnlyList<PawnKindDef> parents)
    {
        if (childDef is not null)
        {
            return HybridParents.TryGetValue(childDef, out parents);
        }

        parents = default;
        return false;
    }

    public static bool TryGetHybrids(PawnKindDef a, PawnKindDef b, out IReadOnlyList<HybridDef> hybrids)
    {
        if (a == b || !TryGetHybrids(a, out var aHybridDict) ||
            !AnimalGroupDef.TryGetGroups(b, out var bGroupsList))
        {
            hybrids = default;
            return false;
        }

        hybrids = bGroupsList.Select(bGroup => aHybridDict.TryGetValue(bGroup, out var l) ? l : null)
            .Where(l => l != null).SelectMany(x => x).Distinct().ToList();
        return hybrids.Count > 0;
    }
}