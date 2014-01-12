using System;
using System.Drawing;
using System.IO;
using Decal.Adapter;
using Decal.Filters;
using Decal.Adapter.Wrappers;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using VirindiViewService;
using MyClasses.MetaViewWrappers;
using VirindiViewService.Controls;
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;
using VirindiViewService.Themes;
using System.Xml.Serialization;
using System.Xml;

namespace GearFoundry
{
	public partial class PluginCore
	{ 				
		private void SalvageCreated(object sender, CreateObjectEventArgs e)
		{
			try
			{
				if(!FoundryInventoryCheck(e.New.Id)) {return;}
				if(e.New.ObjectClass == ObjectClass.Salvage && e.New.Values(LongValueKey.UsesRemaining) <  100)
				{				
					FoundryLoadAction(FoundryActionTypes.SalvageCombine, e.New.Id);
				}
				if(e.New.Name.ToLower() == "pyreal sliver" || e.New.Name.ToLower() == "pyreal nugget")
				{
					if(GISettings.AutoProcess)  FoundryLoadAction(FoundryActionTypes.MoteCombine, e.New.Id);
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private List<int> InspectorPickSalvage(WorldObject SalvageBag)
		{
			try
			{		
				SalvageRule sr = new SalvageRule();
				//Find an applicable material rule.
				List<SalvageRule> materialrules = (from allrules in SalvageRulesList
					where (allrules.material == SalvageBag.Values(LongValueKey.Material)) &&
					       (SalvageBag.Values(DoubleValueKey.SalvageWorkmanship) >= allrules.minwork) && 
						   (SalvageBag.Values(DoubleValueKey.SalvageWorkmanship) <= (allrules.maxwork +0.99))
						   select allrules).ToList();
					
				if(materialrules.Count > 0){sr = materialrules.First();}
				else
				{
					sr.material = SalvageBag.Values(LongValueKey.Material);
					sr.minwork = 1;
					sr.maxwork = 10;
					sr.ruleid = "Default Rule";
				}
				
				List<WorldObject> PartialBags = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Salvage && x.Id != SalvageBag.Id &&
				                                x.Values(LongValueKey.Material) == sr.material && x.Values(LongValueKey.UsesRemaining) < 100  && 
												x.Values(DoubleValueKey.SalvageWorkmanship) >= sr.minwork && x.Values(DoubleValueKey.SalvageWorkmanship) <= (sr.maxwork + 0.99)
												).ToList();
							  					
				//Why work if you don't have to.
				if(PartialBags.Count == 0) {return new List<int>();}
					
				List<int> CombineBagsList = new List<int>();		
				CombineBagsList.Add(SalvageBag.Id);
				int salvagesum = SalvageBag.Values(LongValueKey.UsesRemaining);
				
				foreach(WorldObject salvbag in PartialBags)
				{
					if(salvagesum < 100)
					{
						if(salvagesum + salvbag.Values(LongValueKey.UsesRemaining) < 125)
						{
							CombineBagsList.Add(salvbag.Id);
							salvagesum += salvbag.Values(LongValueKey.UsesRemaining);
						}
					}	
					if(salvagesum >= 100) {break;}
				}
				
				return CombineBagsList;
			}catch(Exception ex){LogError(ex); return new List<int>();}
		}
	}
}




