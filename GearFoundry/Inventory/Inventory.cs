﻿using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using WindowsTimer = System.Windows.Forms.Timer;
using Decal.Adapter;
using Decal.Filters;
using Decal.Adapter.Wrappers;
using System.Text;
using VirindiViewService;
using VirindiViewService.Controls;
using MyClasses.MetaViewWrappers;
using System.Drawing;
using System.Drawing.Imaging;



namespace GearFoundry
{
    public partial class PluginCore : PluginBase
    {
        private bool bInventoryMainTab;
        private bool bInventorySettingsTab;
        private int nInventoryHudWidth = 0;
        private int nInventoryHudHeight = 0;
        private int nInventoryHudFirstWidth = 530;
        private int nInventoryHudFirstHeight = 500;
        private int nInventoryHudWidthNew;
        private int nInventoryHudHeightNew;
        private HudTabView InventoryHudTabView = null;
        private HudFixedLayout InventoryHudTabLayout = null;
        private const int nInventoryRemoveCircle = 0x60011F8;
            
        private HudFixedLayout InventoryHudSettings;
        private HudView InventoryHudView;

        private string inventoryFilename = null;
        private string genInventoryFilename = null;
        private string holdingInventoryFilename = null;
        private XDocument xdocInventorySettings;
        
        private XDocument newDoc = new XDocument();
        private XDocument xdocListInv = new XDocument();
        private static bool getBurden = false;
        private string inventorySelect;



        private HudButton btnInventoryUpdate;
        private HudButton btnInventoryComplete;
        private HudButton btnInventoryStacks;
        private HudCombo cboInventoryClasses;
        private HudStaticText lblInventoryClass;
        private HudTextBox txtMyChoice;
        private HudStaticText lblMyChoice;
        private HudStaticText lblWeapons;
        private HudStaticText lblArmor;
        private HudStaticText lblSalvage;
        private HudStaticText lblMelee;
        private HudCombo cboWieldAttrib;
        private HudStaticText lblSet;
        private HudCombo cboArmorSet;
        private HudStaticText lblMaterial;
        private HudCombo cboMaterial;
        private HudStaticText lblDamage;
        private HudCombo cboDamageType;
        private HudStaticText lblArmorWield;
        private HudCombo cboArmorLevel;
        private HudStaticText lblWork;
        private HudCombo cboSalvWork;
        private HudStaticText lblWield;
        private HudCombo cboLevel;
        private HudStaticText lblCovers;
        private HudCombo cboCoverage;
        private HudStaticText lblEmbues;
        private HudCombo cboEmbues;
        private HudButton btnClrInv;
        private HudButton btnLstInv;
        private HudList.HudListRowAccessor InventoryHudListRow = null;
        private HudList lstHudInventory;
     //   private int nInventoryRow;

        //used by both the inventory and armor programs to hold current object being processed
        private WorldObject currentobj;

        private List<string> moldObjsID = new List<string>();
        private List<WorldObject> mWaitingForID;
        private List<WorldObject> mWaitingForArmorID;

        private List<WorldObject> mIdNotNeeded = new List<WorldObject>();
        private List<long> mwaitingforChangedEvent = new List<long>();
        private List<string> mCurrID = new List<string>();
        private List<string> mIcons = new List<string>();

    //    private static WindowsTimer mWaitingForIDTimer = new WindowsTimer();
        private int m = 500;

        //Used in inventory functions
        private static string objSpellXml = null;
        private static string message = null;
        private static string mySelect = null;
        private List<string> lstMySelects = null;
        private static string objSalvWork = "None";
        private static string objMatName = null;
        private static long objEmbueTypeInt = 0;
        private static string objEmbueTypeStr = null;
        private static long objWieldAttrInt = 0;
        private static long objDamageTypeInt = 0;
        private static long objLevelInt = 1;
        private static long objCovers = 0;
        private static string objCoversName = null;
        private static Int32 objIcon;
        private static long objArmorLevel = 1;
        private static long objArmorSet = 0;
        private static long objSet = 0;
        private static string objArmorSetName = null;
        private static long objMat = 0;
        private static long objMagicDamageInt = 0;
        private static string objDamageType = null;
        private static double objDVar = 0;
        private static long objMaxDamLong = 0;
        private static string objMinDam = null;
        private static string objEmbue = null;
        private static string objDamBon = null;
        private static string objElDam = null;
        private static int objGearScore = 0;



        private static string objClassName = "None";
        private static int objClass = 0;
        private string objName = null;
        private static int objID = 0;
        string objProts;
        string objAl;
        string objWork;
        string objTinks;
        string objLevel;
        string objMissD;
        string objManaC;
        string objMagicD;
        string objMelD;
        string objElemvsMons;
        string objMaxDam;
        string objAttack;
        string objBurden;
        string objStack;
        string objWieldAttr;
        string objMastery;
        int objSkillLevel;
        long objEnchant;
        string objToonLevel;
        long objWieldValue;

        string wieldMess;
        string skillMess;
        int objLore;
  




        private void RenderInventoryHud()
        {


            try
            {

               if (InventoryHudView != null)
                {
                    DisposeInventoryHud();
                }
                if (armorSettingsFilename == "" || armorSettingsFilename == null) { armorSettingsFilename = GearDir + @"\ArmorSettings.xml"; }


                if (nInventoryHudWidth == 0)
                {
                    try
                    {
                        getArmorHudSettings();
                    }
                    catch (Exception ex) { LogError(ex); }
 
                }


                if (nInventoryHudWidth == 0) { nInventoryHudWidth = nInventoryHudFirstWidth;  }
                if (nInventoryHudHeight == 0) { nInventoryHudHeight = nInventoryHudFirstHeight; }

                InventoryHudView = new HudView("Gear", nInventoryHudWidth, nInventoryHudHeight, new ACImage(0x6AA5));
                InventoryHudView.UserAlphaChangeable = false;
                InventoryHudView.ShowInBar = false;
                InventoryHudView.UserResizeable = false;
                InventoryHudView.Visible = true;
                InventoryHudView.Ghosted = false;
                InventoryHudView.UserMinimizable = true;
                InventoryHudView.UserClickThroughable = false;
                InventoryHudTabView = new HudTabView();
                InventoryHudView.Controls.HeadControl = InventoryHudTabView;
                InventoryHudTabLayout = new HudFixedLayout();
                InventoryHudTabView.AddTab(InventoryHudTabLayout,"Inventory");
                InventoryHudView.LoadUserSettings();
                InventoryHudSettings = new HudFixedLayout();
                InventoryHudTabView.AddTab(InventoryHudSettings, "Settings");

                InventoryHudTabView.OpenTabChange += InventoryHudTabView_OpenTabChange;
                InventoryHudView.Resize += InventoryHudView_Resize;
                InventoryHudView.UserResizeable = true;
                RenderInventoryTabLayout();
                InventoryHudView.VisibleChanged += InventoryHudView_VisibleChanged;

            }
            catch (Exception ex) { LogError(ex); }
            return;
        }
        
        private void InventoryHudView_VisibleChanged(object sender, EventArgs e)
        {
        	try
        	{
        		DisposeInventoryHud();
        	}catch(Exception ex){LogError(ex);}
        }

        private void InventoryHudView_Resize(object sender, System.EventArgs e)
        {
                bool bw = Math.Abs(InventoryHudView.Width - nInventoryHudWidth) > 20;
                bool bh = Math.Abs(InventoryHudView.Height - nInventoryHudHeight) > 20;
                if (bh || bw)
                {
                    nInventoryHudWidthNew = InventoryHudView.Width;
                    nInventoryHudHeightNew = InventoryHudView.Height;
                    MasterTimer.Tick += InventoryResizeTimerTick;
                }
             return;



        }

        private void InventoryResizeTimerTick(object sender, EventArgs e)
        {
            nInventoryHudWidth = nInventoryHudWidthNew;
            nInventoryHudHeight = nInventoryHudHeightNew;
            MasterTimer.Tick -= InventoryResizeTimerTick;
            SaveInventorySettings();
            RenderInventoryHud();

        }

        private void SaveInventorySettings()
        {
                if (armorSettingsFilename == "" || armorSettingsFilename == null) { armorSettingsFilename = GearDir + @"\ArmorSettings.xml"; }
                XDocument xdocInvenSet = new XDocument(new XElement("Settings"));
                xdocInvenSet.Element("Settings").Add(new XElement("Setting",
                    new XElement("ArmorHudWidth", ArmorHudWidth),
                    new XElement("ArmorHudHeight", ArmorHudHeight),
                    new XElement("InventoryHudWidth", nInventoryHudWidth),
                    new XElement("InventoryHudHeight", nInventoryHudHeight)));


                xdocInvenSet.Save(armorSettingsFilename);

        }



        private void InventoryHudTabView_OpenTabChange(object sender, System.EventArgs e)
        {
            WriteToChat("Current tab value is " + InventoryHudTabView.CurrentTab.ToString());

            try
            {
                switch (InventoryHudTabView.CurrentTab)
                {
                    case 0:
                        DisposeInventorySettingsLayout();
                        RenderInventoryTabLayout();
                        return;
                    case 1:
                        DisposeInventoryTabLayout();

                        RenderInventorySettingsTabLayout();
                        break;
                }

            }
            catch (Exception ex) { LogError(ex); }
        }


        private void RenderInventoryTabLayout()
        {
            try
            {
                lblInventoryClass = new HudStaticText();
                lblInventoryClass.FontHeight = nmenuFontHeight;
                lblInventoryClass.Text = "Class";
                ControlGroup InventoryClasses = new ControlGroup();
                cboInventoryClasses = new HudCombo(InventoryClasses);
                cboInventoryClasses.Change += (sender, index) => cboInventoryClasses_Change(sender, index);
                int i=0;
                foreach (IDNameLoadable info in ClassInvList)
                {
                    cboInventoryClasses.AddItem(info.name,i);
                    i++;
                }
                lblMyChoice = new HudStaticText();
                lblMyChoice.FontHeight = nmenuFontHeight;
                lblMyChoice.Text = "Search:";
 
                txtMyChoice = new HudTextBox();
                

                lblWeapons = new HudStaticText();
                lblWeapons.FontHeight = 11;
                lblWeapons.Text = "Weapons";
                lblWeapons.TextAlignment = VirindiViewService.WriteTextFormats.Center;
                lblArmor = new HudStaticText();
                lblArmor.FontHeight = 11;
                lblArmor.Text = "Armor/Clothing/Aetheria";
                lblArmor.TextAlignment = VirindiViewService.WriteTextFormats.Center;
                lblSalvage = new HudStaticText();
                lblSalvage.FontHeight = 11;
                lblSalvage.Text = "Salvage";
                lblSalvage.TextAlignment = VirindiViewService.WriteTextFormats.Center;
                lblMelee = new HudStaticText();
                lblMelee.FontHeight = nmenuFontHeight;
                lblMelee.Text = "Mel:";
                ControlGroup WieldAttribTypes = new ControlGroup();
                cboWieldAttrib = new HudCombo(WieldAttribTypes);
               cboWieldAttrib.Change += (sender, index) => cboWieldAttrib_Change(sender, index);

                i = 0;
                foreach (IDNameLoadable info in MeleeTypeInvList)
                {
                    cboWieldAttrib.AddItem(info.name, i);
                    i++;
                }

                lblSet = new HudStaticText();
                lblSet.FontHeight = nmenuFontHeight;
                lblSet.Text = "Set:";
                ControlGroup SetChoices = new ControlGroup();
                cboArmorSet = new HudCombo(SetChoices);
                cboArmorSet.Change += (sender, index) => cboArmorSet_Change(sender, index);

                i = 0;
                foreach (IDNameLoadable info in ArmorSetsInvList)
                {
                    cboArmorSet.AddItem(info.name, i);
                    i++;
                }

                lblMaterial = new HudStaticText();
                lblMaterial.FontHeight = nmenuFontHeight;
                lblMaterial.Text = "Mat:";
                ControlGroup MaterialChoices = new ControlGroup();
                cboMaterial = new HudCombo(MaterialChoices);
                cboMaterial.Change += (sender, index) => cboMaterial_Change(sender, index);

                i = 0;
                foreach (IDNameLoadable info in MaterialInvList)
                {
                    cboMaterial.AddItem(info.name, i);
                    i++;
                }

                lblDamage = new HudStaticText();
                lblDamage.FontHeight = nmenuFontHeight;
                lblDamage.Text = "Dam:";
                ControlGroup DamageTypes = new ControlGroup();
                cboDamageType = new HudCombo(DamageTypes);
                cboDamageType.Change += (sender, index) => cboDamageType_Change(sender, index);

                i = 0;
                foreach (IDNameLoadable info in ElementalInvList)
                {
                    cboDamageType.AddItem(info.name, i);
                    i++;
                }

                lblArmorWield = new HudStaticText();
                lblArmorWield.FontHeight = nmenuFontHeight;
                lblArmorWield.Text = "Lev:";
                ControlGroup ArmorLevels = new ControlGroup();
                cboArmorLevel = new HudCombo(ArmorLevels);
                cboArmorLevel.Change += (sender, index) => cboArmorLevel_Change(sender, index);

                i = 0;
                foreach (IDNameLoadable info in ArmorLevelInvList)
                {
                    cboArmorLevel.AddItem(info.name, i);
                    i++;
                }


                lblWork = new HudStaticText();
                lblWork.FontHeight = nmenuFontHeight;
                lblWork.Text = "Work:";
                ControlGroup WorkChoices = new ControlGroup();
                cboSalvWork = new HudCombo(WorkChoices);
                cboSalvWork.Change += (sender, index) => cboSalvWork_Change(sender, index);

                i = 0;
                foreach (IDNameLoadable info in SalvageWorkInvList)
                {
                    cboSalvWork.AddItem(info.name, i);
                    i++;
                }

                lblWield = new HudStaticText();
                lblWield.FontHeight = nmenuFontHeight;
                lblWield.Text = "Lev:";
                ControlGroup WieldLevels = new ControlGroup();
                cboLevel = new HudCombo(WieldLevels);
                cboLevel.Change += (sender, index) => cboLevel_Change(sender, index);

                i = 0;
                foreach (IDNameLoadable info in WeaponWieldInvList)
                {
                    cboLevel.AddItem(info.name, i);
                    i++;
                }


                lblCovers = new HudStaticText();
                lblCovers.FontHeight = nmenuFontHeight;
                lblCovers.Text = "Cov:";
                ControlGroup CoverageChoices = new ControlGroup();
                cboCoverage = new HudCombo(CoverageChoices);
                cboCoverage.Change += (sender, index) => cboCoverage_Change(sender, index);

                i = 0;
                foreach (IDNameLoadable info in CoverageInvList)
                {
                    cboCoverage.AddItem(info.name, i);
                    i++;
                }

                lblEmbues = new HudStaticText();
                lblEmbues.FontHeight = nmenuFontHeight;
                lblEmbues.Text = "Emb:";
                ControlGroup EmbueChoices = new ControlGroup();
                cboEmbues = new HudCombo(EmbueChoices);

                cboEmbues.Change += (sender, index) => cboEmbues_Change(sender, index);

                i = 0;
                foreach (IDNameLoadable info in EmbueInvList)
                {
                    cboEmbues.AddItem(info.name, i);
                    i++;
                }

                btnClrInv = new HudButton();
                btnClrInv.Text = "Clear List";
                btnClrInv.Hit += (sender, index) => btnClrInv_Hit(sender, index);

                btnLstInv = new HudButton();
                btnLstInv.Text = "List Inventory";
                btnLstInv.Hit += (sender, index) => btnLstInv_Hit(sender, index);
                try
                {
                    lstHudInventory = new HudList();
                    lstHudInventory.AddColumn(typeof(HudPictureBox), 20, null);
                    lstHudInventory.AddColumn(typeof(HudStaticText), Convert.ToInt32(.5 * nInventoryHudWidth), null);
                    lstHudInventory.AddColumn(typeof(HudStaticText), Convert.ToInt32(.44 * nInventoryHudWidth), null);
                    lstHudInventory.AddColumn(typeof(HudStaticText), Convert.ToInt32(.001 * nInventoryHudWidth), null);

                    lstHudInventory.Click += (sender, row, col) => lstHudInventory_Click(sender, row, col);
                }
                            
                catch (Exception ex) { LogError(ex); }

                InventoryHudTabLayout.AddControl(lblInventoryClass, new Rectangle(10, 10, 30, 16));
                InventoryHudTabLayout.AddControl(cboInventoryClasses, new Rectangle(45, 10, 100, 16));
                InventoryHudTabLayout.AddControl(lblMyChoice, new Rectangle(155, 10, 100, 16));
                InventoryHudTabLayout.AddControl(txtMyChoice, new Rectangle(260, 10, 280, 16));
                InventoryHudTabLayout.AddControl(lblWeapons, new Rectangle(10,30,nInventoryHudWidth/3,20));
                InventoryHudTabLayout.AddControl(lblArmor, new Rectangle(nInventoryHudWidth/3, 30, nInventoryHudWidth / 3, 20));
                InventoryHudTabLayout.AddControl(lblSalvage, new Rectangle((2 * nInventoryHudWidth) / 3, 30, nInventoryHudWidth / 3, 20));

                InventoryHudTabLayout.AddControl(lblMelee, new Rectangle(10, 50, 25, 16));
                InventoryHudTabLayout.AddControl(cboWieldAttrib, new Rectangle(40, 50, 100, 16));
                InventoryHudTabLayout.AddControl(lblSet, new Rectangle(nInventoryHudWidth/3, 50, 25, 16));
                InventoryHudTabLayout.AddControl(cboArmorSet, new Rectangle(nInventoryHudWidth/ 3 + 30, 50, 150, 16));
                InventoryHudTabLayout.AddControl(lblMaterial, new Rectangle((2 * nInventoryHudWidth) / 3, 50, 25, 16));
                InventoryHudTabLayout.AddControl(cboMaterial, new Rectangle((2 * nInventoryHudWidth) / 3 + 30, 50, 150, 16));
                InventoryHudTabLayout.AddControl(lblDamage, new Rectangle(10, 70, 25, 16));
                InventoryHudTabLayout.AddControl(cboDamageType, new Rectangle(40, 70, 100, 16));
                InventoryHudTabLayout.AddControl(lblArmorWield, new Rectangle(nInventoryHudWidth / 3, 70, 25, 16));
                InventoryHudTabLayout.AddControl(cboArmorLevel, new Rectangle(nInventoryHudWidth / 3 + 30, 70, 100, 16));
                InventoryHudTabLayout.AddControl(lblWork, new Rectangle((2 * nInventoryHudWidth) / 3, 70, 25, 16));
                InventoryHudTabLayout.AddControl(cboSalvWork, new Rectangle((2 * nInventoryHudWidth) / 3 + 30, 70, 100, 16));
                InventoryHudTabLayout.AddControl(lblWield, new Rectangle(10, 90, 25, 16));
                InventoryHudTabLayout.AddControl(cboLevel, new Rectangle(40,90, 100, 16));
                InventoryHudTabLayout.AddControl(lblCovers, new Rectangle(nInventoryHudWidth / 3, 90, 25, 16));
                InventoryHudTabLayout.AddControl(cboCoverage, new Rectangle(nInventoryHudWidth / 3 + 30, 90, 100, 16));
                InventoryHudTabLayout.AddControl(lblEmbues, new Rectangle(10, 110, 25, 16));
                InventoryHudTabLayout.AddControl(cboEmbues, new Rectangle(40, 110, 100, 16));

                InventoryHudTabLayout.AddControl(btnLstInv, new Rectangle((2* nInventoryHudWidth)/3,100,100,16));
                InventoryHudTabLayout.AddControl(btnClrInv, new Rectangle((2 * nInventoryHudWidth) / 3, 120, 100, 16));
                InventoryHudTabLayout.AddControl(lstHudInventory, new Rectangle(10, 150, nInventoryHudWidth, nInventoryHudHeight - 155));
                

                bInventoryMainTab = true;
 
            }

            catch (Exception ex) { LogError(ex); }
        }


        private void DisposeInventoryTabLayout()
        {
            try
            {
                if (!bInventoryMainTab) { return; }
                clearListVariables();
                cboInventoryClasses.Change -= (sender, index) => cboInventoryClasses_Change(sender, index);
                cboWieldAttrib.Change -= (sender, index) => cboWieldAttrib_Change(sender, index);
                cboArmorSet.Change -= (sender, index) => cboArmorSet_Change(sender, index);
                cboMaterial.Change -= (sender, index) => cboMaterial_Change(sender, index);
                cboDamageType.Change -= (sender, index) => cboDamageType_Change(sender, index);
                cboArmorLevel.Change -= (sender, index) => cboArmorLevel_Change(sender, index);
                cboSalvWork.Change -= (sender, index) => cboSalvWork_Change(sender, index);
                cboLevel.Change -= (sender, index) => cboLevel_Change(sender, index);
                cboCoverage.Change -= (sender, index) => cboCoverage_Change(sender, index);
                lstHudInventory.Click -= (sender, row, col) => lstHudInventory_Click(sender, row, col);
                btnClrInv.Hit -= (sender, index) => btnClrInv_Hit(sender, index);
                btnLstInv.Hit -= (sender, index) => btnLstInv_Hit(sender, index);
                cboEmbues.Change -= (sender, index) => cboEmbues_Change(sender, index);

                cboInventoryClasses = null;
                cboWieldAttrib = null;
                cboArmorSet = null;
                cboMaterial = null;
                cboDamageType = null;
                cboArmorLevel = null;
                cboSalvWork = null;
                cboLevel = null;
                cboCoverage = null;
                lstHudInventory = null;
                btnClrInv = null;
                btnLstInv = null;
                cboEmbues = null;
               

                bInventoryMainTab = false;


            }
            catch (Exception ex) { LogError(ex); }
        }

        private void RenderInventorySettingsTabLayout()
        {
            try
            {

                btnInventoryUpdate = new HudButton();
                btnInventoryUpdate.Text = "Update Inventory";
                btnInventoryUpdate.Hit += (sender, index) => btnInventoryUpdate_Hit(sender, index);
                InventoryHudSettings.AddControl(btnInventoryUpdate, new Rectangle(20, 20, 150, 20));

                btnInventoryComplete = new HudButton();
                btnInventoryComplete.Text = "Do Complete Inventory";
                btnInventoryComplete.Hit += (sender, index) => btnInventoryComplete_Hit(sender, index);
                InventoryHudSettings.AddControl(btnInventoryComplete, new Rectangle(20, 45, 150, 20));

                btnInventoryStacks = new HudButton();
                btnInventoryStacks.Text = "Redo stacks with Update";
                btnInventoryStacks.Hit += (sender, index) => btnInventoryStacks_Hit(sender, index);
                InventoryHudSettings.AddControl(btnInventoryStacks, new Rectangle(20, 70, 150, 20));



 


                bInventorySettingsTab = true;
            }
            catch (Exception ex) { LogError(ex); }
        }



        private void DisposeInventorySettingsLayout()
        {
            try
            {
                if (!bInventorySettingsTab) { return; }
                btnInventoryUpdate.Hit -= (sender, index) => btnInventoryUpdate_Hit(sender, index);
                btnInventoryComplete.Hit -= (sender, index) => btnInventoryComplete_Hit(sender, index);
                btnInventoryStacks.Hit -= (sender, index) => btnInventoryStacks_Hit(sender, index);
                btnInventoryComplete = null;
                btnInventoryUpdate = null;
                btnInventoryStacks = null;



                bInventorySettingsTab = false;
            }
            catch { }
        }



        private void DisposeInventoryHud()
        {

            try
            {
                SaveInventorySettings();
                clearListVariables();
                UnsubscribeInventoryEvents();
                try { DisposeInventoryTabLayout(); }
                catch { }
                try { DisposeInventorySettingsLayout(); }
                catch { }

                if (InventoryHudSettings != null) { InventoryHudSettings.Dispose(); }
                if (InventoryHudTabLayout != null) { InventoryHudTabLayout.Dispose(); }
                if (InventoryHudTabView != null) { InventoryHudTabView.Dispose(); }
                if (InventoryHudTabView != null) { InventoryHudView.Dispose(); }
                
                InventoryHudView = null;
            }
            catch (Exception ex) { LogError(ex); }
            return;
        }

        private void SubscribeInventoryEvents()
        {
            Core.CharacterFilter.Logoff += InventoryLogoff;

        }

        private void InventoryLogoff(object sender, EventArgs e)
        {
            try
            {
                UnsubscribeInventoryEvents();
                DisposeInventoryHud();
            }
            catch (Exception ex) { LogError(ex); }
        }



        private void UnsubscribeInventoryEvents()
        {
            try{
            if (InventoryHudTabView != null) { InventoryHudTabView.OpenTabChange -= InventoryHudTabView_OpenTabChange; }
            if (InventoryHudView != null) { InventoryHudView.Resize -= InventoryHudView_Resize; }
            if (MasterTimer != null) { MasterTimer.Tick -= InventoryResizeTimerTick; }
            Core.CharacterFilter.Logoff -= InventoryLogoff;
            }
            catch (Exception ex) { LogError(ex); }


        }


        private void btnInventoryUpdate_Hit(object sender, EventArgs e)
        {
        	if(!programinv.Contains("armor"))
        	   {
                
         	   doUpdateInventory();

        	   }
        }

        private void btnInventoryComplete_Hit(object sender, EventArgs e)
        {
        	if(!programinv.Contains("armor"))
            {
                m = 500;
                doGetInventory();
            }

        }

        void btnInventoryStacks_Hit(object sender, EventArgs e)
        {
        	if(!programinv.Contains("armor"))
        	{
               getBurden = true;
               doUpdateInventory();
        	}
        }

        void cboInventoryClasses_Change(object sender, EventArgs index)
        {

            try
            {
                objClass = 0;
                objClassName = "";
                 objClass = ClassInvList[cboInventoryClasses.Current].ID;
                objClassName = ClassInvList[cboInventoryClasses.Current].name;
             //   WriteToChat("objClass: " + objClass.ToString());


            }
            catch (Exception ex) { LogError(ex); }


        }

        void cboWieldAttrib_Change(object sender, EventArgs e)
        {
            try
            {
                objWieldAttrInt = 0;
                objWieldAttrInt = MeleeTypeInvList[cboWieldAttrib.Current].ID;
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboArmorSet_Change(object sender, EventArgs e)
        {
            try
            {
                objArmorSet = 0;
                objArmorSetName = "";
               objArmorSet = ArmorSetsInvList[cboArmorSet.Current].ID;
                objArmorSetName = ArmorSetsInvList[cboArmorSet.Current].name;
            //    WriteToChat("objSet: " + objArmorSet.ToString());


            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboMaterial_Change(object sender, EventArgs e)
        {
            try
            {
                objMat = 0;
                objMatName = "";
                objMat = MaterialInvList[cboMaterial.Current].ID;
                objMatName = MaterialInvList[cboMaterial.Current].name;

            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboDamageType_Change(object sender, EventArgs e)
        {
            try
            {
                objDamageType = "";
                objDamageTypeInt = 0;
                objDamageType = ElementalInvList[cboDamageType.Current].name;
                objDamageTypeInt = ElementalInvList[cboDamageType.Current].ID;
            }
            catch (Exception ex) { LogError(ex); }
        }

 
        void cboArmorLevel_Change(object sender, EventArgs e)
        {
            try
            {
                objArmorLevel = 0;
                objArmorLevel = Convert.ToInt16(ArmorLevelInvList[cboArmorLevel.Current].name);

            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboSalvWork_Change(object sender, EventArgs e)
        {
            try
            {
                objSalvWork = "";
                objSalvWork = SalvageWorkInvList[cboSalvWork.Current].name;
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboLevel_Change(object sender, EventArgs e)
        {
            try
            {
                objLevelInt = 0;
                objLevelInt = Convert.ToInt32(WeaponWieldInvList[cboLevel.Current].name);
           }
            catch (Exception ex) { LogError(ex); }
        }

        void cboCoverage_Change(object sender, EventArgs e)
        {
            try
            {
                objCovers = 0;
                objCoversName = "";
                objCovers = CoverageInvList[cboCoverage.Current].ID;
               // WriteToChat("objCovers = " + objCovers.ToString());

            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboEmbues_Change(object sender, EventArgs e)
        {
            try
            {
                objEmbueTypeInt = 0;
                objEmbueTypeStr = "";
                objEmbueTypeInt = EmbueInvList[cboEmbues.Current].ID;
                objEmbueTypeStr = EmbueInvList[cboEmbues.Current].name;
               // WriteToChat("objEmbueTypeStr: " + objEmbueTypeStr);
            }
            catch (Exception ex) { LogError(ex); }
        }



 
        
        void btnUpdateInventory_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
        	if(!programinv.Contains("armor"))
        	{
 	           doUpdateInventory();
        	}
        }

        void btnGetBurden_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
        	if(!programinv.Contains("armor"))
        	{
              getBurden = true;
              doUpdateInventory();
        	}
        }

        void btnItemsWaiting_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
        	if(!programinv.Contains("armor"))
        	{
              if (!mMainSettings.binventoryWaitingEnabled)
                { mMainSettings.binventoryWaitingEnabled = true; }
              else
                { mMainSettings.binventoryWaitingEnabled = false; }
        	}
        }

        private void doCheckFiles()
        {
            if(!File.Exists(genInventoryFilename))
            {
                 XDocument tempGInvDoc = new XDocument(new XElement("Objs"));
                 tempGInvDoc.Save(genInventoryFilename);
                 tempGInvDoc = null;

            }
            if(!File.Exists(inventoryFilename))
            {
                 XDocument tempInvDoc = new XDocument(new XElement("Objs"));
                 tempInvDoc.Save(inventoryFilename);
                 tempInvDoc = null;
            }
         }


        private void doUpdateInventory()
        {
            try
            {
                if (programinv.Contains("armor"))
                {
                    WriteToChat("Cannot run general inventory until armor inventory is completed.");
                }
                else
                {
                    programinv = "inventory";
                    doCheckFiles();
                   //Need a list to hold the inventory
                    mWaitingForID = new List<WorldObject>();

                    // If already have an inventory file for a toon, do not need to duplicate already id'd inventory
                    // moldObjsID is a list that contains the object IDs of the previous inventory for that toon
                    moldObjsID = new List<string>();
                    mgoonInv = true;
                    List<string> mtemps = new List<string>();

                    if (File.Exists(inventoryFilename))
                    {
                        try
                        {
                            xdocToonInventory = XDocument.Load(inventoryFilename);

                            if (getBurden)
                            {
                                IEnumerable<XElement> myelements = xdocToonInventory.Element("Objs").Descendants("Obj");
                                int oldCount = (int)(xdocToonInventory.Element("Objs").Elements("Obj").Count());

                                var obj = from o in xdocToonInventory.Descendants("Obj")
                                          where o.Element("ObjName").Value.Contains("Stipend") ||
                                              o.Element("ObjName").Value.Contains("Crystal") || o.Element("ObjName").Value.Contains("Jewel")
                                              || o.Element("ObjName").Value.Contains("Pearl") || o.Element("ObjName").Value.Contains("Trade Note")
                                              || o.Element("ObjName").Value.Contains("Society Gem") || o.Element("ObjName").Value.Contains("Token")
                                              || o.Element("ObjName").Value.Contains("Field Ration") || o.Element("ObjName").Value.Contains("Pea")
                                              || o.Element("ObjName").Value.Contains("Coin") || o.Element("ObjName").Value.Contains("Sack")
                                              || o.Element("ObjName").Value.Contains("Venom Sac") || o.Element("ObjName").Value.Contains("Glyph")
                                              || o.Element("ObjName").Value.Contains("Arrow")
                                          select o;
                                obj.Remove();
                                int newCount = (int)(xdocToonInventory.Element("Objs").Elements("Obj").Count());


                                GearFoundry.PluginCore.WriteToChat("Count before removal = " + oldCount + ".  Count after removal = " + newCount);
                            }

                            IEnumerable<XElement> elements = xdocToonInventory.Element("Objs").Descendants("ObjID");
                            foreach (XElement element in elements)
                            {
                                //Create list of the ID's currently in the inventory
                                { moldObjsID.Add(element.Value); }
                            }


                        }
                        catch (Exception ex) { mgoonInv = false; doGetInventory(); LogError(ex); }


                    }

                    xdocGenInventory = XDocument.Load(genInventoryFilename);
                    // if left this subprogram because of exception in update need a way to avoid returning to this program
                    if (mgoonInv)
                    {
                        mCurrID = new List<string>();

                        //loop for checking each obj in the current inventory
                        foreach (Decal.Adapter.Wrappers.WorldObject obj in Core.WorldFilter.GetInventory())
                        {
                            try
                            {
                                //Need to find the current inventory objects and create a list of their ids mCurrID
                                objID = obj.Id;
                                string sobjID = objID.ToString();
                                mCurrID.Add(sobjID);
                                
                                //Need to compare the ids in mCurrID with those of the previous inventory 
                                if (!moldObjsID.Contains(sobjID))
                                {
                                    Globals.Host.Actions.RequestId(obj.Id);
                                    mWaitingForID.Add(obj); //if the ID not in previous inventory need to get the data
                                }

                            }
                            catch (Exception ex) { LogError(ex); }


                        } // endof foreach world object



                        GearFoundry.PluginCore.WriteToChat("Items being inventoried " + mWaitingForID.Count);
                        //Do one run through saved ids to get all data that is immediately available
                        if (mWaitingForID.Count > 0)
                        {
                            OnInventoryStart(); // This one in the doupdate
                        }
                        //   else { programinv = ""; }
                        else { mIsFinished(); }
                        //Now need to start routines that will continue to get data as becomes available or will end the search and save the files
                      //  mIsFinished();



                    }
                }

            } //end of try
            catch (Exception ex) { LogError(ex); }
        }

        void btnGetInventory_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
             m = 500;
            doGetInventory();
        }

 
        // items selected need to be added to listview: lstinventory
        private void btnLstInv_Hit(object sender, EventArgs e)
        {
        	if (!File.Exists(genInventoryFilename)){WriteToChat("You must first do an inventory.");}
        	else if(File.Exists(genInventoryFilename))
            {
                try
                {
                    XDocument tempGIDoc = new XDocument(new XElement("Objs"));
                    tempGIDoc.Save(inventorySelect);
                  //  WriteToChat("I have  set  up inventoryselect prior to finding list.");
                    tempGIDoc = null;
                    lstMySelects = new List<string>();

                    if (txtMyChoice.Text != null)
                    {
                        mySelect = txtMyChoice.Text.Trim();
                        mySelect = mySelect.ToLower();
                        
                        if (mySelect.Contains(';'))
                        {
                            string [] split = mySelect.Split(new Char [] {';' });

                            foreach (string s in split)
                            {

                                if (s.Trim() != "")
                                {
                                    lstMySelects.Add(s);
                                }
                            }
                        }
                        else
                        {
                            lstMySelects.Add(mySelect);
                        }

                        
                    }
                    else  { mySelect = null; }
                    xdocListInv = XDocument.Load(genInventoryFilename);
                }//end of try //

                catch (Exception ex) { LogError(ex); }


                try
                {
                    if (objClass < 1)
                    {
                        objClass = 0;
                    }
                  //  WriteToChat("objClass: " + objClass.ToString());

                    switch (objClass)
                    {
                        case 0:
                            if (lstMySelects.Count > 0)
                            {
                                int n = lstMySelects.Count;
                                XDocument tempdoc = new XDocument();
                                newDoc = new XDocument(new XElement("Objs"));
                                for (int i = 0; i < n; i++)
                                {
                                    tempdoc = new XDocument(new XElement("Objs",
                                    from p in xdocListInv.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjName").Value.ToLower().Contains(lstMySelects[n]) ||
                                    p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[n])
                                    select p));
                                    newDoc.Root.Add(tempdoc);
                                    tempdoc = null;
                                }


                            }
                            else { WriteToChat("You must either select a class or write something in textbox"); }
                            break;

                        case 1:
                        case 2:
                        case 11:
                            if (lstMySelects != null && lstMySelects[0].Trim() != "")
                            {
                                int n = lstMySelects.Count;
                                 newDoc = new XDocument(new XElement("Objs"));
                                for(int i = 0;i<n;i++)
                                {
                                  if (objArmorSet == 0 && objArmorLevel == 1 && objCovers == 0)
                                  {

                                    var temp = 
                                    from p in xdocListInv.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                    select p;
                                    newDoc.Root.Add(temp);
                                    temp = null;
                                  //  WriteToChat("In selection of  newDoc for armor");
                                  }


                                  else if (objArmorSet > 0 && objArmorLevel == 1 && objCovers == 0)
                                  {
                                   var temp = 
                                    from p in xdocListInv.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                        p.Element("ObjSet").Value == objArmorSet.ToString() &&
                                        (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                        
                                    select p;
                                    newDoc.Root.Add(temp);
                                    temp = null;
 
                                  }
                                  else if ((objArmorLevel > 1 || objArmorLevel < 1) && objArmorSet == 0 && objCovers == 0)
                                  {
                                     var temp = 
                                    from p in xdocListInv.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                        p.Element("ObjWieldValue").Value == objArmorLevel.ToString() &&
                                        (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                    select p;
                                    newDoc.Root.Add(temp);
                                    temp = null;
 
                                  }
                                  else if (objCovers > 0 && objArmorSet == 0 && objArmorLevel == 1)
                                  {
                                      
                                     var temp = 
                                    from p in xdocListInv.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                          p.Element("ObjCovers").Value == objCovers.ToString() &&
                                        (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                    select p;
                                    newDoc.Root.Add(temp);
                                    temp = null;
 
                                  }
                                  else if (objArmorSet > 0 && (objArmorLevel < 1 || objArmorLevel > 1) && objCovers == 0)
                                  {
                                      var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                            p.Element("ObjSet").Value == objArmorSet.ToString() &&
                                           p.Element("ObjWieldValue").Value == objArmorLevel.ToString() &&
                                         (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                        select p;
                                    newDoc.Root.Add(temp);
                                    temp = null;
 
                                  }
                                  else if (objArmorSet > 0 && objCovers > 0 && objArmorLevel == 1)
                                  {
                                   var   temp = new XDocument(new XElement("Objs",
                                    from p in xdocListInv.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                        p.Element("ObjSet").Value == objArmorSet.ToString() &&
                                          p.Element("ObjCovers").Value == objCovers.ToString() &&
                                        (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                    select p));
                                    newDoc.Root.Add(temp);
                                    temp = null;
 
                                 }
                                 else if (objArmorSet == 0 && (objArmorLevel > 1 || objArmorLevel < 1) && objCovers > 0)
                                 {
                                     var temp = 
                                    from p in xdocListInv.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                       p.Element("ObjWieldValue").Value == objArmorLevel.ToString() &&
                                        p.Element("ObjCovers").Value == objCovers.ToString() &&
                                         (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                    select p;
                                    newDoc.Root.Add(temp);
                                    temp = null;
 
                                 }
                                 else if (objArmorSet > 0 && (objArmorLevel > 1 || objArmorLevel < 1) && objCovers > 0)
                                 {
                                      var temp = 
                                     from p in xdocListInv.Element("Objs").Descendants("Obj")
                                     where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjSet").Value == objArmorSet.ToString() &&
                                          p.Element("ObjWieldValue").Value == objArmorLevel.ToString() &&
                                           p.Element("ObjCovers").Value == objCovers.ToString() &&
                                         (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                     select p;
                                    newDoc.Root.Add(temp);
                                    temp = null;
 
                                }
                              }
                            }

                            else
                            {
                                if (objArmorSet == 0 && objArmorLevel == 1 && objCovers == 0)
                                {

                                    newDoc = new XDocument(new XElement("Objs",
                                    from p in xdocListInv.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName)
                                    select p));
                                }

                                else if (objArmorSet > 0 && objArmorLevel == 1 && objCovers == 0)
                                {

                                    newDoc = new XDocument(new XElement("Objs",
                                    from p in xdocListInv.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                        p.Element("ObjSet").Value == objArmorSet.ToString()
                                    select p));
                                }
                                else if ((objArmorLevel > 1 || objArmorLevel < 1) && objArmorSet == 0 && objCovers == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                    from p in xdocListInv.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                       p.Element("ObjWieldValue").Value == objArmorLevel.ToString()
                                    select p));
                                }
                                else if (objCovers > 0 && objArmorSet == 0 && objArmorLevel == 1)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                    from p in xdocListInv.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                          p.Element("ObjCovers").Value == objCovers.ToString()
                                    select p));
                                }
                                else if (objArmorSet > 0 && (objArmorLevel < 1 || objArmorLevel > 1) && objCovers == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                            p.Element("ObjSet").Value == objArmorSet.ToString() &&
                                           p.Element("ObjWieldValue").Value == objArmorLevel.ToString()
                                        select p));
                                }
                                else if (objArmorSet > 0 && objCovers > 0 && objArmorLevel == 1)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                    from p in xdocListInv.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                        p.Element("ObjSet").Value == objArmorSet.ToString() &&
                                          p.Element("ObjCovers").Value == objCovers.ToString()
                                    select p));
                                }
                                else if (objArmorSet == 0 && (objArmorLevel > 1 || objArmorLevel < 1) && objCovers > 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                    from p in xdocListInv.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                       p.Element("ObjWieldValue").Value == objArmorLevel.ToString() &&
                                        p.Element("ObjCovers").Value == objCovers.ToString()
                                    select p));
                                }
                                else if (objArmorSet > 0 && (objArmorLevel > 1 || objArmorLevel < 1) && objCovers > 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                     from p in xdocListInv.Element("Objs").Descendants("Obj")
                                     where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjSet").Value == objArmorSet.ToString() &&
                                          p.Element("ObjWieldValue").Value == objArmorLevel.ToString() &&
                                           p.Element("ObjCovers").Value == objCovers.ToString()
                                     select p));
                                }


                            }  //end of if spells
                            break;
                        case 5:
                            if (lstMySelects != null && lstMySelects[0].Trim() != "")
                            {
                                int n = lstMySelects.Count;
                                newDoc = new XDocument(new XElement("Objs"));
                                for (int i = 0; i < n; i++)
                                {
                                    if (objWieldAttrInt == 0 && objDamageTypeInt == 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                                {
                                   var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                        select p;
                                   newDoc.Root.Add(temp);
                                   temp = null;
                                }


                                else if (objWieldAttrInt > 0 && objDamageTypeInt == 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                         (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                        select p;
                                   newDoc.Root.Add(temp);
                                   temp = null;
 
                                }

                                else if (objDamageTypeInt > 0 && objWieldAttrInt == 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                        (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                        select p;
                                       newDoc.Root.Add(temp);
                                       temp = null;
 
                                }
                                else if ((objLevelInt < 1 || objLevelInt > 1) && objWieldAttrInt == 0 && objDamageTypeInt == 0 && objEmbueTypeInt == 0)
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                         (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                        select p;
                                       newDoc.Root.Add(temp);
                                       temp = null;
                                }
                                else if (objEmbueTypeInt > 0 && objWieldAttrInt == 0 && objDamageTypeInt == 1 && objLevelInt == 1)
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                        (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                        select p;
                                       newDoc.Root.Add(temp);
                                       temp = null;
                                }
                                else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                        (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                        select p;
                                       newDoc.Root.Add(temp);
                                       temp = null;
                                }
                                else if (objWieldAttrInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objDamageTypeInt == 0 && objEmbueTypeInt == 0)
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                         (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                        select p;
                                       newDoc.Root.Add(temp);
                                       temp = null;
                               }
                                else if (objWieldAttrInt > 0 && objEmbueTypeInt > 0 && objDamageTypeInt == 0 && objLevelInt == 1)
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                         p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                         (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                         select p;
                                       newDoc.Root.Add(temp);
                                       temp = null;
                               }
                                else if (objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objWieldAttrInt == 0 && objEmbueTypeInt == 0)
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                         (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                        select p;
                                       newDoc.Root.Add(temp);
                                       temp = null;
                                }
                                else if (objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objWieldAttrInt == 0 && objEmbueTypeInt == 0)
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                        (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                         select p;
                                       newDoc.Root.Add(temp);
                                       temp = null;
                               }
                                else if (objDamageTypeInt > 0 && objEmbueTypeInt > 0 && objWieldAttrInt == 0 && (objLevelInt == 1))
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                         p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                        (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                        select p;
                                       newDoc.Root.Add(temp);
                                       temp = null;
                                }
                                else if ((objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0 && objWieldAttrInt == 0 && objDamageTypeInt == 0)
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                         p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                         (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                         select p;
                                       newDoc.Root.Add(temp);
                                       temp = null;
                              }
                                else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt == 0)
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                        (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                         select p;
                                       newDoc.Root.Add(temp);
                                       temp = null;
                               }
                                else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && objEmbueTypeInt > 0 && objLevelInt == 1)
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                         p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                        (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                         select p;
                                       newDoc.Root.Add(temp);
                                       temp = null;
                               }
                                else if (objWieldAttrInt > 0 && objDamageTypeInt == 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                         p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                         (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                         select p;
                                       newDoc.Root.Add(temp);
                                       temp = null;
                               }
                                else if (objWieldAttrInt == 0 && objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                         p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                        (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                        select p;
                                       newDoc.Root.Add(temp);
                                       temp = null;
                               }
                                else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                                {
                                    var temp = 
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                         p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                         (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                         select p;
                                       newDoc.Root.Add(temp);
                                       temp = null;
                                 }
                               } //end of case 5 with spells
                             }//end  of case 5 with spells
                            else
                            {
                                if (objWieldAttrInt == 0 && objDamageTypeInt == 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName)
                                        select p));
                                }


                                else if (objWieldAttrInt > 0 && objDamageTypeInt == 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString()
                                        select p));
                                }

                                else if (objDamageTypeInt > 0 && objWieldAttrInt == 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString()
                                        select p));
                                }

                                else if ((objLevelInt < 1 || objLevelInt > 1) && objWieldAttrInt == 0 && objDamageTypeInt == 0 && objEmbueTypeInt == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString()
                                        select p));
                                }
                                else if (objEmbueTypeInt > 0 && objWieldAttrInt == 0 && objDamageTypeInt == 1 && objLevelInt == 1)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                        select p));
                                }
                                else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString()
                                        select p));
                                }
                                else if (objWieldAttrInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objDamageTypeInt == 0 && objEmbueTypeInt == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString()
                                        select p));
                                }
                                else if (objWieldAttrInt > 0 && objEmbueTypeInt > 0 && objDamageTypeInt == 0 && objLevelInt == 1)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                         p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                        select p));
                                }
                                else if (objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objWieldAttrInt == 0 && objEmbueTypeInt == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString()
                                        select p));
                                }
                                else if (objDamageTypeInt > 0 && objEmbueTypeInt > 0 && objWieldAttrInt == 0 && (objLevelInt == 1))
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                         p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                        select p));
                                }

                                else if ((objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0 && objWieldAttrInt == 0 && objDamageTypeInt == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                         p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                        select p));
                                }
                                else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString()
                                        select p));
                                }
                                else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && objEmbueTypeInt > 0 && objLevelInt == 1)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                         p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                        select p));
                                }
                                else if (objWieldAttrInt > 0 && objDamageTypeInt == 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                         p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                        select p));
                                }
                                else if (objWieldAttrInt == 0 && objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                         p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                         p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                        select p));
                                }
                                else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                         p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                        p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                        p.Element("ObjEmbue").Value == (objEmbueTypeInt.ToString())
                                        select p));
                                }

                            } //end of case 5  no spells

                            break;
                        case 4:
                        case 6:
                            if (lstMySelects.Count > 0)
                            {
                                int n = lstMySelects.Count;
                                XDocument tempdoc = new XDocument();
                                newDoc = new XDocument(new XElement("Objs"));
                                for (int i = 0; i < n; i++)
                                {
                                    if (objDamageTypeInt == 0 && objMagicDamageInt == 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                                    {
                                        var temp = 
                                            from p in xdocListInv.Element("Objs").Descendants("Obj")
                                            where p.Element("ObjClass").Value.Contains(objClassName) &&
                                            (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                              p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                            select p;
                                        newDoc.Root.Add(temp);
                                        temp = null;
                                    }


                                    if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && (objLevelInt == 1) && objEmbueTypeInt == 0)
                                    {
                                        var temp = 
                                            from p in xdocListInv.Element("Objs").Descendants("Obj")
                                            where p.Element("ObjClass").Value.Contains(objClassName) &&
                                             (p.Element("ObjDamage").Value == objDamageTypeInt.ToString() ||
                                              p.Element("ObjMagicDamage").Value == objDamageTypeInt.ToString()) &&
                                            (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                              p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                            select p;
                                        newDoc.Root.Add(temp);
                                        temp = null;
                                    }
                                    if ((objLevelInt < 1 || objLevelInt > 1) && objDamageTypeInt == 0 && objMagicDamageInt == 0 && objEmbueTypeInt == 0)
                                    {
                                        var temp = 
                                            from p in xdocListInv.Element("Objs").Descendants("Obj")
                                            where p.Element("ObjClass").Value.Contains(objClassName) &&
                                            p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                             (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                              p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                            select p;
                                        newDoc.Root.Add(temp);
                                        temp = null;
                                    }
                                    if (objEmbueTypeInt > 0 && objDamageTypeInt == 0 && objMagicDamageInt == 0 && objLevelInt == 1)
                                    {
                                        var temp = 
                                            from p in xdocListInv.Element("Objs").Descendants("Obj")
                                            where p.Element("ObjClass").Value.Contains(objClassName) &&
                                            p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                            (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                              p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                            select p;
                                        newDoc.Root.Add(temp);
                                        temp = null;
                                    }

                                    if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt == 0)
                                    {
                                        var temp = 
                                            from p in xdocListInv.Element("Objs").Descendants("Obj")
                                            where p.Element("ObjClass").Value.Contains(objClassName) &&
                                             (p.Element("ObjDamage").Value == objDamageTypeInt.ToString() ||
                                              p.Element("ObjMagicDamage").Value == objDamageTypeInt.ToString()) &&
                                            p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                            (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                              p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                            select p;
                                        newDoc.Root.Add(temp);
                                        temp = null;
                                    }
                                    if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && objEmbueTypeInt > 0 && objLevelInt == 1)
                                    {
                                        var temp = 
                                            from p in xdocListInv.Element("Objs").Descendants("Obj")
                                            where p.Element("ObjClass").Value.Contains(objClassName) &&
                                             (p.Element("ObjDamage").Value == objDamageTypeInt.ToString() ||
                                              p.Element("ObjMagicDamage").Value == objDamageTypeInt.ToString()) &&
                                            p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                            (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                              p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                            select p;
                                        newDoc.Root.Add(temp);
                                        temp = null;
                                    }
                                    if ((objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0 && objDamageTypeInt == 0 && objMagicDamageInt == 0)
                                    {
                                        var temp = 
                                            from p in xdocListInv.Element("Objs").Descendants("Obj")
                                            where p.Element("ObjClass").Value.Contains(objClassName) &&
                                            p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                            p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                            (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[i]) ||
                                              p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[i]))
                                            select p;
                                        newDoc.Root.Add(temp);
                                        temp = null;
                                    }

                                    if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                                    {
                                        var temp = 
                                            from p in xdocListInv.Element("Objs").Descendants("Obj")
                                            where p.Element("ObjClass").Value.Contains(objClassName) &&
                                             (p.Element("ObjDamage").Value == objDamageTypeInt.ToString() ||
                                              p.Element("ObjMagicDamage").Value == objDamageTypeInt.ToString()) &&
                                            p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                            p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                            (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[0]) ||
                                              p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[0]))
                                            select p;
                                        newDoc.Root.Add(temp);
                                        temp = null;
                                    }
                                }
                            }
                            else
                            {
                                if (objDamageTypeInt == 0 && objMagicDamageInt == 0 && (objLevelInt == 1) && objEmbueTypeInt == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName)
                                        select p));
                                }

                                if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && (objLevelInt == 1) && objEmbueTypeInt == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         (p.Element("ObjDamage").Value == objDamageTypeInt.ToString() ||
                                          p.Element("ObjMagicDamage").Value == objDamageTypeInt.ToString())
                                        select p));
                                }
                                if ((objLevelInt < 1 || objLevelInt > 1) && objDamageTypeInt == 0 && objMagicDamageInt == 0 && objEmbueTypeInt == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                        p.Element("ObjWieldValue").Value == objLevelInt.ToString()
                                        select p));
                                }
                                if (objEmbueTypeInt > 0 && objDamageTypeInt == 0 && objMagicDamageInt == 0 && (objLevelInt == 1))
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                        p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                        select p));
                                }
                                if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         (p.Element("ObjDamage").Value == objDamageTypeInt.ToString() ||
                                          p.Element("ObjMagicDamage").Value == objDamageTypeInt.ToString()) &&
                                        p.Element("ObjWieldValue").Value == objLevelInt.ToString()
                                        select p));
                                }

                                if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && objEmbueTypeInt > 0 && objLevelInt == 1)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         (p.Element("ObjDamage").Value == objDamageTypeInt.ToString() ||
                                          p.Element("ObjMagicDamage").Value == objDamageTypeInt.ToString()) &&
                                        p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                        select p));
                                }
                                if ((objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0 && objDamageTypeInt == 0 && objMagicDamageInt == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                        p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                        p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                        select p));
                                }


                                if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                        from p in xdocListInv.Element("Objs").Descendants("Obj")
                                        where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         (p.Element("ObjDamage").Value == objDamageTypeInt.ToString() ||
                                          p.Element("ObjMagicDamage").Value == objDamageTypeInt.ToString()) &&
                                        p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                        p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                        select p));
                                }
                            }

                            break;

                        case 7:

                            if ((objClassName.Contains("Salvage")) && (objSalvWork == "None"))
                            {

                                newDoc = new XDocument(new XElement("Objs",
                                  from p in xdocListInv.Element("Objs").Descendants("Obj")
                                  where p.Element("ObjClass").Value.Contains(objClassName) &&
                                  p.Element("ObjMaterial").Value == objMat.ToString()
                                  select p));
                            }

                            else if ((objClassName.Contains("Salvage")) && ((objSalvWork == "1-6"))) // || (objSalvWork == "7-8") || (objSalvWork == "9")))
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                  from p in xdocListInv.Element("Objs").Descendants("Obj")
                                  where p.Element("ObjClass").Value.Contains(objClassName) &&
                                  p.Element("ObjMaterial").Value == objMat.ToString() &&
                                      //     objSalvWork.Contains(p.Element("ObjWork").Value.Substring(0, 1))
                                   Convert.ToInt16(p.Element("ObjWork").Value) < 7
                                  select p));
                            }

                            else if ((objClassName.Contains("Salvage")) && (objSalvWork == "7-8")) //|| (objSalvWork == "9")))
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                  from p in xdocListInv.Element("Objs").Descendants("Obj")
                                  where p.Element("ObjClass").Value.Contains(objClassName) &&
                                  p.Element("ObjMaterial").Value == objMat.ToString() &&
                                      //     objSalvWork.Contains(p.Element("ObjWork").Value.Substring(0, 1))
                                   Convert.ToInt16(p.Element("ObjWork").Value) > 6 && Convert.ToInt16(p.Element("ObjWork").Value) < 9
                                  select p));
                            }

                            else if ((objClassName.Contains("Salvage")) && (objSalvWork == "9"))
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                  from p in xdocListInv.Element("Objs").Descendants("Obj")
                                  where p.Element("ObjClass").Value.Contains(objClassName) &&
                                  p.Element("ObjMaterial").Value == objMat.ToString() &&
                                    Convert.ToInt16(p.Element("ObjWork").Value) == 9
                                  select p));
                            }

                            else if ((objClassName.Contains("Salvage")) && (objSalvWork == "10"))
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                  from p in xdocListInv.Element("Objs").Descendants("Obj")
                                  where p.Element("ObjClass").Value.Contains(objClassName) &&
                                  p.Element("ObjMaterial").Value == objMat.ToString() &&
                                  objSalvWork.ToString() == p.Element("ObjWork").Value
                                  select p));
                            }

                            break;
                        default:

                            if (objClassName != null && lstMySelects != null && lstMySelects[0].Trim() != "")
                            {

                                newDoc = new XDocument(new XElement("Objs",
                                     from p in xdocListInv.Element("Objs").Descendants("Obj")
                                     where p.Element("ObjClass").Value.Contains(objClassName) &&
                                         (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[0]) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[0]))
                                     select p));
                            }

                            else if (objClassName != null && (mySelect == null || mySelect.Trim() == ""))
                            {

                                newDoc = new XDocument(new XElement("Objs",
                                     from p in xdocListInv.Element("Objs").Descendants("Obj")
                                     where p.Element("ObjClass").Value.Contains(objClassName)
                                     select p));
                            }

                            break;



                    } //end of switch
                    //{
                    newDoc.Save(inventorySelect);
                   // WriteToChat("I just saved inventorySelect");
                    int m = lstMySelects.Count;
                    if (m > 1)
                    {
                        for (int k = 1; k < m; k++)
                        {
                            newDoc = XDocument.Load(inventorySelect);
                            XDocument tempDoc;
                            tempDoc = new XDocument(new XElement("Objs",
                                  from p in newDoc.Element("Objs").Descendants("Obj")
                                  where
                                (p.Element("ObjName").Value.ToLower().Contains(lstMySelects[k]) ||
                                  p.Element("ObjSpellXml").Value.ToLower().Contains(lstMySelects[k]))
                                  select p));
                            tempDoc.Save(inventorySelect);

                        }
                    }
                    newDoc=null;
                    newDoc = null;
                    //}
                    //else
                    if ((mySelect == null || mySelect.Trim() == "") && objClassName == "None")

                    { GearFoundry.PluginCore.WriteToChat("You must choose a class or type something inbox"); }




                } //end of try //

                catch (Exception ex) { LogError(ex); }

                newDoc = XDocument.Load(inventorySelect);

                try
                {

                    IEnumerable<XElement> childElements = newDoc.Element("Objs").Descendants("Obj");

                    foreach (XElement childElement in childElements)
                    {
                        try
                        {
                            objIcon = Convert.ToInt32(childElement.Element("ObjIcon").Value);

                            objName = childElement.Element("ObjName").Value;
                            if (objClassName.Contains("Salvage"))
                            {
                                int objMaterial = Convert.ToInt32(childElement.Element("ObjMaterial").Value);
                                string objMaterialName = "";
                                foreach (IDNameLoadable item in MaterialInvList)
                                {
                                    if (item.ID == objMaterial)
                                    {
                                        objMaterialName = item.name;
                                        break;
                                    }
                                }

                                //     objName = objMaterialName + " " + objName;
                            }
                            toonInvName = childElement.Element("ToonName").Value.ToString();
                            long objID = Convert.ToInt32(childElement.Element("ObjID").Value);
                            string objIDstr = objID.ToString();
                            InventoryHudListRow = lstHudInventory.AddRow();
                            ((HudPictureBox)InventoryHudListRow[0]).Image = objIcon + 0x6000000;
                            ((HudStaticText)InventoryHudListRow[1]).FontHeight = nitemFontHeight;
                            ((HudStaticText)InventoryHudListRow[1]).Text = objName;
                            ((HudStaticText)InventoryHudListRow[2]).FontHeight = nitemFontHeight;
                            ((HudStaticText)InventoryHudListRow[2]).Text = toonInvName;
                            ((HudStaticText)InventoryHudListRow[3]).Text = objIDstr;
                        }


                        catch (Exception ex) { LogError(ex); }

                    }
                    newDoc = null;
             
                }
                catch (Exception ex) { LogError(ex); }
            }

        }// end of btnlist


        //[MVControlEvent("lstInventory", "Selected")]
        //private void lstInventory_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e)
        private void lstHudInventory_Click(object sender, int row, int col)
        {
            try
            {

                newDoc = XDocument.Load(inventorySelect);
                IEnumerable<XElement> myelements = newDoc.Element("Objs").Descendants("Obj");
                List<XElement> inventorySelectList = new List<XElement>();
                var lst = from myelement in myelements
                          select myelement;
                inventorySelectList.AddRange(lst);
                XElement element = inventorySelectList[row];

                newDoc = null;
                if (element.Element("GearScore") != null) { objGearScore = Convert.ToInt32(element.Element("GearScore").Value); }
                objName = element.Element("ObjName").Value;
                objID = Convert.ToInt32(element.Element("ObjID").Value);
                toonInvName = element.Element("ToonName").Value;
                objClassName = element.Element("ObjClass").Value;
                objWork = element.Element("ObjWork").Value;
                objTinks = element.Element("ObjTink").Value;
                objSpellXml = element.Element("ObjSpellXml").Value;
                objBurden = element.Element("ObjBurden").Value;
                objStack = element.Element("ObjStackCount").Value;
                int temp = Convert.ToInt16(element.Element("ObjMaterial").Value);
                if (temp > 0)
                {
                    objMatName = MaterialIndex[Convert.ToInt16(element.Element("ObjMaterial").Value)].name;
                }

                    objToonLevel = element.Element("ObjToonLevel").Value;

                    objLore = Convert.ToInt32(element.Element("ObjLoreReq").Value);
                    objWieldAttr = "";
                    long nobjWieldAttr = Convert.ToInt32(element.Element("ObjWieldAttr").Value);
                    if (nobjWieldAttr == 7) { objWieldAttr = "Missile Defense"; }
                    if (nobjWieldAttr == 15) { objWieldAttr = "Magic Defense"; }
                    if (nobjWieldAttr == 6) { objWieldAttr = "Melee Defense"; }
                   
                    
                    objWieldValue = Convert.ToInt32(element.Element("ObjWieldValue").Value);
                    //     if (nobjWieldAttr == 7 || nobjWieldAttr == 6 || nobjWieldAttr == 15)
                       int tempNum = 180;
                        if (nobjWieldAttr > 0)
                        { objLevel = objToonLevel.ToString(); }
                        else if ((objName.Contains("Radiant")) || (objName.Contains("Eldrytch")) || (objName.Contains("Hand")))
                        { objLevel = tempNum.ToString(); }
                        else if (nobjWieldAttr <= 1)
                        { objLevel = objWieldValue.ToString(); }

                        objSkillLevel = Convert.ToInt32(element.Element("ObjSkillLevReq").Value);
                        int objSkillInt = Convert.ToInt32(element.Element("ObjMastery").Value);
                        if (objSkillInt == 15) { objMastery = "Magic Defense"; }
                        if (objSkillInt == 6) { objMastery = "Melee Defense"; }
                        if (objSkillInt == 7) { objMastery = "Missile Defense"; }
                        objLevel = objWieldValue.ToString();
                        wieldMess = "Level to wield: " + objLevel;

                       try{
                          
                          if(objName.Contains("Radiant"))
                           { wieldMess = "Required to wield: Level 180, Radiant Blood Society Level: " + objWieldValue.ToString(); }
                           if(objName.Contains("Eldrytch"))
                           { wieldMess = "Required to wield: Level 180, Eldrytch Webb Society Level: " + objWieldValue.ToString(); }
                           if(objName.Contains("Celestial"))
                           { wieldMess = "Required to wield: Level 180, Celestial Hand Society Level: " + objWieldValue.ToString(); }
                           if ((objWieldAttr.Contains("Magic")) || (objWieldAttr.Contains("Missile")) || (objWieldAttr.Contains("Melee")))
                           { wieldMess = "Required to wield: Level " + objLevel.ToString() + ", " + objWieldAttr + ": " + objWieldValue.ToString(); }
 
                       }

                       catch (Exception ex) { LogError(ex); }

                            skillMess = "; Required for activation: ObjLore - " + objLore.ToString() + ", "
                                           + objMastery + " - " + objSkillLevel.ToString();

 
                    int nobjEmbue = Convert.ToInt32(element.Element("ObjEmbue").Value);
                    if (nobjEmbue == 0) { objEmbue = "None"; }
                    else
                    {
                        foreach (IDNameLoadable piece in EmbueInvList)
                        {
                            if (piece.ID == nobjEmbue)
                            {
                                objEmbue = piece.name;
                                break;
                            }
                        }
                    }

                    int nobjDamageType = Convert.ToInt32(element.Element("ObjDamage").Value);
                    if (nobjDamageType > 0)
                    {
                        foreach (IDNameLoadable piece in ElementalInvList)
                        {
                            if (piece.ID == nobjDamageType)
                            {
                                objDamageType = piece.name;
                                break;
                            }
                        }
                    }


                    if (objClassName.Contains("Armor") || objClassName.Contains("Clothing"))
                    {
                        objSet = Convert.ToInt32(element.Element("ObjSet").Value);
                        if (Convert.ToInt32(element.Element("ObjSet").Value) > 0)
                        { objArmorSetName = SetsIndex[Convert.ToInt32(element.Element("ObjSet").Value)].name; }
                        else
                        {
                            objArmorSetName = "None";
                        }

                        objCovers = (Convert.ToInt32(element.Element("ObjCovers").Value));
                        if (objCovers > 0)
                        {
                            foreach (IDNameLoadable piece in CoverageInvList)
                            {
                                if (piece.ID == objCovers)
                                {
                                    objCoversName = piece.name;
                                    break;
                                }
                            }
                        }
                        else { objCoversName = "Not found"; }
                    }

                    objEnchant = Convert.ToInt32(element.Element("ObjUnknown10").Value);

                    if (objClassName.Contains("Armor"))
                    {
                        objAl = element.Element("ObjAl").Value;

                        if (objEnchant == 1 || objName.Contains("Covenant"))
                        {
                            string objAcid = ((Math.Round(Convert.ToDouble(element.Element("ObjAcid").Value), 4))).ToString();
                            string objLight = ((Math.Round(Convert.ToDouble(element.Element("ObjLight").Value), 4))).ToString();
                            string objFire = ((Math.Round(Convert.ToDouble(element.Element("ObjFire").Value), 4))).ToString();
                            string objCold = ((Math.Round(Convert.ToDouble(element.Element("ObjCold").Value), 4))).ToString();
                            string objBludg = ((Math.Round(Convert.ToDouble(element.Element("ObjBludg").Value), 4))).ToString();
                            string objSlash = ((Math.Round(Convert.ToDouble(element.Element("ObjSlash").Value), 4))).ToString();
                            string objPierce = ((Math.Round(Convert.ToDouble(element.Element("ObjPierce").Value), 4))).ToString();

                            objProts = objSlash + "/" + objPierce + "/" + objBludg + "/" + objFire + "/" + objCold + "/" + objAcid + "/" + objLight;

                        }
                        else { objProts = ""; }
                   }


                    if (objClassName.Contains("Missile"))
                    {
                        objElDam = element.Element("ObjElemDmg").Value;
                        objDamBon = ((Math.Round(Convert.ToDouble(element.Element("ObjDamageBonus").Value), 2) - 1) * 100).ToString();
                        ////  objMissType = element.Element("ObjMissType").Value;

                    }

                    if (objClassName.Contains("Wand"))
                    {
                        if (Convert.ToDouble(element.Element("ObjElemvsMons").Value) > 0)
                        {
                            objElemvsMons = Math.Round(((Convert.ToDouble(element.Element("ObjElemvsMons").Value) - 1) * 100), 2).ToString();
                            if (Convert.ToDouble(objElemvsMons) < 0) { objElemvsMons = "0"; }
                        }
                    }
                    if (Convert.ToDouble(element.Element("ObjMelD").Value) > 0)
                    {
                        objMelD = ((Math.Round(Convert.ToDouble(element.Element("ObjMelD").Value), 2) - 1) * 100).ToString();
                        if (Convert.ToDouble(objMelD) < 0) { objMelD = "0"; }
                    }
                    if (Convert.ToDouble(element.Element("ObjMagicD").Value) > 0)
                    {
                        objMagicD = ((Math.Round(Convert.ToDouble(element.Element("ObjMagicD").Value), 2) - 1) * 100).ToString();
                        if (Convert.ToDouble(objMagicD) < 0) { objMagicD = "0"; }
                    }
                    if (Convert.ToDouble(element.Element("ObjManaC").Value) > 0)
                    {
                        objManaC = (Math.Round(Convert.ToDouble(element.Element("ObjManaC").Value), 2) * 100).ToString();
                        if (Convert.ToDouble(objManaC) < 0) { objManaC = "0"; }
                    }
                    if (Convert.ToDouble(element.Element("ObjMissileD").Value) > 0)
                    {
                        objMissD = ((Math.Round(Convert.ToDouble(element.Element("ObjMissileD").Value), 2) - 1) * 100).ToString();
                        if (Convert.ToDouble(objMissD) < 0) { objMissD = "0"; }
                    }
                    if (Convert.ToDouble(element.Element("ObjSalvWork").Value) > 0)
                    {
                        objSalvWork = ((Math.Round(Convert.ToDouble(element.Element("ObjSalvWork").Value), 2) - 1)).ToString();
                        if (Convert.ToDouble(objSalvWork) < 0) { objSalvWork = "0"; }

                    }
                    if (objClassName.Contains("Melee"))
                    {

                        objAttack = Math.Round((Convert.ToDouble(element.Element("ObjAttack").Value) - 1) * 100).ToString();

                        if (Convert.ToDouble(element.Element("ObjVariance").Value) > 0)
                        {
                            objDVar = Convert.ToDouble(element.Element("ObjVariance").Value);
                        }


                        objMaxDam = element.Element("ObjMaxDamage").Value;
                        objMaxDamLong = Convert.ToInt32(objMaxDam);
                        objMinDam = Math.Round(objMaxDamLong - ((objDVar) * (objMaxDamLong)), 2).ToString();

                    }


                    //   string objSkillLevReq = element.Element("ObjSkillLevReq").Value;
                    ////  objCatType = element.Element("ObjCatType").Value;
                    ////  objCleaveType  = element.Element("ObjCleaveType").Value;
                    ////  objType = element.Element("ObjType").Value;

                    //  objAtt = element.Element("ObjAtt").Value;
                    // objBnd = element.Element("ObjBnd").Value;
                    // //  objSlayer = element.Element("ObjSlayer").Value;
                    // objWieldAttrInt = Convert.ToInt32(element.Element("ObjWieldAttr").Value);
                    // //  objWieldType = element.Element("ObjWieldType").Value;
                    if (objClass == 0)
                    {
                        if (objClassName.Contains("Armor")) { objClass = 1; }
                        if (objClassName.Contains("Clothing")) { objClass = 2; }
                        if (objClassName.Contains("Jewelry")) { objClass = 3; }
                        if (objClassName.Contains("Wand")) { objClass = 4; }
                        if (objClassName.Contains("Melee")) { objClass = 5; }
                        if (objClassName.Contains("Missile")) { objClass = 6; }
                        if (objClassName == "Salvage") { objClass = 7; }
                    }
                    objName = objMatName + " " + objName;

                    message = objName + ", " + toonInvName + ", GS: " + objGearScore.ToString();
                    switch (objClass)
                    {

                        case 1:
                            message = message + ", Al: " + objAl + "; Work: " + objWork + "; Burden: " + objBurden +
                                 "; Tinks: " + objTinks +
                                "; " + wieldMess + "; Set: " + objArmorSetName +
                                "; covers: " + objCoversName + ", " + skillMess + "; " + objSpellXml;

                            if (objProts != "")
                            {


                                message = message + "; Object is unenchantable; " + objProts;
                            }
                            break;
                        case 2:

                            message = message + "Level to wield: " + objLevel  + "; SetName: " + objArmorSetName + "; " + objSpellXml;
                            break;

                        case 3:
                            message = message + ", Level: " + objLevel + ", " + objSpellXml;
                            break;

                        case 4:

                            message = message + ", Damage: " + objDamageType +
                                ", Wield Level: " + objLevel +
                                ", ElemVsMonster: " + objElemvsMons +
                                ", ManaC: " + objManaC + ", MeleeD: " + objMelD + ", MagicD: " + objMagicD +
                                ", MissileD: " + objMissD + ", Embue: " + objEmbue +
                                ", Work: " + objWork + ", Tinks: " + objTinks + ", Lore required to activate: " + objLore.ToString() + ", "
                                + objMastery + objSkillLevel.ToString() + " to activate" + ", " + objSpellXml;
                            break;
                        case 5:

                            message = message + " , Damage: " + objDamageType +
                                ", WieldLevel: " + objLevel +
                                ", Attack: " + objAttack + ", MeleeD: " + objMelD +
                                " Min-Max Damage: " + objMinDam + "-" + objMaxDam +
                                " , Embue: " + objEmbue + ", Work: " + objWork + ", Tinks: " + objTinks +
                                ", MissD: " + objMissD + ", MagicD " + objMagicD + ", " + ", Lore required to activate: " + ", " + objMastery +
                                objSkillLevel.ToString() + " to activate" + objLore.ToString() + objSpellXml;
                            break;
                        case 6:

                            message = message + ", Damage: " + objDamageType + ", WieldLevel: " + objLevel +
                                ", Elem Dmg: " + objElDam +
                                ", Damage Bonus: " + objDamBon + ", MelD: " + objMelD +
                                ", MissD: " + objMissD + ", MagicD: " + objMagicD + " , Embue: " + objEmbue +
                                ", Work: " + objWork + ", Tinks: " + objTinks + ", Lore required to activate: " + objLore.ToString() + ", " + objMastery + objSkillLevel.ToString() + " to activate"
                                + ", " + objSpellXml;
                            break;
                        case 7:
                            message = message + ", Material: " + objMatName + ", Work: " + objSalvWork + ", Burden: " + objBurden;

                            break;
                        case 11:
                            if (!objName.Contains("Aetheria"))
                            {
                                message = message + ", # in Stack: " + objStack;
                            }
                            else if (objName.Contains("Aetheria"))
                                message = message + "Object level: " + objLevel + "; " + objSpellXml;
                            break;


                        default:
                            message = message + ", Burden: " + objBurden + ", Number: " + objStack + ", Spells: " + objSpellXml;
                            break;


                    }



                    GearFoundry.PluginCore.WriteToChat(message);
                    message = null;
               

               }
            
            catch (Exception ex) { LogError(ex); }

        }



        // It helps to clear the list before making new selection
        //[MVControlEvent("btnClrInventory", "Click")]
        //void btnClrInventory_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        private void btnClrInv_Hit(object sender, EventArgs e)
       {
        clearListVariables();
        }

        private void clearListVariables()
        {

            if (lstHudInventory != null) { lstHudInventory.ClearRows(); }
            if (txtMyChoice != null) { txtMyChoice.Text = ""; }
            if (cboInventoryClasses != null) { cboInventoryClasses.Current = 0; }
            if (cboWieldAttrib != null) { cboWieldAttrib.Current = 0; }
            if (cboDamageType != null) { cboDamageType.Current = 0; }
            if (cboLevel != null) { cboLevel.Current = 0; }
            if (cboArmorSet != null) { cboArmorSet.Current = 0; }
            if (cboMaterial != null) { cboMaterial.Current = 0; }
            if (cboCoverage != null) { cboCoverage.Current = 0; }
            if (cboArmorLevel != null) { cboArmorLevel.Current = 0; }
            if (cboSalvWork != null) { cboSalvWork.Current = 0; }
            if (cboEmbues != null) { cboEmbues.Current = 0; }
            objEmbueTypeInt = 0;
            objDamageTypeInt = 0;
            objLevelInt = 1;
            objWieldAttrInt = 0;
            objWieldAttr = "";
            objMastery = "";
            objSkillLevel = 0;
            objLevel = "";
            objToonLevel = "";
            objWieldValue = 0;
            wieldMess = "";
            skillMess = "";


            objSalvWork = "None";
            objClassName = "";
            objMat = 0;
            objCovers = 0;
            objCoversName = "";
            objArmorLevel = 1;
            objSet = 0;
            newDoc = new XDocument(new XElement("Objs"));
            newDoc.Save(inventorySelect);


            newDoc = null;
            mySelect = "";
            objClass = 0;
        }

    } // end of partial class

} //end of namespace


