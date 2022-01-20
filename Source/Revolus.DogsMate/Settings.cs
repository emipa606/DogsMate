using Verse;

namespace Revolus.DogsMate;

public class Settings : ModSettings
{
    public override void ExposeData()
    {
        Scribe_Values.Look(ref DogsMateMod.MessageInDevMode, "MessageInDevMode",
            DogsMateMod.MessageInDevModeDefault, true);
        Scribe_Values.Look(ref DogsMateMod.MessageAlways, "MessageAlways", DogsMateMod.MessageAlwaysDefault, true);
    }
}