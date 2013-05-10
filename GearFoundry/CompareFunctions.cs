/*
 * Created by SharpDevelop.
 * User: Paul
 * Date: 1/15/2013
 * Time: 6:15 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Decal.Adapter.Wrappers;
using Decal.Adapter;
using Decal.Filters;
using System.Windows.Forms;
using System.Linq;

namespace GearFoundry
{
	public partial class PluginCore
	{

		private List<ItemRule> ItemRulesList = new List<ItemRule>();
		
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
					
					//TODO:  Debug this!
					
//					if(!Boolean.TryParse(splitstringEnabled[0], out tRule.RuleWeaponEnabledA)) {tRule.RuleWeaponEnabledA = false;}
//					if(tRule.RuleWeaponEnabledA)
//					{
//						if(!Boolean.TryParse(splitstringMSCleave[0], out tRule.MSCleaveA)) {tRule.MSCleaveA = false;}
//						if(!Int32.TryParse(splitstringWield[0], out tRule.WieldReqValueA)) {tRule.WieldReqValueA = 0;}
//						damagesplit = splitstringDamage[0].Split('-');
//						if(damagesplit.Length == 2)
//						{
//							if(!Int32.TryParse(damagesplit[1], out tRule.MaxDamageA)) {tRule.MaxDamageA = 0;}
//							int tint;
//							if(!Int32.TryParse(damagesplit[0], out tint)) {tint = 0;}
//							if(tRule.MaxDamageA > 0) {tRule.VarianceA = (tRule.MaxDamageA - tint)/tRule.MaxDamageA;}
//							else {tRule.VarianceA = 0;}
//						}
//						else
//						{
//							if(!Int32.TryParse(damagesplit[0], out tRule.MaxDamageA)) {tRule.MaxDamageA = 0;}
//							tRule.VarianceA = 0;
//						}
//					}
//					else
//					{
//						tRule.MSCleaveA = false; tRule.MaxDamageA = 0; tRule.VarianceA = 0;
//					}
//					
//					if(splitstring.Length > 1)
//					{
//						if(!Boolean.TryParse(splitstringEnabled[1], out tRule.RuleWeaponEnabledB)) {tRule.RuleWeaponEnabledB = false;}
//						if(tRule.RuleWeaponEnabledB)
//						{
//							if(!Boolean.TryParse(splitstringMSCleave[1], out tRule.MSCleaveB)) {tRule.MSCleaveB = false;}
//							if(!Int32.TryParse(splitstringWield[1], out tRule.WieldReqValueB)) {tRule.WieldReqValueB = 0;}
//							damagesplit = splitstringDamage[1].Split('-');
//							if(damagesplit.Length == 2)
//							{
//								if(!Int32.TryParse(damagesplit[1], out tRule.MaxDamageB)) {tRule.MaxDamageB = 0;}
//								int tint;
//								if(!Int32.TryParse(damagesplit[0], out tint)) {tint = 0;}
//								if(tRule.MaxDamageB > 0) {tRule.VarianceB = (tRule.MaxDamageB - tint)/tRule.MaxDamageB;}
//								else {tRule.VarianceB = 0;}
//							
//							}
//							else
//							{
//								if(!Int32.TryParse(damagesplit[0], out tRule.MaxDamageB)) {tRule.MaxDamageB = 0;}
//								tRule.VarianceB = 0;
//							}
//						}
//					}
//					else
//					{
//						tRule.MSCleaveB = false; tRule.MaxDamageB = 0; tRule.VarianceB = 0;
//					}
//					
//					if(splitstring.Length > 2)	
//					{
//						if(!Boolean.TryParse(splitstringEnabled[2], out tRule.RuleWeaponEnabledC)) {tRule.RuleWeaponEnabledC = false;}
//						if(tRule.RuleWeaponEnabledC)
//						{
//							if(!Boolean.TryParse(splitstringMSCleave[2], out tRule.MSCleaveC)) {tRule.MSCleaveC = false;}
//							if(!Int32.TryParse(splitstringWield[2], out tRule.WieldReqValueC)) {tRule.WieldReqValueC = 0;}
//							damagesplit = splitstringDamage[2].Split('-');
//							if(damagesplit.Length == 2)
//							{
//								if(!Int32.TryParse(damagesplit[1], out tRule.MaxDamageC)) {tRule.MaxDamageC = 0;}
//								int tint;
//								if(!Int32.TryParse(damagesplit[0], out tint)) {tint = 0;}
//								if(tRule.MaxDamageC > 0) {tRule.VarianceC = (tRule.MaxDamageC - tint)/tRule.MaxDamageC;}
//								else {tRule.VarianceC = 0;}
//							
//							}
//							else
//							{
//								if(!Int32.TryParse(damagesplit[0], out tRule.MaxDamageC)) {tRule.MaxDamageC = 0;}
//								tRule.VarianceC = 0;
//							}
//						}
//					}
//					else
//					{
//						tRule.MSCleaveC = false; tRule.MaxDamageC = 0; tRule.VarianceC = 0;
//					}
//					
//					if(splitstring.Length > 3)	
//					{
//						if(!Boolean.TryParse(splitstringEnabled[3], out tRule.RuleWeaponEnabledD)) {tRule.RuleWeaponEnabledD = false;}
//						if(tRule.RuleWeaponEnabledD)
//						{
//							if(!Boolean.TryParse(splitstringMSCleave[3], out tRule.MSCleaveD)) {tRule.MSCleaveD = false;}
//							if(!Int32.TryParse(splitstringWield[3], out tRule.WieldReqValueD)) {tRule.WieldReqValueD = 0;}
//							damagesplit = splitstringDamage[3].Split('-');
//							if(damagesplit.Length == 2)
//							{
//								if(!Int32.TryParse(damagesplit[1], out tRule.MaxDamageD)) {tRule.MaxDamageD = 0;}
//								int tint;
//								if(!Int32.TryParse(damagesplit[0], out tint)) {tint = 0;}
//								if(tRule.MaxDamageD > 0) {tRule.VarianceD = (tRule.MaxDamageD - tint)/tRule.MaxDamageD;}
//								else {tRule.VarianceD = 0;}
//							
//							}
//							else
//							{
//								if(!Int32.TryParse(damagesplit[0], out tRule.MaxDamageD)) {tRule.MaxDamageD = 0;}
//								tRule.VarianceD = 0;
//							}
//						}
//					}
//					else
//					{
//						tRule.MSCleaveD = false; tRule.MaxDamageD = 0; tRule.VarianceD = 0;
//					}		
					
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


