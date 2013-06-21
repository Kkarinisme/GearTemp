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
using VirindiHUDs;
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;
using VirindiViewService.Themes;
using System.Xml.Serialization;
using System.Xml;

namespace GearFoundry
{
	public partial class PluginCore
	{
		private void SubscribeItemTrackerLooterEvents()
		{
			try
			{
				Core.ContainerOpened += LootContainerOpened;
				Core.ItemDestroyed += ItemTracker_ItemDestroyed;
				Core.WorldFilter.ReleaseObject += ItemTracker_ObjectReleased; 
				Core.WorldFilter.CreateObject += ItemTrackerActions_ObjectCreated;
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnSubscribeItemTrackerLooterEvents()
		{
			try
			{
				Core.ContainerOpened -= LootContainerOpened;
				Core.ItemDestroyed -= ItemTracker_ItemDestroyed;
				Core.WorldFilter.ReleaseObject -= ItemTracker_ObjectReleased;
				Core.WorldFilter.CreateObject -= ItemTrackerActions_ObjectCreated;
			}catch(Exception ex){LogError(ex);}
		}
		
		private Queue<WorldObject> SalvageCreatedQueue = new Queue<WorldObject>();
		private List<int> CombineSalvageWOList = new List<int>();
		
		private void ItemTrackerActions_ObjectCreated(object sender, CreateObjectEventArgs e)
		{
			try
			{
				if(e.New.ObjectClass == ObjectClass.Salvage)
				{
					dtInspectorLastAction = DateTime.Now;
					SalvageCreatedQueue.Enqueue(e.New);
					Core.RenderFrame += RenderFrame_InspectorCombineAction;
				}
			}catch(Exception ex){LogError(ex);}
		}

		private void RenderFrame_InspectorCombineAction(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100) {return;}
				else
				{
					dtInspectorLastAction = DateTime.Now;
					Core.RenderFrame -= RenderFrame_InspectorCombineAction;
				}
				LooterCombineSalvage();
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void LooterCombineSalvage()
		{
			try
			{
						
				ScanInventoryForSalvageBags();
				//Find an applicable material rule.
				var materialrules = from allrules in SalvageRulesList
					where (allrules.material == SalvageCreatedQueue.First().Values(LongValueKey.Material)) &&
					       (SalvageCreatedQueue.First().Values(DoubleValueKey.SalvageWorkmanship) >= allrules.minwork) && 
						   (SalvageCreatedQueue.First().Values(DoubleValueKey.SalvageWorkmanship) <= (allrules.maxwork +0.99))
						   select allrules;					
					
				if(materialrules.Count() > 0)
				{
					var sr = materialrules.First();
					
					var partbagslinq = from bags in InventorySalvage
								  where bags.Values(LongValueKey.UsesRemaining) < 100  &&
							      	bags.Values(LongValueKey.Material) == sr.material  &&
								  	bags.Values(DoubleValueKey.SalvageWorkmanship) >= sr.minwork &&
								 	 bags.Values(DoubleValueKey.SalvageWorkmanship) <= (sr.maxwork + 0.99)
								  select new PartialBags{ SalvBagID = bags.Id, SalvBagUses = bags.Values(LongValueKey.UsesRemaining), 
					             	 SalvBagWork = bags.Values(DoubleValueKey.SalvageWorkmanship), SalvBagMat = bags.Values(LongValueKey.Material)};
				
					PartialBags[] partbags = partbagslinq.ToArray();
					
					CombineSalvageWOList.Clear();
					
					int salvagesum = 0;
					salvagesum += SalvageCreatedQueue.First().Values(LongValueKey.UsesRemaining);
					CombineSalvageWOList.Add(SalvageCreatedQueue.Dequeue().Id);
				
					for(int i = 0; i < partbags.Count(); i++)
					{
						if(salvagesum < 100)
						{
							if(salvagesum + partbags[i].SalvBagUses < 110)
							{
								if(!CombineSalvageWOList.Contains(partbags[i].SalvBagID))
							    {
									salvagesum += partbags[i].SalvBagUses;
									CombineSalvageWOList.Add(partbags[i].SalvBagID);
								}
							}
						}		
					}
					WriteToChat("Salvage List Count" + CombineSalvageWOList.Count);
					foreach(var salvagebg in CombineSalvageWOList)
					{
						WriteToChat("SvBag ID: " + salvagebg);
					}
					if(CombineSalvageWOList.Count > 1)
					{
						Core.Actions.UseItem(Core.WorldFilter.GetInventory().Where(x => x.Name == "Ust").First().Id, 0);
						foreach(int salvageid in CombineSalvageWOList)
						{
							Core.Actions.SalvagePanelAdd(salvageid);
						}
						Core.Actions.SalvagePanelSalvage();
					}
					CombineSalvageWOList.Clear();	
				}
				if(SalvageItemsQueue.Count > 0)
				{
					dtInspectorLastAction = DateTime.Now;
					Core.RenderFrame += RenderFrame_InspectorSalvageAction;
				}
				else if(ItemHudMoveQueue.Count > 0)
				{
					dtInspectorLastAction = DateTime.Now;
					Core.RenderFrame += RenderFrame_ReopenContainer;
				}
			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_ReopenContainer(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100) {return;}
				else
				{
					Core.RenderFrame -= RenderFrame_ReopenContainer;
					dtInspectorLastAction = DateTime.Now;
				}
				
				Core.Actions.UseItem(LastContainer, 0);
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void ItemTracker_ItemDestroyed(object sender, ItemDestroyedEventArgs e)
		{
			try
			{
				if(mOpenContainer.ContainerIOs.Any(x => x.Id == e.ItemGuid))
				{
					mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == e.ItemGuid);
					UpdateItemHud();
				}
				if(WaitingVTIOs.Any(x => x.Id == e.ItemGuid))
				{
					WaitingVTIOs.RemoveAll(x => x.Id == e.ItemGuid);
					UpdateItemHud();
				}
				if(SalvageItemsList.Any(x => x.Id == e.ItemGuid))
				{
				   	SalvageItemsList.RemoveAll(x => x.Id == e.ItemGuid);
					UpdateItemHud();
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ItemTracker_ObjectReleased(object sender, ReleaseObjectEventArgs e)
		{
			try
			{
				if(mOpenContainer.ContainerIOs.Any(x => x.Id == e.Released.Id))
				{
					mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == e.Released.Id);
					UpdateItemHud();
				}
				if(WaitingVTIOs.Any(x => x.Id == e.Released.Id))
				{
					WaitingVTIOs.RemoveAll(x => x.Id == e.Released.Id);
					UpdateItemHud();
				}
			}catch(Exception ex){LogError(ex);}
		}
				
		public class OpenContainer
		{
			public bool ContainerIsLooting = false;
			public int ContainerGUID = 0;
			public DateTime LastCheck;
			public DateTime LootingStarted;
			public List<LootObject> ContainerIOs = new List<PluginCore.LootObject>();
		}
		
		
		private int LastContainer = 0;		
		private void RenderFrame_InspectorOpenUst(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100) {return;}
				else
				{
					Core.RenderFrame -= RenderFrame_InspectorOpenUst;	
					dtInspectorLastAction = DateTime.Now;
				}
				
				LastContainer = Core.Actions.OpenedContainer;
				
				Core.Actions.UseItem(Core.WorldFilter.GetInventory().Where(x => x.Name == "Ust").First().Id,0);
				
				dtInspectorLastAction = DateTime.Now;
				Core.RenderFrame += RenderFrame_InspectorSalvageAction;
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_InspectorSalvageAction(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 200) {return;}
				else
				{
					Core.RenderFrame -= RenderFrame_InspectorSalvageAction;	
					dtInspectorLastAction = DateTime.Now;
				}
				
				if(SalvageItemsList.Any(x => x.Id == SalvageItemsQueue.First().Id))
				{
					SalvageItemsList.RemoveAll(x => x.Id == SalvageItemsQueue.First().Id);
					UpdateItemHud();
				}
				Core.Actions.SalvagePanelAdd(SalvageItemsQueue.Dequeue().Id);
				Core.Actions.SalvagePanelSalvage();
			
			}catch(Exception ex){LogError(ex);}
			
		}
		
		private void LootContainerOpened(object sender, ContainerOpenedEventArgs e)
		{
			try
			{	
				//FIXME:  Working properly.
				
				WorldObject container = Core.WorldFilter[e.ItemGuid];
				
				//If an item was queued for looting from a closed container, listen for the open			
				if(ItemHudMoveQueue.Count > 0)
				{
					if(container.Id == ItemHudMoveQueue.First().Container)
					{
						WriteToChat("Caught Closed container opening for loot pickup.");
						dtInspectorLastAction = DateTime.Now;
						Core.RenderFrame += RenderFrame_InspectorMoveAction;
						return;
					}
				}
				
				if(container.Name.Contains("Storage")) {return;}
				if(container == null) {return;}
				
				//UNDONE: Process DeadMes?
				if(container.Name.Contains(Core.CharacterFilter.Name)){return;}

				if(container.Name.Contains("Chest") || container.Name.Contains("Vault") || container.Name.Contains("Reliquary") || container.ObjectClass == ObjectClass.Corpse)
				{
					mOpenContainer.ContainerGUID = container.Id;
					mOpenContainer.LootingStarted = DateTime.Now;
					Core.RenderFrame += RenderFrame_CheckContainer;
				}
			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_CheckContainer(object sender, EventArgs e)
		{
			try
			{
				
				if((DateTime.Now - mOpenContainer.LootingStarted).TotalMilliseconds < 100) {return;}
				else{Core.RenderFrame -= RenderFrame_CheckContainer;}
				
				mOpenContainer.LastCheck = DateTime.Now;
				
				foreach(WorldObject wo in Core.WorldFilter.GetByContainer(mOpenContainer.ContainerGUID))
				{
					if(!ItemExclusionList.Any(x => x == wo.Id))
					{
						mOpenContainer.ContainerIOs.Add(new LootObject(wo));
					}
					SeparateItemsToID(mOpenContainer.ContainerIOs.Last());
				}
				LockContainerOpen();
				
			}catch{}
		}
		
		private void LockContainerOpen()
		{
			try
			{
				mOpenContainer.ContainerIsLooting = true;
				Core.RenderFrame += new EventHandler<EventArgs>(RenderFrame_LootingCheck);					
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_LootingCheck(object sender, System.EventArgs e)
		{
			try
			{
				//Check every 300 ms
				if((DateTime.Now - mOpenContainer.LastCheck).TotalMilliseconds < 300) {return;}	
				//if it's been at it 5s, it's not happening
				if((DateTime.Now - mOpenContainer.LootingStarted).TotalSeconds > 5) {UnlockContainer();}
        		//ID function must clean these out to unlock container.  This will hold it open until all IDs complete.
				if(mOpenContainer.ContainerIOs.Count > 0)
				{
					if(mOpenContainer.ContainerGUID != Core.Actions.OpenedContainer)
    				{
    					Core.Actions.UseItem(mOpenContainer.ContainerGUID, 0);
    				}
				}
				else
        		{
        			UnlockContainer();
        		}	
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnlockContainer()
		{
			try
			{
				mOpenContainer.ContainerIsLooting = false;
				mOpenContainer.ContainerIOs.Clear();
				Core.RenderFrame -= RenderFrame_LootingCheck;					
			}catch(Exception ex){LogError(ex);}
		}
			
		private void SeparateItemsToID(LootObject IOItem)
		{
				try
				{
					//Get rid of non-existant items...
					if(!IOItem.isvalid)
					{
						IOItem.IOR = IOResult.nomatch;
						mOpenContainer.ContainerIOs.RemoveAll(x => !x.isvalid);
						return;
					}
					
					//Flag items that need additional info to ID...
					if(!IOItem.HasIdData)
					{
						//This should remove items which require identifications to match and queue them for listening for IDs.  All other items should pass through default.
						switch(IOItem.ObjectClass)
						{
							case ObjectClass.Armor:
							case ObjectClass.Clothing:
							case ObjectClass.Gem:
							case ObjectClass.Jewelry:
							case ObjectClass.MeleeWeapon:
							case ObjectClass.MissileWeapon:
							case ObjectClass.WandStaffOrb:								
								IdqueueAdd(IOItem.Id);
								ItemIDListenList.Add(IOItem.Id);
								return;
					
							case ObjectClass.Misc:
								if(IOItem.Name.Contains("Essence"))
								{
									IdqueueAdd(IOItem.Id);
									ItemIDListenList.Add(IOItem.Id);
									return;
								}
								break;								
						}
						if(!ItemIDListenList.Contains(IOItem.Id)){CheckItemForMatches(IOItem);}
					}
				} catch (Exception ex) {LogError(ex);} 
				return;
			}
		
		
		private void ServerDispatchItem(object sender, Decal.Adapter.NetworkMessageEventArgs e)
        {
        	int iEvent = 0;
            try
            {    
            	if(e.Message.Type == AC_ADJUST_STACK)
            	{
            		ItemHud_OnAdjustStack(e.Message);
            	}
            	if(e.Message.Type == AC_GAME_EVENT)
            	{	
            		try
                    {
                    	iEvent = Convert.ToInt32(e.Message["event"]);
                    }
                    catch{}
                    if(iEvent == GE_IDENTIFY_OBJECT)
                    {
                    	ItemHud_OnIdentItem(e.Message);
                    }
                    if(iEvent == GE_INSERT_INVENTORY_ITEM)
                    {
                    	ItemHud_OnInsertInventory(e.Message);
                    }                      
            	}
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        } 
		
		private void ItemHud_OnAdjustStack(Decal.Adapter.Message pMsg)
		{
			try
			{
				int PossibleMovedItem = Convert.ToInt32(pMsg["item"]);
				if(ItemTrackingList.Any(x => x.Name == Core.WorldFilter[PossibleMovedItem].Name))
        		{
        			ItemTrackingList.RemoveAll(x => x.Name == Core.WorldFilter[PossibleMovedItem].Name);
        			if(ItemHudMoveQueue.First().Name ==  Core.WorldFilter[PossibleMovedItem].Name) 
        			{
        				ItemHudMoveQueue.Dequeue();
        			}
        			
        			UpdateItemHud();
        			
        			if(ItemHudMoveQueue.Count > 0)
        			{
        				FireInspectorActions();
        			}
        		}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ItemHud_OnInsertInventory(Decal.Adapter.Message pMsg)
		{
			try
			{
    	   		int PossibleMovedItem = Convert.ToInt32(pMsg["item"]);		
        		if(ItemTrackingList.Any(x => x.Id == PossibleMovedItem))
        		{
        			if(ItemTrackingList.Find(x => x.Id == PossibleMovedItem).IOR == IOResult.salvage)
        			{
        				SalvageItemsList.Add(ItemTrackingList.Find(x => x.Id == PossibleMovedItem));
        				ItemTrackingList.RemoveAll(x => x.Id == PossibleMovedItem);
        				if(ItemHudMoveQueue.First().Id == PossibleMovedItem)
        				{
        					ItemHudMoveQueue.Dequeue();
        				}
        				UpdateItemHud();
        				if(GISettings.AutoSalvage)
        				{
        					dtInspectorLastAction = DateTime.Now;
        					SalvageItemsQueue.Enqueue(SalvageItemsList.Find(x => x.Id == PossibleMovedItem));
        					Core.RenderFrame += RenderFrame_InspectorOpenUst;	
        				}
        			}
        			else
        			{
	        			ItemTrackingList.RemoveAll(x => x.Id == PossibleMovedItem);
	        			if(ItemHudMoveQueue.First().Id == PossibleMovedItem)
	        			{
	        				ItemHudMoveQueue.Dequeue();
	        			}
	        			UpdateItemHud();
	        			
	        			if(ItemHudMoveQueue.Count > 0)
	        			{
	        				FireInspectorActions();
	        			}
        			}
        		}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void ItemHud_OnIdentItem(Decal.Adapter.Message pMsg)
		{
			try
			{
    	   		int PossibleItemID = Convert.ToInt32(pMsg["object"]);		
        		//Bypass looter and use manual ID feature
        		if(PossibleItemID == Host.Actions.CurrentSelection)
        		{
        			ManualCheckItemForMatches(new LootObject(Core.WorldFilter[PossibleItemID]));
        		}  		
        		if(ItemIDListenList.Contains(PossibleItemID))
        		{
        			//It came back quit listening already....
        			ItemIDListenList.RemoveAll(x => x == PossibleItemID);
        			CheckItemForMatches(new LootObject(Core.WorldFilter[PossibleItemID]));
        		}
			} 
			catch (Exception ex) {LogError(ex);}
		}
		
		
		private void CheckItemForMatches(LootObject IOItem)
		{
			try
			{
				if(IOItem.HasIdData){CheckRulesItem(ref IOItem);}
				if(IOItem.ObjectClass == ObjectClass.Scroll){CheckUnknownScrolls(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {TrophyListCheckItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckSalvageItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckManaItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckValueItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {IOItem.IOR = IOResult.nomatch;}
				
				//Clean out no matches.
				if(IOItem.IOR == IOResult.nomatch)
				{
					if(mOpenContainer.ContainerIOs.Any(x => x.Id == IOItem.Id)) {mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == IOItem.Id);}
				}
				else
				{
					if(GISettings.ModifiedLooting) {ReportStringToChat(IOItem.GSReportString());}
					else {ReportStringToChat(IOItem.LinkString());}
					EvaluateItemMatches(IOItem);
				}
								
			}catch(Exception ex){LogError(ex);}
		}
		
		private void EvaluateItemMatches(LootObject IOItem)
		{
			try
			{
				//Keep those duplicates out
				if(ItemTrackingList.Any(x => x.Id == IOItem.Id) || ItemExclusionList.Contains(IOItem.Id)) {return;}
				
				switch(IOItem.IOR)
				{
					case IOResult.rule:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						if(mOpenContainer.ContainerIOs.Any(x => x.Id == IOItem.Id)) {mOpenContainer.ContainerIOs.RemoveAll(x =>x.Id == IOItem.Id);}
						UpdateItemHud();
						//PlaySound?
						return;
					case IOResult.manatank:
						ItemTrackingList.Add(IOItem);
						ManaTankItems.Add(IOItem.Id);
						ItemExclusionList.Add(IOItem.Id);
						if(mOpenContainer.ContainerIOs.Any(x => x.Id == IOItem.Id)) {mOpenContainer.ContainerIOs.RemoveAll(x =>x.Id == IOItem.Id);}
						UpdateItemHud();
						//PlaySound?
						return;
					case IOResult.rare:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						if(mOpenContainer.ContainerIOs.Any(x => x.Id == IOItem.Id)) {mOpenContainer.ContainerIOs.RemoveAll(x =>x.Id == IOItem.Id);}
						UpdateItemHud();
						//PlaySound?
						return;
					case IOResult.salvage:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						if(mOpenContainer.ContainerIOs.Any(x => x.Id == IOItem.Id)) {mOpenContainer.ContainerIOs.RemoveAll(x =>x.Id == IOItem.Id);}
						UpdateItemHud();
						//PlaySound?
						return;
					case IOResult.spell:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						if(mOpenContainer.ContainerIOs.Any(x => x.Id == IOItem.Id)) {mOpenContainer.ContainerIOs.RemoveAll(x =>x.Id == IOItem.Id);}
						UpdateItemHud();
						//PlaySound?
						return;
					case IOResult.trophy:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						if(mOpenContainer.ContainerIOs.Any(x => x.Id == IOItem.Id)) {mOpenContainer.ContainerIOs.RemoveAll(x =>x.Id == IOItem.Id);}
						UpdateItemHud();
						//PlaySound?
						return;
					case IOResult.val:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						if(GISettings.SalvageHighValue) {IOItem.IOR = IOResult.salvage;}
						if(mOpenContainer.ContainerIOs.Any(x => x.Id == IOItem.Id)) {mOpenContainer.ContainerIOs.RemoveAll(x =>x.Id == IOItem.Id);}
						UpdateItemHud();
						//PlaySound?
						return;
					default:
						ItemExclusionList.Add(IOItem.Id);
						if(mOpenContainer.ContainerIOs.Any(x => x.Id == IOItem.Id)) {mOpenContainer.ContainerIOs.RemoveAll(x =>x.Id == IOItem.Id);}
						return;
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void FireInspectorActions()
		{
			try
			{
				dtInspectorLastAction = DateTime.Now;
				Core.RenderFrame += RenderFrame_InspectorMoveAction;
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private DateTime dtInspectorLastAction;
		private void RenderFrame_InspectorMoveAction(object sender, System.EventArgs e)
		{
			try
			{
				//Fire every 100ms
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_InspectorMoveAction;
					dtInspectorLastAction = DateTime.Now;
					WriteToChat("Inspector Move Action initiated");
				}
				
				//Shut down if move queue is empty.
				if(ItemHudMoveQueue.Count == 0)
				{
					return;
				}
				
				//Open the container if it has been closed.  Move to "open container" to listen.
//				if(Core.Actions.OpenedContainer != ItemHudMoveQueue.ElementAt(0).Container)
//				{
//					WriteToChat("Blocked Container, Listen started");
//					Core.Actions.UseItem(ItemHudMoveQueue.ElementAt(0).Container, 0);
//					ContainerWaiting = ItemHudMoveQueue.ElementAt(0).Container;
//					return;
//				}
				
				//Try to move it, listen in moved object
				Core.Actions.UseItem(ItemHudMoveQueue.ElementAt(0).Id, 0);
				return;	
			}catch(Exception ex){LogError(ex);}			
		}
		
		
		
		//Virindi Tank Looting..............................................................................................	
		private List<LootObject> WaitingVTIOs = new List<LootObject>();
		public int VTLinkDecision(int id, int reserved1, int reserved2)
		{
			try
			{	
				//If VT shoots in a corpse ID, check if it has a rare on it.  If so, Loot, if not, skip.				
				if(reserved2 == 1)
				{
					if(CorpseTrackingList.Any(x => x.IOR == IOResult.corpsewithrare) && reserved1 == CorpseTrackingList.Find(x => x.IOR == IOResult.corpsewithrare).Id)
					{
						return 1;
					}
					else
					{
						return 0;
					}
				}
				
				if(ItemTrackingList.Any(x => x.Id == id))
				{
					switch(ItemTrackingList.Find(x => x.Id == id).IOR)
					{
						case IOResult.rule:
						case IOResult.manatank:
						case IOResult.rare:
						case IOResult.spell:
						case IOResult.trophy:								
							return 1;						
						case IOResult.salvage:
							return 2;
						case IOResult.val:
							if(GISettings.SalvageHighValue) {return 2;}
							else{return 1;}
						default:
							return 0;
					}
				}
				
				LootObject VTIO = new LootObject(Core.WorldFilter[id]);	
				if(!VTIO.HasIdData)
				{
					Core.RenderFrame += new EventHandler<EventArgs>(DoesVTIOHaveID);
					WaitingVTIOs.Add(VTIO);
					SendVTIOtoCallBack(VTIO);
					
				}
				
				CheckRulesItem(ref VTIO);
				if(VTIO.ObjectClass == ObjectClass.Scroll){CheckUnknownScrolls(ref VTIO);}
				if(VTIO.IOR == IOResult.unknown) {TrophyListCheckItem(ref VTIO);}
				if(VTIO.IOR == IOResult.unknown) {CheckSalvageItem(ref VTIO);}
				if(VTIO.IOR == IOResult.unknown) {CheckManaItem(ref VTIO);}
				if(VTIO.IOR == IOResult.unknown) {CheckValueItem(ref VTIO);}
				if(VTIO.IOR == IOResult.unknown) {VTIO.IOR = IOResult.nomatch;}
				
													
				switch(VTIO.IOR)
				{
					case IOResult.rule:
					case IOResult.manatank:
					case IOResult.rare:
					case IOResult.spell:
					case IOResult.trophy:								
						return 1;						
					case IOResult.salvage:
						return 2;
					case IOResult.val:
						if(GISettings.SalvageHighValue) {return 2;}
						else{return 1;}
					default:
						return 0;
				}


						
			}catch(Exception ex){LogError(ex); return 0;}
		}
		
		private void SendVTIOtoCallBack(LootObject VTIO)
		{	
			if(WaitingVTIOs.Count == 0) 
			{
				Core.RenderFrame -= new EventHandler<EventArgs>(DoesVTIOHaveID);
				return;
			}
		}
		
		private void DoesVTIOHaveID(object sender, EventArgs e)
		{
			try
			{
				if(WaitingVTIOs.Any(x => x.HasIdData == true)){WaitingVTIOs.RemoveAll(x => x.HasIdData == true);}
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void IDChecker(object sender, System.EventArgs e)
		{
			try
			{
					
			}catch(Exception ex){LogError(ex);}
			
			
		}
		


	}
}


//ess, Wield Lvl 150, Lore 275
//(Trophy) Ancient Falatacot Trinket
//(Trophy) Quill of Introspection (102.0S, 102.0W)
//[VTank] --------------Object dump--------------
//[VTank] [Meta] Create count: 1
//[VTank] [Meta] Create time: 6/20/2013 7:21 AM
//[VTank] [Meta] Has identify data: True
//[VTank] [Meta] Last ID time: 6/20/2013 7:21 AM
//[VTank] [Meta] Worldfilter valid: True
//[VTank] ID: 88657F88
//[VTank] ObjectClass: CraftedAlchemy
//[VTank] (S) Name: Quill of Introspection
//[VTank] (S) SecondaryName: Quills of Introspection
//[VTank] (I) CreateFlags1: 2650137
//[VTank] (I) Type: 37364
//[VTank] (I) Icon: 26901
//[VTank] (I) Category: 67108864
//[VTank] (I) Behavior: 16
//[VTank] (I) Value: 30000
//[VTank] (I) Unknown10: 524296
//[VTank] (I) UsageMask: 4201088
//[VTank] (I) StackCount: 1
//[VTank] (I) StackMax: 1000
//[VTank] (I) Container: -2006493254
//[VTank] (I) Burden: 4
//[VTank] (I) PhysicsDataFlags: 131073
//(Trophy) Quill of Introspection (102.0S, 102.0W)
//(Trophy) Quill of Introspection (102.0S, 102.0W)
//Pack (102.0S, 102.0W)
//Pack (102.0S, 102.0W)
//Pack (102.0S, 102.0W)
//Pack (102.0S, 102.0W)
//(Trophy) Quill of Introspection (102.0S, 102.0W)
//[VTank] --------------Object dump--------------
//[VTank] [Meta] Create count: 1
//[VTank] [Meta] Create time: 6/20/2013 7:20 AM
//[VTank] [Meta] Has identify data: True
//[VTank] [Meta] Last ID time: 6/20/2013 7:21 AM
//[VTank] [Meta] Worldfilter valid: True
//[VTank] ID: 8769AE8F
//[VTank] ObjectClass: CraftedAlchemy
//[VTank] (S) Name: Quill of Introspection
//[VTank] (S) SecondaryName: Quills of Introspection
//[VTank] (I) CreateFlags1: 2650137
//[VTank] (I) Type: 37364
//[VTank] (I) Icon: 26901
//[VTank] (I) Category: 67108864
//[VTank] (I) Behavior: 16
//[VTank] (I) Value: 270000
//[VTank] (I) Unknown10: 524296
//[VTank] (I) UsageMask: 4201088
//[VTank] (I) StackCount: 9
//[VTank] (I) StackMax: 1000
//[VTank] (I) Container: -2139809713
//[VTank] (I) Burden: 36
//[VTank] (I) PhysicsDataFlags: 131073

//before
//[VTank] --------------Object dump--------------
//[VTank] [Meta] Create count: 1
//[VTank] [Meta] Create time: 6/20/2013 7:29 AM
//[VTank] [Meta] Has identify data: True
//[VTank] [Meta] Last ID time: 6/20/2013 7:29 AM
//[VTank] [Meta] Worldfilter valid: True
//[VTank] ID: 88687EBF
//[VTank] ObjectClass: CraftedAlchemy
//[VTank] (S) Name: Quill of Infliction
//[VTank] (S) SecondaryName: Quills of Infliction
//[VTank] (I) CreateFlags1: 2650137
//[VTank] (I) Type: 37363
//[VTank] (I) Icon: 26900
//[VTank] (I) Category: 67108864
//[VTank] (I) Behavior: 16
//[VTank] (I) Value: 30000
//[VTank] (I) Unknown10: 524296
//[VTank] (I) UsageMask: 4201088
//[VTank] (I) StackCount: 1
//[VTank] (I) StackMax: 1000
//[VTank] (I) Container: -2006417734
//[VTank] (I) Burden: 4
//[VTank] (I) PhysicsDataFlags: 131073
//
//after	
//[VTank] --------------Object dump--------------
//[VTank] [Meta] Create count: 1
//[VTank] [Meta] Create time: 6/20/2013 7:29 AM
//[VTank] [Meta] Has identify data: True
//[VTank] [Meta] Last ID time: 6/20/2013 7:30 AM
//[VTank] [Meta] Worldfilter valid: True
//[VTank] ID: 8768E53B
//[VTank] ObjectClass: CraftedAlchemy
//[VTank] (S) Name: Quill of Infliction
//[VTank] (S) SecondaryName: Quills of Infliction
//[VTank] (I) CreateFlags1: 2650137
//[VTank] (I) Type: 37363
//[VTank] (I) Icon: 26900
//[VTank] (I) Category: 67108864
//[VTank] (I) Behavior: 16
//[VTank] (I) Value: 180000
//[VTank] (I) Unknown10: 524296
//[VTank] (I) UsageMask: 4201088
//[VTank] (I) StackCount: 6
//[VTank] (I) StackMax: 1000
//[VTank] (I) Container: -2139809713
//[VTank] (I) Burden: 24
//[VTank] (I) PhysicsDataFlags: 131073