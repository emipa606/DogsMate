using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Mlie;
using UnityEngine;
using Verse;

namespace Revolus.DogsMate;

public class DogsMateMod : Mod
{
    public const bool MessageInDevModeDefault = false;
    public const bool MessageAlwaysDefault = false;

    public static bool MessageInDevMode = MessageInDevModeDefault;
    public static bool MessageAlways = MessageAlwaysDefault;
    private static string currentVersion;
    public static DogsMateMod instance;

    private static readonly
        Dictionary<PawnKindDef, IReadOnlyDictionary<PawnKindDef, IReadOnlyCollection<HybridDef>>>
        compatibleFemalesDict =
            new Dictionary<PawnKindDef, IReadOnlyDictionary<PawnKindDef, IReadOnlyCollection<HybridDef>>>();

    private Settings settings;

    public DogsMateMod(ModContentPack content) : base(content)
    {
        instance = this;
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(ModLister.GetActiveModWithIdentifier("Mlie.DogsMate"));
        var harmony = new Harmony(nameof(DogsMateMod));
        harmony.PatchAll();
    }

    public Settings Settings
    {
        get
        {
            if (settings == null)
            {
                settings = GetSettings<Settings>();
            }

            return settings;
        }
        set => settings = value;
    }

    public static void Debug(string message, [CallerLineNumberAttribute] int line = 0,
        [CallerMemberName] string caller = null)
    {
        if (MessageAlways || MessageInDevMode && Prefs.DevMode)
        {
            Log.Message($"[DogsMate @ {caller}:{line}] (dev mode message) {message}");
        }
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);

        var listing = new Listing_Standard();
        listing.Begin(inRect);
        listing.CheckboxLabeled("DM.debug".Translate(), ref MessageAlways);
        listing.CheckboxLabeled("DM.debugdev".Translate(), ref MessageInDevMode);

        if (currentVersion != null)
        {
            listing.Gap();
            GUI.contentColor = Color.gray;
            listing.Label("DM.modversion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing.End();
    }

    public override string SettingsCategory()
    {
        return "Dogs Mate";
    }

    public static bool TryGetCompatibleFemales(PawnKindDef malePawnKindDef,
        out IReadOnlyDictionary<PawnKindDef, IReadOnlyCollection<HybridDef>> dict)
    {
        if (malePawnKindDef is null)
        {
            dict = default;
            return false;
        }

        if (compatibleFemalesDict.TryGetValue(malePawnKindDef, out dict))
        {
            return dict != null;
        }

        var d = new Dictionary<PawnKindDef, HashSet<HybridDef>>
        {
            { malePawnKindDef, new HashSet<HybridDef>() }
        };

        // explicitly stated to be compatible
        if (AnimalGroupDef.TryGetGroups(malePawnKindDef, out var sameGroupList))
        {
            var s = sameGroupList.Where(g => g.canMate).Select(g => g.pawnKinds).SelectMany(x => x)
                .Where(p => p != malePawnKindDef);
            foreach (var femalePawnKindDef in s)
            {
                if (!d.ContainsKey(femalePawnKindDef))
                {
                    d.Add(femalePawnKindDef, new HashSet<HybridDef>());
                }
            }
        }

        // possible parents of hybrids are always compatible
        if (HybridDef.TryGetParents(malePawnKindDef, out var parentList))
        {
            foreach (var femalePawnKindDef in parentList)
            {
                if (!d.ContainsKey(femalePawnKindDef))
                {
                    d.Add(femalePawnKindDef, new HashSet<HybridDef>());
                }
            }
        }

        // possible mate to produce a hybrid
        if (HybridDef.TryGetHybrids(malePawnKindDef, out var hybridDict))
        {
            var s = hybridDict.Select(kv =>
                    kv.Key.pawnKinds.Where(p => p != null && p != malePawnKindDef).Select(p => (p, kv.Value)))
                .SelectMany(kv => kv);
            foreach (var (femalePawnKindDef, hybridDefs) in s)
            {
                if (!d.TryGetValue(femalePawnKindDef, out var set))
                {
                    set = new HashSet<HybridDef>();
                    d.Add(femalePawnKindDef, set);
                }

                set.AddRange(hybridDefs);
            }
        }

        // make immutable and memorize
        IReadOnlyDictionary<PawnKindDef, IReadOnlyCollection<HybridDef>> d2 = null;
        if (d.Count > 0)
        {
            d2 = d.ToDictionary(
                kv => kv.Key,
                kv => (IReadOnlyCollection<HybridDef>)kv.Value
            );
        }

        compatibleFemalesDict[malePawnKindDef] = d2;
        dict = d2;
        return dict != null;
    }
}