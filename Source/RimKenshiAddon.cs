using System;
using RimWorld;
using Verse;
using Verse.AI;


namespace RimKenshiAddon
{
    [StaticConstructorOnStartup]
    public static class RimKenshiAddon
    {
        static RimKenshiAddon() //our constructor
        {
            Log.Message("Hello Kenshi!"); //Outputs "Hello Kenshi!" to the dev console.
        }
    }
	
}
