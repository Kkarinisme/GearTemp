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
	/// <summary>
	/// Description of ItemTrackerLooter.
	/// </summary>
	public partial class PluginCore
	{
		
		private List<int> LootedCorpsesList = new List<int>();
		
		private void SubscribeItemTrackerLooterEvents()
		{
			try
			{
				Core.ContainerOpened += LootContainerOpened;
				Core.WorldFilter.ChangeObject += new EventHandler<ChangeObjectEventArgs>(ItemHud_ChangeObject);
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnSubscribeItemTrackerLooterEvents()
		{
			try
			{
				Core.ContainerOpened -= LootContainerOpened;
				Core.WorldFilter.ChangeObject -= new EventHandler<ChangeObjectEventArgs>(ItemHud_ChangeObject);
			}catch(Exception ex){LogError(ex);}
		}
		
		
		public class OpenContainer
		{
			public bool ContainerIsLooting = false;
			public int ContainerGUID = 0;
			public DateTime LastCheck;
			public DateTime LootingStarted;
			public List<IdentifiedObject> ContainerIOs = new List<PluginCore.IdentifiedObject>();
		}
		
		private void ItemHud_ChangeObject(object sender, ChangeObjectEventArgs e)
	 	{
	 		try
	 		{
	 			if(ItemTrackingList.Any(x => x.Id == e.Changed.Id)){ItemTrackingList.RemoveAll(x => x.Id == e.Changed.Id);}
	 			UpdateItemHud();

	 			
	 		}catch(Exception ex){LogError(ex);}
	 	}
		

		
		private void LootContainerOpened(object sender, ContainerOpenedEventArgs e)
		{
			try
			{					
				
				WorldObject container = Core.WorldFilter[e.ItemGuid];
				
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
						if(LootedCorpsesList.Contains(container.Id)) {return;}
						LootedCorpsesList.Add(container.Id);
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
						mOpenContainer.ContainerIOs.Add(new IdentifiedObject(wo));
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
				CoreManager.Current.RenderFrame += new EventHandler<EventArgs>(RenderFrame_LootingCheck);					
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_LootingCheck(object sender, System.EventArgs e)
		{
			try
			{
				//Check every 300 ms
				if((DateTime.Now - mOpenContainer.LastCheck).TotalMilliseconds < 300) {return;}	
				//if it's been at it 10s, it's not happening
				if((DateTime.Now - mOpenContainer.LootingStarted).TotalSeconds > 10) {UnlockContainer();}
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
				CoreManager.Current.RenderFrame -= new EventHandler<EventArgs>(RenderFrame_LootingCheck);					
			}catch(Exception ex){LogError(ex);}
		}
			
		private void SeparateItemsToID(IdentifiedObject IOItem)
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
        			ManualCheckItemForMatches(new IdentifiedObject(Core.WorldFilter[PossibleItemID]));
        		}  		
        		if(ItemIDListenList.Contains(PossibleItemID))
        		{
        			//It came back quit listening already....
        			ItemIDListenList.RemoveAll(x => x == PossibleItemID);
        			CheckItemForMatches(new IdentifiedObject(Core.WorldFilter[PossibleItemID]));
        		}
			} 
			catch (Exception ex) {LogError(ex);}
		}
		
		
		private void CheckItemForMatches(IdentifiedObject IOItem)
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
					if(GISettings.ModifiedLooting) {ReportStringToChat(IOItem.ModString());}
					else {ReportStringToChat(IOItem.LinkString());}
					EvaluateItemMatches(IOItem);
				}
								
			}catch(Exception ex){LogError(ex);}
		}
		
		private void EvaluateItemMatches(IdentifiedObject IOItem)
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
						SalvageItemsList.Add(IOItem);
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
						if(GISettings.SalvageHighValue) {SalvageItemsList.Add(IOItem);}
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
			
		private List<IdentifiedObject> WaitingVTIOs = new List<IdentifiedObject>();
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
				
				IdentifiedObject VTIO = new IdentifiedObject(Core.WorldFilter[id]);	
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
		
		private void SendVTIOtoCallBack(IdentifiedObject VTIO)
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
