using RimWorld;
using Verse;

namespace FoodAlert;

public class FoodAlertSettings : ModSettings
{
    public bool DynamicUpdate = true;
    public float EstimateIngredients = -1; // 0 = disabled, <0 auto, >0 = custom
    public FoodPreferability FoodPreferability = FoodPreferability.RawBad;
    public float UpdateFrequency = 400;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref FoodPreferability, "foodPreferability", FoodPreferability.RawBad, true);
        Scribe_Values.Look(ref DynamicUpdate, "dynamicupdate", true);
        Scribe_Values.Look(ref UpdateFrequency, "updatefrequency", 400);
        Scribe_Values.Look(ref EstimateIngredients, "estimateIngredients", -1);
    }
}