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
				
				WriteToChat("ItemRulesList.Count = " + ItemRulesList.Count());

				
				var AppliesToListMatches = from appliesto in ItemRulesList
					where (appliesto.RuleAppliesTo & IOItemWithID.LValue(LongValueKey.Category)) == IOItemWithID.LValue(LongValueKey.Category)
					select appliesto;
				
				if(AppliesToListMatches.Count() == 0) {return;}
				
				var PropertyListMatches = from properties in AppliesToListMatches
					where (properties.RuleArcaneLore == -1 || IOItemWithID.LValue(LongValueKey.LoreRequirement) <= properties.RuleArcaneLore) &&
						  (properties.RuleWork == -1 || IOItemWithID.LValue(LongValueKey.Workmanship) <= properties.RuleWork) &&
						  (properties.RuleWieldLevel == -1 || IOItemWithID.LValue(LongValueKey.WieldReqType) != 7 ||
					 	  (IOItemWithID.LValue(LongValueKey.WieldReqType) == 7 && IOItemWithID.LValue(LongValueKey.WieldReqValue) <= properties.RuleWieldLevel))
					select properties;	
				
				if(PropertyListMatches.Count() == 0) {return;}				
				
				var SlotListMatches = from slots in PropertyListMatches
					where slots.RuleSlots == 0 || (IOItemWithID.LValue(LongValueKey.EquipableSlots) & slots.RuleSlots) != 0
					select slots;
				
				if(SlotListMatches.Count() == 0) {return;}
				
				for(int i = 0; i < IOItemWithID.SpellCount; i ++)
				{
					ModifiedIOSpells.Add(IOItemWithID.Spell(i));
				}			
				if(IOItemWithID.LValue((LongValueKey)352) ==  2)
				{
					ModifiedIOSpells.Add(10000);
				}
				
				var SpellListMatches = from spellmatches in PropertyListMatches
					where spellmatches.RuleSpellNumber == -1 || ModifiedIOSpells.Intersect(spellmatches.RuleSpells).Count() >= spellmatches.RuleSpellNumber
					select spellmatches;
				
				WriteToChat("Spell List Match = " + SpellListMatches.Count());
				
				if(SpellListMatches.Count() == 0) {return;}
				else{WriteToChat("Spell Matches works");}
					
				var SetMatches = from sets in SpellListMatches
					where sets.RuleArmorSet == null || sets.RuleArmorSet.Contains(IOItemWithID.LValue(LongValueKey.ArmorSet))
					orderby sets.RulePriority
					select sets;
				
				if(SetMatches.Count() == 0) {return;}
				else{WriteToChat("Set Matches Works");}
								
				switch(IOItemWithID.ObjectClass)
				{		

					case ObjectClass.Gem:
							if(!IOItemWithID.Aetheriacheck) {return;}
							
						var reducedaetheriamatches = from ruls in SlotListMatches
							where (ruls.GearScore == -1 || IOItemWithID.GearScore >= ruls.GearScore) 
							orderby ruls.RulePriority
							select ruls;
						
							if(reducedaetheriamatches.Count() > 0)
							{
								IOItemWithID.rulename = SetMatches.First().RuleName; 
								IOItemWithID.IOR = IOResult.rule; 
								return;
							}
							else
							{
								return;
							}

					case ObjectClass.Jewelry:  //Jewelry doesn't have gear scores at this time.						
							if(SpellListMatches.Count() > 0)
							{
								IOItemWithID.rulename = SetMatches.First().RuleName; 
								IOItemWithID.IOR = IOResult.rule; 
								return;
							}
							else
							{
								return;
							}
						
					case ObjectClass.Armor:
						var reducedarmormatches = from ruls in SetMatches
							where (ruls.GearScore == -1 || IOItemWithID.GearScore >= ruls.GearScore) &&
							(ruls.RuleArmorTypes == null || ruls.RuleArmorTypes.Contains(IOItemWithID.ArmorType))
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
							var reducedarmorclothmatches = from ruls in SetMatches
								where (ruls.GearScore == -1 || IOItemWithID.GearScore >= ruls.GearScore) &&
								(ruls.RuleArmorTypes == null || ruls.RuleArmorTypes.Contains(IOItemWithID.ArmorType))
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
							var reducedcloakmatches = from ruls in SetMatches
								where (ruls.GearScore == -1 || IOItemWithID.GearScore >= ruls.GearScore)
								orderby ruls.RulePriority
								select ruls;
							
							if(reducedcloakmatches.Count() > 0)
							{
								IOItemWithID.rulename = SetMatches.First().RuleName; 
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
							if(SetMatches.Count() > 0)
							{
								IOItemWithID.rulename = SetMatches.First().RuleName; 
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
						var reducedmeleematches = from ruls in SpellListMatches
							where IOItemWithID.GearScore >= ruls.GearScore &&
							(ruls.RuleDamageTypes == 0 || (ruls.RuleDamageTypes & IOItemWithID.DamageType) == IOItemWithID.DamageType) &&
							(ruls.RuleWieldSkill == 0 || ruls.RuleWieldSkill == IOItemWithID.LValue(LongValueKey.WieldReqAttribute)) &&
							(ruls.RuleMastery == 0 || IOItemWithID.WeaponMasteryCategory == ruls.RuleMastery) &&
							((!ruls.RuleWeaponEnabledA && !ruls.RuleWeaponEnabledB && !ruls.RuleWeaponEnabledC && !ruls.RuleWeaponEnabledD) ||
							 (ruls.RuleWeaponEnabledA && IOItemWithID.LValue(LongValueKey.WieldReqValue) == ruls.WieldReqValueA) ||
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
	
					case ObjectClass.Misc:
						if(IOItemWithID.Name.ToLower().Contains("essence"))
						{
							
							var reducedessencematches = from ruls in SetMatches
								where IOItemWithID.GearScore >= ruls.GearScore && ruls.RuleWieldSkill == 54 &&
								(ruls.RuleMastery == 0 || IOItemWithID.WeaponMasteryCategory == ruls.RuleMastery) &&
								(ruls.RuleDamageTypes == 0 || (ruls.RuleDamageTypes & IOItemWithID.DamageType) == IOItemWithID.DamageType) &&
								((ruls.RuleWeaponEnabledA && IOItemWithID.LValue(LongValueKey.WieldReqValue) == ruls.WieldReqValueA) ||
							 	 (ruls.RuleWeaponEnabledB && IOItemWithID.LValue(LongValueKey.WieldReqValue) == ruls.WieldReqValueB) ||
							 	 (ruls.RuleWeaponEnabledC && IOItemWithID.LValue(LongValueKey.WieldReqValue) == ruls.WieldReqValueC) ||
							 	 (ruls.RuleWeaponEnabledD && IOItemWithID.LValue(LongValueKey.WieldReqValue) == ruls.WieldReqValueD))
								
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
			}
			catch(Exception ex) {LogError(ex);}
		}
		
//[VTank] --------------Object dump--------------
//[VTank] [Meta] Create count: 1
//[VTank] [Meta] Create time: 8/26/2013 5:38 AM
//[VTank] [Meta] Has identify data: True
//[VTank] [Meta] Last ID time: 8/26/2013 6:00 AM
//[VTank] [Meta] Worldfilter valid: True
//[VTank] [Meta] Client valid: True
//[VTank] ID: 8C06BD10
//[VTank] ObjectClass: WandStaffOrb
//[VTank] (S) Name: Staff
//[VTank] (S) FullDescription: Staff of Shockwave
//[VTank] (I) CreateFlags1: -1855225704
//[VTank] (I) Type: 2547
//[VTank] (I) Icon: 5801
//[VTank] (I) Category: 32768
//[VTank] (I) Behavior: 18
//[VTank] (I) Value: 16402
//[VTank] (I) ItemUsabilityFlags: 6291461
//[VTank] (I) UsageMask: 16
//[VTank] (I) IconOutline: 1
//[VTank] (I) Container: 1343211182
//[VTank] (I) Slot: -1
//[VTank] (I) EquipableSlots: 16777216 (z)
//[VTank] (I) EquippedSlots: 0 (0)
//[VTank] (I) Burden: 50
//[VTank] (I) HookMask: 2
//[VTank] (I) Material: 22
//[VTank] (I) PhysicsDataFlags: 170145
//[VTank] (I) GemSettingQty: 3
//[VTank] (I) GemSettingType: 20
//[VTank] (I) SkillLevelReq: 0
//[VTank] (I) Workmanship: 6
//[VTank] (I) Spellcraft: 328
//[VTank] (I) CurrentMana: 3267
//[VTank] (I) DescriptionFormat: 5
//[VTank] (I) MaximumMana: 3267
//[VTank] (I) LoreRequirement: 344
//[VTank] (I) RankRequirement: 0
//[VTank] (I) EquippedBy: 0
//[VTank] (D) SalvageWorkmanship: 6
//[VTank] (D) ElementalDamageVersusMonsters: 1.07000000029802
//[VTank] (D) ManaCBonus: 0.1
//[VTank] (D) MeleeDefenseBonus: 1.15
//[VTank] (D) ManaRateOfChange: -0.0555555555555556
//[VTank] (L) EquippedBy: 0
//[VTank] (L) Container: 1343211182
//[VTank] Palette Entry 0: ID 0x000BF1, Ex Color: 000000, 0/0
		
		
		private void FillItemRules()
		{
			string[] splitstring;
	        string[] splitstringEnabled;
	        string[] splitstringWield;
	        int[] sumarray;
	        int tempint;
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
		        	
		        	splitstring = ((string)XRule.Element("AppliesToFlag").Value).Split(',');
		        	if(splitstring.Length > 0)
		        	{
			        	sumarray = new int[splitstring.Length];
						for(int j = 0; j < splitstring.Length; j++){if(!Int32.TryParse(splitstring[j], out sumarray[j])){sumarray[j] = 0;}}
						tRule.RuleAppliesTo = sumarray.Sum();
		        	}
		        		        	
		        	tRule.RuleName = (string)XRule.Element("Name").Value;
		        
		        	if(!Int32.TryParse(XRule.Element("ArcaneLore").Value, out tRule.RuleArcaneLore)){tRule.RuleArcaneLore = -1;}
		        	if(!Double.TryParse(XRule.Element("Work").Value, out tRule.RuleWork)){tRule.RuleWork = -1;}
					
					if(!Int32.TryParse(XRule.Element("WieldSkill").Value, out tRule.RuleWieldSkill)) {tRule.RuleWieldSkill = 0;}
					if(!Int32.TryParse(XRule.Element("MasteryType").Value, out tRule.RuleMastery)){tRule.RuleMastery = 0;}
					if(!Int32.TryParse(XRule.Element("GearScore").Value, out tRule.GearScore)){tRule.GearScore = -1;}
		        	
		        	if(!Int32.TryParse(XRule.Element("WieldLevel").Value, out tRule.RuleWieldLevel)) {tRule.RuleWieldLevel = -1;}

	        	
					splitstring = ((string)XRule.Element("DamageType").Value).Split(',');
					
					if(splitstring.Length > 0)
					{
						sumarray = new int[splitstring.Length];      	
						for(int j = 0; j < splitstring.Length; j++){if(!Int32.TryParse(splitstring[j], out sumarray[j])){sumarray[j] = 0;}}
						tRule.RuleDamageTypes = sumarray.Sum();
					}
					
					
					
					splitstringEnabled = (XRule.Element("WieldEnabled").Value).Split(',');
					
					splitstringWield = (XRule.Element("ReqSkill").Value).Split(',');
					
					if(!Boolean.TryParse(splitstringEnabled[0], out tRule.RuleWeaponEnabledA)) {tRule.RuleWeaponEnabledA = false;}
					if(tRule.RuleWeaponEnabledA)
					{
						if(!Int32.TryParse(splitstringWield[0], out tRule.WieldReqValueA)) {tRule.WieldReqValueA = -1;}
					}

					if(!Boolean.TryParse(splitstringEnabled[1], out tRule.RuleWeaponEnabledB)) {tRule.RuleWeaponEnabledB = false;}
					if(tRule.RuleWeaponEnabledB)
					{
						if(!Int32.TryParse(splitstringWield[1], out tRule.WieldReqValueB)) {tRule.WieldReqValueB = -1;}
					}

					if(!Boolean.TryParse(splitstringEnabled[2], out tRule.RuleWeaponEnabledC)) {tRule.RuleWeaponEnabledC = false;}
					if(tRule.RuleWeaponEnabledC)
					{
						if(!Int32.TryParse(splitstringWield[2], out tRule.WieldReqValueC)) {tRule.WieldReqValueC = -1;}
					}

					if(!Boolean.TryParse(splitstringEnabled[3], out tRule.RuleWeaponEnabledD)) {tRule.RuleWeaponEnabledD = false;}
					if(tRule.RuleWeaponEnabledD)
					{
						if(!Int32.TryParse(splitstringWield[3], out tRule.WieldReqValueD)) {tRule.WieldReqValueD = -1;}
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
					
					splitstring = ((string)XRule.Element("Slots").Value).Split(',');
					if(splitstring.Length > 0)
					{
						sumarray = new int[splitstring.Length];      	
						for(int j = 0; j < splitstring.Length; j++){if(!Int32.TryParse(splitstring[j], out sumarray[j])){sumarray[j] = 0;}}
						tRule.RuleSlots = sumarray.Sum();
					}
					
					if((string)XRule.Element("ArmorSet").Value != String.Empty)
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
						tRule.RuleArmorSet = CombineIntList.ToArray();
						CombineIntList.Clear();
					}
					else
					{
						tRule.RuleArmorSet = null;
					}
					
					
					CombineIntList.Clear();
					
					if(!Int32.TryParse((XRule.Element("NumSpells").Value), out tRule.RuleSpellNumber)){tRule.RuleSpellNumber = -1;}
					
					if((string)XRule.Element("Spells").Value == String.Empty)
					{
						tRule.RuleSpells = null;
						tRule.RuleSpellNumber = -1;
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
							
						}

						tRule.RuleSpells = CombineIntList.ToArray();
						CombineIntList.Clear();
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
	        
	        public bool RuleWeaponEnabledA = false;
	        public int WieldReqValueA = -1;
	        public bool RuleWeaponEnabledB = false;
	        public int WieldReqValueB = -1;
	        public bool RuleWeaponEnabledC = false;
	        public int WieldReqValueC = -1;
	        public bool RuleWeaponEnabledD = false;
	        public int WieldReqValueD = -1;

	        public int[] RuleArmorTypes;
	        public int[] RuleArmorSet;
	        public int RuleSlots = 0;

	        public int[] RuleSpells = null;
	        public int RuleSpellNumber = -1;   

	        public int[] Palattes = null;
		}
		
		

	}
}
