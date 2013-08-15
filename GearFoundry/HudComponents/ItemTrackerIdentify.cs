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
					where XTrophies.Element("enabled").Value == "true" && 
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
						WriteToChat("Trophy Rejected on LootMax Check.");
						return;
					}
			
					IOItem.IOR = IOResult.trophy;
				}				
			} catch(Exception ex){LogError(ex);}
			return;
		}
		
		private void CheckRulesItem(ref LootObject IOItemWithIDReference)
		{
			
			//Irq:  Note to self:  Cloak IDs....cloaks w/spells are 352 = 1;  cloaks w/absorb are 352=2
			try
			{
				
				ModifiedIOSpells.Clear();
				LootObject IOItemWithID = IOItemWithIDReference;

				var AppliesToListMatches = from rules in ItemRulesList
					where (rules.RuleAppliesTo & IOItemWithID.LValue(LongValueKey.Category)) == IOItemWithID.LValue(LongValueKey.Category)
					orderby rules.RulePriority
					select rules;
				
				if(AppliesToListMatches.Count() == 0) {return;}
				

				for(int i = 0; i < IOItemWithID.SpellCount; i ++)
				{
					ModifiedIOSpells.Add(IOItemWithID.Spell(i));
				}
				
				if(IOItemWithID.LValue((LongValueKey)352) ==  2)
				{
					ModifiedIOSpells.Add(10000);
				}
				
				switch(IOItemWithID.ObjectClass)
				{					
					case ObjectClass.Armor:
						var reducedarmormatches = from ruls in AppliesToListMatches
							where 
							(ruls.RuleArcaneLore == 0 || IOItemWithID.LValue(LongValueKey.LoreRequirement) <= ruls.RuleArcaneLore) &&
							(ruls.RuleWork == 0 || IOItemWithID.LValue(LongValueKey.Workmanship) <= ruls.RuleWork) &&
							(ruls.RuleWieldLevel == 0 || (IOItemWithID.LValue(LongValueKey.WieldReqType) == 7 && IOItemWithID.LValue(LongValueKey.WieldReqValue) <= ruls.RuleWieldLevel)
							 						  || IOItemWithID.LValue(LongValueKey.WieldReqType) != 7) &&
							(ruls.RuleArmorLevel == 0 || IOItemWithID.ArmorScore >= ruls.RuleArmorLevel) &&
							((ruls.RuleArmorCoverage & IOItemWithID.LValue(LongValueKey.Coverage)) == IOItemWithID.LValue(LongValueKey.Coverage)) &&
							(ruls.RuleArmorTypes == null || ruls.RuleArmorTypes.Contains(IOItemWithID.ArmorType)) &&
							ModifiedIOSpells.Intersect(ruls.RuleSpells).Count() >= ruls.RuleSpellNumber &&	
							(ruls.RuleArmorSet == null || ruls.RuleArmorSet.Contains(IOItemWithID.LValue(LongValueKey.ArmorSet)))  && 
							(!ruls.RuleUnenchantable || IOItemWithID.LValue(LongValueKey.Unenchantable) > 0)
							orderby ruls.RulePriority
							select ruls;
						
						if(reducedarmormatches.Count() > 0)
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
						if(IOItemWithID.LValue(LongValueKey.ArmorLevel) > 0)
						{
							var reducedarmorclothmatches = from ruls in AppliesToListMatches
								where 
								(ruls.RuleArcaneLore == 0 || IOItemWithID.LValue(LongValueKey.LoreRequirement) <= ruls.RuleArcaneLore) &&
								(ruls.RuleWork == 0 || IOItemWithID.LValue(LongValueKey.Workmanship) <= ruls.RuleWork) &&
								(ruls.RuleWieldLevel == 0 || (IOItemWithID.LValue(LongValueKey.WieldReqType) == 7 && IOItemWithID.LValue(LongValueKey.WieldReqValue) <= ruls.RuleWieldLevel)
							 						  || IOItemWithID.LValue(LongValueKey.WieldReqType) != 7) &&
								IOItemWithID.ArmorScore >= ruls.RuleArmorLevel &&
								(ruls.RuleArmorTypes == null || ruls.RuleArmorTypes.Contains(25)) &&
								((ruls.RuleArmorCoverage & IOItemWithID.LValue(LongValueKey.Coverage)) == IOItemWithID.LValue(LongValueKey.Coverage)) &&  
								ModifiedIOSpells.Intersect(ruls.RuleSpells).Count() >= ruls.RuleSpellNumber &&	
								(ruls.RuleArmorSet == null || ruls.RuleArmorSet.Contains(IOItemWithID.LValue(LongValueKey.ArmorSet)))  
								orderby ruls.RulePriority
								select ruls;	
							if(reducedarmorclothmatches.Count() > 0)
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
						else if(IOItemWithID.LValue(LongValueKey.EquipableSlots) == 0x8000000)
						{
							var reducedcloakmatches = from ruls in AppliesToListMatches
								where IOItemWithID.GearScore >= ruls.RuleItemLevel &&
								ModifiedIOSpells.Intersect(ruls.RuleSpells).Count() >= ruls.RuleSpellNumber &&
								(ruls.RuleArmorSet.Contains(IOItemWithID.LValue(LongValueKey.ArmorSet))) &&
								ruls.RuleArmorCoverage == 0
								orderby ruls.RulePriority
								select ruls;
							if(reducedcloakmatches.Count() > 0)
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
						else
						{
							var reducedclothmatches = from ruls in AppliesToListMatches
								where 
								(ruls.RuleArcaneLore == 0 || IOItemWithID.LValue(LongValueKey.LoreRequirement) <= ruls.RuleArcaneLore) &&
								(ruls.RuleWieldLevel == 0 || (IOItemWithID.LValue(LongValueKey.WieldReqType) == 7 && IOItemWithID.LValue(LongValueKey.WieldReqValue) <= ruls.RuleWieldLevel)
							 						  || IOItemWithID.LValue(LongValueKey.WieldReqType) != 7) &&
								ModifiedIOSpells.Intersect(ruls.RuleSpells).Count() >= ruls.RuleSpellNumber &&
								ruls.RuleArmorCoverage == 0
								orderby ruls.RulePriority
								select ruls;
							if(reducedclothmatches.Count() > 0)
							{
								IOItemWithID.rulename = reducedclothmatches.First().RuleName; 
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
						var reducedmeleematches = from ruls in AppliesToListMatches
							where 
							(ruls.RuleArcaneLore == 0 || IOItemWithID.LValue(LongValueKey.LoreRequirement) <= ruls.RuleArcaneLore) &&
							(ruls.RuleWork == 0 || IOItemWithID.LValue(LongValueKey.Workmanship) <= ruls.RuleWork) &&
							(ruls.RuleWieldLevel == 0 || (IOItemWithID.LValue(LongValueKey.WieldReqType) == 7 && IOItemWithID.LValue(LongValueKey.WieldReqValue) <= ruls.RuleWieldLevel)) &&
							IOItemWithID.GearScore >= ruls.WeaponModSum &&
							(ruls.RuleDamageTypes == 0 || (ruls.RuleDamageTypes & IOItemWithID.DamageType) == IOItemWithID.DamageType) &&
							(ruls.RuleWieldAttribute == 0 || ruls.RuleWieldAttribute == IOItemWithID.LValue(LongValueKey.WieldReqAttribute)) &&
							((ruls.RuleWeaponEnabledA && IOItemWithID.LValue(LongValueKey.WieldReqValue) == ruls.WieldReqValueA) ||
							 (ruls.RuleWeaponEnabledB && IOItemWithID.LValue(LongValueKey.WieldReqValue) == ruls.WieldReqValueB) ||
							 (ruls.RuleWeaponEnabledC && IOItemWithID.LValue(LongValueKey.WieldReqValue) == ruls.WieldReqValueC) ||
							 (ruls.RuleWeaponEnabledD && IOItemWithID.LValue(LongValueKey.WieldReqValue) == ruls.WieldReqValueD))
							orderby ruls.RulePriority
							select ruls;
						if(reducedmeleematches.Count() > 0)
						{
							IOItemWithID.rulename = reducedmeleematches.First().RuleName; 
							IOItemWithID.IOR = IOResult.rule; 
							return;
						}
						else
						{
							return;
						}	

							
					case ObjectClass.Gem:
						if(IOItemWithID.Aetheriacheck)
						{
						 	var reducedaetheriamatches = from ruls in AppliesToListMatches
						 		where ((ruls.RuleRed && IOItemWithID.LValue(LongValueKey.EquipableSlots) == 0x40000000) ||
						 		       (ruls.RuleYellow && IOItemWithID.LValue(LongValueKey.EquipableSlots) == 0x20000000) ||
						 		       (ruls.RuleBlue && IOItemWithID.LValue(LongValueKey.EquipableSlots) == 0x10000000)) &&
						 				IOItemWithID.LValue((LongValueKey)NewLongKeys.MaxItemLevel) >= ruls.RuleItemLevel
						 		orderby ruls.RulePriority
						 		select ruls;
						 	if(reducedaetheriamatches.Count() > 0)
							{
								IOItemWithID.rulename = reducedaetheriamatches.First().RuleName; 
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
						
					case ObjectClass.Jewelry:
						var reducedjewelrymatches = from ruls in AppliesToListMatches
							where 
								(ruls.RuleArcaneLore == 0 || IOItemWithID.LValue(LongValueKey.LoreRequirement) <= ruls.RuleArcaneLore) &&
								(ruls.RuleWieldLevel == 0 || (IOItemWithID.LValue(LongValueKey.WieldReqType) == 7 && IOItemWithID.LValue(LongValueKey.WieldReqValue) <= ruls.RuleWieldLevel)
							 						  || IOItemWithID.LValue(LongValueKey.WieldReqType) != 7) &&
								ModifiedIOSpells.Intersect(ruls.RuleSpells).Count() >= ruls.RuleSpellNumber
								orderby ruls.RulePriority
								select ruls;
							if(reducedjewelrymatches.Count() > 0)
							{
								IOItemWithID.rulename = reducedjewelrymatches.First().RuleName; 
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
							
							var reducedessencematches = from ruls in AppliesToListMatches
								where IOItemWithID.RatingScore >= ruls.EssenceModSum &&
								IOItemWithID.EssenceLevel == ruls.RuleEssenceLevel &&
								(ruls.RuleMastery == 0 || IOItemWithID.WeaponMasteryCategory == ruls.RuleMastery) &&
								((ruls.RuleDamageTypes & IOItemWithID.DamageType) == IOItemWithID.DamageType || ruls.RuleDamageTypes == 0)
								orderby ruls.RulePriority
								select ruls;
							
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
				//PITA Looter Code was here.

				
			}
			catch(Exception ex) {LogError(ex);}
		}
		
		private void FillItemRules()
		{
			string[] splitstring;
	        string[] splitstringEnabled;
	        string[] splitstringWield;
	        string[] splitstringDamage;
	        string[] splitstringMSCleave;
	        string[] damagesplit;
	        int[] sumarray;
	        int tempint;
	        List<int> CombineIntList = new List<int>();
			
			try
			{
				ItemRulesList.Clear();				
				for(int i = 0; i < mPrioritizedRulesListEnabled.Count(); i++)
				{
					var XRule = mPrioritizedRulesListEnabled[i];
					ItemRule tRule = new ItemRule();
		        	
		        	if(!bool.TryParse((XRule.Element("Enabled").Value), out tRule.RuleEnabled)){tRule.RuleEnabled = false;}
		        	if(!Int32.TryParse((XRule.Element("Priority").Value), out tRule.RulePriority)){tRule.RulePriority = 0;}
		        	
		        	splitstring = ((string)XRule.Element("AppliesToFlag").Value).Split(',');
		        	if(splitstring.Length > 0)
		        	{
			        	sumarray = new int[splitstring.Length];
						for(int j = 0; j < splitstring.Length; j++){if(!Int32.TryParse(splitstring[j], out sumarray[j])){sumarray[j] = 0;}}
						tRule.RuleAppliesTo = sumarray.Sum();
		        	}
		        		        	
		        	tRule.RuleName = (string)XRule.Element("Name").Value;
		        	if((string)XRule.Element("NameContains").Value != String.Empty)
		        	{
		        		tRule.RuleKeyWords = ((string)XRule.Element("NameContains").Value).Split(' ').ToList();
		        	}
		        	else
		        	{
		        		tRule.RuleKeyWords.Clear();
		        	}
		        	if((string)XRule.Element("NameNotContains").Value != String.Empty)
		        	{
		        		tRule.RuleKeyWordsNot = ((string)XRule.Element("NameNotContains").Value).Split(' ').ToList();
		        	}
		        	else
		        	{
		        		tRule.RuleKeyWordsNot.Clear();
		        	}
		        	
		        	if(!Int32.TryParse(XRule.Element("ArcaneLore").Value, out tRule.RuleArcaneLore)){tRule.RuleArcaneLore = 0;}
		        	if(!Int32.TryParse(XRule.Element("Value").Value, out tRule.RuleValue)){tRule.RuleValue = 0;}
		        	if(!Int32.TryParse(XRule.Element("Burden").Value,out tRule.RuleBurden)){tRule.RuleBurden = 0;}
		        	if(!Double.TryParse(XRule.Element("Work").Value, out tRule.RuleWork)){tRule.RuleWork = 0;}
		        	if(!Int32.TryParse(XRule.Element("WieldReqValue").Value, out tRule.RuleWieldLevel)) {tRule.RuleWieldLevel = 0;}
					if(!Int32.TryParse(XRule.Element("ItemLevel").Value, out tRule.RuleItemLevel)) {tRule.RuleItemLevel = 0;}	        	
					if(!Int32.TryParse(XRule.Element("WieldAttribute").Value, out tRule.RuleWieldAttribute)) {tRule.RuleWieldAttribute = 0;}
					if(!Int32.TryParse(XRule.Element("MasteryType").Value, out tRule.RuleMastery)){tRule.RuleMastery = 0;}
					if(!(tRule.RuleMastery > 0))
					{
		        		if(!Int32.TryParse(XRule.Element("EssMastery").Value, out tRule.RuleMastery)){tRule.RuleMastery = 0;}
					}
					if(!Double.TryParse(XRule.Element("McModAttack").Value, out tRule.RuleMcModAttack)){tRule.RuleMcModAttack = 0;}
					if(!Double.TryParse(XRule.Element("MeleeDef").Value, out tRule.RuleMeleeD)){tRule.RuleMeleeD = 0;}
					if(!Double.TryParse(XRule.Element("MagicDef").Value, out tRule.RuleMagicD)){tRule.RuleMagicD = 0;}
		        	
		        	if(!Int32.TryParse(XRule.Element("WieldLevel").Value, out tRule.RuleWieldLevel)) {tRule.RuleWieldLevel = 0;}
		        	if(!Int32.TryParse(XRule.Element("EssLevel").Value, out tRule.RuleEssenceLevel)){tRule.RuleEssenceLevel = 0;}
		        	if(!Int32.TryParse(XRule.Element("EssDamageLevel").Value, out tRule.RuleEssenceDamage)){tRule.RuleEssenceDamage = 0;}
		        	if(!Int32.TryParse(XRule.Element("EssDRLevel").Value, out tRule.RuleEssenceDamageResist)){tRule.RuleEssenceDamageResist = 0;}
		        	if(!Int32.TryParse(XRule.Element("EssCritLevel").Value, out tRule.RuleEssenceCrit)){tRule.RuleEssenceCrit = 0;}
		        	if(!Int32.TryParse(XRule.Element("EssCRLevel").Value, out tRule.RuleEssenceCritResist)){tRule.RuleEssenceCritResist = 0;}     	
		        	if(!Int32.TryParse(XRule.Element("EssCDLevel").Value, out tRule.RuleEssenceCritDam)){tRule.RuleEssenceCritDam = 0;}     	
		        	if(!Int32.TryParse(XRule.Element("EssCritDamRes").Value, out tRule.RuleEssenceCritDamResist)){tRule.RuleEssenceCritDamResist = 0;}	        	
					splitstring = ((string)XRule.Element("DamageType").Value).Split(',');
					if(splitstring.Length > 0)
					{
						sumarray = new int[splitstring.Length];      	
						for(int j = 0; j < splitstring.Length; j++){if(!Int32.TryParse(splitstring[j], out sumarray[j])){sumarray[j] = 0;}}
						tRule.RuleDamageTypes = sumarray.Sum();
					}
					splitstring = ((string)XRule.Element("EssElements").Value).Split(',');
					if(splitstring.Length > 0 && tRule.RuleDamageTypes == 0)
					{
						sumarray = new int[splitstring.Length];      	
						for(int j = 0; j < splitstring.Length; j++){if(!Int32.TryParse(splitstring[j], out sumarray[j])){sumarray[j] = 0;}}
						tRule.RuleDamageTypes = sumarray.Sum();
					}
					
					splitstringEnabled = (XRule.Element("WieldEnabled").Value).Split(',');
					splitstringWield = (XRule.Element("ReqSkill").Value).Split(',');
					splitstringDamage = (XRule.Element("MinMax").Value).Split(',');
					splitstringMSCleave = (XRule.Element("MSCleave").Value).Split(',');
					
					if(!Boolean.TryParse(splitstringEnabled[0], out tRule.RuleWeaponEnabledA)) {tRule.RuleWeaponEnabledA = false;}
					if(tRule.RuleWeaponEnabledA)
					{
						if(!Boolean.TryParse(splitstringMSCleave[0], out tRule.MSCleaveA)) {tRule.MSCleaveA = false;}
						if(!Int32.TryParse(splitstringWield[0], out tRule.WieldReqValueA)) {tRule.WieldReqValueA = -1;}
						damagesplit = splitstringDamage[0].Split('-');
						if(damagesplit.Length == 2)
						{
							if(!Int32.TryParse(damagesplit[1], out tRule.MaxDamageA)) {tRule.MaxDamageA = 0;}
							int tint;
							if(!Int32.TryParse(damagesplit[0], out tint)) {tint = 0;}
							if(tRule.MaxDamageA > 0) {tRule.VarianceA = ((double)tRule.MaxDamageA - (double)tint)/(double)tRule.MaxDamageA;}
							else {tRule.VarianceA = 0;}
						}
						else
						{
							if(!Int32.TryParse(damagesplit[0], out tRule.MaxDamageA)) {tRule.MaxDamageA = 0;}
							tRule.VarianceA = 0;
						}
					}
					else
					{
						tRule.MSCleaveA = false; tRule.MaxDamageA = 0; tRule.VarianceA = 0;
					}

					if(!Boolean.TryParse(splitstringEnabled[1], out tRule.RuleWeaponEnabledB)) {tRule.RuleWeaponEnabledB = false;}
					if(tRule.RuleWeaponEnabledB)
					{
						if(!Boolean.TryParse(splitstringMSCleave[1], out tRule.MSCleaveB)) {tRule.MSCleaveB = false;}
						if(!Int32.TryParse(splitstringWield[1], out tRule.WieldReqValueB)) {tRule.WieldReqValueB = -1;}
						damagesplit = splitstringDamage[1].Split('-');
						if(damagesplit.Length == 2)
						{
							if(!Int32.TryParse(damagesplit[1], out tRule.MaxDamageB)) {tRule.MaxDamageB = 0;}
							int tint;
							if(!Int32.TryParse(damagesplit[0], out tint)) {tint = 0;}
							if(tRule.MaxDamageB > 0) {tRule.VarianceB = ((double)tRule.MaxDamageB - (double)tint)/(double)tRule.MaxDamageB;}
							else {tRule.VarianceB = 0;}
						
						}
						else
						{
							if(!Int32.TryParse(damagesplit[0], out tRule.MaxDamageB)) {tRule.MaxDamageB = 0;}
							tRule.VarianceB = 0;
						}
					}
					else
					{
						tRule.MSCleaveB = false; tRule.MaxDamageB = 0; tRule.VarianceB = 0;
					}

					if(!Boolean.TryParse(splitstringEnabled[2], out tRule.RuleWeaponEnabledC)) {tRule.RuleWeaponEnabledC = false;}
					if(tRule.RuleWeaponEnabledC)
					{
						if(!Boolean.TryParse(splitstringMSCleave[2], out tRule.MSCleaveC)) {tRule.MSCleaveC = false;}
						if(!Int32.TryParse(splitstringWield[2], out tRule.WieldReqValueC)) {tRule.WieldReqValueC = -1;}
						damagesplit = splitstringDamage[2].Split('-');
						if(damagesplit.Length == 2)
						{
							if(!Int32.TryParse(damagesplit[1], out tRule.MaxDamageC)) {tRule.MaxDamageC = 0;}
							int tint;
							if(!Int32.TryParse(damagesplit[0], out tint)) {tint = 0;}
							if(tRule.MaxDamageC > 0) {tRule.VarianceC = ((double)tRule.MaxDamageC - (double)tint)/(double)tRule.MaxDamageC;}
							else {tRule.VarianceC = 0;}
						
						}
						else
						{
							if(!Int32.TryParse(damagesplit[0], out tRule.MaxDamageC)) {tRule.MaxDamageC = 0;}
							tRule.VarianceC = 0;
						}
					}
					else
					{
						tRule.MSCleaveC = false; tRule.MaxDamageC = 0; tRule.VarianceC = 0;
					}

					if(!Boolean.TryParse(splitstringEnabled[3], out tRule.RuleWeaponEnabledD)) {tRule.RuleWeaponEnabledD = false;}
					if(tRule.RuleWeaponEnabledD)
					{
						if(!Boolean.TryParse(splitstringMSCleave[3], out tRule.MSCleaveD)) {tRule.MSCleaveD = false;}
						if(!Int32.TryParse(splitstringWield[3], out tRule.WieldReqValueD)) {tRule.WieldReqValueD = -1;}
						damagesplit = splitstringDamage[3].Split('-');
						if(damagesplit.Length == 2)
						{
							if(!Int32.TryParse(damagesplit[1], out tRule.MaxDamageD)) {tRule.MaxDamageD = 0;}
							int tint;
							if(!Int32.TryParse(damagesplit[0], out tint)) {tint = 0;}
							if(tRule.MaxDamageD > 0) {tRule.VarianceD = ((double)tRule.MaxDamageD - (double)tint)/(double)tRule.MaxDamageD;}
							else {tRule.VarianceD = 0;}
						
						}
						else
						{
							if(!Int32.TryParse(damagesplit[0], out tRule.MaxDamageD)) {tRule.MaxDamageD = 0;}
							tRule.VarianceD = 0;
						}
					}
					else
					{
						tRule.MSCleaveD = false; tRule.MaxDamageD = 0; tRule.VarianceD = 0;
					}		
					
					
					if((string)XRule.Element("ArmorType").Value != String.Empty)
					{
						CombineIntList.Clear();
						splitstring = (XRule.Element("ArmorType").Value).Split(',');				
						if(splitstring.Length > 0)
						{
							for(int j = 0; j < splitstring.Length; j++)
							{
								tempint = -1;
								Int32.TryParse(splitstring[j], out tempint);
								if(tempint > -1) {CombineIntList.Add(tempint); tempint = -1;}
							}
							tRule.RuleArmorTypes = CombineIntList.ToArray();
						}
					}
					else
					{
						tRule.RuleArmorTypes = null;
					}
					
					
					splitstring = ((string)XRule.Element("Coverage").Value).Split(',');
					if(splitstring.Length > 0)
					{
						sumarray = new int[splitstring.Length];      	
						for(int j = 0; j < splitstring.Length; j++){if(!Int32.TryParse(splitstring[j], out sumarray[j])){sumarray[j] = 0;}}
						tRule.RuleArmorCoverage = sumarray.Sum();
					}
					if((string)XRule.Element("ArmorSet").Value != String.Empty || (string)XRule.Element("CloakSets").Value != String.Empty)
					{
						CombineIntList.Clear();
						splitstring = (XRule.Element("ArmorSet").Value).Split(',');
						if(splitstring.Length > 0)
						{
							for(int j = 0; j < splitstring.Length; j++)
							{
								tempint = -1;
								Int32.TryParse(splitstring[j], out tempint);
								if(tempint > -1) {CombineIntList.Add(tempint); tempint = -1;}
							}
						}
						splitstring = (XRule.Element("CloakSets").Value).Split(',');
						if(splitstring.Length > 0)
						{
							for(int j = 0; j < splitstring.Length; j++)
							{
								tempint = 0;
								Int32.TryParse(splitstring[j], out tempint);
								if(tempint > 0) {CombineIntList.Add(tempint); tempint = 0;}
							}
						}
						tRule.RuleArmorSet = CombineIntList.ToArray();
						CombineIntList.Clear();
					}
					else
					{
						tRule.RuleArmorSet = null;
					}
					
					if(!bool.TryParse((XRule.Element("Unenchantable").Value), out tRule.RuleUnenchantable)){tRule.RuleUnenchantable = false;}
					if(!Int32.TryParse((XRule.Element("MinArmorLevel").Value), out tRule.RuleArmorLevel)){tRule.RuleArmorLevel = 0;}
					
					if(!Boolean.TryParse(XRule.Element("Red").Value, out tRule.RuleRed)) {tRule.RuleRed = false;}
					if(!Boolean.TryParse(XRule.Element("Yellow").Value, out tRule.RuleYellow)) {tRule.RuleYellow = false;}
					if(!Boolean.TryParse(XRule.Element("Blue").Value, out tRule.RuleBlue)) {tRule.RuleBlue = false;}
					
					CombineIntList.Clear();
					
					if((string)XRule.Element("Spells").Value == String.Empty && (string)XRule.Element("CloakSpells").Value == String.Empty)
					{
						tRule.RuleSpells = null;
						tRule.RuleSpellNumber = 0;
					}
					else
					{
						if((string)XRule.Element("Spells").Value != String.Empty)
						{
							splitstring = ((string)XRule.Element("Spells").Value).Split(',');
							for(int j = 0; j < splitstring.Length; j++)
							{
								tempint = 0;
								Int32.TryParse(splitstring[j], out tempint);
								if(tempint > 0) {CombineIntList.Add(tempint);}
							}
							if(!Int32.TryParse((XRule.Element("NumSpells").Value), out tRule.RuleSpellNumber)){tRule.RuleSpellNumber = 0;}
						}
						if((string)XRule.Element("CloakSpells").Value != String.Empty)
						{
							splitstring = ((string)XRule.Element("CloakSpells").Value).Split(',');
							tRule.RuleSpellNumber++;
							for(int j = 0; j < splitstring.Length; j++)
							{
								tempint = 0;
								Int32.TryParse(splitstring[j], out tempint);
								if(tempint > 0) {CombineIntList.Add(tempint);}
							}
							tRule.RuleSpellNumber++;
						}
						tRule.RuleSpells = CombineIntList.ToArray();
						CombineIntList.Clear();
					}
					
	
					
					
					//rules for modified looting behavior
					
					tRule.WeaponModSum = tRule.RuleMcModAttack + tRule.RuleMeleeD + tRule.RuleMagicD;
					tRule.EssenceModSum = tRule.RuleEssenceCrit + tRule.RuleEssenceCritDam +tRule.RuleEssenceCritDamResist + tRule.RuleEssenceCritResist + tRule.RuleEssenceDamage + tRule.RuleEssenceDamageResist;
					
					ItemRulesList.Add(tRule);
				}
				
			} catch(Exception ex){LogError(ex);}
		}
		
		private class ItemRule
		{
			public bool RuleEnabled; 
	        public int RulePriority; 
	        public int RuleAppliesTo;
	        public string RuleName; 
	        public List<string> RuleKeyWords = new List<string>();
	        public List<string> RuleKeyWordsNot = new List<string>();
	        public int RuleArcaneLore;
	        public int RuleBurden;
	        public int RuleValue;
	        public double RuleWork;
	        public int RuleWieldLevel;
	        public int RuleItemLevel;
	        public bool RuleRed;
	        public bool RuleYellow;
	        public bool RuleBlue;
	        	        
	        public int RuleWieldAttribute;
	        public int RuleMastery;
	        public int RuleDamageTypes;
	        public double RuleMcModAttack;
	        public double RuleMeleeD;
	        public double RuleMagicD;
	        
	        public bool RuleWeaponEnabledA;
	        public bool MSCleaveA;
	        public int WieldReqValueA;
	        public int MaxDamageA;
	        public double VarianceA;
	        public bool RuleWeaponEnabledB;
	        public bool MSCleaveB;
	        public int WieldReqValueB;
	        public int MaxDamageB;
	        public double VarianceB;
	        public bool RuleWeaponEnabledC;
	        public bool MSCleaveC;
	        public int WieldReqValueC;
	        public int MaxDamageC;
	        public double VarianceC;
	        public bool RuleWeaponEnabledD;
	        public bool MSCleaveD;
	        public int WieldReqValueD;
	        public int MaxDamageD;
	        public double VarianceD;
	        
	        public int RuleArmorLevel;
	        public int[] RuleArmorTypes;
	        public int[] RuleArmorSet;
	        public int RuleArmorCoverage;
	        public bool RuleUnenchantable;
	        
	        public int RuleEssenceLevel;
	        public int RuleEssenceDamage;
	        public int RuleEssenceDamageResist;
	        public int RuleEssenceCrit;
	        public int RuleEssenceCritResist;
	        public int RuleEssenceCritDam;
	        public int RuleEssenceCritDamResist;

	        public int[] RuleSpells;
	        public int RuleSpellNumber;     

	        public double WeaponModSum;
	        public double EssenceModSum;
	        

		}
		
		

	}
}
