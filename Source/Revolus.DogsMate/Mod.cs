using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Revolus.DogsMate
{
    public class DogsMateMod : Mod
    {
        public const bool MessageInDevModeDefault = false;
        public const bool MessageAlwaysDefault = false;

        public static bool MessageInDevMode = MessageInDevModeDefault;
        public static bool MessageAlways = MessageAlwaysDefault;

        private static readonly
            Dictionary<PawnKindDef, IReadOnlyDictionary<PawnKindDef, IReadOnlyCollection<HybridDef>>>
            compatibleFemalesDict =
                new();

        public DogsMateMod(ModContentPack content) : base(content)
        {
            _ = GetSettings<Settings>();

            var harmony = new Harmony(nameof(DogsMateMod));
            harmony.PatchAll();
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

            var oldFont = Text.Font;
            var oldAnchor = Text.Anchor;
            try
            {
                var listing = new Listing_Standard();
                listing.Begin(inRect);
                try
                {
                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleLeft;

                    var row = listing.GetRect(22f);
                    Widgets.DrawHighlightIfMouseover(row);
                    Widgets.CheckboxLabeled(
                        row,
                        "Show debug messages",
                        ref MessageAlways,
                        placeCheckboxNearText: true
                    );

                    row = listing.GetRect(22f);
                    Widgets.DrawHighlightIfMouseover(row);
                    Widgets.CheckboxLabeled(
                        row,
                        "Show debug messages if dev mode is enabled",
                        ref MessageInDevMode,
                        placeCheckboxNearText: true
                    );
                }
                finally
                {
                    listing.End();
                }
            }
            finally
            {
                Text.Font = oldFont;
                Text.Anchor = oldAnchor;
            }
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
                {malePawnKindDef, new HashSet<HybridDef>()}
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
                    kv => (IReadOnlyCollection<HybridDef>) kv.Value
                );
            }

            compatibleFemalesDict[malePawnKindDef] = d2;
            dict = d2;
            return dict != null;
        }
    }

    public class Settings : ModSettings
    {
        public override void ExposeData()
        {
            Scribe_Values.Look(ref DogsMateMod.MessageInDevMode, "MessageInDevMode",
                DogsMateMod.MessageInDevModeDefault, true);
            Scribe_Values.Look(ref DogsMateMod.MessageAlways, "MessageAlways", DogsMateMod.MessageAlwaysDefault, true);
        }
    }
}