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
	/// <summary>
	/// Description of IOMagToolsEcho.
	/// </summary>
	public class IOMagToolsEcho
	{
		public List<int> ActiveSpells = new List<int>();
		public List<int> Spells = new List<int>();
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		public string ReportMT(WorldObject wo)
		{
			try
			{
				FillMagSpells(wo);
				
				switch(wo.ObjectClass)
				{
					case ObjectClass.MeleeWeapon:
						sb.Append(CalcedBuffedTinkedDoT.ToString("N1") + "/" + GetBuffedIntValueKey(wo, LongValueKey.MaxDamage));
						goto default;
					
					case ObjectClass.MissileWeapon:
						sb.Append(CalcedBuffedMissileDamage.ToString("N1"));
						goto default;
						
					case ObjectClass.WandStaffOrb:
						sb.Append(((GetBuffedDoubleValueKey(wo, DoubleValueKey.ElementalDamageVersusMonsters) - 1) * 100).ToString("N1"));
						goto default;
					default:
						if (wo.Values(DoubleValueKey.AttackBonus, 1) != 1)
						{
							sb.Append(Math.Round(((GetBuffedDoubleValueKey(wo, DoubleValueKey.AttackBonus) - 1) * 100)).ToString("N1") + "/");
						}
						if (wo.Values(DoubleValueKey.MeleeDefenseBonus, 1) != 1)
						{
							sb.Append(Math.Round(((GetBuffedDoubleValueKey(wo, DoubleValueKey.MeleeDefenseBonus) - 1) * 100)).ToString("N1"));
						}
						if (wo.Values(DoubleValueKey.ManaCBonus) != 0)
						{
							sb.Append("/" + Math.Round(GetBuffedDoubleValueKey(wo, DoubleValueKey.ManaCBonus) * 100));
						}	
						return String.Empty;
				}
			}catch(Exception ex){GearFoundry.PluginCore.LogError(ex);}
		}
		
		public int GetBuffedIntValueKey(WorldObject wo, LongValueKey key)
		{
			if (!(wo.Values(key) > 0)){	return 0;}

			int value = wo.Values(key);

			foreach (int spell in ActiveSpells)
			{
				if (Constants.LongValueKeySpellEffects.ContainsKey(spell) && Constants.LongValueKeySpellEffects[spell].Key == key)
					value -= Constants.LongValueKeySpellEffects[spell].Change;
			}

			foreach (int spell in Spells)
			{
				if (Constants.LongValueKeySpellEffects.ContainsKey(spell) && Constants.LongValueKeySpellEffects[spell].Key == key)
					value += Constants.LongValueKeySpellEffects[spell].Bonus;
			}

			return value;
		}
		
		public void FillMagSpells(WorldObject wo)
		{
			try
			{ 
				ActiveSpells.Clear();
				Spells.Clear();
				for (int i = 0 ; i < wo.ActiveSpellCount ; i++)
				ActiveSpells.Add(wo.ActiveSpell(i));

				for (int i = 0; i < wo.SpellCount; i++)
				Spells.Add(wo.Spell(i));
			}catch(Exception ex){GearFoundry.PluginCore.LogError(ex);}
		}
		
		public double GetBuffedDoubleValueKey(WorldObject wo, DoubleValueKey key)
		{
			if (!(wo.Values(key) > 0)){return 0;}

			double value = wo.Values(key);

			foreach (int spell in ActiveSpells)
			{
				if (Constants.DoubleValueKeySpellEffects.ContainsKey(spell) && Constants.DoubleValueKeySpellEffects[spell].Key == key)
				{
					if (Math.Abs(Constants.DoubleValueKeySpellEffects[spell].Change - 1) < Double.Epsilon)
						value /= Constants.DoubleValueKeySpellEffects[spell].Change;
					else
						value -= Constants.DoubleValueKeySpellEffects[spell].Change;
				}
			}
		}
		
//					
//					
//					public double CalcedBuffedTinkedDoT()
//					{
//
//						if (!DoubleValues.ContainsKey(167772171) || !IntValues.ContainsKey(218103842))
//							return -1;
//		
//						double variance = DoubleValues.ContainsKey(167772171) ? DoubleValues[167772171] : 0;
//						int maxDamage = GetBuffedIntValueKey(218103842);
//		
//						int numberOfTinksLeft = Math.Max(10 - Math.Max(Tinks, 0), 0);
//		
//						if (!IntValues.ContainsKey(179) || IntValues[179] == 0)
//							numberOfTinksLeft--; // Factor in an imbue tink
//		
//						// If this is not a loot generated item, it can't be tinked
//						if (!IntValues.ContainsKey(131) || IntValues[131] == 0)
//							numberOfTinksLeft = 0;
//		
//						for (int i = 1; i <= numberOfTinksLeft; i++)
//						{
//							double ironTinkDoT = CalculateDamageOverTime(maxDamage + 24 + 1, variance);
//							double graniteTinkDoT = CalculateDamageOverTime(maxDamage + 24, variance * .8);
//		
//							if (ironTinkDoT >= graniteTinkDoT)
//								maxDamage++;
//							else
//								variance *= .8;
//						}
//						return CalculateDamageOverTime(maxDamage + 24, variance);
//					}
//					
//					public int GetBuffedIntValueKey(int key, int defaultValue = 0)
//		{
//			if (!IntValues.ContainsKey(key))
//				return defaultValue;
//
//			int value = IntValues[key];
//
//			foreach (int spell in ActiveSpells)
//			{
//				if (Constants.LongValueKeySpellEffects.ContainsKey(spell) && Constants.LongValueKeySpellEffects[spell].Key == key)
//					value -= Constants.LongValueKeySpellEffects[spell].Change;
//			}
//
//			foreach (int spell in Spells)
//			{
//				if (Constants.LongValueKeySpellEffects.ContainsKey(spell) && Constants.LongValueKeySpellEffects[spell].Key == key)
//					value += Constants.LongValueKeySpellEffects[spell].Bonus;
//			}
//
//			return value;
//		}
//

//
//			foreach (int spell in Spells)
//			{
//				if (Constants.DoubleValueKeySpellEffects.ContainsKey(spell) && Constants.DoubleValueKeySpellEffects[spell].Key == key && Math.Abs(Constants.DoubleValueKeySpellEffects[spell].Bonus - 0) > Double.Epsilon)
//				{
//					if (Math.Abs(Constants.DoubleValueKeySpellEffects[spell].Change - 1) < Double.Epsilon)
//						value *= Constants.DoubleValueKeySpellEffects[spell].Bonus;
//					else
//						value += Constants.DoubleValueKeySpellEffects[spell].Bonus;
//				}
//			}
//
//			return value;
//		}
//
//		/// <summary>
//		/// maxDamage * ((1 - critChance) * (2 - variance) / 2 + (critChance * critMultiplier));
//		/// </summary>
//		/// <param name="maxDamage"></param>
//		/// <param name="variance"></param>
//		/// <param name="critChance"></param>
//		/// <param name="critMultiplier"></param>
//		/// <returns></returns>
//		public static double CalculateDamageOverTime(int maxDamage, double variance, double critChance = .1, double critMultiplier = 2)
//		{
//			return maxDamage * ((1 - critChance) * (2 - variance) / 2 + (critChance * critMultiplier));
//		}
//				}catch(Exception ex){LogError(Exception);}
//			}
//
	}
}
