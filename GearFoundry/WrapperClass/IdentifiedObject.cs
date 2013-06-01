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
		
		internal enum WeaponMastery
		{	
			None = 0,
			Unarmed = 1,
			Sword = 2,
			Axe = 3,
			Mace = 4,
			Spear = 5,
			Dagger = 6,
			Staff = 7,
			Bow= 8,
			Crossbow = 9,
			Thrown = 10,
			TwoHanded = 11			
		}
		

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
			public double GearScore
			{
				get
				{
					double fudgefactor = 0;
					double gearscorereturn = 0;
					switch(wo.ObjectClass)
					{
						case ObjectClass.Gem:
							if(Aetheriacheck) {gearscorereturn += (double)MaxItemLevel;}
							break;
							
						case ObjectClass.Clothing:
							if(WieldSlot == 0x8000000) {gearscorereturn += (double)MaxItemLevel;}
							if(ArmorLevel > 0) {gearscorereturn += ArmorLevelComaparison;}
							break;

						case ObjectClass.Armor:
							gearscorereturn += ArmorLevelComaparison;
							break;

						case ObjectClass.MeleeWeapon:
							gearscorereturn += DamageComparison + WeaponModifiers;
							break;

						case ObjectClass.MissileWeapon:
							//Best XBow (375):  +165% and + 18 Elemental
							//Best Trown (375):  +160% and + 18 Elemental
							//Best Bow (375):  +140% and + 18 Elemental
							//Target Weapon (375) = +165 and + 18 elemental
							if(WeaponMasteryCategory == (int)WeaponMastery.Bow) {fudgefactor = 6;}
							if(WeaponMasteryCategory == (int)WeaponMastery.Thrown) {fudgefactor = 1;}
							gearscorereturn += DamageComparison + WeaponModifiers + fudgefactor;
							break;

						case ObjectClass.WandStaffOrb:
							gearscorereturn += DamageComparison + WeaponModifiers;
							break;
						
						default:
							gearscorereturn = 0;
							break;

					}
					gearscorereturn += BonusComparison;
					return gearscorereturn;
				}
			}
			
			public string ExtendedGearScore()
			{
				return String.Empty;
			}
			
			public string GearScoreString()
			{
				string gearscorestring = String.Empty;
				if(!wo.HasIdData) {return gearscorestring = "{NO ID}";}
				if(GearScore > 0) {gearscorestring += "{GS " + GearScore.ToString("N0") + "} ";}
				return gearscorestring;
			}
			
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
						return "(" + rulename + ") ";
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
			
			public double BonusComparison
			{
				get
				{
					return Crit + CritResist + CritDam + CritDamResist + Dam + DamResist;
				}
			}
			
			
			//These are not scoring properly
//[VTank] --------------Object dump--------------
//[VTank] [Meta] Create count: 1
//[VTank] [Meta] Create time: 5/31/2013 7:22 AM
//[VTank] [Meta] Has identify data: True
//[VTank] [Meta] Last ID time: 5/31/2013 7:25 AM
//[VTank] [Meta] Worldfilter valid: True
//[VTank] ID: 86BCAFA8
//[VTank] ObjectClass: Misc
//[VTank] (S) Name: Blizzard Wisp Essence
//[VTank] (S) UsageInstructions: Use this essence to summon or dismiss your Blizzard Wisp.
//[VTank] (B) CanBeSold: True
//[VTank] (I) CreateFlags1: 1076382872
//[VTank] (I) Type: 49309
//[VTank] (I) Icon: 29739
//[VTank] (I) Category: 128
//[VTank] (I) Behavior: 67108882
//[VTank] (I) CreateFlags2: 7
//[VTank] (I) IconUnderlay: 29728
//[VTank] (I) Value: 10000
//[VTank] (I) Unknown10: 8
//[VTank] (I) UsageMask: 16
//[VTank] (I) IconOutline: 128
//[VTank] (I) UsesRemaining: 50
//[VTank] (I) UsesTotal: 50
//[VTank] (I) Container: 1342600506
//[VTank] (I) Burden: 50
//[VTank] (I) IconOverlay: 29736
//[VTank] (I) PhysicsDataFlags: 137345
//[VTank] (I) 368: 54
//[VTank] (I) 369: 185
//[VTank] (I) Bonded: 0
//[VTank] (I) Attuned: 0
//[VTank] (I) 374: 7
//[VTank] (I) 375: 13
//[VTank] (I) CooldownID: 213
//[VTank] (I) Workmanship: 8
//[VTank] (I) 366: 54
//[VTank] (I) 367: 570
//[VTank] (D) 167: 45
//[VTank] Palette Entry 0: ID 0x000BEF, Ex Color: 000000, 0/0

//
//[VTank] --------------Object dump--------------
//[VTank] [Meta] Create count: 1
//[VTank] [Meta] Create time: 5/31/2013 7:22 AM
//[VTank] [Meta] Has identify data: True
//[VTank] [Meta] Last ID time: 5/31/2013 7:26 AM
//[VTank] [Meta] Worldfilter valid: True
//[VTank] ID: 86BCB1F3
//[VTank] ObjectClass: Misc
//[VTank] (S) Name: Caustic Grievver Essence
//[VTank] (S) UsageInstructions: Use this essence to summon or dismiss your Caustic Grievver.
//[VTank] (B) CanBeSold: True
//[VTank] (I) CreateFlags1: 1076382872
//[VTank] (I) Type: 49372
//[VTank] (I) Icon: 7664
//[VTank] (I) Category: 128
//[VTank] (I) Behavior: 67108882
//[VTank] (I) CreateFlags2: 7
//[VTank] (I) IconUnderlay: 29728
//[VTank] (I) Value: 10000
//[VTank] (I) Unknown10: 8
//[VTank] (I) UsageMask: 16
//[VTank] (I) IconOutline: 256
//[VTank] (I) UsesRemaining: 50
//[VTank] (I) UsesTotal: 50
//[VTank] (I) Container: 1342600506
//[VTank] (I) Burden: 50
//[VTank] (I) IconOverlay: 29736
//[VTank] (I) PhysicsDataFlags: 137345
//[VTank] (I) 368: 54
//[VTank] (I) 369: 185
//[VTank] (I) Bonded: 0
//[VTank] (I) Attuned: 0
//[VTank] (I) 372: 5
//[VTank] (I) 374: 10
//[VTank] (I) 375: 7
//[VTank] (I) CooldownID: 213
//[VTank] (I) Workmanship: 8
//[VTank] (I) 366: 54
//[VTank] (I) 367: 570
//[VTank] (D) 167: 45
//[VTank] Palette Entry 0: ID 0x000BF0, Ex Color: 000000, 0/0
			
			
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
								//Armor Level modifers
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
								
								//Pierce modifers
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
								
								//Bludgeon  modifers
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
								
								//Acid modifers
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
								
								//Fire modifers
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
								
								//Cold modifers
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
								
								//Lightning modifers
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
			
			public string ArmorLevelComaparisonString()
			{
				return " ArmorScore " + ArmorLevelComaparison.ToString("N1");
			}

			public double DamageComparison
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
			public bool Unehcantable
			{
				get
				{
					if(wo.Values(LongValueKey.Unenchantable) > 0) {return true;}
					else return false;
				}
			}
			
			public string DamageString()
			{
				return " Dam: " + DamageComparison.ToString("N0") ;
			}
			
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
			public int Matieral
			{
				get
				{
					if(wo.Values(LongValueKey.Material) > 0) { return wo.Values(LongValueKey.Material);}
					else {return -1;}
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
			public int Dam
			{	//wo LongValueKey@370 contains
				get
				{
					if (wo.LongKeys.Contains(370)) {return wo.Values((LongValueKey)370);}
					else {return 0;}
				}
			}
			public int DamResist 
			{	//wo LongValueKey@371 contains 
				get
				{
					if (wo.LongKeys.Contains(371)) {return wo.Values((LongValueKey)371);}
					else {return 0;}
				}
			}
			public int Crit 
			{	//wo LongValueKey@372 contains
				get
				{
					if (wo.LongKeys.Contains(372)) {return wo.Values((LongValueKey)372);}
					else {return 0;}
				}
			}
			public int CritResist
			{	//wo LongValueKey@373 contains
				get
				{
					if (wo.LongKeys.Contains(373)) {return wo.Values((LongValueKey)373);}
					else {return 0;}
				}
			}
			public int CritDam
			{
				get
				{
					if (wo.LongKeys.Contains(374)) {return wo.Values((LongValueKey)374);}
					else {return 0;}
				}
			}
			public int CritDamResist
			{
				get
				{
					if (wo.LongKeys.Contains(375)) {return wo.Values((LongValueKey)375);}
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
					if(SpellIndex[wo.Spell(i)].spellname.ToLower().Contains("legendary")) {strspells += ", " + SpellIndex[wo.Spell(i)].spellname;}
					if(SpellIndex[wo.Spell(i)].spellname.ToLower().Contains("epic")) {strspells += ", " + SpellIndex[wo.Spell(i)].spellname;}
					if(SpellIndex[wo.Spell(i)].spellname.ToLower().Contains("major")) {strspells += ", " + SpellIndex[wo.Spell(i)].spellname;}
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
								if(ArmorLevel > 0)
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
								result = IORString() + wo.Name + SetString() + WieldlvlString() + SpellDescriptions();
								break;
							case ObjectClass.Jewelry:
								result = IORString() + wo.Name + SetString() + ImbueString() + SpellDescriptions() + WieldString() + LoreString();
								break;
							case ObjectClass.Lifestone:
								result = IORString() + wo.Name + CoordsStringLink(wo.Coordinates().ToString());
								break;
							case ObjectClass.MeleeWeapon:
								result = IORString() + GearScoreString() + wo.Name +  DamageString() + WeaponModString() +SpellDescriptions() + WieldString() + LoreString();
								break;
							case ObjectClass.MissileWeapon:
								result = IORString() + GearScoreString() + wo.Name + DamageString() + WeaponModString() +SpellDescriptions() + WieldString() + LoreString();
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
								result = IORString() + GearScoreString() + wo.Name + DamageString() + WeaponModString() + SpellDescriptions() + WieldString() + LoreString();
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


		
		
