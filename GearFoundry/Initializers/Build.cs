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


namespace GearFoundry
{

	
	public partial class PluginCore
	{
		DirectoryInfo pluginPersonalFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Decal Plugins\");
		bool writelists = false;
        
        //From: Karin.  Lists for use with XDocuments composed of XElements
        private List<XElement> mSortedMobsList = new List<XElement>();
        private List<XElement> mSortedTrophiesList = new List<XElement>();
        private List<XElement> mSortedSalvageList = new List<XElement>();
        private List<XElement> mSortedMobsListChecked = new List<XElement>();
        private List<XElement> mSortedTrophiesListChecked = new List<XElement>();
        private List<XElement> mSortedSalvageListChecked = new List<XElement>();
        private List<XElement> mPrioritizedRulesList = new List<XElement>();
        private List<XElement> mPrioritizedRulesListEnabled = new List<XElement>();
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

		
		private static List<IDNameLoadable> ElementalList = new List<IDNameLoadable>();
		private static List<IDNameLoadable> MasteryIndex = new List<IDNameLoadable>();
		private static List<IDNameLoadable> SetsIndex = new List<IDNameLoadable>();
        private static List<IDNameLoadable> CloakSetsList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> ArmorSetsList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> ImbueList = new List<IDNameLoadable>();
		private static List<IDNameLoadable> ArmorIndex = new List<IDNameLoadable>();
		private static List<IDNameLoadable> ArmorCoverageList = new List<IDNameLoadable>();
		private static List<IDNameLoadable> ObjectList = new List<IDNameLoadable>();
		private static List<IDNameLoadable> SlotList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> WeaponTypeList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> WeaponSubtypeIndex = new List<IDNameLoadable>();
        private static List<IDNameLoadable> AppliesToList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> EnabledSpellsList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> EssMasteryList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> EssElementsList = new List<IDNameLoadable>();

        //From:  Irquk - to replace old dictionary lookups
        private static List<spellinfo> SpellIndex = new List<spellinfo>();
        private static List<spellinfo> CloakSpellList = new List<spellinfo>();
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
            ClearRuleListVues();
        }

        private void ClearRuleLists()
        {
            if (mSortedMobsList != null) { mSortedMobsList.Clear(); }
            if (mSortedMobsListChecked != null) { mSortedMobsListChecked.Clear(); }
            if (mSortedTrophiesList != null) { mSortedTrophiesList.Clear(); }
            if (mSortedTrophiesListChecked != null) { mSortedTrophiesListChecked.Clear(); }
            if (mSortedSalvageList != null) { mSortedSalvageList.Clear(); }
            if (mSortedSalvageListChecked != null) { mSortedSalvageListChecked.Clear(); }
            if (mPrioritizedRulesList != null) { mPrioritizedRulesList.Clear(); }
            if (mPrioritizedRulesListEnabled != null) { mPrioritizedRulesListEnabled.Clear(); }
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
            if (CloakSetsList != null) { CloakSetsList.Clear(); }
            if (ArmorSetsList != null) { ArmorSetsList.Clear(); }
            if (ImbueList != null) { ImbueList.Clear(); }
            if (ArmorIndex != null) { ArmorIndex.Clear(); }
            if (ArmorCoverageList != null) { ArmorCoverageList.Clear(); }
            if (ObjectList != null) { ObjectList.Clear(); }
            if (SlotList != null) { SlotList.Clear(); }
            if (WeaponTypeList != null) { WeaponTypeList.Clear(); }
            if (WeaponSubtypeIndex != null) { WeaponSubtypeIndex.Clear(); }
            if (AppliesToList != null) { AppliesToList.Clear(); }
            if (EnabledSpellsList != null) { EnabledSpellsList.Clear(); }
            if (EssMasteryList != null) { EssMasteryList.Clear(); }
            if (EssElementsList != null) { EssElementsList.Clear(); }
            if (SpellIndex != null) { SpellIndex.Clear(); }
            if (CloakSpellList != null) { CloakSpellList.Clear(); }
            if (ItemsSpellList != null) { ItemsSpellList.Clear(); }
            if (FilteredSpellIndex != null) { FilteredSpellIndex.Clear(); }
            if (HeritageIndex != null) { HeritageIndex.Clear(); }
            if (SkillIndex != null) { SkillIndex.Clear(); }
            if (AttribIndex != null) { AttribIndex.Clear(); }
            if (MaterialIndex != null)  {MaterialIndex.Clear(); }
            if (SpeciesIndex != null) { SpeciesIndex.Clear(); }

        }

        private void ClearRuleListVues()
        {
            if (lstRuleApplies != null) { lstRuleApplies.Clear(); }
            if (lstDamageTypes != null) { lstDamageTypes.Clear(); }
            if (lstRuleArmorCoverages != null) { lstRuleArmorCoverages.Clear(); }
            if (lstRuleArmorTypes != null) { lstRuleArmorTypes.Clear(); }
            if (lstRuleSets != null) { lstRuleSets.Clear(); }
            if (lstRuleCloakSets != null) { lstRuleCloakSets.Clear(); }
            if (lstRuleCloakSpells != null) { lstRuleCloakSpells.Clear(); }
            if (cboRuleEssMastery != null) { cboRuleEssMastery.Clear(); }
            if (lstRuleEssElements != null) { lstRuleEssElements.Clear(); }
            if (lstRuleSpellsEnabled != null) { lstRuleSpellsEnabled.Clear(); }
            if (lstRuleSpells != null) { lstRuleSpells.Clear(); }
            if (lstRules != null) { lstRules.Clear(); }
            if (cboWeaponAppliesTo != null) { cboWeaponAppliesTo.Clear(); }
            if (cboMasteryType != null) { cboMasteryType.Clear(); }
            if (lstmyTrophies != null) { lstmyTrophies.Clear(); }
            if (lstmyMobs != null) { lstmyMobs.Clear(); }
            if (lstNotifySalvage != null) { lstNotifySalvage.Clear(); }
            if (lstNotifyOptions != null) { lstNotifyOptions.Clear(); }
            if (lstUstList != null) { lstUstList.Clear(); }

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
		}
		
		private void InitListBuilder()
		{
            CreateWeaponTypeList();
            CreateElementalList();
            CreateSpellIndex();
            CreateFilteredSpellIndex();
            CreateCloakSpellList();
			CreateHeritageIndex();
			CreateMasteryIndex();
			CreateSkillIndex();
			CreateAttribIndex();
			CreateMaterialIndex();
			CreateSpeciesIndex();
			CreateSetsIndex();
            CreateCloakSetsList();
			CreateImbueList();
			CreateArmorIndex();
			CreateSlotList();
            CreateArmorCoverageList();
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
            CreateEssMasteryList();
            CreateEssElementsList();
            populateListBoxes();
		}

        private void populateListBoxes()
        {
            populateWeaponDamageListBox();
            populateArmorCoverageListBox();
            populateArmorTypesListBox();
            populateRulesListBox();
            populateListRuleAppliesBox();
            populateSetsListBox();
            populateSpellListBox();
            populateCloakSetsListBox();
            populateCloakSpellsListBox();
            populateUstListBox();
            populateEssElementsListBox();

        }

        private void CreateAppliesToList()
        {
            try
            {


                IDNameLoadable info;

                info = new IDNameLoadable(0x1, "Melee Weapon");
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
                info = new IDNameLoadable(0x800, "Gems");
                AppliesToList.Add(info);
                info = new IDNameLoadable(0x2000, "Scrolls");
                AppliesToList.Add(info);
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
			info = new IDNameLoadable(0x2 ,"Chest(Underwear)");
			SlotList.Add(info);
			info = new IDNameLoadable(0x4 ,"Abdomen(Underwear)");
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
			info = new IDNameLoadable(0x200 ,"Chest(Armor)");
			SlotList.Add(info);
			info = new IDNameLoadable(0x400 ,"Abdomen(Armor)");
			SlotList.Add(info);
			info = new IDNameLoadable(0x800 ,"UpperArms(Armor)");
			SlotList.Add(info);
			info = new IDNameLoadable(0x1000 ,"LowerArms(Armor)");
			SlotList.Add(info);
			info = new IDNameLoadable(0x2000 ,"UpperLegs(Armor)");
			SlotList.Add(info);
			info = new IDNameLoadable(0x4000 ,"LowerLegs(Armor)");
			SlotList.Add(info);
			info = new IDNameLoadable(0x8000 ,"Necklace(Jewelry)");
			SlotList.Add(info);
//			info = new IDNameLoadable(0x10000 ,"Bracelet(Right)");
//			SlotList.Add(info);
//			info = new IDNameLoadable(0x20000 ,"Bracelet(Left)");
//			SlotList.Add(info);
			info = new IDNameLoadable(0x30000 ,"Bracelet(Jewelry)");
			SlotList.Add(info);
//			info = new IDNameLoadable(0x40000 ,"Ring(Right)");
//			SlotList.Add(info);
//			info = new IDNameLoadable(0x80000 ,"Ring(Left)");
//			SlotList.Add(info);
			info = new IDNameLoadable(0xC0000 ,"Ring(Jewelry)");
			SlotList.Add(info);
			info = new IDNameLoadable(0x100000 ,"Melee Weapon");
			SlotList.Add(info);
			info = new IDNameLoadable(0x200000 ,"Shield");
			SlotList.Add(info);
			info = new IDNameLoadable(0x400000 ,"MissileWeapon");
			SlotList.Add(info);
			info = new IDNameLoadable(0x800000 ,"Ammunition");
			SlotList.Add(info);
			info = new IDNameLoadable(0x1000000 ,"Caster");
			SlotList.Add(info);
			info = new IDNameLoadable(0x2000000 ,"TwoHanded");
			SlotList.Add(info);
			info = new IDNameLoadable(0x4000000 ,"Trinket(Jewelry)");
			SlotList.Add(info);
			info = new IDNameLoadable(0x8000000 ,"Cloak");
			SlotList.Add(info);
			info = new IDNameLoadable(0x10000000 ,"Blue Aetheria");
			SlotList.Add(info);
			info = new IDNameLoadable(0x20000000 ,"Yellow Aetheria");
			SlotList.Add(info);
			info = new IDNameLoadable(0x40000000 ,"Red Aetheria");
			
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
		
		private void CreateArmorCoverageList()
		{
			//Irq:  I have prehaps mentioned that flags suck?
			IDNameLoadable info;
			
			info = new IDNameLoadable(0x100 ,"UpperLegs");
			ArmorCoverageList.Add(info);
			info = new IDNameLoadable(0x200 ,"LowerLegs");
			ArmorCoverageList.Add(info);
			info = new IDNameLoadable(0x400 ,"Chest");
			ArmorCoverageList.Add(info);
			info = new IDNameLoadable(0x800 ,"Abdomen");
			ArmorCoverageList.Add(info);
			info = new IDNameLoadable(0x1000 ,"UpperArms");
			ArmorCoverageList.Add(info);
			info = new IDNameLoadable(0x2000 ,"LowerArms");
			ArmorCoverageList.Add(info);
			info = new IDNameLoadable(0x4000 ,"Head");
			ArmorCoverageList.Add(info);
			info = new IDNameLoadable(0x8000 ,"Hands");
			ArmorCoverageList.Add(info);
			info = new IDNameLoadable(0x10000 ,"Feet");
			ArmorCoverageList.Add(info);
//			info = new IDNameLoadable(0x8 ,"Chest(Underwear)");
//			ArmorCoverageList.Add(info);
//			info = new IDNameLoadable(0x10 ,"Abdomen(Underwear)");
//			ArmorCoverageList.Add(info);
			
			if(writelists){
				FileInfo logFile = new FileInfo(GearDir + @"\ArmorCoverage.csv");	
				if(logFile.Exists)
				{
					logFile.Delete();
				}
				StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
				StringBuilder output = new StringBuilder();
				output.Append("Index,SlotID, Name\n");
				int index = 0;
				foreach(var idname in ArmorCoverageList)
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
									"Koujia", "Leather", "Lorica", "Nariyid", "Olthoi", "Olthoi Alduressa", "Olthoi Amuli", "Olthoi Celdon",
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

        private void WriteEnabledSpellsList(int id, string name)
        {
            try
            {

                IDNameLoadable info = new IDNameLoadable(id, name);
                     //if there are already spells in the enabled spells list must be certain to include them in new list
                if (EnabledSpellsList != null && !EnabledSpellsList.Contains(info))
                {

                    EnabledSpellsList.Add(info);
               }
                // if EnabledSpellsList did not exist must create it
                else
                {
                     EnabledSpellsList = new List<IDNameLoadable>();
                    //Now need to add the new spell to the list
                    EnabledSpellsList.Add(info);
                }

               sRuleSpells = "";
               nspells = 0;

                //Now resetup the variables nspells and sRuleSpells
                foreach (IDNameLoadable spl in EnabledSpellsList)
                {
                    string sid = spl.ID.ToString();
                    sRuleSpells = sRuleSpells + sid + ",";
                    nspells++;
                }
                //Now remove the final comma from the variable
                sRuleSpells = sRuleSpells.Substring(0, sRuleSpells.Length - 1);


            }
            catch (Exception ex) { LogError(ex); }
        }

      
        private void CreateWeaponTypeList()
        {
            try
            {
                IDNameLoadable info;
                WeaponTypeList.Clear();
                info = new IDNameLoadable(0, "None");
                addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                info = new IDNameLoadable(47, "Missile");
                addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                info = new IDNameLoadable(41, "TwoHanded");
                addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                info = new IDNameLoadable(44, "Heavy");
                addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                info = new IDNameLoadable(45, "Light");
                addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                info = new IDNameLoadable(46, "Finesse");
                addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                info = new IDNameLoadable(34, "War Magic");
                addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
                info = new IDNameLoadable(43, "Void Magic");
                addInfo(info, WeaponTypeList, cboWeaponAppliesTo);
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
			// Flags suck....
			// TODO:  Spend a fortune finishing this list....
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
								"Weave of Dirty Fighting", "Weave of Dual Wield", "Weave of Recklessness", "Weave of Shield", "Weave of Sneak Attack"
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
				where !sets.name.Contains("Weave") && !sets.name.Contains("Unknown") && !sets.name.Contains("Sigil")
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

        private void CreateCloakSetsList()
        {
        	
        	try
        	{
        		var cloaks = from sets in SetsIndex	
        			where sets.name.Contains("Weave")
        			orderby sets.name
        			select sets;
        		foreach(var cloaksets in cloaks)
        		{
        			CloakSetsList.Add(cloaksets);
        		}
        			
        	}catch{}

            if (writelists)
            {
                FileInfo logFile = new FileInfo(GearDir + @"\CloakSetsList.csv");
                if (logFile.Exists)
                {
                    logFile.Delete();
                }
                StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
                StringBuilder output = new StringBuilder();
                output.Append("Indexed to SetID\n\nSet ID, Name\n");
                foreach (var idname in CloakSetsList)
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
			for(int i = 0; i < FileService.SpeciesTable.Length; i++)
			{
				info = new IDName();
				info.ID = i;		
				try
				{
					info.name = (FileService.SpeciesTable.GetById(i)).Name;
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
			for(int i = 0; i < FileService.MaterialTable.Length; i++)
			{
				info = new IDName();
				info.ID = i;		
				try
				{
					info.name = (FileService.MaterialTable.GetById(i)).Name;
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
			for(int i = 0; i < FileService.AttributeTable.Length; i++)
			{
				IDName info = new IDName();
				info.ID = i;
				try
				{
					info.name = (FileService.AttributeTable.GetById(i)).Name;
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
			for(int i = 0; i < FileService.SkillTable.Length; i++)
			{
				if(FileService.SkillTable[i].Id > loopcounter) {loopcounter = FileService.SkillTable[i].Id;}
			}
			
			for(int i = 0; i <= loopcounter; i++)
			{
				IDName info = new IDName();
				info.ID = i;
				try
				{
					info.name = (FileService.SkillTable.GetById(i)).Name;
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
		
		
		private void CreateMasteryIndex()
		{
			string[] loadme = {"None", "Unarmed", "Sword", "Axe", "Mace", "Spear", "Dagger", "Staff", "Bow", "Crossbow", "Thrown", "TwoHanded"};
			
			int i = 0;
			foreach(string load in loadme)
			{
				IDNameLoadable info = new IDNameLoadable(i++, load);
				MasteryIndex.Add(info);
                cboMasteryType.Add(info.name);
			}	
			
			if(writelists){
                FileInfo logFile = new FileInfo(GearDir + @"\MasteryIndex.csv");	
							if(logFile.Exists)
				{
					logFile.Delete();
				}
			StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
			StringBuilder output = new StringBuilder();
			output.Append("Indexed to Mast.ID\n\nMasteryID, Name\n");
			foreach(var info in MasteryIndex)
			{
				output.Append(info.ID.ToString() + "," + info.name.ToString() + "\n");
			}
			writer0.WriteLine(output);
			writer0.Close();
			}
			
		}

        private void CreateEssMasteryList()
        {
            string[] loadme = { "None", "Naturalist", "Primalist", "Necromancer" };

            int i = 0;
            foreach (string load in loadme)
            {
                IDNameLoadable info = new IDNameLoadable(i++, load);
                EssMasteryList.Add(info);
                cboRuleEssMastery.Add(info.name);
            }

            //if (writelists)
            //{
            //    FileInfo logFile = new FileInfo(GearDir + @"\EssMasteryList.csv");
            //    if (logFile.Exists)
            //    {
            //        logFile.Delete();
            //    }
            //    StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
            //    StringBuilder output = new StringBuilder();
            //    output.Append("Indexed to Mast.ID\n\nMasteryID, Name\n");
            //    foreach (var info in EssMasteryList)
            //    {
            //        output.Append(info.ID.ToString() + "," + info.name.ToString() + "\n");
            //    }
            //    writer0.WriteLine(output);
            //    writer0.Close();
            //}

        }

           
         private void CreateEssElementsList()
        {
            IDNameLoadable info;
            //Again...Flags suck
            
            info = new IDNameLoadable(0, "None");
            EssElementsList.Add(info);
              info = new IDNameLoadable(1, "Slashing");
            EssElementsList.Add(info);
            info = new IDNameLoadable(2, "Piercing");
            EssElementsList.Add(info);
            info = new IDNameLoadable(4, "Bludgeoning");
            EssElementsList.Add(info);
            info = new IDNameLoadable(8, "Cold");
            EssElementsList.Add(info);
             info = new IDNameLoadable(16, "Fire");
             EssElementsList.Add(info);
            info = new IDNameLoadable(32, "Acid");
            EssElementsList.Add(info);
             info = new IDNameLoadable(64, "Electric");
             EssElementsList.Add(info);
             info = new IDNameLoadable(1024, "Void");
             EssElementsList.Add(info);
          
            if(writelists){
                FileInfo logFile = new FileInfo(GearDir + @"\EssElementsList.csv");    
                            if(logFile.Exists)
                {
                    logFile.Delete();
                }
            StreamWriter writer0 = new StreamWriter(logFile.FullName, true);
            StringBuilder output = new StringBuilder();
            output.Append("Index, ID#, Damagetype\n");
            int index = 0;
            foreach (var idname in EssElementsList)
            {
                output.Append(index.ToString() + "," + idname.ID.ToString() + "," + idname.name.ToString() + "\n");
            }
            writer0.WriteLine(output);
            writer0.Close();
            }
        }

		
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
			
		for(int i = 0; i < FileService.HeritageTable.Length; i++)
			{
				IDName info = new IDName();
				info.ID = i;				
				try
				{
					info.name = (FileService.HeritageTable.GetById(i)).Name;
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
             for (int spellid = 0; spellid <= 7000; spellid++)
            {
                spellinfo tsinfo = new spellinfo();
                try
                {
	                 Spell tspell = FileService.SpellTable.GetById(spellid);
	
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
         	    				  (spels1.spellid < 5897 || spels1.spellname.ToLower().Contains("legendary"))
         	    			select spels1;
         	    
         	    foreach(var spel in spl2)
         	    {
         	    	ItemsSpellList.Add(spel);
         	    }
         	   
         	    
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
  
        private void CreateFilteredSpellIndex()
        {
        	try{
        	var spelllist = from tsinfo in ItemsSpellList
        					where (tsinfo.spelllevel == 6 && bRuleFilterlvl6) || (tsinfo.spelllevel == 7 && bRuleFilterlvl7) || 
        		       			  (tsinfo.spelllevel == 8 && bRuleFilterlvl8) || (tsinfo.spellname.Contains("Major") && bRuleFilterMajor) ||
                                  (tsinfo.spellname.Contains("Epic") && bRuleFilterEpic) || (tsinfo.spellname.ToLower().Contains("legendary") && bRuleFilterLegend) 
							orderby tsinfo.spellname
        			 		select tsinfo;
        	
        	foreach(var spel in spelllist)
        	{
        		FilteredSpellIndex.Add(spel);
        	}
        		
                }
                catch
                {
                }
            if (writelists)
            { doWriteLists(FilteredSpellIndex); }
         }

         private void CreateCloakSpellList()
         {
            try
            {

            	var cspells = from spl in SpellIndex
               				where spl.spellname.Contains("Shroud of") || spl.spellname.Contains("Cloaked") || spl.spellname.Contains("Horizon's Blades") ||
            				spl.spellname.Contains("Tectonic Rifts") || spl.spellname.Contains("Nuhmudira's Spines") ||
            				spl.spellname.Contains("Searing Disc") || spl.spellname.Contains("Cassius' Ring of Fire") || spl.spellname.Contains("Halo of Frost") ||
            				spl.spellname.Contains("Eye of the Storm") || spl.spellname.Contains("Clouded Soul")
            				orderby spl.spellname ascending
							select spl;
            	
            	
            	foreach(var selectedspell in cspells)
            	{
            		CloakSpellList.Add(selectedspell);
            	}
            	
            	spellinfo damageabsorb = new spellinfo();
            	damageabsorb.spellname = "Damage Absorb";
            	damageabsorb.spelllevel = 0;
            	damageabsorb.spellid = 10000;
            	
            	CloakSpellList.Add(damageabsorb);
	
            }catch{}
            	

             if (writelists){doWriteLists(CloakSpellList);}
         }

       //These are functions to create lists for comboboxes and initialize them

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
                cmbSelectClass.Add(info.name);
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
                addInfo(info,MeleeTypeInvList,cmbWieldAttrib);
               info = new IDNameLoadable(41, "TwoHanded");
               addInfo(info, MeleeTypeInvList, cmbWieldAttrib);
               info = new IDNameLoadable(44, "Heavy");
               addInfo(info, MeleeTypeInvList, cmbWieldAttrib);
               info = new IDNameLoadable(45, "Light");
               addInfo(info, MeleeTypeInvList, cmbWieldAttrib);
               info = new IDNameLoadable(46, "Finesse");
               addInfo(info, MeleeTypeInvList, cmbWieldAttrib);
            }
            catch (Exception ex) { LogError(ex); }
            if (writelists) { doWriteLists(MeleeTypeInvList); }

        }


        private void CreateArmorSetsInvList()
        {
            ArmorSetsInvList = new List<IDNameLoadable>();
            IDNameLoadable info = new IDNameLoadable(0,"None");
            ArmorSetsInvList.Add(info);
            cmbArmorSet.Add(info.name);
            var armors = from sets in SetsIndex
                         where !sets.name.Contains("Weave") && !sets.name.Contains("Unknown") && !sets.name.Contains("Sigil")
                            orderby sets.name
                         select sets;
            foreach (var armorsets in armors)
            {
                ArmorSetsInvList.Add(armorsets);
                cmbArmorSet.Add(armorsets.name);
            }
            var armors2 = from sets2 in SetsIndex
                         where sets2.name.Contains("Weave") 
                         orderby sets2.name
                         select sets2;
            foreach (var armorsets2 in armors2)
            {
                ArmorSetsInvList.Add(armorsets2);
                cmbArmorSet.Add(armorsets2.name);
            }


        }



        private void CreateMaterialInvList()
        {
            MaterialInvList = new List<IDNameLoadable>();
            var materials = from mat in MaterialIndex
                           orderby mat.name
                           select mat;
            IDNameLoadable info = new IDNameLoadable(0, "None");
            cmbMaterial.Add(info.name);
            MaterialInvList.Add(info);
            foreach (var material in materials)
            {
                info = new IDNameLoadable(material.ID, material.name);
                cmbMaterial.Add(material.name);
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
            addInfo(info,ElementalInvList,cmbDamageType);
            info = new IDNameLoadable(1, "Slashing");
            addInfo(info, ElementalInvList, cmbDamageType);
            info = new IDNameLoadable(2, "Piercing");
            addInfo(info, ElementalInvList, cmbDamageType);
            info = new IDNameLoadable(3, "Slashing:Piercing");
            addInfo(info, ElementalInvList, cmbDamageType);
            info = new IDNameLoadable(4, "Bludgeoning");
            addInfo(info, ElementalInvList, cmbDamageType);
            info = new IDNameLoadable(8, "Cold");
            addInfo(info, ElementalInvList, cmbDamageType);
            info = new IDNameLoadable(16, "Fire");
            addInfo(info, ElementalInvList, cmbDamageType);
            info = new IDNameLoadable(32, "Acid");
            addInfo(info, ElementalInvList, cmbDamageType);
            info = new IDNameLoadable(64, "Electric");
            addInfo(info, ElementalInvList, cmbDamageType);
            info = new IDNameLoadable(1024, "Void");
            addInfo(info, ElementalInvList, cmbDamageType);
            if (writelists) { doWriteLists(ElementalInvList); }

        }


        private void CreateArmorLevelInvList()
        {
            try
            {
                ArmorLevelInvList = new List<IDNameLoadable>();
                IDNameLoadable info;
                info = new IDNameLoadable(0, "None");
                addInfo(info,ArmorLevelInvList,cmbArmorLevel);
                info = new IDNameLoadable(1, "No Wield");
                addInfo(info, ArmorLevelInvList, cmbArmorLevel);
                info = new IDNameLoadable(2, "60");
                addInfo(info, ArmorLevelInvList, cmbArmorLevel);
                info = new IDNameLoadable(3, "90");
                addInfo(info, ArmorLevelInvList, cmbArmorLevel);
                info = new IDNameLoadable(4, "100");
                addInfo(info, ArmorLevelInvList, cmbArmorLevel);
                info = new IDNameLoadable(5, "150");
                addInfo(info, ArmorLevelInvList, cmbArmorLevel);
                info = new IDNameLoadable(6, "180");
                addInfo(info, ArmorLevelInvList, cmbArmorLevel);
                info = new IDNameLoadable(7, "225");
                addInfo(info, ArmorLevelInvList, cmbArmorLevel);
   
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
                addInfo(info,SalvageWorkInvList,cmbSalvWork);
                info = new IDNameLoadable(1, "1-6");
                addInfo(info, SalvageWorkInvList, cmbSalvWork);
                info = new IDNameLoadable(2, "7-8");
                addInfo(info, SalvageWorkInvList, cmbSalvWork);
                info = new IDNameLoadable(3, "9");
                addInfo(info, SalvageWorkInvList, cmbSalvWork);
                info = new IDNameLoadable(4, "10");
                addInfo(info, SalvageWorkInvList, cmbSalvWork);
            }
            catch (Exception ex) { LogError(ex); }
            if (writelists) { doWriteLists(CloakSpellList); }

        }

        private void CreateCoverageInvList()
        {
            try
            {
                CoverageInvList = new List<IDNameLoadable>();
                IDNameLoadable info;
                info = new IDNameLoadable(0, "None");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(256, "Upper_Leg");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(512, "Lower_Leg");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(1024, "Chest");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(2048, "Abdomen");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(4096, "Upper_Arm");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(8192, "Lower_Arm");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(16384, "Head");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(32768, "Hands");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(65536, "Feet");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(768, "Upper_Lower_Legs");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(2304, "Abdomen_Upper_Legs");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(2816, "Abdomen_Upper_Lower_Legs");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(3072, "Chest_Abdomen");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(5120, "Chest_Upper_Arms");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(7168, "Chest_Abdomen_Upper_Arm");
                addInfo(info, CoverageInvList, cmbCoverage);
                 info = new IDNameLoadable(13312, "Chest_Upper_Lower_Arms");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(15360, "Chest_Abdomen_Upper_Lower_Arms");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(8, "Underwear_Chest");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(19, "Underwear_Abdomen_Upper_Legs");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(22, "Underwear_Abdomen_Upper_Lower_Legs");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(40, "Underwear_Chest_UpperArm");
                addInfo(info, CoverageInvList, cmbCoverage);
                info = new IDNameLoadable(131072, "Underwear_Chest_Upper_LowerArms");
                addInfo(info, CoverageInvList, cmbCoverage);
                // case 0: objCovers = 0; break;
                //case 1: objCovers = 256; Upper_Leg;
                //case 2: objCovers = 512; Lower_Leg;
                //case 3: objCovers = 1024; Chest;
                //case 4: objCovers = 2048; Abdomen;
                //case 5: objCovers = 4096; Upper_Arm;
                //case 6: objCovers = 8192; Lower_Arm;
                //case 7: objCovers = 16384; Head
                //case 8: objCovers = 32768; Hands;
                //case 9: objCovers = 65536; Feet
                //case 10: objCovers = 768; Upper_Lower_Legs;
                //case 11: objCovers = 2304; Abdomen_Upper_Legs;
                //case 12: objCovers = 2816; Abdomen_Upper_Lower_Legs;
                //case 13: objCovers = 3072; Chest_Abdomen;
                //case 14: objCovers = 5120; Chest_Upper_Arms;
                //case 15: objCovers = 7168; Chest_Abdomen_Upper_Arm;
                //case 16: objCovers = 13312; Chest_Upper_Lower_Arms;
                //case 17: objCovers = 15360; Chest_Abdomen_Upper_Lower_Arms;
                //case 18: objCovers = 8; Underwear_Chest;
                //case 19: objCovers = 19; Underwear_Abdomen_Upper_Legs;
                //case 20: objCovers = 22; Underwear_Abdomen_Upper_Lower_Legs;
                //case 21: objCovers = 40; Underwear_Chest_UpperArms;
                //case 22: objCovers = 131072; Underwear_Chest_Upper_LowerArms;

                    //                <option text="Upper_Leg" data="2"/>
                    //<option text="Lower_Leg" data="3"/>
                    //<option text="Chest" data="4"/>
                    //<option text="Abdomen" data="5"/>
                    //<option text="Upper_Arm" data="6"/>
                    //<option text="Lower_Arm" data="7"/>
                    //<option text="Head" data="8"/>
                    //<option text="Hands" data="9"/>
                    //<option text="Feet" data="10"/>
                    //<option text="Upper_Lower_Legs" data="11"/>
                    //<option text="Abdomen_Upper_Legs" data="12"/>
                    //<option text="Abdomen_Upper_Lower_Legs" data="13"/>
                    //<option text="Chest_Abdomen" data="14"/>
                    //<option text="Chest_Upper_Arms" data="15"/>
                    //<option text="Chest_Abdomen_Upper_Arm" data="16"/>
                    //<option text="Chest_Upper_Lower_Arms" data="17"/>
                    //<option text="Chest_Abdomen_Upper_Lower_Arms" data="18"/>
                    //<option text="Underwear_Chest" data="19"/>
                    //<option text="Underwear_Abdomen_Upper_Legs" data="20"/>
                    //<option text="Underwear_Abdomen_Upper_Lower_Legs" data="21"/>
                    //<option text="Underwear_Chest_UpperArms" data="22"/>
                    //<option text="Underwear_Chest_Upper_LowerArms" data="23"/>-->
 
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
                addInfo(info, EmbueInvList, cmbEmbue);
                info = new IDNameLoadable(1, "Critical_Strike");
                addInfo(info, EmbueInvList, cmbEmbue);
                info = new IDNameLoadable(2, "Crippling_Blow");
                addInfo(info, EmbueInvList, cmbEmbue);
                info = new IDNameLoadable(4, "Armor");
                addInfo(info, EmbueInvList, cmbEmbue);
                info = new IDNameLoadable(8, "Slashing");
                addInfo(info, EmbueInvList, cmbEmbue);
                info = new IDNameLoadable(16, "Piercing");
                addInfo(info, EmbueInvList, cmbEmbue);
                info = new IDNameLoadable(32, "Bludgeon");
                addInfo(info, EmbueInvList, cmbEmbue);
                info = new IDNameLoadable(64, "Acid");
                addInfo(info, EmbueInvList, cmbEmbue);
                info = new IDNameLoadable(128, "Frost");
                addInfo(info, EmbueInvList, cmbEmbue);
                info = new IDNameLoadable(256, "Lightning");
                addInfo(info, EmbueInvList, cmbEmbue);
                info = new IDNameLoadable(512, "Fire");
                addInfo(info, EmbueInvList, cmbEmbue);
                info = new IDNameLoadable(1000, "MagicD_PlusOne");
                addInfo(info, EmbueInvList, cmbEmbue);


                //                    <!--<option text="None" data="1"/>
                //    <option text="Critical_Strike" data="1"/>
                //    <option text="Crippling_Blow" data="2"/>
                //    <option text="Armor" data="4"/>
                //    <option text="Slashing" data="8"/>
                //    <option text="Piercing" data="16"/>
                //    <option text="Bludgeon" data="32"/>
                //    <option text="Acid" data="64"/>
                //    <option text="Frost" data="128"/>
                //    <option text="Lightning" data="256"/>
                //    <option text="Fire" data="512
                //    <option text="MagicD_PlusOne" data="1000"/>-->

                //         case 0: objEmbueTypeInt = 0; break;
                //case 1: objEmbueTypeInt = 1; break;
                //case 2: objEmbueTypeInt = 2; break;
                //case 3: objEmbueTypeInt = 4; break;
                //case 4: objEmbueTypeInt = 8; break;
                //case 5: objEmbueTypeInt = 16; break;
                //case 6: objEmbueTypeInt = 32; break;
                //case 7: objEmbueTypeInt = 64; break;
                //case 8: objEmbueTypeInt = 128; break;
                //case 9: objEmbueTypeInt = 256; break;
                //case 10: objEmbueTypeInt = 512; break;
                //case 11: objEmbueTypeInt = 1000;


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
                cmbLevel.Add(info.name);
            }
            if (writelists) { doWriteLists(CloakSpellList); }

            }
            catch (Exception ex) { LogError(ex); }
            if (writelists) { doWriteLists(WeaponWieldInvList); }

        }


        private void addInfo(IDNameLoadable info, List<IDNameLoadable> lst, MyClasses.MetaViewWrappers.ICombo cmb)
        {
            lst.Add(info);
            cmb.Add(info.name);
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

 
    
					
	