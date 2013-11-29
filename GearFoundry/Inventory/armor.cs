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
using System.IO;
using VirindiViewService;
using MyClasses.MetaViewWrappers;
using VirindiViewService.Controls;
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;



namespace GearFoundry
{
    public partial class PluginCore : PluginBase
    {
        private XDocument xdocGenArmor;
        private XDocument xdocArmor;
        private XDocument xdocArmorSettings;
        private XDocument xdocArmorAvailable;

        private string armorFilename = null;
        private string genArmorFilename = null;
        private string holdingArmorFilename = null;
        private string armorSettingsFilename = null;
        private string armorSelectFilename = null;

        private HudView ArmorHudView = null;
        private HudTabView ArmorHudTabView = null;
 
        private HudFixedLayout ArmorHudTabLayout = null;
        private HudList ArmorHudList = null;
        private HudList.HudListRowAccessor ArmorHudListRow = null;
 
        private HudFixedLayout ArmorUpdateHudTabLayout = null;
        private HudList ArmorUpdateHudList = null;
        private HudList ArmorAvailableList = null;
        private HudList.HudListRowAccessor ArmorUpdateHudListRow = null;
        private HudList.HudListRowAccessor ArmorAvailableListRow = null;
        private HudButton btnListArmorAvailable;
        private HudButton btnClearListArmorAvailable;
  

       private HudFixedLayout ArmorSettingsTabLayout = null;
       private HudFixedLayout ArmorHudSettings = null;



        private bool ArmorMainTab;
        private bool ArmorUpdateTab;
        private bool ArmorSettingsTab;
        private int ArmorHudWidth = 0;
        private int ArmorHudHeight = 0;
        private int ArmorHudFirstWidth = 988;
        private int ArmorHudFirstHeight = 575;
        private int ArmorHudWidthNew = 0;
        private int ArmorHudHeightNew = 0;

         //Controls on Main Tab 
         private HudStaticText lblToonArmorNameInfo;
        private HudStaticText lblToonArmorName;
        private HudStaticText lblToonLevel;
        private HudStaticText lblToonMaster;


        //Controls on Update Tab
        private HudStaticText lblToonArmorUpdateNameInfo;
        private HudStaticText lblToonArmorUpdateName;
        private HudStaticText lblToonArmorUpdateLevel;
        private HudStaticText lblToonArmorUpdateMaster;
        private HudStaticText lblArmorUpdateChoice;
        private HudTextBox txtArmorUpdateChoice;
        private HudStaticText lblArmorUpdateClass;
        private HudTextBox txtArmorUpdateClass;
        private HudStaticText lblArmorUpdateCovers;
        private HudCombo cboArmorUpdateCovers; 
        private HudStaticText lblArmorUpdateSet;
        private HudCombo cboArmorUpdateSet;

        
         //Controls On settings page
        private HudCombo cboToonArmorName;
        private HudButton btnInventoryArmor;

        private HudStaticText lblToonSettingsNameInfo;
        private string toonArmorUpdateName = String.Empty;

        // lists
        private List<XElement> myArmor;
        private List<XElement> currentArmor;
 //       private List<XElement> availableArmor_Spells;
        private List<XElement> availableSelected;
        private List<XElement> availableArmor;
        private List<String> lstAllToonName;
        private List<String> lstArmorUpdateSelects;
    //    private List<String> lstMyAvailableSelects;

        private XElement currentel;

        //Variables Used in main tab
        private string toonArmorName = "";
        private string armorobjCoversName;
        private string updateSpells;
        private string armorpiece;  //contains name of piece of armor
        private string armorobjSetName;
        private string armorWieldAttr;
        private string armorWieldLevel;
        private int armorSkillLevel;
        private string armorMastery;
        private string armorWieldMess;
        private string armorSkillMess;
 
  
        //Variables Used in Update tab
        //Variables Used in UpdateList
        private string objArmorUpdateSetName;
        private string objArmorUpdateCovers;
        private string armorUpdateClass;
          private static long nArmorUpdateSet = 0;
        private string objArmorUpdateCoversName;
        private long nArmorUpdateCovers = 0;
         private string armorUpdateCoversName;
       private string armorUpdateSetName;
       private string armorUpdateWieldAttr;
        private string armorUpdateWieldLevel;
         private int armorUpdateSkillLevel;
       private string armorUpdateMastery;
        private string armorUpdateWieldMess;
        private string armorUpdateSkillMess;
        private long narmorUpdateWieldAttr;
        private string toonArmorAvailableName;
 

        //Variables Used in AvailableList

        private string armorAvailableSpells;
        private string armorAvailablePiece = null;
        private string armorAvailableClass = null;
        private string armorAvailableSetName = null;
        private string armorAvailableCovers = null;
        private string availableArmorCoversName;
        private string availableArmorSetName;
       private string availableArmorWieldAttr;
        private string availableArmorWieldLevel;
        private int availableArmorSkillLevel;
         private string availableArmorUpdateMastery;
        private string availableArmorWieldMess;
       private string availableArmorSkillMess;
        private string availableArmorMastery;
        private string availableArmorToon;
        private string availableChoices;
        private string availableClass;


         private void doGetArmor()
        {
            try
            {
            	if(programinv.Contains("inventory"))
            	   {
            	   	WriteToChat("Cannot run at this time because inventory program  is running.");
            	   }
            	else
            	{
                programinv = "armor";
                mWaitingForArmorID = new List<WorldObject>();

                armorFilename = toonDir + @"\" + toonName + "Armor.xml";
                armorSettingsFilename = currDir + @"\ArmorSettings.xml"; 
                genArmorFilename = currDir + @"\allToonsArmor.xml";
                holdingArmorFilename = currDir + @"\holdingArmor.xml";
                allStatsFilename = currDir + @"\AllToonStats.xml";
                

                xdocArmor = new XDocument(new XElement("Objs"));

                if (!File.Exists(armorSettingsFilename))
                {
                    XDocument tempArmorDoc = new XDocument(new XElement("Settings"));
                    tempArmorDoc.Save(armorSettingsFilename);
                    tempArmorDoc = null;
                }

                if (!File.Exists(genArmorFilename))
                {

                    XDocument tempgenArmorDoc = new XDocument(new XElement("Objs"));
                    tempgenArmorDoc.Save(genArmorFilename);
                    tempgenArmorDoc = null;


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
 
                ProcessArmorDataInventory();
                mArmorIsFinished();
            	}
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
                         WriteToChat("Armor waiting for id: " + na.ToString());
                         ArmorProcess = DateTime.Now;
                        mArmorCountWait();
                    }

                     else if (mWaitingForArmorID.Count == 0)
                     {

                         try
                         {
                             Core.RenderFrame -= new EventHandler<EventArgs>(RenderFrame_processArmor);

                             xdocArmor.Save(armorFilename);
                             XDocument xdocGenArmor = XDocument.Load(genArmorFilename);
                             xdocGenArmor.Descendants("Obj").Where(x => x.Element("ToonName").Value == toonName).Remove();

                             xdocGenArmor.Root.Add(XDocument.Load(armorFilename).Root.Elements());

                             xdocGenArmor.Save(genArmorFilename);
                             GearFoundry.PluginCore.WriteToChat("General Armor file has been saved. ");
                             xdocGenArmor = null;
                             xdocArmor = null;

                         }
                         catch (Exception ex) { LogError(ex); }


                         programinv = String.Empty;
                         m = 30;
//                         k = 0;
//                         n = 0;
                         mWaitingForID = null;
 //                        xdoc = null;
                     }
                
            }

            catch (Exception ex) { LogError(ex); }
        }

        DateTime ArmorProcess = DateTime.MinValue;
        

        private void mArmorCountWait()
        {
            
            Core.RenderFrame += new EventHandler<EventArgs>(RenderFrame_processArmor);
        }
 

        public void RenderFrame_processArmor(object sender, EventArgs e)
        {
            try
            {
                if ((DateTime.Now - ArmorProcess).TotalSeconds > 1)
                {
                    for (int n = 0; n < mWaitingForArmorID.Count; n++)
                    {
                        try{
                        if (mWaitingForArmorID[n].HasIdData)
                        {
                            Core.RenderFrame -= new EventHandler<EventArgs>(RenderFrame_processArmor);

                            ProcessArmorDataInventory();
                            mArmorIsFinished();

                        }
                        }
                        catch (Exception ex) { }
                    }
                }
                
            

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
                        long armorMastery = (int)currentarmorobj.Values(LongValueKey.ActivationReqSkillId);

                        long armorAl = (int)currentarmorobj.Values(LongValueKey.ArmorLevel);
                        long armorType = (int)currentarmorobj.Values(LongValueKey.Type);
                        long armorTinks = (int)currentarmorobj.Values(LongValueKey.NumberTimesTinkered);
                        long armorWork = (int)currentarmorobj.Values(LongValueKey.Workmanship);
                        long armorBurden = (int)currentarmorobj.Values(LongValueKey.Burden);
                        long armorWieldAttrInt = (int)currentarmorobj.Values(LongValueKey.WieldReqAttribute);

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
                        new XElement("ArmorMastery", armorMastery),
                        new XElement("ArmorWieldAttr", armorWieldAttrInt),
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
            string oXmlSpells = String.Empty;
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

          

        private void RenderArmorHud()
        {

            try
            {

                if (ArmorHudView != null)
                {
                    DisposeArmorHud();
                }
                if (armorSettingsFilename == "" || armorSettingsFilename == null) { armorSettingsFilename = GearDir + @"\ArmorSettings.xml"; }
                if (genArmorFilename == "" || genArmorFilename == null) { genArmorFilename = currDir + @"\allToonsArmor.xml"; }


                if (ArmorHudWidth == 0)
                {
                    getArmorHudSettings();
                }
                if (ArmorHudWidth == 0 || ArmorHudWidth<988) { ArmorHudWidth = ArmorHudFirstWidth;  }
                if (ArmorHudHeight == 0 || ArmorHudHeight<575) { ArmorHudHeight = ArmorHudFirstHeight; }

                ArmorHudView = new HudView("Armor", ArmorHudWidth, ArmorHudHeight, new ACImage(0x6AA5));
                ArmorHudView.UserAlphaChangeable = false;
                ArmorHudView.ShowInBar = false;
                ArmorHudView.UserResizeable = false;
                ArmorHudView.Visible = true;
                ArmorHudView.Ghosted = false;
                ArmorHudView.UserMinimizable = false;
                ArmorHudView.UserClickThroughable = false;
                ArmorHudView.LoadUserSettings();

                ArmorHudTabView = new HudTabView();
                ArmorHudView.Controls.HeadControl = ArmorHudTabView;

                ArmorHudTabLayout = new HudFixedLayout();
                ArmorHudTabView.AddTab(ArmorHudTabLayout, "Armor");

                ArmorUpdateHudTabLayout = new HudFixedLayout();
                ArmorHudTabView.AddTab(ArmorUpdateHudTabLayout, "Update Armor");

                ArmorHudSettings = new HudFixedLayout();
                ArmorHudTabView.AddTab(ArmorHudSettings, "Settings");

                ArmorHudTabView.OpenTabChange += ArmorHudTabView_OpenTabChange;
                ArmorHudView.Resize += ArmorHudView_Resize; 

                RenderArmorTabLayout();
 

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
                   MasterTimer.Tick += ArmorResizeTimerTick;
                }
            }
            catch (Exception ex) { LogError(ex); }
            return;


  
        }

        private void ArmorResizeTimerTick(object sender, EventArgs e)
        {
            ArmorHudWidth = ArmorHudWidthNew;
            ArmorHudHeight = ArmorHudHeightNew;
            MasterTimer.Tick -= ArmorResizeTimerTick;
            SaveArmorSettings();
            RenderArmorHud();

        }


        private void ArmorHudTabView_OpenTabChange(object sender, System.EventArgs e)
        {
            try
            {
 
                switch (ArmorHudTabView.CurrentTab)
                {
                    case 0:
                        DisposeArmorSettingsLayout();
                        DisposeArmorUpdateTabLayout();
                        RenderArmorTabLayout();
                        break;
                    case 1:
                        DisposeArmorTabLayout();
                        DisposeArmorSettingsLayout();
                        RenderArmorUpdateTabLayout();
                        SetUpListAvailableArmor(); 
                        break;
                    case 2:
                        DisposeArmorTabLayout();
                        DisposeArmorUpdateTabLayout();
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
               lblToonArmorName.FontHeight = nmenuFontHeight;
                lblToonLevel = new HudStaticText();
                lblToonLevel.FontHeight = nmenuFontHeight;
                lblToonMaster = new HudStaticText();
                lblToonMaster.FontHeight = nmenuFontHeight;
                ArmorHudList = new HudList();
 
                ArmorHudTabLayout.AddControl(lblToonArmorName, new Rectangle(0, 0, 100, 50));
                ArmorHudTabLayout.AddControl(lblToonLevel, new Rectangle(80,0,40,16));
                ArmorHudTabLayout.AddControl(lblToonMaster, new Rectangle(150,0,60,16));
                
                ArmorHudTabLayout.AddControl(ArmorHudList, new Rectangle(0,30, ArmorHudWidth, ArmorHudHeight-40));

                //ArmorHudList.ControlHeight = Convert.ToInt32(.05*ArmorHudHeight);
                ArmorHudList.AddColumn(typeof(HudPictureBox), 20, null);
                ArmorHudList.AddColumn(typeof(HudStaticText), Convert.ToInt32(.25 * ArmorHudWidth), null);
                ArmorHudList.AddColumn(typeof(HudStaticText), Convert.ToInt32(.18 * ArmorHudWidth), null);
                ArmorHudList.AddColumn(typeof(HudStaticText), Convert.ToInt32(.52 * ArmorHudWidth), null);

                
                ArmorHudList.Click += (sender, row, col) => ArmorHudList_Click(sender, row, col);


                ArmorMainTab = true;
                try{
                    if (toonArmorName == "" || toonArmorName == "None") { toonArmorName = toonName; }
                    lblToonArmorName.Text = toonArmorName;
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
                xdocGenArmor = XDocument.Load(genArmorFilename);
                myArmor = new List<XElement>();

                IEnumerable<XElement> marmor = xdocGenArmor.Element("Objs").Descendants("Obj");
 
                foreach (XElement el in marmor)
                {
                    if (el.Element("ToonName").Value == toonArmorName)
                    {
                        myArmor.Add(el);

                        int icon = Convert.ToInt32(el.Element("ArmorIcon").Value);
                        string armorpiece = el.Element("ArmorName").Value;
                        string spells = el.Element("ArmorSpellXml").Value;
                        string armorclass = el.Element("ArmorClass").Value;
                        objArmorSetName = String.Empty;
                        if (armorclass == "Armor") 
                        {
                            if (Convert.ToInt32(el.Element("ArmorSet").Value) > 0)
                            { objArmorSetName = SetsIndex[Convert.ToInt32(el.Element("ArmorSet").Value)].name; }
                        }
           

 
                        ArmorHudListRow = ArmorHudList.AddRow();

                        ((HudPictureBox)ArmorHudListRow[0]).Image = icon + 0x6000000;
                        ((HudStaticText)ArmorHudListRow[1]).Text = armorpiece;
                        ((HudStaticText)ArmorHudListRow[1]).FontHeight = nitemFontHeight;
                        ((HudStaticText)ArmorHudListRow[2]).Text = objArmorSetName;
                        ((HudStaticText)ArmorHudListRow[2]).FontHeight = nitemFontHeight;
                        ((HudStaticText)ArmorHudListRow[3]).Text = spells;
                        ((HudStaticText)ArmorHudListRow[3]).FontHeight = nitemFontHeight;

                    }
                }
                ArmorHudView.UserResizeable = true;
                xdocGenArmor = null;
           }
            catch (Exception ex) { LogError(ex); }
            
        }



         private void RenderArmorUpdateTabLayout()
        {
            try
            {
               lblToonArmorUpdateName = new HudStaticText();
                lblToonArmorUpdateName.FontHeight = nmenuFontHeight;
                lblToonArmorUpdateName.Text = "";
                lblToonArmorUpdateLevel = new HudStaticText();
                lblToonArmorUpdateLevel.FontHeight = nmenuFontHeight;
                lblToonArmorUpdateMaster = new HudStaticText();
                lblToonArmorUpdateMaster.FontHeight = nmenuFontHeight;
                ArmorUpdateHudList = new HudList();
                ArmorUpdateHudListRow = new HudList.HudListRowAccessor();
                ArmorAvailableList = new HudList();
                ArmorAvailableListRow = new HudList.HudListRowAccessor();
                armorSelectFilename = currDir + @"\armorSelected.xml";
            //    WriteToChat("Height: " + ArmorHudFirstHeight + ", Width: " + ArmorHudWidth);

                ArmorUpdateHudTabLayout.AddControl(lblToonArmorUpdateName, new Rectangle(0, 0, 100, 50));
                //ArmorUpdateHudTabLayout.AddControl(lblToonArmorUpdateLevel, new Rectangle(120, 0, 40, 16));
                //ArmorUpdateHudTabLayout.AddControl(lblToonArmorUpdateMaster, new Rectangle(150, 0, 60, 16));
                ArmorUpdateHudTabLayout.AddControl(ArmorUpdateHudList, new Rectangle(0, 30, ArmorHudWidth, (ArmorHudHeight) / 3));

                //ArmorHudList.ControlHeight = Convert.ToInt32(.05*ArmorHudHeight);
                ArmorUpdateHudList.AddColumn(typeof(HudStaticText), Convert.ToInt32(.25 * ArmorHudWidth), null);
                ArmorUpdateHudList.AddColumn(typeof(HudStaticText), Convert.ToInt32(.18 * ArmorHudWidth), null);
                ArmorUpdateHudList.AddColumn(typeof(HudStaticText), Convert.ToInt32(.52 * ArmorHudWidth), null);

 
                ArmorUpdateHudList.Click += (sender, row, col) => ArmorUpdateHudList_Click(sender, row, col);
                ArmorUpdateHudTabLayout.AddControl(ArmorAvailableList, new Rectangle(0, (ArmorHudHeight)/3 + 50, ArmorHudWidth, (3*(ArmorHudHeight/8))));

                //ArmorHudList.ControlHeight = Convert.ToInt32(.05*ArmorHudHeight);
                ArmorAvailableList.AddColumn(typeof(HudStaticText), Convert.ToInt32(.15 * ArmorHudWidth), null);
                ArmorAvailableList.AddColumn(typeof(HudStaticText), Convert.ToInt32(.15 * ArmorHudWidth), null);
                ArmorAvailableList.AddColumn(typeof(HudStaticText), Convert.ToInt32(.15 * ArmorHudWidth), null);
                ArmorAvailableList.AddColumn(typeof(HudStaticText), Convert.ToInt32(.15 * ArmorHudWidth), null);
                ArmorAvailableList.AddColumn(typeof(HudStaticText), Convert.ToInt32(.52 * ArmorHudWidth), null);
 
                ArmorAvailableList.Click += (sender, row, col) => ArmorAvailableList_Click(sender, row, col);

                lblArmorUpdateCovers = new HudStaticText();
                lblArmorUpdateCovers.FontHeight = nmenuFontHeight;
                lblArmorUpdateCovers.Text = "Cov:";
                ControlGroup CoverageChoices = new ControlGroup();
                cboArmorUpdateCovers = new HudCombo(CoverageChoices);
                cboArmorUpdateCovers.Change += (sender, index) => cboArmorUpdateCovers_Change(sender, index);

                int i = 0;
                foreach (IDNameLoadable info in CoverageInvList)
                {
                    cboArmorUpdateCovers.AddItem(info.name, i);
                    i++;
                }

                lblArmorUpdateSet = new HudStaticText();
                lblArmorUpdateSet.FontHeight = nmenuFontHeight;
                lblArmorUpdateSet.Text = "Set:";
                ControlGroup SetChoices = new ControlGroup();
                cboArmorUpdateSet = new HudCombo(SetChoices);
                cboArmorUpdateSet.Change += (sender, index) => cboArmorUpdateSet_Change(sender, index);

                i = 0;
                foreach (IDNameLoadable info in ArmorSetsInvList)
                {
                    cboArmorUpdateSet.AddItem(info.name, i);
                    i++;
                }
                lblArmorUpdateChoice = new HudStaticText();
                lblArmorUpdateChoice.FontHeight = nmenuFontHeight;
                lblArmorUpdateChoice.Text = "Search spells:";
                lblArmorUpdateClass = new HudStaticText();
                lblArmorUpdateClass.Text = "Armor,Jewelry, or Clothing";
                txtArmorUpdateChoice = new HudTextBox();
                txtArmorUpdateClass = new HudTextBox();

                 btnClearListArmorAvailable = new HudButton();
                btnClearListArmorAvailable.Text = "Clear List";
                btnClearListArmorAvailable.Hit += (sender, index) => btnClearListArmorAvailable_Hit(sender, index);

                btnListArmorAvailable = new HudButton();
                btnListArmorAvailable.Text = "List Inventory";
                btnListArmorAvailable.Hit += (sender, index) => btnListArmorAvailable_Hit(sender, index);


                ArmorUpdateHudTabLayout.AddControl(lblArmorUpdateClass, new Rectangle(0, (5 * (ArmorHudHeight / 6)), ArmorHudWidth/6, 16));
                ArmorUpdateHudTabLayout.AddControl(txtArmorUpdateClass, new Rectangle(ArmorHudWidth / 6, (5 * (ArmorHudHeight / 6)), 60, 16));
                ArmorUpdateHudTabLayout.AddControl(lblArmorUpdateChoice, new Rectangle(ArmorHudWidth/6 + 80, (5 * (ArmorHudHeight / 6)), 80, 16));
                ArmorUpdateHudTabLayout.AddControl(txtArmorUpdateChoice, new Rectangle(ArmorHudWidth/3, (5*(ArmorHudHeight/6)), (ArmorHudWidth / 2), 16));
               ArmorUpdateHudTabLayout.AddControl(lblArmorUpdateCovers, new Rectangle(0, (5 * (ArmorHudHeight / 6)+30),(30), 16));
                ArmorUpdateHudTabLayout.AddControl(cboArmorUpdateCovers, new Rectangle(40, (5 * (ArmorHudHeight / 6)+30), 200, 16));
                ArmorUpdateHudTabLayout.AddControl(lblArmorUpdateSet, new Rectangle(ArmorHudWidth / 2, (5 * (ArmorHudHeight / 6)+30), (ArmorHudWidth / 2) + 30, 16));
                ArmorUpdateHudTabLayout.AddControl(cboArmorUpdateSet, new Rectangle((ArmorHudWidth / 2) + 30, (5 * (ArmorHudHeight / 6)+30), (ArmorHudWidth / 2) + 100, 16));
                ArmorUpdateHudTabLayout.AddControl(btnListArmorAvailable, new Rectangle(ArmorHudWidth/3, (5 * (ArmorHudHeight / 6)+60), 100, 16));
                ArmorUpdateHudTabLayout.AddControl(btnClearListArmorAvailable, new Rectangle(ArmorHudWidth / 2, (5 * (ArmorHudHeight / 6)+60), 100, 16));


                ArmorUpdateTab = true;
                   if (toonArmorName == "" || toonArmorName == "None") { toonArmorName = toonName; }
                    lblToonArmorUpdateName.Text = toonArmorName;
                    toonArmorUpdateName = toonArmorName;
                FillArmorUpdateHudList();
 
 
            }

            catch (Exception ex) { LogError(ex); }
        }

        void cboArmorUpdateCovers_Change(object sender, EventArgs e)
        {
            try
            {
                nArmorUpdateCovers = CoverageInvList[cboArmorUpdateCovers.Current].ID;
                objArmorUpdateCoversName = CoverageInvList[cboArmorUpdateCovers.Current].name;
            }
            catch (Exception ex) { LogError(ex); }
        }


        void cboArmorUpdateSet_Change(object sender, EventArgs e)
        {
            try
            {
                nArmorUpdateSet = ArmorSetsInvList[cboArmorUpdateSet.Current].ID;
                objArmorUpdateSetName = ArmorSetsInvList[cboArmorUpdateSet.Current].name;

            }
            catch (Exception ex) { LogError(ex); }
        }


        private void SetUpListAvailableArmor()
        {
            availableArmor = new List<XElement>();
            xdocArmorAvailable = XDocument.Load(genInventoryFilename);
            IEnumerable<XElement> available = xdocArmorAvailable.Element("Objs").Descendants("Obj");
            foreach (XElement el in available)
            {
                if (el.Element("ObjClass").Value == "Armor" || el.Element("ObjClass").Value == "Clothing" || el.Element("ObjClass").Value == "Jewelry")
                {
                    availableArmor.Add(el);
                }
            }
            xdocArmorAvailable = null;

        }

        private void FillArmorUpdateHudList()
        {
            try
            {
            //     WriteToChat("toonname " + toonArmorUpdateName);
                currentArmor = new List<XElement>();
                string updateSpells = null;
                string armorupdatepiece = null;
                string armorUpdateClass = null;
                string objArmorUpdateSetName = null;
                currentArmor = new List<XElement>();
                string objArmorUpdateCovers = null;
                xdocGenArmor = XDocument.Load(genArmorFilename);
                IEnumerable<XElement> elements = xdocGenArmor.Element("Objs").Descendants("Obj");


                foreach (XElement el in elements)
                {
                    if (el.Element("ToonName").Value == toonArmorUpdateName)
                    {
                        currentArmor.Add(el);
                        armorUpdateClass = el.Element("ArmorClass").Value;
                        int icon = Convert.ToInt32(el.Element("ArmorIcon").Value);
                        armorupdatepiece = el.Element("ArmorName").Value;
                        updateSpells = el.Element("ArmorSpellXml").Value;    
                        objArmorUpdateSetName = String.Empty;
                        objArmorUpdateCovers = String.Empty;
                            try
                            {
                                if (armorUpdateClass == "Armor")
                                {
                                    objCovers = Convert.ToInt32(el.Element("ArmorCovers").Value);
                                    if (Convert.ToInt32(el.Element("ArmorSet").Value) > 0)
                                    { objArmorUpdateSetName = SetsIndex[Convert.ToInt32(el.Element("ArmorSet").Value)].name;  }

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
                                    } //end of if objcovers

                                } // eof if armorupdateClass
 
                            } // eof try

                            catch (Exception ex) { LogError(ex); }





                            ArmorUpdateHudListRow = ArmorUpdateHudList.AddRow();
                            ((HudStaticText)ArmorUpdateHudListRow[0]).Text = armorupdatepiece;
                            ((HudStaticText)ArmorUpdateHudListRow[0]).FontHeight = nitemFontHeight;
                            ((HudStaticText)ArmorUpdateHudListRow[1]).Text = objArmorUpdateSetName;
                            ((HudStaticText)ArmorUpdateHudListRow[1]).FontHeight = nitemFontHeight;
                            ((HudStaticText)ArmorUpdateHudListRow[2]).Text = updateSpells;
                            ((HudStaticText)ArmorUpdateHudListRow[2]).FontHeight = nitemFontHeight;
                        }
                    }

                    //   ArmorUpdateHudView.UserResizeable = true;
                xdocGenArmor = null;
            }
            catch (Exception ex) { LogError(ex); }
 
        }

  
        private void FillArmorAvailableList()
        {
            newDoc = XDocument.Load(armorSelectFilename);
            IEnumerable<XElement> selectedArmor = newDoc.Element("Objs").Descendants("Obj");
            availableSelected = new List<XElement>();
                    foreach (XElement el in selectedArmor)
                    {
                        try
                        {
                            availableSelected.Add(el);
                           // objIcon = Convert.ToInt32(childElement.Element("ObjIcon").Value);
                            armorAvailableClass = txtArmorUpdateClass.Text.ToLower();

                            armorAvailablePiece = el.Element("ObjName").Value;
                            availableArmorToon = el.Element("ToonName").Value.ToString();                          
       
                           armorAvailableSpells = el.Element("ObjSpellXml").Value;
                           armorAvailableSetName = "";
                            armorAvailableCovers = "";
                            if (armorAvailableClass == "armor" || armorAvailableClass == "clothing")
                            {
                                //   WriteToChat("I am in function fill armoravailable list coverage");
                                int narmorAvailableCovers = Convert.ToInt32(el.Element("ObjCovers").Value);
                                //  WriteToChat("Int armor covers: " + narmorAvailableCovers.ToString());
                                if (Convert.ToInt32(el.Element("ObjSet").Value) > 0)
                                { armorAvailableSetName = SetsIndex[Convert.ToInt32(el.Element("ObjSet").Value)].name; } //WriteToChat("ArmorAvailablesetname " + armorAvailableSetName); }

                                if (narmorAvailableCovers > 0)
                                {
                                    //     WriteToChat("I am in function to find name of coverage for "  + narmorAvailableCovers.ToString());
                                    foreach (IDNameLoadable piece in CoverageInvList)
                                    {
                                        if (piece.ID == narmorAvailableCovers)
                                        {
                                            armorAvailableCovers = piece.name;
                                            //   WriteToChat(armorAvailableCovers);
                                            break;
                                        }
                                    }
                                }
                            } 
 
                        ArmorAvailableListRow = ArmorAvailableList.AddRow();
                        ((HudStaticText)ArmorAvailableListRow[0]).Text = armorAvailablePiece;
                        ((HudStaticText)ArmorAvailableListRow[0]).FontHeight = nitemFontHeight;
                        ((HudStaticText)ArmorAvailableListRow[1]).Text = availableArmorToon;
                        ((HudStaticText)ArmorAvailableListRow[1]).FontHeight = nitemFontHeight;
                        ((HudStaticText)ArmorAvailableListRow[2]).Text = armorAvailableSetName;
                        ((HudStaticText)ArmorAvailableListRow[2]).FontHeight = nitemFontHeight;
                        ((HudStaticText)ArmorAvailableListRow[3]).Text = armorAvailableCovers;
                        ((HudStaticText)ArmorAvailableListRow[3]).FontHeight = nitemFontHeight;
                        ((HudStaticText)ArmorAvailableListRow[4]).Text = armorAvailableSpells;
                        ((HudStaticText)ArmorAvailableListRow[4]).FontHeight = nitemFontHeight;

                      }catch (Exception ex) { LogError(ex); }
                   }
                    newDoc = null;
               
            
        }           
 
        

                
  




        private void RenderArmorSettingsTabLayout()
        {
            try
            {
                xdocGenArmor = XDocument.Load(genArmorFilename);

                List<XElement> names = new List<XElement>();
                 IEnumerable<XElement> prenames = xdocGenArmor.Element("Objs").Descendants("Obj");
                var lstsorted = from element in prenames
                                 orderby element.Element("ToonName").Value ascending

                                 select element;
                names.AddRange(lstsorted);

                ControlGroup myToonNames = new ControlGroup();
                cboToonArmorName = new HudCombo(myToonNames);

                cboToonArmorName.Change += (sender,index) => cboToonArmorName_Change(sender,index);
                btnInventoryArmor = new HudButton();
                btnInventoryArmor.Text = "Inventory Armor";
                btnInventoryArmor.Hit += (sender,index) => btnInventoryArmor_Hit(sender,index);



               lstAllToonName = new List<string>();
                try{
                    string name = "";
                    lstAllToonName.Add("None");
                    cboToonArmorName.AddItem("None", 0);
                     foreach (XElement el in names)
                    { 
                        name = el.Element("ToonName").Value;
                        int i = 1;
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
  

                lblToonSettingsNameInfo = new HudStaticText();
                lblToonSettingsNameInfo.FontHeight = nmenuFontHeight;
                lblToonSettingsNameInfo.Text = "Name of toon whose armor to be studied:";

                ArmorHudSettings.AddControl(btnInventoryArmor, new Rectangle(5, 30, 100, 20));

                ArmorHudSettings.AddControl(lblToonSettingsNameInfo,new Rectangle(5,100,300,16));

               ArmorHudSettings.AddControl(cboToonArmorName, new Rectangle(320, 100, 150, 16));


 
                ArmorSettingsTab = true;
                xdocGenArmor = null;
             }
            catch (Exception ex) { LogError(ex); }
       }

        private void cboToonArmorName_Change(object sender, EventArgs e)
        {
            toonArmorName = lstAllToonName[cboToonArmorName.Current];
        }

        private void btnInventoryArmor_Hit(object sender, EventArgs e)
        {
            doGetArmor();

        }

        private void DisposeArmorSettingsLayout()
        {
            try
            {
                if (!ArmorSettingsTab) { return; }
                btnInventoryArmor.Hit -= (sender, index) => btnInventoryArmor_Hit(sender, index);

                cboToonArmorName.Change -= (sender,index) => cboToonArmorName_Change(sender,index);
                btnInventoryArmor = null;
                lblToonArmorNameInfo.Text = "";
                lblToonArmorNameInfo = null;
                cboToonArmorName = null;
                lstAllToonName = null;


                ArmorSettingsTab = false;
            }
            catch { }
        }



        private void DisposeArmorHud()
        {

            try
            {
                SaveArmorSettings();
                ClearArmorHudVariables();
                UnsubscribeArmorEvents();
                try { DisposeArmorTabLayout(); }
                catch { }
                try { DisposeArmorSettingsLayout(); }
                catch { }
                try { DisposeArmorUpdateTabLayout(); }
                catch { }
                ArmorHudSettings.Dispose();
                ArmorHudTabLayout.Dispose();
                ArmorUpdateHudTabLayout.Dispose();
                ArmorHudTabView.Dispose();
                ArmorHudView.Dispose();
                toonArmorName = "";
                lblToonArmorUpdateLevel.Dispose();
                lblToonArmorUpdateMaster.Dispose();
                lblArmorUpdateChoice.Dispose();
                txtArmorUpdateChoice.Dispose();
                lblArmorUpdateClass.Dispose();
                txtArmorUpdateClass.Dispose();
                lblArmorUpdateCovers.Dispose();
                cboArmorUpdateCovers.Dispose();
                lblArmorUpdateSet.Dispose();
                cboArmorUpdateSet.Dispose();
                cboToonArmorName.Dispose();
                
                btnInventoryArmor.Dispose();
                btnClearListArmorAvailable.Dispose();
                btnListArmorAvailable.Dispose();
                


            }
            catch (Exception ex) { LogError(ex); }
            return;
        }

        private void ClearArmorHudVariables()
        {
            genArmorFilename = null;
            armorFilename = null;
            armorSelectFilename = null;
            armorSettingsFilename = null;
            holdingArmorFilename = null;
            ArmorHudFirstHeight = 0;
            ArmorHudFirstWidth = 0;
            ArmorHudHeight = 0;
            ArmorHudHeightNew = 0;
            ArmorHudWidth = 0;
            ArmorHudWidthNew = 0;

            

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
                currentel = myArmor[row];
                string armorobjName = currentel.Element("ArmorName").Value;
                string armorobjAl = currentel.Element("ArmorAl").Value;
                string armorobjWork = currentel.Element("ArmorWork").Value;
                string armorobjTinks = currentel.Element("ArmorTink").Value;
                string armorobjLevel = currentel.Element("ArmorWieldValue").Value;
                long armorToonLevel = Convert.ToInt32(currentel.Element("ArmorToonLevel").Value);
                int armorobjArmorSet = Convert.ToInt32(currentel.Element("ArmorSet").Value);
                int armorobjCovers = Convert.ToInt32(currentel.Element("ArmorCovers").Value);
                 if (armorobjCovers > 0)
                {
                    foreach (IDNameLoadable piece in CoverageInvList)
                    {
                        if (piece.ID == armorobjCovers)
                        {
                            armorobjCoversName = piece.name;
                            break;
                        }
                    }
                }
 

                string armorobjSetName = SetsIndex[armorobjArmorSet].name;

                int armorLore = Convert.ToInt32(currentel.Element("ArmorLoreReq").Value);
                armorWieldAttr = "";
                long narmorWieldAttr = Convert.ToInt32(currentel.Element("ArmorWieldAttr").Value);
                if (narmorWieldAttr == 7) { armorWieldAttr = "Missile Defense"; }
                if (narmorWieldAttr == 15) { armorWieldAttr = "Magic Defense"; }
                if (narmorWieldAttr == 6) { armorWieldAttr = "Melee Defense"; }

                int armorWieldValue = Convert.ToInt32(currentel.Element("ArmorWieldValue").Value);
                //     if (nobjWieldAttr == 7 || nobjWieldAttr == 6 || nobjWieldAttr == 15)
                int tempNum = 180;
                if (narmorWieldAttr > 0)
                { armorWieldLevel = armorToonLevel.ToString(); }
                else if ((objName.Contains("Radiant")) || (objName.Contains("Eldrytch")) || (objName.Contains("Hand")))
                { armorWieldLevel = tempNum.ToString(); }
                else if (narmorWieldAttr <= 1)
                { armorWieldLevel = armorWieldValue.ToString(); }

                armorSkillLevel = Convert.ToInt32(currentel.Element("ArmorSkillLevReq").Value);
                int armorSkillInt = Convert.ToInt32(currentel.Element("ArmorMastery").Value);
                if (armorSkillInt == 15) { armorMastery = "Magic Defense"; }
                if (armorSkillInt == 6) { armorMastery = "Melee Defense"; }
                if (armorSkillInt == 7) { armorMastery = "Missile Defense"; }
                armorWieldLevel = armorWieldValue.ToString();
                armorWieldMess = "Level to wield: " + armorWieldLevel;

                try
                {

                    if (armorobjName.Contains("Radiant"))
                    { armorWieldMess = "Required to wield: Level 180, Radiant Blood Society Level: " + armorWieldValue.ToString(); }
                    if (armorobjName.Contains("Eldrytch"))
                    { armorWieldMess = "Required to wield: Level 180, Eldrytch Webb Society Level: " + armorWieldValue.ToString(); }
                    if (armorobjName.Contains("Celestial"))
                    { armorWieldMess = "Required to wield: Level 180, Celestial Hand Society Level: " + armorWieldValue.ToString(); }
                //    WriteToChat("Armorwieldattr " + armorWieldAttr);
                    if ((armorWieldAttr.Contains("Magic")) || (armorWieldAttr.Contains("Missile")) || (armorWieldAttr.Contains("Melee")))
                    { armorWieldMess = "Required to wield: Level " + objLevel.ToString() + ", " + objWieldAttr + ": " + objWieldValue.ToString(); }

                }

                catch (Exception ex) { LogError(ex); }

                armorSkillMess = "; Required for activation: ObjLore - " + objLore.ToString() + ", "
                               + objMastery + " - " + objSkillLevel.ToString();

 
   


                message = armorobjName + ", Al: " + armorobjAl + " , Work: " + armorobjWork + ", Tinks: " + armorobjTinks + ", Armor Wield Level: " + 
                    armorobjLevel + ", Covers: " + armorobjCoversName + ", Set: " + objArmorSetName + armorWieldMess + armorSkillMess;
                WriteToChat(message);
                
                   
            //    UpdateLandscapeHud();

            }
            catch (Exception ex) { LogError(ex); }
            return;
        }

        private void ArmorUpdateHudList_Click(object sender, int row, int col)
        {
            try
            {
 
                currentel = currentArmor[row];
                string armorpiece = currentel.Element("ArmorName").Value;
                string armorUpdateAl = currentel.Element("ArmorAl").Value;
                string armorUpdateWork = currentel.Element("ArmorWork").Value;
                string armorUpdateTinks = currentel.Element("ArmorTink").Value;
                string armorUpdateLevel = currentel.Element("ArmorWieldValue").Value;
                int armorUpdateWieldValue = Convert.ToInt32(armorUpdateLevel);
                long armorUpdateToonLevel = Convert.ToInt32(currentel.Element("ArmorToonLevel").Value);
                int armorUpdateSet = Convert.ToInt32(currentel.Element("ArmorSet").Value);
                int armorUpdateCovers = Convert.ToInt32(currentel.Element("ArmorCovers").Value);
                string armorUpdateSetName = SetsIndex[armorUpdateSet].name;
                armorUpdateWieldAttr = "";
                narmorUpdateWieldAttr = 0;

                if (armorUpdateCovers > 0)
                {
                    foreach (IDNameLoadable piece in CoverageInvList)
                    {
                        if (piece.ID == armorUpdateCovers)
                        {
                            armorUpdateCoversName = piece.name;
                            break;
                        }
                    }
                }

                int armorUpdateLore = Convert.ToInt32(currentel.Element("ArmorLoreReq").Value);

                narmorUpdateWieldAttr = Convert.ToInt32(currentel.Element("ArmorWieldAttr").Value);
                if (narmorUpdateWieldAttr == 7) { armorUpdateWieldAttr = "Missile Defense"; }
                if (narmorUpdateWieldAttr == 15) { armorUpdateWieldAttr = "Magic Defense"; }
                if (narmorUpdateWieldAttr == 6) { armorUpdateWieldAttr = "Melee Defense"; }
                int tempNum = 180;
                if (narmorUpdateWieldAttr > 0)
                { armorUpdateWieldLevel = armorUpdateToonLevel.ToString(); }
                else if ((armorpiece.Contains("Radiant")) || (armorpiece.Contains("Eldrytch")) || (armorpiece.Contains("Hand")))
                { armorUpdateWieldLevel = tempNum.ToString(); }
                else if (narmorUpdateWieldAttr <= 1)
                { armorUpdateWieldLevel = armorUpdateWieldValue.ToString(); }

                armorUpdateSkillLevel = Convert.ToInt32(currentel.Element("ArmorSkillLevReq").Value);
                int armorUpdateSkillInt = Convert.ToInt32(currentel.Element("ArmorMastery").Value);
                if (armorUpdateSkillInt == 15) { armorUpdateMastery = "Magic Defense"; }
                if (armorUpdateSkillInt == 6) { armorUpdateMastery = "Melee Defense"; }
                if (armorUpdateSkillInt == 7) { armorUpdateMastery = "Missile Defense"; }
                armorUpdateWieldLevel = armorUpdateWieldValue.ToString();
                armorUpdateWieldMess = "Level to wield: " + armorUpdateWieldLevel;

                try
                {

                    if (armorpiece.Contains("Radiant"))
                    { armorUpdateWieldMess = "Required to wield: Level 180, Radiant Blood Society Level: " + armorUpdateWieldValue.ToString(); }
                    if (armorpiece.Contains("Eldrytch"))
                    { armorUpdateWieldMess = "Required to wield: Level 180, Eldrytch Webb Society Level: " + armorUpdateWieldValue.ToString(); }
                    if (armorpiece.Contains("Celestial"))
                    { armorUpdateWieldMess = "Required to wield: Level 180, Celestial Hand Society Level: " + armorUpdateWieldValue.ToString(); }
                    if ((armorUpdateWieldAttr.Contains("Magic")) || (armorUpdateWieldAttr.Contains("Missile")) || (armorUpdateWieldAttr.Contains("Melee")))
                    { armorUpdateWieldMess = "Required to wield: Level " + armorUpdateWieldLevel.ToString() + ", " + armorUpdateWieldAttr + ": " + armorUpdateWieldValue.ToString(); }

                }

                catch (Exception ex) { LogError(ex); }

                armorUpdateSkillMess = "; Required for activation: ObjLore - " + armorUpdateLore.ToString() + ", "
                               + armorUpdateMastery + " - " + armorUpdateSkillLevel.ToString();





                message = armorpiece + ", Al: " + armorUpdateAl + " , Work: " + armorUpdateWork + ", Tinks: " + armorUpdateTinks + ", Armor Wield Level: " +
                    armorUpdateLevel + ", Covers: " + armorUpdateCoversName + ", Set: " + armorUpdateSetName + armorUpdateWieldMess + armorUpdateSkillMess;
                WriteToChat(message);





                UpdateArmorHud();

            }
            catch (Exception ex) { LogError(ex); }
            return;
        }



        private void ArmorAvailableList_Click(object sender, int row, int col)
        {
            try
            {
                //    int mrow = row;
                currentel = availableSelected[row];
                string armorAvailableName = currentel.Element("ObjName").Value;
                string armorAvailableAl = currentel.Element("ObjAl").Value;
                string armorAvailableWork = currentel.Element("ObjWork").Value;
                string armorAvailableTinks = currentel.Element("ObjTink").Value;
                string armorAvailableLevel = currentel.Element("ObjWieldValue").Value;
                int armorAvailableSet = Convert.ToInt32(currentel.Element("ObjSet").Value);
                int armorAvailableToonLevel = Convert.ToInt32(currentel.Element("ObjToonLevel").Value);
                int armorAvailableCovers = Convert.ToInt32(currentel.Element("ObjCovers").Value);
                string availableArmorSetName = SetsIndex[armorAvailableSet].name;

                if (armorAvailableCovers > 0)
                {
                    foreach (IDNameLoadable piece in CoverageInvList)
                    {
                        if (piece.ID == armorAvailableCovers)
                        {
                            availableArmorCoversName = piece.name;
                            break;
                        }
                    }
                }

                int availableArmorLore = Convert.ToInt32(currentel.Element("ObjLoreReq").Value);

                long navailableArmorWieldAttr = Convert.ToInt32(currentel.Element("ObjWieldAttr").Value);
                if (navailableArmorWieldAttr == 7) { availableArmorWieldAttr = "Missile Defense"; }
                if (navailableArmorWieldAttr == 15) { availableArmorWieldAttr = "Magic Defense"; }
                if (navailableArmorWieldAttr == 6) { availableArmorWieldAttr = "Melee Defense"; }

                int availableArmorWieldValue = Convert.ToInt32(currentel.Element("ObjWieldValue").Value);
                int tempNum = 180;
                if (navailableArmorWieldAttr > 0)
                { armorAvailableLevel = armorAvailableToonLevel.ToString(); }
                else if ((armorAvailableName.Contains("Radiant")) || (armorAvailableName.Contains("Eldrytch")) || (armorAvailableName.Contains("Hand")))
                { armorAvailableLevel = tempNum.ToString(); }
                else if (navailableArmorWieldAttr <= 1)
                { armorAvailableLevel = availableArmorWieldValue.ToString(); }

                availableArmorSkillLevel = Convert.ToInt32(currentel.Element("ObjSkillLevReq").Value);
                int availableArmorSkillInt = Convert.ToInt32(currentel.Element("ObjMastery").Value);
                if (availableArmorSkillInt == 15) { availableArmorMastery = "Magic Defense"; }
                if (availableArmorSkillInt == 6) { availableArmorMastery = "Melee Defense"; }
                if (availableArmorSkillInt == 7) { availableArmorMastery = "Missile Defense"; }
                availableArmorWieldLevel = availableArmorWieldValue.ToString();
                availableArmorWieldMess = "Level to wield: " + availableArmorWieldLevel;

                try
                {

                    if (armorAvailableName.Contains("Radiant"))
                    { availableArmorWieldMess = "Required to wield: Level 180, Radiant Blood Society Level: " + availableArmorWieldValue.ToString(); }
                    if (armorAvailableName.Contains("Eldrytch"))
                    { availableArmorWieldMess = "Required to wield: Level 180, Eldrytch Webb Society Level: " + availableArmorWieldValue.ToString(); }
                    if (armorAvailableName.Contains("Celestial"))
                    { availableArmorWieldMess = "Required to wield: Level 180, Celestial Hand Society Level: " + availableArmorWieldValue.ToString(); }
                    if ((availableArmorWieldAttr.Contains("Magic")) || (availableArmorWieldAttr.Contains("Missile")) || (availableArmorWieldAttr.Contains("Melee")))
                    { availableArmorWieldMess = "Required to wield: Level " + armorAvailableLevel.ToString() + ", " + availableArmorWieldAttr + ": " + availableArmorWieldValue.ToString(); }

                }

                catch (Exception ex) { LogError(ex); }

                availableArmorSkillMess = "; Required for activation: ObjLore - " + availableArmorLore.ToString() + ", "
                               + availableArmorMastery + " - " + availableArmorSkillLevel.ToString();





                message = armorAvailableName + ", Al: " + armorAvailableAl + " , Work: " + armorAvailableWork + ", Tinks: " + armorAvailableTinks +                     armorAvailableLevel + ", Covers: " + armorUpdateCoversName + ", Set: " + armorAvailableSetName + ", " + availableArmorWieldMess + ", " + availableArmorSkillMess;
                WriteToChat(message);


            }
            catch (Exception ex) { LogError(ex); }
            return;
        }

        private void btnListArmorAvailable_Hit(object sender, EventArgs e)
        {
            if (!File.Exists(genInventoryFilename)) { WriteToChat("You must first do an inventory."); }
            else if (File.Exists(genInventoryFilename))
            {
                try
                {
                    XDocument tempAUIDoc = new XDocument(new XElement("Objs"));
                    tempAUIDoc.Save(armorSelectFilename);
                    tempAUIDoc = null;
                    lstArmorUpdateSelects = new List<string>();
                    WriteToChat("txtArmorUpdateChoice.Text: " + txtArmorUpdateChoice.Text);
             //       if (txtArmorUpdateChoice.Text != null)
              //      {
                       availableChoices = txtArmorUpdateChoice.Text.Trim();
                       WriteToChat("availableChoices: " + availableChoices);
                        availableChoices = availableChoices.ToLower();
                        WriteToChat("AvailableChoices " + availableChoices);
                        if (availableChoices.Contains(';'))
                        {
                            string[] split = availableChoices.Split(new Char[] { ';' });

                            foreach (string s in split)
                            {

                                if (s.Trim() != "")
                                {
                                    lstArmorUpdateSelects.Add(s);
                                }
                            }
                        }
                        else
                        {
                            lstArmorUpdateSelects.Add(availableChoices);
                        }

                //    }
                //    else { lstArmorUpdateSelects = null; }
                    WriteToChat("Count of selects " + lstArmorUpdateSelects.Count.ToString());
                }//end of try //

                catch (Exception ex) { LogError(ex); }


                try
                {
                   availableClass = txtArmorUpdateClass.Text.ToLower();
                    armorAvailableClass = txtArmorUpdateClass.Text.ToLower();
                    WriteToChat("Class: " + armorAvailableClass);
 
                 //   WriteToChat("armorAvailableClass: " + armorAvailableClass);
                    if (armorAvailableClass.Length == 0)
                    {
                        if (lstArmorUpdateSelects.Count > 0)
                        {

                            int n = lstArmorUpdateSelects.Count;
                      //      WriteToChat(lstArmorUpdateSelects.Count.ToString());
                            for (int i = 0; i < n; i++)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                from p in availableArmor
                                where p.Element("ObjSpellXml").Value.ToLower().Contains(lstArmorUpdateSelects[i])
                                select p));
                            }


                        }
                    }
                    else if (armorAvailableClass.Contains("armor") || armorAvailableClass.Contains("clothing"))
                    {
                       WriteToChat("I am in armoravailableclass" );
                        if (lstArmorUpdateSelects != null && lstArmorUpdateSelects[0].Trim() != "")
                        {
                            int n = lstArmorUpdateSelects.Count;
                            for (int i = 0; i < n; i++)
                            {
                                if (nArmorUpdateSet == 0 && nArmorUpdateCovers == 0)
                                {

                                    newDoc = new XDocument(new XElement("Objs",
                                    from p in availableArmor
                                    where p.Element("ObjClass").Value.ToLower().Contains(armorAvailableClass) &&
                                        p.Element("ObjSpellXml").Value.ToLower().Contains(lstArmorUpdateSelects[0])
                                    select p));

                                }


                                else if (nArmorUpdateSet > 0 && nArmorUpdateCovers == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                    from p in availableArmor
                                    where p.Element("ObjClass").Value.ToLower().Contains(armorAvailableClass) &&
                                        p.Element("ObjSet").Value == nArmorUpdateSet.ToString() &&
                                        p.Element("ObjSpellXml").Value.ToLower().Contains(lstArmorUpdateSelects[0])

                                    select p));

                                }
                                else if (nArmorUpdateCovers > 0 && nArmorUpdateSet == 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                    from p in availableArmor
                                    where p.Element("ObjClass").Value.ToLower().Contains(armorAvailableClass) &&
                                          p.Element("ObjCovers").Value == nArmorUpdateCovers.ToString() &&
                                           p.Element("ObjSpellXml").Value.ToLower().Contains(lstArmorUpdateSelects[0])
                                    select p));
                                }
                                else if (nArmorUpdateSet > 0 && nArmorUpdateCovers > 0)
                                {
                                    newDoc = new XDocument(new XElement("Objs",
                                    from p in availableArmor
                                    where p.Element("ObjClass").Value.ToLower().Contains(armorAvailableClass) &&
                                        p.Element("ObjSet").Value == nArmorUpdateSet.ToString() &&
                                          p.Element("ObjCovers").Value == nArmorUpdateCovers.ToString() &&
                                           p.Element("ObjSpellXml").Value.ToLower().Contains(lstArmorUpdateSelects[0])
                                    select p));
                                }
                            }
                        }

                        else
                        {
                            if (objArmorSet == 0 && objCovers == 0)
                            {

                                newDoc = new XDocument(new XElement("Objs",
                                from p in availableArmor
                                where p.Element("ObjClass").Value.ToLower().Contains(armorAvailableClass)
                                select p));
                            }

                            else if (objArmorSet > 0 && objCovers == 0)
                            {

                                newDoc = new XDocument(new XElement("Objs",
                                from p in availableArmor
                                where p.Element("ObjClass").Value.ToLower().Contains(armorAvailableClass) &&
                                    p.Element("ObjSet").Value == nArmorUpdateSet.ToString()
                                select p));
                            }
                            else if (objCovers > 0 && objArmorSet == 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                from p in availableArmor
                                where p.Element("ObjClass").Value.ToLower().Contains(armorAvailableClass) &&
                                      p.Element("ObjCovers").Value == nArmorUpdateCovers.ToString()
                                select p));
                            }
                            else if (objArmorSet > 0 && objCovers > 0)
                            {
                                newDoc = new XDocument(new XElement("Objs",
                                from p in availableArmor
                                where p.Element("ObjClass").Value.ToLower().Contains(armorAvailableClass) &&
                                    p.Element("ObjSet").Value == nArmorUpdateSet.ToString() &&
                                      p.Element("ObjCovers").Value == nArmorUpdateCovers.ToString()
                                select p));
                            }

                        }  //end of if spells


                    }
                    else if (armorAvailableClass.Contains("jewelry") && lstArmorUpdateSelects != null && lstArmorUpdateSelects[0].Trim() != "")
                        {

                            newDoc = new XDocument(new XElement("Objs",
                                 from p in availableArmor
                                 where p.Element("ObjClass").Value.ToLower().Contains(armorAvailableClass) &&
                                     p.Element("ObjSpellXml").Value.ToLower().Contains(lstArmorUpdateSelects[0])
                                 select p));
                        }

                        else if (armorAvailableClass.Contains("jewelry"))
                        {

                            newDoc = new XDocument(new XElement("Objs",
                                 from p in availableArmor
                                 where p.Element("ObjClass").Value.ToLower().Contains(armorAvailableClass)
                                 select p));
                        }
                        newDoc.Save(armorSelectFilename);
                    }

                
                catch (Exception ex) { LogError(ex); }
                FillArmorAvailableList();
            }
        }

  //   }// end of btnlist


        private void btnClearListArmorAvailable_Hit(object sender, EventArgs e)
        {
            clearArmorAvailableListVariables();
        }

        private void clearArmorAvailableListVariables()
        {
            ArmorAvailableList.ClearRows();
            txtArmorUpdateChoice.Text = "";
            txtArmorUpdateClass.Text = "";
            cboArmorUpdateCovers.Current = 0;
            cboArmorUpdateSet.Current = 0;
            //    newDoc = new XDocument(new XElement("Objs"));
            //    newDoc.Save(inventorySelect);
        }
        private void DisposeArmorTabLayout()
        {
            try
            {
                if (!ArmorMainTab) { return; }

                ArmorHudList.Click -= (sender, row, col) => ArmorHudList_Click(sender, row, col);
                ArmorHudList = null;
                lblToonArmorName.Text = "";
                lblToonArmorName = null;
                lblToonLevel.Text = "";
                lblToonLevel = null;
                lblToonMaster.Text = "";
                lblToonMaster = null;
                ClearMainArmorHudVariables();
                ArmorMainTab = false;


            }
            catch (Exception ex) { LogError(ex); }
        }

        private void ClearMainArmorHudVariables()
        {
        //    toonArmorName = "";
       //     lblToonArmorNameInfo.Text = "";
            lblToonLevel.Text = "";
            lblToonMaster.Text = "";
            myArmor = null;
            armorobjCoversName = null;
            updateSpells = null;
            armorpiece = null;  //contains name of piece of armor
            armorobjSetName = null;
            armorWieldAttr = null;
            armorWieldLevel = null;
            armorSkillLevel = 0;
            armorMastery = null;
            armorWieldMess = null;
            armorSkillMess = null;
            currentArmor = null;



        }

        private void DisposeArmorUpdateTabLayout()
        {
            try
            {
                if (!ArmorUpdateTab) { return; }
                ArmorUpdateHudList.Click -= (sender, row, col) => ArmorUpdateHudList_Click(sender, row, col);
                ArmorUpdateHudListRow = null;
                ArmorUpdateHudList.Dispose();
                ArmorAvailableList.Click -= (sender, row, col) => ArmorAvailableList_Click(sender, row, col);
                ArmorAvailableListRow = null;
                ArmorAvailableList.Dispose();
                lblToonArmorUpdateName.Text = "";
                lblToonArmorUpdateName = null;
                toonArmorUpdateName = null;
                btnClearListArmorAvailable.Hit -= (sender, index) => btnClearListArmorAvailable_Hit(sender, index);
                btnListArmorAvailable.Hit -= (sender, index) => btnListArmorAvailable_Hit(sender, index);
               btnListArmorAvailable = null;
                btnClearListArmorAvailable = null;
                lblToonArmorUpdateLevel.Text = "";
                lblToonArmorUpdateLevel = null;
                lblToonArmorUpdateMaster.Text = "";
                lblToonArmorUpdateMaster = null;
                lblArmorUpdateChoice.Text = "";
                lblArmorUpdateChoice = null;
                txtArmorUpdateChoice.Text = "";
                txtArmorUpdateChoice = null;
                lblArmorUpdateClass.Text = "";
                lblArmorUpdateClass = null;
                txtArmorUpdateClass.Text = "";
                txtArmorUpdateClass= null;
                lblArmorUpdateCovers.Text = "";
                lblArmorUpdateCovers = null;
                cboArmorUpdateCovers.Current = 0;
                cboArmorUpdateCovers = null;
                lblArmorUpdateSet.Text = "";
                lblArmorUpdateSet = null;
                cboArmorUpdateSet.Current = 0;
                cboArmorUpdateSet = null;
                lblToonArmorUpdateNameInfo.Text = "";
                lblToonArmorUpdateNameInfo = null;

                ClearArmorUpdateTabVariables();

                ArmorUpdateTab = false;       


            }
            catch (Exception ex) { LogError(ex); }
        }

        private void ClearArmorUpdateTabVariables()
        {
          objArmorUpdateSetName = null;
          objArmorUpdateCovers = null;
          armorUpdateClass = null;
          nArmorUpdateSet = 0;
          objArmorUpdateCoversName = null;
          nArmorUpdateCovers = 0;
          armorUpdateCoversName = null;
          armorUpdateSetName = null;
          armorUpdateWieldAttr = null;
          armorUpdateWieldLevel = null;
          armorUpdateSkillLevel = 0;
          armorUpdateMastery = null;
          armorUpdateWieldMess = null;
          armorUpdateSkillMess = null;
          currentArmor = null;
          availableSelected = null;
          lstArmorUpdateSelects = null;
    
}

 
        private void UpdateArmorHud()
        {
            try
            {

                if (!ArmorMainTab) { return; }

                ArmorHudList.ClearRows();

             }
            catch (Exception ex) { LogError(ex); }
            return;
        }
    }
}



