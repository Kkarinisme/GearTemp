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

namespace GearFoundry
{

	public partial class PluginCore
	{		

		//Use of this general class has been removed.  It is currently being maintained only for reference purposes.  
		//It has been replaced by LandscapeObject, MonsterObject and LootObject		
		public class IdentifiedObject
		{
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
			
			public double GearScore
			{	
				get
				{
					double fudgefactor = 0;
					double gearscorereturn = 0;
					switch(wo.ObjectClass)
					{
						case ObjectClass.Gem:
							if(wo.Values(LongValueKey.EquipableSlots) == (0x10000000 | 0x20000000 | 0x40000000)) {gearscorereturn += (double)wo.Values((LongValueKey)NewLongKeys.MaxItemLevel);}
							break;
							
						case ObjectClass.Clothing:
							if(wo.Values(LongValueKey.EquipableSlots) == 0x8000000) {gearscorereturn += (double)wo.Values((LongValueKey)NewLongKeys.MaxItemLevel);}
							if(wo.Values(LongValueKey.ArmorLevel) > 0) {gearscorereturn += ArmorScore;}
							break;
		
						case ObjectClass.Armor:
							gearscorereturn += ArmorScore;
							break;
		
						case ObjectClass.MeleeWeapon:
							gearscorereturn += OffenseScore + WeaponModifiers;
							break;
		
						case ObjectClass.MissileWeapon:
							//Best XBow (375):  +165% and + 18 Elemental
							//Best Trown (375):  +160% and + 18 Elemental
							//Best Bow (375):  +140% and + 18 Elemental
							//Target Weapon (375) = +165 and + 18 elemental
							if(WeaponMasteryCategory == (int)WeaponMastery.Bow) {fudgefactor = 6;}
							if(WeaponMasteryCategory == (int)WeaponMastery.Thrown) {fudgefactor = 1;}
							gearscorereturn += OffenseScore + WeaponModifiers + fudgefactor;
							break;
		
						case ObjectClass.WandStaffOrb:
							gearscorereturn += OffenseScore + WeaponModifiers;
							break;
						
						default:
							break;
		
					}
					gearscorereturn += BonusComparison;
					return gearscorereturn;	
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
					if(CantripArray[1] > 0) {ReportString += CantripArray[1].ToString() + "Mj";}
					if(CantripArray[0] > 0) {ReportString += CantripArray[0].ToString() + "Mn";}
					return ReportString;
				}
			}
			
			private double ArmorScore
			{
				get
				{
					//Unworn armor with the ability to be enchanted.
					//Calculate base AL, Add Cantrips, adjust for tinkers.
					if(wo.Values(LongValueKey.Unenchantable) == 0)
					{
						if(wo.Values(LongValueKey.ActiveSpellCount) == 0)
						{
							double steelvalue = wo.Values(LongValueKey.ArmorLevel) / 20;
							double cantripsteelbonus = 0;
							double tinerkersavaible = 10;
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
							return steelvalue + cantripsteelbonus + tinerkersavaible - wo.Values(LongValueKey.NumberTimesTinkered);
						}
						else
						{
							//Non-equipped, but enchanted armor (cantrips not firing)
							//Calculate base AL, Add cantrips, subtract highest impen.  Adjust for tinkers.
							if(wo.Values(LongValueKey.EquippedSlots) == 0)
							{
								double steelvalue = wo.Values(LongValueKey.ArmorLevel) / 20;
								double cantripsteelbonus = 0;
								double tinerkersavaible = 10;
								double enchantmentpenalty = 0;
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
								for(int i = 0; i < wo.ActiveSpellCount; i++)
								{								
									if(wo.Spell(i) == 4407 && enchantmentpenalty < 12){enchantmentpenalty = 12;}
									else if(wo.Spell(i) == 3908 && enchantmentpenalty < 12){enchantmentpenalty = 12;}
									else if(wo.Spell(i) == 2108 && enchantmentpenalty < 11){enchantmentpenalty = 11;}
									else if(wo.Spell(i) == 1486 && enchantmentpenalty < 10){enchantmentpenalty = 10;}
									else if(wo.Spell(i) == 1485 && enchantmentpenalty < 7.5){enchantmentpenalty = 7.5;}
									else if(wo.Spell(i) == 1484 && enchantmentpenalty < 5){enchantmentpenalty = 5;}
									else if(wo.Spell(i) == 1483 && enchantmentpenalty < 3.75){enchantmentpenalty = 3.75;}
									else if(wo.Spell(i) == 1482 && enchantmentpenalty < 2.5){enchantmentpenalty = 2.5;}
									else if(wo.Spell(i) == 51 && enchantmentpenalty < 1){enchantmentpenalty = 1;}
								}
								return steelvalue - enchantmentpenalty + cantripsteelbonus + tinerkersavaible - wo.Values(LongValueKey.NumberTimesTinkered);
							}
							//Equipped, Enchanted
							//Calcuate base AL, Ignore cantrips if firing (have to check), adjust for highest impen, adjust for tinkers.
							else
							{
								double steelvalue = wo.Values(LongValueKey.ArmorLevel) / 20;
								double cantripsteelbonus = 0;
								double tinerkersavaible = 10;
								double enchantmentpenalty = 0;
								int HighestImpenCantrip = 0;
								if(wo.SpellCount > 0)
								{
									for(int i = 0; i < wo.SpellCount; i++)
									{				
										if(wo.Spell(i) == 6095 && cantripsteelbonus < 4){cantripsteelbonus = 4; HighestImpenCantrip = 6095;}
										else if(wo.Spell(i) == 4667 && cantripsteelbonus < 3){cantripsteelbonus = 3; HighestImpenCantrip = 4667;}
										else if(wo.Spell(i) == 2592 && cantripsteelbonus < 2){cantripsteelbonus = 2; HighestImpenCantrip = 2592;}
										else if(wo.Spell(i) == 2604 && cantripsteelbonus < 1){cantripsteelbonus = 1; HighestImpenCantrip = 2604;}
									}
								}
								for(int i = 0; i < wo.ActiveSpellCount; i++)
								{	
									//Remove cantrip bonus if it's firing.
									if(wo.Spell(i) == HighestImpenCantrip) {cantripsteelbonus = 0;}
									if(wo.Spell(i) == 4407 && enchantmentpenalty < 12){enchantmentpenalty = 12;}
									else if(wo.Spell(i) == 3908 && enchantmentpenalty < 12){enchantmentpenalty = 12;}
									else if(wo.Spell(i) == 2108 && enchantmentpenalty < 11){enchantmentpenalty = 11;}
									else if(wo.Spell(i) == 1486 && enchantmentpenalty < 10){enchantmentpenalty = 10;}
									else if(wo.Spell(i) == 1485 && enchantmentpenalty < 7.5){enchantmentpenalty = 7.5;}
									else if(wo.Spell(i) == 1484 && enchantmentpenalty < 5){enchantmentpenalty = 5;}
									else if(wo.Spell(i) == 1483 && enchantmentpenalty < 3.75){enchantmentpenalty = 3.75;}
									else if(wo.Spell(i) == 1482 && enchantmentpenalty < 2.5){enchantmentpenalty = 2.5;}
									else if(wo.Spell(i) == 51 && enchantmentpenalty < 1){enchantmentpenalty = 1;}	
								}
								return steelvalue + cantripsteelbonus - enchantmentpenalty + tinerkersavaible - wo.Values(LongValueKey.NumberTimesTinkered);						
							}
			
						}
						
					}
					//Calculation for non-weilded unenchantable armor.  
					else 
					{
						//No enchants firing			
						if(wo.Values(LongValueKey.ActiveSpellCount) == 0)
						{
							double steelvalue = wo.Values(LongValueKey.ArmorLevel) / 20;
							double cantripsteelbonus = 0;
							double enchantmentsteelbonus = 0;
							double protectionpenatly = 0;
							double tinkersavailable = 10;
							
							
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
							
							return steelvalue + cantripsteelbonus + enchantmentsteelbonus + tinkersavailable - wo.Values(LongValueKey.NumberTimesTinkered) - protectionpenatly;
							
						}
						//Calculation for wielded unenchantable armor with enchants firing
						else 
						{
							double steelvalue = wo.Values(LongValueKey.ArmorLevel) / 20;
							double protectionspenalty = 0;
							double tinkersavailable = 10;
							
							double slashtinkspenalty = 2 - (wo.Values(DoubleValueKey.SlashProt) / 0.2);
							double piercetinkspenalty = 2 - (wo.Values(DoubleValueKey.PierceProt) / 0.2);
							double bludgetinkspenalty = 2 - (wo.Values(DoubleValueKey.BludgeonProt) / 0.2);
							double acidtinkspenalty = 2 - (wo.Values(DoubleValueKey.AcidProt)/ 0.4);
							double coldtinkspenalty = 2 - (wo.Values(DoubleValueKey.ColdProt)/ 0.4);
							double firetinkspenalty = 2 - (wo.Values(DoubleValueKey.FireProt)/ 0.4);
							double lighttinkspentalty = 2 - (wo.Values(DoubleValueKey.LightningProt)/ 0.4);
							
							if(slashtinkspenalty > 0) {protectionspenalty += slashtinkspenalty;}
							if(piercetinkspenalty > 0) {protectionspenalty += piercetinkspenalty;}
							if(bludgetinkspenalty > 0) {protectionspenalty += bludgetinkspenalty;}
							if(acidtinkspenalty > 0) {protectionspenalty += acidtinkspenalty;}
							if(coldtinkspenalty > 0) {protectionspenalty += coldtinkspenalty;}
							if(firetinkspenalty > 0) {protectionspenalty += firetinkspenalty;}
							if(lighttinkspentalty > 0) {protectionspenalty += lighttinkspentalty;}
							
							return steelvalue - protectionspenalty + tinkersavailable - wo.Values(LongValueKey.NumberTimesTinkered);				
						}	
					}
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
					default:
						return String.Empty;
				}  
			}
			
			public string DistanceString()
			{
				return " <" + (DistanceAway * 100).ToString("0") + ">";
			}
		
//			//not in worldfilter set by OnIdentObject:
//			private int mHealthMax;
//			private int mHealthCurrent;
//			private int mStaminaMax;
//			private int mStaminaCurrent;
//			private int mManaMax;
//			private int mManaCurrent;
			
			public double BonusComparison
			{
				get
				{
					return (double)wo.Values((LongValueKey)NewLongKeys.Crit) + (double)wo.Values((LongValueKey)NewLongKeys.CritResist) + 
						(double)wo.Values((LongValueKey)NewLongKeys.CritDam) + (double)wo.Values((LongValueKey)NewLongKeys.CritDamResist) + 
						(double)wo.Values((LongValueKey)NewLongKeys.Dam) + (double)wo.Values((LongValueKey)NewLongKeys.DamResist);
				}
			}
				
			//Modified Looting Properties (calculated)
			public double WeaponModifiers
			{
				get
				{	
					double modsum = 0;
					double cantripattackboosters = 0;
					double cantripdefenseboosters = 0;
					double cantripmanaconversionboosters = 0;
					
					if(wo.DoubleKeys.Contains((int)DoubleValueKey.MeleeDefenseBonus)){modsum += (wo.Values(DoubleValueKey.MeleeDefenseBonus) - 1) * 100;}
					if(wo.DoubleKeys.Contains((int)DoubleValueKey.AttackBonus)) {modsum += (wo.Values(DoubleValueKey.AttackBonus) - 1) * 100;}
					if(wo.DoubleKeys.Contains((int)DoubleValueKey.ManaCBonus)) {modsum += wo.Values(DoubleValueKey.ManaCBonus) * 100;}
					if(wo.DoubleKeys.Contains((int)DoubleValueKey.MagicDBonus)) {modsum += (wo.Values(DoubleValueKey.MagicDBonus) - 1) * 100;}
					if(wo.DoubleKeys.Contains((int)DoubleValueKey.MissileDBonus)){modsum += (wo.Values(DoubleValueKey.MissileDBonus) -1 ) * 100;}	
					
					if(wo.SpellCount > 0)
					{
						for(int i = 0; i <= wo.SpellCount -1; i++)
						{				
							if(wo.Spell(i) == 6094 && cantripattackboosters < 9){cantripattackboosters = 9;}
							else if(wo.Spell(i) == 4666 && cantripattackboosters < 7){cantripattackboosters = 7;}
							else if(wo.Spell(i) == 2591 && cantripattackboosters < 5) {cantripattackboosters = 5;}
							else if(wo.Spell(i) == 2603 && cantripattackboosters < 3) {cantripattackboosters = 3;}
							
							if(wo.Spell(i) == 6091 && cantripdefenseboosters < 9){cantripdefenseboosters = 9;}
							else if(wo.Spell(i) == 4663 && cantripdefenseboosters < 7){cantripdefenseboosters = 7;}
							else if(wo.Spell(i) == 2588 && cantripdefenseboosters < 5) {cantripdefenseboosters = 5;}
							else if(wo.Spell(i) == 2600 && cantripdefenseboosters < 3) {cantripdefenseboosters = 3;}	
							
							if(wo.ObjectClass == ObjectClass.WandStaffOrb)
							{
								if(wo.Spell(i) == 6087 && cantripmanaconversionboosters < 30){cantripmanaconversionboosters = 30;}
								else if(wo.Spell(i) == 6086 && cantripmanaconversionboosters < 25){cantripmanaconversionboosters = 25;}
								else if(wo.Spell(i) == 3200 && cantripmanaconversionboosters < 20){cantripmanaconversionboosters = 20;}
								else if(wo.Spell(i) == 3202 && cantripmanaconversionboosters < 20){cantripmanaconversionboosters = 15;}
								else if(wo.Spell(i) == 3199 && cantripmanaconversionboosters < 10){cantripmanaconversionboosters = 10;}
								else if(wo.Spell(i) == 3201 && cantripmanaconversionboosters < 5){cantripmanaconversionboosters = 5;}
							}
						}
					}
					if(wo.ObjectClass == ObjectClass.WandStaffOrb && wo.Values(DoubleValueKey.ElementalDamageVersusMonsters) == 0)
					{
						return modsum + cantripattackboosters + cantripdefenseboosters + ((wo.Values(DoubleValueKey.ManaCBonus) * 100) * (cantripmanaconversionboosters * .01)) + 10 - wo.Values(LongValueKey.NumberTimesTinkered);
					}
					return modsum + cantripattackboosters + cantripdefenseboosters + ((wo.Values(DoubleValueKey.ManaCBonus) * 100) * (cantripmanaconversionboosters * .01));
				}
			}
			
			public string WeaponModString()
			{
				if(WeaponModifiers > 0) {return " WepMods: " + WeaponModifiers.ToString("N1");}
				else {return String.Empty;}
				
			}
			
			public double ArmorLevelComaparison
			{
				get 
				{
					if(wo.Values(LongValueKey.Unenchantable) > 0)
					{
						double steelvalue = wo.Values(LongValueKey.ArmorLevel) / 20;
						double cantripsteelbonus = 0;
						double enchantmentsteelbonus = 0;
						double protectionpenatly = 0;
						double tinkersavailable = 10;
						
						
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
						
						return steelvalue + cantripsteelbonus + enchantmentsteelbonus + tinkersavailable - wo.Values(LongValueKey.NumberTimesTinkered) - protectionpenatly;
						
					}
					else
					{
						double steelvalue = wo.Values(LongValueKey.ArmorLevel) / 20;
						double cantripsteelbonus = 0;
						double tinerkersavaible = 10;
						if(wo.SpellCount > 0)
						{
							for(int i = 0; i <= wo.SpellCount -1; i++)
							{				
								if(wo.Spell(i) == 6095 && cantripsteelbonus < 4){cantripsteelbonus = 4;}
								else if(wo.Spell(i) == 4667 && cantripsteelbonus < 3){cantripsteelbonus = 3;}
								else if(wo.Spell(i) == 2592 && cantripsteelbonus < 2){cantripsteelbonus = 2;}
								else if(wo.Spell(i) == 2604 && cantripsteelbonus < 1){cantripsteelbonus = 1;}
							}
						}
						return steelvalue + cantripsteelbonus + tinerkersavaible - wo.Values(LongValueKey.NumberTimesTinkered);
						
					}
					
				}
				
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
				return " ArmorScore " + ArmorLevelComaparison.ToString("N1");
			}

			public double OffenseScore
			{
				get
				{
					if(wo.ObjectClass == ObjectClass.MeleeWeapon)
					{
						double availabletinks = 10;
						double granitetinks = 0;
						double fudgefactor = 1;
						double mscleaveadjust = 1;
						double cantripdamageboosters = 0;
						
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
						return (wo.Values(LongValueKey.MaxDamage) * fudgefactor * mscleaveadjust) + (availabletinks - granitetinks) + cantripdamageboosters
							- wo.Values(LongValueKey.NumberTimesTinkered);
					}
					
					if(wo.ObjectClass == ObjectClass.MissileWeapon)
					{
						double availabletinks = 10;
						double mahoganytinks = 0;
						double cantripdamageboosters = 0;
						//Best XBow (375):  +165% and + 18 Elemental
						//Best Trown (375):  +160% and + 18 Elemental
						//Best Bow (375):  +140% and + 18 Elemental
						//Target Weapon (375) = +165 and + 18 elemental
						
						mahoganytinks = ((wo.Values(DoubleValueKey.DamageBonus) - 1) / 0.04);
						
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
						return mahoganytinks + availabletinks + cantripdamageboosters + (double)wo.Values(LongValueKey.ElementalDmgBonus) - wo.Values(LongValueKey.NumberTimesTinkered);										
					}
					
					if(wo.ObjectClass == ObjectClass.WandStaffOrb)
					{
						double availabletinks = 10;
						double cantripdamageboosters = 0;
						double elementaldamagevsmonsters = 0;
						
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
						if(wo.DoubleKeys.Contains((int)DoubleValueKey.ElementalDamageVersusMonsters)){elementaldamagevsmonsters = ((wo.Values(DoubleValueKey.ElementalDamageVersusMonsters) -1) * 100);}
						if(elementaldamagevsmonsters > 0)
						{							
							return availabletinks + elementaldamagevsmonsters  + cantripdamageboosters - wo.Values(LongValueKey.NumberTimesTinkered);
						}
						else
						{
							return 0;
						}
					}										
					return 0;					
				}
			}

			
			public string OffenseString()
			{
				return " Dam: " + OffenseScore.ToString("N0") ;
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
								return 1;  //Naturalist
								
							case 6978:
							case 7285:
							case 9217: 
							case 9218:
							case 29739:		
							case 29743:
							case 29744:
							case 29745:
							case 29746:
						    	return 2;  //Primalist
						    						    
						    case 4646:
						    case 5828:
						    case 13383:
						    	return 3;  //Necro
						    
						    default:
						    	return 0;
						}
					}
					else {return 0;}
				}
			}
			

			//TODO:  This should be rolled into the matching function in identify
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
					if (wo.Values(LongValueKey.EquipableSlots) == (0x10000000 | 0x20000000 | 0x40000000)) {return true;}
					else {return false;}
				}
			}

			public bool isvalid
			{
				get
				{
					return host.Underlying.Hooks.IsValidObject(wo.Id);
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
					if(idx > 0) {name = SetsIndex[idx].name;}
					else { name = "Unknown Set " + wo.Values(LongValueKey.ArmorSet).ToString("0x");}
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

			
			public string ModString()
			{
				//builds result string with appropriate goodies to report
				string result = string.Empty;
				try {
					if (wo != null) 
					{
						switch(wo.ObjectClass)
						{
							case ObjectClass.Armor:
								result = IORString() +  GearScoreString() + wo.Name + SetString() +  ArmorLevelComaparisonString() + SpellDescriptions() + WieldString() + ActivateString() + LoreString();
								break;
							case ObjectClass.Clothing:
								if(wo.Values(LongValueKey.ArmorLevel) > 0 || wo.Values(LongValueKey.EquipableSlots) == 0x8000000)
								{
									result = IORString() + GearScoreString() + wo.Name + SetString() + ArmorLevelComaparisonString() + SpellDescriptions() + WieldString() + ActivateString() +	LoreString();
								}
								else
								{
									result = IORString() + wo.Name + SetString() + SpellDescriptions() + WieldString() + ActivateString() + LoreString();
								}
								break;
							case ObjectClass.Container:
								result = IORString() + wo.Name + CoordsStringLink(wo.Coordinates().ToString());
								break;
							case ObjectClass.Corpse:
								result = IORString() + wo.Name + CoordsStringLink(wo.Coordinates().ToString());
								break;
							case ObjectClass.Gem:
								if(Aetheriacheck)
								{
									result = IORString() + GearScoreString() + wo.Name + SetString() + WieldlvlString() + SpellDescriptions();
								}
								result = IORString() + wo.Name + SetString() + WieldlvlString() + SpellDescriptions();
								break;
							case ObjectClass.Jewelry:
								result = IORString() + wo.Name + SetString() + ImbueString() + SpellDescriptions() + WieldString() + LoreString();
								break;
							case ObjectClass.Lifestone:
								result = IORString() + wo.Name + CoordsStringLink(wo.Coordinates().ToString());
								break;
							case ObjectClass.MeleeWeapon:
								result = IORString() + GearScoreString() + wo.Name +  OffenseString() + WeaponModString() +SpellDescriptions() + WieldString() + LoreString();
								break;
							case ObjectClass.MissileWeapon:
								result = IORString() + GearScoreString() + wo.Name + OffenseString() + WeaponModString() +SpellDescriptions() + WieldString() + LoreString();
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
								result = IORString() + GearScoreString() + wo.Name + OffenseString() + WeaponModString() + SpellDescriptions() + WieldString() + LoreString();
								break;
							case ObjectClass.Misc:
								if(EssenceLevel > 0)
								{
									result = IORString() + GearScoreString() + "L" + EssenceLevel + " " + wo.Name;
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
								result = IORString() + wo.Name +  WeaponMasteryString() + ImbueString() + SlayerString() + TinkersString() + MinMaxDamage() + xModString(wo.Values(DoubleValueKey.AttackBonus), "a") + xModString(wo.Values(DoubleValueKey.MeleeDefenseBonus), "md") +
								SpellDescriptions() + WieldString() + LoreString() + RankString() + RaceString() + CraftString();
								break;
							case ObjectClass.MissileWeapon:
								result = IORString() + wo.Name + WeaponMasteryString() + ImbueString() + SlayerString() +  TinkersString() + xModString(wo.Values(DoubleValueKey.DamageBonus), string.Empty) + ElementalDmgBonusString()  +
									xModString(wo.Values(DoubleValueKey.MeleeDefenseBonus), "md") + SpellDescriptions() + WieldString() + LoreString() + RankString() + RaceString() + CraftString();
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
								result = IORString() + wo.Name + ImbueString() + SlayerString() + TinkersString() + xModString(wo.Values(DoubleValueKey.DamageBonus), "vs. Monsters") + xModString(wo.Values(DoubleValueKey.MeleeDefenseBonus), "md") +
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
								result = IORString() + wo.Name + CoordsStringLink(wo.Coordinates().ToString());
								break;
						}
						
					}
				} catch (Exception ex) {
					LogError(ex);
				}
				return result;
			}
		}
	}
}


		
		
