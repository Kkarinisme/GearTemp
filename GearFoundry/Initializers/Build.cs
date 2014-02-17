/*
 * Created by SharpDevelop.
 * User: Paul
 * Date: 1/7/2013
 * Time: 8:13 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Decal.Adapter;
using Decal.Interop.Core;
using Decal.Filters;
using Decal.Adapter.Wrappers;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using VirindiViewService;
using MyClasses.MetaViewWrappers;
using VirindiViewService.Controls;



namespace GearFoundry
{

	
	public partial class PluginCore
	{
		DirectoryInfo pluginPersonalFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Decal Plugins\");
		bool writelists = false;
        
        //From: Karin.  Lists for use with XDocuments composed of XElements
        private List<XElement> mSortedMobsList = new List<XElement>();
        private List<XElement> mSortedTrophiesList = new List<XElement>();
        private List<XElement> mSalvageList = new List<XElement>();
        private List<XElement> mPrioritizedRulesList = new List<XElement>();
        private List<XElement> mGenSettingsList = new List<XElement>();
        private List<XElement> mSwitchGearSettingsList = new List<XElement>();

        //From: Karin.  Lists for use with cboboxes using IDNameLoadable class
        private static List<IDNameLoadable> ClassInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> MeleeTypeInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> ArmorSetsInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> MaterialInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> ElementalInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> ArmorLevelInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> SalvageWorkInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> WeaponWieldInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> CoverageInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> EmbueInvList = new List<IDNameLoadable>();
        
        //Lists for LootObject GearScore Calcs
        private static List<IntDoubleLoadable> ImpenCantripList = new List<IntDoubleLoadable>();
        private static List<IntDoubleLoadable> ImpenList = new List<IntDoubleLoadable>();
        private static List<IntDoubleLoadable> BaneList = new List<IntDoubleLoadable>();
        private static List<IntDoubleLoadable> BaneCantripList = new List<IntDoubleLoadable>();
        
        //Lists for Advaned Editor
        private static List<IDNameLoadable> LongKeyList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> DoubleKeyList = new List<IDNameLoadable>();
        

		private static List<IDNameLoadable> ElementalList = new List<IDNameLoadable>();
		private static List<IDNameLoadable> MasteryIndex = new List<IDNameLoadable>();
		private static List<IDNameLoadable> SetsIndex = new List<IDNameLoadable>();
        private static List<IDNameLoadable> ArmorSetsList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> ImbueList = new List<IDNameLoadable>();
		private static List<IDNameLoadable> ArmorIndex = new List<IDNameLoadable>();
		private static List<IDNameLoadable> ObjectList = new List<IDNameLoadable>();
		private static List<IDNameLoadable> SlotList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> WeaponTypeList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> AppliesToList = new List<IDNameLoadable>();


        //From:  Irquk - to replace old dictionary lookups
        private static List<spellinfo> SpellIndex = new List<spellinfo>();
        private static List<spellinfo> ItemsSpellList = new List<spellinfo>();
        private static List<spellinfo> FilteredSpellIndex = new List<spellinfo>();
        private static List<IDName> HeritageIndex = new List<IDName>();
        private static List<IDName> SkillIndex = new List<IDName>();
        private static List<IDName> AttribIndex = new List<IDName>();
        private static List<IDName> MaterialIndex = new List<IDName>();
        private static List<IDName> SpeciesIndex = new List<IDName>();


        private void ClearRuleRelatedComponents()
        {
            ClearRuleLists();
       //     ClearRuleListVues();
        }

        private void ClearRuleLists()
        {
            if (mSortedMobsList != null) { mSortedMobsList.Clear(); }
            if (mSortedTrophiesList != null) { mSortedTrophiesList.Clear(); }
            if (mSalvageList != null) { mSalvageList.Clear(); }
            if (mPrioritizedRulesList != null) { mPrioritizedRulesList.Clear(); }
            if (mGenSettingsList != null) { mGenSettingsList.Clear(); }
            if (mSwitchGearSettingsList != null) { mSwitchGearSettingsList.Clear(); }
            if (ClassInvList != null) { ClassInvList.Clear(); }
            if (ArmorSetsInvList != null) { ArmorSetsInvList.Clear(); }
            if (MaterialInvList != null) { MaterialInvList.Clear(); }
            if (ElementalInvList != null) { ElementalInvList.Clear(); }
            if (ArmorLevelInvList != null) { ArmorLevelInvList.Clear(); }
            if (SalvageWorkInvList != null) { SalvageWorkInvList.Clear(); }
            if (WeaponWieldInvList != null) { WeaponWieldInvList.Clear(); }
            if (CoverageInvList != null) { CoverageInvList.Clear(); }
            if (EmbueInvList != null) { EmbueInvList.Clear(); }
            if (ElementalList != null) { ElementalList.Clear(); }
            if (MasteryIndex != null) { MasteryIndex.Clear(); }
            if (SetsIndex != null) { SetsIndex.Clear(); }
            if (ArmorSetsList != null) { ArmorSetsList.Clear(); }
            if (ImbueList != null) { ImbueList.Clear(); }
            if (ArmorIndex != null) { ArmorIndex.Clear(); }
            if (ObjectList != null) { ObjectList.Clear(); }
            if (SlotList != null) { SlotList.Clear(); }
            if (WeaponTypeList != null) { WeaponTypeList.Clear(); }
            if (AppliesToList != null) { AppliesToList.Clear(); }
            if (SpellIndex != null) { SpellIndex.Clear(); }
            if (ItemsSpellList != null) { ItemsSpellList.Clear(); }
            if (FilteredSpellIndex != null) { FilteredSpellIndex.Clear(); }
            if (HeritageIndex != null) { HeritageIndex.Clear(); }
            if (SkillIndex != null) { SkillIndex.Clear(); }
            if (AttribIndex != null) { AttribIndex.Clear(); }
            if (MaterialIndex != null)  {MaterialIndex.Clear(); }
            if (SpeciesIndex != null) { SpeciesIndex.Clear(); }
            if (LongKeyList != null) {LongKeyList.Clear();}
            if (DoubleKeyList != null) {DoubleKeyList.Clear();}

        }

        private void ClearRuleListVues()
        {
            if (lstRuleApplies != null) { lstRuleApplies.ClearRows();}
            if (lstDamageTypes != null) { lstDamageTypes.ClearRows(); }
            if (lstRuleSlots != null) { lstRuleSlots.ClearRows(); }
            if (lstRuleArmorTypes != null) { lstRuleArmorTypes.ClearRows(); }
            if (lstRuleSets != null) { lstRuleSets.ClearRows(); }
            if (lstRuleSpellsEnabled != null) { lstRuleSpellsEnabled.ClearRows(); }
            if (lstRuleSpells != null) { lstRuleSpells.ClearRows(); }
            if (lstRules != null) { lstRules.ClearRows(); }
            if (cboWeaponAppliesTo != null) { cboWeaponAppliesTo.Clear(); }
            if (cboMasteryType != null) { cboMasteryType.Clear(); }
            if (lstmyTrophies != null) { lstmyTrophies.ClearRows(); }
            if (lstmyMobs != null) { lstmyMobs.ClearRows(); }
            if (lstNotifySalvage != null) { lstNotifySalvage.ClearRows(); }

        }

 
        private class IDNameLoadable
		{
			public int ID;
			public string name;
			
			public IDNameLoadable(int i, string s)
			{
				ID =  i;
				name = s;
			}

		}
        
        private class IntDoubleLoadable
		{
			public int ID;
			public double Val;
			public int Ele;
			public int Series;
			
			public IntDoubleLoadable(int i, double j, int elemental, int stack)
			{
				ID =  i;
				Val = j;
				Ele = elemental;
				Series = stack;				
			}
		}
		
		private class IDName
		{
			public int ID = 0;
			public string name = String.Empty;
		}
		
		private class spellinfo
		{
			public int spellid = 0;
			public string spellname = String.Empty;
			public string spellschool = String.Empty;
			public SpellComponentIDs componentIds;
			public int spelllevel = 0;
			public bool irresistable = false;
			public bool isdebuff = false;
			public bool isoffensive = false;
			public bool isuntargeted = false;
			public bool isfellowship = false;
			public int spellicon = 0;
			public double duration = 0;
			public int animation = 0;
		}
		
		private void InitListBuilder()
		{
            CreateWeaponTypeList();
            CreateElementalList();
            CreateSpellIndex();
			CreateHeritageIndex();
		//	CreateMasteryIndex();
			CreateSkillIndex();
			CreateAttribIndex();
			CreateMaterialIndex();
			CreateSpeciesIndex();
			CreateSetsIndex();
			CreateImbueList();
			CreateArmorIndex();
			CreateSlotList();
            CreateAppliesToList();
            CreateItemSpellsList();
            CreateArmorSetsList();
            CreateArmorSetsInvList();
            CreateElementalInvList();
            CreateClassInvList();
            CreateMeleeTypeInvList();
            CreateMaterialInvList();
            CreateArmorLevelInvList();
            CreateSalvageWorkInvList();
            CreateWeaponWieldInvList();
            CreateCoverageInvList();
            CreateEmbueInvList();
            CreateKeyLists();
         //   InitSoundsFunctions();
		}
		
		private void CreateKeyLists()
		{
			try
			{
				IDNameLoadable info;
				info = new IDNameLoadable((int)LongValueKey.ActivationReqSkillId, "ActivationReqSkillId");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)LongValueKey.ActiveSpellCount, "ActiveSpellCount");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)LongValueKey.ArmorLevel, "ArmorLevel");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)LongValueKey.CleaveType, "CleaveType");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)LongValueKey.ElementalDmgBonus, "ElementalDmgBonus");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)LongValueKey.Heritage, "Heritage");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)LongValueKey.MagicDef, "MagicDef");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)LongValueKey.MaxDamage, "MaxDamage");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)LongValueKey.MissileType, "MissileType");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)LongValueKey.RankRequirement, "RankRequirement");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)LongValueKey.Unenchantable, "Unenchantable");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)LongValueKey.WandElemDmgType, "WandElemDmgType");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)LongValueKey.WeapSpeed, "WeapSpeed");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)LongValueKey.WieldReqAttribute, "WieldReqAttribute");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)LongValueKey.WieldReqValue, "WieldReqValue");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)NewLongKeys.Crit, "Crit");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)NewLongKeys.CritDam, "CritDam");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)NewLongKeys.CritDamResist, "CritDamResist");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)NewLongKeys.CritResist, "CritResist");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)NewLongKeys.Dam, "Dam");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)NewLongKeys.DamageAbsorb, "DamAbsorb");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)NewLongKeys.DamResist, "DamResist");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)NewLongKeys.MaxItemLevel, "MaxItemLevel");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)NewLongKeys.SummoningSkill, "SummoningSkill");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)NewLongKeys.WeaponMastery, "WeaponMastery");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)NewLongKeys.WieldReqAttribute2, "WieldReqAttribute2");
				LongKeyList.Add(info);
				info = new IDNameLoadable((int)NewLongKeys.WieldReqValue2, "WieldReqValue2");
				LongKeyList.Add(info);

				info = new IDNameLoadable((int)DoubleValueKey.AttackBonus, "AttackBonus");
				DoubleKeyList.Add(info);
				info = new IDNameLoadable((int)DoubleValueKey.DamageBonus, "DamageBonus");
				DoubleKeyList.Add(info);
				info = new IDNameLoadable((int)DoubleValueKey.ElementalDamageVersusMonsters, "ElementalDamVsMonsters");
				DoubleKeyList.Add(info);
				info = new IDNameLoadable((int)DoubleValueKey.MagicDBonus, "MagicDBonus");
				DoubleKeyList.Add(info);
				info = new IDNameLoadable((int)DoubleValueKey.ManaCBonus, "ManaCBonus");
				DoubleKeyList.Add(info);
				info = new IDNameLoadable((int)DoubleValueKey.MeleeDefenseBonus, "MeleeDBonus");
				DoubleKeyList.Add(info);
				info = new IDNameLoadable((int)DoubleValueKey.MissileDBonus, "MissileDBonus");
				DoubleKeyList.Add(info);
				info = new IDNameLoadable((int)DoubleValueKey.Variance, "Variance");
				DoubleKeyList.Add(info);
				
				
			}catch(Exception ex){LogError(ex);}
		}

        private void CreateAppliesToList()
        {
            try
            {


                IDNameLoadable info;

                info = new IDNameLoadable(0x1, "Melee Weapons");
                AppliesToList.Add(info);
                info = new IDNameLoadable(0x2, "Armor");
                AppliesToList.Add(info);
                info = new IDNameLoadable(0x4, "Clothing");
                AppliesToList.Add(info);
                info = new IDNameLoadable(0x8, "Jewelry");
                AppliesToList.Add(info);
                info = new IDNameLoadable(0x80, "Essences");
                AppliesToList.Add(info);
                info = new IDNameLoadable(0x100, "Missile Weapons");
                AppliesToList.Add(info);
                info = new IDNameLoadable(0x800, "Aetheria");
                AppliesToList.Add(info);
                //info = new IDNameLoadable(0x2000, "Scrolls");
                //AppliesToList.Add(info);
                info = new IDNameLoadable(0x8000, "Casters");
                AppliesToList.Add(info);
                
            }
            catch (Exception ex) { LogError(ex); }

            if (writelists)
            {
                FileInfo logFile = new FileInfo(GearDir + @"\AppliesToList.csv");
                if (logFile.Exists)
                {
                    logFile.Delete();
                }
                StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
                StringBuilder output = new StringBuilder();
                output.Append("Index, ID#, Category\n");
                int i = 0;
                foreach (var idname in AppliesToList)
                {
                    output.Append((i++).ToString() + "," + idname.ID.ToString() + "," + idname.name.ToString() + "\n");
                }
                writer0.WriteLine(output);
                writer0.Close();
            }
        }
		
		private void CreateSlotList()
		{
			//Irq:  One last time, flags suck..
			//Commented out useless slots, included only against future need....
			IDNameLoadable info;
			
			info = new IDNameLoadable(0x1 ,"Head");
			SlotList.Add(info);
			info = new IDNameLoadable(0x2 ,"Shirt");
			SlotList.Add(info);
			info = new IDNameLoadable(0x40 ,"Pants");
			SlotList.Add(info);
//			info = new IDNameLoadable(0x8 ,"UpperArms(Underwear)");
//			SlotList.Add(info);
//			info = new IDNameLoadable(0x10 ,"LowerArms(Underwear)");
//			SlotList.Add(info);
			info = new IDNameLoadable(0x20 ,"Hands");
			SlotList.Add(info);
//			info = new IDNameLoadable(0x40 ,"UpperLegs(Underwear)");
//			SlotList.Add(info);
//			info = new IDNameLoadable(0x80 ,"LowerLegs(Underwear)");
//			SlotList.Add(info);
			info = new IDNameLoadable(0x100 ,"Feet");
			SlotList.Add(info);
			info = new IDNameLoadable(0x200 ,"Breast Plate");
			SlotList.Add(info);
			info = new IDNameLoadable(0x400 ,"Girth");
			SlotList.Add(info);
			info = new IDNameLoadable(0x800 ,"Pauldrons");
			SlotList.Add(info);
			info = new IDNameLoadable(0x1000 ,"Bracers");
			SlotList.Add(info);
			info = new IDNameLoadable(0x2000 ,"Tassets");
			SlotList.Add(info);
			info = new IDNameLoadable(0x4000 ,"Greaves");
			SlotList.Add(info);
			info = new IDNameLoadable(0x8000 ,"Necklace");
			SlotList.Add(info);
//			info = new IDNameLoadable(0x10000 ,"Bracelet(Right)");
//			SlotList.Add(info);
//			info = new IDNameLoadable(0x20000 ,"Bracelet(Left)");
//			SlotList.Add(info);
			info = new IDNameLoadable(0x30000 ,"Bracelet");
			SlotList.Add(info);
//			info = new IDNameLoadable(0x40000 ,"Ring(Right)");
//			SlotList.Add(info);
//			info = new IDNameLoadable(0x80000 ,"Ring(Left)");
//			SlotList.Add(info);
			info = new IDNameLoadable(0xC0000 ,"Ring");
			SlotList.Add(info);
//			info = new IDNameLoadable(0x100000 ,"Melee Weapon");
//			SlotList.Add(info);
			info = new IDNameLoadable(0x200000 ,"Shield");
			SlotList.Add(info);
//			info = new IDNameLoadable(0x400000 ,"MissileWeapon");
//			SlotList.Add(info);
//			info = new IDNameLoadable(0x800000 ,"Ammunition");
//			SlotList.Add(info);
//			info = new IDNameLoadable(0x1000000 ,"Caster");
//			SlotList.Add(info);
//			info = new IDNameLoadable(0x2000000 ,"TwoHanded");
//			SlotList.Add(info);
			info = new IDNameLoadable(0x4000000 ,"Trinket");
			SlotList.Add(info);
			info = new IDNameLoadable(0x8000000 ,"Cloak");
			SlotList.Add(info);
			info = new IDNameLoadable(0x10000000 ,"Blue Aetheria");
			SlotList.Add(info);
			info = new IDNameLoadable(0x20000000 ,"Yellow Aetheria");
			SlotList.Add(info);
			info = new IDNameLoadable(0x40000000 ,"Red Aetheria");
			SlotList.Add(info);
			
			if(writelists){
				FileInfo logFile = new FileInfo(GearDir + @"\SlotList.csv");	
				if(logFile.Exists)
				{
					logFile.Delete();
				}
				StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
				StringBuilder output = new StringBuilder();
				output.Append("Index,SlotID, Name\n");
				int index = 0;
				foreach(var idname in SlotList)
				{
					output.Append(index.ToString() + "," +  idname.ID.ToString() + "," + idname.name.ToString() + "\n");
					index++;
				}
				writer0.WriteLine(output);
				writer0.Close();
			}
		}
		
		
		private void CreateArmorIndex()
		{
			string[] loadme = {"Alduressa", "Amuli", "Celdon", "Chainmail", "Chiran", "Covenant", "Diforsa", "Haebrean", "Knorr",
									"Koujia", "Leather", "Lorica", "Nariyid", "Olthoi Alduressa", "Olthoi Amuli", "Olthoi Celdon", "Olthoi",
									"Olthoi Koujia", "Over-robe", "Platemail", "Scalemail", "Sedgemail", "Studded", "Tenassa", "Yoroi", "Other"};	
			int i = 0;
			foreach(string load in loadme)
			{
				IDNameLoadable info = new IDNameLoadable(i++, load);
				ArmorIndex.Add(info);
			}
			
			if(writelists){
                FileInfo logFile = new FileInfo(GearDir + @"\ArmorIndex.csv");
				if(logFile.Exists)
				{
					logFile.Delete();
				}				
				StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
				StringBuilder output = new StringBuilder();
				output.Append("Indexed to Armor ID\n\nArmorID, Name\n");
				foreach(var idname in ArmorIndex)
				{
					output.Append(idname.ID.ToString() + "," + idname.name.ToString() + "\n");
				}
				writer0.WriteLine(output);
				writer0.Close();
			}

		}
		      
        private void CreateWeaponTypeList()
        {
            try
            {
                //IDNameLoadable info;
                //WeaponTypeList.Clear();
                //info = new IDNameLoadable(0, "None");
                //addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                //info = new IDNameLoadable(47, "Missile");
                //addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                //info = new IDNameLoadable(41, "TwoHanded");
                //addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                //info = new IDNameLoadable(44, "Heavy");
                //addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                //info = new IDNameLoadable(45, "Light");
                //addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                //info = new IDNameLoadable(46, "Finesse");
                //addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                //info = new IDNameLoadable(34, "War Magic");
                //addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                //info = new IDNameLoadable(43, "Void Magic");
                //addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                //info = new IDNameLoadable(54, "Summoning");
                //addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                
            }
            catch (Exception ex) { LogError(ex); }

            if (writelists)
            {
                FileInfo logFile = new FileInfo(GearDir + @"\WeaponTypesIndex.csv");
                if(logFile.Exists)
				{
					logFile.Delete();
				}				
                StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
                StringBuilder output = new StringBuilder();
                output.Append("ID#, WeaponType\n");
                foreach (var idname in WeaponTypeList)
                {
                    output.Append(idname.ID.ToString() + "," + idname.name.ToString() + "\n");
                }
                writer0.WriteLine(output);
                writer0.Close();
            }
        }

		private void CreateImbueList()
		{
			IDNameLoadable info;
			info = new IDNameLoadable(0x1 ,"Critical Strike");
			ImbueList.Add(info);
			info = new IDNameLoadable(0x2 ,"Cripping Blow");
			ImbueList.Add(info);
			info = new IDNameLoadable(0x4 ,"Armor Rending");
			ImbueList.Add(info);
			info = new IDNameLoadable(0x8, "Slash Rending");
			ImbueList.Add(info);
			info = new IDNameLoadable(0x10, "Pierce Rending");
			ImbueList.Add(info);
			info = new IDNameLoadable(0x20, "Bludgeon Rending");
			ImbueList.Add(info);
			info = new IDNameLoadable(0x40, "Acid Rending");
			ImbueList.Add(info);
			info = new IDNameLoadable(0x80, "Cold Rending");
			ImbueList.Add(info);
			info = new IDNameLoadable(0x100, "Light Rending");
			ImbueList.Add(info);
			info = new IDNameLoadable(0x200, "Fire Rending");
			ImbueList.Add(info);
			info = new IDNameLoadable(0x400, "Melee Defense Imbued");
			ImbueList.Add(info);
			info = new IDNameLoadable(0x800, "Missile Defense Imbued");
			ImbueList.Add(info);
			info = new IDNameLoadable(0x1000, "Magic Defense Imbued");
			ImbueList.Add(info);
			info = new IDNameLoadable(0x2000, "Hematited");
			ImbueList.Add(info);
			info = new IDNameLoadable(0x20000000,"Magic Absorb Imbued");
			ImbueList.Add(info);
			
			if(writelists){
                FileInfo logFile = new FileInfo(GearDir + @"\ImbueList.csv");	
				if(logFile.Exists)
				{
					logFile.Delete();
				}
			StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
			StringBuilder output = new StringBuilder();
			output.Append("Index, ID#, ImbueName\n");
			int index =0;
			foreach(var idname in ImbueList)
			{
				output.Append(index.ToString() + "," + idname.ID.ToString() + "," + idname.name.ToString() + "\n");
				index++;
			}
			writer0.WriteLine(output);
			writer0.Close();
			}
			
		}
				
		private void CreateSetsIndex()
		{
			string[] loadme = {"Unknown Set 0", "Unknown Set 1", "Unknown Set 2", "Unknown Set 3", "Unknown Set 4", "Noble Relic Set", "Ancient Relic Set",
								"Relic Alduressa Set", "Shou-jen Set", "Empyrean Rings Set", "Arm, Mind, Heart Set", "Coat of the Perfect Light Set",
								"Leggings of Perfect Light Set", "Soldier's Set", "Adept's Set", "Archer's Set", "Defender's Set", "Tinker's Set", "Crafter's Set",
								"Hearty Set", "Dexterous Set", "Wise Set", "Swift Set", "Hardenend Set", "Reinforced Set", "Interlocking Set", "Flame Proof Set",
								"Acid Proof Set", "Cold Proof Set", "Lightning Proof Set", "Dedication Set", "Gladiatorial Clothing Set", "Protective Clothing Set",
								"Unknown Set 33", "Unknown Set 34", "Sigil of Defense", "Sigil of Destruction", "Sigil of Fury", "Sigil of Growth", "Sigil of Vigor",
								"Heroic Protector Set", "Heroic Destroyer Set", "Unknown Set 42", "Unknown Set 43", "Unknown Set 44", "Unknown Set 45", "Unknown Set 46",
								"Unknown Set 47", "Unknown Set 48", "Weave of Alchemy", "Weave of Arcane Lore", "Weave of Armor Tinkering", "Weave of Assess Person",
								"Weave of Light Weapons", "Weave of Missile Weapons", "Weave of Cooking", "Weave of Creature Enchantment", "Weave of Missile Weapons",
								"Weave of Finesee Weapons", "Weave of Deception", "Weave of Fletching", "Weave of Healing", "Weave of Item Enchantment",
								"Weave of Item Tinkering", "Weave of Leadership", "Weave of Life Magic", "Weave of Loyalty", "Weave of Light Weapons", "Weave of Magic Defense",
								"Weave of Magic Item Tinkering", "Weave of Mana Conversion", "Weave of Melee Defense", "Weave of Missile Defense", "Weave of Salvaging",
								"Weave of Light Weapons", "Weave of Light Weapons", "Weave of Heavy Weapons", "Weave of Missile Weapons", "Weave of Two Handed Combat",
								"Weave of Light Weapons", "Weave of Void Magic", "Weave of War Magic", "Weave of Weapon Tinkering", "Weave of Assess Creature",
								"Weave of Dirty Fighting", "Weave of Dual Wield", "Weave of Recklessness", "Weave of Shield", "Weave of Sneak Attack", 
								"Unknown Set 89","Weave of Summoning"
			};
			
			int i = 0;
			foreach(string load in loadme)
			{
				IDNameLoadable info = new IDNameLoadable(i++, load);
				SetsIndex.Add(info);
               
			}
			
			if(writelists){
                FileInfo logFile = new FileInfo(GearDir + @"\SetsIndex.csv");	
							if(logFile.Exists)
				{
					logFile.Delete();
				}
			StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
			StringBuilder output = new StringBuilder();
			output.Append("Indexed to SetID\n\nSet ID, Name\n");
			foreach(var idname in SetsIndex)
			{
				output.Append(idname.ID.ToString() + "," + idname.name.ToString() + "\n");
			}
			writer0.WriteLine(output);
			writer0.Close();
			}
		}
		
		private void CreateArmorSetsList()
		{
			var armors = from sets in SetsIndex
				where !sets.name.Contains("Unknown") && !sets.name.Contains("Sigil") && !sets.name.Contains("Relic") &&
				!sets.name.Contains("Shou-jen") && !sets.name.Contains("Arm") && !sets.name.Contains("Perfect Light")
				orderby sets.name
				select sets;
			
			foreach(var armorsets in armors)
			{
				ArmorSetsList.Add(armorsets);
			}
			
			if (writelists)
            {
                FileInfo logFile = new FileInfo(GearDir + @"\ArmorSetsList.csv");
                if (logFile.Exists)
                {
                    logFile.Delete();
                }
                StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
                StringBuilder output = new StringBuilder();
                output.Append("\nSet ID, Name\n");
                foreach (var idname in ArmorSetsList)
                {
                    output.Append(idname.ID.ToString() + "," + idname.name.ToString() + "\n");
                }
                writer0.WriteLine(output);
                writer0.Close();
            }
			
		}

		private void CreateSpeciesIndex()
		{
			IDName info; 						
			for(int i = 0; i < fileservice.SpeciesTable.Length; i++)
			{
				info = new IDName();
				info.ID = i;		
				try
				{
					info.name = (fileservice.SpeciesTable.GetById(i)).Name;
				}
				catch
				{
					info.name = "NoSpecies";
				}
				SpeciesIndex.Add(info);
			}
			if(writelists){
                FileInfo logFile = new FileInfo(GearDir + @"\SpeciesIndex.csv");	
							if(logFile.Exists)
				{
					logFile.Delete();
				}
			StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
			StringBuilder output = new StringBuilder();
			output.Append("Indexed to Species ID\n\nSpeciesID, Name\n");
			foreach(IDName idname in SpeciesIndex)
			{
				output.Append(idname.ID.ToString() + "," + idname.name.ToString() + "\n");
			}
			writer0.WriteLine(output);
			writer0.Close();
			}
		}
		
		private void CreateMaterialIndex()
		{
			IDName info; 						
			for(int i = 0; i < fileservice.MaterialTable.Length; i++)
			{
				info = new IDName();
				info.ID = i;		
				try
				{
					info.name = (fileservice.MaterialTable.GetById(i)).Name;
				}
				catch
				{
					info.name = "NoMaterial";
				}
				MaterialIndex.Add(info);
			}
			
			if(writelists){
                FileInfo logFile = new FileInfo(GearDir + @"\MaterialIndex.csv");	
							if(logFile.Exists)
				{
					logFile.Delete();
				}
			StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
			StringBuilder output = new StringBuilder();
			output.Append("Indexed to MatID\n\nMaterialID, Name\n");
			foreach(IDName idname in MaterialIndex)
			{
				output.Append(idname.ID.ToString() + "," + idname.name.ToString() + "\n");
			}
			writer0.WriteLine(output);
			writer0.Close();
			}
		}

		
		private void CreateAttribIndex()
		{	
			for(int i = 0; i < fileservice.AttributeTable.Length; i++)
			{
				IDName info = new IDName();
				info.ID = i;
				try
				{
					info.name = (fileservice.AttributeTable.GetById(i)).Name;
				}
				catch
				{
					info.name = "NoAttrib";
				}
				AttribIndex.Add(info);
			}
			
			if(writelists){
                FileInfo logFile = new FileInfo(GearDir + @"\AttribIndex.csv");	
							if(logFile.Exists)
				{
					logFile.Delete();
				}
			StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
			StringBuilder output = new StringBuilder();
			output.Append("Indexed to AttribID\n\nAttribID, Name\n");
			foreach(var info in AttribIndex)
			{
				output.Append(info.ID.ToString() + "," + info.name.ToString() + "\n");
			}
			writer0.WriteLine(output);
			writer0.Close();
			}
			
			
		}
		
		private void CreateSkillIndex()
		{
			int loopcounter = 0;
			for(int i = 0; i < fileservice.SkillTable.Length; i++)
			{
				if(fileservice.SkillTable[i].Id > loopcounter) {loopcounter = fileservice.SkillTable[i].Id;}
			}
			
			for(int i = 0; i <= loopcounter; i++)
			{
				IDName info = new IDName();
				info.ID = i;
				try
				{
					info.name = (fileservice.SkillTable.GetById(i)).Name;
				}
				catch
				{
					info.name = "NoSkill";
				}
				SkillIndex.Add(info);
			}
			
			if(writelists){
                FileInfo logFile = new FileInfo(GearDir + @"\SkillIndex.csv");	
							if(logFile.Exists)
				{
					logFile.Delete();
				}
			StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
			StringBuilder output = new StringBuilder();
			output.Append("Indexed to SkillID\n\nSkillID, Name\n");
			foreach(var info in SkillIndex)
			{
				output.Append(info.ID.ToString() + "," + info.name.ToString() + "\n");
			}
			writer0.WriteLine(output);
			writer0.Close();
			}
			
		}	
		
        //private void CreateMasteryIndex()
        //{
        //    string[] loadme = {"None", "Unarmed", "Sword", "Axe", "Mace", "Spear", "Dagger", "Staff", "Bow", "Crossbow", "Thrown", "TwoHanded", "Naturalist", "Primalist", "Necromancer"};
        //    //Nat 11, Prim 12, Necro 13
        //    int i = 0;
        //    foreach(string load in loadme)
        //    {
        //        IDNameLoadable info = new IDNameLoadable(i++, load);
        //        MasteryIndex.Add(info);
        //        cboMasteryType.AddItem(info.name,i);
        //    }	
			
        //    if(writelists){
        //        FileInfo logFile = new FileInfo(GearDir + @"\MasteryIndex.csv");	
        //                    if(logFile.Exists)
        //        {
        //            logFile.Delete();
        //        }
        //    StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
        //    StringBuilder output = new StringBuilder();
        //    output.Append("Indexed to Mast.ID\n\nMasteryID, Name\n");
        //    foreach(var info in MasteryIndex)
        //    {
        //        output.Append(info.ID.ToString() + "," + info.name.ToString() + "\n");
        //    }
        //    writer0.WriteLine(output);
        //    writer0.Close();
        //    }
			
        //}

		
		private void CreateElementalList()
		{
			IDNameLoadable info;
			//Again...Flags suck
			
			info = new IDNameLoadable(0, "None");
			ElementalList.Add(info);
  			info = new IDNameLoadable(1, "Slashing");
			ElementalList.Add(info);
            info = new IDNameLoadable(2, "Piercing");
			ElementalList.Add(info);
			info = new IDNameLoadable(4, "Bludgeoning");
			ElementalList.Add(info);
			info = new IDNameLoadable(8, "Cold");
			ElementalList.Add(info);
 			info = new IDNameLoadable(16, "Fire");
			ElementalList.Add(info);
			info = new IDNameLoadable(32, "Acid");
			ElementalList.Add(info);
 			info = new IDNameLoadable(64, "Electric");
			ElementalList.Add(info);
 			info = new IDNameLoadable(1024, "Void");
			ElementalList.Add(info);
  		
			if(writelists){
                FileInfo logFile = new FileInfo(GearDir + @"\ElementalList.csv");	
							if(logFile.Exists)
				{
					logFile.Delete();
				}
			StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
			StringBuilder output = new StringBuilder();
			output.Append("Index, ID#, Damagetype\n");
			int index = 0;
			foreach(var idname in ElementalList)
			{
				output.Append(index.ToString() + "," + idname.ID.ToString() + "," + idname.name.ToString() + "\n");
			}
			writer0.WriteLine(output);
			writer0.Close();
			}
		}
		


		private void CreateHeritageIndex()
		{
			
		for(int i = 0; i < fileservice.HeritageTable.Length; i++)
			{
				IDName info = new IDName();
				info.ID = i;				
				try
				{
					info.name = (fileservice.HeritageTable.GetById(i)).Name;
				}
				catch
				{
					info.name = "NoHeritage";
				}
				HeritageIndex.Add(info);
			}
			if(writelists){
                FileInfo logFile = new FileInfo(GearDir + @"\HeritageIndex.csv");	
							if(logFile.Exists)
				{
					logFile.Delete();
				}
			StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
			StringBuilder output = new StringBuilder();
			output.Append("Indexed to HeritageID\n\nHeritageID, Name\n");
			foreach(var info in HeritageIndex)
			{
				output.Append(info.ID.ToString() + "," + info.name.ToString() + "\n");
			}
			writer0.WriteLine(output);
			writer0.Close();
			}
		}
		
		private void CreateSpellIndex()
		{
			// Irq:  Some spells are null in spellfilter, this exception puts spacers in the list when a NULL spell is returned
            //for (int spellid = 0; spellid <= FileService.SpellTable.Length; spellid++)
             for (int spellid = 0; spellid < 7000; spellid++)
            {
                spellinfo tsinfo = new spellinfo();
                try
                {
	                 Spell tspell = fileservice.SpellTable.GetById(spellid);
	
	                 tsinfo.spellid = spellid;
	                 tsinfo.spellname = tspell.Name;
	                 tsinfo.spellschool = tspell.School.Name;
	                 tsinfo.irresistable = tspell.IsIrresistible;
	                 tsinfo.isdebuff = tspell.IsDebuff;
	                 tsinfo.isoffensive = tspell.IsOffensive;
	                 tsinfo.spellicon = tspell.IconId;
	                 tsinfo.componentIds = tspell.ComponentIDs;
	                 tsinfo.isuntargeted = tspell.IsUntargetted;
	                 tsinfo.isfellowship = tspell.IsFellowship;
	                 tsinfo.duration = tspell.Duration;
	                 tsinfo.animation = tspell.TargetEffect;
	                 	
	                 switch (tspell.ComponentIDs[0])
	                 {
	                     case 112:  //Platnium Scarab
	                         tsinfo.spelllevel = 7;
	                         break;
	                     case 192:  //Dark Scarab 
	                         tsinfo.spelllevel = 0;
	                         break;
	                     case 193:  //Mana Scarab
	                         tsinfo.spelllevel = 8;
	                         break;
	                     default:
	                         tsinfo.spelllevel = tspell.ComponentIDs[0];
	                         //Minor/Moderate/Major/Epic are somewhat random in assignment of scarab.
	                         if (tsinfo.spellname.Contains("Minor")) { tsinfo.spelllevel = 11; }
	                         //if (tsinfo.spellname.Contains("Moderate")) { tsinfo.spelllevel = 12; }
	                         if (tsinfo.spellname.Contains("Major")) { tsinfo.spelllevel = 13; }
	                         if (tsinfo.spellname.Contains("Epic")) { tsinfo.spelllevel = 14; }
	                         if (tsinfo.spellname.Contains("Legendary")) { tsinfo.spelllevel = 15; }
                             break;
                 	}

                	SpellIndex.Add(tsinfo);
               	}
                catch
                {
                    spellinfo tspellcatch = new spellinfo();
                    tspellcatch.spellid = spellid;
                    tspellcatch.spellname = spellid.ToString() + "Spell";
                    SpellIndex.Add(tspellcatch);

                }

            }
            if (writelists)
            {
                FileInfo logFile = new FileInfo(GearDir + @"\SpellIndex.csv");
	             if (logFile.Exists)
	             {
	                 logFile.Delete();
	             }
	             StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
	             StringBuilder output = new StringBuilder();
	             output.Append("Indexed to SpellID\n\nSpell ID,Spell Name,Spell Level,Spell Icon,Spell School,IrResistable,IsDebuff,IsOffensive,IsUntargeted\n");
	             foreach (spellinfo si in SpellIndex)
	             {
	                 output.Append(si.spellid.ToString() + "," + si.spellname.ToString() + "," + si.spelllevel.ToString() + "," +
	                               si.spellicon.ToString() + "," + si.spellschool.ToString() + "," + si.irresistable.ToString() + "," +
	                               si.isdebuff.ToString() + "," + si.isoffensive.ToString() + si.isuntargeted.ToString() + "\n");
	             }
	             writer0.WriteLine(output);
	             writer0.Close();
            }
        }

		 private void CreateItemSpellsList()
         {
         	try
         	{
         		var spl1 = from spels in SpellIndex
         			where spels.isdebuff == false && spels.isoffensive == false && spels.irresistable == true && ((spels.isuntargeted == true) || (spels.spelllevel > 10)) &&
         				  spels.isfellowship == false
         				   select spels;
         		
         	    var spl2 = from spels1 in spl1
         	    			where (spels1.spellid != 6) && (spels1.spellid < 1157 || spels1.spellid > 1301) &&
								  (spels1.spellid < 1644 || spels1.spellid > 1708) && (spels1.spellid < 1997 || spels1.spellid > 2051) && 
         	    				  (spels1.spellid < 2332 || spels1.spellid > 2410) && (spels1.spellid < 2628 || spels1.spellid > 3050) &&
         	    				  (spels1.spellid < 3204 || spels1.spellid > 3249) && (spels1.spellid < 3320 || spels1.spellid > 3478) &&
         	    				  (spels1.spellid < 3530 || spels1.spellid > 3759) && (spels1.spellid != 3810) &&
         	    				  (spels1.spellid < 3848 || spels1.spellid > 3913) && (spels1.spellid < 3977 || spels1.spellid > 3983) &&
								  (spels1.spellid < 4022 || spels1.spellid > 4221) && (spels1.spellid < 4240 || spels1.spellid > 4245) &&         	    	
          	    				  (spels1.spellid < 4647 || spels1.spellid > 4658) && (spels1.spellid < 4754 || spels1.spellid > 4759) &&
								  (spels1.spellid < 4981 || spels1.spellid > 5026) && (spels1.spellid < 5137 || spels1.spellid > 5154) &&
         	    				  (spels1.spellid < 5184 || spels1.spellid > 5329) && (spels1.spellid < 5435 || spels1.spellid > 5779) &&
         	    				  (spels1.spellid < 5898 || spels1.spellid > 6038) && (spels1.spellid < 6108 || spels1.spellid > 6120) &&
         	    	              spels1.spellid < 6127  
         	    			select spels1;
         	    
         	    var cspells = from spl in SpellIndex
               				where spl.spellname.Contains("Shroud of") || spl.spellname.Contains("Cloaked") || spl.spellname == "Horizon's Blades" ||
            				spl.spellname == "Tectonic Rifts" || spl.spellname == "Nuhmudira's Spines" ||
            				spl.spellname == "Searing Disc" || spl.spellname == "Cassius' Ring of Fire" || spl.spellname == "Halo of Frost" ||
            				spl.spellname == "Eye of the Storm" || spl.spellid == 5361 //Clouded Soul is 5331 and 5361.  5331 has a 700 cast level and very high damage compared to cloak damage proc.
            				orderby spl.spellname ascending
							select spl;
         	    
         	    
         	    foreach(var spel in spl2)
         	    {
         	    	if(!spel.spellname.Contains("Bane") && !spel.spellname.Contains("Hunter") && !spel.spellname.Contains("Thirst") && !spel.spellname.Contains("Link") &&
         	    	   !spel.spellname.Contains("Defender"))
         	    	{
         	    		ItemsSpellList.Add(spel);
         	    	}
         	    }
         	    
         	    foreach(var selectedspell in cspells)
            	{
         	    	selectedspell.spelllevel = 20;
            		ItemsSpellList.Add(selectedspell);
            	}
            	
            	spellinfo damageabsorb = new spellinfo();
            	damageabsorb.spellname = "Damage Absorb";
            	damageabsorb.spelllevel = 20;
            	damageabsorb.spellid = 10000;
            	
            	ItemsSpellList.Add(damageabsorb);
         	   
         	    
         	 if(writelists)
         	 {
                 FileInfo logFile = new FileInfo(GearDir + @"\ItemSpells.csv");
	             
		         if (logFile.Exists)
	             {
	                 logFile.Delete();
	             }
	             
		         StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
	             StringBuilder output = new StringBuilder();
	             
	             output.Append("Indexed to SpellID\n\nSpell ID,Spell Name,Spell Level,Spell Icon,Spell School,IsResistable,IsDebuff,IsOffensive,\n");
	             foreach (var si in spl2)
	             {
	                 output.Append(si.spellid.ToString() + "," + si.spellname.ToString() + "," + si.spelllevel.ToString() + "," +
	                               si.spellicon.ToString() + "," + si.spellschool.ToString() + "," + si.irresistable.ToString() + "," +
	                               si.isdebuff.ToString() + "," + si.isoffensive.ToString() + "," + si.isuntargeted.ToString() + "\n");
	             }
	             writer0.WriteLine(output);
	             writer0.Close();
         	 }
         	}
         	catch{}
         }


        private void CreateClassInvList()
        {
            ClassInvList = new List<IDNameLoadable>();
            string[] loadme = {"None","Armor","Clothing","Jewelry","WandStaffOrb","MeleeWeapon", "MissileWeapon",
                                  "Salvage","Scroll","HealingKit","Food" ,"Gem", "CraftedFletching", "BaseCooking",
                                  "CraftedAlchemy", "SpellComponent", "Key", "Misc", "ManaStone", "BooksPaper","TradeNote"};


            int i = 0;
            foreach (string load in loadme)
            {
                IDNameLoadable info = new IDNameLoadable(i++, load);
                ClassInvList.Add(info);
            }
            if (writelists) { doWriteLists(ClassInvList); }

        }

        private void CreateMeleeTypeInvList()
        {
            try
            {
                IDNameLoadable info;
                MeleeTypeInvList = new List<IDNameLoadable>();

                info = new IDNameLoadable(0, "None");
                MeleeTypeInvList.Add(info);
               info = new IDNameLoadable(41, "TwoHanded");
               MeleeTypeInvList.Add(info);
               info = new IDNameLoadable(44, "Heavy");
               MeleeTypeInvList.Add(info);
               info = new IDNameLoadable(45, "Light");
               MeleeTypeInvList.Add(info);
               info = new IDNameLoadable(46, "Finesse");
               MeleeTypeInvList.Add(info);
            }
            catch (Exception ex) { LogError(ex); }
            if (writelists) { doWriteLists(MeleeTypeInvList); }

        }


        private void CreateArmorSetsInvList()
        {
            ArmorSetsInvList = new List<IDNameLoadable>();
            IDNameLoadable info = new IDNameLoadable(0,"None");
            ArmorSetsInvList.Add(info);
           var armors = from sets in SetsIndex
                         where !sets.name.Contains("Weave") && !sets.name.Contains("Unknown") && !sets.name.Contains("Sigil")
                            orderby sets.name
                         select sets;
            foreach (var armorsets in armors)
            {
                ArmorSetsInvList.Add(armorsets);
            }
            var armors2 = from sets2 in SetsIndex
                         where sets2.name.Contains("Weave") 
                         orderby sets2.name
                         select sets2;
            foreach (var armorsets2 in armors2)
            {
                ArmorSetsInvList.Add(armorsets2);
           }


        }



        private void CreateMaterialInvList()
        {
            MaterialInvList = new List<IDNameLoadable>();
            var materials = from mat in MaterialIndex
                           orderby mat.name
                           select mat;
            IDNameLoadable info = new IDNameLoadable(0, "None");
            MaterialInvList.Add(info);
            foreach (var material in materials)
            {
                info = new IDNameLoadable(material.ID, material.name);
                MaterialInvList.Add(info);
            }
            if (writelists) { doWriteLists(MaterialInvList); }


         }

        private void CreateElementalInvList()
        {
            ElementalInvList = new List<IDNameLoadable>();
            IDNameLoadable info;
            //Again...Flags suck

            info = new IDNameLoadable(0, "None");
            ElementalInvList.Add(info);
            info = new IDNameLoadable(1, "Slashing");
            ElementalInvList.Add(info);
            info = new IDNameLoadable(2, "Piercing");
            ElementalInvList.Add(info);
            info = new IDNameLoadable(3, "Slashing:Piercing");
            ElementalInvList.Add(info);
            info = new IDNameLoadable(4, "Bludgeoning");
            ElementalInvList.Add(info);
            info = new IDNameLoadable(8, "Cold");
            ElementalInvList.Add(info);
            info = new IDNameLoadable(16, "Fire");
            ElementalInvList.Add(info);
            info = new IDNameLoadable(32, "Acid");
            ElementalInvList.Add(info);
            info = new IDNameLoadable(64, "Electric");
            ElementalInvList.Add(info);
            info = new IDNameLoadable(1024, "Void");
            ElementalInvList.Add(info);
            if (writelists) { doWriteLists(ElementalInvList); }

        }


        private void CreateArmorLevelInvList()
        {
            try
            {
                ArmorLevelInvList = new List<IDNameLoadable>();
                IDNameLoadable info;
                info = new IDNameLoadable(0, "None");
                ArmorLevelInvList.Add(info);
               info = new IDNameLoadable(1, "No Wield");
               ArmorLevelInvList.Add(info);
               info = new IDNameLoadable(2, "60");
               ArmorLevelInvList.Add(info);
               info = new IDNameLoadable(3, "90");
               ArmorLevelInvList.Add(info);
               info = new IDNameLoadable(4, "100");
               ArmorLevelInvList.Add(info);
               info = new IDNameLoadable(5, "150");
               ArmorLevelInvList.Add(info);
               info = new IDNameLoadable(6, "180");
               ArmorLevelInvList.Add(info);
               info = new IDNameLoadable(7, "225");
               ArmorLevelInvList.Add(info);
  
            }
            catch (Exception ex) { LogError(ex); }
            if (writelists) { doWriteLists(ArmorLevelInvList); }

        }

        private void CreateSalvageWorkInvList()
        {
            try
            {
                SalvageWorkInvList = new List<IDNameLoadable>();
               IDNameLoadable info;
                info = new IDNameLoadable(0, "None");
                SalvageWorkInvList.Add(info);
                info = new IDNameLoadable(1, "1-6");
                SalvageWorkInvList.Add(info);
                info = new IDNameLoadable(2, "7-8");
                SalvageWorkInvList.Add(info);
                info = new IDNameLoadable(3, "9");
                SalvageWorkInvList.Add(info);
                info = new IDNameLoadable(4, "10");
                SalvageWorkInvList.Add(info);
            }
            catch (Exception ex) { LogError(ex); }
            if (writelists) { doWriteLists(SalvageWorkInvList); }

        }

        private void CreateCoverageInvList()
        {
            try
            {
                CoverageInvList = new List<IDNameLoadable>();
                IDNameLoadable info;
                info = new IDNameLoadable(0, "None");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(256, "Upper_Leg");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(512, "Lower_Leg");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(1024, "Chest");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(2048, "Abdomen");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(4096, "Upper_Arm");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(8192, "Lower_Arm");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(16384, "Head");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(32768, "Hands");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(65536, "Feet");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(768, "Upper_Lower_Legs");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(2304, "Abdomen_Upper_Legs");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(2816, "Abdomen_Upper_Lower_Legs");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(3072, "Chest_Abdomen");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(5120, "Chest_Upper_Arms");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(7168, "Chest_Abdomen_Upper_Arm");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(13312, "Chest_Upper_Lower_Arms");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(15360, "Chest_Abdomen_Upper_Lower_Arms");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(8, "Underwear_Chest");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(19, "Underwear_Abdomen_Upper_Legs");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(22, "Underwear_Abdomen_Upper_Lower_Legs");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(40, "Underwear_Chest_UpperArm");
                CoverageInvList.Add(info);
                info = new IDNameLoadable(131072, "Underwear_Chest_Upper_LowerArms");
                CoverageInvList.Add(info);
                }
            catch (Exception ex) { LogError(ex); }
            if (writelists) { doWriteLists(CoverageInvList); }

        }

        private void CreateEmbueInvList()
        {
            try
            {
                EmbueInvList = new List<IDNameLoadable>();
                IDNameLoadable info;
                info = new IDNameLoadable(0, "None");
                EmbueInvList.Add(info);
                info = new IDNameLoadable(1, "Critical_Strike");
                EmbueInvList.Add(info);
                info = new IDNameLoadable(2, "Crippling_Blow");
                EmbueInvList.Add(info);
                info = new IDNameLoadable(4, "Armor");
                EmbueInvList.Add(info);
                info = new IDNameLoadable(8, "Slashing");
                EmbueInvList.Add(info);
                info = new IDNameLoadable(16, "Piercing");
                EmbueInvList.Add(info);
                info = new IDNameLoadable(32, "Bludgeon");
                EmbueInvList.Add(info);
                info = new IDNameLoadable(64, "Acid");
                EmbueInvList.Add(info);
                info = new IDNameLoadable(128, "Frost");
                EmbueInvList.Add(info);
                info = new IDNameLoadable(256, "Lightning");
                EmbueInvList.Add(info);
                info = new IDNameLoadable(512, "Fire");
                EmbueInvList.Add(info);
                info = new IDNameLoadable(1000, "MagicD_PlusOne");
                EmbueInvList.Add(info);
 

            }
            catch (Exception ex) { LogError(ex); }
            if (writelists) { doWriteLists(EmbueInvList); }

        }


        private void CreateWeaponWieldInvList()
        {
            try
            {
             WeaponWieldInvList = new List<IDNameLoadable>();
            string[] loadme = {"None","No Wield","250","270","290","300", "310",
                                  "315","325","330","335" ,"350", "355", "360",
                                  "370", "375", "385", "400", "420"};


            int i = 0;
            foreach (string load in loadme)
            {
                IDNameLoadable info = new IDNameLoadable(i++, load);
                WeaponWieldInvList.Add(info);
            }
            if (writelists) { doWriteLists(WeaponWieldInvList); }

            }
            catch (Exception ex) { LogError(ex); }
            if (writelists) { doWriteLists(WeaponWieldInvList); }

        }

        //may have guessed wrong here ? if should be info.name
 
        private void addInfo(IDNameLoadable info, List<IDNameLoadable> lst, HudCombo cmb)
        {
            lst.Add(info);
            cmb.AddItem(cmb, info.name );
        }

        private void doWriteLists(List<IDNameLoadable> lst)
                     	
          {
              FileInfo logFile = new FileInfo(GearDir + @"\lst.csv");
                if (logFile.Exists)
                {
                    logFile.Delete();
                }
                StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
                StringBuilder output = new StringBuilder();
                output.Append("Indexed to SetID\n\nSet ID, Name\n");
                foreach (var idname in lst)
                {
                    output.Append(idname.ID.ToString() + "," + idname.name.ToString() + "\n");
                }
                writer0.WriteLine(output);
                writer0.Close();

       	 }
        
        
        private void FillLootObjectLists()
        {
        	
//        	private class IntDoubleLoadable
//			{
//				public int ID;
//				public double Val;
//				public int Ele;
//				public int Series;
//				
//				public IntDoubleLoadable(int i, double j, int elemental, int stack)
//				{
//					ID =  i;
//					Val = j;
//					Ele = elemental;
//					Series = stack;				
//				}
//			}

									
								


        	
        	ImpenList.Add(new IntDoubleLoadable(4407, 12, 0, 0));  //Impen 8a
        	ImpenList.Add(new IntDoubleLoadable(3908, 12, 0, 0));  //Impen 8b
        	ImpenList.Add(new IntDoubleLoadable(2108, 11, 0, 0));  //Impen 7
        	ImpenList.Add(new IntDoubleLoadable(1486, 10, 0, 0));  //Impen 6
        	ImpenList.Add(new IntDoubleLoadable(1485, 7.5, 0, 0)); //Impen 5
        	ImpenList.Add(new IntDoubleLoadable(1484, 5, 0, 0));   //Impen 4
        	ImpenList.Add(new IntDoubleLoadable(1483, 3.75, 0, 0));//Impen 3
        	ImpenList.Add(new IntDoubleLoadable(1482, 2.5, 0, 0)); //Impen 2
        	ImpenList.Add(new IntDoubleLoadable(51, 1, 0, 0));  //Impen 1
        	
        	//slash 1
        	BaneList.Add(new IntDoubleLoadable(4293, 2, 1, 0));
        	BaneList.Add(new IntDoubleLoadable(2094, 1.7, 1, 0));
        	BaneList.Add(new IntDoubleLoadable(1562, 1.5, 1, 0));
        	BaneList.Add(new IntDoubleLoadable(1561, 1.0, 1, 0));
        	BaneList.Add(new IntDoubleLoadable(1560, 0.75, 1, 0));
        	BaneList.Add(new IntDoubleLoadable(1559, 0.5, 1, 0));
        	BaneList.Add(new IntDoubleLoadable(1568, 0.25, 1, 0));
        	BaneList.Add(new IntDoubleLoadable(37, 0.1, 1, 0));
        	
        	//Pierce 2
        	BaneList.Add(new IntDoubleLoadable(4212, 2, 2, 0));
        	BaneList.Add(new IntDoubleLoadable(2113, 1.7, 2, 0));
        	BaneList.Add(new IntDoubleLoadable(1574, 1.5, 2, 0));
        	BaneList.Add(new IntDoubleLoadable(1573, 1.0, 2, 0));
        	BaneList.Add(new IntDoubleLoadable(1572, 0.75, 2, 0));
        	BaneList.Add(new IntDoubleLoadable(1571, 0.5, 2, 0));
        	BaneList.Add(new IntDoubleLoadable(1570, 0.25, 2, 0));
        	BaneList.Add(new IntDoubleLoadable(1569, 0.1, 2, 0));
        	
        	//Bludgeon  Modifiers:  4
        	BaneList.Add(new IntDoubleLoadable(4397, 2, 4, 0));
        	BaneList.Add(new IntDoubleLoadable(2098, 1.7, 4, 0));
        	BaneList.Add(new IntDoubleLoadable(1516, 1.5, 4, 0));
        	BaneList.Add(new IntDoubleLoadable(1515, 1.0, 4, 0));
        	BaneList.Add(new IntDoubleLoadable(1514, 0.75, 4, 0));
        	BaneList.Add(new IntDoubleLoadable(1513, 0.5, 4, 0));
        	BaneList.Add(new IntDoubleLoadable(1512, 0.25, 4, 0));
        	BaneList.Add(new IntDoubleLoadable(1511, 0.1, 4, 0));
        	
        	//Acid Modifiers:  32
        	
        	BaneList.Add(new IntDoubleLoadable(4391, 2, 32, 0));
        	BaneList.Add(new IntDoubleLoadable(2092, 1.7, 32, 0));
        	BaneList.Add(new IntDoubleLoadable(1498, 1.5, 32, 0));
        	BaneList.Add(new IntDoubleLoadable(1497, 1.0, 32, 0));
        	BaneList.Add(new IntDoubleLoadable(1496, 0.75, 32, 0));
        	BaneList.Add(new IntDoubleLoadable(1495, 0.5, 32, 0));
        	BaneList.Add(new IntDoubleLoadable(1494, 0.25, 32, 0));
        	BaneList.Add(new IntDoubleLoadable(1493, 0.1, 32, 0));
        	
        	//Fire Modifiers: 16
        	BaneList.Add(new IntDoubleLoadable(4401, 2, 16, 0));
        	BaneList.Add(new IntDoubleLoadable(2102, 1.7, 16, 0));
        	BaneList.Add(new IntDoubleLoadable(1552, 1.5, 16, 0));
        	BaneList.Add(new IntDoubleLoadable(1551, 1.0, 16, 0));
        	BaneList.Add(new IntDoubleLoadable(1550, 0.75, 16, 0));
        	BaneList.Add(new IntDoubleLoadable(1549, 0.5, 16, 0));
        	BaneList.Add(new IntDoubleLoadable(1548, 0.25, 16, 0));
        	BaneList.Add(new IntDoubleLoadable(1547, 0.1, 16, 0));
        	
        	//Cold Modifiers:  8
        	BaneList.Add(new IntDoubleLoadable(4403, 2, 8, 0));
        	BaneList.Add(new IntDoubleLoadable(2104, 1.7, 8, 0));
        	BaneList.Add(new IntDoubleLoadable(1528, 1.5, 8, 0));
        	BaneList.Add(new IntDoubleLoadable(1527, 1.0, 8, 0));
        	BaneList.Add(new IntDoubleLoadable(1526, 0.75, 8, 0));
        	BaneList.Add(new IntDoubleLoadable(1525, 0.5, 8, 0));
        	BaneList.Add(new IntDoubleLoadable(1524, 0.25, 8, 0));
        	BaneList.Add(new IntDoubleLoadable(1523, 0.1, 8, 0));
        	
        	//Lightning Modifiers:  64
        	BaneList.Add(new IntDoubleLoadable(4409, 2, 64, 0));
        	BaneList.Add(new IntDoubleLoadable(2110, 1.7, 64, 0));
        	BaneList.Add(new IntDoubleLoadable(1540, 1.5, 64, 0));
        	BaneList.Add(new IntDoubleLoadable(1539, 1.0, 64, 0));
        	BaneList.Add(new IntDoubleLoadable(1538, 0.75, 64, 0));
        	BaneList.Add(new IntDoubleLoadable(1537, 0.5, 64, 0));
        	BaneList.Add(new IntDoubleLoadable(1536, 0.25, 64, 0));
        	BaneList.Add(new IntDoubleLoadable(1535, 0.1, 64, 0));
        	
			//Impen        	
        	ImpenCantripList.Add(new IntDoubleLoadable(6095, 4, 0, 1));  //L Impen
        	ImpenCantripList.Add(new IntDoubleLoadable(4667, 3, 0, 1));  //E Impen
        	ImpenCantripList.Add(new IntDoubleLoadable(2592, 2, 0, 1));  //J Impen
        	ImpenCantripList.Add(new IntDoubleLoadable(2604, 1, 0, 1));  //N Impen
        	
        	//Slash 1
         	BaneCantripList.Add(new IntDoubleLoadable(6097, 0.25, 1, 1));
        	BaneCantripList.Add(new IntDoubleLoadable(4669, 0.2, 1, 1));
        	BaneCantripList.Add(new IntDoubleLoadable(2594, 0.15, 1, 1));
        	BaneCantripList.Add(new IntDoubleLoadable(2606, 0.1, 1, 1));
        	
        	//Pierce 2
        	BaneCantripList.Add(new IntDoubleLoadable(6096, 0.25, 2, 1));
        	BaneCantripList.Add(new IntDoubleLoadable(4668, 0.2, 2, 1));
        	BaneCantripList.Add(new IntDoubleLoadable(2593, 0.15, 2, 1));
        	BaneCantripList.Add(new IntDoubleLoadable(2605, 0.1, 2, 1));
        	
        	//Bludgeon 4
        	BaneCantripList.Add(new IntDoubleLoadable(6090, 0.25, 4, 1));
        	BaneCantripList.Add(new IntDoubleLoadable(4662, 0.2, 4, 1));
        	BaneCantripList.Add(new IntDoubleLoadable(2587, 0.15, 4, 1));
        	BaneCantripList.Add(new IntDoubleLoadable(2599, 0.1, 4, 1));
        	
        	//Acid 32
        	BaneCantripList.Add(new IntDoubleLoadable(6088, 0.25, 32, 1));
        	BaneCantripList.Add(new IntDoubleLoadable(4660, 0.2, 32, 1));
        	BaneCantripList.Add(new IntDoubleLoadable(2585, 0.15, 32, 1));
        	BaneCantripList.Add(new IntDoubleLoadable(2597, 0.1, 32, 1));
        	
			//Fire 16
			BaneCantripList.Add(new IntDoubleLoadable(6092, 0.25, 16, 1));
			BaneCantripList.Add(new IntDoubleLoadable(4664, 0.2, 16, 1));
			BaneCantripList.Add(new IntDoubleLoadable(2589, 0.15, 16, 1));
			BaneCantripList.Add(new IntDoubleLoadable(2601, 0.1, 16, 1));
			
			//Cold 8
			BaneCantripList.Add(new IntDoubleLoadable(6093, 0.25, 8, 1));
			BaneCantripList.Add(new IntDoubleLoadable(4665, 0.2, 8, 1));
			BaneCantripList.Add(new IntDoubleLoadable(2590, 0.15, 8, 1));
			BaneCantripList.Add(new IntDoubleLoadable(2602, 0.1, 8, 1));
			
			//Lightning 64
			BaneCantripList.Add(new IntDoubleLoadable(6099, 0.25, 64, 1));
			BaneCantripList.Add(new IntDoubleLoadable(4671, 0.2, 64, 1));
			BaneCantripList.Add(new IntDoubleLoadable(2595, 0.15, 64, 1));
			BaneCantripList.Add(new IntDoubleLoadable(2607, 0.1, 64, 1));        	
        }
        
        
             private void doWriteLists(List<spellinfo> index)
         {
             FileInfo logFile = new FileInfo(GearDir + @"\" + index + ".csv");
             if (logFile.Exists)
             {
                 logFile.Delete();
             }
             StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
             StringBuilder output = new StringBuilder();
             //Karin:  I'm not going to put this back, but this isn't correct.  Indexes like SpellIndex ARE indexed to spellID.  Lists like CloakSpellList are not
             //If the .csv file output is going to be meaningful, it will have to be adjusted to be more generic and note wether you are using an index, that
             //is indexed to spellID or heritage ID or whatever or just a simple list that has no order and must be searched because the indexs are meaningless.
             output.Append("Indexed to SpellID\n\nSpell ID,Spell Name,Spell Level,Spell Icon,Spell School,IsResistable,IsDebuff,IsOffensive,\n");
             foreach (spellinfo si in index)
             {
                 output.Append(si.spellid.ToString() + "," + si.spellname.ToString() + "," + si.spelllevel.ToString() + "," +
                               si.spellicon.ToString() + "," + si.spellschool.ToString() + "," + si.irresistable.ToString() + "," +
                               si.isdebuff.ToString() + "," + si.isoffensive.ToString() + "\n");
             }
             writer0.WriteLine(output);
             writer0.Close();

         }
             

    }
}

 
    
					
	