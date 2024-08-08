using System.Linq;
using RimWorld;
using Verse;

namespace FoodAlert.Extensions.VENutrientPaste;

[StaticConstructorOnStartup]
public class VatNutritionCounter
{
    static VatNutritionCounter()
    {
        NutritionCounter.NutritionFinders["FoodAlert.Extensions.VENutrientPaste"] = CountNutrition;
    }

    public static float CountNutrition(Map map)
    {
        var mealDef = ThingDefOf.MealNutrientPaste;
        var mealPreferability = mealDef.ingestible.preferability;
        if (FoodAlertMod.settings.foodPreferability > mealPreferability)
        {
            return 0;
        }
        var mealCount = map?.GetComponent<PipeSystem.PipeNetManager>()
            ?.pipeNets
            ?.Where(pn => pn.def.defName == "VNPE_NutrientPasteNet")
            ?.Sum(pn => pn.CurrentStored())
            ?? 0;
        var nutPerMeal = mealDef.GetStatValueAbstract(StatDefOf.Nutrition);
        return mealCount * nutPerMeal;
    }
}
