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
using VirindiHUDs;
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;
using VirindiViewService.Themes;
using System.Xml.Serialization;
using System.Xml;

namespace GearFoundry
{
	/// <summary>
	/// Description of ItemTrackerIdentify.
	/// </summary>
	public partial class PluginCore
	{
		//Item Tracker Manual ID functions begin here
		
		private void ManualCheckItemForMatches(IdentifiedObject IOItem)
		{
			try
			{
				if(IOItem.HasIdData){CheckRulesItem(ref IOItem);}
				if(IOItem.ObjectClass == ObjectClass.Scroll){CheckUnknownScrolls(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {TrophyListCheckItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckSalvageItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckManaItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {CheckValueItem(ref IOItem);}
				if(IOItem.IOR == IOResult.unknown) {IOItem.IOR = IOResult.nomatch;}
				
				if(GISettings.ModifiedLooting) {ReportStringToChat(IOItem.ModString());}
				else {ReportStringToChat(IOItem.LinkString());}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		
		// Item Tracker ID functions begin here
		private void CheckSalvageItem(ref IdentifiedObject IOItemSalvage)
		{
			try
			{
				IdentifiedObject IoItemSalvageMirror = IOItemSalvage;
				
				if(IOItemSalvage.SalvageWorkmanship > 0)
				{
					var salvagerulecheck = from allrules in SalvageRulesList
						where (allrules.material == IoItemSalvageMirror.Matieral) &&
												(IoItemSalvageMirror.SalvageWorkmanship >= allrules.minwork) &&
												(IoItemSalvageMirror.SalvageWorkmanship <= (allrules.maxwork +0.99))
										select allrules;
					
					if(salvagerulecheck.Count() > 0)
					{
						IOItemSalvage.IOR = IOResult.salvage;
						IOItemSalvage.rulename = salvagerulecheck.First().ruleid;
					}
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void CheckManaItem(ref IdentifiedObject IOItemMana)
		{
			try
			{
				if(GISettings.LootByMana == 0){return;}
				if(IOItemMana.IntValues(LongValueKey.CurrentMana) > GISettings.LootByMana)
				{
					//Irq:  TODO:  Cull manatanks when there is not a mana stone to eat them.  It's irritating.  Make a list of them for destruction as needed.
					//Irq:  TODO:  Add mana value or find it....
					IOItemMana.IOR = IOResult.manatank;
				}
			} catch(Exception ex){LogError(ex);}
		}
		
		private void CheckValueItem(ref IdentifiedObject IOItemVal)
		{
			try
			{
				if(GISettings.LootByValue == 0){return;}
				
				double ratio = ((double)IOItemVal.Value / (double)IOItemVal.Burden);
				if(ratio >= mLootValBurdenRatioMinimum)
				{
					IOItemVal.IOR = IOResult.val;
				}
				else if(IOItemVal.Value >= GISettings.LootByValue)
				{
					IOItemVal.IOR = IOResult.val;
				}
			} catch(Exception ex){LogError(ex);}
		}
		
		private void CheckUnknownScrolls(ref IdentifiedObject IOScroll)
		{
			//TODO:  Refine this to make more useful if there is a community request
			try
			{
				if(!GISettings.CheckForL7Scrolls) 
				{
					IOScroll.IOR = IOResult.nomatch;
					return;
				}
			
			
				if(!Core.CharacterFilter.IsSpellKnown(IOScroll.Spell(0)))
				{
					if(SpellIndex[IOScroll.Spell(0)].spelllevel == 7)
					{
						IOScroll.IOR = IOResult.spell;
					}
				}	
			} catch(Exception ex){LogError(ex);}
			return;
		}
		
		
		private void TrophyListCheckItem(ref IdentifiedObject IOItem)
		{	
			try
			{
				
				
				string namecheck = IOItem.Name;
				var matches = from XTrophies in mSortedTrophiesListChecked
					where (namecheck.ToLower().Contains((string)XTrophies.Element("key").Value.ToLower()) && !Convert.ToBoolean(XTrophies.Element("isexact").Value)) ||
					(namecheck == (string)XTrophies.Element("key").Value && Convert.ToBoolean(XTrophies.Element("isexact").Value))
							  select XTrophies;
					
				if(matches.Count() > 0)
				{
					IOItem.IOR = IOResult.trophy;
				}				
			} catch(Exception ex){LogError(ex);}
			return;
		}
		
		private void CheckRulesItem(ref IdentifiedObject IOItemWithIDReference)
		{
			
			//Irq:  Note to self:  Cloak IDs....cloaks w/spells are 352 = 1;  cloaks w/absorb are 352=2
			try
			{
				
				ModifiedIOSpells.Clear();
				IdentifiedObject IOItemWithID = IOItemWithIDReference;

				var AppliesToListMatches = from rules in ItemRulesList
					where (rules.RuleAppliesTo & IOItemWithID.IntValues(LongValueKey.Category)) == IOItemWithID.IntValues(LongValueKey.Category)
					orderby rules.RulePriority
					select rules;
				
				if(AppliesToListMatches.Count() == 0) {return;}
				
				string RuleName;
				if(GISettings.ModifiedLooting)
				{
					for(int i = 0; i < IOItemWithID.SpellCount; i ++)
					{
						ModifiedIOSpells.Add(IOItemWithID.Spell(i));
					}
					
					switch(IOItemWithID.ObjectClass)
					{					
						case ObjectClass.Armor:
							//this matching currently ignores armor types
							var reducedarmormatches = from ruls in AppliesToListMatches
								where IOItemWithID.ArmorLevel >= ruls.RuleArmorLevel &&  //will match 0 AL values on rule.
								(ruls.RuleArmorCoverage == 0 || (ruls.RuleArmorCoverage & IOItemWithID.ArmorCoverage) == IOItemWithID.ArmorCoverage) &&   //Will ignore or match armor coverage for true
								ModifiedIOSpells.Intersect(ruls.RuleSpells).Count() >= ruls.RuleSpellNumber &&	//will determine if number of spells are on object
								(ruls.RuleArmorSet.Count() == 0 || ruls.RuleArmorSet.Contains(IOItemWithID.ArmorSet))  && //Will ignore or match armor sets
								ruls.RuleUnenchantable == IOItemWithID.Unehcantable
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
							if(IOItemWithID.ArmorLevel > 0)
							{
								var reducedarmorclothmatches = from ruls in AppliesToListMatches
									where IOItemWithID.ArmorLevel >= ruls.RuleArmorLevel &&  //will match 0 AL values on rule.
									(ruls.RuleArmorCoverage == 0 || (ruls.RuleArmorCoverage & IOItemWithID.ArmorCoverage) == IOItemWithID.ArmorCoverage) &&   //Will ignore or match armor coverage for true
									ModifiedIOSpells.Intersect(ruls.RuleSpells).Count() >= ruls.RuleSpellNumber &&	//will determine if number of spells are on object
									(ruls.RuleArmorSet.Count() == 0 || ruls.RuleArmorSet.Contains(IOItemWithID.ArmorSet))  && //Will ignore or match armor sets
									ruls.RuleUnenchantable == IOItemWithID.Unehcantable
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
							else if(IOItemWithID.WieldSlot == 0x8000000)
							{
								var reducedcloakmatches = from ruls in AppliesToListMatches
									where IOItemWithID.MaxItemLevel >= ruls.RuleItemLevel &&
									ModifiedIOSpells.Intersect(ruls.RuleSpells).Count() >= ruls.RuleSpellNumber &&
									(ruls.RuleArmorSet.Count() == 0 || ruls.RuleArmorSet.Contains(IOItemWithID.ArmorSet))
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
									where ModifiedIOSpells.Intersect(ruls.RuleSpells).Count() >= ruls.RuleSpellNumber
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
								where IOItemWithID.GearScore >= ruls.WeaponModSum &&
								((ruls.RuleDamageTypes & IOItemWithID.DamageType) == IOItemWithID.DamageType || ruls.RuleDamageTypes == 0) &&
								ruls.RuleWieldAttribute == IOItemWithID.WieldReqAttribute &&
								((ruls.RuleWeaponEnabledA && IOItemWithID.WieldReqValue == ruls.WieldReqValueA) ||
								 (ruls.RuleWeaponEnabledB && IOItemWithID.WieldReqValue == ruls.WieldReqValueB) ||
								 (ruls.RuleWeaponEnabledC && IOItemWithID.WieldReqValue == ruls.WieldReqValueC) ||
								 (ruls.RuleWeaponEnabledD && IOItemWithID.WieldReqValue == ruls.WieldReqValueD))
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
							 		where ((ruls.RuleRed && IOItemWithID.WieldSlot == 0x40000000) ||
							 		       (ruls.RuleYellow && IOItemWithID.WieldSlot == 0x20000000) ||
							 		       (ruls.RuleBlue && IOItemWithID.WieldSlot == 0x10000000)) &&
							 				IOItemWithID.MaxItemLevel >= ruls.RuleItemLevel
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
								where ModifiedIOSpells.Intersect(ruls.RuleSpells).Count() >= ruls.RuleSpellNumber
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
									where IOItemWithID.BonusComparison >= ruls.EssenceModSum &&
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
				}
				else
				{
					foreach(var rule in AppliesToListMatches)
					{					
						RuleName = rule.RuleName;					
						//If it's already assigned a rule, don't check any longer. 
						if(IOItemWithID.IOR == IOResult.rule){return;}
	
						//Irquk:  Keywords confirmed functional
						if(rule.RuleKeyWords.Count() > 0)
						{
							foreach(string checkstring in rule.RuleKeyWords)
							{
								if(!IOItemWithID.Name.Contains(checkstring)) {RuleName = String.Empty; goto Next;}
							}
						}
						//Irquk:  Exclusion Keywords confirmed functional
						if(rule.RuleKeyWordsNot.Count() > 0)
						{
							foreach(string checkstring in rule.RuleKeyWordsNot)
							{
								if(IOItemWithID.Name.Contains(checkstring)) {RuleName = String.Empty; goto Next;}
							}
						}
						//Irquk:  Confirmed functional Check Arcane Lore (Arcane Lore is a <= field)
						if(rule.RuleArcaneLore > 0)
						{
							if(IOItemWithID.IntValues(LongValueKey.LoreRequirement) > rule.RuleArcaneLore) {RuleName = String.Empty; goto Next;}
						}
	//					//Irquk: confirmed functional Check Value, this is a <= field
						if(rule.RuleValue > 0)
						{
							if(IOItemWithID.IntValues(LongValueKey.Value) > rule.RuleValue) {RuleName = String.Empty; goto Next;}
						}
	//					//Irquk: confirmed functional Check Work, this is a <= field
						if(rule.RuleWork > 0)
						{
							if(IOItemWithID.DblValues(DoubleValueKey.SalvageWorkmanship) > rule.RuleWork) {RuleName = String.Empty; goto Next;}
						}
	//					//Irquk: Confirmed Functional. Check Burden, this is a <= field
						if(rule.RuleBurden > 0)
						{
							if(IOItemWithID.IntValues(LongValueKey.Burden) > rule.RuleBurden) {RuleName = String.Empty; goto Next;}
						}
	//					//Irquk:  Confirmed Functional  Check Wield Level.  Field is a <= field
						if(rule.RuleWieldLevel > 0)
						{
							int levelcheck = 0;
							if(IOItemWithID.WieldReqType == 7) {levelcheck = IOItemWithID.WieldReqValue; }
							if(IOItemWithID.WieldReqType2 == 7) {levelcheck = IOItemWithID.WieldReqValue2;}	
							if(levelcheck > rule.RuleWieldLevel) {RuleName = String.Empty; goto Next;}
						}
	//					//Irquk:  confirmed Functional, basica comparison only to WieldAttribute1
						if(rule.RuleWieldAttribute > 0)
						{
							if(IOItemWithID.WieldReqType != 7) 
							{
								if(IOItemWithID.WieldReqAttribute !=  rule.RuleWieldAttribute){	RuleName = String.Empty; goto Next;}
							}
						}
	//					//Irquk:  Confirmed Functional
						if(rule.RuleMastery > 0)
						{
							if(IOItemWithID.WeaponMasteryCategory != rule.RuleMastery) {RuleName = String.Empty; goto Next;}
						}
	//					//Irquk:  Confirmed Functional
						if(rule.RuleMeleeD > 0)
						{
							if(rule.RuleMeleeD > IOItemWithID.WeaponMeleeBonus) {RuleName = String.Empty; goto Next;}
						}
	//					//Irquk:  Confirmed Functional
						if(rule.RuleMcModAttack > 0)
						{
							if(IOItemWithID.ObjectClass == ObjectClass.WandStaffOrb)
							{
								//NOTE:  Mana C doesn't report as 1.xxxx like all other doubles for weapons.  It has had +1 added in IdentifiedObject get acesssor to correct
								if(rule.RuleMcModAttack > (IOItemWithID.WeaponManaCBonus)) {RuleName = String.Empty; goto Next;}
							}
							if(IOItemWithID.ObjectClass == ObjectClass.MissileWeapon)
							{
								if(rule.RuleMcModAttack > IOItemWithID.WeaponMissileModifier) {RuleName = String.Empty; goto Next;}
							}
							if(IOItemWithID.ObjectClass == ObjectClass.MeleeWeapon)
							{
								if(rule.RuleMcModAttack > IOItemWithID.WeaponAttackBonus) {RuleName = String.Empty; goto Next;}
							}
						}
	//					//Irquk:  confirmed functional for Missile D bonus
						//TODO:  Check magic D bonus
						if(rule.RuleMagicD > 0)
						{
								if(IOItemWithID.WeaponMagicDBonus > 0)
								{
									if(rule.RuleMagicD > IOItemWithID.WeaponMagicDBonus) {RuleName = String.Empty; goto Next;}
								}
								else if(IOItemWithID.WeaponMissileDBonus > 0)
								{
									if(rule.RuleMagicD > IOItemWithID.WeaponMissileDBonus) {RuleName = String.Empty; goto Next;}
								}
								else
								{
									RuleName = String.Empty; goto Next;
								}	
						}
						//Irquk:  Confirmed Functional
						if(rule.RuleWeaponEnabledA || rule.RuleWeaponEnabledB || rule.RuleWeaponEnabledC || rule.RuleWeaponEnabledD)
						{
							bool[] ruletrue = {false, false, false, false};
							if(rule.RuleWeaponEnabledA)
							{	
								if((rule.MSCleaveA == IOItemWithID.MSCleave && rule.WieldReqValueA == IOItemWithID.WieldReqValue && 
								    IOItemWithID.WeaponMaxDamage >= rule.MaxDamageA && IOItemWithID.Variance <= rule.VarianceA))
								     {ruletrue[0] = true;}
							}
							if(rule.RuleWeaponEnabledB)
							{	
								if((rule.MSCleaveB == IOItemWithID.MSCleave && rule.WieldReqValueB == IOItemWithID.WieldReqValue && 
								    IOItemWithID.WeaponMaxDamage >= rule.MaxDamageB && IOItemWithID.Variance <= rule.VarianceB))
								     {ruletrue[1] = true;}
							}
							if(rule.RuleWeaponEnabledC)
							{	
								if((rule.MSCleaveC == IOItemWithID.MSCleave && rule.WieldReqValueC == IOItemWithID.WieldReqValue && 
								    IOItemWithID.WeaponMaxDamage >= rule.MaxDamageC && IOItemWithID.Variance <= rule.VarianceC))
								     {ruletrue[2] = true;}
							}					
							if(rule.RuleWeaponEnabledD)
							{	
								if((rule.MSCleaveD == IOItemWithID.MSCleave && rule.WieldReqValueD == IOItemWithID.WieldReqValue && 
								    IOItemWithID.WeaponMaxDamage >= rule.MaxDamageD && IOItemWithID.Variance <= rule.VarianceD))
								     {ruletrue[3] = true;}
							}
							if(!ruletrue[0] && !ruletrue[1] && !ruletrue[2] && !ruletrue[3]) {RuleName = String.Empty; goto Next;}
						}
						//Irquk:  Confirmed functional
						if(rule.RuleDamageTypes > 0)
						{
							if(!((rule.RuleDamageTypes & IOItemWithID.DamageType) == IOItemWithID.DamageType)) {RuleName = String.Empty; goto Next;}
						}
	//					
	//					//Irquk:  Confirmed functional
						if(rule.RuleArmorLevel > 0)
						{
							if(rule.RuleArmorLevel > IOItemWithID.ArmorLevel) {RuleName = String.Empty; goto Next;}
						}
	//					//Irquk:  Confirmed functional
						if(rule.RuleArmorTypes.Length > 0)
						{
							int IOArmorType = -1;  //I'd prefer 0, but there's a 0 index in the ArmorIndex
							if(!(IOItemWithID.ArmorLevel > 0)) {RuleName = String.Empty; goto Next;}  //If it's not armor, get rid of it
							//If it's unknown type make it other.
							if(!ArmorIndex.Any(x => IOItemWithID.Name.ToLower().Contains(x.name.ToLower()))) 
							{
								IOArmorType = ArmorIndex.Find(x => x.name == "Other").ID;
							}
							else if(ArmorIndex.Any(x => IOItemWithID.Name.ToLower().StartsWith(x.name.ToLower())))
							{
								IOArmorType = ArmorIndex.Find(x => IOItemWithID.Name.ToLower().StartsWith(x.name.ToLower())).ID;
							}
							else if(IOArmorType < 0 && ArmorIndex.Any(x => IOItemWithID.Name.ToLower().Contains(x.name.ToLower())))
							{
								IOArmorType = ArmorIndex.Find(x => IOItemWithID.Name.ToLower().Contains(x.name.ToLower())).ID;
							}
							if(!rule.RuleArmorTypes.Contains(IOArmorType)) {RuleName = String.Empty; goto Next;}
						}
						//Irquk:  Confirmed Functional
						if(rule.RuleArmorSet.Length > 0)
						{
							if(!rule.RuleArmorSet.Contains(IOItemWithID.ArmorSet)){RuleName = String.Empty; goto Next;}
						}
						//Irquk Confirmed Functional
						if(rule.RuleArmorCoverage > 0)
						{
							if((IOItemWithID.ArmorCoverage & rule.RuleArmorCoverage) != IOItemWithID.ArmorCoverage) {RuleName = String.Empty; goto Next;}
						}
						//Irquk Confirmed Functional
						if(rule.RuleUnenchantable)
						{
							if(IOItemWithID.IntValues(LongValueKey.Unenchantable) != 9999) {RuleName = String.Empty; goto Next;}
						}
				
						bool red = false;
						bool yellow = false;
						bool blue = false;
						//Irquk:  Confirmed Functional
						if(rule.RuleRed || rule.RuleYellow || rule.RuleBlue)
						{
							if(rule.RuleRed) {if(IOItemWithID.WieldReqType == 7 && IOItemWithID.WieldReqValue == 225) {red = true;}}
							if(rule.RuleYellow) {if(IOItemWithID.WieldReqType == 7 && IOItemWithID.WieldReqValue == 150) {yellow = true;}}
							if(rule.RuleBlue){if(IOItemWithID.WieldReqType == 7 && IOItemWithID.WieldReqValue  == 75) {blue = true;}}
							if(!red && !yellow && !blue){RuleName = String.Empty; goto Next;}
						}
										
						//Irquk:  Confirmed functional
						if(rule.RuleItemLevel > 0)
						{
							if(IOItemWithID.MaxItemLevel < rule.RuleItemLevel) {RuleName = String.Empty; goto Next;}
						}
						//Irquk:  Confirmed functional
						if(rule.RuleEssenceLevel > 0)
						{
							if(rule.RuleEssenceLevel > IOItemWithID.EssenceLevel) {RuleName = String.Empty; goto Next;}
						}
						//Irquk:  Confirmed functional
						if(rule.RuleEssenceDamage > 0)
						{
							if(IOItemWithID.Dam < rule.RuleEssenceDamage)  {RuleName = String.Empty; goto Next;}
						}
						//Irquk:  confirmed functional
						if(rule.RuleEssenceDamageResist > 0)
						{
							if(IOItemWithID.DamResist < rule.RuleEssenceDamageResist)  {RuleName = String.Empty; goto Next;}
						}
						//Irquk:  confirmed functional
						if(rule.RuleEssenceCrit > 0)
						{
							if(IOItemWithID.Crit < rule.RuleEssenceCrit)  {RuleName = String.Empty; goto Next;}
						}
						//Irquk:  confirmed functional
						if(rule.RuleEssenceCritResist > 0)
						{
							if(IOItemWithID.CritDamResist < rule.RuleEssenceCritResist)  {RuleName = String.Empty; goto Next;}
						}
						//Irquk:  Confirmed functional
						if(rule.RuleEssenceCritDam > 0)
						{
							if(IOItemWithID.CritDam < rule.RuleEssenceCritDam)  {RuleName = String.Empty; goto Next;}
						}
						//Irquk:  confirmed functional
						if(rule.RuleEssenceCritDamResist > 0)
						{
							if(IOItemWithID.CritDamResist < rule.RuleEssenceCritDamResist)  {RuleName = String.Empty; goto Next;}
						}
					
						
						//Irquk:  confirmed functional. 
						if(rule.RuleSpellNumber > 0)
						{
							int spellmatches = 0;
							for(int i = 0; i < IOItemWithID.SpellCount; i++)
							{
								if(rule.RuleSpells.Contains(IOItemWithID.Spell(i))) {spellmatches++;}
							}
							//Irq:  Cloak IDs....cloaks w/spells are 352 = 1;  cloaks w/absorb are 352=2
							if(rule.RuleSpells.Contains(10000)){if(IOItemWithID.IntValues((LongValueKey)352) == 2){spellmatches++;}}
							if(spellmatches < rule.RuleSpellNumber) {RuleName = String.Empty; goto Next;}
						}
	
	
						if(RuleName != String.Empty)
						{
							IOItemWithIDReference.IOR = IOResult.rule;
							IOItemWithIDReference.rulename = RuleName;
							return;
						}
						
						Next:
						if(RuleName == String.Empty)
						{
							IOItemWithIDReference.IOR = IOResult.unknown;
						}	
					}
					return;	
				}
				
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
						if(!Int32.TryParse(splitstringWield[0], out tRule.WieldReqValueA)) {tRule.WieldReqValueA = 0;}
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
						if(!Int32.TryParse(splitstringWield[1], out tRule.WieldReqValueB)) {tRule.WieldReqValueB = 0;}
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
						if(!Int32.TryParse(splitstringWield[2], out tRule.WieldReqValueC)) {tRule.WieldReqValueC = 0;}
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
						if(!Int32.TryParse(splitstringWield[3], out tRule.WieldReqValueD)) {tRule.WieldReqValueD = 0;}
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
					
					
					splitstring = ((string)XRule.Element("Coverage").Value).Split(',');
					if(splitstring.Length > 0)
					{
						sumarray = new int[splitstring.Length];      	
						for(int j = 0; j < splitstring.Length; j++){if(!Int32.TryParse(splitstring[j], out sumarray[j])){sumarray[j] = 0;}}
						tRule.RuleArmorCoverage = sumarray.Sum();
					}
					
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
					
					if(!bool.TryParse((XRule.Element("Unenchantable").Value), out tRule.RuleUnenchantable)){tRule.RuleUnenchantable = false;}
					if(!Int32.TryParse((XRule.Element("MinArmorLevel").Value), out tRule.RuleArmorLevel)){tRule.RuleArmorLevel = 0;}
					
					if(!Boolean.TryParse(XRule.Element("Red").Value, out tRule.RuleRed)) {tRule.RuleRed = false;}
					if(!Boolean.TryParse(XRule.Element("Yellow").Value, out tRule.RuleYellow)) {tRule.RuleYellow = false;}
					if(!Boolean.TryParse(XRule.Element("Blue").Value, out tRule.RuleBlue)) {tRule.RuleBlue = false;}
					if(!Int32.TryParse((XRule.Element("NumSpells").Value), out tRule.RuleSpellNumber)){tRule.RuleSpellNumber = 0;}
					
					CombineIntList.Clear();
					splitstring = ((string)XRule.Element("Spells").Value).Split(',');
					if(splitstring.Length > 0)
					{
						for(int j = 0; j < splitstring.Length; j++)
						{
							tempint = 0;
							Int32.TryParse(splitstring[j], out tempint);
							if(tempint > 0) {CombineIntList.Add(tempint);}
						}
					}
					splitstring = ((string)XRule.Element("CloakSpells").Value).Split(',');
					if(splitstring.Length > 0)
					{
						bool cloakspell;
						if(!bool.TryParse((string)XRule.Element("CloakMustHaveSpell").Value, out cloakspell)) {cloakspell = false;}
						if(cloakspell)
						{
							tRule.RuleSpellNumber++;
						}
						for(int j = 0; j < splitstring.Length; j++)
						{
							tempint = 0;
							Int32.TryParse(splitstring[j], out tempint);
							if(tempint > 0) {CombineIntList.Add(tempint);}
						}
					}
					tRule.RuleSpells = CombineIntList.ToArray();
	
					CombineIntList.Clear();
					
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
