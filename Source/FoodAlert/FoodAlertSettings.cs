using RimWorld;
using Verse;

namespace FoodAlert;

public class FoodAlertSettings : ModSettings
{
    public bool dynamicupdate = true;
    public FoodPreferability foodPreferability = FoodPreferability.RawBad;
    public float estimateIngredients = -1; // 0 = disabled, <0 auto, >0 = custom
    public float updatefrequency = 400;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref foodPreferability, "foodPreferability", FoodPreferability.RawBad, true);
        Scribe_Values.Look(ref dynamicupdate, "dynamicupdate", true);
        Scribe_Values.Look(ref updatefrequency, "updatefrequency", 400);
        Scribe_Values.Look(ref estimateIngredients, "estimateIngredients", -1);
    }
}