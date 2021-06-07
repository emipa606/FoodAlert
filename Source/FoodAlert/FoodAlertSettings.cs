using Verse;

namespace FoodAlert
{
    internal class FoodAlertSettings : ModSettings
    {
        public string foodPreferability = "RawBad";

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref foodPreferability, "foodPreferability", "RawBad", true);
        }
    }
}