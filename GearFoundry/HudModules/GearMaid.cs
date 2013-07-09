
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

		private HudFixedLayout MaidTabLayout = null;
		private HudButton MaidStackInventory = null;
		private HudButton MaidRingKeys = null;
		private HudButton MaidTradeAllSalvage = null;
		private HudButton MaidTradeParialSalvage = null;
		private HudButton MaidTradeFilledSalvage = null;
		private HudButton MaidSalvageCombine = null;
		private HudButton MaidCannibalizeInventory = null;
		private HudCheckBox MaidCannibalizeEnable = null;
		
		private Queue<WorldObject> MaidCannibalizeQueue = new Queue<WorldObject>();
		private List<int> MaidCannibalizeProcessList = new List<int>();
		
		private void RenderButlerHudMaidLayout()
    	{
    		try
    		{	
    			MaidStackInventory = new HudButton();
    			MaidStackInventory.Text = "Stack Inventory";
    			MaidTabLayout.AddControl(MaidStackInventory, new Rectangle(0,0,150,20));
    			
    			MaidRingKeys = new HudButton();
    			MaidRingKeys.Text = "Ring Keys";
    			MaidTabLayout.AddControl(MaidRingKeys, new Rectangle(0,30,150,20));
    			
    			MaidTradeAllSalvage = new HudButton();
    			MaidTradeAllSalvage.Text = "Window All Salvage";    			
    			MaidTabLayout.AddControl(MaidTradeAllSalvage, new Rectangle(0,60,150,20));
    			
    			MaidTradeFilledSalvage = new HudButton();
    			MaidTradeFilledSalvage.Text = "Window Filled Salvage";
    			MaidTabLayout.AddControl(MaidTradeFilledSalvage, new Rectangle(0,90,150,20));
    			
    			MaidTradeParialSalvage = new HudButton();
    			MaidTradeParialSalvage.Text = "Window Partial Salvage";
    			MaidTabLayout.AddControl(MaidTradeParialSalvage, new Rectangle(0,120,150,20));
    			
    			MaidSalvageCombine = new HudButton();
    			MaidSalvageCombine.Text = "Combine Salvage Bags";
    			MaidTabLayout.AddControl(MaidSalvageCombine, new Rectangle(0,150,150,20));
    			
    			MaidCannibalizeEnable = new HudCheckBox();
    			MaidCannibalizeEnable.Text = "Enable Cannibalize Button";
    			MaidTabLayout.AddControl(MaidCannibalizeEnable, new Rectangle(0,180,150,20));
    			
    			MaidStackInventory.Hit += MaidStackInventory_Hit;
    			MaidRingKeys.Hit += MaidRingKeys_Hit;
    			MaidTradeAllSalvage.Hit += MaidTradeAllSalvage_Hit;
    			MaidTradeFilledSalvage.Hit += MaidTradeFilledSalvage_Hit;
    			MaidTradeParialSalvage.Hit += MaidTradeParialSalvage_Hit;
    			MaidSalvageCombine.Hit += MaidSalvageCombine_Hit;
    			MaidCannibalizeEnable.Hit += MaidCannibalizeEnable_Hit;
    			
    			MaidTab = true;
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void DisposeButlerHudMaidLayout()
    	{
    		try
    		{
    			if(!MaidTab) { return;}
    			
    			MaidStackInventory.Hit -= MaidStackInventory_Hit;
    			MaidRingKeys.Hit -= MaidRingKeys_Hit;
    			MaidTradeAllSalvage.Hit -= MaidTradeAllSalvage_Hit;
    			MaidTradeFilledSalvage.Hit -= MaidTradeFilledSalvage_Hit;
    			MaidTradeParialSalvage.Hit -= MaidTradeParialSalvage_Hit;
    			MaidSalvageCombine.Hit -= MaidSalvageCombine_Hit;
    			MaidCannibalizeInventory.Hit -= MaidCannibalizeInventory_Hit;
    			
    			if(MaidCannibalizeEnable != null){MaidCannibalizeInventory.Dispose();}
    			    			
    			MaidCannibalizeEnable.Dispose();
    			MaidSalvageCombine.Dispose();
    			MaidTradeParialSalvage.Dispose();
    			MaidTradeFilledSalvage.Dispose();
    			MaidTradeAllSalvage.Dispose();
    			MaidRingKeys.Dispose();
    			MaidStackInventory.Dispose();	
    			
    			MaidTab = false;
    		}catch(Exception ex){LogError(ex);}
    	}
		
    	private void MaidCannibalizeEnable_Hit(object sender, EventArgs e)
    	{
    		try
    		{
    			WriteToChat("This will eat your stuff.  You were warned.");
    			
    				MaidCannibalizeInventory = new HudButton();
    				MaidCannibalizeInventory.Text = "Cannibalize Inventory";
    				MaidTabLayout.AddControl(MaidCannibalizeInventory, new Rectangle(0,210,150,20));
    				MaidCannibalizeInventory.Hit += MaidCannibalizeInventory_Hit;

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
				MaidStackList = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.StackCount) < x.Values(LongValueKey.StackMax)).OrderBy(x => x.Name).ToList();
				List<WorldObject> PurgeList = new List<WorldObject>();
				foreach(WorldObject checkfortwo in MaidStackList)
				{
					if(MaidStackList.FindAll(x => x.Name == checkfortwo.Name).Count() == 1)
					{
						PurgeList.Add(checkfortwo);
					}
				}
				foreach(WorldObject purgeitems in PurgeList)
				{
					MaidStackList.RemoveAll(x => x.Name == purgeitems.Name);
				}			 
			}catch(Exception ex){LogError(ex);}
		}
		
		private void MaidProcessStack()
		{
			try
			{
				if(MaidStackList.Count() > 0)
				{
					if(stackbase == null || stackbase.Values(LongValueKey.StackCount) == stackbase.Values(LongValueKey.StackMax))
					{
						stackbase = MaidStackList.First();
						MaidStackList.RemoveAll(x => x.Id == stackbase.Id);
					}
					else
					{
						if(MaidStackList.Any(x => x.Name == stackbase.Name))
						{
							stackitem = MaidStackList.First(x => x.Name == stackbase.Name);
							MaidStackList.RemoveAll(x => x.Id == stackitem.Id);
							Core.Actions.MoveItem(stackitem.Id, Core.CharacterFilter.Id, Core.WorldFilter[stackbase.Id].Values(LongValueKey.Slot), true);
						}
						else
						{
							stackbase = MaidStackList.First();
							MaidStackList.RemoveAll(x => x.Id == stackbase.Id);
						}
						
					}
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
				string[] RingableKeysArray = {"legendary key", "black marrow key", "directive key", "granite key", "mana forge key", "master key", "marble key", "singularity key",	"skeletal falatacot key", "sturdy iron key", "sturdy steel key"};
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
		
		private void MaidTradeAllSalvage_Hit(object sender, System.EventArgs e)
		{
			try
			{
				ScanInventoryForSalvageBags();
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
				ScanInventoryForSalvageBags();
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
				ScanInventoryForSalvageBags();
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
		
		private void MaidSalvageCombine_Hit(object sender, System.EventArgs e)
		{
			WriteToChat("Currently Disabled");
		}
		
		private void SellSalvageBags(int bagtype)
		{
			try
			{
				MaidScanInventoryForSalvageBags();
				
				List <WorldObject> tradelist;
				
				if(bagtype == 0)
				{
					tradelist = MaidSalvage.Where(x => x.Values(LongValueKey.UsesRemaining) == 100).OrderBy(x => x.Name).ToList();
				}
				else if(bagtype == 1)
				{
					tradelist = MaidSalvage.ToList();
				}
				else if(bagtype == 2)
				{
					tradelist = MaidSalvage.Where(x => x.Values(LongValueKey.UsesRemaining) < 100).OrderBy(x => x.Name).ToList();
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
					tradelist = MaidSalvage.Where(x => x.Values(LongValueKey.UsesRemaining) == 100).OrderBy(x => x.Name).ToList();
				}
				else if(bagtype == 1)
				{
					tradelist = MaidSalvage.ToList();
				}
				else if(bagtype == 2)
				{
					tradelist = MaidSalvage.Where(x => x.Values(LongValueKey.UsesRemaining) < 100).OrderBy(x => x.Name).ToList();
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
		
		
		private void MaidScanInventoryForSalvageBags()
		{
			try
			{
				MaidSalvage = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower().Contains("salvage")).ToList();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void MaidProcessCannibalize()
		{
			try
			{
				WriteToChat("Checking " + MaidCannibalizeQueue.First().Name + " for destruction.");
				IdqueueAdd(MaidCannibalizeQueue.First().Id);
				ItemIDListenList.Add(MaidCannibalizeQueue.First().Id);
				MaidCannibalizeProcessList.Add(MaidCannibalizeQueue.Dequeue().Id);
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void EvaluateCannibalizeMatches(LootObject IOItem)
		{
			try
			{
				//Keep those duplicates out
				if(ProcessItemsList.Any(x => x.Id == IOItem.Id)) {return;}
				
				if(IOItem.IOR != IOResult.rule && !IOItem.BValue(BoolValueKey.Retained))
				{
					IOItem.IOR = IOResult.salvage;
					ProcessItemsList.Add(IOItem);
					ItemExclusionList.Add(IOItem.Id);
					UpdateItemHud();
				}	
				return;

			}catch(Exception ex){LogError(ex);}
		}
		
		
	
	
	}
}
