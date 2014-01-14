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
		private int FoundryMoveIndex = 0;
		
		internal class FoundryAction
		{
			internal bool FireAction = false;
			internal bool Pending = false;
			internal FoundryActionTypes Action = FoundryActionTypes.None;
			internal DateTime ActionStartTime = DateTime.MinValue;
			internal int ActionDelay = 0;
			internal int ActionTarget = 0; 
			internal List<int> ToDoList = new List<int>();
			internal List<List<int>> ToDoStack = new List<List<int>>();
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
			Desiccate,
			
			ManaStone,
			
			MoteCombine,
			CarvingTool,
			
			OpenUst,
			Salvage,
			SalvageCombine,
			
			
			Pick,

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
			internal int AetheriaManaStoneId = 0;
			internal int AetheriaDesiccantId = 0;
			internal List<int> EmptyManaStoneIds = new List<int>();		
		}
		
		private List<int> FoundryPackIds = new List<int>();

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
							fa.ActionDelay = 600;
							break;
						case FoundryActionTypes.Read:
							fa.ActionDelay = 850;
							break;	
						case FoundryActionTypes.Reveal:
							fa.ActionDelay = 750;
							break;
						case FoundryActionTypes.Desiccate:
							fa.ActionDelay = 750;
							break;
						case FoundryActionTypes.ManaStone:
							fa.ActionDelay = 750;
							break;
						case FoundryActionTypes.OpenUst:
							fa.ActionDelay = 300;
							break;
						case FoundryActionTypes.Salvage:
							fa.ActionDelay = 300;
							break;
					}
					FoundryActionList.Add(fa);
				}
				
				FoundryMoveIndex = FoundryActionList.FindIndex(x => x.Action == FoundryActionTypes.MoveToPack);
				
				Core.CharacterFilter.SpellCast += FoundryActionsSpellCastComplete;
				Core.CharacterFilter.Logoff += FoundryActionsLogOff;
				FoundryActionTimer.Interval = 150;
				FoundryActionTimer.Tick += FoundryActionInitiator;
				Core.WorldFilter.ChangeObject += FoundryChangeObject;				
				
				try
				{
					if(Core.WorldFilter.GetInventory().Any(x => x.Name.ToLower() == "ust"))
					{
						mFoundryToolSet.UstId = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower() == "ust").First().Id;
					}
					if(Core.WorldFilter.GetInventory().Any(x => x.Name.ToLower() == "intricate carving tool"))
					{
						mFoundryToolSet.CarvingToolId = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower() == "intricate carving tool").First().Id;
					}
					if(Core.WorldFilter.GetInventory().Any(x => x.Name.ToLower() == "aetheria mana stone"))
					{
						mFoundryToolSet.AetheriaManaStoneId = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower() == "aetheria mana stone").First().Id;
					}
					//Refresh on use below....
					if(Core.WorldFilter.GetInventory().Any(x => x.Name.ToLower() == "aetheria desiccant"))
					{
						mFoundryToolSet.AetheriaDesiccantId = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower() == "aetheria desiccant").First().Id;
					}
					if(Core.WorldFilter.GetInventory().Any(x => x.ObjectClass == ObjectClass.ManaStone && x.Values(LongValueKey.IconOutline) == 0))
					{
						foreach(WorldObject wo in Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.ManaStone && x.Values(LongValueKey.IconOutline) == 0))
						{
							mFoundryToolSet.EmptyManaStoneIds.Add(wo.Id);
						}
					}
					FoundryFillPackIDs();
				}catch(Exception ex){LogError(ex);}
										
			}catch(Exception ex){LogError(ex);}
		}
		
		private void FoundryFillPackIDs()
		{
			try
			{
				FoundryPackIds.Clear();
				FoundryPackIds.Add(Core.CharacterFilter.Id);
				foreach(WorldObject pack in Core.WorldFilter.GetByContainer(Core.CharacterFilter.Id).Where(x => x.Values(LongValueKey.Unknown10) == 56))
				{
					FoundryPackIds.Add(pack.Id);	
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeFoundryActions()
		{
			try
			{
				FoundryActionList.Clear();
				FoundryPackIds.Clear();
				Core.CharacterFilter.SpellCast -= FoundryActionsSpellCastComplete;
				Core.CharacterFilter.Logoff -= FoundryActionsLogOff;
				FoundryActionTimer.Tick -= FoundryActionInitiator;	
				Core.WorldFilter.ChangeObject -= FoundryChangeObject;				
			}catch(Exception ex){LogError(ex);}	
		}
		
		private void FoundryActionsLogOff(object sender, EventArgs e)
		{
			try
			{
				UnsubscribeFoundryActions();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void FoundryChangeObject(object sender, ChangeObjectEventArgs e)
		{
			try
			{
				if(e.Change == WorldChangeType.StorageChange && e.Changed.Values(LongValueKey.Unknown10) == 56)
				{
					FoundryFillPackIDs();
					foreach(int id in FoundryPackIds)
					{
						WriteToChat(Core.WorldFilter[id].Name + " " + id.ToString());
					}
					      
				}
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
								if(Core.Actions.CombatMode == CombatState.Peace)
								{
									ClearFoundryAction(i); 
									return;
								}
								else
								{
									SetFoundryAction(i);
									FoundryChangeCombatState(CombatState.Peace);
									return;
								}
								
							case FoundryActionTypes.ClearShield:
								if(FoundryInventoryFind("equipped shield") == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								else
								{
									SetFoundryAction(i);
									FoundryUnEquipItem(FoundryInventoryFind("equipped shield"));
									return;
								}
								
							case FoundryActionTypes.EquipWand:
								if(FoundryInventoryFind("equipped shield") != 0)
								{
									FoundryActionList.Find(x => x.Action == FoundryActionTypes.ClearShield).FireAction = true;
									return;
								}	
								if(FoundryInventoryFind("equipped wand") != 0)
								{
									ClearFoundryAction(i);
									return;
								}
								else
								{
									if(mDynamicPortalGearSettings.nOrbGuid == 0)
									{
										SelectDefaultCaster();
									}
									SetFoundryAction(i);
									FoundryEquipItem(mDynamicPortalGearSettings.nOrbGuid);
									return;
								}
								
							case FoundryActionTypes.Magic:
								if(Core.Actions.CombatMode == CombatState.Magic)
								{
									ClearFoundryAction(i);
									return;
								}
								else
								{
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
									SetFoundryAction(i);
									FoundryCastSpell(FoundryActionList[i].ToDoList[0]);
									return;
								}
								
							case FoundryActionTypes.UseLandscape:
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								else
								{
									if(FoundryActionList[i].ToDoList.Count == 0)
									{
										ClearFoundryAction(i);
										return;
									}
									if(FoundryInvalidItemCheck(i))
									{
										return;
									}
									if(Core.WorldFilter.Distance(FoundryActionList[i].ToDoList[0], Core.CharacterFilter.Id) > 0.3)
									{
										WriteToChat("Use disabled due to distance, move closer and try again.");
										FoundryActionList[i].ToDoList.RemoveAt(0);
										return;	
									}
									SetFoundryAction(i);
									FoundryUseItem(FoundryActionList[i].ToDoList[0]);
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
							
							case FoundryActionTypes.OpenContainer:
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									WriteToChat("FAL = 0");
									ClearFoundryAction(i);
									return;
								}
								if(FoundryInvalidItemCheck(i))
								{
									WriteToChat("Container invalid");
									return;
								}
								//TODO:  Carve out a move object that works off LOList and current container.
								if(Core.Actions.OpenedContainer != 0)
								{
									WriteToChat("entered container bypass");
									//Waiting on IDs, don't open and return
									if(LOList.Any(x => x.IOR == IOResult.unknown && x.Container == Core.Actions.OpenedContainer))
									{
										WriteToChat("Unknows in lolist, returned.");
										return;
									}
									//Moving stuff out of current container, this should catch it.
									if(FoundryActionList[FoundryMoveIndex].ToDoList.Count != 0)
									{
										WriteToChat("Pending moves waiting, checking");
										//TODO:  Hangs here, won't clear.
										if(FoundryActionList[FoundryMoveIndex].ToDoList.Any(x => Core.WorldFilter[x].Container == Core.Actions.OpenedContainer))
										{
											try
											{
												for(int looper =  FoundryActionList[FoundryMoveIndex].ToDoList.Count - 1; looper >= 0; looper --)
												{
													if(FoundryInventoryCheck(FoundryActionList[FoundryMoveIndex].ToDoList[looper]) || FoundryInvalidItemCheck(FoundryActionList[FoundryMoveIndex].ToDoList[0]))
													{
														FoundryActionList[FoundryMoveIndex].ToDoList.RemoveAt(looper);
														SynchWithLOList(FoundryActionList[FoundryMoveIndex].Action, FoundryActionList[FoundryMoveIndex].ToDoList[looper]);	
													}
												}
											}
											catch(Exception ex){LogError(ex);}
											WriteToChat("Passed MoveList Cleaning");
											
											if(FoundryActionList[FoundryMoveIndex].ToDoList.Count == 0) {return;}
											
											WriteToChat("Move bypass processing");
											FoundryUseItem(FoundryActionList[i].ToDoList[0]);
											return;
										}
									}
								}
								if(Core.WorldFilter.Distance(FoundryActionList[i].ToDoList[0], Core.CharacterFilter.Id) > 0.3)
								{
										WriteToChat("Open Container disabled due to distance, move closer and try again.");
										FoundryActionList[i].ToDoList.RemoveAt(0);
										return;	
								}
								if(FoundryActionList[i].ToDoList.Any(x => x == Core.Actions.OpenedContainer))
								{
									FoundryActionList[i].ToDoList.RemoveAll(x => x == Core.Actions.OpenedContainer);
									return;
								}
								WriteToChat("Should have opened container");
								SetFoundryAction(i);
								FoundryUseItem(FoundryActionList[i].ToDoList[0]);
								return;
								
							case FoundryActionTypes.MoveToPack:
								//If nothing to do, clear action
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								//If it's not a valid object, clear it
								if(Core.WorldFilter[FoundryActionList[i].ToDoList[0]] == null)
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);																							  
									return;
								}
								//If it's in your pack, clear it.  Flag for processing if needed.
								if(FoundryInventoryCheck(FoundryActionList[i].ToDoList[0]))
								{	
									SynchWithLOList(FoundryActionList[i].Action, FoundryActionList[i].ToDoList[0]);								                              		
									FoundryActionList[i].ToDoList.RemoveAll(x => Core.WorldFilter.GetInventory().Where(y => y.Id == x).Count() > 0);
									return;
								}
								//If it's not in the current container, pop that puppy open.
								if(Core.WorldFilter[FoundryActionList[i].ToDoList[0]].Container != Core.Actions.OpenedContainer)
								{
									FoundryLoadAction(FoundryActionTypes.OpenContainer, Core.WorldFilter[FoundryActionList[i].ToDoList[0]].Container);
									return;
								}
								if(Core.Actions.CombatMode != CombatState.Peace)
								{
									FoundryToggleAction(FoundryActionTypes.Peace);
									return;
								}
								
								SetFoundryAction(i);
								FoundryUseItem(FoundryActionList[i].ToDoList[0]);
								return;
								
							case FoundryActionTypes.Read:
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								//If it's not a valid object, clear it
								if(Core.WorldFilter[FoundryActionList[i].ToDoList[0]] == null)
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								//If it's not in your inventory, clear it.
								if(!FoundryInventoryCheck(FoundryActionList[i].ToDoList[0]))
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								
								WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
								SetFoundryAction(i);
								FoundryUseItem(FoundryActionList[i].ToDoList[0]);
								return;
								
							case FoundryActionTypes.Reveal:
							case FoundryActionTypes.Desiccate:
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								//If it's not a valid object, clear it
								if(Core.WorldFilter[FoundryActionList[i].ToDoList[0]] == null)
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								if(!FoundryInventoryCheck(FoundryActionList[i].ToDoList[0]))
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								
								
								if(FoundryActionList[i].Action == FoundryActionTypes.Reveal)
								{
									if(mFoundryToolSet.AetheriaManaStoneId == 0)
									{
										if(Core.WorldFilter.GetInventory().Any(x => x.Name.ToLower() == "aetheria mana stone"))
										{
											mFoundryToolSet.AetheriaManaStoneId = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower() == "aetheria mana stone").First().Id;
										}
										else
										{
											WriteToChat("Character has no Aetheria Mana Stone!  Reveal Action Disabled.");
											ClearFoundryAction(i);
											return;
										}
									}
									
									WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
									SetFoundryAction(i);
									FoundryApplyItem(mFoundryToolSet.AetheriaManaStoneId, FoundryActionList[i].ToDoList[0]);
									return;
								}
								
								if(FoundryActionList[i].Action == FoundryActionTypes.Desiccate)
								{
									if(mFoundryToolSet.AetheriaDesiccantId == 0)
									{
										if(Core.WorldFilter.GetInventory().Any(x => x.Name.ToLower() == "aetheria desiccant"))
										{
											mFoundryToolSet.AetheriaDesiccantId = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower() == "aetheria desiccant").First().Id;
										}
										else
										{
											WriteToChat("Character has no Aetheria Desiccant!  Desiccate Action Disabled.");
											ClearFoundryAction(i);
											return;
										}
									}
									SetFoundryAction(i);
									FoundryApplyItem(mFoundryToolSet.AetheriaDesiccantId, FoundryActionList[i].ToDoList[0]);
									if(Core.WorldFilter.GetInventory().Any(x => x.Name.ToLower() == "aetheria desiccant"))
									{
										mFoundryToolSet.AetheriaDesiccantId = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower() == "aetheria desiccant").First().Id;
									}
									else 
									{
										mFoundryToolSet.AetheriaDesiccantId = 0;
									}
									return;									
								}
								return;
								
							case FoundryActionTypes.ManaStone:
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								//If it's not a valid object, clear it
								if(Core.WorldFilter[FoundryActionList[i].ToDoList[0]] == null)
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								if(!FoundryInventoryCheck(FoundryActionList[i].ToDoList[0]))
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								//No mana stones, no problem.
								if(mFoundryToolSet.EmptyManaStoneIds.Count == 0)
								{
									if(Core.WorldFilter.GetInventory().Any(x => x.ObjectClass == ObjectClass.ManaStone && x.Values(LongValueKey.IconOutline) == 0))
									{
										foreach(WorldObject wo in Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.ManaStone && x.Values(LongValueKey.IconOutline) == 0))
										{
											mFoundryToolSet.EmptyManaStoneIds.Add(wo.Id);
										}
									}
									else
									{
										WriteToChat("Character has no Empty Mana Stones!  Drain Action Disabled.");
										ClearFoundryAction(i);
										return;
									}	
								}
								
								WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
								SetFoundryAction(i);
								FoundryApplyItem(mFoundryToolSet.EmptyManaStoneIds[0], FoundryActionList[i].ToDoList[0]);
								return;
								
								//Works.
							case FoundryActionTypes.MoteCombine:
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								if(Core.WorldFilter[FoundryActionList[i].ToDoList[0]] == null)
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								if(!FoundryInventoryCheck(FoundryActionList[i].ToDoList[0]))
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								if(!FoundryInventoryCheckHas2(FoundryActionList[i].ToDoList[0]))
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								
								Core.Actions.SelectItem(FoundryActionList[i].ToDoList[0]);
								FoundryMoteCombine(FoundryActionList[i].ToDoList[0]);
								FoundryActionList[i].ToDoList.RemoveAt(0);                                                   
								return;
								
								//Works
							case FoundryActionTypes.OpenUst:
								//This will stop ust opening while waiting for Ids to come back.
								if(LOList.Any(x => x.IOR == IOResult.unknown && x.Container == Core.Actions.OpenedContainer)) {return;}
								if(FoundryChestCheck(Core.Actions.OpenedContainer)) {return;} 
								
								if(!FoundryActionList.Find(x => x.Action == FoundryActionTypes.Salvage).FireAction &&
								   !FoundryActionList.Find(x => x.Action == FoundryActionTypes.SalvageCombine).FireAction)
								{
									ClearFoundryAction(i);
									return;
								}
								if(mFoundryToolSet.UstId == 0)
								{
									ClearFoundryAction(i);
									ClearFoundryAction(FoundryActionList.FindIndex(x => x.Action == FoundryActionTypes.Salvage));
									ClearFoundryAction(FoundryActionList.FindIndex(x => x.Action == FoundryActionTypes.SalvageCombine));
									WriteToChat("Character has no ust!  Salvaging disabled.");
								}
								   
//								WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
								FoundryUseItem(mFoundryToolSet.UstId);
								ClearFoundryAction(i);
								return;
								
								//Works
							case FoundryActionTypes.Salvage:
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								//Remove the item if it's not in invenotry
								if(!FoundryInventoryCheck(FoundryActionList[i].ToDoList[0]))
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								
//								WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
								SetFoundryAction(i);
								FoundrySalvage(FoundryActionList[i].ToDoList[0]);
								return;
								
								//Works
							case FoundryActionTypes.SalvageCombine:
								//Exit if nothing to combine
								//Move created salvages to ToDoList
								//Pick savlage types at time of combining, not prior as multiple salvage bags can get scopped into the lists and not cleared.
								WriteToChat("Salvage Combine");
								if(FoundryActionList[i].ToDoList.Count == 0)
								{
									ClearFoundryAction(i);
									return;
								}
								if(!FoundryInventoryCheck(FoundryActionList[i].ToDoList[0]))
								{
									FoundryActionList[i].ToDoList.RemoveAt(0);
									return;
								}
								
								//Take the first item from the todo list and make a combine list of ints out of it.  Clear it from the todo list.
								FoundryActionList[i].ToDoStack.Add(InspectorPickSalvage(Core.WorldFilter[FoundryActionList[i].ToDoList[0]]));
								FoundryActionList[i].ToDoList.RemoveAt(0);
								
								//If there's one bag, we can't combine it with anything.  Clear and return.
								if(FoundryActionList[i].ToDoStack[0].Count == 1)
								{
									FoundryActionList[i].ToDoStack.RemoveAt(0);
									return;
								}
								
//								WriteToChat(DateTime.Now.ToShortTimeString() + " " + FoundryActionList[i].Action.ToString());
								SetFoundryAction(i);
								FoundrySalvage(FoundryActionList[i].ToDoStack[0]);
								FoundryActionList[i].ToDoStack.RemoveAt(0);
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
				if(Core.WorldFilter[id] == null) {return false;}
				if(FoundryPackIds.Contains(Core.WorldFilter[id].Container)){return true;}
				return false;
			}catch(Exception ex){LogError(ex); return false;}
		}
		
		private bool FoundryInventoryCheck(List<int> Ids, int i)
		{
			try
			{	
				foreach(int id in Ids)
				{
					if(!FoundryInventoryCheck(id)) {FoundryActionList[i].ToDoStack[0].Remove(id);}
				}
				
				if(FoundryActionList[i].ToDoStack[0].Count > 0) {return true;}
				else 
				{
					FoundryActionList[i].ToDoStack[0].RemoveAt(0);
					return false;
				}
			}catch(Exception ex){LogError(ex); return false;}
		}
		
		private bool FoundryInventoryCheckHas2(int id)
		{
			try
			{
				if(Core.WorldFilter.GetInventory().Where(x => x.Name == Core.WorldFilter[id].Name).Count() > 1) {return true;}
				else {return false;}
			}catch(Exception ex){LogError(ex); return false;}
		}
		
		private void FoundryLoadAction(FoundryActionTypes action, int Id)
		{
			try
			{
				if(action == FoundryActionTypes.Salvage || action == FoundryActionTypes.SalvageCombine)
				{
					if(!FoundryActionList.Find(x => x.Action == FoundryActionTypes.OpenUst).FireAction)
					{
						FoundryToggleAction(FoundryActionTypes.OpenUst);
					}
				}
				
				int index =  FoundryActionList.FindIndex(x => x.Action == action);
				FoundryActionList[index].FireAction = true;
				FoundryActionList[index].ToDoList.Add(Id);
			}catch(Exception ex){LogError(ex);}
		}
		
		private void FoundryLoadAction(FoundryActionTypes action, List<int> Ids)
		{
			try
			{
				if(action == FoundryActionTypes.Salvage || action == FoundryActionTypes.SalvageCombine)
				{
					FoundryToggleAction(FoundryActionTypes.OpenUst);
				}
				
				int index =  FoundryActionList.FindIndex(x => x.Action == action);
				FoundryActionList[index].FireAction = true;
				FoundryActionList[index].ToDoStack.Add(Ids);
			}catch(Exception ex){LogError(ex);}
		}
		
		private int FoundryInventoryFind(string item)
		{
			try
			{
				switch(item)
				{
					case "equipped shield":
						 return Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Armor && x.Values(LongValueKey.EquippedSlots) == 0x200000).First().Id;
					case "equipped wand":
						 return Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) == 0x1000000).First().Id;
						default:
						 return 0;
						
						
				}
				
			}catch(Exception ex){LogError(ex); return 0;}
		}
		
		private bool FoundryInvalidItemCheck(int i)
		{
			try
			{	
				if(Core.WorldFilter[FoundryActionList[i].ToDoList[0]] == null)
				{
					FoundryActionList[i].ToDoList.RemoveAt(0);
					return true;
				}
				else return false;
			}catch(Exception ex){LogError(ex); return false;}
		}
		
		

	}
}
