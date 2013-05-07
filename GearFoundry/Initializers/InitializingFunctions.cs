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
                        new XElement("QuickSlotsvEnabled", bquickSlotsvEnabled),
                        new XElement("QuickSlotshEnabled", bquickSlotshEnabled)));
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
            //Directory for the current world in Alinco Directory
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
        }

        public void loadLists()
        {
            try
            {  
            	setUpLists(xdocMobs, mSortedMobsList, mSortedMobsListChecked);
                setUpLists(xdocTrophies, mSortedTrophiesList, mSortedTrophiesListChecked);
                setUpLists(xdocSalvage, mSortedSalvageList, mSortedSalvageListChecked);
                setUpRulesLists(xdocRules, mPrioritizedRulesList, mPrioritizedRulesListEnabled);
                //Irq:  Builds class mirror lists of Mish's xdocs at load time.
                FillSalvageRules();
                FillItemRules();
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

        public void setUpLists(XDocument xdoc, List<XElement> sorted, List<XElement> enabled)
        {
            sorted.Clear();
            enabled.Clear();
            IEnumerable<XElement> myelements = xdoc.Element("GameItems").Descendants("item");

            var lstChecked = from element in myelements
                                            where Convert.ToBoolean(element.Element("checked").Value)
                                            orderby element.Element("key").Value ascending

                                            select element;
            enabled.AddRange(lstChecked);
            sorted.AddRange(enabled);

            var lstUnChecked = from element in myelements
                                              where !Convert.ToBoolean(element.Element("checked").Value)
                                              orderby element.Element("key").Value ascending

                                              select element;

            sorted.AddRange(lstUnChecked);

        }




        public void setUpRulesLists(XDocument xdoc, List<XElement> sorted, List<XElement> enabled)
        {
            try
            {
                sorted.Clear();
                enabled.Clear();

                
                //TODO:  Null reference?  Why?
                IEnumerable<XElement> myelements = xdocRules.Element("Rules").Descendants("Rule");
                int n = myelements.Count();
                 foreach (XElement el in myelements)
                {
                    int num = Convert.ToInt32(el.Element("Priority").Value);
                    if (num == 0)
                        el.Element("Priority").Value = "999";
                }

                var lstChecked = from element in myelements
                                 where Convert.ToBoolean(element.Element("Enabled").Value)
                                 orderby Convert.ToInt32(element.Element("Priority").Value) ascending

                                 select element;
                enabled.AddRange(lstChecked);

                sorted.AddRange(lstChecked);


                var lstUnChecked = from element in myelements
                                   where !Convert.ToBoolean(element.Element("Enabled").Value)
                                   orderby Convert.ToInt32(element.Element("Priority").Value) ascending
                                   select element;

                 sorted.AddRange(lstUnChecked);

            }
            catch (Exception ex) { LogError(ex); }


        }

        //Function clears all rules variables from previous rule
        private void initRulesVariables()
        {
            bRuleEnabled = false;
            nRulePriority = 0;
            sRuleAppliesTo = "";
            sRuleName = "";
            sRuleKeyWords = "";
            sRuleKeyWordsNot = "";
            sRuleDescr = "";
            nRuleArcaneLore = 0;
            nRuleBurden = 0;
            nRuleValue = 0;
            nRuleWork = 0;
            nRuleWieldReqValue = 0;
            nRuleItemLevel = 0;
            nRuleMinArmorLevel = 0;
            nRuleWieldAttribute = 0;
            nRuleMasteryType = 0;
            sRuleDamageTypes = "";
            nRuleMcModAttack = 0;
            nRuleMeleeD = 0;
            nRuleMagicD = 0;
            sRuleReqSkill = "";
            sRuleReqSkilla = "";
            sRuleReqSkillb = "";
            sRuleReqSkillc = "";
            sRuleReqSkilld = "";
            sRuleMinMax = "";
            sRuleMinMaxa = "";
            sRuleMinMaxb = "";
            sRuleMinMaxc = "";
            sRuleMinMaxd = "";
            sRuleWeapons = "";
            sRuleWeaponsa = "false";
            sRuleWeaponsb = "false";
            sRuleWeaponsc = "false";
            sRuleWeaponsd = "false";
            sRuleMSCleave = "";
            sRuleMSCleavea = "false";
            sRuleMSCleaveb = "false";
            sRuleMSCleavec = "false";
            sRuleMSCleaved = "false";
            sRuleArmorType = "";
            //bRuleMustBeSet = false;
            //bRuleAnySet = false;
            sRuleArmorSet = "";
            sRuleArmorCoverage = "";
            bRuleMustBeUnEnchantable = false;
            nRuleMustHaveSpell = 0;
            sRuleCloakSets = "";
            sRuleCloakSpells = "";
            bRuleRed = false;
            bRuleYellow = false;
            bRuleBlue = false;
            bRuleFilterLegend = false;
            bRuleFilterEpic = false;
            bRuleFilterMajor = false;
            bRuleFilterlvl8 = false;
            bRuleFilterlvl7 = false;
            bRuleFilterlvl6 = false;
            sRuleDamageTypes = "";
            sRuleSpells = "";
            nRuleNumSpells = 0;

        }

 
        private void fillSettingsVariables()
        {
            try
            {

                foreach (XElement el in mGenSettingsList)
                {
                    if (el.Name == "CorpseHudEnabled") { bCorpseHudEnabled = Convert.ToBoolean(el.Value); }
                    if (el.Name == "LandscapeHudEnabled") { bLandscapeHudEnabled = Convert.ToBoolean(el.Value); }
                    if (el.Name == "InspectorHudEnabled") { bGearInspectorEnabled = Convert.ToBoolean(el.Value); }
                    if (el.Name == "ButlerHudEnabled") { bGearButlerEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "ToonKillsEnabled") { btoonKillsEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "ToonCorpsesEnabled") { btoonCorpsesEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "VulnedIconsEnabled") { bvulnedIconsEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "PortalsEnabled") { bportalsEnabled = Convert.ToBoolean(el.Value); }
                    if (el.Name == "QuickSlotsvEnabled") { bquickSlotsvEnabled = Convert.ToBoolean(el.Value); }
                    if (el.Name == "QuickSlotshEnabled") { bquickSlotshEnabled = Convert.ToBoolean(el.Value); }
                    if (el.Name == "InventoryEnabled") { binventoryEnabled = Convert.ToBoolean(el.Value); }
                    if (el.Name == "InventoryBurdenEnabled") { binventoryBurdenEnabled = Convert.ToBoolean(el.Value); }
                    if (el.Name == "InventoryCompleteEnabled") { binventoryCompleteEnabled = Convert.ToBoolean(el.Value); }
                  //  if (el.Name == "SalvageCombEnabled") { bsalvageCombEnabled = Convert.ToBoolean(el.Value); }
                    if (el.Name == "ToonStatsEnabled") { btoonStatsEnabled = Convert.ToBoolean(el.Value); }
                 //   if (el.Name == "ToonArmorEnabled") { btoonArmorEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "MuteEnabled") { bmuteEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "FullScreenEnabled") { bfullScreenEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "Scrolls7Enabled") { bscrolls7Enabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "Scrolls7TndEnabled") { bscrolls7TndEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "AllScrollsEnabled") { ballScrollsEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "AllPlayersEnabled") { ballPlayersEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "AllegEnabled") { ballegEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "FellowEnabled") { bfellowEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "SelectedMobsEnabled") { bselectedMobsEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "TellsEnabled") { btellsEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "EvadesEnabled") { bevadesEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "ResistsEnabled") { bresistsEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "SpellCastingEnabled") { bspellCastingEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "SpellsExpireEnabled") { bspellsExpireEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "VendorTellsEnabled") { bvendorTellsEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "StackingEnabled") { bstackingEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "PickupEnabled") { bpickupEnabled = Convert.ToBoolean(el.Value); }
                    //if (el.Name == "UstEnabled") { bustEnabled = Convert.ToBoolean(el.Value); }

                }


                    chkQuickSlotsv.Checked = bquickSlotsvEnabled;
                    chkQuickSlotsh.Checked = bquickSlotshEnabled;
                    
                    //GearVisection Section                   
                    chkGearVisectionEnabled.Checked = bCorpseHudEnabled;

                    //chkToonKills.Checked = btoonKillsEnabled;
                    //chkFellowKills.Checked = bFellowKillsEnabled;
                    //chkToonCorpses.Checked = btoonCorpsesEnabled;
                    //chkPermittedCorpses.Checked = bPermittedCorpses;
                    
                    //GearSense Section
                    chkGearSenseEnabled.Checked = bLandscapeHudEnabled;
                    //chkAllMobs.Checked = bShowAllMobs; 
                    //chkSelectedMobs.Checked = bselectedMobsEnabled;
                    //chkAllNPCs.Checked = bShowAllNPCs;
                    //chkSelectedTrophies.Checked = bLandscapeTrophiesEnabled;
                    //chkAllPlayers.Checked = ballPlayersEnabled;
                    //chkAllegPlayers.Checked = ballegEnabled;
                    //chkFellow.Checked = bfellowEnabled;
                    //chkPortals.Checked = bportalsEnabled;
                    //chkLifestones.Checked = bLandscapeLifestonesEnabled;

                    //GearInspector Section
                    chkGearInspectorEnabled.Checked = bGearInspectorEnabled;
                    
                   //GearButler Section
                     chkGearButlerEnabled.Checked = bGearButlerEnabled;


                  //  chkVulnedIcons.Checked = bvulnedIconsEnabled;
                    //chkSelectedMobs.Checked = bselectedMobsEnabled;
                    //chkPortals.Checked = bportalsEnabled;
                    chkInventory.Checked = binventoryEnabled;
                    chkInventoryBurden.Checked = binventoryBurdenEnabled;
                    chkInventoryComplete.Checked = binventoryCompleteEnabled;
                  //  chkSalvageComb.Checked = bsalvageCombEnabled;
                    chkToonStats.Checked = btoonStatsEnabled;
                 //   chkToonArmor.Checked = btoonArmorEnabled;
                    //chk3DArrow.Checked = b3DArrowEnabled;
                  //  chkMute.Checked = bmuteEnabled;
                  //  chkFullScreen.Checked = bfullScreenEnabled;
                   // chkScrolls7.Checked = bscrolls7Enabled;
                   // chkScrolls7Tnd.Checked = bscrolls7TndEnabled;
                   // chkAllScrolls.Checked = ballScrollsEnabled;
                    //chkAllPlayers.Checked = ballPlayersEnabled;
                    //chkAllegPlayers.Checked = ballegEnabled;
                    //chkFellow.Checked = bfellowEnabled;
                   // chkTells.Checked = btellsEnabled;
                    //chkEvades.Checked = bevadesEnabled;
                    //chkResists.Checked = bresistsEnabled;
                    //chkSpellCasting.Checked = bspellCastingEnabled;
                    //chkSpellsExpire.Checked = bspellsExpireEnabled;
                    //chkVendorTells.Checked = bvendorTellsEnabled;
                    //chkStacking.Checked = bstackingEnabled;
                    //chkPickup.Checked = bpickupEnabled;
                    //chkUst.Checked = bustEnabled;
                
                }
            catch (Exception ex) { LogError(ex); }
        
        }

        private void startRoutines()
        {
            if (bLandscapeHudEnabled)
            {
                RenderLandscapeHud();
            }

            if (bCorpseHudEnabled)
            {
                RenderCorpseHud();
            }

            if (bGearInspectorEnabled)
            {
                RenderItemHud();
            }

            if (bGearButlerEnabled)
            {
                RenderButlerHud();
            }

            if (binventoryCompleteEnabled)
            {
                binventoryBurdenEnabled = false;
                binventoryEnabled = false;
                m = 500;
                doGetInventory();

            }
            if (binventoryBurdenEnabled)
            {
                binventoryEnabled = false;
                getBurden = true;
                doUpdateInventory();
            }
            if (binventoryEnabled)
            { doUpdateInventory(); }

            if (btoonStatsEnabled)
            { getStats(); }

            if (bquickSlotsvEnabled)
            {

                RenderVerticalQuickSlots(); 
            }

            if (bquickSlotshEnabled)
            {
                RenderHorizontalQuickSlots(); 
            }

            

        }


        //Removes any previous rule data from textboxes and checkboxes and adds current rule data
        private void initRulesCtrls()
        {

            chkRuleEnabled.Checked = bRuleEnabled;
            txtRulePriority.Text = nRulePriority.ToString();
            txtRuleName.Text = sRuleName;
            txtRuleKeywords.Text = sRuleKeyWords;
            txtRuleKeyWordsNot.Text = sRuleKeyWordsNot;
            txtRuleDescr.Text = sRuleDescr;
            txtRuleArcaneLore.Text = nRuleArcaneLore.ToString();
            txtRulePrice.Text = nRuleValue.ToString();
            txtRuleMaxBurden.Text = nRuleBurden.ToString();
            txtRuleMaxCraft.Text = nRuleWork.ToString();
            txtRuleWieldReqValue.Text = nRuleWieldReqValue.ToString();
            txtRuleWieldLevel.Text = nRuleWieldLevel.ToString();
            txtRuleItemLevel.Text = nRuleItemLevel.ToString();
            //chkRuleTradeBot.Checked = bRuleTradeBot;
            //chkRuleTradeBotOnly.Checked = bRuleTradeBotOnly;
            try
            {
                int i=0;
                foreach (IDNameLoadable item in WeaponTypeList)
                {

                    if (item.ID == nRuleWieldAttribute)
                    {
                        cboWeaponAppliesTo.Selected = i;
                         break;
                    }
                    i++;
                }
            }
            catch (Exception ex) { LogError(ex); }

            cboMasteryType.Selected = nRuleMasteryType;
            txtRuleMcModAttack.Text = nRuleMcModAttack.ToString();
            txtRuleMeleeD.Text = nRuleMeleeD.ToString();
            txtRuleMagicD.Text = nRuleMagicD.ToString();
            chkRuleWeaponsa.Checked = Convert.ToBoolean(sRuleWeaponsa);
            chkRuleWeaponsb.Checked = Convert.ToBoolean(sRuleWeaponsb);
            chkRuleWeaponsc.Checked = Convert.ToBoolean(sRuleWeaponsc);
            chkRuleWeaponsd.Checked = Convert.ToBoolean(sRuleWeaponsd);
            chkRuleMSCleavea.Checked = Convert.ToBoolean(sRuleMSCleavea);
            chkRuleMSCleaveb.Checked = Convert.ToBoolean(sRuleMSCleaveb);
            chkRuleMSCleavec.Checked = Convert.ToBoolean(sRuleMSCleavec);
            chkRuleMSCleaved.Checked = Convert.ToBoolean(sRuleMSCleaved);
              
            txtRuleReqSkilla.Text = sRuleReqSkilla;
             txtRuleReqSkillb.Text = sRuleReqSkillb;
            txtRuleReqSkillc.Text = sRuleReqSkillc;
            txtRuleReqSkilld.Text = sRuleReqSkilld;
            txtRuleMinMaxa.Text = sRuleMinMaxa;
            txtRuleMinMaxb.Text = sRuleMinMaxb;
            txtRuleMinMaxc.Text = sRuleMinMaxc;
            txtRuleMinMaxd.Text = sRuleMinMaxd;
            chkRuleMustBeUnenchantable.Checked = bRuleMustBeUnEnchantable;
            chkRuleRed.Checked = bRuleRed;
            chkRuleYellow.Checked = bRuleYellow;
            chkRuleBlue.Checked = bRuleBlue;
            
            //chkRuleAnySet.Checked = bRuleAnySet;
            //chkRuleMustBeSet.Checked = bRuleMustBeSet;
            if (nRuleMustHaveSpell > 0)
            {
                getSpellName(nRuleMustHaveSpell.ToString());
                txtRuleSpellMatches.Text = sname;
            }
            else { txtRuleSpellMatches.Text = ""; }

            chkRuleFilterLegend.Checked = bRuleFilterLegend;
            chkRuleFilterEpic.Checked = bRuleFilterEpic;
            chkRuleFilterMajor.Checked = bRuleFilterMajor;
            chkRuleFilterlvl8.Checked = bRuleFilterlvl8;
            chkRuleFilterlvl7.Checked = bRuleFilterlvl7;
            chkRuleFilterlvl6.Checked = bRuleFilterlvl6;
            populateSpellListBox();
        }



        void chkInventory_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                binventoryEnabled = e.Checked;

                SaveSettings();
            }
            catch (Exception ex) { LogError(ex); }

        }

        void chkInventoryBurden_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                binventoryBurdenEnabled = e.Checked;

                SaveSettings();
            }
            catch (Exception ex) { LogError(ex); }

        }

        void chkInventoryComplete_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                binventoryCompleteEnabled = e.Checked;

                SaveSettings();
            }
            catch (Exception ex) { LogError(ex); }

        }


        void chkInventoryWaiting_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                binventoryWaitingEnabled = e.Checked;

                //   SaveSettings();
            }
            catch (Exception ex) { LogError(ex); }

        }



        void chkQuickSlotsv_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bquickSlotsvEnabled = e.Checked;


                SaveSettings();

                if (bquickSlotsvEnabled)
                {
                	RenderVerticalQuickSlots();
                    //quickSlotsvEnabled();
                }
                else
                {
                	DisposeVerticalQuickSlots();
//                    quickSlotsvNotEnabled();
                }

            }
            catch (Exception ex) { LogError(ex); }

        }

        void chkQuickSlotsh_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bquickSlotshEnabled = e.Checked;

                SaveSettings();

                if (bquickSlotshEnabled)
                {
                    RenderHorizontalQuickSlots();
                }
                else if (!bquickSlotshEnabled)
                {
                    DisposeHorizontalQuickSlots();
                }

            }
            catch (Exception ex) { LogError(ex); }

        }


		//Gear Hound Contols	
		void chkGearVisectionEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
		{
			try
			{
				bCorpseHudEnabled = e.Checked;
				SaveSettings();
				if(e.Checked)
				{
					RenderCorpseHud();
				}
				else
				{
					DisposeCorpseHud();
				}
			}
			catch(Exception ex){LogError(ex);}
			
		}

        //void chkToonKills_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    WriteToChat("Checkbox works");
        //    try
        //    {
        //        btoonKillsEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}
        
        //void chkFellowKills_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    WriteToChat("Checkbox works");
        //    try{
        		
        	
        //    bFellowKillsEnabled = e.Checked;
        //    SaveSettings();
        //    }catch{}
        //}

        //void chkToonCorpses_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    WriteToChat("Checkbox works");
        //    try
        //    {
        //        btoonCorpsesEnabled = e.Checked;

        //        SaveSettings();
        //   }
        //    catch (Exception ex) { LogError(ex); }

        //}
        
        //void chkPermittedCorpses_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    WriteToChat("Checkbox works");
        //    try{
        //        bPermittedCorpses = e.Checked;
        //        SaveSettings();
        //    }catch{}
        //}
        
        ////GearSense  Controls
        
        void chkGearSenseEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
        	try
        	{
        		bLandscapeHudEnabled = e.Checked;
				SaveSettings();
				if(e.Checked)
				{
					RenderLandscapeHud();
				}
				else
				{
					DisposeLandscapeHud();
				}	
        	}catch{}
        }
        
        //void chkAllMobs_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {

        //    }catch{}
        //}
        
        //void chkSelectedMobs_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
	
        //    }catch{}
        //}
        
        //void chkAllNPCs_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {

        //    }catch{}
        //}
        
        //void chkSelectedTrophies_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {

        //    }catch{}
        //}
        
        //void chkAllPlayers_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {

        //    }catch{}
        //}
        
        //void chkAllegPlayers_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {

        //    }catch{}
        //}
        
        //void chkFellow_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
	
        //    }catch{}
        //}
        
        //void chkPortals_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {

        //    }catch{}
        //}
        
        //void chkLifestones_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {

        //    }catch{}
        //}
        
        //GearButler Settings
        void chkGearButlerEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
        	try
        	{
        		bGearButlerEnabled = e.Checked;
        		SaveSettings();
        		if(bGearButlerEnabled)
        		{
        			RenderButlerHud();
        		}
        		else
        		{
        			DisposeButlerHud();
        		}

        	}catch{}
        }
     
        //void chkAutoRingKeys_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {

        //    }catch{}
        //}
        


        void chkGearInspectorEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bGearInspectorEnabled = e.Checked;
                SaveSettings();
                if (e.Checked)
                {
                    RenderItemHud();
                }
                else
                {
                    DisposeItemHud();
                }
            }
            catch { }
        }

        //void chkVulnedIcons_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}


        //void chkSalvageComb_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}
        void chkToonStats_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                btoonStatsEnabled = e.Checked;

                SaveSettings();
            }
            catch (Exception ex) { LogError(ex); }

        }
        void chkToonArmor_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
              //  btoonArmorEnabled = e.Checked;

                SaveSettings();
            }
            catch (Exception ex) { LogError(ex); }

        }
        //void chk3DArrow_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        b3DArrowEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}
        //void chkMute_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        bmuteEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}
        //void chkFullScreen_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        bfullScreenEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}
        //void chkScrolls7_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        bscrolls7Enabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}
        //void chkScrolls7Tnd_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        bscrolls7TndEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}
        //void chkAllScrolls_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        ballScrollsEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}


        //void chkAllegPlayers_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        ballegEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

       // }

        //void chkTells_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        btellsEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}

        //void chkEvades_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        bevadesEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}
        //void chkResists_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        bresistsEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}
        //void chkSpellCasting_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        bspellCastingEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}
        //void chkSpellsExpire_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        bspellsExpireEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}
        ////void chkVendorTells_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        bvendorTellsEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}
        //void chkStacking_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        bstackingEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}

        //void chkPickup_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        bpickupEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}
        //void chkUst_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        //{
        //    try
        //    {
        //        bustEnabled = e.Checked;

        //        SaveSettings();
        //    }
        //    catch (Exception ex) { LogError(ex); }

        //}


        private void SaveSettings()
        {
            try
            {
                xdoc = new XDocument(new XElement("Settings"));
                xdoc.Element("Settings").Add(new XElement("Setting",
                         new XElement("CorpseHudEnabled", bCorpseHudEnabled),
                         new XElement("LandscapeHudEnabled", bLandscapeHudEnabled),
                         new XElement("InspectorHudEnabled", bGearInspectorEnabled),
                         new XElement("ButlerHudEnabled", bGearButlerEnabled),
                    //new XElement("ToonKillsEnabled", btoonKillsEnabled),
                    //new XElement("ToonCorpsesEnabled", btoonCorpsesEnabled),
                         //new XElement("VulnedIconsEnabled", bvulnedIconsEnabled),
                         //new XElement("PortalsEnabled", bportalsEnabled),
                         new XElement("QuickSlotsvEnabled", bquickSlotsvEnabled),
                         new XElement("QuickSlotshEnabled", bquickSlotshEnabled),
                         new XElement("InventoryEnabled", binventoryEnabled),
                         new XElement("InventoryBurdenEnabled", binventoryBurdenEnabled),
                         new XElement("InventoryCompleteEnabled", binventoryCompleteEnabled),
                    //   new XElement("SalvageCombEnabled", bsalvageCombEnabled),
                         new XElement("ToonStatsEnabled", btoonStatsEnabled)));
                      //   new XElement("ToonArmorEnabled", btoonArmorEnabled),
                        // new XElement("3DArrowEnabled", b3DArrowEnabled)));
                         //new XElement("MuteEnabled", bmuteEnabled),
                         //new XElement("FullScreenEnabled", bfullScreenEnabled),
                         //new XElement("Scrolls7Enabled", bscrolls7Enabled),
                         //new XElement("Scrolls7TndEnabled", bscrolls7TndEnabled),
                         //new XElement("AllScrollsEnabled", ballScrollsEnabled),
                         //new XElement("AllPlayersEnabled", ballPlayersEnabled),
                         //new XElement("AllegEnabled", ballegEnabled),
                         //new XElement("FellowEnabled", bfellowEnabled),
                         //new XElement("SelectedMobsEnabled", bselectedMobsEnabled),
                         //new XElement("TellsEnabled", btellsEnabled),
                         //new XElement("EvadesEnabled", bevadesEnabled),
                         //new XElement("ResistsEnabled", bresistsEnabled),
                         //new XElement("SpellCastingEnabled", bspellCastingEnabled),
                         //new XElement("SpellsExpireEnabled", bspellsExpireEnabled),
                         // new XElement("VendorTellsEnabled", bvendorTellsEnabled),
                         //new XElement("StackingEnabled", bstackingEnabled),
                         //new XElement("PickupEnabled", bpickupEnabled),
                         //new XElement("UstEnabled", bustEnabled)));
                xdoc.Save(genSettingsFilename);

            }
            catch (Exception ex) { LogError(ex); }

        }

    }
}



