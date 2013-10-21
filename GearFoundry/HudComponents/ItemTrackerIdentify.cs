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
using System.Xml.Linq;

namespace GearFoundry
{
	/// <summary>
	/// Description of ItemTrackerIdentify.
	/// </summary>
	public partial class PluginCore
	{
		//Item Tracker Manual ID functions begin here
		
		private List<ItemRule> ItemRulesList = new List<ItemRule>();	 
		private List<SalvageRule> SalvageRulesList = new List<SalvageRule>();
		
		
		private int LastReportGUID = 0;
		private void ManualCheckItemForMatches(LootObject IOItem)
		{
			try
			{
				if(IOItem.Id == LastReportGUID) {return;}
				else{LastReportGUID = IOItem.Id;}
		
				if(IOItem.ObjectClass == ObjectClass.Scroll){CheckUnknownScrolls(ref IOItem);}	
				if(IOItem.IOR == IOResult.unknown){CheckRare(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {TrophyListCheckItem(ref IOItem);}
				if(IOItem.HasIdData && IOItem.IOR == IOResult.unknown){CheckRulesItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown && GISettings.IdentifySalvage) {CheckSalvageItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckManaItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckValueItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {IOItem.IOR = IOResult.nomatch;}
				
				if(GISettings.GSStrings) {ReportStringToChat(IOItem.GSReportString());}
				if(GISettings.AlincoStrings){ReportStringToChat(IOItem.LinkString());}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CheckItemForMatches(int loId)
		{
			try
			{
				LootObject IOItem = LOList.Find(x => x.Id == loId);
				
				if(IOItem.ObjectClass == ObjectClass.Scroll){CheckUnknownScrolls(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown){CheckRare(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {TrophyListCheckItem(ref IOItem);}
				if(IOItem.HasIdData){CheckRulesItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown && GISettings.IdentifySalvage) {CheckSalvageItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckManaItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckValueItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {IOItem.IOR = IOResult.nomatch;}
				
				if(IOItem.IOR == IOResult.rule){playSoundFromResource(mSoundsSettings.CorpseRule);}
				if(IOItem.IOR == IOResult.salvage){playSoundFromResource(mSoundsSettings.CorpseSalvage);}
				if(IOItem.IOR == IOResult.trophy){playSoundFromResource(mSoundsSettings.CorpseTrophy);}
				
				if(MaidCannibalizeProcessList.Count > 0 && MaidCannibalizeProcessList.Contains(IOItem.Id))
				{
					MaidCannibalizeProcessList.RemoveAll(x => x == IOItem.Id);
					EvaluateCannibalizeMatches(IOItem.Id);
					return;
				}
				
				if(IOItem.IOR == IOResult.nomatch)
				{
					return;
				}
				else
				{
					if(GISettings.GSStrings) {ReportStringToChat(IOItem.GSReportString());}
					if(GISettings.AlincoStrings){ReportStringToChat(IOItem.LinkString());}
					EvaluateItemMatches(IOItem.Id);
				}
								
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CheckRare(ref LootObject IOItemRare)
		{
			try
			{
				if(IOItemRare.LValue(LongValueKey.RareId) > 0)
				{
					IOItemRare.IOR = IOResult.rare;
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		// Item Tracker ID functions begin here
		private void CheckSalvageItem(ref LootObject IOItemSalvage)
		{
			try
			{
				LootObject IoItemSalvageMirror = IOItemSalvage;
				
				if(IOItemSalvage.DValue(DoubleValueKey.SalvageWorkmanship) > 0)
				{
					var salvagerulecheck = from allrules in SalvageRulesList
						where (allrules.material == IoItemSalvageMirror.LValue(LongValueKey.Material)) &&
												(IoItemSalvageMirror.DValue(DoubleValueKey.SalvageWorkmanship) >= allrules.minwork) &&
												(IoItemSalvageMirror.DValue(DoubleValueKey.SalvageWorkmanship) <= (allrules.maxwork +0.99))
										select allrules;
					
					if(salvagerulecheck.Count() > 0)
					{
						IOItemSalvage.IOR = IOResult.salvage;
						IOItemSalvage.rulename = salvagerulecheck.First().ruleid;
					}
				}
				
				if(IOItemSalvage.Aetheriacheck && GISettings.AutoDessicate)
				{
					IOItemSalvage.IOR = IOResult.dessicate;
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CheckManaItem(ref LootObject IOItemMana)
		{
			try
			{
				if(GISettings.LootByMana == 0){return;}
				if(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.ManaStone && x.Values(LongValueKey.IconOutline) == 0).Count() == 0){return;}
				if(IOItemMana.LValue(LongValueKey.CurrentMana) > GISettings.LootByMana)
				{
					IOItemMana.IOR = IOResult.manatank;
				}
			} catch(Exception ex){LogError(ex);}
		}
		
		private void CheckValueItem(ref LootObject IOItemVal)
		{
			try
			{
				if(GISettings.LootByValue == 0){return;}
				
				if(IOItemVal.LValue(LongValueKey.Value) >= GISettings.LootByValue)
				{
					if(GISettings.SalvageHighValue)
					{
						IOItemVal.IOR = IOResult.salvage;
						IOItemVal.rulename = "Value";
					}
					else
					{
						IOItemVal.IOR = IOResult.val;
					}
				}
			} catch(Exception ex){LogError(ex);}
		}
		
		private void CheckUnknownScrolls(ref LootObject IOScroll)
		{
			try
			{
				
//				    			try
//    			{
//    				Decal.Interop.Filters.SkillInfo lockpickinfo = Core.CharacterFilter.Underlying.get_Skill((Decal.Interop.Filters.eSkillID)0x17);
//    			
//	    			if(lockpickinfo.Training.ToString() == "eTrainSpecialized" || lockpickinfo.Training.ToString() == "eTrainTrained")
//	    			{
//	    				ButlerHudPickCurrentSelection = new HudButton();
//	    				ButlerHudPickCurrentSelection.Text = "Pick";
//	    				ButlerHudTabLayout.AddControl(ButlerHudPickCurrentSelection, new Rectangle(5,30,50,20));
//	    			}
//    			}catch(Exception ex){LogError(ex);}

				//Decal.Interop.Filters.SkillInfo warinfo = Core.CharacterFilter.Underlying.get_Skill(Decal.Interop.Filters.eSkillID)
				
				if(GISettings.CheckForL7Scrolls && SpellIndex[IOScroll.Spell(0)].spelllevel == 7)
				{	
					if(!Core.CharacterFilter.IsSpellKnown(IOScroll.Spell(0)))
					{
						IOScroll.IOR = IOResult.spell;
					}
					else
					{
						IOScroll.IOR = IOResult.nomatch;
						return;
					}
				}
				else
				{
					IOScroll.IOR = IOResult.nomatch;
					return;
				}	
			} catch(Exception ex){LogError(ex);}
			return;
		}
				
		private void TrophyListCheckItem(ref LootObject IOItem)
		{	
			try
			{
				string namecheck = IOItem.Name;
				List<XElement> matches;
				
				var exact = from XTrophies in mSortedTrophiesList
					where XTrophies.Element("checked").Value == "true" && 
					XTrophies.Element("isexact").Value == "true"
					select XTrophies;
				
				matches = (from exTrophies in exact
					where (string)@exTrophies.Element("key").Value == @namecheck
					select exTrophies).ToList();
				
				if(IOItem.ObjectClass == ObjectClass.Scroll)
				{
					if(matches.Count() == 0){return;}
				}
				
				if(matches.Count() == 0)
				{
					var notexacttrophies = from XTrophies in mSortedTrophiesList
					where XTrophies.Element("checked").Value == "true" && 
					XTrophies.Element("isexact").Value == "false"
					select XTrophies;
					
					matches = (from nxTrophies in notexacttrophies
						where @namecheck.ToLower().Contains((string)@nxTrophies.Element("key").Value.ToLower())
						select nxTrophies).ToList();
				}

				if(matches.Count() > 0)
				{
					int LootMaxCheck;
					if(!Int32.TryParse(matches.First().Element("Guid").Value, out LootMaxCheck)) {LootMaxCheck = 0;}
					
					int InventoryCount = 0;
					if(Convert.ToBoolean(matches.First().Element("isexact").Value))
					{
						var inventorymatches = Core.WorldFilter.GetInventory().Where(x => x.Name == (string)@matches.First().Element("key").Value);
						if(inventorymatches.Count() == 0)
						{
							InventoryCount = 0;
						}
						else foreach(WorldObject inv in inventorymatches)
						{
							if(!inv.LongKeys.Contains((int)LongValueKey.StackCount)){InventoryCount++;}
							else{InventoryCount += inv.Values(LongValueKey.StackCount);}
						}
						
					}
					else
					{
						var inventorymatches =  Core.WorldFilter.GetInventory().Where(x => x.Name.Contains((string)@matches.First().Element("key").Value));
						if(inventorymatches.Count() == 0)
						{
							InventoryCount = 0;
						}
						else foreach(WorldObject inv in inventorymatches)
						{
							if(!inv.LongKeys.Contains((int)LongValueKey.StackCount)){InventoryCount++;}
							else{InventoryCount += inv.Values(LongValueKey.StackCount);}
						}
					}					
					if(LootMaxCheck > 0 && InventoryCount >= LootMaxCheck) 
					{
						return;
					}
			
					IOItem.IOR = IOResult.trophy;
				}				
			} catch(Exception ex){LogError(ex);}
			return;
		}
		
		private void writerulestochat(string stage, List<ItemRule> listorules)
		{
			WriteToChat(stage);
			foreach(var rule in listorules)
			{
				WriteToChat(rule.RuleName);
			}
		}
		
		private void CheckRulesItem(ref LootObject IOItemWithIDReference)
		{
			
			//Irq:  Note to self:  Cloak IDs....cloaks w/spells are 352 = 1;  cloaks w/absorb are 352=2
			try
			{
				
				LootObject IOItemWithID = IOItemWithIDReference;
				
				List<ItemRule> AppliesToListMatches = (from appliesto in ItemRulesList
					where (appliesto.RuleAppliesTo & IOItemWithID.LValue(LongValueKey.Category)) == IOItemWithID.LValue(LongValueKey.Category)
					select appliesto).ToList();
				
				if(AppliesToListMatches.Count == 0) {return;}	
				
				List<ItemRule> SlotListMatches = (from slots in AppliesToListMatches
					where slots.RuleSlots == 0 || (IOItemWithID.LValue(LongValueKey.EquipableSlots) & slots.RuleSlots) != 0
					select slots).ToList();
				
				if(SlotListMatches.Count == 0) {return;}
				
			    List<ItemRule> SetMatches = (from sets in SlotListMatches
					where sets.RuleArmorSet.Count == 0 || sets.RuleArmorSet.Contains(IOItemWithID.LValue(LongValueKey.ArmorSet))
					select sets).ToList();
				
				if(SetMatches.Count == 0) {return;}
								
				List<ItemRule> PropertyListMatches = (from properties in SetMatches
					where (properties.RuleArcaneLore == -1 || IOItemWithID.LValue(LongValueKey.LoreRequirement) <= properties.RuleArcaneLore) &&
						  (properties.RuleWork == -1 || IOItemWithID.LValue(LongValueKey.Workmanship) <= properties.RuleWork) &&
						  (properties.RuleWieldLevel == -1 || 
					       (IOItemWithID.LValue(LongValueKey.WieldReqType) != 7 &&  IOItemWithID.LValue((LongValueKey)NewLongKeys.WieldReqType2) != 7) ||
					 	   (IOItemWithID.LValue(LongValueKey.WieldReqType) == 7 && IOItemWithID.LValue(LongValueKey.WieldReqValue) <= properties.RuleWieldLevel) ||
					 	   (IOItemWithID.LValue((LongValueKey)NewLongKeys.WieldReqType2) == 7 && IOItemWithID.LValue((LongValueKey)NewLongKeys.WieldReqValue2) <= properties.RuleWieldLevel))
					select properties).ToList();
				
				if(PropertyListMatches.Count == 0) {return;}
				
				List<ItemRule> AdvancedMatches = (from advancedmatches in PropertyListMatches
				               where !advancedmatches.AdvSettings || MatchAdvanced(advancedmatches, IOItemWithID)
				               select advancedmatches).ToList();
				
			
				List<ItemRule> SpellListMatches = (from spellmatches in AdvancedMatches
					where spellmatches.RuleSpellNumber == -1 || spellmatches.RuleSpells.Count == 0 || 
					IOItemWithID.SpellsOnItem.Intersect(spellmatches.RuleSpells).Count() >= spellmatches.RuleSpellNumber
					select spellmatches).ToList();
				
				if(SpellListMatches.Count == 0) {return;}
								
				switch(IOItemWithID.ObjectClass)
				{		

					case ObjectClass.Gem:
							if(!IOItemWithID.Aetheriacheck) {return;}
							
						List<ItemRule> reducedaetheriamatches = (from ruls in SpellListMatches
							where (ruls.GearScore == -1 || IOItemWithID.GearScore >= ruls.GearScore) 
							orderby ruls.RulePriority
							select ruls).ToList();
						
							if(reducedaetheriamatches.Count > 0)
							{
								IOItemWithID.rulename = reducedaetheriamatches.First().RuleName; 
								IOItemWithID.IOR = IOResult.rule; 
								return;
							}
							else
							{
								return;
							}

					case ObjectClass.Jewelry:  //Jewelry doesn't have gear scores at this time.	
							List<ItemRule> reducedjewelrymatches = (from ruls in SpellListMatches
									where (ruls.GearScore == -1 || IOItemWithID.GearScore >= ruls.GearScore) 
									orderby ruls.RulePriority
									select ruls).ToList();
							
							if(reducedjewelrymatches.Count > 0)
							{
								IOItemWithID.rulename = reducedjewelrymatches.First().RuleName; 
								IOItemWithID.IOR = IOResult.rule; 
								return;
							}
							else
							{
								return;
							}
						
					case ObjectClass.Armor:
						List<ItemRule> reducedarmormatches = (from ruls in  SpellListMatches
							where (ruls.GearScore == -1 || IOItemWithID.GearScore >= ruls.GearScore) 
							orderby ruls.RulePriority
							select ruls).ToList();
						
						if(reducedarmormatches.Count > 0)
						{
							IOItemWithID.rulename = reducedarmormatches.First().RuleName; 
							IOItemWithID.IOR = IOResult.rule; 
							return;
						}
						else
						{
							return;
						}
						
					case ObjectClass.Clothing:
						
						if(IOItemWithID.LValue(LongValueKey.EquipableSlots) == 0x8000000)
						{														
							List<ItemRule> reducedcloakmatches = (from ruls in SpellListMatches
								where (ruls.GearScore == -1 || IOItemWithID.GearScore >= ruls.GearScore)
								orderby ruls.RulePriority
								select ruls).ToList();
							
							if(reducedcloakmatches.Count > 0)
							{
								IOItemWithID.rulename = reducedcloakmatches.First().RuleName; 
								IOItemWithID.IOR = IOResult.rule; 
								return;
							}
							else
							{
								return;
							}
						}
						else if(IOItemWithID.LValue(LongValueKey.ArmorLevel) > 0)
						{
							List<ItemRule> reducedarmorclothmatches = (from ruls in SpellListMatches
								where (ruls.GearScore == -1 || IOItemWithID.GearScore >= ruls.GearScore)
								orderby ruls.RulePriority
								select ruls).ToList();
						
							if(reducedarmorclothmatches.Count > 0)
							{
								IOItemWithID.rulename = reducedarmorclothmatches.First().RuleName; 
								IOItemWithID.IOR = IOResult.rule; 
								return;
							}
							else 
							{
								return;
							}
						}

						else
						{	
							List<ItemRule> reducedclothingmatches = (from ruls in SpellListMatches
									where (ruls.GearScore == -1 || IOItemWithID.GearScore >= ruls.GearScore) 
									orderby ruls.RulePriority
									select ruls).ToList();
							
							if(reducedclothingmatches.Count > 0)
							{
								IOItemWithID.rulename = reducedclothingmatches.First().RuleName; 
								IOItemWithID.IOR = IOResult.rule; 
								return;
							}
							else
							{
								return;
							}						
						}
					case ObjectClass.MeleeWeapon:
					case ObjectClass.MissileWeapon:
					case ObjectClass.WandStaffOrb:
						List<ItemRule> reducedweaponmatches = (from ruls in SpellListMatches
							where IOItemWithID.GearScore >= ruls.GearScore &&
							(ruls.RuleDamageTypes == 0 || (ruls.RuleDamageTypes & IOItemWithID.DamageType) == IOItemWithID.DamageType) &&
							(ruls.RuleWieldSkill == 0 || (IOItemWithID.LValue(LongValueKey.WieldReqType) == 7 && ruls.RuleWieldSkill == 0) ||
								ruls.RuleWieldSkill == IOItemWithID.LValue(LongValueKey.WieldReqAttribute)) &&
							(ruls.RuleMastery == 0 || IOItemWithID.WeaponMasteryCategory == ruls.RuleMastery) &&
							(!ruls.WieldRequirements.Any(x => x.WieldEnabled) || 
							  ruls.WieldRequirements.Any(x => x.WieldEnabled && IOItemWithID.LValue(LongValueKey.WieldReqValue) == x.WieldReqValue) ||
							  (ruls.WieldRequirements.Any(x => x.WieldEnabled && x.WieldReqValue == 0) && 
							   (IOItemWithID.LValue(LongValueKey.WieldReqType) == 0 || IOItemWithID.LValue(LongValueKey.WieldReqType) == 7)))
							orderby ruls.RulePriority
							select ruls).ToList();
						
						if(reducedweaponmatches.Count > 0)
						{
							IOItemWithID.rulename = reducedweaponmatches.First().RuleName; 
							IOItemWithID.IOR = IOResult.rule; 
							return;
						}
						else
						{
							return;
						}	
	
					case ObjectClass.Misc:
						if(IOItemWithID.Name.ToLower().Contains("essence"))
						{							
							List<ItemRule> reducedessencematches = (from ruls in SpellListMatches
								where IOItemWithID.GearScore >= ruls.GearScore && ruls.RuleWieldSkill == 54 &&
								(ruls.RuleMastery == 0 || IOItemWithID.WeaponMasteryCategory == ruls.RuleMastery) &&
								(ruls.RuleDamageTypes == 0 || (ruls.RuleDamageTypes & IOItemWithID.DamageType) == IOItemWithID.DamageType) &&
								(!ruls.WieldRequirements.Any(x => x.WieldEnabled) || 
								 ruls.WieldRequirements.Any(x => x.WieldEnabled && IOItemWithID.LValue((LongValueKey)NewLongKeys.SummoningSkill) == x.WieldReqValue))
								orderby ruls.RulePriority
								select ruls).ToList();
							
							if(reducedessencematches.Count() > 0)
							{
								IOItemWithID.rulename = reducedessencematches.First().RuleName; 
								IOItemWithID.IOR = IOResult.rule; 
								return;
							}
							else
							{
								return;
							}	
						}
						else
						{
							return;
						}
					default:
						return;
				}				
			}
			catch(Exception ex) {LogError(ex);}
		}
		
		private bool MatchAdvanced(ItemRule rule, LootObject item)
		{
			bool result = false;
			bool[] tumbler = {false,false,false,false,false};
			List<int> ands = new List<int>();
			
			try
			{
				for(int i = 0; i < rule.Advanced.Count; i ++)
				{
					if(rule.Advanced[i].keylink == 1) {ands.Add(i);}
					
					if(rule.Advanced[i].keytype == 0)
					{
						switch(rule.Advanced[i].keycompare)
						{
							case 0:
								if(item.DValue((DoubleValueKey)rule.Advanced[i].key) == rule.Advanced[i].keyvalue) {tumbler[i] = true;}
								break;
							case 1:
								if(item.DValue((DoubleValueKey)rule.Advanced[i].key) != rule.Advanced[i].keyvalue) {tumbler[i] = true;}
								break;
							case 2: 
								if(item.DValue((DoubleValueKey)rule.Advanced[i].key) >= rule.Advanced[i].keyvalue) {tumbler[i] = true;}
								break;
							case 3: 
								if(item.DValue((DoubleValueKey)rule.Advanced[i].key) <= rule.Advanced[i].keyvalue) {tumbler[i] = true;}
								break;
						}
					}
					else if(rule.Advanced[i].keytype == 1)
					{
						switch(rule.Advanced[i].keycompare)
						{
							case 0:
								if(item.LValue((LongValueKey)rule.Advanced[i].key) == rule.Advanced[i].keyvalue) {tumbler[i] = true;}
								break;
							case 1:
								if(item.LValue((LongValueKey)rule.Advanced[i].key) != rule.Advanced[i].keyvalue) {tumbler[i] = true;}
								break;
							case 2: 
								if(item.LValue((LongValueKey)rule.Advanced[i].key) >= rule.Advanced[i].keyvalue) {tumbler[i] = true;}
								break;
							case 3: 
								if(item.LValue((LongValueKey)rule.Advanced[i].key) <= rule.Advanced[i].keyvalue) {tumbler[i] = true;}
								break;
						}
					}	
				}
				
				switch(rule.Advanced.Count)
				{
					case 1:  //2 ^ 0 == 1
						if(tumbler[0]) {result = true;}
						break;
					case 2:  //2 ^ 1 == 2
						if(rule.Advanced[0].keylink == 1)
						{
							if(tumbler[0] && tumbler[1]) {result = true;}
						}
						else if(rule.Advanced[0].keylink == 2)
						{
							if(tumbler[0] || tumbler[1]) {result = true;}
						}
						break;
					case 3:  //2 ^ 2 == 4
						if(rule.Advanced[0].keylink == 1 && rule.Advanced[1].keylink == 1)
						{
							if(tumbler[0] && tumbler[1] && tumbler[2]) {result = true;}
						}
						else if(rule.Advanced[0].keylink == 1 && rule.Advanced[1].keylink == 2)
						{
							if((tumbler[0] && tumbler[1]) || tumbler[2]) {result = true;}
						}
						else if(rule.Advanced[0].keylink == 2 && rule.Advanced[1].keylink == 1)
						{
							if(tumbler[0] || (tumbler[1] && tumbler[2])) {result = true;}
						}
						else if(rule.Advanced[0].keylink == 2 && rule.Advanced[1].keylink == 2)
						{
							if(tumbler[0] || tumbler[1] || tumbler[2]) {result = true;}
						}
						break;
					case 4:  //2 ^ 3 == 8
						if(rule.Advanced[0].keylink == 1 && rule.Advanced[1].keylink == 1 && rule.Advanced[2].keylink == 1)
						{
							if(tumbler[0] && tumbler[1] && tumbler[2] && tumbler[3]) {result = true;}
						}
						else if(rule.Advanced[0].keylink == 1 && rule.Advanced[1].keylink == 1 && rule.Advanced[2].keylink == 2)
						{
							if((tumbler[0] && tumbler[1] && tumbler[2]) || tumbler[3]) {result = true;}
						}
						else if(rule.Advanced[0].keylink == 1 && rule.Advanced[1].keylink == 2 && rule.Advanced[2].keylink == 1)
						{
							if((tumbler[0] && tumbler[1]) || (tumbler[2] && tumbler[3])) {result = true;}
						}
						else if(rule.Advanced[0].keylink == 2 && rule.Advanced[1].keylink == 1 && rule.Advanced[2].keylink == 1)
						{
							if(tumbler[0] || (tumbler[1] && tumbler[2] && tumbler[3])) {result = true;}
						}
						else if(rule.Advanced[0].keylink == 1 && rule.Advanced[1].keylink == 2 && rule.Advanced[2].keylink == 2)
						{
							if((tumbler[0] && tumbler[1]) || tumbler[2] || tumbler[3]) {result = true;}
						}
						else if(rule.Advanced[0].keylink == 2 && rule.Advanced[1].keylink == 2 && rule.Advanced[2].keylink == 1)
						{
							if(tumbler[0] || tumbler[1] || (tumbler[2] && tumbler[3])) {result = true;}
						}
						else if(rule.Advanced[0].keylink == 2 && rule.Advanced[1].keylink == 2 && rule.Advanced[2].keylink == 2)
						{
							if(tumbler[0] || tumbler[1] || tumbler[2] || tumbler[3]) {result = true;}
						}
						else if(rule.Advanced[0].keylink == 2 && rule.Advanced[1].keylink == 1 && rule.Advanced[2].keylink == 2)
						{
							if(tumbler[0] || (tumbler[1] && tumbler[2]) || tumbler[3]) {result = true;}
						}
						break;
					case 5:  //2 ^ 4 == 16
						// 0 Or
						if(rule.Advanced[0].keylink == 1 && rule.Advanced[1].keylink == 1 && rule.Advanced[2].keylink == 1 && rule.Advanced[3].keylink == 1)
						{
							if(tumbler[0] && tumbler[1] && tumbler[2] && tumbler[3] && tumbler[4]) {result = true;}
						}
						
						// 1 Or
						if(rule.Advanced[0].keylink == 1 && rule.Advanced[1].keylink == 1 && rule.Advanced[2].keylink == 1 && rule.Advanced[3].keylink == 2)
						{
							if((tumbler[0] && tumbler[1] && tumbler[2] && tumbler[3]) || tumbler[4]) {result = true;}
						}
						if(rule.Advanced[0].keylink == 1 && rule.Advanced[1].keylink == 1 && rule.Advanced[2].keylink == 2 && rule.Advanced[3].keylink == 1)
						{
							if((tumbler[0] && tumbler[1] && tumbler[2]) || (tumbler[3] && tumbler[4])) {result = true;}
						}
						if(rule.Advanced[0].keylink == 1 && rule.Advanced[1].keylink == 2 && rule.Advanced[2].keylink == 1 && rule.Advanced[3].keylink == 1)
						{
							if((tumbler[0] && tumbler[1]) || (tumbler[2] && tumbler[3] && tumbler[4])) {result = true;}
						}
						if(rule.Advanced[0].keylink == 2 && rule.Advanced[1].keylink == 1 && rule.Advanced[2].keylink == 1 && rule.Advanced[3].keylink == 1)
						{
							if(tumbler[0] || (tumbler[1] && tumbler[2] && tumbler[3] && tumbler[4])) {result = true;}
						}
						
						// 2 Or
						if(rule.Advanced[0].keylink == 1 && rule.Advanced[1].keylink == 1 && rule.Advanced[2].keylink == 2 && rule.Advanced[3].keylink == 2)
						{
							if((tumbler[0] && tumbler[1] && tumbler[2]) || tumbler[3] || tumbler[4]) {result = true;}
						}
						if(rule.Advanced[0].keylink == 1 && rule.Advanced[1].keylink == 2 && rule.Advanced[2].keylink == 2 && rule.Advanced[3].keylink == 1)
						{
							if((tumbler[0] && tumbler[1]) || tumbler[2] || (tumbler[3] && tumbler[4])) {result = true;}
						}
						if(rule.Advanced[0].keylink == 2 && rule.Advanced[1].keylink == 2 && rule.Advanced[2].keylink == 1 && rule.Advanced[3].keylink == 1)
						{
							if(tumbler[0] || tumbler[1] || (tumbler[2] && tumbler[3] && tumbler[4])) {result = true;}
						}
						if(rule.Advanced[0].keylink == 1 && rule.Advanced[1].keylink == 2 && rule.Advanced[2].keylink == 1 && rule.Advanced[3].keylink == 2)
						{
							if((tumbler[0] && tumbler[1]) || (tumbler[2] && tumbler[3]) || tumbler[4]) {result = true;}
						}
						if(rule.Advanced[0].keylink == 2 && rule.Advanced[1].keylink == 1 && rule.Advanced[2].keylink == 2 && rule.Advanced[3].keylink == 1)
						{
							if(tumbler[0] || (tumbler[1] && tumbler[2]) || (tumbler[3] && tumbler[4])) {result = true;}
						}
						if(rule.Advanced[0].keylink == 2 && rule.Advanced[1].keylink == 1 && rule.Advanced[2].keylink == 1 && rule.Advanced[3].keylink == 2)
						{
							if(tumbler[0] || (tumbler[1] && tumbler[2] && tumbler[3}) ||  tumbler[4]) {result = true;}
						}
						
						// 3 Or
						if(rule.Advanced[0].keylink == 1 && rule.Advanced[1].keylink == 2 && rule.Advanced[2].keylink == 2 && rule.Advanced[3].keylink == 2)
						{
							if((tumbler[0] && tumbler[1]) || tumbler[2] || tumbler[3] || tumbler[4]) {result = true;}
						}
						if(rule.Advanced[0].keylink == 2 && rule.Advanced[1].keylink == 1 && rule.Advanced[2].keylink == 2 && rule.Advanced[3].keylink == 2)
						{
							if(tumbler[0] || (tumbler[1] && tumbler[2]) || tumbler[3] || tumbler[4]) {result = true;}
						}
						if(rule.Advanced[0].keylink == 2 && rule.Advanced[1].keylink == 2 && rule.Advanced[2].keylink == 2 && rule.Advanced[3].keylink == 1)
						{
							if(tumbler[0] || tumbler[1] || tumbler[2] || (tumbler[3] && tumbler[4])) {result = true;}
						}
						if(rule.Advanced[0].keylink == 2 && rule.Advanced[1].keylink == 2 && rule.Advanced[2].keylink == 1 && rule.Advanced[3].keylink == 2)
						{
							if(tumbler[0] || tumbler[1] || (tumbler[2] && tumbler[3]) || tumbler[4]) {result = true;}
						}
						
						// 4 Or
						if(rule.Advanced[0].keylink == 2 && rule.Advanced[1].keylink == 2 && rule.Advanced[2].keylink == 2 && rule.Advanced[3].keylink == 2)
						{
							if(tumbler[0] || tumbler[1] || tumbler[2] || tumbler[3] || tumbler[4]) {result = true;}
						}
						
						
						
						
						
						break;
				}	
			}catch(Exception ex){LogError(ex);}
			return result;
		}

		
		
		private void FillItemRules()
		{
	        List<int> CombineIntList = new List<int>();
			
			try
			{
				ItemRulesList.Clear();	
				
				var ruleslistenabled = from rules in mPrioritizedRulesList
					where rules.Element("Enabled").Value == "true"
					select rules;
				
				foreach(var XRule in ruleslistenabled)
				{
					ItemRule tRule = new ItemRule();
		        	
		        	if(!Int32.TryParse((XRule.Element("Priority").Value), out tRule.RulePriority)){tRule.RulePriority = 999;}
		        	
		        	tRule.RuleAppliesTo = _ConvertCommaStringToIntList((string)XRule.Element("AppliesToFlag").Value).Sum();
		        		        	
		        	tRule.RuleName = (string)XRule.Element("Name").Value;
		        
		        	if(!Int32.TryParse(XRule.Element("ArcaneLore").Value, out tRule.RuleArcaneLore)){tRule.RuleArcaneLore = -1;}
		        	if(!Double.TryParse(XRule.Element("Work").Value, out tRule.RuleWork)){tRule.RuleWork = -1;}					
					if(!Int32.TryParse(XRule.Element("WieldSkill").Value, out tRule.RuleWieldSkill)) {tRule.RuleWieldSkill = 0;}
					if(!Int32.TryParse(XRule.Element("MasteryType").Value, out tRule.RuleMastery)){tRule.RuleMastery = 0;}
					if(!Int32.TryParse(XRule.Element("GearScore").Value, out tRule.GearScore)){tRule.GearScore = -1;}		        	
		        	if(!Int32.TryParse(XRule.Element("WieldLevel").Value, out tRule.RuleWieldLevel)) {tRule.RuleWieldLevel = -1;}
		        	if(!Int32.TryParse((XRule.Element("NumSpells").Value), out tRule.RuleSpellNumber)){tRule.RuleSpellNumber = -1;}

		        	tRule.RuleDamageTypes = _ConvertCommaStringToIntList((string)XRule.Element("DamageType").Value).Sum();
		        	tRule.WieldRequirements = _ConvertCommaStringsToWREVist((string)XRule.Element("WieldEnabled").Value, (string)XRule.Element("ReqSkill").Value);
		        	tRule.RuleArmorTypes = _ConvertCommaStringToIntList((string)XRule.Element("ArmorType").Value);
		        	tRule.RuleSlots = _ConvertCommaStringToIntList((string)XRule.Element("Slots").Value).Sum();
		        	tRule.RuleArmorSet = _ConvertCommaStringToIntList((string)XRule.Element("ArmorSet").Value);
		        	tRule.RuleSpells = _ConvertCommaStringToIntList((string)XRule.Element("Spells").Value);
		        	
		        	if(((string)XRule.Element("Advanced").Value).StartsWith("true"))
		        	{
		        		tRule.AdvSettings = true;
		        		tRule.Advanced = _ConvertAdvStringToAdvanced((string)XRule.Element("Advanced").Value);
		        	}
		        	else
		        	{
		        		tRule.AdvSettings = false;
		        	}
					
					ItemRulesList.Add(tRule);
				}
				
			} catch(Exception ex){LogError(ex);}
		}
		
		
		
		private class ItemRule
		{
			public int GearScore = -1;
	        public int RulePriority = 999; 
	        public int RuleAppliesTo = 0;
	        public string RuleName = String.Empty; 
	        public int RuleArcaneLore = -1;
	        public double RuleWork = -1;
	        public int RuleWieldLevel = -1;
	        	        
	        public int RuleWieldSkill = 0;
	        public int RuleMastery = 0;
	        public int RuleDamageTypes = 0;
	        
	        public List<WREV> WieldRequirements = new List<WREV>();

	        public List<int> RuleArmorTypes  = new List<int>();
	        public List<int> RuleArmorSet = new List<int>();
	        public int RuleSlots = 0;

	        public List<int> RuleSpells = new List<int>();
	        public int RuleSpellNumber = -1;   

	        public List<int> Palattes = new List<int>();
	        public bool AdvSettings = false;
	        public List<advsettings> Advanced = new List<advsettings>();
	        
	        public class WREV
	        {
	        	public int WieldReqValue = -1;
	        	public bool WieldEnabled = false;
	        }
	        
	        public class advsettings
	        {
	        	public int keytype = 0;
	        	public int key = 0;
	        	public double keyvalue = 0;
	        	public int keycompare = 0;
	        	public int keylink = 0;
	        }
	        	
		}
		
		private List<ItemRule.WREV> _ConvertCommaStringsToWREVist(string EnabledString, string WieldString)
        {
        	try
        	{
        		List<ItemRule.WREV> wrevList = new List<ItemRule.WREV>();
        		
        		string[] EnabledSplitString = EnabledString.Split(',');
        		string[] WieldSplitString = WieldString.Split(',');
        		
        		for(int i = 0; i < 4; i++)
        		{
        			ItemRule.WREV wrev = new ItemRule.WREV();
        			wrev.WieldReqValue = Convert.ToInt32(WieldSplitString[i]);
        			wrev.WieldEnabled = Convert.ToBoolean(EnabledSplitString[i]);
        			wrevList.Add(wrev);
        		}
 				
				return wrevList;        		   		
        	}catch(Exception ex){LogError(ex); WriteToChat("Wield String = " + WieldString); return new List<ItemRule.WREV>();}
        }
		
		string[] splitstring;
		string[] splstr;
		private void FillSalvageRules()
		{
			try
			{
				SalvageRulesList.Clear();
				var EnabledSalvage = from salv in mSalvageList
					where salv.Element("checked").Value == "true"
					select salv;
				
				foreach(var XSalv in EnabledSalvage)
				{
					
					splitstring = XSalv.Element("combine").Value.Split(',');
					
					if(splitstring.Count() == 1)
					{
						SalvageRule sr = new SalvageRule();
						Int32.TryParse(XSalv.Element("intvalue").Value, out sr.material);
						
						if(splitstring[0].Contains("-"))
						{
							splstr = splitstring[0].Split('-');
							   	bool success0 = Double.TryParse(splstr[0], out sr.minwork);
							   	bool success1 = Double.TryParse(splstr[1], out sr.maxwork);
							   	sr.ruleid = MaterialIndex[sr.material].name + " " + sr.minwork.ToString("N0") + "-" + sr.maxwork.ToString("N0");
							   	if(success0 && success1) {SalvageRulesList.Add(sr);}
						}
						else
						{
							bool success0 = Double.TryParse(splitstring[0], out sr.minwork);
							sr.maxwork = 10;
							sr.ruleid = MaterialIndex[sr.material].name + " " + sr.minwork.ToString("N0") + "-" + sr.maxwork.ToString("N0");
							if(success0) {SalvageRulesList.Add(sr);}
						}
					}
					else
					{
						foreach(string salvstring in splitstring)
						{
							SalvageRule sr = new SalvageRule();					
							Int32.TryParse(XSalv.Element("intvalue").Value, out sr.material);
							
							if(salvstring.Contains("-"))
							{
							   	string[] splstr = salvstring.Split('-');
							   	bool success0 = Double.TryParse(splstr[0], out sr.minwork);
							   	bool success1 = Double.TryParse(splstr[1], out sr.maxwork);
							   	sr.ruleid = MaterialIndex[sr.material].name + " " + sr.minwork.ToString("N0") + "-" + sr.maxwork.ToString("N0");
							   	if(success0 && success1) {SalvageRulesList.Add(sr);}
							}
							else
							{
								bool success = Double.TryParse(salvstring, out sr.minwork);
								sr.maxwork = sr.minwork;
								sr.ruleid = MaterialIndex[sr.material].name + " " + sr.minwork.ToString("N0") + "-" + sr.maxwork.ToString("N0");
								if(success) {SalvageRulesList.Add(sr);}
							}
							
							
						}
					}
				}
			} catch{}
		}
		
		public class SalvageRule
		{
			public string ruleid;
			public int material;
			public double minwork;
			public double maxwork;
		}

	}
}
