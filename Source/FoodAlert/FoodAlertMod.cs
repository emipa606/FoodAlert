using System;
using Mlie;
using SettingsHelper;
using UnityEngine;
using Verse;

namespace FoodAlert
{
    internal class FoodAlertMod : Mod
    {
        public static FoodAlertSettings settings;

        private static string currentVersion;

        private static readonly string[] preferabilities =
            { "DesperateOnly", "RawBad", "RawTasty", "MealAwful", "MealSimple", "MealFine", "MealLavish" };

        public FoodAlertMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<FoodAlertSettings>();
            currentVersion =
                VersionFromManifest.GetVersionFromModMetaData(ModLister.GetActiveModWithIdentifier("Mlie.FoodAlert"));
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
            listing_Standard.GapLine();
            listing_Standard.Label("FA.updatetype.label".Translate());
            listing_Standard.CheckboxLabeled("FA.typedynamic.label".Translate(), ref settings.dynamicupdate,
                "FA.typedynamic.description".Translate());
            if (!settings.dynamicupdate)
            {
                listing_Standard.AddLabeledSlider(
                    "FA.typestatic.slider".Translate(Math.Round((decimal)settings.updatefrequency / 2500, 2)),
                    ref settings.updatefrequency, 100, 10000);
            }

            if (currentVersion != null)
            {
                listing_Standard.Gap();
                GUI.contentColor = Color.gray;
                listing_Standard.Label("FA.modversion".Translate(currentVersion));
                GUI.contentColor = Color.white;
            }

            listing_Standard.End();

            settings.Write();
        }
    }
}