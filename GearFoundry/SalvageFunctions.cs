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
	
	    private List<SalvageRule> SalvageRulesList = new List<SalvageRule>();
        private List<WorldObject> InventorySalvage = new List<WorldObject>();
		private Queue<IdentifiedObject> SalvageObjectQueue = new Queue<IdentifiedObject>();  
		private bool TradeOpen = false;

		void SubscribeSalvageEvents()
		{
			Core.WorldFilter.EnterTrade += new EventHandler<EnterTradeEventArgs>(OnTradeOpened);
			Core.WorldFilter.EndTrade += new EventHandler<EndTradeEventArgs>(OnTradeClosed);
		}
		
		void UnsubscribeSalvageEvents()
		{
			Core.WorldFilter.EnterTrade -= new EventHandler<EnterTradeEventArgs>(OnTradeOpened);
			Core.WorldFilter.EndTrade -= new EventHandler<EndTradeEventArgs>(OnTradeClosed);
		}
		
		void OnTradeOpened(object sender, EnterTradeEventArgs e)
		{
			TradeOpen = true;
		}
		
		void OnTradeClosed(object sener, EndTradeEventArgs e)
		{
			TradeOpen = false;
		}
			
		private void FillSalvageRules()
		{
			try
			{
				SalvageRulesList.Clear();
				for(int i = 0; i < mSortedSalvageListChecked.Count(); i++)
				{
					string[] splitstring = mSortedSalvageListChecked[i].Element("combine").Value.Split(',');
					foreach(string salstr in splitstring)
					{
						SalvageRule sr = new SalvageRule();
						
						if(salstr.Contains("-"))
						{
						   	string[] splstr = salstr.Split('-');
						   	bool success0 = Double.TryParse(splstr[0], out sr.minwork);
						   	bool success1 = Double.TryParse(splstr[1], out sr.maxwork);
						   	bool success2 = Int32.TryParse(mSortedSalvageListChecked[i].Element("intvalue").Value, out sr.material);
						   	sr.ruleid = sr.material.ToString("00") + sr.minwork.ToString("00") + sr.maxwork.ToString("00");
						   	if(success0 && success1 & success2) {SalvageRulesList.Add(sr);}
						}
						else
						{
							bool success = Double.TryParse(salstr, out sr.minwork);
							sr.maxwork = sr.minwork;
							bool success1 = Int32.TryParse(mSortedSalvageListChecked[i].Element("intvalue").Value, out sr.material);
							sr.ruleid = sr.material.ToString("00") + sr.minwork.ToString("00") + sr.maxwork.ToString("00");
							if(success && success1) {SalvageRulesList.Add(sr);}
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
				WorldObjectCollection AllInventory = Core.WorldFilter.GetInventory();
		 		var SalvageBags = from inventory in AllInventory
								where inventory.Name.Contains("Salvage")
								orderby inventory.Values(LongValueKey.Material)
								select inventory; 
		 		
		 		InventorySalvage = SalvageBags.ToList();		
			}catch{}
		}
		
		
		
		
		
		private void SalvageItems()
		{
			try
			{
				for(int i = 0; i < SalvageObjectQueue.Count; i ++)
				{	
					IdentifiedObject io_crush = SalvageObjectQueue.Dequeue();
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
