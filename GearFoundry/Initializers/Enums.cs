/*
 * Created by SharpDevelop.
 * User: Paul
 * Date: 6/12/2013
 * Time: 6:55 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace GearFoundry
{
	/// <summary>
	/// Description of Enums.
	/// </summary>
	public partial class PluginCore
	{
		internal enum NewLongKeys
		{
			SummoningSkill = 367,
			WeaponMastery = 353,
			WieldReqAttribute2 = 271,
			WieldReqValue2 = 272,
			WieldReqType2 = 270,
			Dam = 370,
			DamResist = 371,
			Crit = 372,
			CritResist = 373,
			CritDam = 374,
			CritDamResist = 375,
			DamageAbsorb = 352,	
			MaxItemLevel = 319				
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
            dessicate,
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
            aetheria,
           	unknown
        }
	}
}
