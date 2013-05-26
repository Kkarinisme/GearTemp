using System;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using System.IO;
using System.Collections.ObjectModel;
using System.Text;
using Decal.Filters;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System.Collections;
using System.Collections.Generic;

namespace GearFoundry
{

	public partial class PluginCore
	{
		
		internal enum IOResult
        {
            nomatch = 0,
            allmonster,
            needsident,
            players,
            allegplayers,
            fellowplayer,
            portal,
            lifestone,
            trophy,
            salvage,
            rare,
            spell,
            rule,
            monster,
            corpseselfkill,
            corpsefellowkill,
            corpsepermitted,
            corpsewithrare,
            corpseofself,
            allcorpses,
            val,
            manatank,
            npc,
            allnpcs,
           	unknown
        }
		

		public class IdentifiedObject
		{
			// establishes class IdentifiedObject to hold properties associted  with world objects
		
			// wo 
			private WorldObject wo;
		
			public IdentifiedObject(WorldObject obj)
			{
				wo = obj;
			}
		
			public IdentifiedObject()
			{
			}
		
			//wrappers: the overloads of function Values in wrappers.worldobject makes me type too much -Losado
			//wrappers below allow direct access to all world object properties not expressly
			//included in the IdentifiedObject (IO) data class.  Many properties are expressly defined and loaded.  -Irquk
			public int IntValues(Decal.Adapter.Wrappers.LongValueKey eval) {
				return wo.Values(eval); 
			}
			public double DblValues(Decal.Adapter.Wrappers.DoubleValueKey eval) {
				return wo.Values(eval); 
			}
			public string StringValues(Decal.Adapter.Wrappers.StringValueKey eval) {
				return wo.Values(eval);
			}
			public bool BoolValues(Decal.Adapter.Wrappers.BoolValueKey eval) {
				return wo.Values(eval); 
			}
			
			//Irq:  Added to reduce number of components moving around in NotifyObject
			//Irq:  eSr set to .needsident in order ot init. matching function properly.

			internal IOResult IOR = IOResult.unknown;
			public bool addtoloot;
			public bool notify;
			public string rulename;
			public double DistanceAway;
//			public rule matched_rule;
			
			public List<DebuffSpell> DebuffSpellList = new List<DebuffSpell>();
						
			public class DebuffSpell
			{
				public int SpellId = 0;
				public DateTime SpellCastTime = DateTime.Now;
				public double SecondsRemaining = 0;
			}
			
			public string IORString()
			{
				switch(IOR)
				{
					case IOResult.portal:
						return "(Portal) ";
					case IOResult.players:
						return "(Player) ";
					case IOResult.lifestone:
						return "(Lifestone) ";
					case IOResult.trophy:
						return "(Trophy) ";
					case IOResult.rare:
						return "(Rare) ";
					case IOResult.spell:
						return "(Spell) ";
					case IOResult.rule:
						return "(" + rulename + ") ";
					case IOResult.monster:
						return "(Mob) ";
					case IOResult.corpseselfkill:
						return "(Corpse) ";
					case IOResult.corpsepermitted:
						return "(Corpse:Permitted) ";
					case IOResult.corpsewithrare:
						return "(Corpse:Rare) ";
					case IOResult.corpseofself:
						return "(Corpse:Self) ";
					case IOResult.corpsefellowkill:
						return "(Corpse:Fellow) ";
					case IOResult.val:
						return "(Value) ";
					case IOResult.manatank:
						return "(Manatank) ";
					case IOResult.allegplayers:
						return "(Allegiance) ";
					case IOResult.npc:
						return "(NPC) ";
					case IOResult.salvage:
						return "(Salvage) ";
					default:
						return String.Empty;
				}
					
            
			}
			
			public string DistanceString()
			{
				return " <" + (DistanceAway * 100).ToString("0") + ">";
			}
		
			//not in worldfilter set by OnIdentObject:
			private int mHealthMax;
			private int mHealthCurrent;
			private int mStaminaMax;
			private int mStaminaCurrent;
			private int mManaMax;
			private int mManaCurrent;
			
			//wo.properties used in loot selection
			public int ArmorSet
			{
				get 
				{
					if (wo.Values(LongValueKey.ArmorSet) > 0) {return wo.Values((LongValueKey.ArmorSet));}
					else {return 0;}
				}
			}	
			public int WieldReqValue2
			{
				get
				{
					if (wo.Values((LongValueKey)272) > 0) {return wo.Values((LongValueKey)272);}
					else {return 0;}
				}
			}
			public int WieldReqAttribute2
			{
				get
				{
					if (wo.Values((LongValueKey)271) > 0) {return wo.Values((LongValueKey)271);}
					else {return 0;}
				}
			}
			public int WieldReqType2
			{
				get
				{
					if (wo.Values((LongValueKey)270) > 0) {return wo.Values((LongValueKey)270);}
					else {return 0;}
				}
			}
			public int WieldReqValue 
			{
				get
				{
					if (wo.Values(LongValueKey.WieldReqValue) > 0) {return wo.Values(LongValueKey.WieldReqValue);}
					else {return 0;}
				}
			}	
			public int WieldReqAttribute
			{
				get
				{
					if (wo.Values(LongValueKey.WieldReqAttribute) > 0) {return wo.Values(LongValueKey.WieldReqAttribute);}
					else {return 0;}
				}
			}
			public int ArmorLevel
			{
				get 
				{
					if (wo.Values(LongValueKey.ArmorLevel) > 0) {return wo.Values(LongValueKey.ArmorLevel);}
					else {return 0;}
				}
			}	
			public int WieldSlot
			{	
				get
				{
					if (wo.Values(LongValueKey.EquipableSlots) > 0) {return wo.Values(LongValueKey.EquipableSlots);}
					else {return 0;}
				}
			}
			public int MissileType 
			{
				get
				{
					if (wo.Values(LongValueKey.MissileType) > 0) {return wo.Values(LongValueKey.MissileType);}
					else {return 0;}
				}
			}
			public int RareID
			{
				get 
				{
					if (wo.Values(LongValueKey.RareId) > 0) {return wo.Values(LongValueKey.RareId);}
					else {return 0;}
				}
			}
			public int DamageType 
			{
				get
				{		
					if(wo.ObjectClass == ObjectClass.MissileWeapon || wo.ObjectClass == ObjectClass.MeleeWeapon)
					{
						if (wo.Values(LongValueKey.DamageType) > 0) {return wo.Values(LongValueKey.DamageType);}
						else {return 0;}
					}
					else if(wo.ObjectClass == ObjectClass.WandStaffOrb)
					{
						if (wo.Values(LongValueKey.WandElemDmgType) > 0) {return wo.Values(LongValueKey.WandElemDmgType);}
						else {return 0;}
					}
					else if(wo.ObjectClass == ObjectClass.Misc)
					{
						//IconOutline 256 = Acid
						//32 = fire
						//128 = frost
						//64 = lightning
						//1 = golem
						//Reads the outline and converts them to standard elemental types for essences
						switch(wo.Values(LongValueKey.IconOutline))
						{
							case 1:
								return 4;
							case 32:
								return 16;
							case 64:
								return 64;
							case 128:
								return 8;
							case 256:
								return 32;
							default:
								return 0;
						}
					}
					else {return 0;}	
				}
			}
			public int ElementalDmgBonus 
			{
				get
				{
					if (wo.Values(LongValueKey.ElementalDmgBonus) > 0) {return wo.Values(LongValueKey.ElementalDmgBonus);}
					else {return 0;}
				}
			}
			public int Burden
			{
				get 
				{
					if (wo.Values(LongValueKey.Burden) > 0) {return wo.Values(LongValueKey.Burden);}
					else {return 0;}
				}
			}
			public int Value 
			{
				get 
				{
					if (wo.Values(LongValueKey.Value) > 0) {return wo.Values(LongValueKey.Value);}
					else {return 0;}
				}
			}
			public double Variance 
			{
				get 
				{
					if (wo.Values(DoubleValueKey.Variance) > 0) {return wo.Values(DoubleValueKey.Variance);}
					else {return 0;}
				}
			}
			public int ArmorCoverage 
			{
				get 
				{
					if (wo.Values(LongValueKey.Coverage) > 0) {return wo.Values(LongValueKey.Coverage);}
					else {return 0;}
				}
			}
			public int Workmanship 
			{	//NOTE:  This property requires an ID before it is set on wo
				get 
				{
					if (wo.Values(LongValueKey.Workmanship) > 0) {return wo.Values(LongValueKey.Workmanship);}
					else {return 0;}
				}
			}
			public double SalvageWorkmanship 
			{   //NOTE:  This property DOES NOT require an ID before it is set on wo
				get 
				{
					if (wo.Values(DoubleValueKey.SalvageWorkmanship) > 0) {return wo.Values(DoubleValueKey.SalvageWorkmanship);}
					else {return 0;}
				}
			}
			public int MaxItemLevel 
			{	//wo LongValueKey@319 contains max item level
				get
				{	
					if (wo.Values((LongValueKey)319) > 0){return wo.Values((LongValueKey)319);}
					else {return 0;}
					
				}
			}	
			public int WeaponMasteryCategory 
			{	//wo LongValueKey@353 contains WeaponMastery
				get
				{
					if(wo.ObjectClass == ObjectClass.MeleeWeapon || wo.ObjectClass == ObjectClass.MissileWeapon)
					{
						if (wo.Values((LongValueKey)353) > 0) {return wo.Values((LongValueKey)353);}
						else {return 0;}
					}
					if(wo.ObjectClass == ObjectClass.Misc)
					{
						switch(wo.Values(LongValueKey.Icon))
						{
							case 7664:
							case 29738:
							case 4154:
								return 1;  //Naturalist
							case 6978:
							case 9217: 
							case 29743:
							case 29739:
						    	return 2;  //Primalist
						    case 13383:
						    case 5828:
						    case 4646:
						    	return 3;  //Necro
						    default: 
						    	return 0;
						}
					}
					else {return 0;}
				}
			}
			public int EssenceDam
			{	//wo LongValueKey@370 contains
				get
				{
					if (wo.Values((LongValueKey)370) > 0) {return wo.Values((LongValueKey)370);}
					else {return 0;}
				}
			}
			public int EssenceDamResist 
			{	//wo LongValueKey@371 contains 
				get
				{
					if (wo.Values((LongValueKey)371) > 0) {return wo.Values((LongValueKey)371);}
					else {return 0;}
				}
			}
			public int EssenceCrit 
			{	//wo LongValueKey@372 contains
				get
				{
					if (wo.Values((LongValueKey)372) > 0) {return wo.Values((LongValueKey)372);}
					else {return 0;}
				}
			}
			public int EssenceCritResist
			{	//wo LongValueKey@373 contains
				get
				{
					if (wo.Values((LongValueKey)373) > 0) {return wo.Values((LongValueKey)373);}
					else {return 0;}
				}
			}
			public int EssenceCritDam
			{
				get
				{
					if (wo.Values((LongValueKey)374) > 0) {return wo.Values((LongValueKey)373);}
					else {return 0;}
				}
			}
			public int EssenceCritDamResist
			{
				get
				{
					if (wo.Values((LongValueKey)375) > 0) {return wo.Values((LongValueKey)373);}
					else {return 0;}
				}
			}
			public int EssenceSummoningSkill
			{	//wo LongValueKey@367 contains
				get
				{
					if (wo.Values((LongValueKey)367) > 0) {return wo.Values((LongValueKey)367);}
					else {return 0;}
				}
			}
			public int WeaponMaxDamage
			{
				get
				{
					if(wo.ObjectClass == ObjectClass.MeleeWeapon)
					{
						if(wo.Values(LongValueKey.MaxDamage) > 0) {return wo.Values(LongValueKey.MaxDamage);}
						else{return 0;}
					}
					if(wo.ObjectClass == ObjectClass.MissileWeapon)
					{
						if(wo.Values(LongValueKey.ElementalDmgBonus) > 0) {return wo.Values(LongValueKey.ElementalDmgBonus);}
						else{return 0;}
					}
					else if(wo.ObjectClass == ObjectClass.WandStaffOrb)
					{
						if(wo.Values(DoubleValueKey.ElementalDamageVersusMonsters) > 0) 
						{
							return Convert.ToInt32((wo.Values(DoubleValueKey.ElementalDamageVersusMonsters)-1)*100);
						}
						else{return 0;}
					}
					else 
					{
						return 0;
					}
				}
			}
			public int WieldReqType 
			{
				get 
				{
					if (wo.Values(LongValueKey.WieldReqType) > 0) {return wo.Values(LongValueKey.WieldReqType);}
					else {return 0;}
				}
			}
			public int SpellCount 
			{
				get { return wo.SpellCount; }
			}
			public int Id 
			{
				get { return wo.Id; }
			}
			public bool HasIdData 
			{
				get { return wo.HasIdData; }
			}
			public int Icon 
			{
				get { return wo.Icon; }
			}	
			public string Name
			{
				get { return wo.Name; }
			}
			public Decal.Adapter.Wrappers.ObjectClass ObjectClass 
			{
				get { return wo.ObjectClass; }
			}
			public Decal.Adapter.Wrappers.CoordsObject Coordinates 
			{
				get 
				{
					//Todo not always true, but don't care for now -Losado
					if (wo.Container != 0) 
					{
						return null;
						//Not sure I do either -Irquk
					}
					return wo.Coordinates();
				}
			}
			public int Container 
			{
				get { return wo.Container; }
			}
		
		
			//wo.properties not readily available from Decal.Adapter.Wrappers (calculated)
			public bool Aetheriacheck
			{
				get
				{
					if (wo.Values(LongValueKey.EquipableSlots) == (0x10000000 | 0x20000000 | 0x40000000)) {return true;}
					else {return false;}
				}
			}
			public int WeaponSubtype
			{
				get
				{
					//Weapons w/Wield Reqs use that value, Weapons w/o WieldReqs use EquipSkill (possibly could  use equipskill for all
					//TODO:  Check and see if EquipSkill can replace WieldReqAttribute entirely here.
                    
					if (wo.Values(LongValueKey.WieldReqAttribute) > 0) {return wo.Values(LongValueKey.WieldReqAttribute);}
					else if (wo.Values(LongValueKey.EquipSkill) > 0) {return wo.Values(LongValueKey.EquipSkill);}
					else {return 0;}
				}
			}
			public double MinDamage
			{
				get
				{
					if (wo.Values(DoubleValueKey.Variance) > 0) 
					{
						return Math.Round((wo.Values(LongValueKey.MaxDamage) - wo.Values(LongValueKey.MaxDamage) * wo.Values(DoubleValueKey.Variance)), 2);
					} 
					else {return 0;}
				}
			}
			public bool isvalid
			{
				get
				{
					return Convert.ToBoolean(wo != null);
				}
			}
			public double DamageVsMonsters
			{
				get
				{
					if (wo.Values(DoubleValueKey.ElementalDamageVersusMonsters) > 0)
					{
						return wo.Values(DoubleValueKey.ElementalDamageVersusMonsters);
					} 
					else {return 0;}
				}
			}		
			public double WeaponMissileModifier
			{
				get
				{
					if (wo.Values(DoubleValueKey.DamageBonus) > 0)
					{
						return wo.Values(DoubleValueKey.DamageBonus);
					} 
					else {return 0;}
				}
			}		
			public double WeaponMagicDBonus
			{
				get
				{
					if (wo.Values(DoubleValueKey.MagicDBonus) > 0) 
					{
						return wo.Values(DoubleValueKey.MagicDBonus);
					} 
					else {return 0;}
				}
			}	
			public double WeaponMissileDBonus
			{
				get
				{
					if (wo.Values(DoubleValueKey.MissileDBonus) > 0) 
					{
						return wo.Values(DoubleValueKey.MissileDBonus);
					} 
					else {return 0;}
				}
			}
			
			
			public double WeaponManaCBonus
			{
				get
				{
					if (wo.Values(DoubleValueKey.ManaCBonus) > 0) 
					{
						return wo.Values(DoubleValueKey.ManaCBonus) + 1;
					} 
					else {return 0;}
				}
			}
			public double WeaponAttackBonus
			{
				get
				{
					if (wo.Values(DoubleValueKey.AttackBonus) > 0) 
					{
						return wo.Values(DoubleValueKey.AttackBonus);
					} 
					else {return 0;}
				}
			}
			public double WeaponMeleeBonus
			{
				get
				{
					if (wo.Values(DoubleValueKey.MeleeDefenseBonus) > 0) 
					{
						return wo.Values(DoubleValueKey.MeleeDefenseBonus);
					} 
					else {return 0;}
				}
			}
			public bool MSCleave 
			{
				get 
				{
					if (wo.Values((LongValueKey)292) == 2) {return true;}
					else if (wo.Values((LongValueKey)47) == 160) {return true;}
					else if (wo.Values((LongValueKey)47) == 320) {return true;}
					else {return false;}
				}
			}
			public int Spell (int idx) 
			{
				return wo.Spell(idx);
			}
			public int wieldlvl
			{
				get
				{
					if (wo.Values(LongValueKey.WieldReqType) == 7) 
					{
						return wo.Values(LongValueKey.WieldReqValue, 0);
					}
					else { return 0; }
				}
			}
			public int EssenceLevel
			{
				get
				{
					switch(wo.Values(LongValueKey.IconOverlay))
					{
						case 29730:
							return 50;
						case 29731:
							return 80;
						case 29732:
							return 100;
						case 29733:
							return 125;
						case 29734:
							return 150;
						case 29735:
							return 180;
						case 29736:
							return 200;
						default:
							return 0;			
					}	
				}
			}
			

			

			//wo.properites which require an ID to calculate (pushed from outside)
			//This XP value is pushed in from notifyobject no.itemxp = value
			//TODO:  Review how to automate the ID and improve this.....
			private long mItemxp;
			private long mnextitemlvlxp;
			public long Itemxp {
				get { return mItemxp; }
				internal set { mItemxp = value; }
			}
			public long NextItemlvlxp 
			{
				get 
				{
					int x = CurrentItemLevel();
					int nextlvl = x + 1;
					if (nextlvl <= MaxItemLevel & mItemxp != 0) 
					{
						//aetheria =  2^N-1 billion XP
						//rare = 2000000000
						long totalxpneededforlevel = 0;
						if (wo.Values(LongValueKey.RareId) == 0) 
						{
							totalxpneededforlevel = Convert.ToInt64(((Math.Pow(2, nextlvl)) * 1000000000) - 1000000000);
						} else 
						{
							totalxpneededforlevel = nextlvl * 2000000000;
						}
						mnextitemlvlxp = totalxpneededforlevel - mItemxp;
					}
					return mnextitemlvlxp;
				}
				internal set { mnextitemlvlxp = value; }
			}
			public int CurrentItemLevel()
			{
				//defines CurrentItemLevel and calculates it
				int x = MaxItemLevel;
				long result = 0;
				int lvl = 0;
				if (MaxItemLevel > 0) {
					//1,2,4,8,16
					if (wo.Values(LongValueKey.RareId) == 0) {
						if (mItemxp < 1000000000) {
							lvl = 0;
						} else if (mItemxp < 3000000000L) {
							lvl = 1;
						} else if (mItemxp < 7000000000L) {
							lvl = 2;
						} else if (mItemxp < 15000000000L) {
							lvl = 3;
						} else if (mItemxp < 31000000000L) {
							lvl = 4;
						} else {
							lvl = 5;
						}
					//2000000000 per level
					} else {
						lvl = Convert.ToInt32(mItemxp / 2000000000);
					}
		
					return lvl;
				}
				return Convert.ToInt32(result);
			}	
			public int HealthMax {
				get { return mHealthMax; }
				set { mHealthMax = value; }
			}		
			public int HealthCurrent {
				get { return mHealthCurrent; }
				set { mHealthCurrent = value; }
			}
			public int StaminaMax{
				get { return mStaminaMax; }
				set { mStaminaMax = value; }
			}
			public int StaminaCurrent{
				get { return mStaminaCurrent; }
				set { mStaminaCurrent = value; }
			}
			public int ManaMax{
				get { return mManaMax; }
				set { mManaMax = value; }
			}
			public int ManaCurrent{
				get { return mManaCurrent; }
				set { mManaCurrent = value; }
			}
			
	
			//StringBuilders for ToString() override below...
			private string CurrentItemLevelString()
			{
				if (MaxItemLevel > 0) {return "(" + CurrentItemLevel() + "/" + MaxItemLevel + ")";}
				else return String.Empty;
			}	
			private string SlayerString()
			{
				string slaystring = String.Empty;
				if(wo.Values(LongValueKey.SlayerSpecies) > 0)
				{
					int idx = SpeciesIndex.FindIndex(x => x.ID == wo.Values(LongValueKey.SlayerSpecies));
					if(idx > 0) {slaystring = ", " + (SpeciesIndex[idx].name) +" Slayer";}
					else{slaystring += ("Unknown Slayer: " + wo.Values(LongValueKey.SlayerSpecies).ToString());}
				}
				return slaystring;
							
			}
			private string ImbueString()
			{
				string imbstring = string.Empty;
				if(wo.Values(LongValueKey.Imbued) > 0)
				{
					int idx = ImbueList.FindIndex(x => x.ID == wo.Values(LongValueKey.Imbued));
					if(idx >= 0) {imbstring = ", " + ImbueList[idx].name;}
					else{imbstring += (", Unk Imbue " + wo.Values(LongValueKey.Imbued).ToString("0x"));}
				}
				return imbstring;
			}
			private string SpellDescriptions()
			{
				//Lookup function for spelldata
				string strspells = string.Empty;
				for (int i = 0; i <= wo.SpellCount - 1; i++) 
				{
					strspells += ", " + SpellIndex[wo.Spell(i)].spellname;
				}
				return strspells;
			}			
			private string SetString()
			{
				string name = string.Empty;
				if (ArmorSet > 0) 
				{   //Could go directly to [ArmorSet] but this  won't throw an exception if it's not there
					int idx = SetsIndex.FindIndex(x => x.ID == ArmorSet);
					if(idx > 0) {name = SetsIndex[idx].name;}
					else { name = "Unknown Set " + ArmorSet.ToString("0x");}
				}
				return name;
			}
			private string ALString()
			{
				//converts armor level to string
				string result = string.Empty;
				if(wo.Values(LongValueKey.ArmorLevel) > 0) {result = ", AL " + wo.Values(LongValueKey.ArmorLevel);}
				return result;
			}
			private string ActivateString()
			{
				//converts activation requirements to string
				string result = string.Empty;
				int SkillLevelReq = wo.Values(LongValueKey.SkillLevelReq);
				if (SkillLevelReq > 0) {
					int ActivationReqSkillId = wo.Values(LongValueKey.ActivationReqSkillId);
					result = SkillIndex[ActivationReqSkillId].name;
					result = ", " + result + " " + SkillLevelReq + " to Activate";
				}
				return result;
			}
			private string LoreString()
			{
				if(wo.Values(LongValueKey.LoreRequirement) > 0)
				{
					return ", Lore " + wo.Values(LongValueKey.LoreRequirement).ToString();
				}
				else return String.Empty;
			}
			private string RankString()
			{
				if(wo.Values(LongValueKey.Rank) > 0)
				{
					return "Rank" + wo.Values(LongValueKey.Rank).ToString();
				}
				else {return String.Empty;}
			}
			private string RaceString()
			{
				if(wo.Values(LongValueKey.Heritage) > 0)
				{
					return "," + HeritageIndex[wo.Values(LongValueKey.Heritage)].name;
				}
				else {return String.Empty;}
			}	
			private string CraftString()
			{
				if(wo.Values(DoubleValueKey.SalvageWorkmanship) > 0)
				{
					return ", Craft " + Convert.ToInt32(wo.Values(DoubleValueKey.SalvageWorkmanship)).ToString();
				}
				else {return String.Empty;}
			}
			private string ProtsString()
			{
				//converts base protections to string
				string result = string.Empty;
				if (wo.Values(LongValueKey.Unenchantable) != 0) {
					result = wo.Values(DoubleValueKey.SlashProt).ToString("0.00;") + "\\";
					result += wo.Values(DoubleValueKey.PierceProt).ToString("0.00")  + "\\";
					result += wo.Values(DoubleValueKey.BludgeonProt).ToString("0.00")  + "\\";
					result += wo.Values(DoubleValueKey.FireProt).ToString("0.00")  + "\\";
					result += wo.Values(DoubleValueKey.ColdProt).ToString("0.00")  + "\\";
					result += wo.Values(DoubleValueKey.AcidProt).ToString("0.00;")  + "\\";
					result += wo.Values(DoubleValueKey.LightningProt).ToString("0.00")  + "\\";
					result = ", [" + result + "]";
				}
				return result;
			}
			private string ValueString()
			{
				if(wo.Values(LongValueKey.Value) > 0)
				{
					return ", Val " + wo.Values(LongValueKey.Value).ToString();
				}
				else{return String.Empty;}
			}
			private string BurdenString()
			{
				if(wo.Values(LongValueKey.Burden) > 0)
				{
					return ", Bu " + wo.Values(LongValueKey.Burden).ToString();
				}
				else{return String.Empty;}
			}
			private string TinkersString()
			{
				if(wo.Values(LongValueKey.NumberTimesTinkered) > 0)
				{
					return ", " + wo.Values(LongValueKey.NumberTimesTinkered).ToString() + " Tinkers";
				}
				else {return String.Empty;}
			}
			private string WieldlvlString()
			{
				if (wo.Values(LongValueKey.WieldReqValue) > 0) 
				{
					return ", Wield Lvl " + wo.Values(LongValueKey.WieldReqValue).ToString();
				} else { return String.Empty;}
			}
			private string WeaponMasteryString()
			{
				if (WeaponMasteryCategory > 0) 
				{
					return " (" + MasteryIndex[WeaponMasteryCategory].name + ")";	
				}
				else {return String.Empty;}
			}
			private string WieldString()
			{
				string result = string.Empty;
				if (wo.Values(LongValueKey.WieldReqType) > 0) 
				{
					if (wo.Values(LongValueKey.WieldReqType) == 7) 
					{
						result = ", Wield Lvl " + wo.Values(LongValueKey.WieldReqValue).ToString();
					} else if (wo.Values(LongValueKey.WieldReqValue) > 0) 
					{
						result = ", " + SkillIndex[wo.Values(LongValueKey.WieldReqAttribute)].name + " " + wo.Values(LongValueKey.WieldReqValue).ToString() + " to Wield";
					}
				}
				return result;
			}
			private string SalvageString()
			{
				string result = string.Empty;
				string salvagetype = string.Empty;
				string work = string.Empty;
				string bagcount = string.Empty;
				bagcount = "(" + wo.Values(LongValueKey.UsesRemaining) + ")";
				salvagetype = MaterialIndex[wo.Values(LongValueKey.Material)].name;
				work = wo.Values(DoubleValueKey.SalvageWorkmanship).ToString("0.##");
				return salvagetype + " Salvage " + bagcount + ", Craft " + work;
			}
			private string xModString(double x, string suffix)
			{
				//missle mods to string
				if (x > 0) 
				{
					//TODO:  make this return formatted strings for doubles.
					//return Math.Round(((wo.Values(DoubleValueKey.MeleeDefenseBonus) - 1) * 100), 2);
					return ", " + x.ToString("0.") + " " + suffix;
				}
				return string.Empty;
			}
			private string ElementalDmgBonusString()
			{
				if (wo.Values(LongValueKey.ElementalDmgBonus) > 0)
				{
					return " +" + wo.Values(LongValueKey.ElementalDmgBonus).ToString();
				}
				else{return string.Empty;}
			}
			private string MinMaxDamage()
			{
				if(wo.Values(DoubleValueKey.Variance) > 0)
				{
					double WeaponVariance = wo.Values(DoubleValueKey.Variance);
					return ", " + Math.Round((WeaponMaxDamage - (WeaponMaxDamage * WeaponVariance)), 2).ToString("0.00") + "-" + WeaponMaxDamage.ToString();
				}
				else {return String.Empty;}
			}	

			public string CoordsStringLink(string inputcoords)
			{
				return " (" + "<Tell:IIDString:" + GOARROWLINK_ID + ":" + inputcoords + ">" + inputcoords + "<\\Tell>" + ")";
			}



			public string LinkString()
			{
				//builds result string with appropriate goodies to report
				string result = string.Empty;
				try {
					if (wo != null) 
					{
						switch(wo.ObjectClass)
						{
							case ObjectClass.Armor:
								result = IORString() + wo.Name + CurrentItemLevelString() + SetString() + ALString() + ImbueString() + TinkersString() + SpellDescriptions() + WieldString() + ActivateString() + 
										LoreString() + RankString() + RaceString() + CraftString() + ProtsString();
								break;
							case ObjectClass.Clothing:
								if(ArmorLevel > 0)
								{
									result = IORString() + wo.Name + CurrentItemLevelString() + SetString() + ALString() + ImbueString() + TinkersString() + SpellDescriptions() + WieldString() + ActivateString() + 
										LoreString() + RankString() + RaceString() + CraftString() + ProtsString();
								}
								else
								{
									result = IORString() + wo.Name + SetString() + ALString() + ImbueString() + TinkersString() + SpellDescriptions() + WieldString() + ActivateString() + 
										LoreString() + RankString() + RaceString() + CraftString() + SpellDescriptions();
								}
								break;
							case ObjectClass.Container:
								result = IORString() + wo.Name + CoordsStringLink(wo.Coordinates().ToString());
								break;
							case ObjectClass.Corpse:
								result = IORString() + wo.Name + CoordsStringLink(wo.Coordinates().ToString());
								break;
							case ObjectClass.Gem:
								result = IORString() + wo.Name +SetString() + WieldlvlString() + SpellDescriptions();
								break;
							case ObjectClass.Jewelry:
								result = IORString() + wo.Name + SetString() + ALString() + ImbueString() + TinkersString() + SpellDescriptions() + WieldString() + LoreString() + 
									RankString() + RaceString() + CraftString();
								break;
							case ObjectClass.Lifestone:
								result = IORString() + wo.Name + CoordsStringLink(wo.Coordinates().ToString());
								break;
							case ObjectClass.MeleeWeapon:
								result = IORString() + wo.Name +  WeaponMasteryString() + ImbueString() + SlayerString() + TinkersString() + MinMaxDamage() + xModString(WeaponAttackBonus, "a") + xModString(WeaponMeleeBonus, "md") +
								SpellDescriptions() + WieldString() + LoreString() + RankString() + RaceString() + CraftString();
								break;
							case ObjectClass.MissileWeapon:
								result = IORString() + wo.Name + WeaponMasteryString() + ImbueString() + SlayerString() +  TinkersString() + xModString(WeaponMissileModifier, string.Empty) + ElementalDmgBonusString()  +
								xModString(WeaponMeleeBonus, "md") + SpellDescriptions() + WieldString() + LoreString() + RankString() + RaceString() + CraftString();
								break;
							case ObjectClass.Monster:
								result = IORString() + wo.Name + "(" + HealthCurrent + "/" + HealthMax + ") " + wo.Name + " [L:" + Convert.ToString(wo.Values(LongValueKey.CreatureLevel)) + "]" + CoordsStringLink(wo.Coordinates().ToString());
								break;
							case ObjectClass.Npc:
								result = IORString() + wo.Name + CoordsStringLink(wo.Coordinates().ToString());
								break;
							case ObjectClass.Player:
								result = IORString() + wo.Name;
								break;
							case ObjectClass.Portal:
								result = IORString() + wo.Values(StringValueKey.PortalDestination) + CoordsStringLink(wo.Coordinates().ToString());
								break;
							case ObjectClass.Salvage:
								result = SalvageString();
								break;
							case ObjectClass.Vendor:
								result = IORString() + wo.Name + CoordsStringLink(wo.Coordinates().ToString());
								break;
							case ObjectClass.WandStaffOrb:
								result = IORString() + wo.Name + ImbueString() + SlayerString() + TinkersString() + xModString(DamageVsMonsters, "vs. Monsters") + xModString(WeaponMeleeBonus, "md") +
									xModString(WeaponManaCBonus, "mc") + SpellDescriptions() + WieldString() + LoreString() + RankString() + RaceString() + CraftString();
								break;
							case ObjectClass.Misc:
								if(EssenceLevel > 0)
								{
									result = IORString() + "L" + EssenceLevel + wo.Name;
									break;
								}
								else goto default;
							default:
								result = IORString() + wo.Name + CoordsStringLink(wo.Coordinates().ToString());
								break;
						}
						
					}
				} catch (Exception ex) {
					LogError(ex);
				}
				return result;
			}

			//Begin reproduction of Mag-Tools buffed weapon values
	


			
//			public override string ToString()
//			{
//				//builds result string with appropriate goodies to report
//				string result = string.Empty;
//				try {
//					if (wo != null) 
//					{
//						result = IORString();
//						result += wo.Name + CurrentItemLevelString();
//						if (wo.ObjectClass == ObjectClass.Armor) 
//						{
//							result += SetString() + ALString() + ImbueString() + TinkersString() + SpellDescriptions() + WieldString() + ActivateString() + 
//								LoreString() + RankString() + RaceString() + CraftString() + ProtsString();
//						}
//						else if (wo.ObjectClass == ObjectClass.Clothing) 
//						{
//							result += SetString() + ALString() + ImbueString() + TinkersString() + SpellDescriptions() + WieldString() + ActivateString() + 
//								LoreString() + RankString() + RaceString() + CraftString();
//						} 
//						else if (wo.ObjectClass == ObjectClass.Jewelry)
//						{
//							result += SetString() + ALString() + ImbueString() + TinkersString() + SpellDescriptions() + WieldString() + LoreString() + 
//								RankString() + RaceString() + CraftString();
//						}
//						else if (wo.ObjectClass == ObjectClass.MissileWeapon) 
//						{
//							result += WeaponMasteryString() + ImbueString() + SlayerString() +  TinkersString() + xModString(WeaponMissileModifier, string.Empty) + ElementalDmgBonusString()  +
//								xModString(WeaponMeleeBonus, "md") + SpellDescriptions() + WieldString() + LoreString() + RankString() + RaceString() + CraftString();
//						} 
//						else if (wo.ObjectClass == ObjectClass.MeleeWeapon) 
//						{
//							result += WeaponMasteryString() + ImbueString() + SlayerString() + TinkersString() + MinMaxDamage() + xModString(WeaponAttackBonus, "a") + xModString(WeaponMeleeBonus, "md") +
//								SpellDescriptions() + WieldString() + LoreString() + RankString() + RaceString() + CraftString();
//						} 
//						else if	(wo.ObjectClass == ObjectClass.WandStaffOrb) 
//						{
//							result += ImbueString() + SlayerString() + TinkersString() + xModString(DamageVsMonsters, "vs. Monsters") + xModString(WeaponMeleeBonus, "md") +
//								xModString(WeaponManaCBonus, "mc") + SpellDescriptions() + WieldString() + LoreString() + RankString() + RaceString() + CraftString();
//						}
//						else if (wo.ObjectClass == ObjectClass.Gem & Aetheriacheck)
//						{
//							result += SetString() + WieldlvlString() + SpellDescriptions();
//						}
//						else if (wo.HasIdData && (wo.ObjectClass == ObjectClass.Player || wo.ObjectClass == ObjectClass.Monster)) 
//						{
//							result = "(" + HealthCurrent + "/" + HealthMax + ") " + wo.Name + " [L:" + Convert.ToString(wo.Values(LongValueKey.CreatureLevel)) + "]";
//						}
//						else if (wo.ObjectClass == ObjectClass.Salvage)
//						{
//							result = SalvageString();
//						}
//						else if (wo.ObjectClass == ObjectClass.Portal)
//						{
//							result += wo.Values(StringValueKey.PortalDestination) + " (" + wo.Coordinates().ToString() + ")";                  
//						}
//					}
//				} catch (Exception ex) {
//					LogError(ex);
//				}
//				return result;
//			}
		}
	}
}


		
		
