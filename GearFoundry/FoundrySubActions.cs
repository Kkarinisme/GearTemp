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
		private void FoundryChangeCombatState(CombatState RequiredState)
		{
			try
			{
				//If entering Magic mode, must have a caster equipped.  If no caster, exit this action and equip a wand.
				if(RequiredState == CombatState.Magic && Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) == 0x1000000).Count() == 0)
				{
					FoundryActionList.Find(x => x.Action == FoundryActionTypes.EquipWand).FireAction = true;
					return;
				}
				
				if(Core.Actions.CombatMode != RequiredState)
				{
					Core.Actions.SetCombatMode(RequiredState);
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void FoundryUnEquipItem(int Id)
		{
			try
			{
				if(Core.WorldFilter[Id].Values(LongValueKey.EquippedSlots) != 0)
				{
					Core.Actions.UseItem(Id,0);
				}	
			}catch(Exception ex){LogError(ex);}
		}
		
		private void FoundryEquipItem(int Id)
		{
			try
			{
				if(Core.WorldFilter[Id].Values(LongValueKey.EquippedSlots) == 0)
				{
					Core.Actions.UseItem(Id, 0);
				}  
			}catch(Exception ex){LogError(ex);}
		}
		
		private void FoundryCastSpell(int SpellId)
		{
			try
			{
				if(Core.Actions.CombatMode != CombatState.Magic)
				{
					FoundryActionList.Find(x => x.Action == FoundryActionTypes.Magic).FireAction = true;
					return;
				}
				Core.Actions.CastSpell(SpellId, Core.Actions.CurrentSelection);
			}catch(Exception ex){LogError(ex);}
		}
		
		private void FoundryActionsSpellCastComplete(object sender, SpellCastEventArgs e)
		{
			try
			{
				if(!FoundryActionsPending) {return;}
				if(FoundryActionList.Any(x => x.Action == FoundryActionTypes.Portal && x.ToDoList.Contains(e.SpellId)))
				{
					FoundryActionList.Find(x => x.Action == FoundryActionTypes.Portal && x.ToDoList.Contains(e.SpellId)).ToDoList.RemoveAll(x => x == e.SpellId);
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void FoundryUseItem(int Id)
		{
			try
			{
				Core.Actions.UseItem(Id,0);
				return;
			}catch(Exception ex){LogError(ex);}
		}
		
		private void FoundrySalvgeAdd(List<int> salvlist)
		{
			try
			{
				foreach(int item in salvlist)
				{
					Core.Actions.SalvagePanelAdd(item);
				}
								
				Core.Actions.SalvagePanelSalvage();
			}catch(Exception ex){LogError(ex);}
		}
		
			
		private void FoundryLoadAction(FoundryActionTypes action, int Id)
		{
			try
			{
				int index =  FoundryActionList.FindIndex(x => x.Action == action);
				FoundryActionList[index].FireAction = true;
				FoundryActionList[index].ToDoList.Add(Id);
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private bool FoundryChestCheck(int ChestID)
		{
			try
			{
				if(ChestID == 0) {return false;}
				
				string containername = Core.WorldFilter[ChestID].Name;
				
				if(containername.Contains("Chest") || containername.Contains("Vault") ||  containername.Contains("Reliquary")) {return true;}
				else{return false;}
				
			}catch(Exception ex){LogError(ex);return false;}
		}
		
		private void FoundryCraftItem(int ToolId, int TargetId)
		{
			
		}
				

	}
}
