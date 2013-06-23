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
		private Queue<WorldObject> SalvageCreatedQueue = new Queue<WorldObject>();
		private int LooterLastItemSelected = 0;
		private string[] RingableKeysArray = {"legendary", "black marrow", "directive", "granite", "mana forge", "master", "marble", "singularity",	"skeletal falatacot"};
		private List<int> AutoDeQueue = new List<int>();
		
		private void SubscribeItemTrackerLooterEvents()
		{
			try
			{
				Core.ContainerOpened += LootContainerOpened;
				Core.ItemDestroyed += ItemTracker_ItemDestroyed;
				Core.WorldFilter.ReleaseObject += ItemTracker_ObjectReleased; 
				Core.WorldFilter.CreateObject += ItemTrackerActions_ObjectCreated;
				Core.WorldFilter.ChangeObject += ItemTrackerActions_ObjectChanged;
				Core.ItemSelected += ItemTracker_ItemSelected;
				Core.CharacterFilter.ActionComplete += ItemTracker_ActionComplete;
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
				Core.WorldFilter.ChangeObject -= ItemTrackerActions_ObjectChanged;
				Core.ItemSelected -= ItemTracker_ItemSelected;
				Core.CharacterFilter.ActionComplete += ItemTracker_ActionComplete;
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void ItemTracker_ItemSelected(object sender, ItemSelectedEventArgs e)
		{
			try
			{
				if(host.Underlying.Hooks.IsValidObject(e.ItemGuid))
				{
					LooterLastItemSelected = e.ItemGuid;
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		LootObject ItemJustChanged = null;
		private void ItemTrackerActions_ObjectChanged(object sender, ChangeObjectEventArgs e)
		{
			try
			{	
				if(e.Changed == null){return;}
				
				if(e.Change == WorldChangeType.IdentReceived)
				{
					if(e.Changed.Id == Host.Actions.CurrentSelection)
	        		{
	        			ManualCheckItemForMatches(new LootObject(e.Changed));
	        		}  		
					else if(ItemIDListenList.Contains(e.Changed.Id))
	        		{
	        			ItemIDListenList.RemoveAll(x => x == e.Changed.Id);
	        			CheckItemForMatches(new LootObject(e.Changed));
	        		}					
				}
				else if(e.Change == WorldChangeType.StorageChange)
				{
					if(ItemHudMoveQueue.Count > 0)
	        		{
					   	if(ItemHudMoveQueue.First().Id == e.Changed.Id)
					   	{
					   		ItemJustChanged = ItemHudMoveQueue.Dequeue();
					   	}	
					   	if(ItemTrackingList.Any(x => x.Id == ItemJustChanged.Id))
					   	{
					   		ItemTrackingList.RemoveAll(x => x.Id == e.Changed.Id);  		
					   	}
					   	if(ItemJustChanged.IOR == IOResult.salvage || ItemJustChanged.IOR == IOResult.dessicate || ItemJustChanged.IOR == IOResult.manatank || ItemJustChanged.ObjectClass == ObjectClass.Key)
	    				{
	    					ProcessItemsList.Add(ItemJustChanged); 					  					
		    				if(ItemJustChanged.IOR == IOResult.salvage && GISettings.AutoSalvage)
		    				{
		    					SalvageItemsQueue.Enqueue(ItemJustChanged);
		    				}
		    				else if(ItemJustChanged.IOR == IOResult.dessicate && GISettings.AutoSalvage)
		    				{
		    					DesiccateItemsQueue.Enqueue(ItemJustChanged);
		    				}
		    				else if(ItemJustChanged.IOR == IOResult.manatank && GISettings.AutoSalvage)
		    				{
		    					ManaTankQueue.Enqueue(ItemJustChanged);
		    				}
		    				else if(ItemJustChanged.ObjectClass == ObjectClass.Key)
		    				{
		    					if(RingableKeysArray.Any(x => ItemJustChanged.Name.ToLower().Contains(x)))
		    					{
		    						KeyItemsQueue.Enqueue(ItemJustChanged);
		    					}
		    				}
	    				}
					   	
					   	UpdateItemHud();
						FireInspectorActions();
						return;
	        		}	
					else if(ItemTrackingList.Any(x => x.Id == e.Changed.Id))
					{
						ItemJustChanged = ItemTrackingList.Find(x => x.Id == e.Changed.Id);
						ItemTrackingList.RemoveAll(x => x.Id == ItemJustChanged.Id);
						AutoDeQueue.Add(ItemJustChanged.Id);
						UpdateItemHud();
						return;
					}
					else
					{
						return;
					}
				}
				else if(e.Change == WorldChangeType.SizeChange)
				{
					if(ItemHudMoveQueue.Count > 0)
					{	
						if(ItemHudMoveQueue.First().Name == e.Changed.Name)
						{
							ItemJustChanged = ItemHudMoveQueue.Dequeue();
							if(ItemTrackingList.Any(x => x.Id == ItemJustChanged.Id))
							{
								ItemTrackingList.RemoveAll(x => x.Id == ItemJustChanged.Id);
							}
							
							if(ItemJustChanged.IOR == IOResult.salvage || ItemJustChanged.IOR == IOResult.dessicate || ItemJustChanged.IOR == IOResult.manatank || ItemJustChanged.ObjectClass == ObjectClass.Key)
		    				{
		    					ProcessItemsList.Add(ItemJustChanged); 					  					
			    				if(ItemJustChanged.IOR == IOResult.salvage && GISettings.AutoSalvage)
			    				{
			    					SalvageItemsQueue.Enqueue(ItemJustChanged);
			    				}
			    				else if(ItemJustChanged.IOR == IOResult.dessicate && GISettings.AutoSalvage)
			    				{
			    					DesiccateItemsQueue.Enqueue(ItemJustChanged);
			    				}
			    				else if(ItemJustChanged.IOR == IOResult.manatank && GISettings.AutoSalvage)
			    				{
			    					ManaTankQueue.Enqueue(ItemJustChanged);
			    				}
			    				else if(ItemJustChanged.ObjectClass == ObjectClass.Key && GISettings.AutoRingKeys)
			    				{
			    					if(RingableKeysArray.Any(x => ItemJustChanged.Name.ToLower().Contains(x)))
			    					{
			    						KeyItemsQueue.Enqueue(ItemJustChanged);
			    					}
			    				}
		    				}
							
							UpdateItemHud();
							FireInspectorActions();
							
							return;
						}
					}
					else if(ItemTrackingList.Any(x => x.Name == e.Changed.Name))
	        		{
						if(ItemTrackingList.FindAll(x => x.Name == e.Changed.Name).Count == 1)
						{
							ItemJustChanged = ItemTrackingList.Find(x => x.Name == e.Changed.Name);
							AutoDeQueue.Add(ItemJustChanged.Id);	
							ItemTrackingList.RemoveAll(x => x.Id == ItemJustChanged.Id);
						}
						else if(ItemTrackingList.Any(x => x.Id == LooterLastItemSelected))
						{
							ItemJustChanged = ItemTrackingList.Find(x => x.Id == LooterLastItemSelected);
							AutoDeQueue.Add(ItemJustChanged.Id);	
							ItemTrackingList.RemoveAll(x => x.Id == ItemJustChanged.Id);
						}
						else
						{
							WriteToChat("Looter Tracking List Corrupted!");
							UpdateItemHud();
						}
	        		}
					UpdateItemHud();	
				}
				else
				{
					return;
				}	
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
				if(ProcessItemsList.Any(x => x.Id == e.ItemGuid))
				{
				   	ProcessItemsList.RemoveAll(x => x.Id == e.ItemGuid);
					UpdateItemHud();
				}
				if(ItemTrackingList.Any(x => x.Id == e.ItemGuid))
				{
					ItemTrackingList.RemoveAll(x => x.Id == e.ItemGuid);
					UpdateItemHud();
				}
				if(AutoDeQueue.Any(x => x == e.ItemGuid))
				{
					AutoDeQueue.RemoveAll(x => x == e.ItemGuid);
				}
				if(ItemExclusionList.Any(x => x == e.ItemGuid))
				{
					ItemExclusionList.RemoveAll(x => x == e.ItemGuid);
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
				if(ProcessItemsList.Any(x => x.Id == e.Released.Id))
				{
				   	ProcessItemsList.RemoveAll(x => x.Id == e.Released.Id);
					UpdateItemHud();
				}
				if(ItemTrackingList.Any(x => x.Id == e.Released.Id))
				{
					ItemTrackingList.RemoveAll(x => x.Id == e.Released.Id);
					UpdateItemHud();
				}
				if(AutoDeQueue.Any(x => x == e.Released.Id))
				{
					AutoDeQueue.RemoveAll(x => x == e.Released.Id);
				}
				if(ItemExclusionList.Any(x => x == e.Released.Id))
				{
					ItemExclusionList.RemoveAll(x => x == e.Released.Id);
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
		
		private void LootContainerOpened(object sender, ContainerOpenedEventArgs e)
		{
			try
			{	
				if(!Host.Underlying.Hooks.IsValidObject(e.ItemGuid)){return;}
				

				
				WorldObject container = Core.WorldFilter[e.ItemGuid];
				
				mOpenContainer.ContainerGUID = container.Id;
				mOpenContainer.LootingStarted = DateTime.Now;
				
//				if(mOpenContainer.ContainerGUID == ListenForContainerID)
//				{
//					ListenForContainerID = 0;
//					InspectorActionPending = false;
//					if(!InspectorRenderFrameActive){InitiateInspectorRenderFrame();}
//					return;
//				}
				
				if(ItemExclusionList.Count > 0 && ItemExclusionList.Contains(e.ItemGuid))
				{
					return;
				}
				
				Core.RenderFrame += RenderFrame_LootContainerOpened;
			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_LootContainerOpened(object sender, EventArgs e)
		{
			try
			{	
			
				if((DateTime.Now - mOpenContainer.LootingStarted).TotalMilliseconds < 200)
				{
					if(!Core.WorldFilter[mOpenContainer.ContainerGUID].HasIdData){return;}
					else{Core.RenderFrame -= RenderFrame_LootContainerOpened;}
				}
				else{Core.RenderFrame -= RenderFrame_LootContainerOpened;}				
							
				if(Core.WorldFilter[mOpenContainer.ContainerGUID] == null) {return;}				
				if(Core.WorldFilter[mOpenContainer.ContainerGUID].Name.Contains("Storage")) {return;}
				//UNDONE: Process DeadMes?
				if(Core.WorldFilter[mOpenContainer.ContainerGUID].Name.Contains(Core.CharacterFilter.Name)){ItemExclusionList.Add(mOpenContainer.ContainerGUID); return;}

				if(Core.WorldFilter[mOpenContainer.ContainerGUID].Name.Contains("Chest") || Core.WorldFilter[mOpenContainer.ContainerGUID].Name.Contains("Vault") || 
				   Core.WorldFilter[mOpenContainer.ContainerGUID].Name.Contains("Reliquary") || Core.WorldFilter[mOpenContainer.ContainerGUID].ObjectClass == ObjectClass.Corpse)
				{
					if(Core.WorldFilter[mOpenContainer.ContainerGUID].ObjectClass == ObjectClass.Corpse){ItemExclusionList.Add(mOpenContainer.ContainerGUID);}
					mOpenContainer.LastCheck = DateTime.Now;
					Core.RenderFrame += RenderFrame_CheckContainer;
				}
			}
			catch(Exception ex){LogError(ex);}	
		}
		
		private void RenderFrame_CheckContainer(object sender, EventArgs e)
		{
			try
			{
				
				if((DateTime.Now - mOpenContainer.LastCheck).TotalMilliseconds < 100) {return;}
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
				Core.RenderFrame += RenderFrame_LootingCheck;					
			}catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_LootingCheck(object sender, System.EventArgs e)
		{
			try
			{
				//Check every 300 ms
				if((DateTime.Now - mOpenContainer.LastCheck).TotalMilliseconds < 300) {return;}	
				//if it's been at it 5s, it's not happening
				if((DateTime.Now - mOpenContainer.LootingStarted).TotalSeconds > 3) {UnlockContainer();}
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
					case IOResult.rare:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						if(mOpenContainer.ContainerIOs.Any(x => x.Id == IOItem.Id)) {mOpenContainer.ContainerIOs.RemoveAll(x =>x.Id == IOItem.Id);}
						UpdateItemHud();
						//PlaySound?
						return;
					case IOResult.salvage:
					case IOResult.dessicate:
					case IOResult.manatank:
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
					Core.RenderFrame += DoesVTIOHaveID;
					WaitingVTIOs.Add(VTIO);
					SendVTIOtoCallBack(VTIO);
					
				}
				
				CheckRulesItem(ref VTIO);
				if(VTIO.ObjectClass == ObjectClass.Scroll){CheckUnknownScrolls(ref VTIO);}
				if(VTIO.IOR == IOResult.unknown) {TrophyListCheckItem(ref VTIO);}
				if(VTIO.IOR == IOResult.unknown && GISettings.IdentifySalvage) {CheckSalvageItem(ref VTIO);}
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
						return 1;
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
