using RimWorld;
using SettingsHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FoodAlert
{
    class FoodAlertMod : Mod
    {
        public static FoodAlertSettings settings;

        public static string[] preferabilities = new string[] { "DesperateOnly", "RawBad", "RawTasty", "MealAwful", "MealSimple", "MealFine", "MealLavish" };

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
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.AddLabeledRadioList("SettingDescription".Translate(), preferabilities, ref settings.foodPreferability);
            listing_Standard.Label("SettingExplanation".Translate());
            listing_Standard.End();

            settings.Write();
        }
    }
}
