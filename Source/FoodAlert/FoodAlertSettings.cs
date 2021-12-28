using Verse;

namespace FoodAlert;

internal class FoodAlertSettings : ModSettings
{
    public bool dynamicupdate = true;
    public string foodPreferability = "RawBad";
    public float updatefrequency = 400;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref foodPreferability, "foodPreferability", "RawBad", true);
        Scribe_Values.Look(ref dynamicupdate, "dynamicupdate", true);
        Scribe_Values.Look(ref updatefrequency, "updatefrequency", 400);
    }
}