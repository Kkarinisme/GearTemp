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
				Core.WorldFilter.ChangeObject += new EventHandler<ChangeObjectEventArgs>(ItemHud_ChangeObject);
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
				Core.WorldFilter.ChangeObject -= new EventHandler<ChangeObjectEventArgs>(ItemHud_ChangeObject);
				Core.ItemDestroyed -= ItemTracker_ItemDestroyed;
				Core.WorldFilter.ReleaseObject -= ItemTracker_ObjectReleased;
				Core.WorldFilter.CreateObject -= ItemTrackerActions_ObjectCreated;
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ItemTrackerActions_ObjectCreated(object sender, CreateObjectEventArgs e)
		{
			try
			{
				if(e.New.ObjectClass == ObjectClass.Salvage)
				{
					dtInspectorLastAction = DateTime.Now;
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
				//TODO:  Combine Salvage Here
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void ItemTracker_ItemDestroyed(object sender, ItemDestroyedEventArgs e)
		{
			try
			{
				if(mOpenContainer.ContainerIOs.Any(x => x.Id == e.ItemGuid)){mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == e.ItemGuid);}
				if(WaitingVTIOs.Any(x => x.Id == e.ItemGuid)){WaitingVTIOs.RemoveAll(x => x.Id == e.ItemGuid);}
				if(SalvageItemsList.First().Id == e.ItemGuid){SalvageItemsList.RemoveAt(0);}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ItemTracker_ObjectReleased(object sender, ReleaseObjectEventArgs e)
		{
			try
			{
				if(mOpenContainer.ContainerIOs.Any(x => x.Id == e.Released.Id)){mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == e.Released.Id);}
				if(WaitingVTIOs.Any(x => x.Id == e.Released.Id)){WaitingVTIOs.RemoveAll(x => x.Id == e.Released.Id);}			
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
		
		private void ItemHud_ChangeObject(object sender, ChangeObjectEventArgs e)
	 	{
	 		try
	 		{
	 			if(ItemTrackingList.Any(x => x.Id == e.Changed.Id)){ItemTrackingList.RemoveAll(x => x.Id == e.Changed.Id);}
	 			if(ItemHudMoveQueue.ElementAt(0).Id == e.Changed.Id) 
	 			{
	 				LootObject JustMovedItem = ItemHudMoveQueue.Dequeue();	
	 				//Now that it is in inventory, keep track of it for further processing.
	 				if(JustMovedItem.IOR == IOResult.salvage)
	 				{
	 					SalvageItemsList.Add(JustMovedItem);
	 					if(GISettings.AutoSalvage)
	 					{
	 						Core.RenderFrame += RenderFrame_InspectorSalvageAction;	
	 					}
	 				}
	 				if(JustMovedItem.IOR == IOResult.manatank)
	 				{
	 					ManaTankItems.Add(JustMovedItem.Id);
	 				}
	 				
	 				//This *should* chain loot with a 100ms delay between loots.
	 				//FIXME:  Add additional render frame timers here following looting.
	 				if(ItemHudMoveQueue.Count > 0)
	 				{
	 					dtInspectorLastAction = DateTime.Now;
	 					Core.RenderFrame += RenderFrame_InspectorMoveAction;
	 				}
	 			}
	 			UpdateItemHud();	
	 		}catch(Exception ex){LogError(ex);}
	 	}
		
		
		private void RenderFrame_InspectorSalvageAction(object sender, EventArgs e)
		{
			try
			{
				if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100) {return;}
				else
				{
					Core.RenderFrame -= RenderFrame_InspectorSalvageAction;	
					dtInspectorLastAction = DateTime.Now;
				}
				Core.Actions.SalvagePanelAdd(SalvageItemsList.First().Id);
				Core.Actions.SalvagePanelSalvage();
				
				//Listen in ItemDestroyed, clear from list
				//Listen in CreateObject, combine salvage action
			}catch(Exception ex){LogError(ex);}
			
		}
		
		private void LootContainerOpened(object sender, ContainerOpenedEventArgs e)
		{
			try
			{					
				
				WorldObject container = Core.WorldFilter[e.ItemGuid];
				
				//If an item was queued for looting from a closed container, listen for the open			
				if(container.Id == ContainerWaiting)
				{
					ContainerWaiting = 0;
					dtInspectorLastAction = DateTime.Now;
					Core.RenderFrame += RenderFrame_InspectorMoveAction;
					return;
				}
				
				if(container.Name.Contains("Storage")) {return;}
				if(container == null) {return;}

				//this will currently ID items off other player's corpses.
				if(container.ObjectClass == ObjectClass.Corpse)
				{
					if(container.Name.Contains(Core.CharacterFilter.Name))
					{
						//Should cross over and remove dead me's when addon completes
						//for now will just prevent looting of own corpses.
						//ghSettings.DeadMeList.RemoveAll(x => x.GUID == container.Id);
						return;
					}
					//Don't loot out permitted corpses.....
//					else if(PermittedCorpsesList.Count() > 0 && container.Name
//					{
//
//					}
					else
					{
						CheckContainer(container);
					}
				}
				if(container.Name.Contains("Chest") || container.Name.Contains("Vault") || container.Name.Contains("Reliquary"))
				{
					CheckContainer(container);	
				}
			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void CheckContainer(WorldObject container)
		{
			try
			{
				
				if(Core.WorldFilter.GetByContainer(container.Id).Count == 0) {return;}
				
				mOpenContainer.ContainerGUID = container.Id;
				mOpenContainer.LastCheck = DateTime.Now;
				mOpenContainer.LootingStarted = DateTime.Now;
				
				foreach(WorldObject wo in Core.WorldFilter.GetByContainer(container.Id))
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
				mOpenContainer.ContainerGUID = 0;
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
            	if(e.Message.Type == AC_GAME_EVENT)
            	{	
            		try
                    {
                    	iEvent = Convert.ToInt32(e.Message["event"]);
                    }
                    catch{}
                    if(iEvent == GE_IDENTIFY_OBJECT)
                    {
                    	 OnIdentItem(e.Message);
                    }
                    
            	}
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        } 
		
		private void OnIdentItem(Decal.Adapter.Message pMsg)
		{
			try
			{
				if(!bItemHudEnabled) {return;}
    	   		int PossibleItemID = Convert.ToInt32(pMsg["object"]);		
        		//Bypass looter and use manual ID feature
        		if(PossibleItemID == Host.Actions.CurrentSelection && bReportItemStrings)
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
			if(!Looting) 
			{
				dtInspectorLastAction = DateTime.Now;
				Core.RenderFrame += new EventHandler<EventArgs>(RenderFrame_InspectorMoveAction);
			}
		}
		
		
		private DateTime dtInspectorLastAction;
		private int ContainerWaiting = 0;
		private void RenderFrame_InspectorMoveAction(object sender, System.EventArgs e)
		{
			//Fire every 100ms
			if((DateTime.Now - dtInspectorLastAction).TotalMilliseconds < 100){return;}
			else{dtInspectorLastAction = DateTime.Now;}
			
			//Shut down if move queue is empty.
			if(ItemHudMoveQueue.Count == 0)
			{
				Core.RenderFrame -= RenderFrame_InspectorMoveAction;
				return;
			}
			
			//Open the container if it has been closed.  Move to "open container" to listen.
			if(mOpenContainer.ContainerGUID != ItemHudMoveQueue.ElementAt(0).Container)
			{
				Core.RenderFrame -= RenderFrame_InspectorMoveAction;
				Core.Actions.UseItem(ItemHudMoveQueue.ElementAt(0).Container, 0);
				ContainerWaiting = ItemHudMoveQueue.ElementAt(0).Container;
			}
			
			//Turn it off so that we don't spam the server with move actions.
			Core.RenderFrame -= RenderFrame_InspectorMoveAction;
			//Try to move it, listen in change object
			Core.Actions.MoveItem(ItemHudMoveQueue.ElementAt(0).Id,Core.CharacterFilter.Id,0,true);
			
			return;			
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
