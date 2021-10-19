using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimKenshiAddon
{
	public class MentalStateWorker_FogSick : MentalStateWorker
	{
		public override bool StateCanOccur(Pawn pawn)
		{
			return base.StateCanOccur(pawn) && MurderousRageMentalStateUtility.FindPawnToKill(pawn) != null;
		}
	}
}
