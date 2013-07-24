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
		private int LooterLastItemSelected;
		private string[] RingableKeysArray = {"legendary", "black marrow", "directive", "granite", "mana forge", "master", "marble", "singularity",	"skeletal falatacot"};
		
		private void SubscribeItemTrackerLooterEvents()
		{
			try
			{
				LooterLastItemSelected = 0;	
				Core.ContainerOpened += LootContainerOpened;
				Core.ItemDestroyed += ItemTracker_ItemDestroyed;
				Core.WorldFilter.ReleaseObject += ItemTracker_ObjectReleased; 
				Core.WorldFilter.ChangeObject += ItemTrackerActions_ObjectChanged;
				Core.ItemSelected += ItemTracker_ItemSelected;
				Core.WorldFilter.CreateObject += SalvageCreated;
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnSubscribeItemTrackerLooterEvents()
		{
			try
			{	
				Core.ContainerOpened -= LootContainerOpened;
				Core.ItemDestroyed -= ItemTracker_ItemDestroyed;
				Core.WorldFilter.ReleaseObject -= ItemTracker_ObjectReleased;
				Core.WorldFilter.ChangeObject -= ItemTrackerActions_ObjectChanged;
				Core.ItemSelected -= ItemTracker_ItemSelected;
				Core.WorldFilter.CreateObject -= SalvageCreated;
			}catch(Exception ex){LogError(ex);}
		}		
		
		private void ItemTracker_ItemSelected(object sender, ItemSelectedEventArgs e)
		{
			try
			{
				if(Core.WorldFilter[e.ItemGuid] != null)
				{
					LooterLastItemSelected = e.ItemGuid;
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ItemTrackerActions_ObjectChanged(object sender, ChangeObjectEventArgs e)
		{
			try
			{	
				if(e.Change == WorldChangeType.IdentReceived)
				{
					if(e.Changed.Id == Host.Actions.CurrentSelection)
	        		{
	        			ManualCheckItemForMatches(new LootObject(e.Changed));
	        			return;
	        		}  		
					else if(LOList.Any(x => x.Id == e.Changed.Id && x.Listen))
	        		{
						LootObject lo = LOList.Find(x => x.Id == e.Changed.Id);
						lo.Listen = false;
	        			CheckItemForMatches(lo.Id);
	        			return;
	        		}					
				}
				else if(e.Change == WorldChangeType.StorageChange)
				{
					if(LOList.Any(x => x.Id == e.Changed.Id && x.ActionTarget))
					{
						Core.RenderFrame += InspectorMoveCheckBack;
						return;
					}
					
					if(LOList.Any(x => x.Id == e.Changed.Id && !x.ActionTarget))
					{
						if(LOList.Any(x => x.Open)) {return;}
						LOList.Find(x => x.Id == e.Changed.Id).ActionTarget = true;
						Core.RenderFrame += InspectorMoveCheckBack;
						return;
					}
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
				if(LOList.Any(x => x.Id == e.ItemGuid))
				{
					LOList.RemoveAll(x => x.Id == e.ItemGuid || x.Container == e.ItemGuid);
					UpdateItemHud();
				}
								
				if(WaitingVTIOs.Any(x => x.Id == e.ItemGuid))
				{
					WaitingVTIOs.RemoveAll(x => x.Id == e.ItemGuid);
					UpdateItemHud();
				}
				
				return;
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ItemTracker_ObjectReleased(object sender, ReleaseObjectEventArgs e)
		{
			try
			{
				if(LOList.Any(x => x.Id == e.Released.Id))
				{
					LOList.RemoveAll(x => x.Id == e.Released.Id || x.Container == e.Released.Id);
					UpdateItemHud();
					return;
				}
				
				if(WaitingVTIOs.Any(x => x.Id == e.Released.Id))
				{
					WaitingVTIOs.RemoveAll(x => x.Id == e.Released.Id);
					UpdateItemHud();
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void LootContainerOpened(object sender, ContainerOpenedEventArgs e)
		{
			try
			{	
				if(Core.WorldFilter[e.ItemGuid] == null){return;}
								
				if(LOList.Any(x => x.Open))
				{
					Core.RenderFrame += OpenContainerCheckback;
					return;
				}

				LootObject lo;
				if(!LOList.Any(x => x.Id == e.ItemGuid))
				{
					lo = new LootObject(Core.WorldFilter[e.ItemGuid]);
					LOList.Add(lo);
				}
				else
				{
					lo = LOList.Find(x => x.Id == e.ItemGuid);
				}
				
				lo.ActionTarget = true;
				lo.LastActionTime = DateTime.Now;
				
				Core.RenderFrame += RenderFrame_LootContainerOpened;
				return;
								

			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void RenderFrame_LootContainerOpened(object sender, EventArgs e)
		{
			try
			{	
				if(!LOList.Any(x => x.ActionTarget))
				{
					Core.RenderFrame -= RenderFrame_LootContainerOpened;
					return;
				}
				else if((DateTime.Now - LOList.Find(x => x.ActionTarget).LastActionTime).TotalMilliseconds < 100){return;}
				else
				{
					Core.RenderFrame -= RenderFrame_LootContainerOpened;
				}
				
				LootObject container= LOList.Find(x => x.ActionTarget);
				container.ActionTarget = false;				
				
				if(container.Name.Contains(Core.CharacterFilter.Name)){container.Exclude = true; return;}

				if(container.Name.Contains("Chest") || container.Name.Contains("Vault") || 
				   container.Name.Contains("Reliquary") || container.ObjectClass == ObjectClass.Corpse)
				{
					if(container.ObjectClass == ObjectClass.Corpse){container.Exclude = true;}

					foreach(WorldObject wo in Core.WorldFilter.GetByContainer(container.Id))
					{
						if(!LOList.Any(x => x.Id == wo.Id))
						{
							LootObject lo = new LootObject(wo);
							LOList.Add(lo);
							SeparateItemsToID(lo.Id);
						}
					}	
				}
			}
			catch(Exception ex){LogError(ex);}	
		}
			
		private void SeparateItemsToID(int loId)
		{
				try
				{
					
					LootObject IOItem = LOList.Find(x => x.Id == loId);
					if(IOItem == null) {return;}
					
					
					//Flag items that need additional info to ID...
					if(!IOItem.HasIdData)
					{
						//This should remove items which require identifications to match and queue them for listening for IDs.  All other items should pass through default.
						switch(IOItem.ObjectClass)
						{
							case ObjectClass.Armor:
							case ObjectClass.Clothing:
							case ObjectClass.Jewelry:
								if(IOItem.LValue(LongValueKey.IconOutline) > 0)
								{
									IdqueueAdd(IOItem.Id);
									IOItem.Listen = true;
									return;	
								}
								break;	
							case ObjectClass.Gem:
								if(IOItem.Aetheriacheck)
								{
									IdqueueAdd(IOItem.Id);
									IOItem.Listen = true;
									return;	
								}
								break;
							case ObjectClass.Scroll:
							case ObjectClass.MeleeWeapon:
							case ObjectClass.MissileWeapon:
							case ObjectClass.WandStaffOrb:								
								IdqueueAdd(IOItem.Id);
								IOItem.Listen = true;
								return;	
							case ObjectClass.Misc:
								if(IOItem.Name.Contains("Essence"))
								{
									IdqueueAdd(IOItem.Id);
									IOItem.Listen = true;
									return;
								}
								break;								
						}
						if(!IOItem.Listen){CheckItemForMatches(IOItem.Id);}
					}
				} catch (Exception ex) {LogError(ex);} 
				return;
			}
		

		
		private void EvaluateItemMatches(int loId)
		{
			try
			{
				LootObject IOItem = LOList.Find(x => x.Id == loId);
				//Keep those duplicates out
				if(IOItem.Exclude) {return;}
				
				switch(IOItem.IOR)
				{
					case IOResult.trophy:
					case IOResult.rule:
					case IOResult.rare:
						IOItem.InspectList = true;
						break;
					case IOResult.salvage:
						IOItem.InspectList = true;
						IOItem.ProcessAction = IAction.Salvage;
						break;
					case IOResult.dessicate:
						IOItem.InspectList = true;
						IOItem.ProcessAction = IAction.Desiccate;
						break;
					case IOResult.manatank:
						IOItem.InspectList = true;
						IOItem.ProcessAction = IAction.ManaStone;
						break;
					case IOResult.spell:
						IOItem.InspectList = true;
						IOItem.ProcessAction = IAction.Read;
						break;
					case IOResult.val:
						IOItem.InspectList = true;
						if(GISettings.SalvageHighValue) {IOItem.ProcessAction = IAction.Salvage;}
						break;
				}
				if(GISettings.AutoLoot)
				{
					IOItem.Move = true;
					ToggleInspectorActions(1);
					InitiateInspectorActionSequence();
				}
				IOItem.Exclude = true;
				UpdateItemHud();
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
				
				try
				{
					if(LOList.Any(x => x.Id == id && x.InspectList))
					{
						switch(LOList.Find(x => x.Id == id).IOR)
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
				}catch(Exception ex){LogError(ex);}
				
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
			try
			{
				if(WaitingVTIOs.Count == 0) 
				{
					Core.RenderFrame -= new EventHandler<EventArgs>(DoesVTIOHaveID);
					return;
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DoesVTIOHaveID(object sender, EventArgs e)
		{
			try
			{
				if(WaitingVTIOs.Any(x => x.HasIdData == true)){WaitingVTIOs.RemoveAll(x => x.HasIdData == true);}
			}catch(Exception ex){LogError(ex);}
		}
		
		public bool VTSalvageCombineDesision(int id1, int id2)
		{
			try
			{
				return false;
			}catch(Exception ex){LogError(ex); return  false;}
		}
	}
}
