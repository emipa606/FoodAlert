using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FoodAlert
{
    class FoodAlertSettings : ModSettings
    {
        public string foodPreferability = "RawBad";

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref foodPreferability, "foodPreferability", defaultValue: "RawBad", forceSave: true);
        }
    }
}
