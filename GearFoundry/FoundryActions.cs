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
			Open,
			Move,
			Pick,
			Desiccate,
			Ring,
			Cut,
			Salvage,
			SalvageCombine,
			Read,
			ManaStone,
			Reveal,
			MoteCombine,
			Stack,
			Trade,
			Sell
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
							fa.ActionDelay = 300;
							break;
						case FoundryActionTypes.Magic:
							fa.ActionDelay = 600;
							break;
						case FoundryActionTypes.Portal:
							fa.ActionDelay = 1000;
							break;
					}
					FoundryActionList.Add(fa);
				}
				
				Core.CharacterFilter.SpellCast += FoundryActionsSpellCastComplete;
				Core.CharacterFilter.Logoff += FoundryActionsLogOff;
				FoundryActionTimer.Interval = 150;
				FoundryActionTimer.Tick += FoundryActionInitiator;
				
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
									FoundryCastSpell(FoundryActionList[i].ToDoList.First());
									return;
								}

						}
					}
					if(!FoundryActionList.Any(x => x.FireAction)) {TerminateFoundryActions();}
					
					
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ClearFoundryAction(int i)
		{
			FoundryActionList[i].Pending = false;
			FoundryActionList[i].FireAction = false;
			FoundryActionList[i].ActionStartTime = DateTime.MinValue;
		}
		
		private void SetFoundryAction(int i)
		{
			FoundryActionList[i].Pending = true;
			FoundryActionList[i].ActionStartTime = DateTime.Now;
		}
		
		
		

		
	}
}
