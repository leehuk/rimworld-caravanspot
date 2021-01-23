using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;

namespace CaravanJourneySpot
{
    [StaticConstructorOnStartup]
    static class CSHarmonyPatches
    {
        static CSHarmonyPatches()
        {
            var harmony = new Harmony("CaravanSpot");
            MethodInfo rwmethod = AccessTools.Method(typeof(Dialog_FormCaravan), "TryFindExitSpot", new[] { typeof(List<Pawn>), typeof(bool), typeof(IntVec3).MakeByRefType() });
            MethodInfo ptmethod = typeof(CSHarmonyPatches).GetMethod("TryFindExitSpotPatch");

            if(rwmethod != null && ptmethod != null)
            {
                var hrmethod = new HarmonyMethod(ptmethod);
                harmony.Patch(rwmethod, null, hrmethod);
                Log.Message("CaravanJourneySpot: Patched TryFindExitSpot()");
            }
            else
            {
                Log.Error("CaravanSpot: Unable to patch TryFindExitSpot()");
            }
        }

        public static void TryFindExitSpotPatch(Dialog_FormCaravan __instance, ref List<Pawn> pawns, ref bool reachableForEveryColonist, ref IntVec3 spot)
        {
            if (Current.Game.CurrentMap != null && reachableForEveryColonist == true)
            {
                foreach (Building building in Current.Game.CurrentMap.listerBuildings.allBuildingsColonist)
                {
                    if (building.def.defName.Equals("CaravanJourneySpot"))
                    {
                        for(int i = 0; i < pawns.Count; i++)
                        {
                            if (pawns[i].IsColonist && !pawns[i].Downed && !pawns[i].CanReach(building.Position, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
                            {
                                return;
                            }
                        }

                        spot = building.Position;
                    }
                }
            }
        }

    }

    public class CaravanJourneySpot : Building
    {
        public CaravanJourneySpot()
        {
            if (Current.Game.CurrentMap != null)
            {
                foreach (Building building in Current.Game.CurrentMap.listerBuildings.allBuildingsColonist)
                {
                    if (building.def.defName.Equals("CaravanJourneySpot"))
                    {
                        building.Destroy(DestroyMode.Vanish);
                        Messages.Message("CaravanJourneySpot.AlreadyOnMap".Translate(), MessageTypeDefOf.NegativeEvent);
                        break;
                    }
                }
            }
        }
    }
}
