using RimWorld;
using Verse;

namespace FoodAlert;

public class FoodAlertSettings : ModSettings
{
    public bool dynamicupdate = true;
    public FoodPreferability foodPreferability = FoodPreferability.RawBad;
    public float updatefrequency = 400;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref foodPreferability, "foodPreferability", FoodPreferability.RawBad, true);
        Scribe_Values.Look(ref dynamicupdate, "dynamicupdate", true);
        Scribe_Values.Look(ref updatefrequency, "updatefrequency", 400);
    }
}