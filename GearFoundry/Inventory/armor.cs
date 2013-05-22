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
        private List<String> lstAllToonName;
        private HudView ArmorHudView = null;
        private HudFixedLayout ArmorHudLayout = null;
        private HudTabView ArmorHudTabView = null;
        private HudFixedLayout ArmorHudTabLayout = null;
        private HudList ArmorHudList = null;
        private HudList.HudListRowAccessor ArmorHudListRow = null;
        private const int ArmorRemoveCircle = 0x60011F8;

        private HudFixedLayout ArmorHudSettings;
        private HudStaticText lblToonArmorName;
        private HudCombo cboToonArmorName;
        private HudCheckBox ShowAllSpells;
        private HudCheckBox ShowWieldedArmor;
        private HudCheckBox ShowArmorinInventory;
        private HudCheckBox ShowBraceletsinInventory;
        private HudCheckBox ShowRingsinInventory;
        private HudCheckBox ShowNecklacesinInventory;

        XDocument xdocArmor;
        WindowsTimer mWaitingForArmorIDTimer = new WindowsTimer();


        void btnGetToonArmor_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            doGetArmor();
        }

        private void doGetArmor()
        {
            try
            {
                WriteToChat("ToonArmorButton was clicked");
                mWaitingForArmorID = new List<WorldObject>();

                armorFilename = toonDir + @"\" + toonName + "Armor.xml";
                genArmorFilename = currDir + @"\allToonsArmor.xml";
                holdingArmorFilename = world + @"\holdingArmor.xml";
                

                xdocArmor = new XDocument(new XElement("Objs"));


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
                //   xdocArmor.Save(genInventoryFilename);
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
                        string armorSpellXml = GoGetSpells(currentarmorobj);
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


                        //  xdoc.Save(inventoryFilename);


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

        //private HudCheckBox ShowAllMobs;Wie
        //private HudCheckBox ShowSelectedMobs;
        //private HudCheckBox ShowAllPlayers;
        //private HudCheckBox ShowAllegancePlayers;
        //private HudCheckBox ShowFellowPlayers;
        //private HudCheckBox ShowTrophies;
        //private HudCheckBox ShowLifeStones;
        //private HudCheckBox ShowAllPortals;
        //private HudCheckBox ShowAllNPCs;

        //private HudStaticText txtLSS1;
        //private HudStaticText txtLSS2;
        private string toonArmorName = null;
        private bool ArmorMainTab;
        private bool ArmorSettingsTab;

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

                ArmorHudView = new HudView("Armor", 300, 220, new ACImage(0x6AA5));
               // LandscapeHudView.Theme = VirindiViewService.HudViewDrawStyle.GetThemeByName("Minimalist Transparent");
                ArmorHudView.UserAlphaChangeable = false;
                ArmorHudView.ShowInBar = false;
                ArmorHudView.UserResizeable = true;
                ArmorHudView.Visible = true;
                ArmorHudView.Ghosted = false;
                ArmorHudView.UserMinimizable = false;
                ArmorHudView.UserClickThroughable = false;
                ArmorHudView.LoadUserSettings();


                ArmorHudLayout = new HudFixedLayout();
                ArmorHudView.Controls.HeadControl = ArmorHudLayout;

                ArmorHudTabView = new HudTabView();
                ArmorHudLayout.AddControl(ArmorHudTabView, new Rectangle(0, 0, 300, 220));

                ArmorHudTabLayout = new HudFixedLayout();
                ArmorHudTabView.AddTab(ArmorHudTabLayout, "Armor");

                ArmorHudSettings = new HudFixedLayout();
                ArmorHudTabView.AddTab(ArmorHudSettings, "Settings");

                ArmorHudTabView.OpenTabChange += ArmorHudTabView_OpenTabChange;

                RenderArmorTabLayout();

          //      SubscribeArmorEvents();

            }
            catch (Exception ex) { LogError(ex); }
            return;
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
            catch { }

        }


        private void RenderArmorTabLayout()
        {
            try
            {

                ArmorHudList = new HudList();
                ArmorHudTabLayout.AddControl(ArmorHudList, new Rectangle(0, 0, 300, 220));

                ArmorHudList.ControlHeight = 16;
                ArmorHudList.AddColumn(typeof(HudPictureBox), 16, null);
                ArmorHudList.AddColumn(typeof(HudStaticText), 230, null);
                ArmorHudList.AddColumn(typeof(HudPictureBox), 16, null);

                ArmorHudList.Click += (sender, row, col) => ArmorHudList_Click(sender, row, col);

            //    UpdateArmorHud();

                ArmorMainTab = true;

                


            }
             
            catch (Exception ex) { LogError(ex); }
       }

        private void FillArmorHudList()
        {
            xdocGenArmor = XDocument.Load(genArmorFilename);
            IEnumerable<XElement> names = xdocGenArmor.Element("Objs").Descendants("Obj");

            foreach(XElement el in names)
            {
                if(el.Element("ToonName").Value == toonArmorName)
                {
                    int icon = Convert.ToInt32(el.Element("ObjIcon").Value);
                    string armorpiece = el.Element("ObjName").Value;
                    string spells = el.Element("ObjSpellXML").Value;
                    ArmorHudListRow = ArmorHudList.AddRow();

                    ((HudPictureBox)ArmorHudListRow[0]).Image = icon + 0x6000000;
	    	    	((HudStaticText)ArmorHudListRow[1]).Text = armorpiece;
	    	    	((HudStaticText)ArmorHudListRow[2]).Text = spells;

                }
            }
            
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
                    
               xdocGenArmor = XDocument.Load(genArmorFilename);
                IEnumerable<XElement> names = xdocGenArmor.Element("Objs").Descendants("Obj");
                ControlGroup myToonNames = new ControlGroup();
                cboToonArmorName = new HudCombo(myToonNames);
         //       cboToonArmorName.Change += new MVIndexChangeEventArgs(cboToonArmorName_Change);


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


        // private HudTextBox ToonArmorName;
        //private HudCheckBox ShowAllSpells;
        //private HudCheckBox ShowWieldedArmor;
        //private HudCheckBox ShowArmorinInventory;
        //private HudCheckBox ShowBraceletsinInventory;
        //private HudCheckBox ShowRingsinInventory;
        //private HudCheckBox ShowNecklacesinInventory;
                lblToonArmorName = new HudStaticText();
                lblToonArmorName.Text = "Name of toon whose armor is being studied:";
                ArmorHudSettings.AddControl(lblToonArmorName,new Rectangle(0,0,250,16));

               ArmorHudSettings.AddControl(cboToonArmorName, new Rectangle(5, 15, 200, 16));

                //ShowAllMobs = new HudCheckBox();
                //ShowAllMobs.Text = "Track All Mobs";
                //LandscapeHudSettings.AddControl(ShowAllMobs, new Rectangle(0, 0, 150, 16));
                //ShowAllMobs.Checked = gsSettings.bShowAllMobs;

                //ShowSelectedMobs = new HudCheckBox();
                //ShowSelectedMobs.Text = "Track Selected Mobs";
                //LandscapeHudSettings.AddControl(ShowSelectedMobs, new Rectangle(0, 18, 150, 16));
                //ShowSelectedMobs.Checked = gsSettings.bShowSelectedMobs;

                //ShowAllPlayers = new HudCheckBox();
                //ShowAllPlayers.Text = "Track All Players";
                //LandscapeHudSettings.AddControl(ShowAllPlayers, new Rectangle(0, 36, 150, 16));
                //ShowAllPlayers.Checked = gsSettings.bShowAllPlayers;

                //ShowAllegancePlayers = new HudCheckBox();
                //ShowAllegancePlayers.Text = "Track Allegiance Players";
                //LandscapeHudSettings.AddControl(ShowAllegancePlayers, new Rectangle(0, 54, 150, 16));
                //ShowAllegancePlayers.Checked = gsSettings.bShowAllegancePlayers;

                //ShowFellowPlayers = new HudCheckBox();
                //ShowFellowPlayers.Text = "Track Fellowship Players";
                //LandscapeHudSettings.AddControl(ShowFellowPlayers, new Rectangle(0, 72, 150, 16));
                //ShowFellowPlayers.Checked = gsSettings.bShowFellowPlayers;

                //ShowAllNPCs = new HudCheckBox();
                //ShowAllNPCs.Text = "Track All NPCs";
                //LandscapeHudSettings.AddControl(ShowAllNPCs, new Rectangle(0, 90, 150, 16));
                //ShowAllNPCs.Checked = gsSettings.bShowAllNPCs;

                //ShowTrophies = new HudCheckBox();
                //ShowTrophies.Text = "Track Selected NPCs and Trophies";
                //LandscapeHudSettings.AddControl(ShowTrophies, new Rectangle(0, 108, 150, 16));
                //ShowTrophies.Checked = gsSettings.bShowTrophies;

                //ShowLifeStones = new HudCheckBox();
                //ShowLifeStones.Text = "Track Lifestones";
                //LandscapeHudSettings.AddControl(ShowLifeStones, new Rectangle(0, 126, 150, 16));
                //ShowLifeStones.Checked = gsSettings.bShowLifeStones;

                //ShowAllPortals = new HudCheckBox();
                //ShowAllPortals.Text = "Track Portals";
                //LandscapeHudSettings.AddControl(ShowAllPortals, new Rectangle(0, 144, 150, 16));
                //ShowAllPortals.Checked = gsSettings.bShowAllPortals;

                //txtLSS1 = new HudStaticText();
                //txtLSS1.Text = "Player tracking funtions do not request player IDs.";
                //txtLSS2 = new HudStaticText();
                //txtLSS2.Text = "Players will not track until ID'd another way.";
                //LandscapeHudSettings.AddControl(txtLSS1, new Rectangle(0, 162, 300, 16));
                //LandscapeHudSettings.AddControl(txtLSS2, new Rectangle(0, 180, 300, 16));

                //ShowAllMobs.Change += ShowAllMobs_Change;
                //ShowSelectedMobs.Change += ShowSelectedMobs_Change;
                //ShowAllPlayers.Change += ShowAllPlayers_Change;
                //ShowFellowPlayers.Change += ShowFellowPlayers_Change;
                //ShowAllegancePlayers.Change += ShowAllegancePlayers_Change;
                //ShowAllNPCs.Change += ShowAllNPCs_Change;
                //ShowTrophies.Change += ShowTrophies_Change;
                //ShowLifeStones.Change += ShowLifeStones_Change;
                //ShowAllPortals.Change += ShowAllPortals_Change;

                ArmorSettingsTab = true;
             }
            catch (Exception ex) { LogError(ex); }
       }

        private void cboToonArmorName_Change()
        {
            toonArmorName = cboToonArmorName.Current.ToString();
            FillArmorHudList();
            
        }

        private void DisposeArmorSettingsLayout()
        {
            try
            {
                if (!ArmorSettingsTab) { return; }
                //ShowAllMobs.Change -= ShowAllMobs_Change;
                //ShowSelectedMobs.Change -= ShowSelectedMobs_Change;
                //ShowAllPlayers.Change -= ShowAllPlayers_Change;
                //ShowFellowPlayers.Change -= ShowFellowPlayers_Change;
                //ShowAllegancePlayers.Change -= ShowAllegancePlayers_Change;
                //ShowAllNPCs.Change -= ShowAllNPCs_Change;
                //ShowTrophies.Change -= ShowTrophies_Change;
                //ShowLifeStones.Change -= ShowLifeStones_Change;
                //ShowAllPortals.Change -= ShowAllPortals_Change;

                //txtLSS2.Dispose();
                //txtLSS1.Dispose();
                //ShowAllPortals.Dispose();
                //ShowLifeStones.Dispose();
                //ShowTrophies.Dispose();
                //ShowAllNPCs.Dispose();
                //ShowFellowPlayers.Dispose();
                //ShowAllegancePlayers.Dispose();
                //ShowAllPlayers.Dispose();
                //ShowSelectedMobs.Dispose();
                //ShowAllMobs.Dispose();

                ArmorSettingsTab = false;
            }
            catch { }
        }



        private void DisposeArmorHud()
        {

            try
            {
                WriteToChat("I am in method to dispose armor hud");
              //  UnsubscribeArmorEvents();
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

        private void ArmorHudList_Click(object sender, int row, int col)
        {
            try
            {
                if (col == 0)
                {
//                    Host.Actions.UseItem(LandscapeTrackingList[row].Id, 0);
                }
                if (col == 1)
                {
                    //Host.Actions.SelectItem(LandscapeTrackingList[row].Id);
                    //int textcolor;

                    //switch (LandscapeTrackingList[row].IOR)
                    //{
                    //    case IOResult.lifestone:
                    //        textcolor = 13;
                    //        break;
                    //    case IOResult.monster:
                    //        textcolor = 6;
                    //        break;
                    //    case IOResult.allegplayers:
                    //        textcolor = 13;
                    //        break;
                    //    case IOResult.npc:
                    //        textcolor = 3;
                    //        break;
                    //    default:
                    //        textcolor = 2;
                    //        break;
                   // }
                    //HudToChat(LandscapeTrackingList[row].LinkString(), textcolor);
                    //nusearrowid = LandscapeTrackingList[row].Id;
                    //useArrow();
                }
                if (col == 2)
                {
                //    LandscapeExclusionList.Add(LandscapeTrackingList[row].Id);
                //    LandscapeTrackingList.RemoveAt(row);
                //}
//                UpdateLandscapeHud();
                }

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

                //foreach (IdentifiedObject spawn in LandscapeTrackingList)
                //{
                //    LandscapeHudListRow = LandscapeHudList.AddRow();

                //    ((HudPictureBox)LandscapeHudListRow[0]).Image = spawn.Icon + 0x6000000;
                //    ((HudStaticText)LandscapeHudListRow[1]).Text = spawn.IORString() + spawn.Name + spawn.DistanceString();
                //    if (spawn.IOR == IOResult.trophy) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Gold; }
                //    if (spawn.IOR == IOResult.lifestone) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.SkyBlue; }
                //    if (spawn.IOR == IOResult.monster) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Orange; }
                //    if (spawn.IOR == IOResult.npc) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Yellow; }
                //    if (spawn.IOR == IOResult.portal) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.MediumPurple; }
                //    if (spawn.IOR == IOResult.players) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.AntiqueWhite; }
                //    if (spawn.IOR == IOResult.fellowplayer) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.LightGreen; }
                //    if (spawn.IOR == IOResult.allegplayers) { ((HudStaticText)LandscapeHudListRow[1]).TextColor = Color.Tan; }
                //    ((HudPictureBox)LandscapeHudListRow[2]).Image = LandscapeRemoveCircle;
                //}
            }
            catch (Exception ex) { LogError(ex); }
            return;
        }
    }
}



