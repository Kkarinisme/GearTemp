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
				
				ModifiedIOSpells.Clear();
				LootObject IOItemWithID = IOItemWithIDReference;
				
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
			
				var SpellListMatches = from spellmatches in SlotListMatches
					where spellmatches.RuleSpellNumber == -1 || spellmatches.RuleSpells.Count == 0 || 
					ModifiedIOSpells.Intersect(spellmatches.RuleSpells).Count() >= spellmatches.RuleSpellNumber
					select spellmatches;
				
				if(SpellListMatches.Count() == 0) {return;}
					
				var SetMatches = from sets in SpellListMatches
					where sets.RuleArmorSet.Count == 0 || sets.RuleArmorSet.Contains(IOItemWithID.LValue(LongValueKey.ArmorSet))
					select sets;
				
				if(SetMatches.Count() == 0) {return;}
								
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
							(ruls.RuleArmorTypes.Count == 0 || ruls.RuleArmorTypes.Contains(IOItemWithID.ArmorType))
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
						
						if(IOItemWithID.LValue(LongValueKey.EquipableSlots) == 0x8000000)
						{														
							var reducedcloakmatches = from ruls in SetMatches
								where (ruls.GearScore == -1 || IOItemWithID.GearScore >= ruls.GearScore)
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
						else if(IOItemWithID.LValue(LongValueKey.ArmorLevel) > 0)
						{
							var reducedarmorclothmatches = from ruls in SetMatches
								where (ruls.GearScore == -1 || IOItemWithID.GearScore >= ruls.GearScore) &&
								(ruls.RuleArmorTypes.Count == 0  || ruls.RuleArmorTypes.Contains(IOItemWithID.ArmorType))
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
							(ruls.RuleWieldSkill == 0 || (IOItemWithID.LValue(LongValueKey.WieldReqType) == 7 && ruls.RuleWieldSkill == 0) ||
								ruls.RuleWieldSkill == IOItemWithID.LValue(LongValueKey.WieldReqAttribute)) &&
							(ruls.RuleMastery == 0 || IOItemWithID.WeaponMasteryCategory == ruls.RuleMastery) &&
							(!ruls.WieldRequirements.Any(x => x.WieldEnabled) || 
							  ruls.WieldRequirements.Any(x => x.WieldEnabled && IOItemWithID.LValue(LongValueKey.WieldReqValue) == x.WieldReqValue))
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
								(!ruls.WieldRequirements.Any(x => x.WieldEnabled) || 
							 	ruls.WieldRequirements.Any(x => x.WieldEnabled && IOItemWithID.LValue(LongValueKey.WieldReqValue) == x.WieldReqValue))
								
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
	        
	        public class WREV
	        {
	        	public int WieldReqValue = -1;
	        	public bool WieldEnabled = false;
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
		
		

	}
}
