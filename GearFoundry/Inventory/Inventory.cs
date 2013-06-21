using System;
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
        WindowsTimer mInventoryTimer = null;
        int inventoryTimer = 0;
        private bool InventoryMainTab;
        private bool InventorySettingsTab;
        private int InventoryHudWidth = 0;
        private int InventoryHudHeight = 0;
        private int InventoryHudFirstWidth = 530;
        private int InventoryHudFirstHeight = 500;
        private int InventoryHudWidthNew;
        private int InventoryHudHeightNew;
        private HudFixedLayout InventoryHudLayout = null;
        private HudTabView InventoryHudTabView = null;
        private HudFixedLayout InventoryHudTabLayout = null;
        private const int InventoryRemoveCircle = 0x60011F8;

        private HudFixedLayout InventoryHudSettings;
        private HudView InventoryHudView;

        private string inventoryFilename = null;
        private string genInventoryFilename = null;
        private string holdingInventoryFilename = null;
        private static bool binventoryEnabled;
        private static bool binventoryBurdenEnabled;
        private static bool binventoryCompleteEnabled;
        private static bool binventoryWaitingEnabled;
        private XDocument xdocInventorySettings;

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





        private void RenderInventoryHud()
        {

            try
            {

                WriteToChat("I am in Render Inventory hud");
                if (InventoryHudView != null)
                {
                    DisposeInventoryHud();
                }
                if (armorSettingsFilename == "" || armorSettingsFilename == null) { armorSettingsFilename = GearDir + @"\ArmorSettings.xml"; }


                if (InventoryHudWidth == 0)
                {
                    try
                    {
                        getArmorHudSettings();
                        WriteToChat("Inventoryhudwidth: " + InventoryHudWidth.ToString());
                    }
                    catch (Exception ex) { LogError(ex); }
 
                }
                if (InventoryHudWidth == 0) { InventoryHudWidth = InventoryHudFirstWidth;  }
                if (InventoryHudHeight == 0) { InventoryHudHeight = InventoryHudFirstHeight; }

                InventoryHudView = new HudView("Inventory", InventoryHudWidth, InventoryHudHeight, new ACImage(0x6AA5));
                InventoryHudView.UserAlphaChangeable = false;
                InventoryHudView.ShowInBar = false;
                InventoryHudView.UserResizeable = false;
                InventoryHudView.Visible = true;
                InventoryHudView.Ghosted = false;
                InventoryHudView.UserMinimizable = false;
                InventoryHudView.UserClickThroughable = false;
                InventoryHudView.LoadUserSettings();

                InventoryHudLayout = new HudFixedLayout();
                InventoryHudView.Controls.HeadControl = InventoryHudLayout;

                InventoryHudTabView = new HudTabView();
                InventoryHudLayout.AddControl(InventoryHudTabView, new Rectangle(0, 0, InventoryHudWidth, InventoryHudHeight));

                InventoryHudTabLayout = new HudFixedLayout();
                InventoryHudTabView.AddTab(InventoryHudTabLayout, "Inventory");

                InventoryHudSettings = new HudFixedLayout();
                InventoryHudTabView.AddTab(InventoryHudSettings, "Settings");

                InventoryHudTabView.OpenTabChange += InventoryHudTabView_OpenTabChange;
                InventoryHudView.Resize += InventoryHudView_Resize;
                InventoryHudView.UserResizeable = true;
                RenderInventoryTabLayout();

                //      SubscribeArmorEvents();

            }
            catch (Exception ex) { LogError(ex); }
            return;
        }

        private void InventoryHudView_Resize(object sender, System.EventArgs e)
        {
            try
            {
                bool bw = Math.Abs(InventoryHudView.Width - InventoryHudWidth) > 20;
                bool bh = Math.Abs(InventoryHudView.Height - InventoryHudHeight) > 20;
                if (bh || bw)
                {
                    InventoryHudWidthNew = InventoryHudView.Width;
                    InventoryHudHeightNew = InventoryHudView.Height;
                    MasterTimer.Tick += InventoryResizeTimerTick;
                }
            }
            catch (Exception ex) { LogError(ex); }
            return;



        }

        private void InventoryResizeTimerTick(object sender, EventArgs e)
        {
            InventoryHudWidth = InventoryHudWidthNew;
            InventoryHudHeight = InventoryHudHeightNew;
            MasterTimer.Tick -= InventoryResizeTimerTick;
            SaveInventorySettings();
            RenderInventoryHud();

        }

        private void SaveInventorySettings()
        {
            try
            {
                if (armorSettingsFilename == "" || armorSettingsFilename == null) { armorSettingsFilename = GearDir + @"\ArmorSettings.xml"; }
                WriteToChat("I am in save inventory settings and inventory settings filename is " + armorSettingsFilename);
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



        private void InventoryHudTabView_OpenTabChange(object sender, System.EventArgs e)
        {
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
                lblMyChoice.Text = "Type preference:";
 
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
                    lstHudInventory.AddColumn(typeof(HudStaticText), Convert.ToInt32(.5 * InventoryHudWidth), null);
                    lstHudInventory.AddColumn(typeof(HudStaticText), Convert.ToInt32(.44 * InventoryHudWidth), null);
                    lstHudInventory.AddColumn(typeof(HudStaticText), Convert.ToInt32(.001 * InventoryHudWidth), null);

                    lstHudInventory.Click += (sender, row, col) => lstHudInventory_Click(sender, row, col);
                }
                            
                catch (Exception ex) { LogError(ex); }

                InventoryHudTabLayout.AddControl(lblInventoryClass, new Rectangle(10, 10, 30, 16));
                InventoryHudTabLayout.AddControl(cboInventoryClasses, new Rectangle(45, 10, 100, 16));
                InventoryHudTabLayout.AddControl(lblMyChoice, new Rectangle(155, 10, 100, 16));
                InventoryHudTabLayout.AddControl(txtMyChoice, new Rectangle(260, 10, 280, 16));
                InventoryHudTabLayout.AddControl(lblWeapons, new Rectangle(10,30,InventoryHudWidth/3,20));
                InventoryHudTabLayout.AddControl(lblArmor, new Rectangle(InventoryHudWidth/3, 30, InventoryHudWidth / 3, 20));
                InventoryHudTabLayout.AddControl(lblSalvage, new Rectangle((2 * InventoryHudWidth) / 3, 30, InventoryHudWidth / 3, 20));

                InventoryHudTabLayout.AddControl(lblMelee, new Rectangle(10, 50, 25, 16));
                InventoryHudTabLayout.AddControl(cboWieldAttrib, new Rectangle(40, 50, 100, 16));
                InventoryHudTabLayout.AddControl(lblSet, new Rectangle(InventoryHudWidth/3, 50, 25, 16));
                InventoryHudTabLayout.AddControl(cboArmorSet, new Rectangle(InventoryHudWidth/ 3 + 30, 50, 150, 16));
                InventoryHudTabLayout.AddControl(lblMaterial, new Rectangle((2 * InventoryHudWidth) / 3, 50, 25, 16));
                InventoryHudTabLayout.AddControl(cboMaterial, new Rectangle((2 * InventoryHudWidth) / 3 + 30, 50, 150, 16));
                InventoryHudTabLayout.AddControl(lblDamage, new Rectangle(10, 70, 25, 16));
                InventoryHudTabLayout.AddControl(cboDamageType, new Rectangle(40, 70, 100, 16));
                InventoryHudTabLayout.AddControl(lblArmorWield, new Rectangle(InventoryHudWidth / 3, 70, 25, 16));
                InventoryHudTabLayout.AddControl(cboArmorLevel, new Rectangle(InventoryHudWidth / 3 + 30, 70, 100, 16));
                InventoryHudTabLayout.AddControl(lblWork, new Rectangle((2 * InventoryHudWidth) / 3, 70, 25, 16));
                InventoryHudTabLayout.AddControl(cboSalvWork, new Rectangle((2 * InventoryHudWidth) / 3 + 30, 70, 100, 16));
                InventoryHudTabLayout.AddControl(lblWield, new Rectangle(10, 90, 25, 16));
                InventoryHudTabLayout.AddControl(cboLevel, new Rectangle(40,90, 100, 16));
                InventoryHudTabLayout.AddControl(lblCovers, new Rectangle(InventoryHudWidth / 3, 90, 25, 16));
                InventoryHudTabLayout.AddControl(cboCoverage, new Rectangle(InventoryHudWidth / 3 + 30, 90, 100, 16));
                InventoryHudTabLayout.AddControl(lblEmbues, new Rectangle(10, 110, 25, 16));
                InventoryHudTabLayout.AddControl(cboEmbues, new Rectangle(40, 110, 100, 16));

                InventoryHudTabLayout.AddControl(btnLstInv, new Rectangle((2* InventoryHudWidth)/3,100,100,16));
                InventoryHudTabLayout.AddControl(btnClrInv, new Rectangle((2 * InventoryHudWidth) / 3, 120, 100, 16));
                InventoryHudTabLayout.AddControl(lstHudInventory, new Rectangle(10, 150, InventoryHudWidth, InventoryHudHeight - 155));
                

                InventoryMainTab = true;
                try
                {
                   // FillArmorHudList();
                }

                catch (Exception ex) { LogError(ex); }




            }

            catch (Exception ex) { LogError(ex); }
        }


        private void DisposeInventoryTabLayout()
        {
            try
            {
                if (!InventoryMainTab) { return; }
                cboInventoryClasses.Change -= (sender, index) => cboInventoryClasses_Change(sender, index);
                cboInventoryClasses = null;


                //InventoryHudList.Click -= (sender, row, col) => InventoryHudList_Click(sender, row, col);
                //InventoryHudList.Dispose();

                InventoryMainTab = false;


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



                //lstAllToonName = new List<string>();
                //try
                //{
                //    string name = "";
                //    foreach (XElement el in names)
                //    {
                //        name = el.Element("ToonName").Value;
                //        int i = 0;
                //        if (!lstAllToonName.Contains(name))
                //        {
                //            try
                //            {
                //                lstAllToonName.Add(name);
                //                cboToonArmorName.AddItem(name, i);
                //                i++;
                //            }
                //            catch (Exception ex) { LogError(ex); }

                //        }
                //    }
                //}
                //catch (Exception ex) { LogError(ex); }


                //lblToonArmorNameInfo = new HudStaticText();
                //lblToonArmorNameInfo.Text = "Name of toon whose armor is being studied:";

                //ArmorHudSettings.AddControl(btnInventoryArmor, new Rectangle(5, 20, 100, 20));

                //ArmorHudSettings.AddControl(lblToonArmorNameInfo, new Rectangle(5, 60, ArmorHudWidth, 16));

                //ArmorHudSettings.AddControl(cboToonArmorName, new Rectangle(10, 75, ArmorHudWidth - 20, 16));



                InventorySettingsTab = true;
            }
            catch (Exception ex) { LogError(ex); }
        }

        //private void cboToonArmorName_Change(object sender, EventArgs e)
        //{
        //    toonArmorName = lstAllToonName[cboToonArmorName.Current];
        //    //  WriteToChat(toonArmorName + "has been selected");
        //    lblToonArmorName.Text = toonArmorName;


        //}


        private void DisposeInventorySettingsLayout()
        {
            try
            {
                if (!InventorySettingsTab) { return; }
                btnInventoryUpdate.Hit -= (sender, index) => btnInventoryUpdate_Hit(sender, index);
                btnInventoryComplete.Hit -= (sender, index) => btnInventoryComplete_Hit(sender, index);
                btnInventoryStacks.Hit -= (sender, index) => btnInventoryStacks_Hit(sender, index);
                btnInventoryComplete = null;
                btnInventoryUpdate = null;
                btnInventoryStacks = null;



                InventorySettingsTab = false;
            }
            catch { }
        }



        private void DisposeInventoryHud()
        {

            try
            {
                SaveInventorySettings();
                UnsubscribeInventoryEvents();
                try { DisposeInventoryTabLayout(); }
                catch { }
                try { DisposeInventorySettingsLayout(); }
                catch { }

                InventoryHudSettings.Dispose();
                InventoryHudLayout.Dispose();
                InventoryHudTabLayout.Dispose();
                InventoryHudTabView.Dispose();
                InventoryHudView.Dispose();
            }
            catch (Exception ex) { LogError(ex); }
            return;
        }

        private void UnsubscribeInventoryEvents()
        {
            InventoryHudTabView.OpenTabChange -= InventoryHudTabView_OpenTabChange;
            InventoryHudView.Resize -= InventoryHudView_Resize;
            MasterTimer.Tick -= InventoryResizeTimerTick;


        }


        private void btnInventoryUpdate_Hit(object sender, EventArgs e)
        {

            doUpdateInventory();
        }

        private void btnInventoryComplete_Hit(object sender, EventArgs e)
        {
            {
                m = 500;
                doGetInventory();
            }

        }

        void btnInventoryStacks_Hit(object sender, EventArgs e)
        {
             getBurden = true;
            doUpdateInventory();
        }

        void cboInventoryClasses_Change(object sender, EventArgs index)
        {

            try
            {
 
                objClass = ClassInvList[cboInventoryClasses.Current].ID;
                objClassName = ClassInvList[cboInventoryClasses.Current].name;
                GearFoundry.PluginCore.WriteToChat("Class: " + objClassName.ToString() + "; objClass: " + objClass.ToString());

            }
            catch (Exception ex) { LogError(ex); }


        }

        void cboWieldAttrib_Change(object sender, EventArgs e)
        {
            try
            {
                objWieldAttrInt = MeleeTypeInvList[cboWieldAttrib.Current].ID;
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboArmorSet_Change(object sender, EventArgs e)
        {
            try
            {
                //  objArmorSet = ArmorSetsInvList[cmbArmorSet.Selected].ID;
                objArmorSet = ArmorSetsInvList[cboArmorSet.Current].ID;
                objArmorSetName = ArmorSetsInvList[cboArmorSet.Current].name;

            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboMaterial_Change(object sender, EventArgs e)
        {
            try
            {
                objMat = MaterialInvList[cboMaterial.Current].ID;
                objMatName = MaterialInvList[cboMaterial.Current].name;
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboDamageType_Change(object sender, EventArgs e)
        {
            try
            {
                objDamageType = ElementalInvList[cboDamageType.Current].name;
                objDamageTypeInt = ElementalInvList[cboDamageType.Current].ID;
            }
            catch (Exception ex) { LogError(ex); }
        }

 
        void cboArmorLevel_Change(object sender, EventArgs e)
        {
            try
            {
                objArmorLevel = Convert.ToInt16(ArmorLevelInvList[cboArmorLevel.Current].name);

            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboSalvWork_Change(object sender, EventArgs e)
        {
            try
            {
                objSalvWork = SalvageWorkInvList[cboSalvWork.Current].name;
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboLevel_Change(object sender, EventArgs e)
        {
            try
            {
                objLevelInt = Convert.ToInt32(WeaponWieldInvList[cboLevel.Current].name);
           }
            catch (Exception ex) { LogError(ex); }
        }

        void cboCoverage_Change(object sender, EventArgs e)
        {
            try
            {
                objCovers = CoverageInvList[cboCoverage.Current].ID;
                objCoversName = CoverageInvList[cboCoverage.Current].name;
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboEmbues_Change(object sender, EventArgs e)
        {
            try
            {
                objEmbueTypeInt = EmbueInvList[cboEmbues.Current].ID;
                objEmbueTypeStr = EmbueInvList[cboEmbues.Current].name;

            }
            catch (Exception ex) { LogError(ex); }
        }






        //private void ArmorHudList_Click(object sender, int row, int col)
        //{
        //    try
        //    {
        //        int mrow = row;
        //        currentel = myChoice[row];
        //        string armorobjName = currentel.Element("ArmorName").Value;
        //        string armorobjAl = currentel.Element("ArmorAl").Value;
        //        string armorobjWork = currentel.Element("ArmorWork").Value;
        //        string armorobjTinks = currentel.Element("ArmorTink").Value;
        //        string armorobjLevel = currentel.Element("ArmorWieldValue").Value;
        //        int armorobjArmorSet = Convert.ToInt32(currentel.Element("ArmorSet").Value);
        //        int armorobjCovers = Convert.ToInt32(currentel.Element("ArmorCovers").Value);
        //        string objArmorSetName = SetsIndex[armorobjArmorSet].name;

        //        message = armorobjName + ", Al: " + armorobjAl + " , Work: " + armorobjWork + ", Tinks: " + armorobjTinks + ", Armor Wield Level: " +
        //            armorobjLevel + ", Set: " + objArmorSetName;
        //        WriteToChat(message);


        //        UpdateLandscapeHud();

        //    }
        //    catch (Exception ex) { LogError(ex); }
        //    return;
        //}

        
        
        void btnUpdateInventory_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            GearFoundry.PluginCore.WriteToChat("The button to update inventory was clicked");
            doUpdateInventory();
        }

        void btnGetBurden_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            GearFoundry.PluginCore.WriteToChat("The button to update burden was clicked");
            getBurden = true;
            doUpdateInventory();
        }

        void btnItemsWaiting_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            if (!binventoryWaitingEnabled)
            { binventoryWaitingEnabled = true; }
            else
            { binventoryWaitingEnabled = false; }
            
        }

        private void doCheckFiles()
        {
            if(!File.Exists(genInventoryFilename))
            {
                 XDocument tempDoc = new XDocument(new XElement("Objs"));
                 tempDoc.Save(genInventoryFilename);
                 tempDoc = null;

            }
            if(!File.Exists(inventoryFilename))
            {
                 XDocument tempDoc = new XDocument(new XElement("Objs"));
                 tempDoc.Save(inventoryFilename);
                 tempDoc = null;
            }
         }


        private void doUpdateInventory()
        {
            try
            {
                doCheckFiles();
                //Need a timer for processing inventory
                mWaitingForIDTimer = new WindowsTimer();
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
                           // xdocToonInventory.Save(inventoryFilename);


                            GearFoundry.PluginCore.WriteToChat("Count before removal = " + oldCount + ".  Count after removal = " + newCount);
                        }
                      //  xdoc = XDocument.Load(inventoryFilename);

                        IEnumerable<XElement> elements = xdocToonInventory.Element("Objs").Descendants("ObjID");
                        foreach (XElement element in elements)
                        {
                            //Create list of the ID's currently in the inventory
                            { moldObjsID.Add(element.Value); }
                        }
 

                    }
                    catch (Exception ex) {WriteToChat("I am in the catch exception"); mgoonInv = false; doGetInventory(); LogError(ex);}


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
                        // initialize event timer for processing inventory
                        mWaitingForIDTimer.Tick += new EventHandler(TimerEventProcessor);

                       //  Sets the timer interval to 5 seconds.
                        mWaitingForIDTimer.Interval = 10000;
                        ProcessDataInventory(); // This one in the doupdate
                    }
                    //Now need to start routines that will continue to get data as becomes available or will end the search and save the files
                   mIsFinished();  

                    mIsFinished();  

                }

            } //end of try
            catch (Exception ex) { LogError(ex); }
        }

        void btnGetInventory_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
             m = 500;
            doGetInventory();
        }

        // The following code has to do with selection of inventory to display in listbox
        // First it is necessary to choose the class of inventory; ie, weapons, armor etc. 
        [MVControlEvent("cmbSelectClass", "Change")]
        void cmbSelectClass_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
                objClass = ClassInvList[cmbSelectClass.Selected].ID;
                 objClassName = ClassInvList[cmbSelectClass.Selected].name;
               GearFoundry.PluginCore.WriteToChat("Class: " + objClassName.ToString() + "; objClass: " + objClass.ToString());

            }
            catch (Exception ex) { LogError(ex); }
        }


        // In case of  Weapons will want to find weapons of specific type; e.g., missile
        [MVControlEvent("cmbWieldAttrib", "Change")]
        void cmbWieldAttrib_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
                objWieldAttrInt = MeleeTypeInvList[cmbWieldAttrib.Selected].ID;
            }
            catch (Exception ex) { LogError(ex); }
        }

        // Need to determine damage type of weapon or wand
        [MVControlEvent("cmbDamageType", "Change")]
        void cmbDamageType_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
                objDamageType = ElementalInvList[cmbDamageType.Selected].name;
                objDamageTypeInt = ElementalInvList[cmbDamageType.Selected].ID;
            //    int tempeDamageTypeInt = cmbDamageType.Selected;
            //    findDamageTypeInt(tempeDamageTypeInt);
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cmbLevel_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
                objLevelInt = Convert.ToInt32(WeaponWieldInvList[cmbLevel.Selected].name);
                //int tempeLevelInt = cmbLevel.Selected;
                //findLevelInt(tempeLevelInt);
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cmbArmorSet_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
              //  objArmorSet = ArmorSetsInvList[cmbArmorSet.Selected].ID;
                objArmorSet = ArmorSetsInvList[cmbArmorSet.Selected].ID;
                objArmorSetName = ArmorSetsInvList[cmbArmorSet.Selected].name;
           
             }
            catch (Exception ex) { LogError(ex); }
        }

        void cmbArmorLevel_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
                objArmorLevel = Convert.ToInt16(ArmorLevelInvList[cmbArmorLevel.Selected].name);
            //    int tempeArmorLevelInt = cmbArmorLevel.Selected;
            //    findArmorLevelInt(tempeArmorLevelInt);
                GearFoundry.PluginCore.WriteToChat("ArmorLevel = " + ArmorLevelInvList[cmbArmorLevel.Selected].name); 

            }
            catch (Exception ex) { LogError(ex); }
        }

        void cmbCoverage_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
                objCovers = CoverageInvList[cmbCoverage.Selected].ID;
                objCoversName = CoverageInvList[cmbCoverage.Selected].name;
                //int tempeCoverageInt = cmbCoverage.Selected;
                //findArmorCoverage(tempeCoverageInt);
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cmbMaterial_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try{
                             objMat = MaterialInvList[cmbMaterial.Selected].ID;
                            objMatName = MaterialInvList[cmbMaterial.Selected].name;
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cmbSalvWork_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
                objSalvWork = SalvageWorkInvList[cmbSalvWork.Selected].name;
            ////    int tempeSalvWorkInt = cmbSalvWork.Selected;
            ////    findobjSalvWork(tempeSalvWorkInt);
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cmbEmbue_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
                objEmbueTypeInt = EmbueInvList[cmbEmbue.Selected].ID;
                objEmbueTypeStr = EmbueInvList[cmbEmbue.Selected].name;

            //    int tempeEmbueInt = cmbEmbue.Selected;
            //    findEmbueTypeInt(tempeEmbueInt);
            }
            catch (Exception ex) { LogError(ex); }
        }



        // items selected need to be added to listview: lstinventory
        //[MVControlEvent("btnLstInventory", "Click")]
        //void btnLstInventory_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        private void btnLstInv_Hit(object sender, EventArgs e)
        {
            try
            {
               XDocument tempDoc = new XDocument(new XElement("Objs"));
                tempDoc.Save(inventorySelect);
                tempDoc = null;
                mySelect = null;

                if (txbSelect.Text != null)
                {
                    mySelect = txbSelect.Text.Trim();
                    mySelect = mySelect.ToLower();
                }
                else
                { mySelect = null; }
                xdoc = XDocument.Load(genInventoryFilename);
            }//end of try //

            catch (Exception ex) { LogError(ex); }


            try
            {
                switch (objClass)
                {

                    case 0:

                        if (mySelect != null && mySelect != "")
                        {

                            newDoc = new XDocument(new XElement("Objs",
                             from p in xdoc.Element("Objs").Descendants("Obj")
                             where p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                            p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect)
                             select p));

                        }
                        else if (mySelect == null || mySelect == "")
                        { GearFoundry.PluginCore.WriteToChat("You must choose a class or type something inbox"); }

                        break;
                    case 1:
                    case 2:
                    case 11:
                        if (mySelect != null && mySelect.Trim() != "")
                        {
                            if (objArmorSet == 0 && objArmorLevel == 1 && objCovers == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                from p in xdoc.Element("Objs").Descendants("Obj")
                                where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                      p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                select p));

                            }


                            else if (objArmorSet > 0 && objArmorLevel == 1 && objCovers == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                from p in xdoc.Element("Objs").Descendants("Obj")
                                where p.Element("ObjClass").Value.Contains(objClassName) &&
                                    p.Element("ObjSet").Value == objArmorSet.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                      p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                select p));

                            }
                            else if ((objArmorLevel > 1 || objArmorLevel < 1) && objArmorSet == 0 && objCovers == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                from p in xdoc.Element("Objs").Descendants("Obj")
                                where p.Element("ObjClass").Value.Contains(objClassName) &&
                                    p.Element("ObjWieldValue").Value == objArmorLevel.ToString() &&
                                      (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                      p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                select p));
                            }
                            else if (objCovers > 0 && objArmorSet == 0 && objArmorLevel == 1)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                from p in xdoc.Element("Objs").Descendants("Obj")
                                where p.Element("ObjClass").Value.Contains(objClassName) &&
                                      p.Element("ObjCovers").Value == objCovers.ToString() &&
                                      (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                      p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                select p));
                            }
                            else if (objArmorSet > 0 && (objArmorLevel < 1 || objArmorLevel > 1) && objCovers == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                        p.Element("ObjSet").Value == objArmorSet.ToString() &&
                                       p.Element("ObjWieldValue").Value == objArmorLevel.ToString() &&
                                          (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                          p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            else if (objArmorSet > 0 && objCovers > 0 && objArmorLevel == 1)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                from p in xdoc.Element("Objs").Descendants("Obj")
                                where p.Element("ObjClass").Value.Contains(objClassName) &&
                                    p.Element("ObjSet").Value == objArmorSet.ToString() &&
                                      p.Element("ObjCovers").Value == objCovers.ToString() &&
                                      (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                      p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                select p));
                            }
                            else if (objArmorSet == 0 && (objArmorLevel > 1 || objArmorLevel < 1) && objCovers > 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                from p in xdoc.Element("Objs").Descendants("Obj")
                                where p.Element("ObjClass").Value.Contains(objClassName) &&
                                   p.Element("ObjWieldValue").Value == objArmorLevel.ToString() &&
                                    p.Element("ObjCovers").Value == objCovers.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                      p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                select p));
                            }
                            else if (objArmorSet > 0 && (objArmorLevel > 1 || objArmorLevel < 1) && objCovers > 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                 from p in xdoc.Element("Objs").Descendants("Obj")
                                 where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjSet").Value == objArmorSet.ToString() &&
                                      p.Element("ObjWieldValue").Value == objArmorLevel.ToString() &&
                                       p.Element("ObjCovers").Value == objCovers.ToString() &&
                                       (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                       p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                 select p));
                            }
                        }

                        else
                        {
                             if (objArmorSet == 0 && objArmorLevel == 1 && objCovers == 0)
                            {

                                newDoc = new XDocument(new XElement("Objs",
                                from p in xdoc.Element("Objs").Descendants("Obj")
                                where p.Element("ObjClass").Value.Contains(objClassName)
                                select p));
                            }

                            else if (objArmorSet > 0 && objArmorLevel == 1 && objCovers == 0)
                            {
 
                                newDoc = new XDocument(new XElement("Objs",
                                from p in xdoc.Element("Objs").Descendants("Obj")
                                where p.Element("ObjClass").Value.Contains(objClassName) &&
                                    p.Element("ObjSet").Value == objArmorSet.ToString()
                                select p));
                            }
                            else if ((objArmorLevel > 1 || objArmorLevel < 1) && objArmorSet == 0 && objCovers == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                from p in xdoc.Element("Objs").Descendants("Obj")
                                where p.Element("ObjClass").Value.Contains(objClassName) &&
                                   p.Element("ObjWieldValue").Value == objArmorLevel.ToString()
                                select p));
                            }
                            else if (objCovers > 0 && objArmorSet == 0 && objArmorLevel == 1)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                from p in xdoc.Element("Objs").Descendants("Obj")
                                where p.Element("ObjClass").Value.Contains(objClassName) &&
                                      p.Element("ObjCovers").Value == objCovers.ToString()
                                select p));
                            }
                            else if (objArmorSet > 0 && (objArmorLevel < 1 || objArmorLevel > 1) && objCovers == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                        p.Element("ObjSet").Value == objArmorSet.ToString() &&
                                       p.Element("ObjWieldValue").Value == objArmorLevel.ToString()
                                    select p));
                            }
                            else if (objArmorSet > 0 && objCovers > 0 && objArmorLevel == 1)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                from p in xdoc.Element("Objs").Descendants("Obj")
                                where p.Element("ObjClass").Value.Contains(objClassName) &&
                                    p.Element("ObjSet").Value == objArmorSet.ToString() &&
                                      p.Element("ObjCovers").Value == objCovers.ToString()
                                select p));
                            }
                            else if (objArmorSet == 0 && (objArmorLevel > 1 || objArmorLevel < 1) && objCovers > 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                from p in xdoc.Element("Objs").Descendants("Obj")
                                where p.Element("ObjClass").Value.Contains(objClassName) &&
                                   p.Element("ObjWieldValue").Value == objArmorLevel.ToString() &&
                                    p.Element("ObjCovers").Value == objCovers.ToString()
                                select p));
                            }
                            else if (objArmorSet > 0 && (objArmorLevel > 1 || objArmorLevel < 1) && objCovers > 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                 from p in xdoc.Element("Objs").Descendants("Obj")
                                 where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjSet").Value == objArmorSet.ToString() &&
                                      p.Element("ObjWieldValue").Value == objArmorLevel.ToString() &&
                                       p.Element("ObjCovers").Value == objCovers.ToString()
                                 select p));
                            }


                        }  //end of if spells
                        break;
                    case 5:
                        if (mySelect != null && mySelect != "")
                        {
                            if (objWieldAttrInt == 0 && objDamageTypeInt == 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }


                            else if (objWieldAttrInt > 0 && objDamageTypeInt == 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                                
                            else if (objDamageTypeInt > 0 && objWieldAttrInt == 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            else if ((objLevelInt < 1 || objLevelInt > 1) && objWieldAttrInt == 0 && objDamageTypeInt == 0 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            else if (objEmbueTypeInt > 0 && objWieldAttrInt == 0 && objDamageTypeInt == 1 && objLevelInt == 1)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            else if (objWieldAttrInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objDamageTypeInt == 0 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            else if (objWieldAttrInt > 0 && objEmbueTypeInt > 0 && objDamageTypeInt == 0 && objLevelInt == 1)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                     p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            else if (objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objWieldAttrInt == 0 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            else if (objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objWieldAttrInt == 0 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            else if (objDamageTypeInt > 0 && objEmbueTypeInt > 0 && objWieldAttrInt == 0 && (objLevelInt == 1))
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                     p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                    (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            else if ((objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0 && objWieldAttrInt == 0 && objDamageTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                     p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && objEmbueTypeInt > 0 && objLevelInt == 1)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                     p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            else if (objWieldAttrInt > 0 && objDamageTypeInt == 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                     p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            else if (objWieldAttrInt == 0 && objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                     p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                     p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                        } //end of case 5 with spells
                        else
                        {
                            if (objWieldAttrInt == 0 && objDamageTypeInt == 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName)
                                    select p));
                            }


                            else if (objWieldAttrInt > 0 && objDamageTypeInt == 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString()
                                    select p));
                            }

                            else if (objDamageTypeInt > 0 && objWieldAttrInt == 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString()
                                    select p));
                            }

                            else if ((objLevelInt < 1 || objLevelInt > 1) && objWieldAttrInt == 0 && objDamageTypeInt == 0 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString()
                                    select p));
                            }
                            else if (objEmbueTypeInt > 0 && objWieldAttrInt == 0 && objDamageTypeInt == 1 && objLevelInt == 1)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                    select p));
                            }
                            else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString()
                                    select p));
                            }
                            else if (objWieldAttrInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objDamageTypeInt == 0 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString()
                                    select p));
                            }
                            else if (objWieldAttrInt > 0 && objEmbueTypeInt > 0 && objDamageTypeInt == 0 && objLevelInt == 1)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                     p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                    select p));
                            }
                            else if (objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objWieldAttrInt == 0 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString()
                                    select p));
                            }
                            else if (objDamageTypeInt > 0 && objEmbueTypeInt > 0 && objWieldAttrInt == 0 && (objLevelInt == 1))
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                     p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                    select p));
                            }

                            else if ((objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0 && objWieldAttrInt == 0 && objDamageTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                     p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                    select p));
                            }
                            else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString()
                                    select p));
                            }
                            else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && objEmbueTypeInt > 0 && objLevelInt == 1)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                     p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                    select p));
                            }
                            else if (objWieldAttrInt > 0 && objDamageTypeInt == 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjWieldAttr").Value == objWieldAttrInt.ToString() &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                     p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                    select p));
                            }
                            else if (objWieldAttrInt == 0 && objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     p.Element("ObjDamage").Value == objDamageTypeInt.ToString() &&
                                     p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                     p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                    select p));
                            }
                            else if (objWieldAttrInt > 0 && objDamageTypeInt > 0 && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
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
                        if (mySelect != null && mySelect != "")
                        {
                            if (objDamageTypeInt == 0 && objMagicDamageInt == 0 && objLevelInt == 1 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                      p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }


                            if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && (objLevelInt == 1) && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     (p.Element("ObjDamage").Value == objDamageTypeInt.ToString() ||
                                      p.Element("ObjMagicDamage").Value == objDamageTypeInt.ToString()) &&
                                     (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                      p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            if ((objLevelInt < 1 || objLevelInt > 1) && objDamageTypeInt == 0 && objMagicDamageInt == 0 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                    p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                    (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                    p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            if (objEmbueTypeInt > 0 && objDamageTypeInt == 0 && objMagicDamageInt == 0 && objLevelInt == 1)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                    p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                    (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                    p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }

                            if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     (p.Element("ObjDamage").Value == objDamageTypeInt.ToString() ||
                                      p.Element("ObjMagicDamage").Value == objDamageTypeInt.ToString()) &&
                                    p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                    (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                    p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && objEmbueTypeInt > 0 && objLevelInt == 1)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     (p.Element("ObjDamage").Value == objDamageTypeInt.ToString() ||
                                      p.Element("ObjMagicDamage").Value == objDamageTypeInt.ToString()) &&
                                    p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                    (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                    p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }
                            if ((objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0 && objDamageTypeInt == 0 && objMagicDamageInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                    p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                    p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                    (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                    p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }

                            if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     (p.Element("ObjDamage").Value == objDamageTypeInt.ToString() ||
                                      p.Element("ObjMagicDamage").Value == objDamageTypeInt.ToString()) &&
                                    p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                    p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString() &&
                                    (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                    p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                    select p));
                            }

                        }
                        else
                        {
                            if (objDamageTypeInt == 0 && objMagicDamageInt == 0 && (objLevelInt == 1) && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName)
                                    select p));
                            }

                            if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && (objLevelInt == 1) && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     (p.Element("ObjDamage").Value == objDamageTypeInt.ToString() ||
                                      p.Element("ObjMagicDamage").Value == objDamageTypeInt.ToString())
                                    select p));
                            }
                            if ((objLevelInt < 1 || objLevelInt > 1) && objDamageTypeInt == 0 && objMagicDamageInt == 0 && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                    p.Element("ObjWieldValue").Value == objLevelInt.ToString()
                                    select p));
                            }
                            if (objEmbueTypeInt > 0 && objDamageTypeInt == 0 && objMagicDamageInt == 0 && (objLevelInt == 1))
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                    p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                    select p));
                            }
                            if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     (p.Element("ObjDamage").Value == objDamageTypeInt.ToString() ||
                                      p.Element("ObjMagicDamage").Value == objDamageTypeInt.ToString()) &&
                                    p.Element("ObjWieldValue").Value == objLevelInt.ToString()
                                    select p));
                            }

                            if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && objEmbueTypeInt > 0 && objLevelInt == 1)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                     (p.Element("ObjDamage").Value == objDamageTypeInt.ToString() ||
                                      p.Element("ObjMagicDamage").Value == objDamageTypeInt.ToString()) &&
                                    p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                    select p));
                            }
                            if ((objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0 && objDamageTypeInt == 0 && objMagicDamageInt == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
                                    where p.Element("ObjClass").Value.Contains(objClassName) &&
                                    p.Element("ObjWieldValue").Value == objLevelInt.ToString() &&
                                    p.Element("ObjEmbue").Value == objEmbueTypeInt.ToString()
                                    select p));
                            }


                            if ((objDamageTypeInt > 0 || objMagicDamageInt > 0) && (objLevelInt < 1 || objLevelInt > 1) && objEmbueTypeInt > 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                    from p in xdoc.Element("Objs").Descendants("Obj")
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
                              from p in xdoc.Element("Objs").Descendants("Obj")
                              where p.Element("ObjClass").Value.Contains(objClassName) &&
                              p.Element("ObjMaterial").Value == objMat.ToString()
                              select p));
                        }

                        else if ((objClassName.Contains("Salvage")) && ((objSalvWork == "1-6") || (objSalvWork == "7-8") || (objSalvWork == "9")))
                        {
                            newDoc = new XDocument(new XElement("Objs",
                              from p in xdoc.Element("Objs").Descendants("Obj")
                              where p.Element("ObjClass").Value.Contains(objClassName) &&
                              p.Element("ObjMaterial").Value == objMat.ToString() &&
                              objSalvWork.Contains(p.Element("ObjWork").Value.Substring(0, 1))
                              select p));
                        }

                        else if ((objClassName.Contains("Salvage")) && (objSalvWork == "10"))
                        {
                            newDoc = new XDocument(new XElement("Objs",
                              from p in xdoc.Element("Objs").Descendants("Obj")
                              where p.Element("ObjClass").Value.Contains(objClassName) &&
                              p.Element("ObjMaterial").Value == objMat.ToString() &&
                              objSalvWork.ToString() == p.Element("ObjWork").Value
                              select p));
                        }

                        break;
                    default:

                        if (objClassName != null && mySelect != null && mySelect.Trim() != "")
                        {

                            newDoc = new XDocument(new XElement("Objs",
                                 from p in xdoc.Element("Objs").Descendants("Obj")
                                 where p.Element("ObjClass").Value.Contains(objClassName) &&
                                        (p.Element("ObjName").Value.ToLower().Contains(mySelect) ||
                                    p.Element("ObjSpellXml").Value.ToLower().Contains(mySelect))
                                 select p));
                        }

                        else if (objClassName != null && (mySelect == null || mySelect.Trim() == ""))
                        {

                            newDoc = new XDocument(new XElement("Objs",
                                 from p in xdoc.Element("Objs").Descendants("Obj")
                                 where p.Element("ObjClass").Value.Contains(objClassName)
                                 select p));
                        }

                        break;



                } //end of switch
                if ((mySelect != null || mySelect.Trim() != "") && objClassName != "None")
                {

                    xdoc = null;
                    newDoc.Save(inventorySelect);
                    newDoc = null;
                }
                else

                { GearFoundry.PluginCore.WriteToChat("You must choose a class or type something inbox"); }




            } //end of try //

            catch (Exception ex) { LogError(ex); }

            newDoc = XDocument.Load(inventorySelect);



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
                        WriteToChat("objName: " + objName); // + "; //objMaterialName " + objMaterialName);
                        foreach (IDNameLoadable item in MaterialInvList)
                        {
                            if (item.ID == objMaterial)
                            {
                                objMaterialName = item.name;
                                break;
                            }
                        }

                        objName = objMaterialName + " " + objName;
                    }
                    toonInvName = childElement.Element("ToonName").Value.ToString();
                    WriteToChat(toonInvName);
                    long objID = Convert.ToInt32(childElement.Element("ObjID").Value);
                    string objIDstr = objID.ToString();
                    InventoryHudListRow = lstHudInventory.AddRow();
                    ((HudPictureBox)InventoryHudListRow[0]).Image = objIcon + 0x6000000;
                    ((HudStaticText)InventoryHudListRow[1]).FontHeight = 10;
                    ((HudStaticText)InventoryHudListRow[1]).Text = objName;
                    ((HudStaticText)InventoryHudListRow[2]).FontHeight = 10;
                    ((HudStaticText)InventoryHudListRow[2]).Text = toonInvName;
                    ((HudStaticText)InventoryHudListRow[3]).Text = objIDstr;
                }


                //    IListRow newRow = lstInventory.AddRow();
                //   // newRow[0][1] = objIcon;
                //    newRow[0][1] = objIcon;
                //    newRow[1][0] = objName;
                //    newRow[2][0] = toonInvName;
                //    newRow[3][0] = objIDstr;
                //}
                catch (Exception ex) { LogError(ex); }

            }
            newDoc = null;
        }// end of btnlist

        //[MVControlEvent("lstInventory", "Selected")]
        //private void lstInventory_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e)
        private void lstHudInventory_Click(object sender, int row, int col)
        {
            try
            {
                WriteToChat("I am in function to write selected inventory to chat.");
                xdoc = XDocument.Load(inventorySelect);
                IEnumerable<XElement> myelements = xdoc.Element("Objs").Descendants("Obj");
                List<XElement> inventorySelectList = new List<XElement>();
                 var lst = from myelement in myelements
                          select myelement;
               inventorySelectList.AddRange(lst);
                XElement element = inventorySelectList[row];

               // int mrow = row;
               // IListRow row = lstInventory[e.Row];

                // the object name is included in the text file on this row
               // objID = Convert.ToInt32(row[3][0]);
              //  int mcol = 3;
 
         //       objID = Convert.ToInt32(lstHudInventory[row][3]);

                // objID = Convert.ToInt32(myelements[row]
                //elements = xdoc.Element("Objs").Descendants("Obj");
                //element = new XElement(new XElement("Objs",
                //    from p in xdoc.Element("Objs").Descendants("Obj")
                //    where (p.Element("ObjID").Value.ToLower().Contains(objID.ToString()))
                //    select p));

                xdoc = null;
              //  List<XElement> ElementsList = new List<XElement>();


                //childElements = element.Descendants();
                //foreach (XElement childElement in childElements)

                //{ ElementsList.Add(childElement); }

                //objName = ElementsList[1].Value;
                //toonInvName = ElementsList[3].Value;

                objName = element.Element("ObjName").Value;
                toonInvName = element.Element("ToonName").Value;
                message = objName + ", " + toonInvName;
                WriteToChat("Objclass: " + objClass.ToString());
                switch (objClass)
                {
                    //case 0:
                    //    objAl = element.Element("ObjAl").Value;
                    //    objWork = element.Element("ObjWork").Value;
                    //    objTinks = element.Element("ObjTinks").Value;
                    //    objLevel = element.Element("ObjLevel").Value;
                    //    objArmorSet = Convert.ToInt32(element.Element("ObjSet").Value);
                    //    objCovers = Convert.ToInt32(element.Element("ObjCovers").Value);
                    //    objSpells = element.Element("ObjSpellXml").Value.ToString();
                    //    //findArmorSetName(objArmorSet);
                    //    //findCoversName(objCovers);
                    //    objMissD = ((Math.Round(Convert.ToDouble(element.Element("ObjMissD").Value), 2) - 1) * 100).ToString();
                    //    if (Convert.ToDouble(objMissD) < 0) { objMissD = "0"; }
                    //    objManaC = (Math.Round(Convert.ToDouble(element.Element("ObjMana").Value), 2) * 100).ToString();
                    //    objMagicD = ((Math.Round(Convert.ToDouble(element.Element("ObjMagicD").Value), 2) - 1) * 100).ToString();
                    //    if (Convert.ToDouble(objMagicD) < 0) { objMagicD = "0"; }
                    //    objMelD = Math.Round(((Convert.ToDouble(element.Element("ObjMelD").Value) - 1) * 100), 2).ToString();
                    //    if (Convert.ToDouble(objMelD) < 0) { objMelD = "0"; }
                    //    objElemvsMons = Math.Round(((Convert.ToDouble(element.Element("ObjElemvsMons").Value) - 1) * 100), 2).ToString();
                    //    if (Convert.ToDouble(objElemvsMons) < 0) { objElemvsMons = "0"; }
                    //    objEmbueTypeInt = Convert.ToInt32(element.Element("ObjEmbue").Value);
                    //    objAttack = Math.Round((Convert.ToDouble(element.Element("ObjAttack").Value) - 1) * 100).ToString();
                    //    if (Convert.ToDouble(objAttack) < 0) { objAttack = "0"; }
                    //    objMaxDam = element.Element("ObjMaxDam").Value.ToString();
                    //    objVar = Math.Round(Convert.ToDouble(element.Element("ObjVar").Value), 2).ToString();
                    //    objBurden = element.Element("ObjBurden").Value;
                    //    objStack = element.Element("ObjStackCount").Value;
                    //    if (element.Element("ObjAcid").Value != "0")
                    //    {
                    //        string objAcid = ((Math.Round(Convert.ToDouble(element.Element("ObjAcid").Value), 4))).ToString();
                    //        string objLight = ((Math.Round(Convert.ToDouble(element.Element("ObjLight").Value), 4))).ToString();
                    //        string objFire = ((Math.Round(Convert.ToDouble(element.Element("ObjFire").Value), 4))).ToString();
                    //        string objCold = ((Math.Round(Convert.ToDouble(element.Element("ObjCold").Value), 4))).ToString();
                    //        string objBludg = ((Math.Round(Convert.ToDouble(element.Element("ObjBludg").Value), 4))).ToString();
                    //        string objSlash = ((Math.Round(Convert.ToDouble(element.Element("ObjSlash").Value), 4))).ToString();
                    //        string objPierce = ((Math.Round(Convert.ToDouble(element.Element("ObjPierce").Value), 4))).ToString();

                    //        objProts = objSlash + "/" + objPierce + "/" + objBludg + "/" + objFire + "/" + objCold + "/" + objAcid + "/" + objLight;
                    //    }
                    //    else
                    //    { objProts = ""; }

                    //    if (objProts != "")
                    //    {
                    //        message = message + ", Al: " + objAl + ", Prots: " + objProts + ", Work: " + objWork + ", Burden: " + objBurden +
                    //            " , Number: " + objStack + " , Tinks: " + objTinks +
                    //           ", Level: " + objLevel + ", " + objArmorSetName + " Set, " + objSpells +
                    //           ", covers: " + objCoversName + ", ManaC: " + objManaC +
                    //           ", MeleeD: " + objMelD + ", MagicD: " + objMagicD + ", MissileD: " + objMissD +
                    //           ", ElemVsMonster: " + objElemvsMons + ", Attack: " + objAttack +
                    //           ", MaxDam: " + objMaxDam + ", Variance: " + objVar + ", Embue: " + objEmbueTypeStr;
                    //    }
                    //    else
                    //    {

                    //        message = message + ", Al: " + objAl + ", Work: " + objWork + ", Burden: " + objBurden +
                    //             " , Number: " + objStack + " , Tinks: " + objTinks +
                    //            ", Level: " + objLevel + ", " + objArmorSetName + " Set, " + objSpells +
                    //            ", covers: " + objCoversName + ", ManaC: " + objManaC +
                    //            ", MeleeD: " + objMelD + ", MagicD: " + objMagicD + ", MissileD: " + objMissD +
                    //            ", ElemVsMonster: " + objElemvsMons + ", Attack: " + objAttack +
                    //            ", MaxDam: " + objMaxDam + ", Variance: " + objVar + ", Embue: " + objEmbueTypeStr;
                    //    }

                    //    break;

                    // *                
                    //case 1:
                    //case 2:
                    //case 11:
                    //    if (objClass == 1)
                    //    {
                    //        objAl = element.Element("ObjAl").Value;
                    //        objWork = element.Element("ObjWork").Value;
                    //        objTinks = element.Element("ObjTinks").Value;


                    //        //if (element.Element("ObjAcid").Value != "0")
                    //        //{
                    //        //    string objAcid = ((Math.Round(Convert.ToDouble(element.Element("ObjAcid").Value), 4))).ToString();
                    //        //    string objLight = ((Math.Round(Convert.ToDouble(element.Element("ObjLight").Value), 4))).ToString();
                    //        //    string objFire = ((Math.Round(Convert.ToDouble(element.Element("ObjFire").Value), 4))).ToString();
                    //        //    string objCold = ((Math.Round(Convert.ToDouble(element.Element("ObjCold").Value), 4))).ToString();
                    //        //    string objBludg = ((Math.Round(Convert.ToDouble(element.Element("ObjBludg").Value), 4))).ToString();
                    //        //    string objSlash = ((Math.Round(Convert.ToDouble(element.Element("ObjSlash").Value), 4))).ToString();
                    //        //    string objPierce = ((Math.Round(Convert.ToDouble(element.Element("ObjPierce").Value), 4))).ToString();

                    //        //    objProts = objSlash + "/" + objPierce + "/" + objBludg + "/" + objFire + "/" + objCold + "/" + objAcid + "/" + objLight;
                    //        //}
                    //        //else
                    //        { objProts = ""; }
                    //        message = ", Al: " + objAl + ", Work: " + objWork +
                    //               ", Tinks: " + objTinks;
                    //    //    if (objProts != "")
                    //    //    {

                    //    //        message = message + ", Al: " + objAl + " " + objProts + ", Work: " + objWork +
                    //    //           ", Tinks: " + objTinks;
                    //    //    }
                    //    //    else
                    //    //    {
                    //    //        message = message + ", Al: " + objAl + ", Work: " + objWork +
                    //    //           ", Tinks: " + objTinks;
                    //    //    }
                    //    //}
                    ////    if (objClass == 1 || objClass == 2)
                    ////    {
                    ////        objCovers = Convert.ToInt32(element.Element("ObjCovers").Value);
                    ////        // findCoversName(objCovers);
                    ////        message = message + ", Covers: " + objCoversName;
                    ////    }
                    ////    if (objClass == 1 || objClass == 2 || objName.Contains("Aetheria"))
                    ////    {
                    ////        objLevel = element.Element("ObjLevel").Value;
                    ////        objArmorSet = Convert.ToInt32(element.Element("ObjSet").Value);
                    ////        objSpells = element.Element("ObjSpellsXml").Value.ToString();
                    ////        // findArmorSetName(objArmorSet);
                    ////        objBurden = element.Element("ObjBurden").Value;

                    ////        message = message + ", Level: " + objLevel + ", Set: " + objArmorSetName
                    ////            + ", Spells: " + objSpells;
                    ////    }
                    //    break;
                    ////    if (objClass == 11 && !objName.Contains("Aetheria"))
                    //    {
                    //        objStack = element.Element("ObjStackCount").Value;
                    //        message = message + ", # in Stack: " + objStack;
                    //    }

                    //case 3:
                    //    objLevel = element.Element("ObjLevel").Value;
                    //    objSpells = element.Element("ObjSpellXml").Value.ToString();
                    //    message = message + ", Level: " + objLevel + ", " + objSpells;
                    //    break;

                    //case 4:
                    //    objWork = element.Element("ObjWork").Value;
                    //    objTinks = element.Element("ObjTinks").Value;
                    //    objLevel = element.Element("ObjLevel").Value;
                    //    objDamageTypeInt = Convert.ToInt32(element.Element("ObjDamage").Value);
                    //    objMissD = ((Math.Round(Convert.ToDouble(element.Element("ObjMissD").Value), 2) - 1) * 100).ToString();
                    //    if (Convert.ToDouble(objMissD) < 0) { objMissD = "0"; }
                    //    objManaC = (Math.Round(Convert.ToDouble(element.Element("ObjManaC").Value), 2) * 100).ToString();
                    //    objMagicD = ((Math.Round(Convert.ToDouble(element.Element("MagicD").Value), 2) - 1) * 100).ToString();
                    //    if (Convert.ToDouble(objMagicD) < 0) { objMagicD = "0"; }
                    //    objMelD = Math.Round(((Convert.ToDouble(element.Element("ObjMelD").Value) - 1) * 100), 2).ToString();
                    //    objElemvsMons = Math.Round(((Convert.ToDouble(element.Element("ObjElemvsMons").Value) - 1) * 100), 2).ToString();
                    //    objEmbueTypeInt = Convert.ToInt32(element.Element("ObjEmbue").Value);
                    //    //findEmbueTypeStr(objEmbueTypeInt);
                    //    //findDamageType();
                    //    objSpells = element.Element("ObjSpellXml").Value.ToString();

                    //    message = message + ", Damage: " + objDamageType + ", Wield Level: " + objLevel +
                    //        ", ElemVsMonster: " + objElemvsMons +
                    //        ", ManaC: " + objManaC + ", MeleeD: " + objMelD + ", MagicD: " + objMagicD +
                    //        ", MissileD: " + objMissD + ", Embue: " + objEmbueTypeStr +
                    //        ", Work: " + objWork + ", Tinks: " + objTinks + ", " + objSpells;
                    //    break;
                    //case 5:
                    //    objDamageTypeInt = Convert.ToInt32(element.Element("ObjDamage").Value);
                    //    //findDamageType();
                    //    objAttack = Math.Round((Convert.ToDouble(element.Element("ObjAttack").Value) - 1) * 100).ToString();
                    //    if (Convert.ToDouble(objAttack) < 0) { objAttack = "0"; }
                    //    objMaxDam = element.Element("ObjMaxDam").Value.ToString();
                    //    objMaxDamLong = Convert.ToInt32(objMaxDam);
                    //    objDVar = (Convert.ToDouble(element.Element("ObjVariance").Value));
                    //    objMinDam = Math.Round(objMaxDamLong - ((objDVar) * (objMaxDamLong)), 2).ToString();
                    //    objEmbueTypeInt = Convert.ToInt32(element.Element("ObjEmbue").Value);
                    //    //findEmbueTypeStr(objEmbueTypeInt);
                    //    objWork = element.Element("ObjWork").Value.ToString();
                    //    objTinks = element.Element("ObjTinks").Value.ToString();
                    //    objLevel = element.Element("ObjLevel").Value.ToString();
                    //    objMissD = ((Math.Round(Convert.ToDouble(element.Element("ObjMissD").Value), 2) - 1) * 100).ToString();
                    //    if (Convert.ToDouble(objMissD) < 0) { objMissD = "0"; }
                    //    objMagicD = ((Math.Round(Convert.ToDouble(element.Element("ObjMagicD").Value), 2) - 1) * 100).ToString();
                    //    if (Convert.ToDouble(objMagicD) < 0) { objMagicD = "0"; }
                    //    objMelD = Math.Round(((Convert.ToDouble(element.Element("ObjMelD").Value) - 1) * 100), 2).ToString();
                    //    objSpells = element.Element("ObjSpellXml").Value.ToString();
                    //    message = message + " , Damage: " + objDamageType + ", WieldLevel: " + objLevel +
                    //        ", Attack: " + objAttack + ", MeleeD: " + objMelD +
                    //        " Min-Max Damage: " + objMinDam + "-" + objMaxDam +
                    //        " , Embue: " + objEmbueTypeStr + ", Work: " + objWork + ", Tinks: " + objTinks +
                    //        ", MissD: " + objMissD + ", MagicD " + objMagicD + ", " + objSpells;
                    //    break;
                    case 6:
                        objDamageTypeInt = Convert.ToInt32(element.Element("ObjDamage").Value);
                        //findDamageType();
                        objWork = element.Element("ObjWork").Value.ToString();
                        objTinks = element.Element("ObjTink").Value.ToString();
                        objLevel = element.Element("ObjWieldValue").Value.ToString();
                        string objElDam = element.Element("ObjElemDmg").Value.ToString();
                        string objDamBon = ((Math.Round(Convert.ToDouble(element.Element("ObjDamageBonus").Value), 2) - 1) * 100).ToString();
                        objMissD = ((Math.Round(Convert.ToDouble(element.Element("ObjMissD").Value), 2) - 1) * 100).ToString();
                        if (Convert.ToDouble(objMissD) < 0) { objMissD = "0"; }
                        objMagicD = ((Math.Round(Convert.ToDouble(element.Element("ObjMagicD").Value), 2) - 1) * 100).ToString();
                        if (Convert.ToDouble(objMagicD) < 0) { objMagicD = "0"; }
                        objMelD = Math.Round(((Convert.ToDouble(element.Element("ObjMelD").Value) - 1) * 100), 2).ToString();

                        if (Convert.ToDouble(objDamBon) < 0) { objDamBon = "0"; }

                        objSpells = element.Element("ObjSpellXml").Value.ToString();
                        //message = message + ", Work: " + objWork + ", Tinks: " + objTinks + ", " + objSpells;

                        message = message + ", Damage Type: " + objDamageType + ", WieldLevel: " + objLevel +
                            ", Elem Dmg: " + objElDam +
                            ", Damage Bonus: " + objDamBon + "MelD: " + objMelD +
                            ", MissD: " + objMissD + ", MagicD: " + objMagicD +
                            ", Work: " + objWork + ", Tinks: " + objTinks + ", " + objSpells;
                        break;
                    case 7:
                        string objSalvWork = element.Element("ObjSalvWork").Value.ToString();
                        objBurden = element.Element("ObjBurden").Value;

                        //long objMat = Convert.ToInt32(element.Element("ObjSalvMat").Value);
                        //objMatName = 
                        //     findMaterialName(objMat);
                        message = message + ", Material: " + objMatName + ", Work: " + objSalvWork + ", Burden: " + objBurden;

                        break;

                    default:
                        objBurden = element.Element("ObjBurden").Value;
                        objStack = element.Element("ObjStackCount").Value;
                        objSpells = element.Element("ObjSpellXml").Value.ToString();

                        message = message + ", Burden: " + objBurden + ", Number: " + objStack + ", Spells: " + objSpells;
                        break;


                }



                GearFoundry.PluginCore.WriteToChat(message);
                message = null;
                elements = null;
                childElements = null;
                element = null;

            }


            catch (Exception ex) { LogError(ex); }
        }



        // It helps to clear the list before making new selection
        //[MVControlEvent("btnClrInventory", "Click")]
        //void btnClrInventory_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        private void btnClrInv_Hit(object sender, EventArgs e)
    {
           // lstInventory.Clear();
            lstHudInventory.ClearRows();
            cmbSelectClass.Selected = 0;
            cmbWieldAttrib.Selected = 0;
            cmbDamageType.Selected = 0;
            cmbLevel.Selected = 0;
            cmbArmorSet.Selected = 0;
            cmbMaterial.Selected = 0;
            cmbCoverage.Selected = 0;
            cmbArmorLevel.Selected = 0;
            cmbSalvWork.Selected = 0;
            cmbEmbue.Selected = 0;
            objEmbueTypeInt = 0;
            objDamageTypeInt = 0;
            objLevelInt = 1;
            objWieldAttrInt = 0;
            objSalvWork = "None";
            objClassName = null;
            objMat = 0;
            objCovers = 0;
            objArmorLevel = 1;
            objArmorSet = 0;
            newDoc = null;
            xdoc = null;
            childElements = null;
            elements = null;
            txbSelect.Text = "";
            mySelect = "";
        }

    } // end of partial class

} //end of namespace


