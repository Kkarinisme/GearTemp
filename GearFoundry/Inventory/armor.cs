using System;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using WindowsTimer = System.Windows.Forms.Timer;
using Decal.Adapter;
using Decal.Filters;
using Decal.Adapter.Wrappers;
using System.Xml.Linq;
using System.IO;
using VirindiViewService;
using MyClasses.MetaViewWrappers;
using VirindiViewService.Controls;
using VirindiHUDs;
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;



namespace GearFoundry
{
    public partial class PluginCore : PluginBase
    {
        private XDocument xdocGenArmor;
        private XDocument xdocArmor;
        private XDocument xdocArmorSettings;
        private XDocument xdocAllStats;

        private string armorFilename = null;
        private string genArmorFilename = null;
        private string holdingArmorFilename = null;
        private string armorSettingsFilename = null;


        private List<String> lstAllToonName;
        private HudView ArmorHudView = null;
        private HudFixedLayout ArmorHudLayout = null;
        private HudTabView ArmorHudTabView = null;
        private HudFixedLayout ArmorHudTabLayout = null;
        private HudList ArmorHudList = null;
        private HudList.HudListRowAccessor ArmorHudListRow = null;
        private const int ArmorRemoveCircle = 0x60011F8;

        private HudFixedLayout ArmorHudSettings;
        private HudStaticText lblToonArmorNameInfo;
        private HudStaticText lblToonArmorName;
        private HudStaticText lblToonLevel;
        private HudStaticText lblToonMaster;
        private HudCombo cboToonArmorName;

        private string toonArmorName = "";

        WindowsTimer mWaitingForArmorIDTimer = new WindowsTimer();


        void btnGetToonArmor_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            doGetArmor();
        }

        private void doGetArmor()
        {
            try
            {
                mWaitingForArmorID = new List<WorldObject>();

                armorFilename = toonDir + @"\" + toonName + "Armor.xml";
                genArmorFilename = currDir + @"\allToonsArmor.xml";
                holdingArmorFilename = world + @"\holdingArmor.xml";
                allStatsFilename = currDir + @"\AllToonStats.xml";
                armorSettingsFilename = currDir + @"\ArmorSettings.xml";
                

                xdocArmor = new XDocument(new XElement("Objs"));

                if (!File.Exists(armorSettingsFilename))
                {
                    XDocument tempDoc = new XDocument(new XElement("Settings"));
                    tempDoc.Save(armorSettingsFilename);
                    tempDoc = null;
                }

                if (!File.Exists(genArmorFilename))
                {

                    XDocument tempDoc = new XDocument(new XElement("Objs"));
                    tempDoc.Save(genArmorFilename);
                    tempDoc = null;


                }



                foreach (Decal.Adapter.Wrappers.WorldObject armorobj in Core.WorldFilter.GetInventory())
                {
                    try
                    {

                        if (armorobj.Values(LongValueKey.Slot) == -1) 
                        {
                            bool b = armorobj.ObjectClass.Equals("Armor");
                            bool b1 = armorobj.ObjectClass.Equals("Clothing");
                            bool b2 = armorobj.ObjectClass.Equals("Jewelry");
                            bool b3 = armorobj.Name.Contains("Aetheria");
                            if (b || b1 || b2 || b3)
                           Globals.Host.Actions.RequestId(armorobj.Id);
                            mWaitingForArmorID.Add(armorobj);
                        }
                    }
                    catch (Exception ex) { LogError(ex); }

                } // endof foreach world object
                // initialize event timer for processing inventory
                mWaitingForArmorIDTimer.Tick += new EventHandler(ArmorTimerEventProcessor);

                // Sets the timer interval to 5 seconds.
                mWaitingForArmorIDTimer.Interval = 10000;


                ProcessArmorDataInventory();
                mArmorIsFinished();
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void mArmorIsFinished()
        {
            try
            {
                int na = mWaitingForArmorID.Count;
                     if (mWaitingForArmorID.Count > 0)
                    {

                        mArmorCountWait();
                    }

                    else if (mWaitingForArmorID.Count == 0)
                    {

                        try
                        {
                            xdocArmor.Save(armorFilename);
                            XDocument xdocGenArmor = XDocument.Load(genArmorFilename);
                            xdocGenArmor.Descendants("Obj").Where(x => x.Element("ToonName").Value == toonName).Remove();

                            xdocGenArmor.Root.Add(XDocument.Load(armorFilename).Root.Elements());

                            xdocGenArmor.Save(genArmorFilename);
                            GearFoundry.PluginCore.WriteToChat("General Armor file has been saved. ");

                        }
                        catch (Exception ex) { LogError(ex); }

                    }

                    m = 30;
                    k = 0;
                    n = 0;
                    mWaitingForID = null;
                    xdoc = null;
                    fn = null;

                
            }

            catch (Exception ex) { LogError(ex); }
        }

        private void mArmorCountWait()
        {
            mWaitingForArmorIDTimer.Start();
        }
 

        public void ArmorTimerEventProcessor(Object Sender, EventArgs mWaitingForArmorIDTimer_Tick)
        {
            try
            {
                mWaitingForArmorIDTimer.Stop();
                for (int n = 0; n < mWaitingForArmorID.Count; n++)
                {
                    if (mWaitingForArmorID[n].HasIdData)
                    {
                        ProcessArmorDataInventory();
                        mArmorIsFinished();

                    }
                    else
                    {
                        mArmorCountWait(); }
                }

            }
            catch (Exception ex) { LogError(ex); }

        }

 
        public void removeToonfromArmorFile()
        {
            try
            {

                xdocArmor = XDocument.Load(genArmorFilename);

                IEnumerable<XElement> myelements = xdocArmor.Element("Objs").Descendants("Obj");

                xdocArmor.Descendants("Obj").Where(x => x.Element("ToonName").Value == toonName).Remove();
                xdocArmor = null;

            }
            catch (Exception ex) { LogError(ex); }

        }


         //This is routine that puts the data of an obj into the armor file xml
        private void ProcessArmorDataInventory()
        {
            for (int n = 0; n < mWaitingForArmorID.Count; n++)
            {
                try
                {

                    if (mWaitingForArmorID[n].HasIdData)
                    {
                        WorldObject currentarmorobj = mWaitingForArmorID[n];
                        mWaitingForArmorID.Remove(mWaitingForArmorID[n]);
                        //   if ((fn == "armorFilename") && (currentobj.Values(LongValueKey.Imbued) == 0)) { break; }
                        string armorClassName = currentarmorobj.ObjectClass.ToString();
                        string armorName = currentarmorobj.Name;
                        
                        Int32 armorID = currentarmorobj.Id;

                        Int32 armorIcon = currentarmorobj.Icon;

                        long armorDesc = currentarmorobj.Values(LongValueKey.DescriptionFormat);
                        long armorMat = currentarmorobj.Values(LongValueKey.Material);
                        long armorCatType = (int)currentarmorobj.Values(LongValueKey.Category);
                        long armorAtt = (int)currentarmorobj.Values(LongValueKey.Attuned);
                        long armorBnd = (int)currentarmorobj.Values(LongValueKey.Bonded);
                        long armorToonLevel = (int)currentarmorobj.Values(LongValueKey.MinLevelRestrict);
                        long armorLore = (int)currentarmorobj.Values(LongValueKey.LoreRequirement);
                        long armorSet = (int)currentarmorobj.Values(LongValueKey.ArmorSet);
 
                        long armorAl = (int)currentarmorobj.Values(LongValueKey.ArmorLevel);
                        long armorType = (int)currentarmorobj.Values(LongValueKey.Type);
                        long armorTinks = (int)currentarmorobj.Values(LongValueKey.NumberTimesTinkered);
                        long armorWork = (int)currentarmorobj.Values(LongValueKey.Workmanship);
                        long armorBurden = (int)currentarmorobj.Values(LongValueKey.Burden);

                        long armorCovers = currentarmorobj.Values(LongValueKey.Coverage);
                        long armorEqSlot = currentarmorobj.Values(LongValueKey.EquipableSlots);
                        long armorWieldValue = (int)currentarmorobj.Values(LongValueKey.WieldReqValue);
                        long armorSkillLevReq = (int)currentarmorobj.Values(LongValueKey.SkillLevelReq);
                        double armorSalvWork = currentarmorobj.Values(DoubleValueKey.SalvageWorkmanship);
                        double armorAcid = currentarmorobj.Values(DoubleValueKey.AcidProt);
                        double armorLight = currentarmorobj.Values(DoubleValueKey.LightningProt);
                        double armorFire = currentarmorobj.Values(DoubleValueKey.FireProt);
                        double armorCold = currentarmorobj.Values(DoubleValueKey.ColdProt);
                        double armorBludg = currentarmorobj.Values(DoubleValueKey.BludgeonProt);
                        double armorSlash = currentarmorobj.Values(DoubleValueKey.SlashProt);
                        double armorPierce = currentarmorobj.Values(DoubleValueKey.PierceProt);
                        string armorSpellXml = GoGetArmorSpells(currentarmorobj);
                        long armorRareID = currentarmorobj.Values(LongValueKey.RareId);
                       long armorModel = currentarmorobj.Values(LongValueKey.Model);
                        xdocArmor.Element("Objs").Add(new XElement("Obj",
                        new XElement("ArmorName", armorName),
                        new XElement("ArmorID", armorID),
                        new XElement("ToonName", toonName),
                        new XElement("ArmorIcon", armorIcon),
                        new XElement("ArmorClass", armorClassName),
                        new XElement("ArmorDesc", armorDesc),
                        new XElement("ArmorMaterial", armorMat),
                        new XElement("ArmorAl", armorAl),
                        new XElement("ArmorSet", armorSet),
                        new XElement("ArmorCovers", armorCovers),
                        new XElement("ArmorEqSlot", armorEqSlot),
                        new XElement("ArmorToonLevel", armorToonLevel),
                        new XElement("ArmorLoreReq", armorLore),
                        new XElement("ArmorSkillLevReq", armorSkillLevReq),
                        new XElement("ArmorWork", armorWork),
                        new XElement("ArmorTink", armorTinks),
                        new XElement("ArmorCatType", armorCatType),
                        new XElement("ArmorType", armorType),
                        new XElement("ArmorAtt", armorAtt),
                        new XElement("ArmorBnd", armorBnd),
                   //     new XElement("ArmorWieldAttr", armorWieldAttrInt),
                        new XElement("ArmorWieldValue", armorWieldValue),
                        new XElement("ArmorSalvWork", armorSalvWork),
                        new XElement("ArmorSpellXml", armorSpellXml),
                        new XElement("ArmorBurden", armorBurden),
                        new XElement("ArmorAcid", armorAcid),
                        new XElement("ArmorLight", armorLight),
                        new XElement("ArmorFire", armorFire),
                        new XElement("ArmorCold", armorCold),
                        new XElement("ArmorBludg", armorBludg),
                        new XElement("ArmorSlash", armorSlash),
                        new XElement("ArmorPierce", armorPierce),
                        new XElement("ArmorRareID", armorRareID)));


 

                        currentarmorobj = null;
                        armorClassName = null;
                        armorName = null;
                        armorDesc = 0;
                        armorID = 0;
                        armorIcon = 0;

                        armorAl = 0;
                        armorSet = 0;
                        armorMat = 0;
                        armorCovers = 0;
                        armorToonLevel = 0;
                        armorLore = 0;
                        armorSkillLevReq = 0;
                        armorTinks = 0;
                        armorWork = 0;
                        armorCatType = 0;
                        armorType = 0;

                        armorAtt = 0;
                        armorBnd = 0;
                       // objWieldAttrInt = 0;
                        armorWieldValue = 0;
                        armorSalvWork = 0;
                        armorEqSlot = 0;
                        armorSpellXml = null;
                        armorRareID = 0;
                        armorBurden = 0;
                        armorAcid = 0;
                        armorLight = 0;
                        armorFire = 0;
                        armorCold = 0;
                        armorBludg = 0;
                        armorSlash = 0;
                        armorPierce = 0;

                        armorModel = 0;

                    } // end of if


                } // endof try

                catch (Exception ex) { LogError(ex); }

            } // end of for



        } // end of process data

        private string GoGetArmorSpells(Decal.Adapter.Wrappers.WorldObject o)
        {
            FileService fs = (FileService)Core.FileService;
            int intspellcnt = o.SpellCount;
            string oXmlSpells = "";
            for (int i = 0; i < intspellcnt; i++)
            {
                int spellId = o.Spell(i);

                Spell spell = fs.SpellTable.GetById(spellId);

                string spellName = spell.Name;
                if (spellName.Contains("Legendary") || spellName.Contains("Epic") ||
                  spellName.Contains("Incantation") || spellName.Contains("Surge")
                    || spellName.Contains("Cloaked in Skill"))
                {
                    oXmlSpells = oXmlSpells + "," + spellName;
                }

                else
                    if (spellName.Contains("Major")) { oXmlSpells = oXmlSpells + ", " + spellName; }
            }
            if (oXmlSpells.Length > 0)
            {
                if (oXmlSpells.Substring(0, 1) == ",") { return oXmlSpells.Substring(1); }
                else { return oXmlSpells; }
            }
            else { return ""; }
        }  //endof gogetspells

          
        private bool ArmorMainTab;
        private bool ArmorSettingsTab;
        private int ArmorHudWidth = 0;
        private int ArmorHudHeight = 0;
        private int ArmorHudFirstWidth = 400;
        private int ArmorHudFirstHeight = 300;
        private int ArmorHudWidthNew;
        private int ArmorHudHeightNew;
        private List<XElement> myChoice;
        private XElement currentel;


        private void RenderArmorHud()
        {
            try
            {
                //GearSenseReadWriteSettings(true);

            }
            catch { }

            try
            {


                if (ArmorHudView != null)
                {
                    DisposeArmorHud();
                }
                xdocGenArmor = new XDocument();
                xdocGenArmor = XDocument.Load(genArmorFilename);

                if (ArmorHudWidth == 0) { ArmorHudWidth = ArmorHudFirstWidth; }
                if (ArmorHudHeight == 0) { ArmorHudHeight = ArmorHudFirstHeight; }

                ArmorHudView = new HudView("Armor", ArmorHudWidth, ArmorHudHeight, new ACImage(0x6AA5));
                ArmorHudView.UserAlphaChangeable = false;
                ArmorHudView.ShowInBar = false;
                ArmorHudView.UserResizeable = false;
                ArmorHudView.Visible = true;
                ArmorHudView.Ghosted = false;
                ArmorHudView.UserMinimizable = false;
                ArmorHudView.UserClickThroughable = false;
                ArmorHudView.LoadUserSettings();


                ArmorHudLayout = new HudFixedLayout();
                ArmorHudView.Controls.HeadControl = ArmorHudLayout;

                ArmorHudTabView = new HudTabView();
                ArmorHudLayout.AddControl(ArmorHudTabView, new Rectangle(0, 0, ArmorHudWidth, ArmorHudHeight));

                ArmorHudTabLayout = new HudFixedLayout();
                ArmorHudTabView.AddTab(ArmorHudTabLayout, "Armor");

                ArmorHudSettings = new HudFixedLayout();
                ArmorHudTabView.AddTab(ArmorHudSettings, "Settings");

                ArmorHudTabView.OpenTabChange += ArmorHudTabView_OpenTabChange;
                ArmorHudView.Resize += ArmorHudView_Resize; 

                RenderArmorTabLayout();

          //      SubscribeArmorEvents();

            }
            catch (Exception ex) { LogError(ex); }
            return;
        }

        private void ArmorHudView_Resize(object sender, System.EventArgs e)
        {
            try{
                bool bw = Math.Abs(ArmorHudView.Width - ArmorHudWidth) > 20;
                bool bh = Math.Abs(ArmorHudView.Height - ArmorHudHeight)> 20;
                if  ( bh|| bw)
                {
                   ArmorHudWidthNew = ArmorHudView.Width;
                    ArmorHudHeightNew = ArmorHudView.Height;
//                    MasterTimer.Interval = 1000;
//                    MasterTimer.Enabled = true;
//                    MasterTimer.Start();
                    MasterTimer.Tick += ArmorResizeTimerTick;
                }
            }
            catch (Exception ex) { LogError(ex); }
            return;


  
        }

        private void ArmorResizeTimerTick(object sender, EventArgs e)
        {
//            MasterTimer.Stop();
            ArmorHudWidth = ArmorHudWidthNew;
            ArmorHudHeight = ArmorHudHeightNew;
            MasterTimer.Tick -= ArmorResizeTimerTick;
            RenderArmorHud();
            }
            catch (Exception ex) { LogError(ex); }


        }

        private void SaveArmorSettings()
        {
         try


            {
                xdoc = new XDocument(new XElement("Settings"));
                xdoc.Element("Settings").Add(new XElement("Setting",
                    new XElement("ArmorHudWidth", ArmorHudWidth),
                    new XElement("ArmorHudHeight", ArmorHudHeight)));
                xdoc.Save(armorSettingsFilename);
            }
         catch (Exception ex) { LogError(ex); }

        }



        private void ArmorHudTabView_OpenTabChange(object sender, System.EventArgs e)
        {
            try
            {
                switch (ArmorHudTabView.CurrentTab)
                {
                    case 0:
                        DisposeArmorSettingsLayout();
                        RenderArmorTabLayout();
                        return;
                    case 1:
                        DisposeArmorTabLayout();

                        RenderArmorSettingsTabLayout();
                        break;
                }

            }
            catch (Exception ex) { LogError(ex); }
        }


        private void RenderArmorTabLayout()
        {
            try
            {
               lblToonArmorName = new HudStaticText();
                lblToonLevel = new HudStaticText();
                lblToonMaster = new HudStaticText();
                ArmorHudList = new HudList();
                ArmorHudTabLayout.AddControl(lblToonArmorName, new Rectangle(0,0,ArmorHudWidth/2,16));
                ArmorHudTabLayout.AddControl(lblToonLevel, new Rectangle(ArmorHudWidth/2 + 10,0,ArmorHudWidth/4,16));
                ArmorHudTabLayout.AddControl(lblToonMaster, new Rectangle(ArmorHudWidth*3/4 + 10,0,ArmorHudWidth/4,16));
                
                ArmorHudTabLayout.AddControl(ArmorHudList, new Rectangle(0,20, ArmorHudWidth, ArmorHudHeight));

                ArmorHudList.ControlHeight = Convert.ToInt32(.05*ArmorHudHeight);
                ArmorHudList.AddColumn(typeof(HudPictureBox), Convert.ToInt32(.05*ArmorHudWidth), null);
                ArmorHudList.AddColumn(typeof(HudStaticText), Convert.ToInt32(.35 * ArmorHudWidth), null);
                ArmorHudList.AddColumn(typeof(HudStaticText), Convert.ToInt32(.60*ArmorHudWidth), null);

                ArmorHudList.Click += (sender, row, col) => ArmorHudList_Click(sender, row, col);

            //    UpdateArmorHud();

                ArmorMainTab = true;
                try{
                FillArmorHudList();
                }

                catch (Exception ex) { LogError(ex); }

                


            }
             
            catch (Exception ex) { LogError(ex); }
       }

        private void FillArmorHudList()
        {
            try
            {
                if (toonArmorName == "") { toonArmorName = toonName; }
                myChoice = new List<XElement>();

                IEnumerable<XElement> marmor = xdocGenArmor.Element("Objs").Descendants("Obj");
 
                foreach (XElement el in marmor)
                {
                    if (el.Element("ToonName").Value == toonArmorName)
                    {
                        myChoice.Add(el);

                        int icon = Convert.ToInt32(el.Element("ArmorIcon").Value);
                        string armorpiece = el.Element("ArmorName").Value;
                        string spells = el.Element("ArmorSpellXml").Value;
                        ArmorHudListRow = ArmorHudList.AddRow();

                        ((HudPictureBox)ArmorHudListRow[0]).Image = icon + 0x6000000;
                        ((HudStaticText)ArmorHudListRow[1]).Text = armorpiece;
                        ((HudStaticText)ArmorHudListRow[2]).Text = spells;

                    }
                }
                //if(allStatsFilename != null)
                //{
                //    string toonLevel;
                //    string toonMastery;
                //    xdocAllStats = new XDocument();
                //    xdocAllStats = XDocument.Load(allStatsFilename);
                //    IEnumerable<XElement> mStats = xdocAllStats.Element("Toons").Descendants("Toon");

                //    foreach (XElement elName in mStats)
                //    {
                //        WriteToChat("toonArmorName = " + toonArmorName); 
                //        if (elName.Element("ToonName").Value == toonArmorName)
                //        {
                //            WriteToChat("I am in the foreach for mstats");
                //            toonLevel = elName.Element("Level").Value;
                //            lblToonLevel.Text = "Level: " + toonLevel;
                //            //  toonMastery = elName.Element("Mastery").Value;
                //            break;
                //        }
                //    }
                // }
                ArmorHudView.UserResizeable = true;
           }
            catch (Exception ex) { LogError(ex); }
            
        }



        private void DisposeArmorTabLayout()
        {
            try
            {
                if (!ArmorMainTab) { return; }

                ArmorHudList.Click -= (sender, row, col) => ArmorHudList_Click(sender, row, col);
                ArmorHudList.Dispose();

                ArmorMainTab = false;

            
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void RenderArmorSettingsTabLayout()
        {
            try
            {

                List<XElement> names = new List<XElement>();
                IEnumerable<XElement> prenames = xdocGenArmor.Element("Objs").Descendants("Obj");
                var lstsorted = from element in prenames
                                 orderby element.Element("ToonName").Value ascending

                                 select element;
                names.AddRange(lstsorted);

                ControlGroup myToonNames = new ControlGroup();
                cboToonArmorName = new HudCombo(myToonNames);

                cboToonArmorName.Change += (sender,index) => cboToonArmorName_Change(sender,index);



               lstAllToonName = new List<string>();
                try{
                    string name = "";
                    foreach (XElement el in names)
                    { 
                        name = el.Element("ToonName").Value;
                        int i = 0;
                        if (!lstAllToonName.Contains(name))
                        {
                            try
                            {
                                lstAllToonName.Add(name);
                                cboToonArmorName.AddItem(name, i);
                                i++;
                            }
                            catch (Exception ex) { LogError(ex); }

                        }
                    }
               }
                catch (Exception ex) { LogError(ex); }


                lblToonArmorNameInfo = new HudStaticText();
                lblToonArmorNameInfo.Text = "Name of toon whose armor is being studied:";
                ArmorHudSettings.AddControl(lblToonArmorNameInfo,new Rectangle(0,10,ArmorHudWidth,16));

               ArmorHudSettings.AddControl(cboToonArmorName, new Rectangle(5, 25, ArmorHudWidth-20, 16));


 
                ArmorSettingsTab = true;
             }
            catch (Exception ex) { LogError(ex); }
       }

        private void cboToonArmorName_Change(object sender, EventArgs e)
        {
            toonArmorName = lstAllToonName[cboToonArmorName.Current];
          //  WriteToChat(toonArmorName + "has been selected");
            lblToonArmorName.Text = toonArmorName;
 
            
        }
        private void DisposeArmorSettingsLayout()
        {
            try
            {
                if (!ArmorSettingsTab) { return; }

                ArmorSettingsTab = false;
            }
            catch { }
        }



        private void DisposeArmorHud()
        {

            try
            {
                SaveArmorSettings();
                UnsubscribeArmorEvents();
                try { DisposeArmorTabLayout(); }
                catch { }
                try { DisposeArmorSettingsLayout(); }
                catch { }

                ArmorHudSettings.Dispose();
                ArmorHudLayout.Dispose();
                ArmorHudTabLayout.Dispose();
                ArmorHudTabView.Dispose();
                ArmorHudView.Dispose();
            }
            catch (Exception ex) { LogError(ex); }
            return;
        }

        private void UnsubscribeArmorEvents()
        {
            ArmorHudTabView.OpenTabChange -= ArmorHudTabView_OpenTabChange;
            ArmorHudView.Resize -= ArmorHudView_Resize;
            MasterTimer.Tick -= ArmorResizeTimerTick;


        }

        private void ArmorHudList_Click(object sender, int row, int col)
        {
            try
            {
                int mrow = row;
                currentel = myChoice[row];
                string armorobjName = currentel.Element("ArmorName").Value;
                string armorobjAl = currentel.Element("ArmorAl").Value;
                string armorobjWork = currentel.Element("ArmorWork").Value;
                string armorobjTinks = currentel.Element("ArmorTink").Value;
                string armorobjLevel = currentel.Element("ArmorWieldValue").Value;
                int armorobjArmorSet = Convert.ToInt32(currentel.Element("ArmorSet").Value);
                int armorobjCovers = Convert.ToInt32(currentel.Element("ArmorCovers").Value);
                string objArmorSetName = ArmorSetsInvList[armorobjArmorSet].name;

                message = armorobjName + ", Al: " + armorobjAl + " , Work: " + armorobjWork + ", Tinks: " + armorobjTinks + ", Armor Wield Level: " + 
                    armorobjLevel + ", Set: " + objArmorSetName;
                WriteToChat(message);
                
                   
                UpdateLandscapeHud();

            }
            catch (Exception ex) { LogError(ex); }
            return;
        }

        private void UpdateArmorHud()
        {
            try
            {
                //if ((DateTime.Now - LastGearSenseUpdate).TotalMilliseconds < 1000) { return; }
                //else { LastGearSenseUpdate = DateTime.Now; }

                if (!ArmorMainTab) { return; }

                ArmorHudList.ClearRows();

                //foreach (IdentifiedObject item in LandscapeTrackingList)
                //{
                //    LandscapeHudListRow = LandscapeHudList.AddRow();

                //    ((HudPictureBox)LandscapeHudListRow[0]).Image = item.Icon + 0x6000000;
                //    ((HudStaticText)LandscapeHudListRow[1]).Text = item.IORString() + item.Name + item.DistanceString();
                //    if (item.IOR == IOResult.trophy) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Gold; }
                //    if (item.IOR == IOResult.lifestone) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.SkyBlue; }
                //    if (item.IOR == IOResult.monster) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Orange; }
                //    if (item.IOR == IOResult.npc) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Yellow; }
                //    if (item.IOR == IOResult.portal) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.MediumPurple; }
                //    if (item.IOR == IOResult.players) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.AntiqueWhite; }
                //    if (item.IOR == IOResult.fellowplayer) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.LightGreen; }
                //    if (item.IOR == IOResult.allegplayers) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Tan; }
                //    ((HudPictureBox)LandscapeHudListRow[2]).Image = LandscapeRemoveCircle;
                //}
            }
            catch (Exception ex) { LogError(ex); }
            return;
        }
    }
}



