using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimKenshiAddon

{
    public class IncidentWorker_FogRaid : IncidentWorker_RaidEnemy
    { 
        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            parms.faction = Find.FactionManager.FirstFactionOfDef(RimKenshiAddon_DefOf.HiverFogmen);
            return true;
        }

        protected override void ResolveRaidPoints(IncidentParms parms)
        {
            if (parms.points <= 0f)
            {
                Log.Error("RaidEnemy is resolving raid points. They should always be set before initiating the incident.");
                parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target);
            }
        }

        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
        }

        public override void ResolveRaidArriveMode(IncidentParms parms)
        {
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
        }

        public List<Pawn> SpawnThreats(IncidentParms parms)
        {
            if (parms.pawnKind != null)
            {
                List<Pawn> list = new List<Pawn>();
                for (int i = 0; i < parms.pawnCount; i++)
                {
                    PawnGenerationRequest request = new PawnGenerationRequest(parms.pawnKind, parms.faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: false, mustBeCapableOfViolence: true, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: false);
                    Pawn pawn = PawnGenerator.GeneratePawn(request);
                    pawn.health.AddHediff(RimKenshiAddon_DefOf.FogSickness, null, null, null);
                    pawn.mindState.mentalStateHandler.TryStartMentalState(RimKenshiAddon_DefOf.FogSick, null, false, false, null, false, false, false);
                    pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Rand.Range(60000, 80000);
                    if (pawn != null)
                    {
                        list.Add(pawn);
                    }
                }
            }
            return null;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            ResolveRaidPoints(parms);
            if (!TryResolveRaidFaction(parms))
            {
                return false;
            }
            if (!parms.faction.HostileTo(Faction.OfPlayer))
            {
                return false;
            }
            PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;
            ResolveRaidStrategy(parms, combat);
            ResolveRaidArriveMode(parms);
            parms.raidStrategy.Worker.TryGenerateThreats(parms);
            if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
            {
                return false;
            }
            float points = parms.points;
            parms.points = AdjustedRaidPoints(parms.points, parms.raidArrivalMode, parms.raidStrategy, parms.faction, combat);
            List<Pawn> list = SpawnThreats(parms);
            if (list == null)
            {
                list = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms)).ToList();
                if (list.Count == 0)
                {
                    Log.Error("Got no pawns spawning raid from parms " + parms);
                    return false;
                }
            }

          
            parms.raidArrivalMode.Worker.Arrive(list, parms);
            GenerateRaidLoot(parms, points, list);
            TaggedString letterLabel = GetLetterLabel(parms);
            TaggedString letterText = GetLetterText(parms, list);
            List<TargetInfo> list2 = new List<TargetInfo>();
            if (parms.pawnGroups != null)
            {
                List<List<Pawn>> list3 = IncidentParmsUtility.SplitIntoGroups(list, parms.pawnGroups);
                List<Pawn> list4 = list3.MaxBy((List<Pawn> x) => x.Count);
                if (list4.Any())
                {
                    list2.Add(list4[0]);
                }
                for (int i = 0; i < list3.Count; i++)
                {
                    if (list3[i] != list4 && list3[i].Any())
                    {
                        list2.Add(list3[i][0]);
                    }
                }
            }
            else if (list.Any())
            {
                foreach (Pawn item in list)
                {
                    list2.Add(item);
                }
            }


            SendStandardLetter(letterLabel, letterText, GetLetterDef(), parms, list2);
            var lord = new LordJob_AssaultColony(parms.faction, true, true, false, false, false, false, false);
            LordMaker.MakeNewLord(parms.faction, lord, (Map)parms.target, list);
            foreach (var pawn in list)
            {
                if (pawn.RaceProps.Humanlike)
                {
                    pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("RimKenshi_HHCaptureDowned"));
               }
                else
                {
                    pawn.mindState.duty = new PawnDuty(DutyDefOf.WanderClose);
                }

            }
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Important);

            return true;
        }
           //lesson insert
        protected override string GetLetterLabel(IncidentParms parms)
        {
            return base.def.letterLabel;
        }

        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            return "RimKenshi.FogRaidDesc".Translate(parms.faction);
        }

        protected override LetterDef GetLetterDef()
        {
            return LetterDefOf.ThreatBig;
        }

        protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
        {
            return TranslatorFormattedStringExtensions.Translate("LetterRelatedPawnsRaidEnemy", Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
        }
    }
}

