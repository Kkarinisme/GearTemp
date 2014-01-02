
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
using System.Xml.Serialization;
using System.Xml;

namespace GearFoundry
{

	public partial class PluginCore
	{

		
		
		private Queue<WorldObject> MaidCannibalizeQueue = new Queue<WorldObject>();
		private List<int> MaidCannibalizeProcessList = new List<int>();
		private List<WorldObject> MaidKeyRings = new List<WorldObject>();
		private List<WorldObject> MaidSalvageList = new List<WorldObject>();
		private List<WorldObject> MaidStackList = new List<WorldObject>();
		private List<WorldObject> MaidKeyList = new List<WorldObject>();
		private Queue<WorldObject> MaidCombineQueue = new Queue<WorldObject>();
		private System.Windows.Forms.Timer MaidTimer = new System.Windows.Forms.Timer();
		private List<WorldObject> MaidCompsList = new List<WorldObject>();
		
		private bool maidworking = false;
		
		private void RenderButlerHudMaidLayout()
    	{
    		try
    		{	
    			
    			    			
    		}catch(Exception ex){LogError(ex);}
    	}
    			
    	private void MaidCannibalizeEnable_Hit(object sender, EventArgs e)
    	{
    		try
    		{
    			WriteToChat("This will eat your stuff.  You were warned.");
    			
    			if(MaidCannibalizeInventory == null)
    			{
    			
    				MaidCannibalizeInventory = new HudButton();
    				MaidCannibalizeInventory.Text = "Cannibalize Inventory";
    				MaidTabLayout.AddControl(MaidCannibalizeInventory, new Rectangle(0,240,150,20));
    				MaidCannibalizeInventory.Hit += MaidCannibalizeInventory_Hit;
    			}
    			else
    			{
    				  if(MaidCannibalizeInventory != null) {MaidCannibalizeInventory.Hit -= MaidCannibalizeInventory_Hit;}
    				  if(MaidCannibalizeInventory != null){MaidCannibalizeInventory.Dispose(); MaidCannibalizeInventory = null;}	
    			}

    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void MaidCannibalizeInventory_Hit(object sender, EventArgs e)
    	{
    		try
    		{
    			MaidCannibalizeQueue.Clear();
    			
    			List<WorldObject> CannibalizeList = Core.WorldFilter.GetInventory().Where(x => x.LongKeys.Contains((int)LongValueKey.Material) &&
    			                                                                       !x.LongKeys.Contains((int)LongValueKey.Imbued) &&
    			                                                                       x.ObjectClass != ObjectClass.Salvage).ToList();
    			
    			foreach(var item in CannibalizeList)
    			{
    				MaidCannibalizeQueue.Enqueue(item);
    			}
    			WriteToChat("Cannibalize has queued " + MaidCannibalizeQueue.Count + " objects for potential annhilation.");
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
	
		private void MaidStackInventory_Hit(object sender, System.EventArgs e)
		{
			try
			{
				if(maidworking)
				{
					WriteToChat("Maid is currently processing another request.  Please wait for completion.");
				}
				else
				{
					maidworking = true;
					WriteToChat("Maid stacking started.");
					MaidTimer.Interval = 333;
					MaidTimer.Start();
					FillMaidStackList();	
					MaidTimer.Tick += MaidTimerStack;
				}					 
			}catch(Exception ex){LogError(ex);}
		}
		
		private void FillMaidStackList()
		{
			try
			{
				MaidStackList.Clear();
				
				MaidStackList = (from allitems in Core.WorldFilter.GetInventory()
					where allitems.Values(LongValueKey.StackMax) > 0 && (allitems.Values(LongValueKey.StackCount) < allitems.Values(LongValueKey.StackMax)) &&
					Core.WorldFilter.GetInventory().Where(x => x.Name == allitems.Name && x.Type == allitems.Type && x.Values(LongValueKey.StackCount) < x.Values(LongValueKey.StackMax)).Count() > 1
					select allitems).ToList();
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void MaidTimerStack(object sender, EventArgs e)
		{
			try
			{
				if(MaidStackList.Count > 0) 
				{
					MaidProcessStack();
					return;
				}
				else
				{
					WriteToChat("Stacking Completed.");
					MaidTimer.Stop();
					MaidTimer.Tick -= MaidTimerStack;
					maidworking = false;
				}
			}catch(Exception ex){LogError(ex);}
		}
				
		private void MaidProcessStack()
		{
			try
			{
				List<WorldObject> stacklist = MaidStackList.FindAll(x => x.Name == MaidStackList.First().Name && x.Type == MaidStackList.First().Type);
				if(stacklist.Count == 1 || stacklist.Count == 0)
				{
					FillMaidStackList();
					return;
				}
				else
				{
					Core.Actions.MoveItem(stacklist[1].Id, stacklist[0].Container, stacklist[0].Values(LongValueKey.Slot), true);
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		
		
		private int MaidMatchKey(string keyname)
		{
			try
			{
					WorldObject matchedkeyring = null;
					switch(keyname.ToLower())
					{
						case "legendary key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("burning sands"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "black marrow key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("black marrow"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "directive key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("directive"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "granite key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("granite"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "mana forge key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("black coral"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "master key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("master"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "marble key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("marble"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "singularity key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("singularity"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "skeletal falatacot key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("skeletal falatacot"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "sturdy iron key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("sturdy iron"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "sturdy steel key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("sturdy steel"));
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						default:
							return 0;
					}		
			}catch(Exception ex)
			{
				LogError(ex);
				return 0;
			}
		}
		
		private void MaidProcessRingKeys()
		{
			try
			{
				if(MaidKeyList.Count() > 0)
				{
					MaidKeyToRing = MaidKeyList.First().Id;
					MatchedKeyRingId = MaidMatchKey(Core.WorldFilter[MaidKeyToRing].Name);
					
					if(MatchedKeyRingId != 0)
					{
						Core.Actions.SelectItem(MaidKeyToRing);
						Core.Actions.UseItem(MatchedKeyRingId,1);
						if(Core.WorldFilter[MatchedKeyRingId].Values(LongValueKey.KeysHeld) == 24 || Core.WorldFilter[MatchedKeyRingId].Values(LongValueKey.UsesRemaining) == 0)
						{
							MaidKeyRings.RemoveAll(x => x.Id == MatchedKeyRingId);
						}
						return;
					}
					else
					{
						MaidKeyList.RemoveAll(x => x.Name == Core.WorldFilter[MaidKeyToRing].Name);
						MaidProcessRingKeys();
					}
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void MaidRingKeys_Hit(object sender, System.EventArgs e)
		{
			try
			{
				string[] KeyringMatchingArray = {"burning sands", "black marrow", "directive", "granite", "black coral", "master", "marble", "singularity", "skeletal falatacot", "sturdy iron", "sturdy steel"};
							
				MaidKeyRings = (from keyrings in Core.WorldFilter.GetInventory()
					where keyrings.Name.ToLower().Contains("keyring") && keyrings.Values(LongValueKey.UsesRemaining) > 0 && keyrings.Values(LongValueKey.KeysHeld) < 24
					orderby keyrings.Values(LongValueKey.KeysHeld) descending
					select keyrings).ToList();
				
				MaidKeyList = (from items in Core.WorldFilter.GetInventory()
				    where items.ObjectClass == ObjectClass.Key && RingableKeysArray.Contains(items.Name.ToLower())
					select items).ToList();
				
				MaidProcessRingKeys();

			}catch(Exception ex){LogError(ex);}
		}
		
		private void MaidTradeAllEightComps_Hit(object sender, EventArgs e)
		{
			try
			{
				ScanInventoryForComps();
				if(bButlerTradeOpen)
				{
					TradeComps();
					return;
				}
				else
				{
					WriteToChat("Open a trade window.");
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void MaidTradeAllSalvage_Hit(object sender, System.EventArgs e)
		{
			try
			{
				MaidScanInventoryForSalvageBags();
				if(bButlerTradeOpen)
				{
					TradeSalvageBags(1);
					return;
				}
				else if(Core.Actions.VendorId != 0)
				{
					SellSalvageBags(1);
				}
				else
				{
					WriteToChat("Open a trade or vendor window.");
				}
			}catch(Exception ex){LogError(ex);}
		}
		private void MaidTradeFilledSalvage_Hit(object sender, System.EventArgs e)
		{
			try
			{
				MaidScanInventoryForSalvageBags();
				if(bButlerTradeOpen)
				{
					TradeSalvageBags(0);
					return;
				}
				else if(Core.Actions.VendorId != 0)
				{
					SellSalvageBags(0);
				}
				else
				{
					WriteToChat("Open a trade or vendor window.");
				}
			}catch(Exception ex){LogError(ex);}
		}
		private void MaidTradeParialSalvage_Hit(object sender, System.EventArgs e)
		{
			try
			{
				MaidScanInventoryForSalvageBags();
				if(bButlerTradeOpen)
				{
					TradeSalvageBags(2);
					return;
				}
				else if(Core.Actions.VendorId != 0)
				{
					SellSalvageBags(2);
				}
				else
				{
					WriteToChat("Open a trade or vendor window.");
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void MaidScanInventoryForSalvageBags()
		{
			try
			{
				MaidSalvageList = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Salvage).ToList();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void SellSalvageBags(int bagtype)
		{
			try
			{
				MaidScanInventoryForSalvageBags();
				
				List <WorldObject> tradelist;
				
				if(bagtype == 0)
				{
					tradelist = MaidSalvageList.Where(x => x.Values(LongValueKey.UsesRemaining) == 100).OrderBy(x => x.Name).ToList();
				}
				else if(bagtype == 1)
				{
					tradelist = MaidSalvageList.ToList();
				}
				else if(bagtype == 2)
				{
					tradelist = MaidSalvageList.Where(x => x.Values(LongValueKey.UsesRemaining) < 100).OrderBy(x => x.Name).ToList();
				}
				else
				{
					tradelist = new List<WorldObject>();
				}	
				foreach(WorldObject sb in tradelist)
				{
					Core.Actions.VendorAddSellList(sb.Id);
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void TradeSalvageBags(int bagtype)
		{
			try
			{
				
				MaidScanInventoryForSalvageBags();
				
				List<WorldObject> tradelist;
				
				if(bagtype == 0)
				{
					tradelist = MaidSalvageList.Where(x => x.Values(LongValueKey.UsesRemaining) == 100).OrderBy(x => x.Name).ToList();
				}
				else if(bagtype == 1)
				{
					tradelist = MaidSalvageList.ToList();
				}
				else if(bagtype == 2)
				{
					tradelist = MaidSalvageList.Where(x => x.Values(LongValueKey.UsesRemaining) < 100).OrderBy(x => x.Name).ToList();
				}
				else
				{
					tradelist = new List<WorldObject>();
				}
				
				foreach(WorldObject bags in tradelist)
				{
					Core.Actions.TradeAdd(bags.Id);
				}
				

			}catch(Exception ex){LogError(ex);}
		}
		
		private void TradeComps()
		{
			try
			{
				foreach(WorldObject comp in MaidCompsList)
				{
					Core.Actions.TradeAdd(comp.Id);
				}
				
			}catch(Exception ex){LogError(ex);}
		}
				
		private void ScanInventoryForComps()
		{
			try
			{
				MaidCompsList = (from items in Core.WorldFilter.GetInventory()
					where items.Name.ToLower().Contains("ink of") || items.Name.ToLower().Contains("inks of") ||
					items.Name.ToLower().Contains("glyph of") || items.Name.ToLower().Contains("glyphs of") ||
					items.Name.ToLower().Contains("quill of") || items.Name.ToLower().Contains("quills of") ||
					items.Name.ToLower().Contains("alacritous") || items.Name.ToLower().Contains("parabolic")
					select items).ToList();
					
			}catch(Exception ex){LogError(ex);}
		}
		
		private void MaidProcessCannibalize()
		{
			try
			{
				WriteToChat("Checking " + MaidCannibalizeQueue.First().Name + " for destruction.");
				IdqueueAdd(MaidCannibalizeQueue.First().Id);
				LOList.Add(new LootObject(MaidCannibalizeQueue.First()));
				MaidCannibalizeProcessList.Add(MaidCannibalizeQueue.Dequeue().Id);
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void EvaluateCannibalizeMatches(int id)
		{
			try
			{				
				LootObject IOItem = LOList.Find(x => x.Id == id);
				
				if(IOItem.IOR != IOResult.rule && !IOItem.BValue(BoolValueKey.Retained))
				{
					IOItem.IOR = IOResult.salvage;
					IOItem.ProcessList = true;
					IOItem.ProcessAction = IAction.Salvage;
					
					if(GISettings.AutoProcess)
					{
						IOItem.Process = true;
						ToggleInspectorActions(2);
						InitiateInspectorActionSequence();
					}
					UpdateItemHud();
				}	
				return;

			}catch(Exception ex){LogError(ex);}
		}
				
		private void MaidSalvageCombine_Hit(object sender, System.EventArgs e)
		{
			try
			{
				if(maidworking)
				{
					WriteToChat("Maid is currently processing another request.  Please wait for completion.");
				}
				else
				{
					maidworking = true;
					WriteToChat("Maid salvage combining started.");
					MaidUseUst();
					MaidTimer.Interval = 333;
					MaidTimer.Start();
					MaidTimer.Tick += MaidTimerCombine;
				}
			}catch(Exception ex){LogError(ex);}

		}
		

		
		
		private void MaidTimerCombine(object sender, EventArgs e)
		{
			try
			{
				if(MaidCombineQueue.Count > 0) 
				{
					MaidCombineAction();
					return;
				}
				else
				{
					WriteToChat("Combine Actions Completed.");
					MaidTimer.Stop();
					MaidTimer.Tick -= MaidTimerCombine;
					maidworking = false;
				}
			}catch(Exception ex){LogError(ex);}
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
		
		private void CombineSalvageBags()
		{
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
				

//				//cycle through rules and combine salvage accordingly
//				foreach(SalvageRule sr in SalvageRulesList)
//				{
//					var partbaggroups = from bags in partbags
//										where bags.ruleid == sr.ruleid
//										select bags;
//					
//					int salvagesum = 0;
//					foreach(PartialBags pb in partbaggroups)
//					{
//						if(salvagesum < 100)
//						{
//							if(salvagesum + pb.SalvBagUses < 110)
//							{
//								salvagesum += pb.SalvBagUses;
//								CombineSalvageBagsList.Add(pb);
//							}
//						}			
//						if(salvagesum > 100)
//						{
//							Host.Actions.UseItem(MyUst.Id, 0);
//							foreach(PartialBags cb in CombineSalvageBagsList)
//							{
//								host.Actions.SalvagePanelAdd(cb.SalvBagID);
//							}
//							host.Actions.SalvagePanelSalvage();
//							CombineSalvageBagsList.Clear();
//							salvagesum = 0;
//						}
//								
//					}
//					CombineSalvageBagsList.Clear();
//				}		
			}
			catch{}
			
		}	

		private void MaidCombineAction()
		{
			try
			{
				
				List<WorldObject> PartialSalvageList = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Salvage && x.Values(LongValueKey.UsesRemaining) < 100).ToList();
				for(int i = 0; i < PartialSalvageList.Count; i++)
				{								
					ScanInventoryForSalvageBags();
					
					//Find an applicable material rule.
					var materialrules = from allrules in SalvageRulesList
						where (allrules.material == PartialSalvageList[i].Values(LongValueKey.Material)) &&
							  (PartialSalvageList[i].Values(DoubleValueKey.SalvageWorkmanship) >= allrules.minwork) &&
							  (PartialSalvageList[i].Values(DoubleValueKey.SalvageWorkmanship) <= (allrules.maxwork +0.99))
							  select allrules;		
						
					if(materialrules.Count() > 0)
					{
						var sr = materialrules.First();
						
						PartialBags[] partbags = (from bags in InventorySalvage
									  where bags.Values(LongValueKey.UsesRemaining) < 100  &&
								      	bags.Values(LongValueKey.Material) == sr.material  &&
									  	bags.Values(DoubleValueKey.SalvageWorkmanship) >= sr.minwork &&
									 	 bags.Values(DoubleValueKey.SalvageWorkmanship) <= (sr.maxwork + 0.99)
									  select new PartialBags{ SalvBagID = bags.Id, SalvBagUses = bags.Values(LongValueKey.UsesRemaining), 
									 	 	SalvBagWork = bags.Values(DoubleValueKey.SalvageWorkmanship), SalvBagMat = bags.Values(LongValueKey.Material)}).ToArray();
						
						CombineSalvageWOList.Clear();
						
						int salvagesum = 0;
						salvagesum += PartialSalvageList[i].Values(LongValueKey.UsesRemaining);
						CombineSalvageWOList.Add(PartialSalvageList[i].Id);
					
						for(int j = 0; j < partbags.Count(); j++)
						{
							if(salvagesum < 100)
							{
								if(salvagesum + partbags[j].SalvBagUses < 125)
								{
									if(!CombineSalvageWOList.Contains(partbags[j].SalvBagID))
								    {
										salvagesum += partbags[j].SalvBagUses;
										CombineSalvageWOList.Add(partbags[j].SalvBagID);
									}
								}
							}		
						}
						if(CombineSalvageWOList.Count() > 1)
						{	
							
							foreach(int salvageid in CombineSalvageWOList)
							{
								Core.Actions.SalvagePanelAdd(salvageid);
							}
							Core.Actions.SalvagePanelSalvage();
							CombineSalvageWOList.Clear();
							return;
						}
					}
				}
				
				WriteToChat("Combine Actions Completed.");
				MaidTimer.Stop();
				MaidTimer.Tick -= MaidTimerCombine;
				maidworking = false;
				
				
			}catch(Exception ex){LogError(ex);}
		}		
	
		
		private void MaidUseUst()
		{
			try
			{				
				WorldObject ust = Core.WorldFilter.GetInventory().Where(x => x.Name == "Ust").First();
				
				if(ust == null)
				{
					ToggleInspectorActions(0);
					WriteToChat("Character Has no Ust. Actions disabled.");
				}
				
				Core.Actions.UseItem(ust.Id,0);					
				
			}catch(Exception ex){LogError(ex);}		
		}
	
	}
}
