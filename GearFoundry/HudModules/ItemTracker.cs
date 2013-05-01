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
						DeadMeCoordinatesList.RemoveAll(x => x.GUID == container.Id);
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
				//Only pull active rules that "ApplyTo" the category of object hitting the rules comparison
				//Irquk:  Confirmed Functional
				var AppliesToListMatches = from rules in ItemRulesList
					where (rules.RuleAppliesTo & IOItemWithID.IntValues(LongValueKey.Category)) == IOItemWithID.IntValues(LongValueKey.Category)
					orderby rules.RulePriority
					select rules;
				
				//No matches?  return
				//Irquk Conrimed Functional
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
//					//WieldRequiredAttribute
//					if(rule.RuleWieldAttribute > 0)
//					{
//						WriteToChat("RuleWRA: " + rule.RuleWieldAttribute.ToString());
//						WriteToChat("ItemWRA: " + IOItemWithID.WieldReqAttribute.ToString());
//						WriteToChat("ItemWRA2: " + IOItemWithID.WieldReqAttribute2.ToString());
//						if(IOItemWithID.WieldReqType != 7) {if(IOItemWithID.WieldReqAttribute !=  rule.RuleWieldAttribute){RuleName = String.Empty; goto Next;}}
//						if(IOItemWithID.WieldReqType2 != 7 && IOItemWithID.WieldReqAttribute2 > 0) {if(IOItemWithID.WieldReqAttribute2 != rule.RuleWieldAttribute){RuleName = String.Empty; goto Next;}}
//					}
//					//WeaponMastery
//					if(rule.RuleMastery > 0)
//					{
//						if(IOItemWithID.WeaponMasteryCategory != rule.RuleMastery) {RuleName = String.Empty; goto Next;}
//					}
//					//Melee Defense
//					if(rule.RuleMeleeD > 0)
//					{
//						if(rule.RuleMeleeD > IOItemWithID.WeaponMeleeBonus) {RuleName = String.Empty; goto Next;}
//					}
//					//Mana Conversion, +modifer + attack for weapons
//					if(rule.RuleMcModAttack > 0)
//					{
//						if(IOItemWithID.ObjectClass == ObjectClass.WandStaffOrb)
//						{
//							if(rule.RuleMcModAttack > IOItemWithID.WeaponManaCBonus) {RuleName = String.Empty; goto Next;}
//						}
//						if(IOItemWithID.ObjectClass == ObjectClass.MissileWeapon)
//						{
//							if(rule.RuleMcModAttack > IOItemWithID.WeaponMissileModifier) {RuleName = String.Empty; goto Next;}
//						}
//						if(IOItemWithID.ObjectClass == ObjectClass.MeleeWeapon)
//						{
//							if(rule.RuleMcModAttack > IOItemWithID.WeaponAttackBonus) {RuleName = String.Empty; goto Next;}
//						}
//					}
//					//Magic Defense (add missile defense bonus as well?)
//					if(rule.RuleMagicD > 0)
//					{
//						if(rule.RuleMagicD > IOItemWithID.WeaponMagicDBonus) {RuleName = String.Empty; goto Next;}
//					}
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
//					if(rule.RuleDamageTypes > 0)
//					{
//						if(!((rule.RuleDamageTypes & IOItemWithID.DamageType) == IOItemWithID.DamageType)) {RuleName = String.Empty; goto Next;}
//					}
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
		
		//Irq:  Begin Item hud....
		
		private HudView ItemHudView = null;
		private HudFixedLayout ItemHudLayout = null;
		private HudTabView ItemHudTabView = null;
		private HudFixedLayout ItemHudTabLayout = null;
		private HudList ItemHudList = null;
		private HudList.HudListRowAccessor ItemHudListRow = null;
		
		//Assembly tests
		
		private const int ItemRemoveCircle = 0x60011F8;
			
    	private void RenderItemHud()
    	{
    		try{
    			    			
    			if(ItemHudView != null)
    			{
    				DisposeItemHud();
    			}			
    			
    			ItemHudView = new HudView("Item", 300, 220, new ACImage(0x107E));
    			ItemHudView.UserAlphaChangeable = false;
    			ItemHudView.ShowInBar = true;
    			ItemHudView.UserResizeable = false;
    			ItemHudView.Theme = VirindiViewService.HudViewDrawStyle.GetThemeByName("Minimalist Transparent");
    			
    			ItemHudLayout = new HudFixedLayout();
    			ItemHudView.Controls.HeadControl = ItemHudLayout;
    			
    			ItemHudTabView = new HudTabView();
    			ItemHudLayout.AddControl(ItemHudTabView, new Rectangle(0,0,300,220));
    		
    			ItemHudTabLayout = new HudFixedLayout();
    			ItemHudTabView.AddTab(ItemHudTabLayout, "Item");
    			
    			ItemHudList = new HudList();
    			ItemHudTabLayout.AddControl(ItemHudList, new Rectangle(0,0,300,220));
				ItemHudList.ControlHeight = 16;	
				ItemHudList.AddColumn(typeof(HudPictureBox), 16, null);
				ItemHudList.AddColumn(typeof(HudStaticText), 230, null);
				ItemHudList.AddColumn(typeof(HudPictureBox), 16, null);
				
				ItemHudList.Click += (sender, row, col) => ItemHudList_Click(sender, row, col);		

				SubscribeLootEvents();
			  							
    		}catch(Exception ex) {WriteToChat(ex.ToString());}
    		
    	}
    	
    	void DisposeItemHud()
    	{
    			
    		try
    		{
    			UnsubscribeLootEvents();
    			
    			ItemHudList.Click -= (sender, row, col) => ItemHudList_Click(sender, row, col);		
    			ItemHudList.Dispose();
    			ItemHudLayout.Dispose();
    			ItemHudTabLayout.Dispose();
    			ItemHudTabView.Dispose();
    			ItemHudView.Dispose();		
    		}	
    		catch{}
    	}
    		
    	void ItemHudList_Click(object sender, int row, int col)
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
	    		ItemHudList.ClearRows();
	    		   	    		
	    	    foreach(IdentifiedObject spawn in ItemTrackingList)
	    	    {
	    	    	ItemHudListRow = ItemHudList.AddRow();
	    	    	
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
		
		
		
		
		
		
		
		
		
	}
}
