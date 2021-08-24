using SettingsHelper;
using UnityEngine;
using Verse;

namespace FoodAlert
{
    internal class FoodAlertMod : Mod
    {
        private static FoodAlertSettings settings;

        private static readonly string[] preferabilities =
            { "DesperateOnly", "RawBad", "RawTasty", "MealAwful", "MealSimple", "MealFine", "MealLavish" };

        public FoodAlertMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<FoodAlertSettings>();
        }

        public override string SettingsCategory()
        {
            return "Food Alert";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.AddLabeledRadioList("SettingDescription".Translate(), preferabilities,
                ref settings.foodPreferability);
            listing_Standard.Label("SettingExplanation".Translate());
            listing_Standard.End();

            settings.Write();
        }
    }
}