using System.Collections.Generic;
using Verse;

namespace Revolus.DogsMate;

public class AnimalGroupDef : Def
{
    public readonly bool canMate = true;
    public readonly HashSet<ThingDef> FoundRaces = [];
    private readonly List<string> pawnKinds = [];

    private List<PawnKindDef> foundPawnKinds;

    private bool? isUsable;

    public List<PawnKindDef> FoundPawnKinds
    {
        get
        {
            if (foundPawnKinds != null)
            {
                return foundPawnKinds;
            }

            foundPawnKinds = [];
            foreach (var pawnKind in pawnKinds)
            {
                var kindFound = DefDatabase<PawnKindDef>.GetNamedSilentFail(pawnKind);
                if (kindFound == null)
                {
                    continue;
                }

                foundPawnKinds.Add(kindFound);
                FoundRaces.Add(kindFound.race);
            }

            return foundPawnKinds;
        }
    }

    public bool IsUsable
    {
        get
        {
            isUsable ??= FoundPawnKinds != null && FoundPawnKinds.Count(p => p != null) > 1;
            return isUsable.Value;
        }
    }
}