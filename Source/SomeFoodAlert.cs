using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace FoodAlert
{
    public class Alert_SomeFood : Alert
    {
        public Alert_SomeFood()
        {
            defaultLabel = "SomeFood".Translate();
            defaultPriority = AlertPriority.Medium;
        }

        public override TaggedString GetExplanation()
        {
            var map = MapWithSomeFood();

            if (map == null)
            {
                return string.Empty;
            }

            var totalHumanEdibleNutrition = map.resourceCounter.TotalHumanEdibleNutrition;
            var num = map.mapPawns.FreeColonistsSpawnedCount + (from pr in map.mapPawns.PrisonersOfColony
                where pr.guest.CanBeBroughtFood
                select pr).Count();
            var num2 = Mathf.FloorToInt(totalHumanEdibleNutrition / num);
            return string.Format("SomeFoodDesc".Translate(), totalHumanEdibleNutrition.ToString("F0"),
                num.ToStringCached(), num2.ToStringCached());
        }

        public override AlertReport GetReport()
        {
            //if (Find.TickManager.TicksGame < 150000)
            //{
            //    return false;
            //}

            return MapWithSomeFood() != null;
        }

        private Map MapWithSomeFood()
        {
            if (HarmonyPatches.IsSosLoaded)
            {
                return Find.CurrentMap;
            }

            var maps = Find.Maps;
            foreach (var map in maps)
            {
                if (!map.IsPlayerHome)
                {
                    continue;
                }

                var freeColonistsSpawnedCount = map.mapPawns.FreeColonistsSpawnedCount;
                if (map.resourceCounter.TotalHumanEdibleNutrition > 4f * freeColonistsSpawnedCount)
                {
                    return map;
                }
            }

            return null;
        }
    }
}