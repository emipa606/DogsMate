using System.Collections.Generic;
using Verse;

namespace Revolus.DogsMate;

public class AnimalGroupDef : Def
{
    public readonly HashSet<ThingDef> FoundRaces = [];
    public readonly bool canMate = true;
    private readonly List<string> pawnKinds = [];

    private bool? isUsable;

    private List<PawnKindDef> FoundPawnKinds
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            field = [];
            foreach (var pawnKind in pawnKinds)
            {
                var kindFound = DefDatabase<PawnKindDef>.GetNamedSilentFail(pawnKind);
                if (kindFound == null)
                {
                    continue;
                }

                field.Add(kindFound);
                FoundRaces.Add(kindFound.race);
            }

            return field;
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