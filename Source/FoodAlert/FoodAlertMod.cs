using System;
using Mlie;
using RimWorld;
using UnityEngine;
using Verse;

namespace FoodAlert;

public class FoodAlertMod : Mod
{
    public static FoodAlertSettings settings;

    private static string currentVersion;

    private static readonly FoodPreferability[] preferabilities =
    [
        FoodPreferability.DesperateOnly, FoodPreferability.RawBad, FoodPreferability.RawTasty,
        FoodPreferability.MealAwful, FoodPreferability.MealSimple, FoodPreferability.MealFine,
        FoodPreferability.MealLavish
    ];

    public FoodAlertMod(ModContentPack content) : base(content)
    {
        settings = GetSettings<FoodAlertSettings>();
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public override string SettingsCategory()
    {
        return "Food Alert";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(inRect);
        foreach (var preferability in preferabilities)
        {
            var prefName = Enum.GetName(typeof(FoodPreferability), preferability);
            if (listing_Standard.RadioButton(prefName, settings.foodPreferability == preferability))
            {
                settings.foodPreferability = preferability;
            }
        }

        listing_Standard.Label("SettingExplanation".Translate());
        listing_Standard.GapLine();
        if (settings.foodPreferability >= FoodPreferability.MealAwful)
        {
            listing_Standard.Label("EstimateIngredients.label".Translate());
            if (listing_Standard.RadioButton("EstimateIngredients.none".Translate(), settings.estimateIngredients == 0))
            {
                settings.estimateIngredients = 0;
            }

            if (listing_Standard.RadioButton("EstimateIngredients.auto".Translate(), settings.estimateIngredients < 0))
            {
                settings.estimateIngredients = -1;
            }

            if (listing_Standard.RadioButton("EstimateIngredients.custom".Translate(),
                    settings.estimateIngredients > 0))
            {
                if (settings.estimateIngredients <= 0)
                {
                    settings.estimateIngredients = 1;
                }
            }

            if (settings.estimateIngredients > 0)
            {
                settings.estimateIngredients = listing_Standard.SliderLabeled(
                    "EstimateIngredients.slider".Translate(Math.Round(settings.estimateIngredients, 2).ToString()),
                    settings.estimateIngredients, 0.1f, 10f);
            }
        }

        listing_Standard.GapLine();
        listing_Standard.Label("FA.updatetype.label".Translate());
        listing_Standard.CheckboxLabeled("FA.typedynamic.label".Translate(), ref settings.dynamicupdate,
            "FA.typedynamic.description".Translate());
        if (!settings.dynamicupdate)
        {
            settings.updatefrequency = listing_Standard.SliderLabeled(
                "FA.typestatic.slider".Translate(Math.Round((decimal)settings.updatefrequency / 2500, 2).ToString()),
                settings.updatefrequency, 100, 10000);
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