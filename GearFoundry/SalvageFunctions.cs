/*
 * Created by SharpDevelop.
 * User: Paul
 * Date: 1/26/2013
 * Time: 12:59 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Decal.Adapter;
using Decal.Filters;
using Decal.Adapter.Wrappers;
using System.Linq;
using System.Collections.Generic;

namespace GearFoundry
{
	/// <summary>
	/// Description of SalvageFunctions.
	/// </summary>
	public partial class PluginCore
	{

//[VI] [Vigeneral] Irquk says, "Got a guy here picking up a bounce house we had for my 4 year old's bday party.  I may go silent suddently but will return"
//[VI] [Vigeneral] Virindi says, "I want a bouncy house for my birthday."
//[VI] [Vigeneral] Irquk says, "If you use the loot addon with Vtank it works as a passive ID addon and gives all the work to Vtank."
//[VI] [Vigeneral] Irquk says, "If you use the manual portions, only items from containers which are added to your inventory get ID'd because of using a listen list to check against."
//[VI] [Vigeneral] Virindi says, "ah...I noticed it doesn't use the combine decision interfaces"
//[VI] [Vigeneral] Irquk says, "How do I implement?"
//[VI] [Vigeneral] Irquk says, "I'm still learning all the ins and outs."
//[VI] [Vigeneral] Irquk says, "For now, it lets Vtank do it's thing.  LOL"
//[VI] [Vigeneral] Virindi says, "when I implemented value salvaging I added ILootPluginCapability_SalvageCombineDecision2"
//[VI] [Vigeneral] Virindi says, "your lootplugin object, just implements either ILootPluginCapability_SalvageCombineDecision or ILootPluginCapability_SalvageCombineDecision2 interface"
//[VI] [Vigeneral] Irquk says, "I put in a button in another panel that will later do combining with code I wrote if you want to clean up inventory."
//[VI] [Vigeneral] Virindi says, "if it implements either, vtank will query it to determine how to combine salvage."
//[VI] [Vigeneral] Virindi says, "decision1 passes in pairs of bags and you return yes or no to combine"
//[VI] [Vigeneral] Virindi says, "decision2 passes you a list of bags of the same material, and you return a list of what you want combined in one operation."
//[VI] [Vigeneral] Virindi says, "vtclassic has an example of how to use them"

//[VI] [Vigeneral] Virindi says, "http://www.virindi.net/repos/virindi_public/trunk/VirindiTankLootPlugins/"

//[VI] [Vigeneral] Virindi says, "the lootpluginbase implementation is in http://www.virindi.net/repos/virindi_public/trunk/VirindiTankLootPlugins/VTClassic/VTClassic/LootCore.cs"
	
	    private List<SalvageRule> SalvageRulesList = new List<SalvageRule>();
        private List<WorldObject> InventorySalvage = new List<WorldObject>();
		private Queue<LootObject> SalvageObjectQueue = new Queue<LootObject>();  
			
		string[] splitstring;
		string[] splstr;
		private void FillSalvageRules()
		{
			try
			{
				SalvageRulesList.Clear();
				foreach(var XSalv in mSortedSalvageListChecked)
				{
					
					splitstring = XSalv.Element("combine").Value.Split(',');
					
					if(splitstring.Count() == 1)
					{
						SalvageRule sr = new SalvageRule();
						Int32.TryParse(XSalv.Element("intvalue").Value, out sr.material);
						
						if(splitstring[0].Contains("-"))
						{
							splstr = splitstring[0].Split('-');
							   	bool success0 = Double.TryParse(splstr[0], out sr.minwork);
							   	bool success1 = Double.TryParse(splstr[1], out sr.maxwork);
							   	sr.ruleid = MaterialIndex[sr.material].name + " " + sr.minwork.ToString("N") + "-" + sr.maxwork.ToString("N");
							   	if(success0 && success1) {SalvageRulesList.Add(sr);}
						}
						else
						{
							bool success0 = Double.TryParse(splitstring[0], out sr.minwork);
							sr.maxwork = 10;
							sr.ruleid = MaterialIndex[sr.material].name + " " + sr.minwork.ToString("N") + "-" + sr.maxwork.ToString("N");
							if(success0) {SalvageRulesList.Add(sr);}
						}
					}
					else
					{
						foreach(string salvstring in splitstring)
						{
							SalvageRule sr = new SalvageRule();					
							Int32.TryParse(XSalv.Element("intvalue").Value, out sr.material);
							
							if(salvstring.Contains("-"))
							{
							   	string[] splstr = salvstring.Split('-');
							   	bool success0 = Double.TryParse(splstr[0], out sr.minwork);
							   	bool success1 = Double.TryParse(splstr[1], out sr.maxwork);
							   	sr.ruleid = MaterialIndex[sr.material].name + " " + sr.minwork.ToString("N") + "-" + sr.maxwork.ToString("N");
							   	if(success0 && success1) {SalvageRulesList.Add(sr);}
							}
							else
							{
								bool success = Double.TryParse(salvstring, out sr.minwork);
								sr.maxwork = sr.minwork;
								sr.ruleid = MaterialIndex[sr.material].name + " " + sr.minwork.ToString("N") + "-" + sr.maxwork.ToString("N");
								if(success) {SalvageRulesList.Add(sr);}
							}
							
							
						}
					}
				}
			} catch{}
		}
		
		public class SalvageRule
		{
			public string ruleid;
			public int material;
			public double minwork;
			public double maxwork;
		}

		private void ScanInventoryForSalvageBags()
		{
			try
			{
				InventorySalvage.Clear();
		 		InventorySalvage = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower().Contains("salvage")).OrderBy(x => x.Values(LongValueKey.Material)).ToList();
			}catch{}
		}
		
		private void SalvageItems()
		{
			try
			{
				for(int i = 0; i < SalvageObjectQueue.Count; i ++)
				{	
					LootObject io_crush = SalvageObjectQueue.Dequeue();
					Host.Actions.SalvagePanelAdd(io_crush.Id);
					host.Actions.SalvagePanelSalvage();
					CombineSalvageBags();	
				}
			}
			catch{}	
		}
		
		private class PartialBags
		{
			public int SalvBagID;
			public int SalvBagUses;
			public int SalvBagMat;
			public double SalvBagWork;
			public string ruleid;
		}
		
		//Irq:  Confirmed Functional...
		List<PartialBags> CombineSalvageBagsList = new List<PartialBags>();
		private void CombineSalvageBags()
		{
			//WriteToChat("CombineSalvageBagsFires");
			try
			{
				//No sense trying to salvage if you can't....
				List<WorldObject> UstSearch = Core.WorldFilter.GetInventory().ToList();				
				WorldObject MyUst = UstSearch[UstSearch.FindIndex(x => x.Name == "Ust")];
				
				if (MyUst == null)
				{
					WriteToChat("No Ust Found.");
					return;
				} 

				//refresh the list of salvagebags in inventory
				ScanInventoryForSalvageBags();

				//Create a list of partial bags of salvage from inventory
				var partbagslinq = from bags in InventorySalvage
								  where bags.Values(LongValueKey.UsesRemaining) < 100
								  select new PartialBags{ SalvBagID = bags.Id, SalvBagUses = bags.Values(LongValueKey.UsesRemaining), 
					              SalvBagWork = bags.Values(DoubleValueKey.SalvageWorkmanship), SalvBagMat = bags.Values(LongValueKey.Material)};
				
				PartialBags[] partbags = partbagslinq.ToArray();
				
				
				//Build combine rules for all partial bags in inventory
				foreach(PartialBags pb in partbags)
				{
					var materialrules = from allrules in SalvageRulesList
						where (allrules.material == pb.SalvBagMat) && (pb.SalvBagWork >= allrules.minwork) && (pb.SalvBagWork <= (allrules.maxwork +0.99))
								select allrules;					
					
					if(materialrules.Count() > 0)
					{
						SalvageRule sr = materialrules.First();
						pb.ruleid = sr.ruleid;
					}
				}
				

				//cycle through rules and combine salvage accordingly
				foreach(SalvageRule sr in SalvageRulesList)
				{
					var partbaggroups = from bags in partbags
										where bags.ruleid == sr.ruleid
										select bags;
					
					int salvagesum = 0;
					foreach(PartialBags pb in partbaggroups)
					{
						if(salvagesum < 100)
						{
							if(salvagesum + pb.SalvBagUses < 110)
							{
								salvagesum += pb.SalvBagUses;
								CombineSalvageBagsList.Add(pb);
							}
						}			
						if(salvagesum > 100)
						{
							Host.Actions.UseItem(MyUst.Id, 0);
							foreach(PartialBags cb in CombineSalvageBagsList)
							{
								host.Actions.SalvagePanelAdd(cb.SalvBagID);
							}
							host.Actions.SalvagePanelSalvage();
							CombineSalvageBagsList.Clear();
							salvagesum = 0;
						}
								
					}
					CombineSalvageBagsList.Clear();
				}		
			}
			catch{}
			
		}

		//Irquk:  TODO: Feature
		private void AutoCrushAetheria()
		{
			
		}
		

	}
}
