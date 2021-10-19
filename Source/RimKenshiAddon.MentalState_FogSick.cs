using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimKenshiAddon
{
    public class MentalState_FogSick : MentalState
    {
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Pawn>(ref this.target, "target", false);
		}
		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
		public override void PreStart()
		{
			base.PreStart();
			this.TryFindNewTarget();
		}
		public override void PostStart(string reason)
		{
			base.PostStart(reason);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.AnimalsDontAttackDoors, OpportunityType.Critical);
		}

		public override bool ForceHostileTo(Thing t)
		{
			Pawn pawn;
			return ((pawn = (t as Pawn)) == null || !pawn.RaceProps.Roamer || pawn.health.Downed) && t.Faction != null && this.ForceHostileTo(t.Faction);
            //attack downed pawns
   
        }

		public override bool ForceHostileTo(Faction f)
		{
			return f.def.humanlikeFaction;
		}

		private bool TryFindNewTarget()
		{
			this.target = MurderousRageMentalStateUtility.FindPawnToKill(this.pawn);
			return this.target != null;
		}

		public bool IsTargetStillValidAndReachable()
		{
			return this.target != null && this.target.SpawnedParentOrMe != null && (!(this.target.SpawnedParentOrMe is Pawn) || this.target.SpawnedParentOrMe == this.target) && this.pawn.CanReach(this.target.SpawnedParentOrMe, PathEndMode.Touch, Danger.Deadly, true, false, TraverseMode.ByPawn);
		}

		public Pawn target;

		private const int NoLongerValidTargetCheckInterval = 120;
	}
        
}