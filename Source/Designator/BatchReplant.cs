using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace Replace_Stuff_Compatibility.Designator
{
	public class BatchReplant: Verse.Designator
	{
		public override int DraggableDimensions => 2;

		public override bool DragDrawMeasurements => true;
		
		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			var minifiedTree = DefDatabase<ThingDef>.GetNamed("Plant_TreePoplar");
			
			WorkGiver_Replant
			if (!c.InBounds(this.Map))
				return (AcceptanceReport) false;
			
			var acceptanceReport = minifiedTree.CanEverPlantAt(c, this.Map, out Thing _, true);
			if (!(bool) acceptanceReport)
				return new AcceptanceReport((string) ("CannotBePlantedHere".Translate() + ": " + acceptanceReport.Reason.CapitalizeFirst()));
			
			if (minifiedTree.plant.interferesWithRoof && c.Roofed(this.Map))
				return (AcceptanceReport) ("CannotBePlantedHere".Translate() + ": " + "BlockedByRoof".Translate().CapitalizeFirst());
			
			if (!minifiedTree.CanNowPlantAt(c, this.Map, true))
				return new AcceptanceReport((string) "CannotBePlantedHere".Translate());
			
			return GenConstruct.CanPlaceBlueprintAt(DefDatabase<ThingDef>.GetNamed("MinifiedTree"), c, Rot4.North, Map);
		}
		
		public BatchReplant()
		{
			this.defaultLabel = "Mass Replant";
			this.defaultDesc = "Designate an area to replant uprooted trees. Any extracted trees on this map will be automatically designated to be planted within this zone";
			this.useMouseIcon = true;
			this.icon = TexCommand.Replant;
		}
		
		public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
		{
			var trees = this.Map.spawnedThings.ToList().FindAll(thing => thing.def.defName == "MinifiedTree" && !Map.listerBuildings.TryGetReinstallBlueprint(thing.GetInnerIfMinified(), out _));
			bool somethingSucceeded = false;
			bool flag = false;
			foreach (IntVec3 cell in cells)
			{
				if (trees.Count == 0) break;

				if (!this.CanDesignateCell(cell).Accepted) continue;
				var tree = trees.First() as MinifiedTree;
				if (tree == null) break;
					
				trees.RemoveAt(0);

				GenConstruct.PlaceBlueprintForInstall(tree, cell, this.Map, tree.Rotation, Faction.OfPlayer);
					
				somethingSucceeded = true;
				if (!flag)
					flag = this.ShowWarningForCell(cell);
			}
			this.Finalize(somethingSucceeded);
		}

		public override void RenderHighlight(List<IntVec3> dragCells) => DesignatorUtility.RenderHighlightOverSelectableCells((Verse.Designator) this, dragCells);
	}
}