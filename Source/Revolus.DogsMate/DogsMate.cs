using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Revolus.DogsMate;

[StaticConstructorOnStartup]
public static class DogsMate
{
    public static readonly List<ThingDef> ValidAnimals;

    static DogsMate()
    {
        var correctedAnimals = new HashSet<ThingDef>();
        foreach (var animal in DefDatabase<ThingDef>.AllDefsListForReading.Where(def =>
                     def.race?.Animal == true && !def.IsCorpse))
        {
            animal.race.canCrossBreedWith ??= [];

            if (!animal.race.canCrossBreedWith.Any())
            {
                continue;
            }

            foreach (var otherAnimal in animal.race.canCrossBreedWith)
            {
                otherAnimal.race.canCrossBreedWith ??= [];
                if (otherAnimal.race.canCrossBreedWith.Contains(animal))
                {
                    continue;
                }

                otherAnimal.race.canCrossBreedWith.Add(animal);
                correctedAnimals.Add(otherAnimal);
            }
        }

        var animalGroups = DefDatabase<AnimalGroupDef>.AllDefsListForReading.Where(def => def.IsUsable);
        HashSet<string> modifiedAnimals = [];
        foreach (var animalGroup in animalGroups)
        {
            modifiedAnimals.Add(
                $"{animalGroup.label}: [{string.Join(", ", animalGroup.FoundRaces.Select(def => def.label))}]");
            foreach (var animal in animalGroup.FoundRaces)
            {
                animal.race.canCrossBreedWith ??= [];
                foreach (var otherAnimal in animalGroup.FoundRaces.Where(def => def != animal))
                {
                    if (!animal.race.canCrossBreedWith.Contains(otherAnimal))
                    {
                        animal.race.canCrossBreedWith.Add(otherAnimal);
                    }
                }
            }
        }

        if (modifiedAnimals.Any())
        {
            Log.Message(
                $"[DogsMate]: Modified {modifiedAnimals.Count} animal-groups to allow crossbreeding: {Environment.NewLine}{string.Join(Environment.NewLine, modifiedAnimals)}");
        }

        if (correctedAnimals.Any())
        {
            Log.Message(
                $"[DogsMate]: Corrected {correctedAnimals.Count} faulty configured animals: {Environment.NewLine}{string.Join(Environment.NewLine, correctedAnimals.Select(def => def.label))}");
        }

        ValidAnimals = DefDatabase<ThingDef>.AllDefsListForReading.Where(def =>
            def.race?.Animal == true && !def.IsCorpse && def.race.canCrossBreedWith?.Any() == true).ToList();
    }
}