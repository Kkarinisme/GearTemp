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
          //  RedoSalvageFile();
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
                setUpSettingsList();
                FillSalvageRules();
                FillItemRules();
                
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

                IEnumerable<XElement> myelements = xdocRules.Element("Rules").Descendants("Rule");
                int n = myelements.Count();
                nNextRuleNum = n;

                foreach (XElement el in myelements)
                {

                    int num = Convert.ToInt32(el.Element("Priority").Value);
                    if (num == 0) { el.Element("Priority").Value = "999"; }
                    int rulenum = Convert.ToInt32(el.Element("RuleNum").Value);
                    if(rulenum>nNextRuleNum){nNextRuleNum=rulenum;}
                }
                //Will be number of next rule
                nNextRuleNum++;
 

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
            nRuleNum = 0;
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
            sRuleArmorSet = "";
            sRuleArmorCoverage = "";
            bRuleMustBeUnEnchantable = false;
            sRuleCloakSets = "";
            sRuleCloakSpells = "";
            bRuleRed = false;
            bRuleYellow = false;
            bRuleBlue = false;
            bRuleCloakMustHaveSpell = false;
            nRuleEssCDLevel = 0;
            nRuleEssCritLevel = 0;
            nRuleEssCritDamResLevel = 0;
            nRuleEssCRLevel = 0;
            nRuleEssDamageLevel = 0;
            nRuleEssDRLevel = 0;
            nRuleEssLevel = 0;
            nRuleEssMastery = 0;
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
                xdoc = XDocument.Load(genSettingsFilename);
                
                XElement el = xdoc.Root.Element("Setting");

                bCorpseHudEnabled = Convert.ToBoolean(el.Element("CorpseHudEnabled").Value);
               bLandscapeHudEnabled = Convert.ToBoolean(el.Element("LandscapeHudEnabled").Value);
                bGearInspectorEnabled = Convert.ToBoolean(el.Element("InspectorHudEnabled").Value);
                bGearButlerEnabled = Convert.ToBoolean(el.Element("ButlerHudEnabled").Value);
               bCombatHudEnabled = Convert.ToBoolean(el.Element("CombatHudEnabled").Value);
               bRemoteGearEnabled = Convert.ToBoolean(el.Element("RemoteGearEnabled").Value);
               bPortalGearEnabled = Convert.ToBoolean(el.Element("PortalGearEnabled").Value);
               bquickSlotsvEnabled = Convert.ToBoolean(el.Element("QuickSlotsvEnabled").Value);
                bquickSlotshEnabled = Convert.ToBoolean(el.Element("QuickSlotshEnabled").Value);
                binventoryHudEnabled = Convert.ToBoolean(el.Element("InventoryHudEnabled").Value);
                binventoryEnabled = Convert.ToBoolean(el.Element("InventoryEnabled").Value);
                binventoryCompleteEnabled = Convert.ToBoolean(el.Element("InventoryCompleteEnabled").Value);
                btoonStatsEnabled = Convert.ToBoolean(el.Element("ToonStatsEnabled").Value);
                btoonArmorEnabled = Convert.ToBoolean(el.Element("ToonArmorEnabled").Value);
                bArmorHudEnabled = Convert.ToBoolean(el.Element("ArmorHudEnabled").Value);
                bMuteSounds = Convert.ToBoolean(el.Element("MuteSounds").Value);
                bEnableTextFiltering = Convert.ToBoolean(el.Element("EnableTextFiltering").Value);
                bTextFilterAllStatus = Convert.ToBoolean(el.Element("TextFilterAllStatus").Value);
                bTextFilterBusyStatus = Convert.ToBoolean(el.Element("TextFilterBusyStatus").Value);
                bTextFilterCastingStatus = Convert.ToBoolean(el.Element("TextFilterCastingStatus").Value);
                bTextFilterMyDefenseMessages = Convert.ToBoolean(el.Element("TextFilterMyDefenseMessages").Value);
                bTextFilterMobDefenseMessages = Convert.ToBoolean(el.Element("TextFilterMobDefenseMessages").Value);
                bTextFilterMyKillMessages = Convert.ToBoolean(el.Element("TextFilterMyKillMessages").Value);
                bTextFilterPKFails = Convert.ToBoolean(el.Element("TextFilterPKFails").Value);
                bTextFilterDirtyFighting = Convert.ToBoolean(el.Element("TextFilterDirtyFighting").Value);
                bTextFilterMySpellCasting = Convert.ToBoolean(el.Element("TextFilterMySpellCasting").Value);
                bTextFilterOthersSpellCasting = Convert.ToBoolean(el.Element("TextFilterOthersSpellCasting").Value);
                bTextFilterSpellExpirations = Convert.ToBoolean(el.Element("TextFilterSpellExpirations").Value);
                bTextFilterManaStoneMessages = Convert.ToBoolean(el.Element("TextFilterManaStoneMessages").Value);
                bTextFilterHealingMessages = Convert.ToBoolean(el.Element("TextFilterHealingMessages").Value);
                bTextFilterSalvageMessages = Convert.ToBoolean(el.Element("TextFilterSalvageMessages").Value);
                bTextFilterBotSpam = Convert.ToBoolean(el.Element("TextFilterBotSpam").Value);
                bTextFilterIdentFailures = Convert.ToBoolean(el.Element("TextFilterIdentFailures").Value);
                bTextFilterKillTaskComplete = Convert.ToBoolean(el.Element("TextFilterKillTaskComplete").Value);
                bTextFilterVendorTells = Convert.ToBoolean(el.Element("TextFilterVendorTells").Value);
                bTextFilterMonsterTells = Convert.ToBoolean(el.Element("TextFilterMonsterTells").Value);
                bTextFilterNPCChatter = Convert.ToBoolean(el.Element("TextFilterNPCChatter").Value);
                nitemFontHeight = Convert.ToInt32(el.Element("ItemFontHeight").Value);
                nmenuFontHeight = Convert.ToInt32(el.Element("MenuFontHeight").Value);


                if (nitemFontHeight == 0) { nitemFontHeight = 10; }
                txtItemFontHeight.Text = nitemFontHeight.ToString();
                if (nmenuFontHeight == 0) { nmenuFontHeight = 10; }
                txtMenuFontHeight.Text = nmenuFontHeight.ToString();

 
                    chkQuickSlotsv.Checked = bquickSlotsvEnabled;
                    chkQuickSlotsh.Checked = bquickSlotshEnabled;
 
                   // GearVisection Section                   
                    chkGearVisectionEnabled.Checked = bCorpseHudEnabled;
                    
                    //GearSense Section
                    chkGearSenseEnabled.Checked = bLandscapeHudEnabled;
                    //GearInspector Section
                    chkGearInspectorEnabled.Checked = bGearInspectorEnabled;
                    
                   //GearButler Section
                   chkGearButlerEnabled.Checked = bGearButlerEnabled;

                   //Gear Tactician Section
                   chkCombatHudEnabled.Checked = bCombatHudEnabled;

                   //RemoteGear 
                   chkRemoteGearEnabled.Checked = bRemoteGearEnabled;
                   
                   //PortalGear 
                   chkPortalGearEnabled.Checked = bPortalGearEnabled;
  
                   //Misc Gears Section
                   chkMuteSounds.Checked = bMuteSounds;

                   //Inventory Section
                   chkInventory.Checked = binventoryEnabled;
                    chkInventoryComplete.Checked = binventoryCompleteEnabled;
                   chkToonStats.Checked = btoonStatsEnabled;
                   chkToonArmor.Checked = btoonArmorEnabled;
                   chkArmorHud.Checked = bArmorHudEnabled;

 

                   //Filter Section
                   chkEnableTextFiltering.Checked = bEnableTextFiltering;
                   chkTextFilterAllStatus.Checked = bTextFilterAllStatus;
                   chkTextFilterBusyStatus.Checked = bTextFilterBusyStatus;
                   chkTextFilterCastingStatus.Checked = bTextFilterCastingStatus;
                   chkTextFilterMyDefenseMessages.Checked = bTextFilterMyDefenseMessages;
                   chkTextFilterMobDefenseMessages.Checked = bTextFilterMobDefenseMessages;
                   chkTextFilterMyKillMessages.Checked = bTextFilterMyKillMessages;
                   chkTextFilterPKFails.Checked = bTextFilterPKFails;
                   chkTextFilterDirtyFighting.Checked = bTextFilterDirtyFighting;
                   chkTextFilterMySpellCasting.Checked = bTextFilterMySpellCasting;
                   chkTextFilterOthersSpellCasting.Checked = bTextFilterOthersSpellCasting;
                   chkTextFilterSpellExpirations.Checked = bTextFilterSpellExpirations;
                   chkTextFilterManaStoneMessages.Checked = bTextFilterManaStoneMessages;
                   chkTextFilterHealingMessages.Checked = bTextFilterHealingMessages;
                   chkTextFilterSalvageMessages.Checked = bTextFilterSalvageMessages;
                   chkTextFilterBotSpam.Checked = bTextFilterBotSpam;
                   chkTextFilterIdentFailures.Checked = bTextFilterIdentFailures;
                   chkTextFilterKillTaskComplete.Checked = bTextFilterKillTaskComplete;
                   chkTextFilterVendorTells.Checked = bTextFilterVendorTells;
                   chkTextFilterMonsterTells.Checked = bTextFilterMonsterTells;
                   chkTextFilterNPCChatter.Checked = bTextFilterNPCChatter;

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

            if (bCombatHudEnabled)
            {
                RenderCombatHud();
            }

            if (bRemoteGearEnabled)
            {
                RenderRemoteGearHud();
            }

            if (bPortalGearEnabled)
            {
                RenderPortalGearHud();
                RenderPortal2GearHud();
            }

            if (binventoryCompleteEnabled)
            {
                binventoryBurdenEnabled = false;
                binventoryEnabled = false;
                m = 500;
                doGetInventory();

            }

            if (btoonArmorEnabled)
            { doGetArmor(); }


            if (bArmorHudEnabled)
            {
                RenderArmorHud(); 
            }

            if (binventoryHudEnabled)
            { RenderInventoryHud(); }



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

            if (bEnableTextFiltering)
            {
                SubscribeChatEvents(); 
            }

            if (bTextFilterAllStatus)
            {
                SubscribeChatEvents();
            }

            if (bTextFilterBusyStatus)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterCastingStatus)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterMyDefenseMessages)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterMobDefenseMessages)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterMyKillMessages)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterPKFails)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterDirtyFighting)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterMySpellCasting)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterOthersSpellCasting)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterSpellExpirations)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterManaStoneMessages)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterHealingMessages)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterSalvageMessages)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterBotSpam)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterIdentFailures)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterKillTaskComplete)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterVendorTells)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterMonsterTells)
            {
                SubscribeChatEvents();
            }
            if (bTextFilterNPCChatter)
            {
                SubscribeChatEvents();
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
            chkRuleCloakMustHaveSpell.Checked = bRuleCloakMustHaveSpell;
            cboRuleEssMastery.Selected = nRuleEssMastery;
            txtRuleEssCDLevel.Text = nRuleEssCDLevel.ToString();
            txtRuleEssCritLevel.Text = nRuleEssCritLevel.ToString();
            txtRuleEssCRLevel.Text = nRuleEssCRLevel.ToString();
            txtRuleEssDamageLevel.Text = nRuleEssDamageLevel.ToString();
            txtRuleEssDRLevel.Text = nRuleEssDRLevel.ToString();
            txtRuleEssLevel.Text = nRuleEssLevel.ToString();
            txtRuleNumSpells.Text = nRuleNumSpells.ToString();

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



        void chkQuickSlotsv_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bquickSlotsvEnabled = e.Checked;


                SaveSettings();

                if (bquickSlotsvEnabled)
                {
                	RenderVerticalQuickSlots();
                }
                else
                {
                	DisposeVerticalQuickSlots();
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

        void chkCombatHudEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bCombatHudEnabled = e.Checked;
                SaveSettings();
                if (bCombatHudEnabled)
                {
                    RenderCombatHud();
                }
                else
                {
                    DisposeCombatHud();
                }
            }
            catch { }
        }

        void chkRemoteGearEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bRemoteGearEnabled = e.Checked;
                SaveSettings();
                if (bRemoteGearEnabled)
                {
                    RenderRemoteGearHud();
                }
                else
                {
                    DisposeRemoteGearHud();
                }
            }
            catch { }
        }

        void chkPortalGearEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bPortalGearEnabled = e.Checked;
                SaveSettings();
                if (bPortalGearEnabled)
                {
                    RenderPortalGearHud();
                }
                else
                {
                    DisposePortalGearHud();
                }
            }
            catch { }
        }

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
                btoonArmorEnabled = e.Checked;
               

                SaveSettings();

            }
            catch (Exception ex) { LogError(ex); }

        }
 
       //Misc Gear Settings
        void chkMuteSounds_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bMuteSounds = e.Checked;


                SaveSettings();

            }
            catch (Exception ex) { LogError(ex); }

        }

        void chkArmorHud_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bArmorHudEnabled = e.Checked;
                
                SaveSettings();
                if (bArmorHudEnabled) 
                {
                    RenderArmorHud();

                }
                else { DisposeArmorHud(); }

            }
            catch (Exception ex) { LogError(ex); }

        }


        void getArmorHudSettings()
        {

            try{
                    
                xdocArmorSettings = XDocument.Load(armorSettingsFilename);
                ArmorHudWidth = Convert.ToInt32(xdocArmorSettings.Element("Settings").Element("Setting").Element("ArmorHudWidth").Value);
                 ArmorHudHeight = Convert.ToInt32(xdocArmorSettings.Element("Settings").Element("Setting").Element("ArmorHudHeight").Value);
                 InventoryHudWidth = Convert.ToInt32(xdocArmorSettings.Element("Settings").Element("Setting").Element("InventoryHudWidth").Value);
                 InventoryHudHeight = Convert.ToInt32(xdocArmorSettings.Element("Settings").Element("Setting").Element("InventoryHudHeight").Value);
            }
            catch (Exception ex) { LogError(ex); }

        }

        void chkInventoryHudEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                binventoryHudEnabled = e.Checked;

                SaveSettings();
                if (binventoryHudEnabled)
                {
                    if (File.Exists(armorSettingsFilename))
                    { getInventoryHudSettings(); }
                    RenderInventoryHud();

                }
                else { DisposeInventoryHud(); }

            }
            catch (Exception ex) { LogError(ex); }

        }


        void getInventoryHudSettings()
        {

            try
            {
                xdocInventorySettings = XDocument.Load(armorSettingsFilename);
                InventoryHudWidth = Convert.ToInt32(xdocInventorySettings.Element("Settings").Element("Setting").Element("InventoryHudWidth").Value);
                InventoryHudHeight = Convert.ToInt32(xdocInventorySettings.Element("Settings").Element("Setting").Element("InventoryHudHeight").Value);
            }
            catch (Exception ex) { LogError(ex); }

        }


       // settings are stored in ArmorSettings.xml for both Armor hud and Inventory hud

        private void SaveArmorSettings()
        {
            try
            {
                if (armorSettingsFilename == "" || armorSettingsFilename == null) { armorSettingsFilename = GearDir + @"\ArmorSettings.xml"; }
                xdoc = new XDocument(new XElement("Settings"));
                xdoc.Element("Settings").Add(new XElement("Setting",
                    new XElement("ArmorHudWidth", ArmorHudWidth),
                    new XElement("ArmorHudHeight", ArmorHudHeight),
                    new XElement("InventoryHudWidth", InventoryHudWidth),
                    new XElement("InventoryHudHeight", InventoryHudHeight)));


                xdoc.Save(armorSettingsFilename);
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

        void chkTextFilterBusyStatus_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterBusyStatus = e.Checked;
                SaveSettings();
                if (bTextFilterBusyStatus)
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

        void chkTextFilterCastingStatus_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterCastingStatus = e.Checked;
                SaveSettings();
                if (bTextFilterCastingStatus)
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

        void chkTextFilterMyDefenseMessages_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterMyDefenseMessages = e.Checked;
                SaveSettings();
                if (bTextFilterMyDefenseMessages)
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

        void chkTextFilterMobDefenseMessages_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterMobDefenseMessages = e.Checked;
                SaveSettings();
                if (bTextFilterMobDefenseMessages)
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

        void chkTextFilterMyKillMessages_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterMyKillMessages = e.Checked;
                SaveSettings();
                if (bTextFilterMyKillMessages)
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


        void chkTextFilterPKFails_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterPKFails = e.Checked;
                SaveSettings();
                if (bTextFilterPKFails)
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

        void chkTextFilterDirtyFighting_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterDirtyFighting = e.Checked;
                SaveSettings();
                if (bTextFilterDirtyFighting)
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

        void chkTextFilterMySpellCasting_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterMySpellCasting = e.Checked;
                SaveSettings();
                if (bTextFilterMySpellCasting)
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

        void chkTextFilterOthersSpellCasting_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterOthersSpellCasting = e.Checked;
                SaveSettings();
                if (bTextFilterOthersSpellCasting)
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

        void chkTextFilterSpellExpirations_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterSpellExpirations = e.Checked;
                SaveSettings();
                if (bTextFilterSpellExpirations)
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

        void chkTextFilterManaStoneMessages_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterManaStoneMessages = e.Checked;
                SaveSettings();
                if (bTextFilterManaStoneMessages)
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

        void chkTextFilterSalvageMessages_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterSalvageMessages = e.Checked;
                SaveSettings();
                if (bTextFilterSalvageMessages)
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
        void chkTextFilterHealingMessages_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterHealingMessages = e.Checked;
                SaveSettings();
                if (bTextFilterHealingMessages)
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

        void chkTextFilterBotSpam_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterBotSpam = e.Checked;
                SaveSettings();
                if (bTextFilterBotSpam)
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

        void chkTextFilterIdentFailures_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterIdentFailures = e.Checked;
                SaveSettings();
                if (bTextFilterIdentFailures)
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

        void chkTextFilterKillTaskComplete_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterKillTaskComplete = e.Checked;
                SaveSettings();
                if (bTextFilterKillTaskComplete)
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
        void chkTextFilterVendorTells_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterVendorTells = e.Checked;
                SaveSettings();
                if (bTextFilterVendorTells)
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
        void chkTextFilterMonsterTells_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterMonsterTells = e.Checked;
                SaveSettings();
                if (bTextFilterMonsterTells)
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

        void chkTextFilterNPCChatter_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                bTextFilterNPCChatter = e.Checked;
                SaveSettings();
                if (bTextFilterNPCChatter)
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
            if (binventoryHudEnabled)
                RenderInventoryHud();
            if (bCombatHudEnabled)
                RenderCombatHud();
            if (bCorpseHudEnabled)
                RenderCorpseHud();
            if (bLandscapeHudEnabled)
                RenderLandscapeHud();
            if (bGearInspectorEnabled)
                RenderItemHud();
            if (bGearButlerEnabled)
                RenderButlerHud();
            if (bArmorHudEnabled)
                RenderArmorHud();
            SaveSettings();
        }

        void txtMenuFontHeight_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
            nmenuFontHeight = Convert.ToInt32(txtMenuFontHeight.Text);
            if (binventoryHudEnabled)
                RenderInventoryHud();
            if (bCombatHudEnabled)
                RenderCombatHud();
            if (bCorpseHudEnabled)
                RenderCorpseHud();
            if (bLandscapeHudEnabled)
                RenderLandscapeHud();
            if (bGearInspectorEnabled)
                RenderItemHud();
            if (bGearButlerEnabled)
                RenderButlerHud();
            if (bArmorHudEnabled)
                RenderArmorHud();
            SaveSettings();
        }


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
                         new XElement("CombatHudEnabled", bCombatHudEnabled),
                         new XElement("RemoteGearEnabled", bRemoteGearEnabled),
                         new XElement("PortalGearEnabled", bPortalGearEnabled),
                         new XElement("QuickSlotsvEnabled", bquickSlotsvEnabled),
                         new XElement("QuickSlotshEnabled", bquickSlotshEnabled),
                         new XElement("InventoryHudEnabled", binventoryHudEnabled),
                         new XElement("InventoryEnabled", binventoryEnabled),
                         new XElement("InventoryCompleteEnabled", binventoryCompleteEnabled),
                         new XElement("ToonStatsEnabled", btoonStatsEnabled),
                         new XElement("ToonArmorEnabled", btoonArmorEnabled),
                         new XElement("ArmorHudEnabled", bArmorHudEnabled),
                          new XElement("MuteSounds", bMuteSounds),
                         new XElement("EnableTextFiltering", bEnableTextFiltering),
                         new XElement("TextFilterAllStatus", bTextFilterAllStatus),
                         new XElement("TextFilterBusyStatus", bTextFilterBusyStatus),
                         new XElement("TextFilterCastingStatus", bTextFilterCastingStatus),
                         new XElement("TextFilterMyDefenseMessages", bTextFilterMyDefenseMessages),
                         new XElement("TextFilterMobDefenseMessages", bTextFilterMobDefenseMessages),
                         new XElement("TextFilterMyKillMessages", bTextFilterMyKillMessages),
                         new XElement("TextFilterPKFails", bTextFilterPKFails),
                         new XElement("TextFilterDirtyFighting", bTextFilterDirtyFighting),
                         new XElement("TextFilterMySpellCasting", bTextFilterMySpellCasting),
                         new XElement("TextFilterOthersSpellCasting", bTextFilterOthersSpellCasting),
                         new XElement("TextFilterSpellExpirations", bTextFilterSpellExpirations),
                         new XElement("TextFilterManaStoneMessages", bTextFilterManaStoneMessages),
                         new XElement("TextFilterHealingMessages", bTextFilterHealingMessages),
                         new XElement("TextFilterSalvageMessages", bTextFilterSalvageMessages),
                         new XElement("TextFilterBotSpam", bTextFilterBotSpam),
                         new XElement("TextFilterIdentFailures", bTextFilterIdentFailures),
                         new XElement("TextFilterKillTaskComplete", bTextFilterKillTaskComplete),
                         new XElement("TextFilterVendorTells", bTextFilterVendorTells),
                         new XElement("TextFilterMonsterTells", bTextFilterMonsterTells),
                         new XElement("TextFilterNPCChatter", bTextFilterNPCChatter),
                         new XElement("ItemFontHeight", nitemFontHeight),
                         new XElement("MenuFontHeight", nmenuFontHeight)));

               xdoc.Save(genSettingsFilename);

            }
            catch (Exception ex) { LogError(ex); }

        }
        
    }
}


