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

namespace GearFoundry
{
	/// <summary>
	/// Description of ItemTracker.
	/// </summary>
	public partial class PluginCore
	{		
		//
		private List<ItemRule> ItemRulesList = new List<ItemRule>();
		
		private OpenContainer mOpenContainer = new OpenContainer();
		private List<int> ItemExclusionList = new List<int>();
		private List<IdentifiedObject> ItemTrackingList = new List<IdentifiedObject>();
		private List<int> ItemIDListenList = new List<int>();
		private List<int> ManaTankItems = new List<int>();
		
		public class OpenContainer
		{
			public bool ContainerIsLooting = false;
			public int ContainerGUID;
			public List<IdentifiedObject> ContainerIOs = new List<PluginCore.IdentifiedObject>();
		}
		
		void SubscribeLootEvents()
		{
			try
			{
				CoreManager.Current.ContainerOpened += LootContainerOpened;
             	Core.EchoFilter.ServerDispatch += ServerDispatchItem;
			}
			catch{}
		}
		
		void UnsubscribeLootEvents()
		{
			try
			{
				CoreManager.Current.ContainerOpened -= LootContainerOpened;
             	Core.EchoFilter.ServerDispatch -= ServerDispatchItem;
			}catch{}
		}
		
		void LootContainerOpened(object sender, ContainerOpenedEventArgs e)
		{
			//Patterned off Mag-Tools Looter
			try
			{
				//Check to see if previous container was still being IDd
				if(e.ItemGuid != mOpenContainer.ContainerGUID)
				{
					//This should close a new container and reopen the old one.  If you simply closed the old one, or reopened the old one.
					if(mOpenContainer.ContainerIsLooting) {Core.Actions.UseItem(mOpenContainer.ContainerGUID, 0);}
					return;
				}
				
				WorldObject container = Core.WorldFilter[e.ItemGuid];
				
				if(container == null) {return;}
				if(container.Name.Contains("Storage")) {return;}
				if(container.ObjectClass == ObjectClass.Corpse)
				{
					if(container.Name.Contains(Core.CharacterFilter.Name))
					{
						ghSettings.DeadMeList.RemoveAll(x => x.GUID == container.Id);
						return;
					}
					//Don't loot out permitted corpses.....
//					else if(PermittedCorpsesList.Count() > 0 && container.Name
//					{
//
//					}
					else
					{
						CheckContainer(container);
					}
				}
				if(container.Name.Contains("Chest") || container.Name.Contains("Vault") || container.Name.Contains("Reliquary"))
				{
					CheckContainer(container);
				}
			}
			catch{}
		}
		
		private void ServerDispatchItem(object sender, Decal.Adapter.NetworkMessageEventArgs e)
        {
        	int iEvent = 0;
            try
            {
            	if(e.Message.Type == AC_GAME_EVENT)
            	{
            		try
                    {
                    	iEvent = Convert.ToInt32(e.Message["event"]);
                    }
                    catch{}
                    if(iEvent == GE_IDENTIFY_OBJECT)
                    {
                    	 OnIdentItem(e.Message);
                    }
                    
            	}
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }  
		
		
		private void OnIdentItem(Decal.Adapter.Message pMsg)
		{
			try
			{
				if(!bItemHudEnabled) {return;}
        		int PossibleItemID = Convert.ToInt32(pMsg["object"]);
        		//Read and report manual IDs, but keep them out of 
        		if(PossibleItemID == Host.Actions.CurrentSelection && bReportItemStrings)
        		{
        			ManualCheckItemForMatches(new IdentifiedObject(Core.WorldFilter[PossibleItemID]));
        		}
        		if(!ItemIDListenList.Contains(PossibleItemID)) {return;}
        		CheckItemForMatches(new IdentifiedObject(Core.WorldFilter[PossibleItemID]));
			} 
			catch (Exception ex) {LogError(ex);}
		}
		
		
		private void CheckContainer(WorldObject container)
		{
			try
			{
				mOpenContainer.ContainerIsLooting = true;
				mOpenContainer.ContainerGUID = container.Id;
				WorldObject[] ContainerContents = Core.WorldFilter.GetByContainer(container.Id).ToArray();
				foreach(WorldObject wo in ContainerContents)
				{
					if(ItemExclusionList.FindIndex(x => x == wo.Id) < 0)
					{
						mOpenContainer.ContainerIOs.Add(new IdentifiedObject(wo));
					}
				}
				if(mOpenContainer.ContainerIOs.Count() == 0)
				{
					mOpenContainer.ContainerIsLooting = false;
				}
				else
				{
					foreach(IdentifiedObject IOItem in mOpenContainer.ContainerIOs)
					{
						SeparateItemsToID(IOItem);
					}
				}
			}catch{}
		}
		

		
		void SeparateItemsToID(IdentifiedObject IOItem)
		{
			try
			{
				//Get rid of non-existant items...
				if(!IOItem.isvalid)
				{
					IOItem.IOR = IOResult.nomatch;
					ItemExclusionList.Add(IOItem.Id);
					mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == IOItem.Id);
					StillLootingCheck();
					return;
				}
				
				//Flag items that need additional info to ID...
				if(!IOItem.HasIdData)
				{
					switch(IOItem.ObjectClass)
					{
						case ObjectClass.Armor:
							IdqueueAdd(IOItem.Id);
							mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
							ItemIDListenList.Add(IOItem.Id);
							return;
							
						case ObjectClass.Clothing:
							IdqueueAdd(IOItem.Id);
							mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
							ItemIDListenList.Add(IOItem.Id);
							return;
							
						case ObjectClass.Gem:
							if(IOItem.Aetheriacheck)
							{
								IdqueueAdd(IOItem.Id);
								mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
								ItemIDListenList.Add(IOItem.Id);
								return;
							}
							break;							
						case ObjectClass.Jewelry:
							IdqueueAdd(IOItem.Id);
							mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
							ItemIDListenList.Add(IOItem.Id);
							return;		
						case ObjectClass.MeleeWeapon:
							IdqueueAdd(IOItem.Id);
							mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
							ItemIDListenList.Add(IOItem.Id);
							return;
							
						case ObjectClass.MissileWeapon:	//bow = mastery 8, crossbow = mastery 9, atlan = mastery 10, don't ID rocks....
							if (IOItem.WeaponMasteryCategory == 8 | IOItem.WeaponMasteryCategory == 9 | IOItem.WeaponMasteryCategory == 10) 
							{
								IdqueueAdd(IOItem.Id);
								mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
								ItemIDListenList.Add(IOItem.Id);
								return;
							}
							break;							
						case ObjectClass.Misc:
							if(IOItem.Name.Contains("Essence"))
							{
								IdqueueAdd(IOItem.Id);
								mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
								ItemIDListenList.Add(IOItem.Id);
								return;
							}
							break;							
						case ObjectClass.WandStaffOrb:
							IdqueueAdd(IOItem.Id);
							mOpenContainer.ContainerIOs[mOpenContainer.ContainerIOs.FindIndex(x => x.Id == IOItem.Id)].IOR = IOResult.unknown;
							ItemIDListenList.Add(IOItem.Id);
							return;	
					}
					CheckItemForMatches(IOItem);
					
				}
			} catch (Exception ex) {LogError(ex);} 
			return;
		}
		
		
		
		void CheckItemForMatches(IdentifiedObject IOItem)
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
			}catch{}		
		}	
		
		
		void ManualCheckItemForMatches(IdentifiedObject IOItem)
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
				
				ReportStringToChat(IOItem.LinkString());
			}catch{}
		}
		

		private void CheckUnknownScrolls(ref IdentifiedObject IOScroll)
		{
			//TODO:  Refine this to make more useful if there is a community request
			try
			{
				if(!bscrolls7Enabled) 
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
			} catch{}
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
			} catch {}
			return;
		}
		
		private void CheckSalvageItem(ref IdentifiedObject IOItemSalvage)
		{
			try
			{
				//reference pointers and Linq functions don't mix
				IdentifiedObject IoItemSalvageMirror = IOItemSalvage;
				
				if(IOItemSalvage.SalvageWorkmanship > 0)
				{
					//If an active rule exists for this salvage type, review it for a match.  
					var salvagerulecheck = from allrules in SalvageRulesList
										where (allrules.material == IoItemSalvageMirror.IntValues(LongValueKey.Material)) &&
												(IoItemSalvageMirror.SalvageWorkmanship >= allrules.minwork) &&
						       					(IoItemSalvageMirror.SalvageWorkmanship <= (allrules.maxwork +0.99))
										select allrules;
					
					if(salvagerulecheck.Count() > 0)
					{
						IOItemSalvage.IOR = IOResult.salvage;
					}
				}
			}
			catch{}
			return;
		}
		
		private void CheckManaItem(ref IdentifiedObject IOItemMana)
		{
			try
			{
				if(IOItemMana.IntValues(LongValueKey.CurrentMana) > mLootManaMinimum)
				{
					//Irq:  TODO:  Cull manatanks when there is not a mana stone to eat them.  It's irritating.  Make a list of them for destruction as needed.
					//Irq:  TODO:  Add mana value or find it....
					IOItemMana.IOR = IOResult.manatank;
				}
			} catch {}
			return;
		}
		
		private void CheckValueItem(ref IdentifiedObject IOItemVal)
		{
			try
			{
				double ratio = ((double)IOItemVal.Value / (double)IOItemVal.Burden);
				if(ratio > mLootValBurdenRatioMinimum)
				{
					IOItemVal.IOR = IOResult.val;
				}
				else if(IOItemVal.Value > mLootValMinimum)
				{
					IOItemVal.IOR = IOResult.val;
				}
			} catch {}
			return;
		}
		
		
		//Irq:  This code is untested and only partially complete
		private void CheckRulesItem(ref IdentifiedObject IOItemWithIDReference)
		{
			
			//Irq:  Note to self:  Cloak IDs....cloaks w/spells are 352 = 1;  cloaks w/absorb are 352=2
			try
			{
				
				IdentifiedObject IOItemWithID = IOItemWithIDReference;

				var AppliesToListMatches = from rules in ItemRulesList
					where (rules.RuleAppliesTo & IOItemWithID.IntValues(LongValueKey.Category)) == IOItemWithID.IntValues(LongValueKey.Category)
					orderby rules.RulePriority
					select rules;
				
				if(AppliesToListMatches.Count() == 0) {return;}
				
				string RuleName;
				foreach(var rule in AppliesToListMatches)
				{	
					
					//DEBUG CODE
					WriteToChat(rule.RuleName);
					
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
//					
//					if(rule.RuleWeaponEnabledA || rule.RuleWeaponEnabledB || rule.RuleWeaponEnabledC || rule.RuleWeaponEnabledD)
//					{
//						bool[] ruletrue = {false, false, false, false};
//						if(rule.RuleWeaponEnabledA)
//						{	
//							if((rule.MSCleaveA == IOItemWithID.MSCleave && rule.WieldReqValueA == IOItemWithID.WieldReqValue && 
//							    IOItemWithID.WeaponMaxDamage >= rule.MaxDamageA && IOItemWithID.Variance <= rule.VarianceA))
//							     {ruletrue[0] = true;}
//						}
//						if(rule.RuleWeaponEnabledB)
//						{	
//							if((rule.MSCleaveB == IOItemWithID.MSCleave && rule.WieldReqValueB == IOItemWithID.WieldReqValue && 
//							    IOItemWithID.WeaponMaxDamage >= rule.MaxDamageB && IOItemWithID.Variance <= rule.VarianceB))
//							     {ruletrue[1] = true;}
//						}
//						if(rule.RuleWeaponEnabledC)
//						{	
//							if((rule.MSCleaveC == IOItemWithID.MSCleave && rule.WieldReqValueC == IOItemWithID.WieldReqValue && 
//							    IOItemWithID.WeaponMaxDamage >= rule.MaxDamageC && IOItemWithID.Variance <= rule.VarianceC))
//							     {ruletrue[2] = true;}
//						}					
//						if(rule.RuleWeaponEnabledD)
//						{	
//							if((rule.MSCleaveD == IOItemWithID.MSCleave && rule.WieldReqValueD == IOItemWithID.WieldReqValue && 
//							    IOItemWithID.WeaponMaxDamage >= rule.MaxDamageD && IOItemWithID.Variance <= rule.VarianceD))
//							     {ruletrue[3] = true;}
//						}
//						if(!ruletrue[0] && !ruletrue[1] && !ruletrue[2] && !ruletrue[3]) {RuleName = String.Empty; goto Next;}
//					}
					//Irquk:  Confirmed functional
					if(rule.RuleDamageTypes > 0)
					{
						if(!((rule.RuleDamageTypes & IOItemWithID.DamageType) == IOItemWithID.DamageType)) {RuleName = String.Empty; goto Next;}
					}
//					
//					//Armor Levels
//					if(rule.RuleArmorLevel > 0)
//					{
//						if(rule.RuleArmorLevel > IOItemWithID.ArmorLevel) {RuleName = String.Empty; goto Next;}
//					}
//					//Armor Types
////					if(rule.RuleArmorTypes >= 0)
////					{
////						
////					}
//					if(rule.RuleArmorCoverage > 0)
//					{
//						if((IOItemWithID.ArmorCoverage & rule.RuleArmorCoverage) != rule.RuleArmorCoverage) {RuleName = String.Empty; goto Next;}
//					}
//					if(rule.RuleUnenchantable)
//					{
//						if(IOItemWithID.IntValues(LongValueKey.Unenchantable) != 9999) {RuleName = String.Empty; goto Next;}
//					}
//					
//					
//					bool red = false;
//					bool yellow = false;
//					bool blue = false;
//					//Cloaks & Aetheria
//					if(rule.RuleRed || rule.RuleYellow || rule.RuleBlue)
//					{
//						if(IOItemWithID.WieldReqType == 7 && IOItemWithID.WieldReqValue == 225) {red = true;}
//						if(IOItemWithID.WieldReqType == 7 && IOItemWithID.WieldReqValue == 150) {yellow = true;}
//						if(IOItemWithID.WieldReqType == 7 && IOItemWithID.WieldReqValue  == 75) {blue = true;}
//						if(!red && !yellow && !blue) {RuleName = String.Empty; goto Next;}
//					}
//									
//					if(rule.RuleItemLevel > 0)
//					{
//						if(IOItemWithID.MaxItemLevel < rule.RuleItemLevel) {RuleName = String.Empty; goto Next;}
//					}
//					if(rule.RuleSpellNumber > 0)
//					{
//						int spellmatches = 0;
//						for(int i = 0; i < IOItemWithID.SpellCount; i++)
//						{
//							if(rule.RuleSpells.Contains(IOItemWithID.Spell(i))) {spellmatches++;}
//						}
//						//Irq:  Cloak IDs....cloaks w/spells are 352 = 1;  cloaks w/absorb are 352=2
//						if(rule.RuleSpells.Contains(10000)){if(IOItemWithID.IntValues((LongValueKey)352) == 2){spellmatches++;}}
//						if(spellmatches < rule.RuleSpellNumber) {RuleName = String.Empty; goto Next;}
//					}


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
			catch(Exception ex) {WriteToChat(ex.ToString());}
		}
		
		
		//Irq:  There is probably a more sophisticated way to do this with less code, but I like this as it's easy to follow.....
		//Irq:  It also has the advantage of only sifting out particular IDs which means that lifestones and such can't somehow sneak into the items page.
		private void EvaluateItemMatches(IdentifiedObject IOItem)
		{
			try
			{
				switch(IOItem.IOR)
				{
					case IOResult.unknown:
						ItemExclusionList.Add(IOItem.Id);
						mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == IOItem.Id);
						StillLootingCheck();
						return;
					case IOResult.rule:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == IOItem.Id);
						StillLootingCheck();
						//PlaySound?
						return;
					case IOResult.manatank:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == IOItem.Id);
						StillLootingCheck();
						//PlaySound?
						return;
					case IOResult.rare:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == IOItem.Id);
						StillLootingCheck();
						//PlaySound?
						return;
					case IOResult.salvage:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == IOItem.Id);
						StillLootingCheck();
						//PlaySound?
						return;
					case IOResult.spell:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == IOItem.Id);
						StillLootingCheck();
						//PlaySound?
						return;
					case IOResult.trophy:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == IOItem.Id);
						StillLootingCheck();
						//PlaySound?
						return;
					case IOResult.val:
						ItemTrackingList.Add(IOItem);
						ItemExclusionList.Add(IOItem.Id);
						mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == IOItem.Id);
						StillLootingCheck();
						//PlaySound?
						return;
					default:
						ItemExclusionList.Add(IOItem.Id);
						mOpenContainer.ContainerIOs.RemoveAll(x => x.Id == IOItem.Id);
						StillLootingCheck();
						return;
				}
			}catch{}
		}

		private void StillLootingCheck()
        {
        	try
        	{
        		bool StillLootingFlag = false;
        		
        		IdentifiedObject[] StillLootingCheckArray = mOpenContainer.ContainerIOs.ToArray();
        		if(StillLootingCheckArray.Count() > 0)
        		{
        			foreach(IdentifiedObject IOLootingCheck in StillLootingCheckArray)
	        		{
        				if(!StillLootingFlag)
        				{
        					 if(IOLootingCheck.IOR == IOResult.unknown) {StillLootingFlag = true;}
        				}
	        		}
        			mOpenContainer.ContainerIsLooting = StillLootingFlag;
        		}
        		else
        		{
        			mOpenContainer.ContainerIsLooting = false;
        		}
        		
        	}
        	catch{}
        }
			
		private bool InspectorTab = false;
		private bool InspectorUstTab = false;
		private bool InspectorSettingsTab = false;
		
		private HudView ItemHudView = null;
		private HudFixedLayout ItemHudLayout = null;
		private HudTabView ItemHudTabView = null;
		private HudFixedLayout ItemHudInspectorLayout = null;
		private HudFixedLayout ItemHudUstLayout = null;
		private HudFixedLayout ItemHudSettingsLayout = null;
		
		
		
		private HudList ItemHudInspectorList = null;
		private HudList.HudListRowAccessor ItemHudListRow = null;
		
		
		private HudList ItemHudUstList = null;
		private HudButton ItemHudUstButton = null;
		
		private const int ItemRemoveCircle = 0x60011F8;
			
    	private void RenderItemHud()
    	{
    		try{
    			    			
    			if(ItemHudView != null)
    			{
    				DisposeItemHud();
    			}			
    			
    			ItemHudView = new HudView("Inspector", 300, 220, new ACImage(0x6AA8));
    			ItemHudView.Theme = VirindiViewService.HudViewDrawStyle.GetThemeByName("Minimalist Transparent");
    			ItemHudView.UserAlphaChangeable = false;
    			ItemHudView.ShowInBar = false;
    			ItemHudView.Visible = true;
    			ItemHudView.UserResizeable = false;
    			ItemHudView.LoadUserSettings();
    			
    			ItemHudLayout = new HudFixedLayout();
    			ItemHudView.Controls.HeadControl = ItemHudLayout;
    			
    			ItemHudTabView = new HudTabView();
    			ItemHudLayout.AddControl(ItemHudTabView, new Rectangle(0,0,300,220));
    		
    			ItemHudInspectorLayout = new HudFixedLayout();
    			ItemHudTabView.AddTab(ItemHudInspectorLayout, "Inspector");
    			
    			ItemHudUstLayout = new HudFixedLayout();
    			ItemHudTabView.AddTab(ItemHudUstLayout, "Ust");
    			
    			ItemHudSettingsLayout = new HudFixedLayout();
    			ItemHudTabView.AddTab(ItemHudSettingsLayout, "Settings");
    			
    			ItemHudTabView.OpenTabChange += ItemHudTabView_OpenTabChange;
  				
    			RenderItemHudInspectorTab();
    			
				SubscribeLootEvents();
			  							
    		}catch(Exception ex) {WriteToChat(ex.ToString());}
    		
    	}
    	
    	private void ItemHudTabView_OpenTabChange(object sender, System.EventArgs e)
    	{
    		try
    		{
    			switch(ItemHudTabView.CurrentTab)
    			{
    				case 0:
    					DisposeItemHudUstTab();
    					DisposeItemHudSettingsTab();
    					RenderItemHudInspectorTab();
    					return;
    				case 1:
    					DisposeItemHudInspectorTab();
    					DisposeItemHudSettingsTab();
    					RenderItemHudUstTab();
    					return;
    				case 2:
    					DisposeItemHudInspectorTab();
    					DisposeItemHudUstTab();
    					RenderItemHudSettingsTab();
    					return;
    			}
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void RenderItemHudUstTab()
    	{
    		try
    		{
    			ItemHudUstButton = new HudButton();
    			ItemHudUstButton.Text = "Salvage List";
    			ItemHudUstLayout.AddControl(ItemHudUstButton, new Rectangle(75,0,150,20));
    			
    			ItemHudUstList = new HudList();
    			ItemHudUstList.AddColumn(typeof(HudPictureBox), 16, null);
    			ItemHudUstList.AddColumn(typeof(HudStaticText), 200, null);
    			ItemHudUstList.AddColumn(typeof(HudPictureBox), 16, null);
    			ItemHudUstLayout.AddControl(ItemHudUstList, new Rectangle(0,30,300,270));
		
    			
    			InspectorUstTab = true;
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void DisposeItemHudUstTab()
    	{
    		try
    		{
    			if(!InspectorUstTab){return;}
    			
    			ItemHudUstList.Dispose();
    			ItemHudUstButton.Dispose();
    			
    			InspectorUstTab = false;
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void RenderItemHudSettingsTab()
    	{
    		try
    		{
    			
    			InspectorSettingsTab = true;
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void DisposeItemHudSettingsTab()
    	{
    		try
    		{
    			if(!InspectorSettingsTab){return;}
    			
    			InspectorSettingsTab = false;
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	
    	
    	private void RenderItemHudInspectorTab()
    	{
    		try
    		{
    			ItemHudInspectorList = new HudList();
    			ItemHudInspectorLayout.AddControl(ItemHudInspectorList, new Rectangle(0,0,300,220));
				ItemHudInspectorList.ControlHeight = 16;	
				ItemHudInspectorList.AddColumn(typeof(HudPictureBox), 16, null);
				ItemHudInspectorList.AddColumn(typeof(HudStaticText), 230, null);
				ItemHudInspectorList.AddColumn(typeof(HudPictureBox), 16, null);
				
				ItemHudInspectorList.Click += (sender, row, col) => ItemHudInspectorList_Click(sender, row, col);	

				InspectorTab = true;				

    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void DisposeItemHudInspectorTab()
    	{
    		try
    		{
    			if(!InspectorTab){return;}
    			
    			ItemHudInspectorList.Click -= (sender, row, col) => ItemHudInspectorList_Click(sender, row, col);	 			
    			ItemHudInspectorList.Dispose(); 
    			
    			InspectorTab = false;
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	
    	
    	
    	private void DisposeItemHud()
    	{
    			
    		try
    		{
    			UnsubscribeLootEvents();
    			
    			ItemHudSettingsLayout.Dispose();
    			ItemHudUstLayout.Dispose();
    			ItemHudInspectorLayout.Dispose();   			
    			ItemHudTabView.Dispose();
    			ItemHudLayout.Dispose();
    			ItemHudView.Dispose();
    		}	
    		catch{}
    	}
    		
    	private void ItemHudInspectorList_Click(object sender, int row, int col)
    	{
    		try
			{
    			if(col == 0)
    			{
    				Host.Actions.SelectItem(ItemTrackingList[row].Id);
    				int textcolor;
    				switch(ItemTrackingList[row].IOR)
    				{
    					case IOResult.lifestone:
    						textcolor = 13;
    						break;
    					case IOResult.monster:
    						textcolor = 6;
    						break;
    					case IOResult.allegplayers:
    						textcolor = 13;
    						break;
    					case IOResult.npc:
    						textcolor = 3;
    						break;
    					default:
    						textcolor = 2;
    						break;
    				}
    				
    				
    				HudToChat(ItemTrackingList[row].LinkString(), textcolor);
    			}
    			if(col == 1)
    			{
    				Host.Actions.UseItem(ItemTrackingList[row].Id, 0);
    			}
    			if(col == 2)
    			{    				
    				ItemExclusionList.Add(ItemTrackingList[row].Id);
    				ItemTrackingList.RemoveAt(row);
    			}
				UpdateItemHud();
			}
			catch (Exception ex) { LogError(ex); }	
    	}
    		
	    private void UpdateItemHud()
	    {  	
	    	try
	    	{    		
	    		ItemHudInspectorList.ClearRows();
	    		   	    		
	    	    foreach(IdentifiedObject spawn in ItemTrackingList)
	    	    {
	    	    	ItemHudListRow = ItemHudInspectorList.AddRow();
	    	    	
	    	    	((HudPictureBox)ItemHudListRow[0]).Image = spawn.Icon + 0x6000000;
	    	    	((HudStaticText)ItemHudListRow[1]).Text = spawn.IORString() + spawn.Name;
	    	    	if(spawn.IOR == IOResult.trophy) {((HudStaticText)ItemHudListRow[1]).TextColor = Color.SlateGray;}
	    	    	if(spawn.IOR == IOResult.lifestone) {((HudStaticText)ItemHudListRow[1]).TextColor = Color.Blue;}
	    	    	if(spawn.IOR == IOResult.monster) {((HudStaticText)ItemHudListRow[1]).TextColor = Color.Orange;}
	    	    	if(spawn.IOR == IOResult.npc) {((HudStaticText)ItemHudListRow[1]).TextColor = Color.Yellow;}
	    	    	if(spawn.IOR == IOResult.portal)  {((HudStaticText)ItemHudListRow[1]).TextColor = Color.Purple;}
	    	    	if(spawn.IOR == IOResult.players)  {((HudStaticText)ItemHudListRow[1]).TextColor = Color.AntiqueWhite;}
	    	    	if(spawn.IOR == IOResult.allegplayers)  {((HudStaticText)ItemHudListRow[1]).TextColor = Color.DarkSlateBlue;}
					((HudPictureBox)ItemHudListRow[2]).Image = ItemRemoveCircle;
	    	    }
	       	}catch(Exception ex){WriteToChat(ex.ToString());}
	    	
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

//				WriteToChat("mPrioritizedRulesListEnabled.Count() = " + mPrioritizedRulesListEnabled.Count().ToString());
				
				for(int i = 0; i < mPrioritizedRulesListEnabled.Count(); i++)
				{
					
					var XRule = mPrioritizedRulesListEnabled[i];
					ItemRule tRule = new ItemRule();

					//WriteToChat("RuleName = " + (string)mPrioritizedRulesListEnabled[i].Element("Name").Value);
		        	
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
		        	if(!Int32.TryParse(XRule.Element("MasteryType").Value, out tRule.RuleMastery)) {tRule.RuleMastery = 0;}
		        	if(!Int32.TryParse(XRule.Element("WieldLevel").Value, out tRule.RuleWieldLevel)) {tRule.RuleWieldLevel = 0;}
		        	
					splitstring = ((string)XRule.Element("DamageType").Value).Split(',');
					if(splitstring.Length > 0)
					{
						sumarray = new int[splitstring.Length];      	
						for(int j = 0; j < splitstring.Length; j++){if(!Int32.TryParse(splitstring[j], out sumarray[j])){sumarray[j] = 0;}}
						tRule.RuleDamageTypes = sumarray.Sum();
					}
					
					WriteToChat("DamageType Sum = " + tRule.RuleDamageTypes.ToString());
					
					if(!Double.TryParse(XRule.Element("McModAttack").Value, out tRule.RuleMcModAttack)){tRule.RuleMcModAttack = 0;}
					
					if(tRule.RuleMcModAttack > 0) {tRule.RuleMcModAttack += (tRule.RuleMcModAttack*0.01) + 1;}  //convert for direct comparison
					
					if(!Double.TryParse(XRule.Element("MeleeDef").Value, out tRule.RuleMeleeD)){tRule.RuleMeleeD = 0;}
					
					if(tRule.RuleMeleeD > 0) {tRule.RuleMeleeD = (tRule.RuleMeleeD*0.01) + 1;}  //convert for direct comparison
					
					if(!Double.TryParse(XRule.Element("MagicDef").Value, out tRule.RuleMagicD)){tRule.RuleMagicD = 0;}
					
					if(tRule.RuleMagicD != 0) {tRule.RuleMagicD = (tRule.RuleMagicD*0.01) + 1;}  //convert for direct comparison
	
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
							if(tRule.MaxDamageA > 0) {tRule.VarianceA = (tRule.MaxDamageA - tint)/tRule.MaxDamageA;}
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
							if(tRule.MaxDamageB > 0) {tRule.VarianceB = (tRule.MaxDamageB - tint)/tRule.MaxDamageB;}
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
							if(tRule.MaxDamageC > 0) {tRule.VarianceC = (tRule.MaxDamageC - tint)/tRule.MaxDamageC;}
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
							if(tRule.MaxDamageD > 0) {tRule.VarianceD = (tRule.MaxDamageD - tint)/tRule.MaxDamageD;}
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
					
					splitstring = (XRule.Element("ArmorType").Value).Split(',');
					if(splitstring.Length > 0)
					{
						tRule.RuleArmorTypes = new int[splitstring.Length];
						for(int j = 0; j < splitstring.Length; j++)
						{
							if(!Int32.TryParse(splitstring[j], out tRule.RuleArmorTypes[j])) {tRule.RuleArmorTypes[j] = 0;}
						}
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
							tempint = 0;
							Int32.TryParse(splitstring[j], out tempint);
							if(tempint > 0) {CombineIntList.Add(tempint); tempint = 0;}
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
						tRule.RuleSpellNumber++;  //Add a spell number to indicate that the cloak spells are required.  Should increment from 0 to 1.
						for(int j = 0; j < splitstring.Length; j++)
						{
							tempint = 0;
							Int32.TryParse(splitstring[j], out tempint);
							if(tempint > 0) {CombineIntList.Add(tempint);}
						}
					}
					tRule.RuleSpells = CombineIntList.ToArray();
					CombineIntList.Clear();
					
					
					
					
					ItemRulesList.Add(tRule);	
				}
				WriteToChat("ItemRulesList:  " + ItemRulesList.Count().ToString());
				
			} catch(Exception ex1){WriteToChat(ex1.ToString());}
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
	        
	        //Irq:  Not sure we're keeping these
	        //bool bRuleTradeBotOnly = false;
	        //bool bRuleTradeBot = false;
	        
	        public int RuleWieldAttribute;
	        public int RuleMastery;
	        public int RuleDamageTypes;
	        public double RuleMcModAttack;
	        public double RuleMeleeD;
	        public double RuleMagicD;
	        
	        //Weapon Matching
	        public bool RuleWeaponEnabledA;
	        public bool MSCleaveA;
	        public int WieldReqValueA;
	        public int MaxDamageA;
	        public int VarianceA;
	        public bool RuleWeaponEnabledB;
	        public bool MSCleaveB;
	        public int WieldReqValueB;
	        public int MaxDamageB;
	        public int VarianceB;
	        public bool RuleWeaponEnabledC;
	        public bool MSCleaveC;
	        public int WieldReqValueC;
	        public int MaxDamageC;
	        public int VarianceC;
	        public bool RuleWeaponEnabledD;
	        public bool MSCleaveD;
	        public int WieldReqValueD;
	        public int MaxDamageD;
	        public int VarianceD;
	        
	        public int RuleArmorLevel;
	        public int[] RuleArmorTypes;
	        public int[] RuleArmorSet;
	        public int RuleArmorCoverage;
	        //public bool RuleAnySet;
	        public bool RuleUnenchantable;

	        public int[] RuleSpells;
	        public int RuleSpellNumber; 	
		}
		
		
		
		
		
		
		
	}
}
