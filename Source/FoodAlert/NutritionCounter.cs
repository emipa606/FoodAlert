using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FoodAlert;

public class NutritionCounter
{
    /// <summary>
    ///     A collection of functions which search the map for nutrition sources other than Things with nutrition values.
    ///     The key should be a globally unique string, allowing inspection and deregistering of specific counters.
    /// </summary>
    public static readonly IDictionary<string, Func<Map, float>> NutritionFinders =
        new Dictionary<string, Func<Map, float>>
        {
            ["default"] = map =>
            {
                var num = 0f;
                var selectedPreferability = FoodAlertMod.settings.foodPreferability;
                var estimateIngredients = selectedPreferability >= FoodPreferability.MealAwful
                    ? FoodAlertMod.settings.estimateIngredients : 0f;
                if( estimateIngredients < 0 )
                {
                    estimateIngredients = selectedPreferability switch
                    {
                        FoodPreferability.MealAwful => 3f,
                        FoodPreferability.MealSimple => 1.8f,
                        FoodPreferability.MealFine => 1.8f, // normal fine meal (vege/meat need custom)
                        FoodPreferability.MealLavish => 1f,
                        _ => 0f
                    };
                }
                foreach (var keyValuePair in map.resourceCounter.AllCountedAmounts)
                {
                    if (keyValuePair.Value <= 0)
                    {
                        continue;
                    }

                    if (!keyValuePair.Key.IsNutritionGivingIngestible)
                    {
                        continue;
                    }

                    if (!keyValuePair.Key.ingestible.HumanEdible)
                    {
                        continue;
                    }

                    if (selectedPreferability > keyValuePair.Key.ingestible.preferability)
                    {
                        // If configured, try to guess if it is usable as a cooking ingredient
                        // and estimate how much food of the preferred type can be made from it.
                        if (estimateIngredients > 0
                            && keyValuePair.Key.ingestible.preferability < FoodPreferability.MealAwful
                            && (keyValuePair.Key.ingestible.foodType & FoodTypeFlags.OmnivoreHuman) != 0)
                        {
                            num += keyValuePair.Key.GetStatValueAbstract(StatDefOf.Nutrition)
                                * keyValuePair.Value * estimateIngredients;
                        }
                        continue;
                    }

                    num += keyValuePair.Key.GetStatValueAbstract(StatDefOf.Nutrition) * keyValuePair.Value;
                }

                return num;
            }
        };

    public static float GetEdibleStuff(Map map)
    {
        var num = 0f;
        foreach (var keyValuePair in NutritionFinders)
        {
            // If one extension fails, don't let it crash the whole counter.
            try
            {
                num += keyValuePair.Value(map);
            }
            catch (Exception ex)
            {
                Log.Error($"[FoodAlert]: Error in nutrition finder \"{keyValuePair.Key}\": {ex.Message}");
            }
        }

        return num;
    }
}