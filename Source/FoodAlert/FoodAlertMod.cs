using System;
using Mlie;
using RimWorld;
using UnityEngine;
using Verse;

namespace FoodAlert;

public class FoodAlertMod : Mod
{
    public static FoodAlertSettings Settings;

    private static string currentVersion;

    private static readonly FoodPreferability[] preferabilities =
    [
        FoodPreferability.DesperateOnly, FoodPreferability.RawBad, FoodPreferability.RawTasty,
        FoodPreferability.MealAwful, FoodPreferability.MealSimple, FoodPreferability.MealFine,
        FoodPreferability.MealLavish
    ];

    public FoodAlertMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<FoodAlertSettings>();
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public override string SettingsCategory()
    {
        return "Food Alert";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        foreach (var preferability in preferabilities)
        {
            var prefName = Enum.GetName(typeof(FoodPreferability), preferability);
            if (listingStandard.RadioButton(prefName, Settings.FoodPreferability == preferability))
            {
                Settings.FoodPreferability = preferability;
            }
        }

        listingStandard.Label("SettingExplanation".Translate());
        listingStandard.GapLine();
        if (Settings.FoodPreferability >= FoodPreferability.MealAwful)
        {
            listingStandard.Label("EstimateIngredients.label".Translate());
            if (listingStandard.RadioButton("EstimateIngredients.none".Translate(), Settings.EstimateIngredients == 0))
            {
                Settings.EstimateIngredients = 0;
            }

            if (listingStandard.RadioButton("EstimateIngredients.auto".Translate(), Settings.EstimateIngredients < 0))
            {
                Settings.EstimateIngredients = -1;
            }

            if (listingStandard.RadioButton("EstimateIngredients.custom".Translate(),
                    Settings.EstimateIngredients > 0))
            {
                if (Settings.EstimateIngredients <= 0)
                {
                    Settings.EstimateIngredients = 1;
                }
            }

            if (Settings.EstimateIngredients > 0)
            {
                Settings.EstimateIngredients = listingStandard.SliderLabeled(
                    "EstimateIngredients.slider".Translate(Math.Round(Settings.EstimateIngredients, 2).ToString()),
                    Settings.EstimateIngredients, 0.1f, 10f);
            }
        }

        listingStandard.GapLine();
        listingStandard.Label("FA.updatetype.label".Translate());
        listingStandard.CheckboxLabeled("FA.typedynamic.label".Translate(), ref Settings.DynamicUpdate,
            "FA.typedynamic.description".Translate());
        if (!Settings.DynamicUpdate)
        {
            Settings.UpdateFrequency = listingStandard.SliderLabeled(
                "FA.typestatic.slider".Translate(Math.Round((decimal)Settings.UpdateFrequency / 2500, 2).ToString()),
                Settings.UpdateFrequency, 100, 10000);
        }

        if (currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("FA.modversion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();

        Settings.Write();
    }
}