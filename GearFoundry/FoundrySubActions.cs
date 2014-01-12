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
		
		private void FoundryUseItem(int tool, int target)
		{
			try
			{
				Core.Actions.UseItem(tool, 1, target);
			}catch(Exception ex){LogError(ex);}
		}
		
		private void FoundryApplyItem(int tool, int target)
		{
			try
			{
				Core.Actions.ApplyItem(tool, target);
			}catch(Exception ex){LogError(ex);}
		}
		
		private void FoundrySalvage(int item)
		{
			try
			{
				Core.Actions.SalvagePanelAdd(item);				
				Core.Actions.SalvagePanelSalvage();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void FoundrySalvage(List<int> bags)
		{
			try
			{
				foreach(int bag in bags)
				{
					Core.Actions.SalvagePanelAdd(bag);
				}
				Core.Actions.SalvagePanelSalvage();
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
		
		private int FoundryMatchInventory(int MatchId)
		{
			try
			{
				//TODO  Finish
				return Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == Core.WorldFilter[MatchId].ObjectClass &&
				                                             x.Name.ToLower() == Core.WorldFilter[MatchId].Name.ToLower() &&
				                                             x.Id != MatchId).First().Id;
			}catch(Exception ex){LogError(ex); return 0;}
			
		}
		
		private int FoundryMatchKey(string keyname)
		{
			try
			{
					WorldObject matchedkeyring = null;
					switch(keyname.ToLower())
					{
						case "legendary key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("burning sands") && 
							                x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "black marrow key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("black marrow") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "directive key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("directive") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "granite key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("granite") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "mana forge key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("black coral") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "master key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("master") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "marble key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("marble") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "singularity key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("singularity") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "skeletal falatacot key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("skeletal falatacot") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "sturdy iron key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("sturdy iron") &&
							                 x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
							if(matchedkeyring != null) {return matchedkeyring.Id;}
							else{goto default;}
						case "sturdy steel key":
							matchedkeyring = MaidKeyRings.FirstOrDefault(x => x.Name.ToLower().Contains("sturdy steel") &&
							                x.Values(LongValueKey.UsesRemaining) < 0 && x.Values(LongValueKey.KeysHeld) < 24);
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
				

	}
}
