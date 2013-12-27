using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Decal.Adapter;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Win32;
using Decal.Filters;
using Decal.Interop;
using System.Runtime.InteropServices;
using Decal.Adapter.Wrappers;
using System.Drawing;
using System.Xml;
using System.Xml.Linq;
using System.ComponentModel;
using VirindiViewService;
using MyClasses.MetaViewWrappers;
using System.Windows.Forms;

// I am using this file -- Karin 4/16/13
namespace GearFoundry
{
    public partial class PluginCore : Decal.Adapter.PluginBase
    {
        //Need to set up directories if not present and to create filenames for needed files
        private void InitPaths()
        {
            try
            {
                toonName = Core.CharacterFilter.Name;
                world = Core.CharacterFilter.Server;

                // need to set path for saving folders to the current world which is called currDir
                setPathsForDirs();
                chkDirsExist();

                // Believe this path or whatever is needed for using dictionery functions
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();

                //information -- just putting  here but perhaps should be in another function for readability
                System.Version AppVersion = asm.GetName().Version;
                dllversion = AppVersion.ToString();
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void InitFilenames()
        {
            try
            {
                toonSettingsFilename = toonDir + @"\Settings.xml";
                genSettingsFilename = GearDir + @"\Settings.xml";
                switchGearSettingsFilename = GearDir + @"\SwitchGearSettings.xml";
                mobsFilename = GearDir + @"\Mobs.xml";
                trophiesFilename = GearDir + @"\Trophies.xml";
                rulesFilename = GearDir + @"\Rules.xml";
                salvageFilename = GearDir + @"\Salvage.xml";
                tempFilename = GearDir + @"\Temporary.xml";
                inventoryFilename = toonDir + @"\" + toonName + "Inventory.xml";
                genInventoryFilename = currDir + @"\inventory.xml";
                holdingInventoryFilename = currDir + @"\holdingInventory.xml";
                inventorySelect = currDir + @"\inventorySelected.xml";
                quickSlotsvFilename = toonDir + @"\" + "QuickSlotsv.xml";
                quickSlotshFilename = toonDir + @"\" + "QuickSlotsh.xml";
                remoteGearFilename = GearDir + @"\" + "RemoteGear.xml";
                portalGearFilename = toonDir + @"\" + "PortalGear.xml";

                if (!File.Exists(rulesFilename))
                {
                    try
                    {
                    	
                    	string filedefaults = GetResourceTextFile("Rules.xml");
                    	using (StreamWriter writedefaults = new StreamWriter(rulesFilename, true))
						{
							writedefaults.Write(filedefaults);
							writedefaults.Close();
						}

                    }
                    catch (Exception ex) { LogError(ex); }

                }


                if (!File.Exists(mobsFilename))
                {
                    try
                    {	
                    	
						string filedefaults =  GetResourceTextFile("Mobs.xml");
						using (StreamWriter writedefaults = new StreamWriter(mobsFilename, true))
						{
							writedefaults.Write(filedefaults);
							writedefaults.Close();
						}
                    }
                    catch (Exception ex) { LogError(ex); }

                }
                if (!File.Exists(trophiesFilename))
                {
                    try
                    {
                    	
                    	string filedefaults = GetResourceTextFile("Trophies.xml");
                    	using (StreamWriter writedefaults = new StreamWriter(trophiesFilename, true))
						{
							writedefaults.Write(filedefaults);
							writedefaults.Close();
						}
                   }
                    catch (Exception ex) { LogError(ex); }

                }
                if (!File.Exists(salvageFilename))
                {
                    try
                    {
                    	string filedefaults = GetResourceTextFile("Salvage.xml");
                    	using (StreamWriter writedefaults = new StreamWriter(salvageFilename, true))
						{
							writedefaults.Write(filedefaults);
							writedefaults.Close();
						}
                    }
                    catch (Exception ex) { LogError(ex); }
                }
                
                if (!File.Exists(genSettingsFilename))
                {
                    try
                    {
                    	string filedefaults = GetResourceTextFile("Settings.xml");
                    	using (StreamWriter writedefaults = new StreamWriter(genSettingsFilename, true))
						{
							writedefaults.Write(filedefaults);
							writedefaults.Close();
						}
                    }
                    catch (Exception ex) { LogError(ex); }
                }

                if (!File.Exists(switchGearSettingsFilename))
                {
                    try
                    {                    
                        xdocSwitchGearSettings = new XDocument(new XElement("Settings"));
                        xdocSwitchGearSettings.Element("Settings").Add(new XElement("Setting",
                        new XElement("QuickSlotsvEnabled", mMainSettings.bquickSlotsvEnabled),
                        new XElement("QuickSlotshEnabled", mMainSettings.bquickSlotshEnabled)));
                        xdocSwitchGearSettings.Save(switchGearSettingsFilename);
                      
                    }
                        
                    catch (Exception ex) { LogError(ex); }
                }




            } //end of try
            catch (Exception ex) { LogError(ex); }
        }

        public void setPathsForDirs()
        {
            //Directory for the alinco Document files
            GearDir = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Decal Plugins\" + Globals.PluginName);
            //Directory for the current world in Gear Directory
            currDir = String.Concat(GearDir + @"\" + world);
            //Directory for the toon in the current world
            toonDir = String.Concat(currDir + @"\" + toonName);
        }



        // need to be certain above directories exist and create them if they don't
        public void chkDirsExist()
        {

            if (!Directory.Exists(GearDir))
            { DirectoryInfo dirInfo = Directory.CreateDirectory(GearDir); }
            if (!Directory.Exists(currDir))
            { DirectoryInfo dirInfo = Directory.CreateDirectory(currDir); }
            if (!Directory.Exists(toonDir))
            { DirectoryInfo dirInfo = Directory.CreateDirectory(toonDir); }
        }

        public void loadFiles()
        {
            xdocMobs = XDocument.Load(mobsFilename);
            xdocTrophies = XDocument.Load(trophiesFilename);
            xdocSalvage = XDocument.Load(salvageFilename);
            xdocRules = XDocument.Load(rulesFilename);
            xdocGenSettings = XDocument.Load(genSettingsFilename);
            xdocSwitchGearSettings = XDocument.Load(switchGearSettingsFilename);
            
            try
            {  
            	setUpLists(xdocMobs, mSortedMobsList);
                setUpLists(xdocTrophies, mSortedTrophiesList);              
                InitSalvageList();
                InitRules();
                setUpSettingsList();
                                
            }
            catch (Exception ex) { LogError(ex); }
        }

        public void setUpSettingsList()
        {
          //  if (xdocGenSettings != null)
            try
            {
 
                IEnumerable<XElement> elements = xdocGenSettings.Element("Settings").Elements("Setting");
                foreach (XElement el in elements.Descendants())
                {
                    mGenSettingsList.Add(el);
                }


                fillSettingsVariables();
                
            }

            catch (Exception ex) { LogError(ex); }

    }

        public void setUpLists(XDocument xdoc, List<XElement> sorted)
        {
            sorted.Clear();
            
            IEnumerable<XElement> myelements = xdoc.Element("GameItems").Descendants("item");

            var lstChecked = from element in myelements
                                            where Convert.ToBoolean(element.Element("checked").Value)
                                            orderby element.Element("key").Value ascending

                                            select element;
            
            sorted.AddRange(lstChecked);

            var lstUnChecked = from element in myelements
                                              where !Convert.ToBoolean(element.Element("checked").Value)
                                              orderby element.Element("key").Value ascending

                                              select element;

            sorted.AddRange(lstUnChecked);

        }
        
        
        public void InitSalvageList()
        {
        	try
        	{
        		mSalvageList.Clear();
        		mSalvageList = xdocSalvage.Element("GameItems").Descendants("item").ToList();
        		FillSalvageRules();
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void _UpdateRulesTabs()
        {
        	try
        	{
        		string[] SplitString;
        		int HoldRulesPosition = lstRules.ScrollPosition;
        		
        		if(mPrioritizedRulesList.Count == 0) {mPrioritizedRulesList.Add(CreateRulesXElement());}
        		if(mSelectedRule == null){mSelectedRule = mPrioritizedRulesList.First();}
        		
        		List<XElement> SortedRulesList = mPrioritizedRulesList.OrderByDescending(x => x.Element("Enabled").Value).ThenBy(y => y.Element("Name").Value).ToList();
        		
        		lstRules.Clear();  		
        		
				foreach (XElement element in SortedRulesList)
                {
					MyClasses.MetaViewWrappers.IListRow newRow = lstRules.AddRow();
					newRow[0][0] = Convert.ToBoolean(element.Element("Enabled").Value);
					newRow[1][0] = element.Element("Priority").Value;
					newRow[2][0] = element.Element("Name").Value;
                    newRow[3][1] = 0x6005e6a;
                    newRow[4][0] = element.Element("RuleNum").Value;
				}
        		        		
        		//Not Visible:  "RuleNum"
        		txtRuleName.Text = mSelectedRule.Element("Name").Value;
        		txtRulePriority.Text = mSelectedRule.Element("Priority").Value;
        		chkRuleEnabled.Checked = Convert.ToBoolean(mSelectedRule.Element("Enabled").Value);
        		txtGearScore.Text = mSelectedRule.Element("GearScore").Value;
        		txtRuleMaxCraft.Text = mSelectedRule.Element("Work").Value;
        		txtRuleArcaneLore.Text = mSelectedRule.Element("ArcaneLore").Value;
        		txtRuleWieldLevel.Text = mSelectedRule.Element("WieldLevel").Value;
        		txtRuleNumSpells.Text = mSelectedRule.Element("NumSpells").Value;
        		
        		SplitString = mSelectedRule.Element("WieldEnabled").Value.Split(',');
        		chkRuleWeaponsa.Checked = Convert.ToBoolean(SplitString[0]);
        		chkRuleWeaponsb.Checked = Convert.ToBoolean(SplitString[1]);
        		chkRuleWeaponsc.Checked = Convert.ToBoolean(SplitString[2]);
        		chkRuleWeaponsd.Checked = Convert.ToBoolean(SplitString[3]);
        		
        		SplitString = mSelectedRule.Element("ReqSkill").Value.Split(',');
        		txtRuleReqSkilla.Text = SplitString[0];
        		txtRuleReqSkillb.Text = SplitString[1];
        		txtRuleReqSkillc.Text = SplitString[2];
        		txtRuleReqSkilld.Text = SplitString[3];
        		   
        		int HoldAppliesPosition = lstRuleApplies.ScrollPosition;
        		_PopulateList(lstRuleApplies, AppliesToList, _ConvertCommaStringToIntList(mSelectedRule.Element("AppliesToFlag").Value)); 
        		int HoldSlotsPosition = lstRuleSlots.ScrollPosition;
				_PopulateList(lstRuleSlots, SlotList, _ConvertCommaStringToIntList(mSelectedRule.Element("Slots").Value));
				int HoldDamagePosition = lstDamageTypes.ScrollPosition;
				_PopulateList(lstDamageTypes, ElementalList, _ConvertCommaStringToIntList(mSelectedRule.Element("DamageType").Value));
				int HoldArmorPosition = lstRuleArmorTypes.ScrollPosition;
				_PopulateList(lstRuleArmorTypes, ArmorIndex, _ConvertCommaStringToIntList(mSelectedRule.Element("ArmorType").Value));
				int HoldSetPosition = lstRuleSets.ScrollPosition;
				_PopulateList(lstRuleSets, ArmorSetsList, _ConvertCommaStringToIntList(mSelectedRule.Element("ArmorSet").Value));
        		
				cboWeaponAppliesTo.Selected = WeaponTypeList.FindIndex(x => x.ID == Convert.ToInt32(mSelectedRule.Element("WieldSkill").Value));
        		cboMasteryType.Selected = Convert.ToInt32(mSelectedRule.Element("MasteryType").Value);
        		
        		int HoldEnabledSpellsPostion = lstRuleSpellsEnabled.ScrollPosition;
        		_UpdateSpellEnabledListBox();
        		int HoldSpellListPosition = lstRuleSpells.ScrollPosition;
        		_UpdateSpellListBox();
        		
        		lstRules.ScrollPosition = HoldRulesPosition;
        		lstRuleApplies.ScrollPosition = HoldAppliesPosition;
        		lstRuleSlots.ScrollPosition = HoldSlotsPosition;
        		lstDamageTypes.ScrollPosition = HoldDamagePosition;
        		lstRuleArmorTypes.ScrollPosition = HoldArmorPosition;
        		lstRuleSets.ScrollPosition = HoldSetPosition;
        		lstRuleSpellsEnabled.ScrollPosition = HoldEnabledSpellsPostion;
        		lstRuleSpells.ScrollPosition = HoldSpellListPosition;
        		
        		_UpdateAdvancedRulesTab();
        		
        		
        		//UNDONE:  Palettes
        		
        		
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void _UpdateAdvancedRulesTab()
        {
        	try
        	{
        		chkAdvEnabled.Checked = false;
        		cboAdv1KeyType.Clear();
        		cboAdv1Key.Clear();
        		cboAdv1KeyCompare.Clear();
        		cboAdv1Link.Clear();
        		txtAdv1KeyValue.Text = String.Empty;
        		
        		cboAdv2KeyType.Clear();
        		cboAdv2Key.Clear();
        		cboAdv2KeyCompare.Clear();
        		cboAdv2Link.Clear();
        		txtAdv2KeyValue.Text = String.Empty;
        		
        		cboAdv3KeyType.Clear();
        		cboAdv3Key.Clear();
        		cboAdv3KeyCompare.Clear();
        		cboAdv3Link.Clear();
        		txtAdv3KeyValue.Text = String.Empty;
        		
        		cboAdv4KeyType.Clear();
        		cboAdv4Key.Clear();
        		cboAdv4KeyCompare.Clear();
        		cboAdv4Link.Clear();
        		txtAdv4KeyValue.Text = String.Empty;
        		
        		cboAdv5KeyType.Clear();
        		cboAdv5Key.Clear();
        		cboAdv5KeyCompare.Clear();
        		txtAdv5KeyValue.Text = String.Empty;
        		
        		if((string)mSelectedRule.Element("Advanced").Value == "false")
        		{
					chkAdvEnabled.Checked = false;
					return;					
        		}
        		else
        		{
        			chkAdvEnabled.Checked = true;
        			List<ItemRule.advsettings> advsettings = _ConvertAdvStringToAdvanced((string)mSelectedRule.Element("Advanced").Value);
        			
        			WriteToChat("Advanced Settings Count " + advsettings.Count);
        			
        			if(advsettings.Count > 0)
        			{
        				foreach(string item in KeyTypes) {cboAdv1KeyType.Add(item);}
        				cboAdv1KeyType.Selected = advsettings[0].keytype;
        				FillAdvancedKeyList(cboAdv1KeyType.Selected, cboAdv1Key);	
        				if(cboAdv1KeyType.Selected == 0) {cboAdv1Key.Selected = DoubleKeyList.FindIndex(x => x.ID == advsettings[0].key);}
        				else{cboAdv1Key.Selected = LongKeyList.FindIndex(x => x.ID == advsettings[0].key);}		
          				foreach(string item in KeyCompare) {cboAdv1KeyCompare.Add(item);}
        				cboAdv1KeyCompare.Selected = advsettings[0].keycompare;
        				foreach(string item in KeyLink) {cboAdv1Link.Add(item);}	
        				cboAdv1Link.Selected = advsettings[0].keylink;
        				txtAdv1KeyValue.Text = advsettings[0].keyvalue.ToString();
        			}
        			
        			if(advsettings.Count > 1)
        			{
        				foreach(string item in KeyTypes) {cboAdv2KeyType.Add(item);}
		        		cboAdv2KeyType.Selected = advsettings[1].keytype;
		        		FillAdvancedKeyList(cboAdv2KeyType.Selected, cboAdv2Key);	
		        		if(cboAdv2KeyType.Selected == 0) {cboAdv2Key.Selected = DoubleKeyList.FindIndex(x => x.ID == advsettings[1].key);}
        				else{cboAdv2Key.Selected = LongKeyList.FindIndex(x => x.ID == advsettings[1].key);}		
		        		foreach(string item in KeyCompare) {cboAdv2KeyCompare.Add(item);}
		        		cboAdv2KeyCompare.Selected = advsettings[1].keycompare;
		        		foreach(string item in KeyLink) {cboAdv2Link.Add(item);}	
		        		cboAdv2Link.Selected = advsettings[1].keylink;
		        		txtAdv2KeyValue.Text = advsettings[1].keyvalue.ToString();
        			}
	        		
        			if(advsettings.Count > 2)
        			{
        				foreach(string item in KeyTypes) {cboAdv3KeyType.Add(item);}
        				cboAdv3KeyType.Selected = advsettings[2].keytype;
        				FillAdvancedKeyList(cboAdv3KeyType.Selected, cboAdv3Key);	
		        		if(cboAdv3KeyType.Selected == 0) {cboAdv3Key.Selected = DoubleKeyList.FindIndex(x => x.ID == advsettings[2].key);}
        				else{cboAdv3Key.Selected = LongKeyList.FindIndex(x => x.ID == advsettings[2].key);}		
		        		foreach(string item in KeyCompare) {cboAdv3KeyCompare.Add(item);}
		        		cboAdv3KeyCompare.Selected = advsettings[2].keycompare;
		        		foreach(string item in KeyLink) {cboAdv3Link.Add(item);}	
		        		cboAdv3Link.Selected = advsettings[2].keylink;
		        		txtAdv3KeyValue.Text = advsettings[2].keyvalue.ToString();
        			}
        			
        			if(advsettings.Count > 3)
        			{
						foreach(string item in KeyTypes) {cboAdv4KeyType.Add(item);}        				
		        		cboAdv4KeyType.Selected = advsettings[3].keytype;
		        		FillAdvancedKeyList(cboAdv4KeyType.Selected, cboAdv4Key);	
		        		if(cboAdv4KeyType.Selected == 0) {cboAdv4Key.Selected = DoubleKeyList.FindIndex(x => x.ID == advsettings[3].key);}
        				else{cboAdv4Key.Selected = LongKeyList.FindIndex(x => x.ID == advsettings[3].key);}		
		        		foreach(string item in KeyCompare) {cboAdv4KeyCompare.Add(item);}
		        		cboAdv4KeyCompare.Selected = advsettings[3].keycompare;
		        		foreach(string item in KeyLink) {cboAdv4Link.Add(item);}	
		        		cboAdv4Link.Selected = advsettings[3].keylink;
		        		txtAdv4KeyValue.Text = advsettings[3].keyvalue.ToString();
        			}
        			
        			if(advsettings.Count > 4)
        			{		        		
        				foreach(string item in KeyTypes) {cboAdv5KeyType.Add(item);}
		        		cboAdv5KeyType.Selected = advsettings[4].keytype;
		        		FillAdvancedKeyList(cboAdv5KeyType.Selected, cboAdv5Key);	
		        		if(cboAdv5KeyType.Selected == 0) {cboAdv5Key.Selected = DoubleKeyList.FindIndex(x => x.ID == advsettings[4].key);}
        				else{cboAdv5Key.Selected = LongKeyList.FindIndex(x => x.ID == advsettings[4].key);}		
		        		foreach(string item in KeyCompare) {cboAdv5KeyCompare.Add(item);}
		        		cboAdv5KeyCompare.Selected = advsettings[4].keycompare;	
		        		txtAdv5KeyValue.Text = advsettings[4].keyvalue.ToString();
        			}	
        		}
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void _PopulateList(MyClasses.MetaViewWrappers.IList target, List<IDNameLoadable> source, List<int> selected)
        {
        	try
        	{
        		IListRow row;
        		target.Clear();
        		foreach(IDNameLoadable entry in source)
        		{
        			row = target.AddRow();
        			if(selected.Contains(entry.ID)) {row[0][0] = true;}
        			row[1][0] = entry.name;
        			row[2][0] = entry.ID.ToString();
        		}
        		
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void _UpdateSpellEnabledListBox()
        {
            try
            {
            	MyClasses.MetaViewWrappers.IListRow newRow;
            	lstRuleSpellsEnabled.Clear();
            	
            	List<int> SpellIds = _ConvertCommaStringToIntList(mSelectedRule.Element("Spells").Value);
            	
            	if(SpellIds.Count > 0)
            	{
	            	List<spellinfo> enabledspells = new List<spellinfo>();
	            	foreach(int spell in SpellIds)
	            	{
	            		enabledspells.Add(SpellIndex[spell]);
	            	}
	
	            	
	            	foreach(var spel in enabledspells)
	            	{
	            		newRow = lstRuleSpellsEnabled.AddRow();
	            		newRow[0][0] = spel.spellname;
	            		newRow[1][0] = spel.spellid.ToString();
	            	}
            	}
            }

            catch (Exception ex) { LogError(ex); }
        }
        
        private void _UpdateSpellListBox()
        {
        	try
        	{
        		lstRuleSpells.Clear();
        		
        		List<int> exclude = _ConvertCommaStringToIntList(mSelectedRule.Element("Spells").Value);
        		
        		var spelllist = from tsinfo in ItemsSpellList
        			where ((chkRuleFilterlvl8.Checked && tsinfo.spelllevel == 8) || (chkRuleFilterMajor.Checked && tsinfo.spelllevel == 13) ||
        				  (chkRuleFilterEpic.Checked && tsinfo.spelllevel == 14) || (chkRuleFilterLegend.Checked && tsinfo.spelllevel == 15) ||
        				  (chkRuleFilterCloak.Checked && tsinfo.spelllevel == 20)) && !exclude.Contains(tsinfo.spellid)
					orderby tsinfo.spellname
        			select tsinfo;
        		          
	            foreach (spellinfo element in spelllist)
	            {	             
	                    MyClasses.MetaViewWrappers.IListRow newRow = lstRuleSpells.AddRow();
	                    newRow[0][0] = element.spellname;
	                    newRow[1][0] = element.spellid.ToString();
	             }
                
            }catch (Exception ex) { LogError(ex); }
        }
        

        private void fillSettingsVariables()
        {
            try
            {
                XDocument xdocGenSet = XDocument.Load(genSettingsFilename);
                
                XElement el = xdocGenSet.Root.Element("Setting");

                mMainSettings.bGearVisection = Convert.ToBoolean(el.Element("CorpseHudEnabled").Value);
               	mMainSettings.bGearSenseHudEnabled = Convert.ToBoolean(el.Element("LandscapeHudEnabled").Value);
                mMainSettings.bGearInspectorEnabled = Convert.ToBoolean(el.Element("InspectorHudEnabled").Value);
                mMainSettings.bGearButlerEnabled = Convert.ToBoolean(el.Element("ButlerHudEnabled").Value);
               	mMainSettings.bGearTacticianEnabled = Convert.ToBoolean(el.Element("CombatHudEnabled").Value);
               	mMainSettings.bRemoteGearEnabled = Convert.ToBoolean(el.Element("RemoteGearEnabled").Value);
               	mMainSettings.bPortalGearEnabled = Convert.ToBoolean(el.Element("PortalGearEnabled").Value);
               	mMainSettings.bGearTaskerEnabled = Convert.ToBoolean(el.Element("KillTaskGearEnabled").Value);
               	mMainSettings.bquickSlotsvEnabled = Convert.ToBoolean(el.Element("QuickSlotsvEnabled").Value);
				mMainSettings.bquickSlotshEnabled = Convert.ToBoolean(el.Element("QuickSlotshEnabled").Value);
                mMainSettings.binventoryHudEnabled = Convert.ToBoolean(el.Element("InventoryHudEnabled").Value);
                mMainSettings.binventoryEnabled = Convert.ToBoolean(el.Element("InventoryEnabled").Value);
                mMainSettings.binventoryCompleteEnabled = Convert.ToBoolean(el.Element("InventoryCompleteEnabled").Value);
                mMainSettings.btoonStatsEnabled = Convert.ToBoolean(el.Element("ToonStatsEnabled").Value);
                mMainSettings.bArmorHudEnabled = Convert.ToBoolean(el.Element("ArmorHudEnabled").Value);
      
                bEnableTextFiltering = Convert.ToBoolean(el.Element("EnableTextFiltering").Value);
                bTextFilterAllStatus = Convert.ToBoolean(el.Element("TextFilterAllStatus").Value);
                nitemFontHeight = Convert.ToInt32(el.Element("ItemFontHeight").Value);
                nmenuFontHeight = Convert.ToInt32(el.Element("MenuFontHeight").Value);


                if (nitemFontHeight == 0) { nitemFontHeight = 10; }
                txtItemFontHeight.Text = nitemFontHeight.ToString();
                if (nmenuFontHeight == 0) { nmenuFontHeight = 10; }
                txtMenuFontHeight.Text = nmenuFontHeight.ToString();

 
                    chkQuickSlotsv.Checked = mMainSettings.bquickSlotsvEnabled;
                    chkQuickSlotsh.Checked = mMainSettings.bquickSlotshEnabled;
 
                   // GearVisection Section                   
                    chkGearVisectionEnabled.Checked = mMainSettings.bGearVisection;
                    
                    //GearSense Section
                    chkGearSenseEnabled.Checked = mMainSettings.bGearSenseHudEnabled;
                    //GearInspector Section
                    chkGearInspectorEnabled.Checked = mMainSettings.bGearInspectorEnabled;
                    
                   //GearButler Section
                   chkGearButlerEnabled.Checked = mMainSettings.bGearButlerEnabled;

                   //Gear Tactician Section
                   chkCombatHudEnabled.Checked = mMainSettings.bGearTacticianEnabled;

                   //RemoteGear 
                   chkRemoteGearEnabled.Checked = mMainSettings.bRemoteGearEnabled;
                   
                   //PortalGear 
                   chkPortalGearEnabled.Checked = mMainSettings.bPortalGearEnabled;

                   //KillTaskGear
                   chkKillTaskGearEnabled.Checked = mMainSettings.bGearTaskerEnabled;
  
                   //Misc Gears Section
                   	chkMuteSounds.Checked = mSoundsSettings.MuteSounds;
                   	cboTrophyLandscape.Selected = mSoundsSettings.LandscapeTrophies;
		     		cboMobLandscape.Selected = mSoundsSettings.LandscapeMobs;
		        	cboPlayerLandscape.Selected = mSoundsSettings.LandscapePlayers;
		        	cboCorpseRare.Selected = mSoundsSettings.CorpseRare;
		        	cboCorpseSelfKill.Selected = mSoundsSettings.CorpseSelfKill;
		        	cboCorpseFellowKill.Selected = mSoundsSettings.CorpseFellowKill;
		        	cboDeadMe.Selected = mSoundsSettings.DeadMe;
		        	cboDeadPermitted.Selected = mSoundsSettings.DeadPermitted;
		        	cboTrophyCorpse.Selected = mSoundsSettings.CorpseTrophy;
		        	cboRuleCorpse.Selected = mSoundsSettings.CorpseRule;
			        cboSalvageCorpse.Selected = mSoundsSettings.CorpseSalvage;

                   //Inventory Section
                   chkInventory.Checked = mMainSettings.binventoryEnabled;
                    chkInventoryComplete.Checked = mMainSettings.binventoryCompleteEnabled;
                   chkToonStats.Checked = mMainSettings.btoonStatsEnabled;
                   chkArmorHud.Checked = mMainSettings.bArmorHudEnabled;

 

                   //Filter Section
                   chkEnableTextFiltering.Checked = bEnableTextFiltering;
                   chkTextFilterAllStatus.Checked = bTextFilterAllStatus;


                }
            catch (Exception ex) { LogError(ex); }
        
        }

        private void startRoutines()
        {
            if (mMainSettings.bGearSenseHudEnabled)
            {
            	SubscribeLandscapeEvents();
                RenderLandscapeHud();
            }

            if (mMainSettings.bGearVisection)
            {
            	SubscribeCorpseEvents();
                RenderCorpseHud();
            }

            if (mMainSettings.bGearInspectorEnabled)
            {
            	SubscribeItemEvents();
            	RenderItemHud();
            }

            if (mMainSettings.bGearButlerEnabled)
            {
            	SubscribeButlerEvents();
                RenderButlerHud();
            }

            if (mMainSettings.bGearTacticianEnabled)
            {
            	SubscribeCombatEvents();
            	RenderTacticianHud();
            }

            if (mMainSettings.bRemoteGearEnabled)
            {
            	RenderDynamicRemoteGear();
                //RenderRemoteGearHud();
            }

            if (mMainSettings.bPortalGearEnabled)
            {
            	SubscribePortalEvents();
                RenderPortalGearHud();
                RenderPortal2GearHud();
            }
            if (mMainSettings.bGearTaskerEnabled)
            {
            	SubscribeKillTasks();
                RenderKillTaskPanel();
            }

            if (mMainSettings.binventoryCompleteEnabled)
            {
                mMainSettings.binventoryBurdenEnabled = false;
                mMainSettings.binventoryEnabled = false;
                m = 500;
                doGetInventory();
            }


            if (mMainSettings.bArmorHudEnabled)
            {
                WriteToChat("bArmorHudEnabled; I am going to renderarmorhud");
                RenderArmorHud(); 
            }

            if (mMainSettings.binventoryHudEnabled)
            { RenderInventoryHud(); }



            if (mMainSettings.binventoryBurdenEnabled)
            {
                mMainSettings.binventoryEnabled = false;
                getBurden = true;
                doUpdateInventory();
            }
            if (mMainSettings.binventoryEnabled)
            { doUpdateInventory(); }

            if (mMainSettings.btoonStatsEnabled)
            { getStats(); }

            if (mMainSettings.bquickSlotsvEnabled)
            {
                RenderVerticalQuickSlots(); 
            }

            if (mMainSettings.bquickSlotshEnabled)
            {
                RenderHorizontalQuickSlots();
            }

            if (bEnableTextFiltering || bTextFilterAllStatus)
            {
                SubscribeChatEvents(); 
            }
        }



        void chkInventory_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                mMainSettings.binventoryEnabled = e.Checked;

                SaveSettings();
            }
            catch (Exception ex) { LogError(ex); }

        }

        void chkInventoryBurden_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                mMainSettings.binventoryBurdenEnabled = e.Checked;

                SaveSettings();
            }
            catch (Exception ex) { LogError(ex); }

        }

        void chkInventoryComplete_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                mMainSettings.binventoryCompleteEnabled = e.Checked;

                SaveSettings();
            }
            catch (Exception ex) { LogError(ex); }

        }



        void chkQuickSlotsv_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                mMainSettings.bquickSlotsvEnabled = e.Checked;
                SaveSettings();

                if (mMainSettings.bquickSlotsvEnabled)
                {
                	RenderVerticalQuickSlots();
                }
                else
                {
                	DisposeVerticalQuickSlots();
                }
                RenderDynamicRemoteGear();

            }
            catch (Exception ex) { LogError(ex); }

        }

        void chkQuickSlotsh_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                mMainSettings.bquickSlotshEnabled = e.Checked;
                SaveSettings();

                if (mMainSettings.bquickSlotshEnabled)
                {
                    RenderHorizontalQuickSlots();
                }
                else
                {
                    DisposeHorizontalQuickSlots();
                }
                RenderDynamicRemoteGear();
            }
            catch (Exception ex) { LogError(ex); }
        }

		void chkGearVisectionEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
		{
			try
			{
				mMainSettings.bGearVisection = e.Checked;
				SaveSettings();
				if(e.Checked)
				{
					SubscribeCorpseEvents();
					RenderCorpseHud();
					WriteToChat("GearVisection Enabled.");
				}
				else
				{
					UnsubscribeCorpseEvents();
					DisposeCorpseHud();
					WriteToChat("GearVisection Disabled.");
				}
				RenderDynamicRemoteGear();
			}
			catch(Exception ex){LogError(ex);}
			
		}

        
        void chkGearSenseEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
        	try
        	{
        		mMainSettings.bGearSenseHudEnabled = e.Checked;
				SaveSettings();
				if(e.Checked)
				{
					SubscribeLandscapeEvents();
					RenderLandscapeHud();
					WriteToChat("GearSense Enabled.");
				}
				else
				{
					UnsubscribeLandscapeEvents();
					DisposeLandscapeHud();
					WriteToChat("GearSense Disabled.");
				}
				RenderDynamicRemoteGear();				
        	}catch{}
        }
        
        void chkGearButlerEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
        	try
        	{
        		mMainSettings.bGearButlerEnabled = e.Checked;
        		SaveSettings();
        		if(mMainSettings.bGearButlerEnabled)
        		{
        			SubscribeButlerEvents();
        			RenderButlerHud();
        			WriteToChat("GearButler Enabled.");
        		}
        		else
        		{
        			UnsubscribeButlerEvents();
        			DisposeButlerHud();
        			WriteToChat("GearButler Disabled.");
        		}
        		RenderDynamicRemoteGear();
        	}catch{}
        }
     
        void chkGearInspectorEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                mMainSettings.bGearInspectorEnabled = e.Checked;
                SaveSettings();
                if (mMainSettings.bGearInspectorEnabled)
                {
                	SubscribeItemEvents();
                	RenderItemHud();
                	WriteToChat("GearInspector Enabled.");
                }
                else
                {
                	UnsubscribeItemEvents();
                    DisposeItemHud();
                    WriteToChat("GearInspector Disabled.");
                }
                RenderDynamicRemoteGear();
            }
            catch { }
        }

        void chkCombatHudEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                mMainSettings.bGearTacticianEnabled = e.Checked;
                SaveSettings();
                if (mMainSettings.bGearTacticianEnabled)
                {
                	SubscribeCombatEvents();
                	RenderTacticianHud();
                	WriteToChat("GearTactician Enabled.");
                }
                else
                {
                	UnsubscribeCombatEvents();
                	DisposeTacticianHud();
                	WriteToChat("GearTactician Disabled.");
                }
                RenderDynamicRemoteGear();
            }
            catch { }
        }

        void chkRemoteGearEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                mMainSettings.bRemoteGearEnabled = e.Checked;
                SaveSettings();
                if (mMainSettings.bRemoteGearEnabled)
                {
                	RenderDynamicRemoteGear();
                	//RenderRemoteGearHud();
                }
                else
                {
                	DisposeDynamicRemote();
                    //DisposeRemoteGearHud();
                }
            }
            catch { }
        }

        void chkPortalGearEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                mMainSettings.bPortalGearEnabled = e.Checked;
                SaveSettings();
                if (mMainSettings.bPortalGearEnabled)
                {
                	SubscribePortalEvents();
                    RenderPortalGearHud();
                    RenderPortal2GearHud();
                    WriteToChat("PortalGear Enabled.");
                }
                else
                {
                	UnsubscribePortalEvents();
                    DisposePortalGearHud();
                    DisposePortalRecallGearHud();
                    WriteToChat("PortalGear Disabled.");
                }
                RenderDynamicRemoteGear();
            }
            catch { }
        }

        void chkKillTaskGearEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                mMainSettings.bGearTaskerEnabled = e.Checked;
                SaveSettings();
                if (mMainSettings.bGearTaskerEnabled)
                {
                	SubscribeKillTasks();
                	RenderKillTaskPanel();
                	WriteToChat("GearTasker Enabled");
                }
                else
                {
                	UnsubscribeKillTasks();
                    DisposeKillTaskPanel();
                    WriteToChat("GearTasker Disabled");
                }
                RenderDynamicRemoteGear();
            }
            catch { }
        }
        void chkToonStats_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                mMainSettings.btoonStatsEnabled = e.Checked;

                SaveSettings();
            }
            catch (Exception ex) { LogError(ex); }

        }
 
		//Misc Gear Settings
        void chkArmorHud_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                mMainSettings.bArmorHudEnabled = e.Checked;
                
                SaveSettings();
                if (mMainSettings.bArmorHudEnabled) 
                {
                    RenderArmorHud();

                }
                else { DisposeArmorHud(); }
                RenderDynamicRemoteGear();

            }
            catch (Exception ex) { LogError(ex); }

        }


        void getArmorHudSettings()
        {

            try{
                    
                xdocArmorSettings = XDocument.Load(armorSettingsFilename);
                ArmorHudWidth = Convert.ToInt32(xdocArmorSettings.Element("Settings").Element("Setting").Element("ArmorHudWidth").Value);
                 ArmorHudHeight = Convert.ToInt32(xdocArmorSettings.Element("Settings").Element("Setting").Element("ArmorHudHeight").Value);
                 nInventoryHudWidth = Convert.ToInt32(xdocArmorSettings.Element("Settings").Element("Setting").Element("InventoryHudWidth").Value);
                 nInventoryHudHeight = Convert.ToInt32(xdocArmorSettings.Element("Settings").Element("Setting").Element("InventoryHudHeight").Value);
            }
            catch (Exception ex) { LogError(ex); }

        }

        void chkInventoryHudEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                mMainSettings.binventoryHudEnabled = e.Checked;

                SaveSettings();
                if (mMainSettings.binventoryHudEnabled)
                {
                    if (File.Exists(armorSettingsFilename))
                    { getInventoryHudSettings(); }
                    RenderInventoryHud();

                }
                else { DisposeInventoryHud(); }
                RenderDynamicRemoteGear();

            }
            catch (Exception ex) { LogError(ex); }

        }


        void getInventoryHudSettings()
        {

            try
            {
                xdocInventorySettings = XDocument.Load(armorSettingsFilename);
                nInventoryHudWidth = Convert.ToInt32(xdocInventorySettings.Element("Settings").Element("Setting").Element("InventoryHudWidth").Value);
                nInventoryHudHeight = Convert.ToInt32(xdocInventorySettings.Element("Settings").Element("Setting").Element("InventoryHudHeight").Value);
            }
            catch (Exception ex) { LogError(ex); }

        }


       // settings are stored in ArmorSettings.xml for both Armor hud and Inventory hud

        private void SaveArmorSettings()
        {
            try
            {
                if (armorSettingsFilename == "" || armorSettingsFilename == null) { armorSettingsFilename = GearDir + @"\ArmorSettings.xml"; }
                xdocArmorSettings = new XDocument(new XElement("Settings"));
                xdocArmorSettings.Element("Settings").Add(new XElement("Setting",
                    new XElement("ArmorHudWidth", ArmorHudWidth),
                    new XElement("ArmorHudHeight", ArmorHudHeight),
                    new XElement("InventoryHudWidth", nInventoryHudWidth),
                    new XElement("InventoryHudHeight", nInventoryHudHeight)));


                xdocArmorSettings.Save(armorSettingsFilename);
            }
            catch (Exception ex) { LogError(ex); }

        }

        //Gear Filter Settings
        
        void chkEnableTextFiltering_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bEnableTextFiltering = e.Checked;
                SaveSettings();
                if (bEnableTextFiltering)
                {
                    SubscribeChatEvents();
                }
                else
                {
                    UnsubscribeChatEvents();
                }

            }
            catch { }
        }

        void chkTextFilterAllStatus_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterAllStatus = e.Checked;
                SaveSettings();
                if (bTextFilterAllStatus)
                {
                    SubscribeChatEvents();
                }
                else
                {
                    UnsubscribeChatEvents();
                }

            }
            catch { }
        }


        void txtItemFontHeight_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
            nitemFontHeight = Convert.ToInt32(txtItemFontHeight.Text);
            if (mMainSettings.binventoryHudEnabled)
                RenderInventoryHud();
            if (mMainSettings.bGearTacticianEnabled)
            	RenderTacticianHud();
            if (mMainSettings.bGearVisection)
                RenderCorpseHud();
            if (mMainSettings.bGearSenseHudEnabled)
                RenderLandscapeHud();
            if (mMainSettings.bGearInspectorEnabled)
                RenderItemHud();
            if (mMainSettings.bGearButlerEnabled)
                RenderButlerHud();
            if (mMainSettings.bArmorHudEnabled)
                RenderArmorHud();
            SaveSettings();
        }

        void txtMenuFontHeight_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
            nmenuFontHeight = Convert.ToInt32(txtMenuFontHeight.Text);
            if (mMainSettings.binventoryHudEnabled)
                RenderInventoryHud();
            if (mMainSettings.bGearTacticianEnabled)
            	RenderTacticianHud();
            if (mMainSettings.bGearVisection)
                RenderCorpseHud();
            if (mMainSettings.bGearSenseHudEnabled)
                RenderLandscapeHud();
            if (mMainSettings.bGearInspectorEnabled)
                RenderItemHud();
            if (mMainSettings.bGearButlerEnabled)
                RenderButlerHud();
            if (mMainSettings.bArmorHudEnabled)
                RenderArmorHud();
            SaveSettings();
        }


        private void SaveSettings()
        {
            try
            {
                XDocument xdocGeneralSet = new XDocument(new XElement("Settings"));
                xdocGeneralSet.Element("Settings").Add(new XElement("Setting",
                        new XElement("CorpseHudEnabled", mMainSettings.bGearVisection),
                         new XElement("LandscapeHudEnabled", mMainSettings.bGearSenseHudEnabled),
                         new XElement("InspectorHudEnabled", mMainSettings.bGearInspectorEnabled),
                         new XElement("ButlerHudEnabled", mMainSettings.bGearButlerEnabled),
                         new XElement("CombatHudEnabled", mMainSettings.bGearTacticianEnabled),
                         new XElement("RemoteGearEnabled", mMainSettings.bRemoteGearEnabled),
                         new XElement("PortalGearEnabled", mMainSettings.bPortalGearEnabled),
                         new XElement("KillTaskGearEnabled", mMainSettings.bGearTaskerEnabled),
                         new XElement("QuickSlotsvEnabled", mMainSettings.bquickSlotsvEnabled),
                         new XElement("QuickSlotshEnabled", mMainSettings.bquickSlotshEnabled),
                         new XElement("InventoryHudEnabled", mMainSettings.binventoryHudEnabled),
                         new XElement("InventoryEnabled", mMainSettings.binventoryEnabled),
                         new XElement("InventoryCompleteEnabled", mMainSettings.binventoryCompleteEnabled),
                         new XElement("ToonStatsEnabled", mMainSettings.btoonStatsEnabled),
                        new XElement("ArmorHudEnabled", mMainSettings.bArmorHudEnabled),
                         new XElement("EnableTextFiltering", bEnableTextFiltering),
                         new XElement("TextFilterAllStatus", bTextFilterAllStatus),
                         new XElement("ItemFontHeight", nitemFontHeight),
                         new XElement("MenuFontHeight", nmenuFontHeight)));

               xdocGeneralSet.Save(genSettingsFilename);
               xdocGeneralSet = null;

            }
            catch (Exception ex) { LogError(ex); }

        }
        
        private XElement CreateRulesXElement()
        {
        	
        	    List<int> NumList = new List<int>();
           		foreach(XElement xe in mPrioritizedRulesList)
           		{
           			NumList.Add(Convert.ToInt32(xe.Element("RuleNum").Value));
           		}
           		
           		int NewRuleNumber = 0;
           		for(NewRuleNumber = 0; NewRuleNumber < NumList.Count; )
           		{
           			if(!NumList.Contains(NewRuleNumber)){break;}
           			else{NewRuleNumber++;}
           		}

        		return new XElement("Rule",
           		                    new XElement("RuleNum", NewRuleNumber.ToString()),
        		                    new XElement("Enabled", "false"),
        		                    new XElement("Priority", "999"),
        		                    new XElement("AppliesToFlag", String.Empty),
        		                    new XElement("Name", "New Rule " + NewRuleNumber.ToString()),
        		                    new XElement("ArcaneLore", "-1"),
        		                    new XElement("Work", "-1"),
        		                    new XElement("WieldLevel", "-1"),
        		                    new XElement("WieldSkill", "0"),
        		                    new XElement("MasteryType", "0"),
        		                    new XElement("DamageType", "0"),
        		                    new XElement("GearScore", "-1"),
        		                    new XElement("WieldEnabled", "false,false,false,false"),
        		                    new XElement("ReqSkill", "-1,-1,-1,-1"),
        		                    new XElement("Slots", String.Empty),
        		                    new XElement("ArmorType", String.Empty),
        		                    new XElement("ArmorSet", String.Empty),
        		                    new XElement("Spells", String.Empty),
        		                    new XElement("NumSpells", "-1"),
        		                    new XElement("Advanced", "false"),
        		                    new XElement("Palettes", String.Empty));

        }
        
    }
}


