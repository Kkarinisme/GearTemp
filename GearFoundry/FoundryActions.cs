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
		private List<FoundryAction> FoundryActionList = new List<FoundryAction>();
		private System.Windows.Forms.Timer FoundryActionTimer = new System.Windows.Forms.Timer();
		private bool FoundryActionsPending = false;
		private FoundryCraftTools mFoundryToolSet = new FoundryCraftTools();
		
		internal class FoundryAction
		{
			internal bool FireAction = false;
			internal bool Pending = false;
			internal FoundryActionTypes Action = FoundryActionTypes.None;
			internal DateTime ActionStartTime = DateTime.MinValue;
			internal int ActionDelay = 0;
			internal int ActionTarget = 0; 
			internal List<int> ToDoList = new List<int>();
		}
				
		internal enum FoundryActionTypes
		{
			None,
			Peace,
			ClearShield,
			EquipWand,
			Magic,
			Portal,
			UseLandscape,
			OpenContainer,
			MoveToPack,
			Read,
			Reveal,
			
			ManaStone,
			Dessicate,
			
			MoteCombine,
			
			OpenUst,
			Salvage,
			SalvageCombine,
			
			
			Move,
			Pick,
			Desiccate,
			Ring,
			Cut,
			

			Stack,
			Trade,
			Sell
		}
		
		internal class FoundryCraftTools
		{
			internal int UstId = 0;
			internal int CarvingToolId = 0; 
		
			
		}

		private void SubscribeFoundryActions()
		{
			try
			{
				foreach(FoundryActionTypes act in Enum.GetValues(typeof(FoundryActionTypes)))
				{
					FoundryAction fa = new FoundryAction();
					fa.Action = act;
					switch(fa.Action)
					{
						case FoundryActionTypes.None:
							fa.ActionDelay = 0;
							break;
						case FoundryActionTypes.Peace:
							fa.ActionDelay = 600;
							break;
						case FoundryActionTypes.ClearShield:
							fa.ActionDelay = 300;
							break;
						case FoundryActionTypes.EquipWand:
							fa.ActionDelay = 600;
							break;
						case FoundryActionTypes.Magic:
							fa.ActionDelay = 600;
							break;
						case FoundryActionTypes.Portal:
							fa.ActionDelay = 1000;
							break;
						case FoundryActionTypes.UseLandscape:
							fa.ActionDelay = 1000;
							break;
						case FoundryActionTypes.OpenContainer:
							fa.ActionDelay = 1000;
							break;
						case FoundryActionTypes.MoveToPack:
							fa.ActionDelay = 450;
							break;
						case FoundryActionTypes.Read:
							fa.ActionDelay = 850;
							break;	
						case FoundryActionTypes.OpenUst:
							fa.ActionDelay = 100;
							break;
						case FoundryActionTypes.Salvage:
							fa.ActionDelay = 300;
							break;
					}
					FoundryActionList.Add(fa);
				}
				
				Core.CharacterFilter.SpellCast += FoundryActionsSpellCastComplete;
				Core.CharacterFilter.Logoff += FoundryActionsLogOff;
				FoundryActionTimer.Interval = 150;
				FoundryActionTimer.Tick += FoundryActionInitiator;	
				
				try
				{
					mFoundryToolSet.UstId = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower() == "ust").First().Id;
				}catch(Exception ex){LogError(ex);}
				try
				{
					
				}catch(Exception ex){LogError(ex);}


								
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeFoundryActions()
		{
			try
			{
				FoundryActionList.Clear();
				Core.CharacterFilter.SpellCast -= FoundryActionsSpellCastComplete;
				Core.CharacterFilter.Logoff -= FoundryActionsLogOff;
				FoundryActionTimer.Tick -= FoundryActionInitiator;				
			}catch(Exception ex){LogError(ex);}	
		}
		
		private void FoundryActionsLogOff(object sender, EventArgs e)
		{
			try
			{
				UnsubscribeFoundryActions();
			}catch(Exception ex){LogError(ex);}
		}
		
		
		
		private void InitiateFoundryActions()
		{
			try
			{
				if(!FoundryActionsPending)
				{
					FoundryActionsPending = true;
					FoundryActionTimer.Start();
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void TerminateFoundryActions()
		{
			try
			{
				if(!FoundryActionsPending) {return;}
				FoundryActionsPending = false;
				FoundryActionTimer.Stop();
			}catch(Exception ex){LogError (ex);}
		}
		
		private void FoundryActionInitiator(object sender, EventArgs e)
		{
			FireFoundryActions();
		}
		
		private void FireFoundryActions()
		{
			try
			{
				for(int i = 0; i < FoundryActionList.Count; i++)
				{
					if(FoundryActionList[i].FireAction)
					{
						if((DateTime.Now - FoundryActionList[i].ActionStartTime).TotalMilliseconds < FoundryActionList[i].ActionDelay) {return;}
						
						switch(FoundryActionList[i].Action)
						{
							case FoundryActionTypes.None:
								return;
							case FoundryActionTypes.Peace:
								//If in peace mode, ignore
								if(Core.Actions.CombatMode == CombatState.Peace)
								{
									ClearFoundryAction(i);
									return;
								}
								//Peace out baby
								else
								{
									WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
									SetFoundryAction(i);
									FoundryChangeCombatState(CombatState.Peace);
									return;
								}
								
							case FoundryActionTypes.ClearShield:
								//You don't have to clear a shield you don't have
								if(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Armor && x.Values(LongValueKey.EquippedSlots) == 0x200000).Count() == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								//Remove the shield you're wearing
								else
								{
									WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
									SetFoundryAction(i);
									FoundryUnEquipItem(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Armor && x.Values(LongValueKey.EquippedSlots) == 0x200000).First().Id);
									return;
								}
								
							case FoundryActionTypes.EquipWand:
								//You can't equip a wand if you're wearing a shield.  Clear the shield, then try again.
								if(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Armor && x.Values(LongValueKey.EquippedSlots) == 0x200000).Count() != 0)
								{
									FoundryActionList.Find(x => x.Action == FoundryActionTypes.ClearShield).FireAction = true;
									return;
								}	
								//If you've already got a wand in hand, clear the action
								if(Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) == 0x1000000).Count() > 0)
								{
									ClearFoundryAction(i);
									return;
								}
								else
								{
									//If you don't have a caster, make it select one from portal gear
									if(mDynamicPortalGearSettings.nOrbGuid == 0)
									{
										SelectDefaultCaster();
									}
									//Equip that caster
									WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
									SetFoundryAction(i);
									FoundryEquipItem(mDynamicPortalGearSettings.nOrbGuid);
									return;
								}
								
							case FoundryActionTypes.Magic:
								//If in magic mode, clear the action
								if(Core.Actions.CombatMode == CombatState.Magic)
								{
									ClearFoundryAction(i);
									return;
								}
								//Set magic mode
								else
								{
									WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
									SetFoundryAction(i);
									FoundryChangeCombatState(CombatState.Magic);
									return;
								}
							
							case FoundryActionTypes.Portal:
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								else
								{
									WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
									SetFoundryAction(i);
									FoundryCastSpell(FoundryActionList[i].ToDoList.First());
									return;
								}
								
							case FoundryActionTypes.UseLandscape:
								//If nothing to use, disable
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								//Attempt one use, then dump the item from the list
								else
								{
									if(FoundryActionList[i].ToDoList.Count == 0)
									{
										ClearFoundryAction(i);
										return;
									}
									if(Core.WorldFilter[FoundryActionList[i].ToDoList.First()] == null)
									{
										FoundryActionList[i].ToDoList.RemoveAt(0);
										return;
									}
									if(Core.WorldFilter.Distance(FoundryActionList[i].ToDoList.First(), Core.CharacterFilter.Id) > 0.3)
									{
										WriteToChat("Use disabled due to distance, move closer and try again.");
										FoundryActionList[i].ToDoList.RemoveAt(0);
										return;	
									}
									WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
									SetFoundryAction(i);
									FoundryUseItem(FoundryActionList[i].ToDoList.First());
									if(FoundryActionList[i].ToDoList.Count > 0)
									{
										FoundryActionList[i].ToDoList.RemoveAt(0);
									}
									return;
								}
							
							case FoundryActionTypes.OpenContainer:
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								if(Core.WorldFilter[FoundryActionList[i].ToDoList.First()] == null)
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								if(Core.WorldFilter.Distance(FoundryActionList[i].ToDoList.First(), Core.CharacterFilter.Id) > 0.3)
								{
										WriteToChat("Open Container disabled due to distance, move closer and try again.");
										FoundryActionList[i].ToDoList.RemoveAt(0);
										return;	
								}
								if(FoundryActionList[i].ToDoList.Any(x => x == Core.Actions.OpenedContainer))
								{
									FoundryActionList[i].ToDoList.RemoveAll(x => x == Core.Actions.OpenedContainer);
									ClearFoundryAction(i);
									return;
								}
		
								SetFoundryAction(i);
								FoundryUseItem(FoundryActionList[i].ToDoList.First());
								return;
								
							case FoundryActionTypes.MoveToPack:
								//If nothing to do, clear action
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								//If it's not a valid object, clear it
								if(Core.WorldFilter[FoundryActionList[i].ToDoList.First()] == null)
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								//If it's in your pack, clear it.  Flag for processing if needed.
								if(FoundryInventoryCheck(FoundryActionList[i].ToDoList.First()))
								{			
																	
									//check to see if it's coming from the looter
									int loIndex = LOList.FindIndex(x => x.Id == FoundryActionList[i].ToDoList.First());
									if(loIndex >= 0)
									{
										//If it has some process action, load it.
										if(LOList[loIndex].FoundryProcess != FoundryActionTypes.None)
										{
											FoundryLoadAction(LOList[loIndex].FoundryProcess, LOList[loIndex].Id);
											if(LOList[loIndex].FoundryProcess == FoundryActionTypes.Salvage || LOList[loIndex].FoundryProcess == FoundryActionTypes.SalvageCombine)
											{
												FoundryToggleAction(FoundryActionTypes.OpenUst);
											}
										}
									}
									                              		
									FoundryActionList[i].ToDoList.RemoveAll(x => Core.WorldFilter.GetInventory().Where(y => y.Id == x).Count() > 0);
									return;
								}
								//If it's not in the current container, pop that puppy open.
								if(Core.WorldFilter[FoundryActionList[i].ToDoList.First()].Container != Core.Actions.OpenedContainer)
								{
									FoundryLoadAction(FoundryActionTypes.OpenContainer, Core.WorldFilter[FoundryActionList[i].ToDoList.First()].Container);
									return;
								}
								if(Core.Actions.CombatMode != CombatState.Peace)
								{
									FoundryToggleAction(FoundryActionTypes.Peace);
									return;
								}
								
								WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
								SetFoundryAction(i);
								FoundryUseItem(FoundryActionList[i].ToDoList.First());
								return;
								
							case FoundryActionTypes.Read:
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								//If it's not a valid object, clear it
								if(Core.WorldFilter[FoundryActionList[i].ToDoList.First()] == null)
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								//If it's not in your inventory, clear it.
								if(!FoundryInventoryCheck(FoundryActionList[i].ToDoList.First()))
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								
								WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
								SetFoundryAction(i);
								FoundryUseItem(FoundryActionList[i].ToDoList.First());
								return;
								
							case FoundryActionTypes.Reveal:
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								//If it's not a valid object, clear it
								if(Core.WorldFilter[FoundryActionList[i].ToDoList.First()] == null)
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								if(!FoundryInventoryCheck(FoundryActionList[i].ToDoList.First()))
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								
								WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
								SetFoundryAction(i);
								FoundryUseItem(FoundryActionList[i].ToDoList.First());
								return;
								
							case FoundryActionTypes.OpenUst:
								//Wait for IDs to come back before opening ust.
								if(FoundryChestCheck(Core.Actions.OpenedContainer))
								{
									if(LOList.Any(x => x.Container == Core.Actions.OpenedContainer && x.IOR == IOResult.unknown))
									{
										return;
									}
								}
								if(Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower() == "ust").Count() == 0)
								{
									WriteToChat("Character has no Ust!  All ust actions disabled.");
									ClearFoundryAction(i);
									ClearFoundryAction(FoundryActionList.FindIndex(x => x.Action == FoundryActionTypes.Salvage));
									ClearFoundryAction(FoundryActionList.FindIndex(x => x.Action == FoundryActionTypes.SalvageCombine));
									return;
								}
								
								//if(Core.Actions.Underlying.
								WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
								FoundryUseItem(Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower() == "ust").First().Id);
								//TODO:  Remove this after you figure out how to see where the salvage panel is.  It's weaksauce.
								ClearFoundryAction(i);
								return;
								
							case FoundryActionTypes.Salvage:
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								//Remove the item if it's not in invenotry
								if(!FoundryInventoryCheck(FoundryActionList[i].ToDoList.First()))
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
								}
								
								WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
								SetFoundryAction(i);
								List<int> salv = new List<int>();
								salv.Add(FoundryActionList[i].ToDoList.First());
								FoundrySalvgeAdd(salv);
								return;
								

						}
					}
					if(!FoundryActionList.Any(x => x.FireAction)) {TerminateFoundryActions();}
					
					
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ClearFoundryAction(int i)
		{
			FoundryActionList[i].FireAction = false;
			FoundryActionList[i].ActionStartTime = DateTime.MinValue;
		}
		
		private void SetFoundryAction(int i)
		{
			FoundryActionList[i].ActionStartTime = DateTime.Now;
		}
		
		private void FoundryToggleAction(FoundryActionTypes action)
		{
			FoundryActionList.Find(x => x.Action == action).FireAction = true;
		}
		
		private bool FoundryInventoryCheck(int id)
		{
			try
			{
				if(Core.WorldFilter.GetInventory().Any(x => x.Id == id)) {return true;}
				else {return false;}
			}catch(Exception ex){LogError(ex); return false;}
		}
		
		
		
		

		
	}
}
