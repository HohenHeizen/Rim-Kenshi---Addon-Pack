using System;
using Verse;
using RimWorld;
using Verse.AI;


namespace RimKenshiAddon
{   //define defs VEF:Vikings
    [DefOf]
    class RimKenshiAddon_DefOf
    {
#pragma warning disable 0649
        // raids
        public static FactionDef HiverFogmen;
        //mental break
        public static HediffDef FogSickness;

        public static MentalStateDef FogSick;
        //incident
        public static IncidentDef FogRaid;
    }

}
