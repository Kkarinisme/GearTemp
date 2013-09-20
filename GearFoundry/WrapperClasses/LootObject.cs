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
using System.Linq;
using System.Runtime.InteropServices;

namespace GearFoundry
{

	public partial class PluginCore
	{	
//		bool b = ((IList<int>)new int[] { 3, 7, 12, 5 }).Contains(5);
		private static int AetheriaSlots = 0x70000000;
		private static int CloakSlot = 0x8000000;
		private static int UnderwearSlots = 0x6;

		public class LootObject
		{
			
			public bool InspectList = false;
			public bool ProcessList = false;
			
			public bool Exclude = false;
			public bool Listen = false;
			public bool ActionTarget = false;
			public bool Move = false;
			public bool Process = false;
			public bool Open = false;
			
			public IAction ProcessAction = IAction.None;
			
			public DateTime LastActionTime = DateTime.MinValue;
			
			public List<pale> Palettes = new List<pale>();
			
			public class pale
			{
				int entry = 0;
				int color = 0;
			}
					
			private WorldObject wo;
		
			public LootObject(WorldObject obj)
			{
				wo = obj;
			}
		
			public LootObject()
			{
			}
		
			//wrappers: the overloads of function Values in wrappers.worldobject makes me type too much -Losado
			//wrappers below allow direct access to all world object properties not expressly
			//included in the IdentifiedObject (IO) data class.  Many properties are expressly defined and loaded.  -Irquk
			
			public int LValue(Decal.Adapter.Wrappers.LongValueKey eval) {
				return wo.Values(eval); 
			}
			public double DValue(Decal.Adapter.Wrappers.DoubleValueKey eval) {
				return wo.Values(eval); 
			}
			public string SValue(Decal.Adapter.Wrappers.StringValueKey eval) {
				return wo.Values(eval);
			}
			public bool BValue(Decal.Adapter.Wrappers.BoolValueKey eval) {
				return wo.Values(eval); 
			}
			
			internal IOResult IOR = IOResult.unknown;
			public bool addtoloot;
			public bool notify;
			public string rulename;
			public double DistanceAway;
			
//			public string model
//			{
//				get
//				{
//					return wo.Values(LongValueKey.Model);
//				}
//			}
			
			public int GearScore
			{	
				get
				{
					double fudgefactor = 0;
					double gearscorereturn = 0;
					switch(wo.ObjectClass)
					{
						case ObjectClass.Gem:
							if((wo.Values(LongValueKey.EquipableSlots) & AetheriaSlots) == wo.Values(LongValueKey.EquipableSlots)) 
							{
								gearscorereturn += (double)wo.Values((LongValueKey)NewLongKeys.MaxItemLevel);
								break;
							}
							break;
							
						case ObjectClass.Clothing:
							if((wo.Values(LongValueKey.EquipableSlots) & CloakSlot) == wo.Values(LongValueKey.EquipableSlots)) 
							{
								gearscorereturn += (double)wo.Values((LongValueKey)NewLongKeys.MaxItemLevel);
								break;
							}
							else if((wo.Values(LongValueKey.EquipableSlots) & UnderwearSlots) == wo.Values(LongValueKey.EquipableSlots))
							{
								gearscorereturn += 0;
								break;
							}
							else if(wo.Values(LongValueKey.ArmorLevel) > 0) 
							{
								gearscorereturn += ArmorScore;
								break;
							}
							break;
		
						case ObjectClass.Armor:
							gearscorereturn += ArmorScore;
							break;
		
						case ObjectClass.MeleeWeapon:
							gearscorereturn += OffenseScore + SkillScore;
							break;
		
						case ObjectClass.MissileWeapon:
							//Best XBow (375):  +165% and + 18 Elemental
							//Best Trown (375):  +160% and + 18 Elemental
							//Best Bow (375):  +140% and + 18 Elemental
							//Target Weapon (375) = +165 and + 18 elemental
							//I am not certian this should be handled with a flat increase, it might be more appropriate to do a multipler.
							//Additional research is necessary
							if(WeaponMasteryCategory == (int)WeaponMastery.Bow) {fudgefactor = 6;}
							if(WeaponMasteryCategory == (int)WeaponMastery.Thrown) {fudgefactor = 1;}
							gearscorereturn += OffenseScore + SkillScore + fudgefactor;
							break;
		
						case ObjectClass.WandStaffOrb:
							gearscorereturn += OffenseScore + SkillScore;
							break;
						
						default:
							break;
		
					}
					if(wo.LongKeys.Contains((int)LongValueKey.Imbued)) {gearscorereturn++;}
					gearscorereturn += RatingScore;
					return Convert.ToInt32(gearscorereturn);
				}
			}
			
			public string SpellScore
			{
				get
				{
					int[] CantripArray = new int[4];
					if(wo.SpellCount == 0) { return String.Empty;}
					else
					{
						for(int i = 0; i <= wo.SpellCount -1; i++)
						{				
							if(SpellIndex[wo.Spell(i)].spelllevel == 11) {CantripArray[0]++;}
							if(SpellIndex[wo.Spell(i)].spelllevel == 13) {CantripArray[1]++;}
							if(SpellIndex[wo.Spell(i)].spelllevel == 14) {CantripArray[2]++;}
							if(SpellIndex[wo.Spell(i)].spelllevel == 15) {CantripArray[3]++;}		
						}
					}
					if(CantripArray.Sum() == 0) {return String.Empty;}
					string ReportString = String.Empty;
					if(CantripArray[3] > 0) {ReportString += CantripArray[3].ToString() + "L";}
					if(CantripArray[2] > 0) {ReportString += CantripArray[2].ToString() + "E";}
					if(CantripArray[1] > 0) {ReportString += CantripArray[1].ToString() + "J";}
					if(CantripArray[0] > 0) {ReportString += CantripArray[0].ToString() + "N";}
					return ReportString;
				}
			}
			
			public int ArmorScore
			{
				get
				{
					//Enchantable Armors
					if((wo.Values(LongValueKey.EquipableSlots) & UnderwearSlots) == wo.Values(LongValueKey.EquipableSlots) ||
					   (wo.Values(LongValueKey.EquipableSlots) & CloakSlot) == wo.Values(LongValueKey.EquipableSlots))
					{
						return 0;
					}
					
					if(wo.Values(LongValueKey.Unenchantable) == 0)
					{
						double observedarmortinks = 0;
						if(wo.LongKeys.Contains((int)LongValueKey.ArmorLevel)) {observedarmortinks = wo.Values(LongValueKey.ArmorLevel) / 20;}
						double availabletinks = 0;
						if(wo.LongKeys.Contains((int)DoubleValueKey.SalvageWorkmanship)) {availabletinks = 10 - wo.Values(LongValueKey.NumberTimesTinkered);}
						double basearmortinks = 0;
						double cantrippenality = 0;
						double cantripsteelbonus = 0;
						double enchantmentpenalty = 0;
						double impen7or8 = 12;
						
						//Determine the base, unenchanted steel tinks in the item.
						if(wo.Values(LongValueKey.ActiveSpellCount) == 0) {basearmortinks = observedarmortinks;}
						else
						{
							for(int i = 0; i < wo.ActiveSpellCount; i++)
							{
								//Determine highest level Impen Cantrip bonus
								if(wo.ActiveSpell(i) == 6095 && cantrippenality < 4){cantrippenality = 4;}
								else if(wo.ActiveSpell(i) == 4667 && cantrippenality < 3){cantrippenality = 3;}
								else if(wo.ActiveSpell(i) == 2592 && cantrippenality < 2){cantrippenality = 2;}
								else if(wo.ActiveSpell(i) == 2604 && cantrippenality < 1){cantrippenality = 1;}
								
								//Determine highest level Impen Bonus
								if(wo.ActiveSpell(i) == 4407 && enchantmentpenalty < 12){enchantmentpenalty = 12;}
								else if(wo.ActiveSpell(i) == 3908 && enchantmentpenalty < 12){enchantmentpenalty = 12;}
								else if(wo.ActiveSpell(i) == 2108 && enchantmentpenalty < 11){enchantmentpenalty = 11;}
								else if(wo.ActiveSpell(i) == 1486 && enchantmentpenalty < 10){enchantmentpenalty = 10;}
								else if(wo.ActiveSpell(i) == 1485 && enchantmentpenalty < 7.5){enchantmentpenalty = 7.5;}
								else if(wo.ActiveSpell(i) == 1484 && enchantmentpenalty < 5){enchantmentpenalty = 5;}
								else if(wo.ActiveSpell(i) == 1483 && enchantmentpenalty < 3.75){enchantmentpenalty = 3.75;}
								else if(wo.ActiveSpell(i) == 1482 && enchantmentpenalty < 2.5){enchantmentpenalty = 2.5;}
								else if(wo.ActiveSpell(i) == 51 && enchantmentpenalty < 1){enchantmentpenalty = 1;}	
							}

							basearmortinks = observedarmortinks - cantrippenality - enchantmentpenalty;


						}						
						
						if(wo.SpellCount > 0)
						{
							for(int i = 0; i < wo.SpellCount; i++)
							{				
								if(wo.Spell(i) == 6095 && cantripsteelbonus < 4){cantripsteelbonus = 4;}
								else if(wo.Spell(i) == 4667 && cantripsteelbonus < 3){cantripsteelbonus = 3;}
								else if(wo.Spell(i) == 2592 && cantripsteelbonus < 2){cantripsteelbonus = 2;}
								else if(wo.Spell(i) == 2604 && cantripsteelbonus < 1){cantripsteelbonus = 1;}
							}
						}


						return Convert.ToInt32(basearmortinks + cantripsteelbonus + availabletinks + impen7or8);
						
					}
					//Calculation for unenchantable armor.  
					else 
					{
						//No enchants firing			
						if(wo.Values(LongValueKey.ActiveSpellCount) == 0)
						{
							double steelvalue = wo.Values(LongValueKey.ArmorLevel) / 20;
							double cantripsteelbonus = 0;
							double enchantmentsteelbonus = 0;
							double protectionpenatly = 0;
							double tinkersavailable = 10 - wo.Values(LongValueKey.NumberTimesTinkered);
								
							double slashbase = wo.Values(DoubleValueKey.SlashProt);
							double piercebase = wo.Values(DoubleValueKey.PierceProt);
							double bludgebase = wo.Values(DoubleValueKey.BludgeonProt);
							double acidbase = wo.Values(DoubleValueKey.AcidProt);
							double coldbase = wo.Values(DoubleValueKey.ColdProt);
							double firebase = wo.Values(DoubleValueKey.FireProt);
							double lightbase = wo.Values(DoubleValueKey.LightningProt);
							
							double slashtinksbonus = 0;
							double piercetinksbonus = 0;
							double bludgtinksbonus = 0;
							double acidtinksbonus = 0;
							double coldtinksbonus = 0;
							double firetinksbonus = 0;
							double lighttinksbonus = 0;
							
							double scbonus = 0;
							double sebonus = 0;
							double pcbonus = 0;
							double pebonus = 0;
							double bcbonus = 0;						
							double bebonus = 0;
							double acbonus = 0;
							double aebonus = 0;
							double ccbonus = 0;
							double cebonus = 0;
							double fcbonus = 0;
							double febonus = 0;
							double lcbonus = 0;
							double lebonus = 0;
							
							if(wo.SpellCount > 0)
							{
								for(int i = 0; i <= wo.SpellCount -1; i++)
								{				
									//Armor Level Modifiers
									if(wo.Spell(i) == 6095 && cantripsteelbonus < 4){cantripsteelbonus = 4;}
									else if(wo.Spell(i) == 4667 && cantripsteelbonus < 3){cantripsteelbonus = 3;}
									else if(wo.Spell(i) == 2592 && cantripsteelbonus < 2){cantripsteelbonus = 2;}
									else if(wo.Spell(i) == 2604 && cantripsteelbonus < 1){cantripsteelbonus = 1;}								
									if(wo.Spell(i) == 4407 && enchantmentsteelbonus < 12){enchantmentsteelbonus = 12;}
									else if(wo.Spell(i) == 3908 && enchantmentsteelbonus < 12){enchantmentsteelbonus = 12;}
									else if(wo.Spell(i) == 2108 && enchantmentsteelbonus < 11){enchantmentsteelbonus = 11;}
									else if(wo.Spell(i) == 1486 && enchantmentsteelbonus < 10){enchantmentsteelbonus = 10;}
									else if(wo.Spell(i) == 1485 && enchantmentsteelbonus < 7.5){enchantmentsteelbonus = 7.5;}
									else if(wo.Spell(i) == 1484 && enchantmentsteelbonus < 5){enchantmentsteelbonus = 5;}
									else if(wo.Spell(i) == 1483 && enchantmentsteelbonus < 3.75){enchantmentsteelbonus = 3.75;}
									else if(wo.Spell(i) == 1482 && enchantmentsteelbonus < 2.5){enchantmentsteelbonus = 2.5;}
									else if(wo.Spell(i) == 51 && enchantmentsteelbonus < 1){enchantmentsteelbonus = 1;}
									
									
									//Slash modifiers
									if(wo.Spell(i) == 4293 && sebonus < 2){sebonus = 2;}
									else if(wo.Spell(i) == 2094 && sebonus < 1.70){sebonus = 1.7;}
									else if(wo.Spell(i) == 1562 && sebonus < 1.50){sebonus = 1.5;}
									else if(wo.Spell(i) == 1561 && sebonus < 1.0){sebonus = 1.0;}
									else if(wo.Spell(i) == 1560 && sebonus < 0.75){sebonus = 0.75;}
									else if(wo.Spell(i) == 1559 && sebonus < 0.5){sebonus = 0.5;}
									else if(wo.Spell(i) == 1568 && sebonus < 0.25){sebonus = 0.25;}
									else if(wo.Spell(i) == 37 && sebonus < 0.1){sebonus = 0.1;}
									if(wo.Spell(i) == 6097 && scbonus < 0.25){scbonus = 0.25;}
									else if(wo.Spell(i) == 4669 && scbonus < 0.2){scbonus = 0.2;}
									else if(wo.Spell(i) == 2594 && scbonus < 0.15){scbonus = 0.15;}
									else if(wo.Spell(i) == 2606 && scbonus < 0.1){scbonus = 0.1;}	
									
									//Pierce Modifiers
									if(wo.Spell(i) == 4212 && pebonus < 2){pebonus = 2;}
									else if(wo.Spell(i) == 2113 && pebonus < 1.70){pebonus = 1.7;}
									else if(wo.Spell(i) == 1574 && pebonus < 1.50){pebonus = 1.5;}
									else if(wo.Spell(i) == 1573 && pebonus < 1.0){pebonus = 1.0;}
									else if(wo.Spell(i) == 1572 && pebonus < 0.75){pebonus = 0.75;}
									else if(wo.Spell(i) == 1571 && pebonus < 0.5){pebonus = 0.5;}
									else if(wo.Spell(i) == 1570 && pebonus < 0.25){pebonus = 0.25;}
									else if(wo.Spell(i) == 1569 && pebonus < 0.1){pebonus = 0.1;}	
									if(wo.Spell(i) == 6096 && pcbonus < 0.25){pcbonus = 0.25;}
									else if(wo.Spell(i) == 4668 && pcbonus < 0.2){pcbonus = 0.2;}
									else if(wo.Spell(i) == 2593 && pcbonus < 0.15){pcbonus = 0.15;}
									else if(wo.Spell(i) == 2605 && pcbonus < 0.1){pcbonus = 0.1;}								
									
									//Bludgeon  Modifiers
									if(wo.Spell(i) == 4397 && bebonus < 2){bebonus = 2;}
									else if(wo.Spell(i) == 2098 && bebonus < 1.70){bebonus = 1.7;}
									else if(wo.Spell(i) == 1516 && bebonus < 1.50){bebonus = 1.5;}
									else if(wo.Spell(i) == 1515 && bebonus < 1.0){bebonus = 1.0;}
									else if(wo.Spell(i) == 1514 && bebonus < 0.75){bebonus = 0.75;}
									else if(wo.Spell(i) == 1513 && bebonus < 0.5){bebonus = 0.5;}
									else if(wo.Spell(i) == 1512 && bebonus < 0.25){bebonus = 0.25;}
									else if(wo.Spell(i) == 1511 && bebonus < 0.1){bebonus = 0.1;}	
									if(wo.Spell(i) == 6090 && bcbonus < 0.25){bcbonus = 0.25;}
									else if(wo.Spell(i) == 4662 && bcbonus < 0.2){bcbonus = 0.2;}
									else if(wo.Spell(i) == 2587 && bcbonus < 0.15){bcbonus = 0.15;}
									else if(wo.Spell(i) == 2599 && bcbonus < 0.1){bcbonus = 0.1;}
									
									//Acid Modifiers
									if(wo.Spell(i) == 4391 && aebonus < 2){aebonus = 2;}
									else if(wo.Spell(i) == 2092 && aebonus < 1.70){aebonus = 1.7;}
									else if(wo.Spell(i) == 1498 && aebonus < 1.50){aebonus = 1.5;}
									else if(wo.Spell(i) == 1497 && aebonus < 1.0){aebonus = 1.0;}
									else if(wo.Spell(i) == 1496 && aebonus < 0.75){aebonus = 0.75;}
									else if(wo.Spell(i) == 1495 && aebonus < 0.5){aebonus = 0.5;}
									else if(wo.Spell(i) == 1494 && aebonus < 0.25){aebonus = 0.25;}
									else if(wo.Spell(i) == 1493 && aebonus < 0.1){aebonus = 0.1;}	
									if(wo.Spell(i) == 6088 && acbonus < 0.25){acbonus = 0.25;}
									else if(wo.Spell(i) == 4660 && acbonus < 0.2){acbonus = 0.2;}
									else if(wo.Spell(i) == 2585 && acbonus < 0.15){acbonus = 0.15;}
									else if(wo.Spell(i) == 2597 && acbonus < 0.1){acbonus = 0.1;}
									
									//Fire Modifiers
									if(wo.Spell(i) == 4401 && febonus < 2){febonus = 2;}
									else if(wo.Spell(i) == 2102 && febonus < 1.70){febonus = 1.7;}
									else if(wo.Spell(i) == 1552 && febonus < 1.50){febonus = 1.5;}
									else if(wo.Spell(i) == 1551 && febonus < 1.0){febonus = 1.0;}
									else if(wo.Spell(i) == 1550 && febonus < 0.75){febonus = 0.75;}
									else if(wo.Spell(i) == 1549 && febonus < 0.5){febonus = 0.5;}
									else if(wo.Spell(i) == 1548 && febonus < 0.25){febonus = 0.25;}
									else if(wo.Spell(i) == 1547 && febonus < 0.1){febonus = 0.1;}	
									if(wo.Spell(i) == 6092 && fcbonus < 0.25){fcbonus = 0.25;}
									else if(wo.Spell(i) == 4664 && fcbonus < 0.2){fcbonus = 0.2;}
									else if(wo.Spell(i) == 2589 && fcbonus < 0.15){fcbonus = 0.15;}
									else if(wo.Spell(i) == 2601 && fcbonus < 0.1){fcbonus = 0.1;}
									
									//Cold Modifiers
									if(wo.Spell(i) == 4403 && cebonus < 2){cebonus = 2;}
									else if(wo.Spell(i) == 2104 && cebonus < 1.70){cebonus = 1.7;}
									else if(wo.Spell(i) == 1528 && cebonus < 1.50){cebonus = 1.5;}
									else if(wo.Spell(i) == 1527 && cebonus < 1.0){cebonus = 1.0;}
									else if(wo.Spell(i) == 1526 && cebonus < 0.75){cebonus = 0.75;}
									else if(wo.Spell(i) == 1525 && cebonus < 0.5){cebonus = 0.5;}
									else if(wo.Spell(i) == 1524 && cebonus < 0.25){cebonus = 0.25;}
									else if(wo.Spell(i) == 1523 && cebonus < 0.1){cebonus = 0.1;}	
									if(wo.Spell(i) == 6093 && ccbonus < 0.25){ccbonus = 0.25;}
									else if(wo.Spell(i) == 4665 && ccbonus < 0.2){ccbonus = 0.2;}
									else if(wo.Spell(i) == 2590 && ccbonus < 0.15){ccbonus = 0.15;}
									else if(wo.Spell(i) == 2602 && ccbonus < 0.1){ccbonus = 0.1;}
									
									//Lightning Modifiers
									if(wo.Spell(i) == 4409 && lebonus < 2){lebonus = 2;}
									else if(wo.Spell(i) == 2110 && lebonus < 1.70){lebonus = 1.7;}
									else if(wo.Spell(i) == 1540 && lebonus < 1.50){lebonus = 1.5;}
									else if(wo.Spell(i) == 1539 && lebonus < 1.0){lebonus = 1.0;}
									else if(wo.Spell(i) == 1538 && lebonus < 0.75){lebonus = 0.75;}
									else if(wo.Spell(i) == 1537 && lebonus < 0.5){lebonus = 0.5;}
									else if(wo.Spell(i) == 1536 && lebonus < 0.25){lebonus = 0.25;}
									else if(wo.Spell(i) == 1535 && lebonus < 0.1){lebonus = 0.1;}	
									if(wo.Spell(i) == 6099 && lcbonus < 0.25){lcbonus = 0.25;}
									else if(wo.Spell(i) == 4671 && lcbonus < 0.2){lcbonus = 0.2;}
									else if(wo.Spell(i) == 2595 && lcbonus < 0.15){lcbonus = 0.15;}
									else if(wo.Spell(i) == 2607 && lcbonus < 0.1){lcbonus = 0.1;}	
								}
							}
							
							slashtinksbonus = (slashbase + slashbase * sebonus + slashbase * scbonus) / 0.2;
							piercetinksbonus = (piercebase + piercebase * pebonus + piercebase * pcbonus) / 0.2; 
							bludgtinksbonus = (bludgebase + bludgebase * bebonus + bludgebase * bcbonus) / 0.2;
							acidtinksbonus = (acidbase + acidbase * aebonus + acidbase * acbonus) / 0.4;
							coldtinksbonus = (coldbase + coldbase * cebonus + coldbase * ccbonus) / 0.4;
							firetinksbonus = (firebase + firebase * febonus + firebase * fcbonus) / 0.4;
							lighttinksbonus = (lightbase + lightbase * lebonus + lightbase * lcbonus) / 0.4;
							
							if(slashtinksbonus < 10) {protectionpenatly += 10 - slashtinksbonus;}
							if(piercetinksbonus < 10) {protectionpenatly += 10 - piercetinksbonus;}
							if(bludgtinksbonus < 10) {protectionpenatly += 10 - bludgtinksbonus;}
							if(acidtinksbonus < 5) {protectionpenatly += 5 - acidtinksbonus;}
							if(firetinksbonus < 5) {protectionpenatly += 5 - firetinksbonus;}
							if(lighttinksbonus < 5) {protectionpenatly += 5 - lighttinksbonus;}
							if(coldtinksbonus < 5) {protectionpenatly += 5 - coldtinksbonus;}
							
							return Convert.ToInt32(steelvalue + cantripsteelbonus + enchantmentsteelbonus + tinkersavailable - protectionpenatly);
							
						}
						//Calculation for unenchantable armor with enchants firing
						else 
						{
							double steelvalue = wo.Values(LongValueKey.ArmorLevel) / 20;
							double protectionspenalty = 0;
							double tinkersavailable = 10 - wo.Values(LongValueKey.NumberTimesTinkered);
							
							double slashtinkspenalty = 10 - (wo.Values(DoubleValueKey.SlashProt) / 0.2);
							double piercetinkspenalty = 10 - (wo.Values(DoubleValueKey.PierceProt) / 0.2);
							double bludgetinkspenalty = 10 - (wo.Values(DoubleValueKey.BludgeonProt) / 0.2);
							double acidtinkspenalty = 5 - (wo.Values(DoubleValueKey.AcidProt)/ 0.4);
							double coldtinkspenalty = 5 - (wo.Values(DoubleValueKey.ColdProt)/ 0.4);
							double firetinkspenalty = 5 - (wo.Values(DoubleValueKey.FireProt)/ 0.4);
							double lighttinkspentalty = 5 - (wo.Values(DoubleValueKey.LightningProt)/ 0.4);
							
							if(slashtinkspenalty > 0) {protectionspenalty += slashtinkspenalty;}
							if(piercetinkspenalty > 0) {protectionspenalty += piercetinkspenalty;}
							if(bludgetinkspenalty > 0) {protectionspenalty += bludgetinkspenalty;}
							if(acidtinkspenalty > 0) {protectionspenalty += acidtinkspenalty;}
							if(coldtinkspenalty > 0) {protectionspenalty += coldtinkspenalty;}
							if(firetinkspenalty > 0) {protectionspenalty += firetinkspenalty;}
							if(lighttinkspentalty > 0) {protectionspenalty += lighttinkspentalty;}
							
							return Convert.ToInt32(steelvalue - protectionspenalty + tinkersavailable);
						}	
					}
				}
			}
			
			public int RatingScore
			{
				get
				{
					return wo.Values((LongValueKey)NewLongKeys.Crit) + wo.Values((LongValueKey)NewLongKeys.CritResist) + 
						wo.Values((LongValueKey)NewLongKeys.CritDam) + wo.Values((LongValueKey)NewLongKeys.CritDamResist) + 
						wo.Values((LongValueKey)NewLongKeys.Dam) + wo.Values((LongValueKey)NewLongKeys.DamResist);
				}
			}
				
			//Modified Looting Properties (calculated)
			public int SkillScore
			{
				get
				{	
					
					double meleedobs = 0;
					double missiledobs = 0;
					double magicdobs = 0;
					double manacobs = 0;
					double attackobs = 0;
					
					if(wo.DoubleKeys.Contains((int)DoubleValueKey.MeleeDefenseBonus)) {meleedobs = (wo.Values(DoubleValueKey.MeleeDefenseBonus) - 1) * 100;}
					if(wo.DoubleKeys.Contains((int)DoubleValueKey.MissileDBonus)) {missiledobs = (wo.Values(DoubleValueKey.MissileDBonus) -1 ) * 100;}
					if(wo.DoubleKeys.Contains((int)DoubleValueKey.MagicDBonus)) {magicdobs = (wo.Values(DoubleValueKey.MagicDBonus) - 1) * 100;}
					if(wo.DoubleKeys.Contains((int)DoubleValueKey.ManaCBonus)) {manacobs = wo.Values(DoubleValueKey.ManaCBonus) * 100;}
					if(wo.DoubleKeys.Contains((int)DoubleValueKey.AttackBonus)) {attackobs = (wo.Values(DoubleValueKey.AttackBonus) - 1) * 100;}
										
					double meleedbase = 0;
					double missiledbase = 0;
					double magicdbase = 0;
					double manacbase = 0;
					double attackbase = 0;
					
					double cantripdefenseboosters = 0;
					double cantripattackboosters = 0;
					double cantripmanaconversionboosters = 0;
					
					double basesum = 0;
					
					if(wo.Values(LongValueKey.EquippedSlots) == 0)
					{
						meleedbase = meleedobs;
						missiledbase = missiledobs;
						magicdbase = magicdobs;
						manacbase = manacobs;
						attackbase = attackobs;
						basesum = meleedbase + missiledbase + magicdbase + manacbase + attackbase;
						if(basesum == 0) {return 0;}
					} //Calculate the base values if not enchanted.
					else
					{
						double cantripattackpenalty = 0;
						double cantripdefensepenalty = 0;
						double cantripmanacpenalty = 0;
						double enchantattackpenalty = 0;
						double enchantdefensepenalty = 0;
						double enchantmanacpenalty = 0;
						
						var ToonEnchants = CoreManager.Current.CharacterFilter.Enchantments;
						
						for(int i = 0; i < wo.ActiveSpellCount; i++)
						{
							if(wo.ActiveSpell(i) == 6091 && cantripdefensepenalty < 9){cantripdefensepenalty = 9;}
							else if(wo.ActiveSpell(i) == 4663 && cantripdefensepenalty < 7){cantripdefensepenalty= 7;}
							else if(wo.ActiveSpell(i) == 2588 && cantripdefensepenalty < 5) {cantripdefensepenalty = 5;}
							else if(wo.ActiveSpell(i) == 2600 && cantripdefensepenalty < 3) {cantripdefensepenalty = 3;}	
										
							if(wo.ObjectClass == ObjectClass.MeleeWeapon)
							{
								if(wo.ActiveSpell(i) == 6094 && cantripattackpenalty < 9){cantripattackpenalty = 9;}
								else if(wo.ActiveSpell(i) == 4666 && cantripattackpenalty < 7){cantripattackpenalty = 7;}
								else if(wo.ActiveSpell(i) == 2591 && cantripattackpenalty < 5) {cantripattackpenalty = 5;}
								else if(wo.ActiveSpell(i) == 2603 && cantripattackpenalty < 3) {cantripattackpenalty = 3;}
							}

							if(wo.ObjectClass == ObjectClass.WandStaffOrb)
							{
								if(wo.ActiveSpell(i) == 6087 && cantripmanacpenalty < 0.30){cantripmanacpenalty = 0.30;}
								else if(wo.ActiveSpell(i) == 6086 && cantripmanacpenalty < 0.25){cantripmanacpenalty = 0.25;}
								else if(wo.ActiveSpell(i) == 3200 && cantripmanacpenalty < 0.20){cantripmanacpenalty = 0.20;}
								else if(wo.ActiveSpell(i) == 3202 && cantripmanacpenalty < 0.20){cantripmanacpenalty = 0.15;}
								else if(wo.ActiveSpell(i) == 3199 && cantripmanacpenalty < 0.10){cantripmanacpenalty = 0.10;}
								else if(wo.ActiveSpell(i) == 3201 && cantripmanacpenalty < 0.05){cantripmanacpenalty = 0.05;}				
							}		
						}
						
						for(int i = 0; i < ToonEnchants.Count; i++)
						{
							if((ToonEnchants[i].SpellId == 4400 || ToonEnchants[i].SpellId == 6006) && enchantdefensepenalty < 20) {enchantdefensepenalty = 20;}
							else if((ToonEnchants[i].SpellId == 6005 || ToonEnchants[i].SpellId == 2101) && enchantdefensepenalty < 17) {enchantdefensepenalty = 17;}
							else if((ToonEnchants[i].SpellId == 1605 || ToonEnchants[i].SpellId == 6004) && enchantdefensepenalty < 15) {enchantdefensepenalty = 15;}
							else if((ToonEnchants[i].SpellId == 1604 || ToonEnchants[i].SpellId == 6003) && enchantdefensepenalty < 13) {enchantdefensepenalty = 13;}
							else if((ToonEnchants[i].SpellId == 1603 || ToonEnchants[i].SpellId == 6002) && enchantdefensepenalty < 10) {enchantdefensepenalty = 10;}
							else if((ToonEnchants[i].SpellId == 1602 || ToonEnchants[i].SpellId == 6001) && enchantdefensepenalty < 7.5) {enchantdefensepenalty = 7.5;}
							else if((ToonEnchants[i].SpellId == 1601 || ToonEnchants[i].SpellId == 6000) && enchantdefensepenalty < 5) {enchantdefensepenalty = 5;}
							else if((ToonEnchants[i].SpellId == 1599 || ToonEnchants[i].SpellId == 5999) && enchantdefensepenalty < 3) {enchantdefensepenalty = 3;}
							
							if(wo.ObjectClass == ObjectClass.MeleeWeapon)
							{								
								if((ToonEnchants[i].SpellId == 4405 || ToonEnchants[i].SpellId == 6014) && enchantattackpenalty < 20){enchantattackpenalty = 20;}
								else if((ToonEnchants[i].SpellId == 2106 || ToonEnchants[i].SpellId == 6013) && enchantattackpenalty < 17){enchantattackpenalty = 17;}
								else if((ToonEnchants[i].SpellId == 1592 || ToonEnchants[i].SpellId == 6012) && enchantattackpenalty < 15){enchantattackpenalty = 15;}
								else if((ToonEnchants[i].SpellId == 1591 || ToonEnchants[i].SpellId == 6011) && enchantattackpenalty < 12.5){enchantattackpenalty = 12.5;}
								else if((ToonEnchants[i].SpellId == 1590 || ToonEnchants[i].SpellId == 6010) && enchantattackpenalty < 10){enchantattackpenalty = 10;}
								else if((ToonEnchants[i].SpellId == 1589 || ToonEnchants[i].SpellId == 6009) && enchantattackpenalty < 7.5){enchantattackpenalty = 7.5;}
								else if((ToonEnchants[i].SpellId == 1588 || ToonEnchants[i].SpellId == 6008) && enchantattackpenalty < 5){enchantattackpenalty = 5;}
								else if((ToonEnchants[i].SpellId == 1587 || ToonEnchants[i].SpellId == 6007) && enchantattackpenalty < 2.5){enchantattackpenalty = 2.5;}
							}	
							if(wo.ObjectClass == ObjectClass.WandStaffOrb)
							{								
								if((ToonEnchants[i].SpellId == 4418 || ToonEnchants[i].SpellId == 5989) && enchantmanacpenalty < 0.80) {enchantmanacpenalty = 0.80;}
								else if((ToonEnchants[i].SpellId == 2117 || ToonEnchants[i].SpellId == 5988) && enchantmanacpenalty < 0.70) {enchantmanacpenalty = 0.70;}
								else if((ToonEnchants[i].SpellId == 1480 || ToonEnchants[i].SpellId == 5987) && enchantmanacpenalty < 0.60) {enchantmanacpenalty = 0.60;}
								else if((ToonEnchants[i].SpellId == 1479 || ToonEnchants[i].SpellId == 5986) && enchantmanacpenalty < 0.50) {enchantmanacpenalty = 0.50;}
								else if((ToonEnchants[i].SpellId == 1478 || ToonEnchants[i].SpellId == 5985) && enchantmanacpenalty < 0.40) {enchantmanacpenalty = 0.40;}
								else if((ToonEnchants[i].SpellId == 1477 || ToonEnchants[i].SpellId == 5984) && enchantmanacpenalty < 0.30) {enchantmanacpenalty = 0.30;}
								else if((ToonEnchants[i].SpellId == 1476 || ToonEnchants[i].SpellId == 5983) && enchantmanacpenalty < 0.20) {enchantmanacpenalty = 0.20;}
								else if((ToonEnchants[i].SpellId == 1474 || ToonEnchants[i].SpellId == 5982) && enchantmanacpenalty < 0.10) {enchantmanacpenalty = 0.10;}				
							}
						}
						meleedbase = meleedobs - enchantdefensepenalty - cantripdefensepenalty;
						attackbase = attackobs - enchantattackpenalty - cantripattackpenalty;
						manacbase = manacobs / (1 + enchantmanacpenalty + cantripmanacpenalty);
						missiledbase = missiledobs;
						magicdbase = magicdobs;	
						basesum = meleedbase + missiledbase + magicdbase + manacbase + attackbase;
					}	
					
					if(wo.SpellCount > 0)
					{
						for(int i = 0; i < wo.SpellCount; i++)
						{	
							
							if(wo.Spell(i) == 6091 && cantripdefenseboosters < 9){cantripdefenseboosters = 9;}
							else if(wo.Spell(i) == 4663 && cantripdefenseboosters < 7){cantripdefenseboosters = 7;}
							else if(wo.Spell(i) == 2588 && cantripdefenseboosters < 5) {cantripdefenseboosters = 5;}
							else if(wo.Spell(i) == 2600 && cantripdefenseboosters < 3) {cantripdefenseboosters = 3;}	
							
							if(wo.ObjectClass == ObjectClass.MeleeWeapon)
							{
								if(wo.Spell(i) == 6094 && cantripattackboosters < 9){cantripattackboosters = 9;}
								else if(wo.Spell(i) == 4666 && cantripattackboosters < 7){cantripattackboosters = 7;}
								else if(wo.Spell(i) == 2591 && cantripattackboosters < 5) {cantripattackboosters = 5;}
								else if(wo.Spell(i) == 2603 && cantripattackboosters < 3) {cantripattackboosters = 3;}
							}
							if(wo.ObjectClass == ObjectClass.WandStaffOrb)
							{
								if(wo.Spell(i) == 6087 && cantripmanaconversionboosters < 0.30){cantripmanaconversionboosters = 0.30;}
								else if(wo.Spell(i) == 6086 && cantripmanaconversionboosters < 0.25){cantripmanaconversionboosters = 0.25;}
								else if(wo.Spell(i) == 3200 && cantripmanaconversionboosters < 0.20){cantripmanaconversionboosters = 0.20;}
								else if(wo.Spell(i) == 3202 && cantripmanaconversionboosters < 0.20){cantripmanaconversionboosters = 0.15;}
								else if(wo.Spell(i) == 3199 && cantripmanaconversionboosters < 0.10){cantripmanaconversionboosters = 0.10;}
								else if(wo.Spell(i) == 3201 && cantripmanaconversionboosters < 0.05){cantripmanaconversionboosters = 0.05;}
							}
						}
					}
					if(wo.ObjectClass == ObjectClass.WandStaffOrb && !wo.DoubleKeys.Contains((int)DoubleValueKey.ElementalDamageVersusMonsters))
					{
						return Convert.ToInt32(basesum + cantripattackboosters + cantripdefenseboosters + manacbase * cantripmanaconversionboosters + 10 - wo.Values(LongValueKey.NumberTimesTinkered));
					}
					else
					{
						return Convert.ToInt32(basesum + cantripattackboosters + cantripdefenseboosters + manacbase * cantripmanaconversionboosters);
					}
				}
					
			}
			
			public int OffenseScore
			{
				get
				{
					if(wo.ObjectClass == ObjectClass.MeleeWeapon)
					{
						
						double availabletinks = 0;
						if(wo.DoubleKeys.Contains((int)DoubleValueKey.SalvageWorkmanship)) {availabletinks = 10 - wo.Values(LongValueKey.NumberTimesTinkered);}
						double granitetinks = 0;
						double fudgefactor = 1;
						double mscleaveadjust = 1;
						double cantripdamageboosters = 0;
						
						double damageobs = 0;
						if(wo.LongKeys.Contains((int)LongValueKey.MaxDamage)){damageobs = wo.Values(LongValueKey.MaxDamage);}
						double damagebase = 0;
						
						if(wo.Values(LongValueKey.EquippedSlots) == 0)
						{
							damagebase = damageobs;
						}
						else
						{	
							double enchantmentdamagepenalty = 0;
							double cantripdamagepenalty = 0;
							
							//Spells from the weapon
							for(int i = 0; i < wo.ActiveSpellCount; i++)
							{
								if(wo.ActiveSpell(i) == 6089 && cantripdamagepenalty < 10){cantripdamagepenalty = 10;}
								else if(wo.ActiveSpell(i) == 4661 && cantripdamagepenalty < 7){cantripdamagepenalty = 7;}
								else if(wo.ActiveSpell(i) == 2586 && cantripdamagepenalty < 4){cantripdamagepenalty = 4;}
								else if(wo.ActiveSpell(i) == 2598 && cantripdamagepenalty < 2) {cantripdamagepenalty = 2;}
								else if(wo.ActiveSpell(i) == 2486 && cantripdamagepenalty < 2) {cantripdamagepenalty = 2;}	   
							}
							
							var ToonEnchants = CoreManager.Current.CharacterFilter.Enchantments;
							//Auras from Toon
							//Should probably convert this to List.Union;
							for(int i = 0; i < ToonEnchants.Count; i++)
							{
								if(enchantmentdamagepenalty < 24 && (ToonEnchants[i].SpellId == 5183 || ToonEnchants[i].SpellId == 4395 || ToonEnchants[i].SpellId == 5997 || ToonEnchants[i].SpellId == 5998))
								{
										enchantmentdamagepenalty = 24;
								}
								else if((ToonEnchants[i].SpellId == 2096 || ToonEnchants[i].SpellId == 5996) && enchantmentdamagepenalty < 22) {enchantmentdamagepenalty = 22;}
								else if((ToonEnchants[i].SpellId == 1616 || ToonEnchants[i].SpellId == 5995) && enchantmentdamagepenalty < 20) {enchantmentdamagepenalty = 20;}
								else if((ToonEnchants[i].SpellId == 1615 || ToonEnchants[i].SpellId == 5994) && enchantmentdamagepenalty < 16) {enchantmentdamagepenalty = 16;}
								else if((ToonEnchants[i].SpellId == 1614 || ToonEnchants[i].SpellId == 5993) && enchantmentdamagepenalty < 12) {enchantmentdamagepenalty = 12;}
								else if((ToonEnchants[i].SpellId == 1613 || ToonEnchants[i].SpellId == 5992) && enchantmentdamagepenalty < 8) {enchantmentdamagepenalty = 8;}
								else if((ToonEnchants[i].SpellId == 1612 || ToonEnchants[i].SpellId == 5991) && enchantmentdamagepenalty < 4) {enchantmentdamagepenalty = 4;}
								else if((ToonEnchants[i].SpellId == 35 || ToonEnchants[i].SpellId == 5990) && enchantmentdamagepenalty < 2) {enchantmentdamagepenalty = 2;}
							}
							damagebase = damageobs - enchantmentdamagepenalty - cantripdamagepenalty;
						}
						if(wo.Values(LongValueKey.WieldReqAttribute) == 44)
						{
							//Best Heavy Weapon (420): 0.29 variance and 63 damage
							//Best Heavy Weapon (420) UA: 0.44 Variance and 55 damage
							//Best Heavy Weapon (420) MS: 0.4 Variance and 34 damage
							//Target weapon (420) == 0.33 Variance and 73 damage (approximate tinks of granite to make it Best 420 normal
							//MS and UA are fudged for animation time since I do not know it exactly
							for(int i = 0; i < 10; i++)
							{
								if((wo.Values(DoubleValueKey.Variance) * Math.Pow(0.80,i)) < 0.33) {granitetinks = i; break;}
							}
							if(MSCleave){mscleaveadjust = 2; fudgefactor = 0.9480;}
							if(WeaponMasteryCategory == 1) {fudgefactor = 1.158;}
						}
						if(wo.Values(LongValueKey.WieldReqAttribute) == 45 || wo.Values(LongValueKey.WieldReqAttribute) == 46)
						{
							//Best L/F Weapon (420): 0.23 Variance and 50 damage
							//Best L/F Weapon (420) UA: 0.43 Variance and 44 damage
							//Best L/F Weapon (420) MS: 0.24 Variance and 24 damage
							//Target Weapon (420) == 0.28 Variance and 60 damage
							//MS and UA are fuged for animation times since I do know now them exactly
							for(int i = 0; i < 10; i++)
							{
								if((wo.Values(DoubleValueKey.Variance) * Math.Pow(0.80,i)) < 0.28) {granitetinks = i; break;}
							}
							//Double the max damage if it is a cleaving/multistrike for comparison
							if(MSCleave){mscleaveadjust = 2; fudgefactor = 0.9666;}
							if(WeaponMasteryCategory == 1) {fudgefactor = 1.153;}	
						}
						if(wo.Values(LongValueKey.WieldReqAttribute) == 41)
						{
							//Best 2H Weapon (420):  0.3 Variance and 45 damage
							//Best 2H Weapon (420) Cleave: 0.3 Variance 42 damage
							//Target 2H Weapon (420):  0.3 Variance and 55 damage
							//The double strike weapon (spear) is adjusted down just as the Multi-Strike ones are animated slower.
							//TODO:  Go back and check this carefully, data from Thresher may not be properly separated.  I do not know if Spears return MSCleave true
							if(!MSCleave){mscleaveadjust = .9454;}	
						}
						
						if(wo.SpellCount > 0)
						{
							for(int i = 0; i <= wo.SpellCount -1; i++)
							{				
								if(wo.Spell(i) == 6089 && cantripdamageboosters < 10){cantripdamageboosters = 10;}
								else if(wo.Spell(i) == 4661 && cantripdamageboosters < 7){cantripdamageboosters = 7;}
								else if(wo.Spell(i) == 2586 && cantripdamageboosters < 4){cantripdamageboosters = 4;}
								else if(wo.Spell(i) == 2598 && cantripdamageboosters < 2) {cantripdamageboosters = 2;}
								else if(wo.Spell(i) == 2486 && cantripdamageboosters < 2) {cantripdamageboosters = 2;}
							}
						}					
						return Convert.ToInt32((damagebase * fudgefactor * mscleaveadjust) + (availabletinks - granitetinks) + cantripdamageboosters);
					}
					
					if(wo.ObjectClass == ObjectClass.MissileWeapon)
					{
						if(!wo.DoubleKeys.Contains((int)DoubleValueKey.DamageBonus)){return 0;}
						double availabletinks = 0;
						if(wo.DoubleKeys.Contains((int)DoubleValueKey.SalvageWorkmanship)) {availabletinks = 10 - wo.Values(LongValueKey.NumberTimesTinkered);}
						double mahoganytinks = 0;
						if(wo.DoubleKeys.Contains((int)DoubleValueKey.DamageBonus)){mahoganytinks = ((wo.Values(DoubleValueKey.DamageBonus) - 1) / 0.04);}
						double elementaldamagebonus = 0;
						if(wo.LongKeys.Contains((int)LongValueKey.ElementalDmgBonus)) {elementaldamagebonus = wo.Values(LongValueKey.ElementalDmgBonus);}
						double cantripdamageboosters = 0;
						//Best XBow (375):  +165% and + 18 Elemental
						//Best Trown (375):  +160% and + 18 Elemental
						//Best Bow (375):  +140% and + 18 Elemental
						//Target Weapon (375) = +165 and + 18 elemental
						
						if(wo.SpellCount > 0)
						{
							for(int i = 0; i <= wo.SpellCount -1; i++)
							{				
								if(wo.Spell(i) == 6089 && cantripdamageboosters < 10){cantripdamageboosters = 10;}
								else if(wo.Spell(i) == 4661 && cantripdamageboosters < 7){cantripdamageboosters = 7;}
								else if(wo.Spell(i) == 2586 && cantripdamageboosters < 4){cantripdamageboosters = 4;}
								else if(wo.Spell(i) == 2598 && cantripdamageboosters < 2) {cantripdamageboosters = 2;}
								else if(wo.Spell(i) == 2486 && cantripdamageboosters < 2) {cantripdamageboosters = 2;}
							}
						}

						return Convert.ToInt32(mahoganytinks + availabletinks + cantripdamageboosters + elementaldamagebonus);
					}
					
					if(wo.ObjectClass == ObjectClass.WandStaffOrb)
					{
						double availabletinks = 0;
						if(wo.DoubleKeys.Contains((int)DoubleValueKey.SalvageWorkmanship)) { availabletinks = 10 - wo.Values(LongValueKey.NumberTimesTinkered);}
						double cantripdamageboosters = 0;
						double elementaldamagevsmonstersobs = 0;
						if(wo.DoubleKeys.Contains((int)DoubleValueKey.ElementalDamageVersusMonsters)) {elementaldamagevsmonstersobs = (wo.Values(DoubleValueKey.ElementalDamageVersusMonsters) -1) * 100;}
						double elementaldamagevsmonstersbase = 0;
						
						if(wo.Values(LongValueKey.EquippedSlots) == 0)
						{
							elementaldamagevsmonstersbase = elementaldamagevsmonstersobs;
						}
						else
						{
							double enchantdampenalty = 0;
							double cantripdampenalty = 0;
							for(int i = 0; i < wo.ActiveSpellCount; i++)
							{

								
								if(wo.ActiveSpell(i) == 6098 && cantripdampenalty < 7){cantripdampenalty = 7;}
								else if(wo.ActiveSpell(i) == 4670 && cantripdampenalty < 5){cantripdampenalty = 5;}
								else if(wo.ActiveSpell(i) == 3250 && cantripdampenalty < 3){cantripdampenalty = 3;}
								else if(wo.ActiveSpell(i) == 3251 && cantripdampenalty < 1) {cantripdampenalty = 1;}
							}
							
							var ToonEnchants = CoreManager.Current.CharacterFilter.Enchantments;
							for(int i = 0; i < ToonEnchants.Count; i++)
							{
								if((ToonEnchants[i].SpellId == 4414 || ToonEnchants[i].SpellId == 5182 || ToonEnchants[i].SpellId == 6022 || ToonEnchants[i].SpellId == 6023) && enchantdampenalty < 8)
								{
									enchantdampenalty = 8;
								}
								else if((ToonEnchants[i].SpellId == 3259 || ToonEnchants[i].SpellId == 6021) && enchantdampenalty < 7) {enchantdampenalty = 7;}
								else if((ToonEnchants[i].SpellId == 3258 || ToonEnchants[i].SpellId == 6020) && enchantdampenalty < 6) {enchantdampenalty = 6;}
								else if((ToonEnchants[i].SpellId == 3257 || ToonEnchants[i].SpellId == 6019) && enchantdampenalty < 5) {enchantdampenalty = 5;}
								else if((ToonEnchants[i].SpellId == 3256 || ToonEnchants[i].SpellId == 6018) && enchantdampenalty < 4) {enchantdampenalty = 4;}
								else if((ToonEnchants[i].SpellId == 3255 || ToonEnchants[i].SpellId == 6017) && enchantdampenalty < 3) {enchantdampenalty = 3;}
								else if((ToonEnchants[i].SpellId == 3254 || ToonEnchants[i].SpellId == 6016) && enchantdampenalty < 2) {enchantdampenalty = 2;}
								else if((ToonEnchants[i].SpellId == 3253 || ToonEnchants[i].SpellId == 6015) && enchantdampenalty < 1) {enchantdampenalty = 1;}
							}
							
							elementaldamagevsmonstersbase = elementaldamagevsmonstersobs - cantripdampenalty - enchantdampenalty;
						}
						
						if(wo.SpellCount > 0)
						{
							for(int i = 0; i <= wo.SpellCount -1; i++)
							{				
								if(wo.Spell(i) == 6098 && cantripdamageboosters < 7){cantripdamageboosters = 7;}
								else if(wo.Spell(i) == 4670 && cantripdamageboosters < 5){cantripdamageboosters = 5;}
								else if(wo.Spell(i) == 3250 && cantripdamageboosters < 3){cantripdamageboosters = 3;}
								else if(wo.Spell(i) == 3251 && cantripdamageboosters < 1) {cantripdamageboosters = 1;}
							}
						}
						if(wo.DoubleKeys.Contains((int)DoubleValueKey.ElementalDamageVersusMonsters))
						{
							return Convert.ToInt32(elementaldamagevsmonstersbase + cantripdamageboosters + availabletinks);
						}
						else{return 0;}
					}	
					return 0;					
				}
			}
						
			public string GearScoreString()
			{
				string gearscorestring = String.Empty;
				if(!wo.HasIdData) {return gearscorestring = "{NO ID} ";}
				if(GearScore  > 0 || SpellScore != String.Empty) {gearscorestring += "{GS";}
				if(GearScore > 0) {gearscorestring += " " + GearScore.ToString("N0");}	
				if(SpellScore != String.Empty) {gearscorestring += " " + SpellScore;}
				if(GearScore  > 0 || SpellScore != String.Empty) {gearscorestring += "} ";}
				return gearscorestring;		
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
						return "(" + rulename + ") ";
					case IOResult.dessicate:
						return "(Dessicate) ";
					default:
						return String.Empty;
				}  
			}
			
			public string MiniIORString()
			{
				switch(IOR)
				{
					case IOResult.trophy:
						return "(T) ";
					case IOResult.rare:
						return "(R) ";
					case IOResult.spell:
						return "(7) ";
					case IOResult.rule:
						return "(" + GearScore + ") ";
					case IOResult.val:
						return "(V) ";
					case IOResult.manatank:
						return "(M) ";
					case IOResult.salvage:
						return "(S) ";
					case IOResult.dessicate:
						return "(D) ";
					default:
						return String.Empty;
				}  
			}
			
			public string TruncateName()
			{
				try
				{	
					if(wo.Name.Length > 8)
					{
						string ReturnString = wo.Name;
						if(ReturnString.Contains("of "))
						{
							ReturnString = ReturnString.Replace("of ","");
						}
						if(ReturnString.Length > 8)
						{
							if(ReturnString.Contains("a"))
							{
								ReturnString = ReturnString.Replace("a", "");
							}
							if(ReturnString.Contains("e"))
							{
								ReturnString = ReturnString.Replace("e", "");
							}
							if(ReturnString.Contains("i"))
							{
								ReturnString = ReturnString.Replace("i", "");
							}
							if(ReturnString.Contains("o"))
							{
								ReturnString = ReturnString.Replace("o", "");
							}
							if(ReturnString.Contains("u"))
							{
								ReturnString = ReturnString.Replace("u", "");
							}
							if(ReturnString.Length > 8)
							{
								if(ReturnString.Contains(" "))
								{
									
									string[] splitstring = ReturnString.Split(' ');
									ReturnString = String.Empty;
									foreach(string piece in splitstring)
									{
										if(piece.Length > 2)
										{
											ReturnString += piece.Substring(0,2);
										}
										else
										{
											ReturnString += piece;
										}
									}
									return ReturnString;
								}
								else
								{
									return ReturnString.Substring(0,10) + ".";
								}
							}
							else
							{
								return ReturnString;
							}	
						}
						else
						{
							return ReturnString;
						}
					}
					else
					{
						return wo.Name;
					}
				}catch(Exception ex){LogError(ex); return String.Empty;}
			}
			
			
			public string DistanceString()
			{
				return " <" + (DistanceAway * 100).ToString("0") + ">";
			}
			
		
			
			public string SkillString()
			{
				if(SkillScore > 0) {return ", Skill Modifiers: " + SkillScore.ToString("N0");}
				else {return String.Empty;}
				
			}
			
			public string RatingString()
			{
				if(RatingScore > 0) {return ", Rating Score: " + RatingScore.ToString("N0");}
				else {return String.Empty;}
			}
			
			public int ArmorType
			{
				get
				{
					//If it doesn't have an armor value, return negative
					if(!wo.LongKeys.Contains((int)LongValueKey.ArmorLevel)) {return -1;}
					//If it's unknown type make it other.
					if(!ArmorIndex.Any(x => wo.Name.ToLower().Contains(x.name.ToLower()))) 
					{
						return ArmorIndex.Find(x => x.name == "Other").ID;
					}
					else if(ArmorIndex.Any(x => wo.Name.ToLower().StartsWith(x.name.ToLower())))
					{
						return ArmorIndex.Find(x => wo.Name.ToLower().StartsWith(x.name.ToLower())).ID;
					}		
					else if(ArmorIndex.Any(x => wo.Name.ToLower().Contains(x.name.ToLower())))
					{
						return ArmorIndex.Find(x => wo.Name.ToLower().Contains(x.name.ToLower())).ID;
					}
					else
					{
						return ArmorIndex.Find(x => x.name == "Other").ID;
					}			
				}
			}
			
			public string ArmorLevelComaparisonString()
			{
				return ", ArmorScore: " + ArmorScore.ToString("N0");
			}

			
			
			public string OffenseString()
			{
				return ", Damage Score: " + OffenseScore.ToString("N0");
			}
			
			//Because of the need to read the icons from the essences for damage types, this can't be read directly from wo.  Combined all for for ease of reference
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
							case 4154:
							case 7664:
							case 29738:
								return 12;  //Naturalist
								
							case 6978:
							case 7285:
							case 9217: 
							case 9218:
							case 29739:		
							case 29743:
							case 29744:
							case 29745:
							case 29746:
						    	return 13;  //Primalist
						    						    
						    case 4646:
						    case 5828:
						    case 13383:
						    	return 14;  //Necro
						    
						    default:
						    	return 0;
						}
					}
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
			
			//I'm keeping these for ease of access.
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
			public int Container 
			{
				get { return wo.Container; }
			}
		
			//wo.properties not readily available from Decal.Adapter.Wrappers (calculated)
			public bool Aetheriacheck
			{
				get
				{
					if(wo.ObjectClass == ObjectClass.Gem && wo.LongKeys.Contains((int)LongValueKey.EquipableSlots) &&
					   (wo.Values(LongValueKey.EquipableSlots) & AetheriaSlots) == wo.Values(LongValueKey.EquipableSlots)) {return true;}
					else {return false;}
				}
			}

			public bool isvalid
			{
				get
				{
					if(wo != null) {return true;}
					else{return false;}
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
					if (nextlvl <= wo.Values((LongValueKey)NewLongKeys.MaxItemLevel) & mItemxp != 0)
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
				int x = wo.Values((LongValueKey)NewLongKeys.MaxItemLevel);
				long result = 0;
				int lvl = 0;
				if (wo.Values((LongValueKey)NewLongKeys.MaxItemLevel) > 0) {
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
			
	
			//StringBuilders for ToString() override below...
			private string CurrentItemLevelString()
			{
				if (wo.LongKeys.Contains((int)NewLongKeys.MaxItemLevel)) {return "(" + CurrentItemLevel() + "/" + wo.Values((LongValueKey)NewLongKeys.MaxItemLevel) + ")";}
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
					if(SpellIndex[wo.Spell(i)].spellname.ToLower().Contains("legendary")) {strspells += ", " + SpellIndex[wo.Spell(i)].spellname;}
					if(SpellIndex[wo.Spell(i)].spellname.ToLower().Contains("epic")) {strspells += ", " + SpellIndex[wo.Spell(i)].spellname;}
					if(SpellIndex[wo.Spell(i)].spellname.ToLower().Contains("major")) {strspells += ", " + SpellIndex[wo.Spell(i)].spellname;}
				}
				return strspells;
			}			
			private string SetString()
			{
				string name = string.Empty;
				if (wo.Values(LongValueKey.ArmorSet) > 0)
				{   //Could go directly to [ArmorSet] but this  won't throw an exception if it's not there
					int idx = SetsIndex.FindIndex(x => x.ID == wo.Values(LongValueKey.ArmorSet));
					if(idx > 0) {name = ", " + SetsIndex[idx].name;}
					else { name = ", Unknown Set " + wo.Values(LongValueKey.ArmorSet).ToString("0x");}
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
			private string ShortWieldString()
			{
				string result = string.Empty;
				if (wo.Values(LongValueKey.WieldReqType) > 0) 
				{
					if (wo.Values(LongValueKey.WieldReqType) == 7) 
					{
						result = " (L" + wo.Values(LongValueKey.WieldReqValue).ToString() + ")";
					} else if (wo.Values(LongValueKey.WieldReqValue) > 0) 
					{
						result = "(" + wo.Values(LongValueKey.WieldReqValue).ToString() +")";
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
				if(x > 1) {return ", " + ((x - 1)*100).ToString("N0") + " " + suffix;}
				else if(x > 0) {return ", " + (x*100).ToString("N0") + " " + suffix;}
				else return String.Empty;				
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
			
			public string GSReportString()
			{
				//builds result string with appropriate goodies to report
				string result = string.Empty;
				try {
					if (wo != null) 
					{
						switch(wo.ObjectClass)
						{
							case ObjectClass.Armor:
								result = IORString() +  GearScoreString() + wo.Name + SetString() +  ArmorLevelComaparisonString() + RatingString() + SpellDescriptions();
								break;
							case ObjectClass.Clothing:
								if(wo.Values(LongValueKey.ArmorLevel) > 0)
								{
									result = IORString() +  GearScoreString() + wo.Name + SetString() +  ArmorLevelComaparisonString() + RatingString() + SpellDescriptions();
									break;
								}
								else if(wo.Values(LongValueKey.EquipableSlots) == CloakSlot)
								{
									result = IORString() +  GearScoreString() + wo.Name + SetString() + RatingString() + SpellDescriptions();
									break;
								}
								else
								{
									result = IORString() + GearScoreString() +  wo.Name + RatingString() + SpellDescriptions();
									break;
								}
							case ObjectClass.Gem:
								if(Aetheriacheck)
								{
									result = IORString() + GearScoreString() + wo.Name + SetString() + SpellDescriptions();
									break;
								}
								else
								{
									result = IORString() + GearScoreString() + wo.Name + SpellDescriptions();
									break;
								}
							case ObjectClass.Jewelry:
								result = IORString() + GearScoreString() + wo.Name + SetString() + RatingString() + SpellDescriptions() + WieldString() + LoreString();
								break;
							case ObjectClass.MeleeWeapon:
							case ObjectClass.MissileWeapon:
							case ObjectClass.WandStaffOrb:
								result = IORString() + GearScoreString() + wo.Name + OffenseString() + SkillString() + RatingString() + SpellDescriptions() + WieldString();
								break;
							case ObjectClass.Salvage:
								result = SalvageString();
								break;
							case ObjectClass.Misc:
								if(EssenceLevel > 0)
								{
									result = IORString() + GearScoreString() + "L" + EssenceLevel + " " + wo.Name + RatingString();
									break;
								}
								else goto default;
							default:
								result = IORString() + wo.Name;
								break;
						}
						
					}
				} catch (Exception ex) {
					LogError(ex);
				}
				return result;
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
								if(wo.Values(LongValueKey.ArmorLevel) > 0)
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
							case ObjectClass.Gem:
								result = IORString() + wo.Name +SetString() + WieldlvlString() + SpellDescriptions();
								break;
							case ObjectClass.Jewelry:
								result = IORString() + wo.Name + SetString() + ALString() + ImbueString() + TinkersString() + SpellDescriptions() + WieldString() + LoreString() + 
									RankString() + RaceString() + CraftString();
								break;
							case ObjectClass.MeleeWeapon:
								result = IORString() + wo.Name +  WeaponMasteryString() + ImbueString() + SlayerString() + TinkersString() + MinMaxDamage() + xModString(wo.Values(DoubleValueKey.AttackBonus), "a") + xModString(wo.Values(DoubleValueKey.MeleeDefenseBonus), "md") +
								SpellDescriptions() + WieldString() + LoreString() + RankString() + RaceString() + CraftString();
								break;
							case ObjectClass.MissileWeapon:
								result = IORString() + wo.Name + WeaponMasteryString() + ImbueString() + SlayerString() +  TinkersString() + xModString(wo.Values(DoubleValueKey.DamageBonus), "ele") + ElementalDmgBonusString()  +
									xModString(wo.Values(DoubleValueKey.MeleeDefenseBonus), "md") + SpellDescriptions() + WieldString() + LoreString() + RankString() + RaceString() + CraftString();
								break;
							case ObjectClass.Salvage:
								result = SalvageString();
								break;
							case ObjectClass.WandStaffOrb:
								result = IORString() + wo.Name + ImbueString() + SlayerString() + TinkersString() + xModString(wo.Values(DoubleValueKey.DamageBonus), "ele") + xModString(wo.Values(DoubleValueKey.MeleeDefenseBonus), "md") +
									xModString(wo.Values(DoubleValueKey.ManaCBonus), "mc") + SpellDescriptions() + WieldString() + LoreString() + RankString() + RaceString() + CraftString();
								break;
							case ObjectClass.Misc:
								if(EssenceLevel > 0)
								{
									result = IORString() + "L" + EssenceLevel + wo.Name;
									break;
								}
								else goto default;
							default:
								result = IORString() + wo.Name;
								break;
						}
						
					}
				} catch (Exception ex) {
					LogError(ex);
				}
				return result;
			}
			
			public void Pals()
			{
				int ModelDWord = wo.Values(LongValueKey.Model);
				
				
			}
		}
	}
}


//Studded Leather Coat
//Model data:  33554644
//Binary:  10000000000000000011010100
//Dword Max:   11111111111111111111111111111111
//Byte 11, always 0x11
//0xFF packed D word Palette
//Binary:  11111111


//using System.Runtime.InteropServices;
//public static byte[] getBytes(object o)
//        {
//            int size = Marshal.SizeOf(o);
//            byte[] arr = new byte[size];
//            IntPtr ptr = Marshal.AllocHGlobal(size);
//            Marshal.StructureToPtr(o, ptr, true);
//            Marshal.Copy(ptr, arr, 0, size);
//            Marshal.FreeHGlobal(ptr);
//            return arr;
//        } 
//
//
//public static object getStruct(byte[] arr, object o)
//        {
//            //object str = new object();
//            int size = Marshal.SizeOf(o);
//            IntPtr ptr = Marshal.AllocHGlobal(size);
//            Marshal.Copy(arr, 0, ptr, size);
//            o = (object)Marshal.PtrToStructure(ptr, o.GetType());
//            Marshal.FreeHGlobal(ptr);
//            return o;
//        }


//private void Echo_CreateObject(IMessage2 msg) // F745
//		{
//			string test = "Start";
//            object o = null;
//			try 
//			{
//                int guid = (int)msg.get_Value("object");
//                //ITEM_CLASS eClass;
//				test = "paletteCount";
//                IMessageMember mem = msg.get_Struct(test = "model");
//				int i;
//				for (i=0; i< m_nKnownItems; i++)
//				{
//					if (KnownItemArray[i].guid == guid) return;
//				}
//                o = mem.get_Value(test = "paletteCount");
//                test = "cast paletteCount";
//                int paletteCount = (byte)o;
//                o = mem.get_Value(test = "textureCount");
//                test = "case textureCount";
//                int textureCount = (byte)o;
//				IMessageIterator aPalettes = (IMessageIterator)mem.get_Struct(test = "palettes");
//                IMessageIterator aTextures = (IMessageIterator)mem.get_Struct(test = "textures");
//
//                mem = msg.get_Struct(test = "game");
//                o = mem.get_Value(test = "name");
//                string name = o.ToString();
//				
//                o = mem.get_Value(test = "type");
//				int model = (int)o;
//
//                o = mem.get_Value(test = "icon");
//                int icon = (int)o;
//
//                o = mem.get_Value(test = "category");
//                int nTypeFlags = (int)o;
//				if ((nTypeFlags & 0x06)==0) return;   // Only Armor and Clothing
//
//				int pyrealvalue;
//				int coverage;
//				int burden;      
//				try { pyrealvalue  = (int)mem.get_Value("value");    }catch{pyrealvalue=0;}
//				try { coverage     = (int)mem.get_Value("coverage1");}catch{coverage   =0;}
//				try { burden       = (int)mem.get_Value("burden");   }catch{burden     =0;}
//			
//				int[]  Color = new int[paletteCount];
//				for(i=0; i<paletteCount; i++)
//				{
//					if (m_bStats)
//					{
//						test = "palette iteration #"+i.ToString();
//						IMessageIterator palette = aPalettes.NextObjectIndex;
//						int iOS = palette.get_NextInt(test="palette");
//                        int iOffset = palette.get_NextInt(test="offset");
//                        int iSize = palette.get_NextInt(test ="length");
//                        Color[i] = iOS; //  + palette.get_NextInt("length") * 256;
//						//if (ColorTable.Contains(Color[i]) == false)
//						{
//							test = Color[i].ToString()
//								+ "," + iOffset.ToString()
//								+ "," + iSize.ToString();
//                            m_Hooks.AddChatText("DCS: Color " + test + " on " + name, 14, 1);
//							StreamWriter sw = new StreamWriter(/*m_sAssemblyPath+*/"F:\\ColorTrap.csv",true);
//							sw.Write(name+","+model.ToString()+",#"+coverage.ToString("X8")+","+i.ToString()+","+test+m_sEOL);
//							sw.Flush();
//							sw.Close();
//						}
//					}
//					else
//					{
//						test = "palette iteration #"+i.ToString();
//						IMessageIterator palette = aPalettes.NextObjectIndex;
//                        Color[i] = palette.get_NextInt(test = "palette");
//					}
//				}
//
//				COLOR_INFO NewColor;
//				//AC_MODEL   acModel;
//				ushort     iModel = (ushort)model;
//				/*
//				eClass = ITEM_CLASS.NONE;
//				switch (coverage)
//				{
//					case 0x00200000:  // Shield
//						return;
//					case 0x00000400:  // Girth
//						eClass |= ITEM_CLASS.GIRTH;
//						acModel = new AC_MODEL(iModel, "AB", name);
//						break;
//					case 0x00000200:  // Breastplate
//						eClass |= ITEM_CLASS.BREASTPLATE;
//						acModel = new AC_MODEL(iModel, "AB", name);
//						break;
//					case 0x00001800:  // Sleeves
//						eClass |= ITEM_CLASS.SLEEVES;
//						acModel = new AC_MODEL(iModel, "AC", name);
//						break;
//					case 0x00000600:  // Curaiss
//						eClass |= ITEM_CLASS.CURAISS;
//						acModel = new AC_MODEL(iModel, "AC", name);
//						break;
//					case 0x00000E00:  // Short Sleeve Shirt
//						eClass |= ITEM_CLASS.OVERSHIRT;
//						acModel = new AC_MODEL(iModel, "AD", name);
//						break;
//					case 0x00001A00:  // Coat
//						eClass |= ITEM_CLASS.COAT;
//						acModel = new AC_MODEL(iModel, "ABD", name);
//						break;
//					case 0x00002000:  // Tassets (have no color)
//						acModel = new AC_MODEL(iModel, "", name);
//						break;
//					case 0x00007F00:  // Robe
//					case 0x00007F01:  // Hooded Robe
//						eClass |= ITEM_CLASS.ROBE;
//						acModel = new AC_MODEL(iModel, "ABDE", name);
//						break;
//					case 0x00001E00:  // Hauberk  
//						eClass |= ITEM_CLASS.HAUBERK;
//						acModel = new AC_MODEL(iModel, "AE", name);
//						break;
//					case 0x00006400:  // Pants
//						eClass |= ITEM_CLASS.OVERPANTS;
//						if (model == 6004)
//							acModel = new AC_MODEL(iModel, "ACDE", name);
//						else
//							acModel = new AC_MODEL(iModel, "AC", name);
//						break;
//					default: 
//						acModel = new AC_MODEL(iModel, "ABCD", name);
//						break;
//				}
//				*/
//
//				
//				/*if (ModelTable.Contains(model))
//				{
//					AC_MODEL acModel = (AC_MODEL)ModelTable[model];
//					NewColor = new COLOR_INFO(guid,name,model,icon,coverage,acModel.Colors);
//					for (i=0; i < acModel.Colors; i++)
//					{
//						NewColor.SetColor(i,Color[acModel.GetColor(i)]);
//					}
//				}
//				else 
//				{ */
//					NewColor = new COLOR_INFO(guid,name,model,icon,coverage,4);
//					string sColors = "";
//					string sColorCodes = ",";
//					if (paletteCount > 0)
//					{
//						sColors = "A";
//						sColorCodes = ","+Color[0].ToString();
//						NewColor.SetColor(0,Color[0]);
//						for (i=1; i < paletteCount; i++)
//						{
//							sColorCodes += ","+Color[i].ToString();
//							if (sColors.Length < 4)
//							{
//								if (Color[i] != Color[i-1])
//								{
//									NewColor.SetColor(sColors.Length,Color[i]);
//									sColors += (char)('A' + i);
//								}
//							}
//						}
//					}
//					if (m_bDebug)
//					{
//						ModelTable[model] = new AC_MODEL(iModel,sColors,name);
//						StreamWriter sw = new StreamWriter(m_sAssemblyPath+"\\ModelTrap.csv",true);
//						sw.WriteLine(model.ToString()+",\""+name+"\",#"+coverage.ToString("X8")+sColorCodes);
//						sw.Flush();
//						sw.Close();
//						m_Hooks.AddChatText(model.ToString()+",\""+name+"\",#"+coverage.ToString("X8")+","+sColors+sColorCodes,7,1);
//					}
//				/*}*/
//				
//				if (m_nKnownItems > MAX_ITEMS)
//				{
//					CheckKnown();
//				}
//				else if (m_nKnownItems > MAX_ITEMS)
//				{
//					m_Hooks.AddChatText("DCS: ****ERROR*** Inventory Overflow",10,1);
//					return;
//				}
//				int ins = m_nKnownItems;
//				while (ins>0)
//				{
//					ins--;
//					if (KnownItemArray[ins].coverage > coverage) {ins++; break;}
//					if (KnownItemArray[ins].coverage == coverage)
//					{
//						if( KnownItemArray[ins].model < model) {ins++; break;}
//						if( KnownItemArray[ins].model == model)
//						{
//							if( KnownItemArray[ins].icon < icon) {ins++; break;}
//							if( KnownItemArray[ins].icon == icon)
//							{
//								if( KnownItemArray[ins].name.CompareTo(name)<=0) {ins++; break;}
//							}
//						}
//					}
//				}
//				int iPos;
//				for (iPos=m_nKnownItems-1;iPos>=ins;iPos--)
//				{
//					KnownItemArray[iPos+1] = KnownItemArray[iPos];
//				}
//				//				m_Hooks.AddChatText(name,4);
//				//				for (iPos--;iPos>=0;iPos--)
//				//				{
//				//					m_Hooks.AddChatText(KnownItemArray[iPos].name,7);
//				//				}
//
//				KnownItemArray[ins] = NewColor;
//				m_nKnownItems++;    
//				if (cbAll.Checked)
//				{
//					m_Hooks.IDQueueAdd(guid);
//				}
//			}
//			catch (Exception ex)
//			{
//				/*if (m_bDebug)*/ m_Hooks.AddChatText("DCS: Error on " + test + "--" + ex.Message + "( object is "+o.GetType().ToString()+")",7,1);
//			}
//		}


//Palette Reporting
//	foreach (br current8 in A_0.t)
//	{
//		List<string> arg_8A7_0 = list;
//		string[] array = new string[10];
//		array[0] = "Palette Entry ";
//		array[1] = num++.ToString();
//		array[2] = ": ID 0x";
//		int num2 = current8.a;
//		array[3] = num2.ToString("X6");
//		array[4] = ", Ex Color: ";
//		array[5] = (current8.a().ToArgb() & 16777215).ToString("X6");   //16777215 == FFFFFF
//		array[6] = ", ";
//		byte b = current8.b;
//		array[7] = b.ToString();
//		array[8] = "/";
//		string[] arg_89F_0 = array;
//		int arg_89F_1 = 9;
//		byte b2 = current8.c;
//		arg_89F_0[arg_89F_1] = b2.ToString();
//		byte b2 = current8.c;
//		array[9] = b2.ToString();
//		arg_8A7_0.Add(string.Concat(array));
//	}

//public Color a()
//{
//	int num = (int)(this.c * 16 + this.b * 32 + 8);
//	Color result;
//	try
//	{
//		byte[] array = g.a(this.a);
//		result = Color.FromArgb((int)array[num + 3], (int)array[num + 2], (int)array[num + 1], (int)array[num]);
//	}
//	catch
//	{
//		result = Color.Black;
//	}
//	return result;
//}

//internal struct br
//{
//	public int a;
//	public byte b;
//	public byte c;
//	public Color a()
//	{
//		int num = (int)(this.c * 16 + this.b * 32 + 8);
//		Color result;
//		try
//		{
//			byte[] array = g.a(this.a);
//			result = Color.FromArgb((int)array[num + 3], (int)array[num + 2], (int)array[num + 1], (int)array[num]);
//		}
//		catch
//		{
//			result = Color.Black;
//		}
//		return result;
//	}
//}

//		private void a()
//		{
//			this.e = new List<GameItemInfo.PaletteData>();
//			foreach (br current in this.a.t)
//			{
//				this.e.Add(new GameItemInfo.PaletteData(current));
//			}
//		}

		
		
