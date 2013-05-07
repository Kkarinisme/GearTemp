using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VirindiViewService;
using VirindiViewService.Controls;
using VirindiHUDs;
using MyClasses.MetaViewWrappers;
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;
using System.Drawing;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Linq;
using System.IO;
using VirindiViewService.Themes;

namespace GearFoundry
{

    public partial class PluginCore : PluginBase
    {
        XDocument xdocQuickSlotsv = null;
        XDocument xdocQuickSlotsh = null;

        private static VirindiViewService.HudView quickiesvHud = null;
        private static VirindiViewService.Controls.HudFixedLayout quickiesvHud_Head = null;
        private static VirindiViewService.Controls.HudTabView quickiesvTabView = null;
        private static VirindiViewService.Controls.HudFixedLayout quickiesvTabFixedLayout = null;

        private VirindiViewService.Controls.HudButton btnQuickiesvAdd = null;
        private VirindiViewService.Controls.HudButton btnQuickiesvRemove = null;

        private HudImageStack mQuickStackv0 = null;
        private HudImageStack mQuickStackv1 = null;
        private HudImageStack mQuickStackv2 = null;
        private HudImageStack mQuickStackv3 = null;
        private HudImageStack mQuickStackv4 = null;
        private HudImageStack mQuickStackv5 = null;
        private HudImageStack mQuickStackv6 = null;
        private HudImageStack mQuickStackv7 = null;
        private HudImageStack mQuickStackv8 = null;
        private HudImageStack mQuickStackv9 = null;
        private HudImageStack mQuickStackv10 = null;
        private HudImageStack mQuickStackv11 = null;
        private HudImageStack mQuickStackv12 = null;
        private HudImageStack mQuickStackv13 = null;
        private HudImageStack mQuickStackv14 = null;


        int nQuickieIDv0 = 0;
        int nQuickieIDv1 = 0;
        int nQuickieIDv2 = 0;
        int nQuickieIDv3 = 0;
        int nQuickieIDv4 = 0;
        int nQuickieIDv5 = 0;
        int nQuickieIDv6 = 0;
        int nQuickieIDv7 = 0;
        int nQuickieIDv8 = 0;
        int nQuickieIDv9 = 0;
        int nQuickieIDv10 = 0;
        int nQuickieIDv11 = 0;
        int nQuickieIDv12 = 0;
        int nQuickieIDv13 = 0;
        int nQuickieIDv14 = 0;




        private static VirindiViewService.HudView quickieshHud = null;
        private static VirindiViewService.Controls.HudFixedLayout quickieshHud_Head = null;
        private static VirindiViewService.Controls.HudTabView quickieshTabView = null;
        private static VirindiViewService.Controls.HudFixedLayout quickieshTabFixedLayout = null;

        private HudImageStack mQuickStackh0 = null;
        private HudImageStack mQuickStackh1 = null;
        private HudImageStack mQuickStackh2 = null;
        private HudImageStack mQuickStackh3 = null;
        private HudImageStack mQuickStackh4 = null;
        private HudImageStack mQuickStackh5 = null;
        private HudImageStack mQuickStackh6 = null;
        private HudImageStack mQuickStackh7 = null;
        private HudImageStack mQuickStackh8 = null;
        private HudImageStack mQuickStackh9 = null;
        private HudImageStack mQuickStackh10 = null;
        private HudImageStack mQuickStackh11 = null;
        private HudImageStack mQuickStackh12 = null;
        private HudImageStack mQuickStackh13 = null;
        private HudImageStack mQuickStackh14 = null;

        private VirindiViewService.Controls.HudButton btnQuickieshAdd = null;
        private VirindiViewService.Controls.HudButton btnQuickieshRemove = null;

        int nQuickieIDh0 = 0;
        int nQuickieIDh1 = 0;
        int nQuickieIDh2 = 0;
        int nQuickieIDh3 = 0;
        int nQuickieIDh4 = 0;
        int nQuickieIDh5 = 0;
        int nQuickieIDh6 = 0;
        int nQuickieIDh7 = 0;
        int nQuickieIDh8 = 0;
        int nQuickieIDh9 = 0;
        int nQuickieIDh10 = 0;
        int nQuickieIDh11 = 0;
        int nQuickieIDh12 = 0;
        int nQuickieIDh13 = 0;
        int nQuickieIDh14 = 0;

        private static VirindiViewService.HudView quickiesHud = null;



        WorldObject quickie = null;
        bool baddItem = false;
        bool bremoveItem = false;
        int nquickiev = 0;
        int nquickieh = 0;
        QuickSlotData thisQuickie = null;
        Point vpt;
        Point hpt;
        VirindiViewService.HudViewDrawStyle mvtheme = null;
        VirindiViewService.HudViewDrawStyle mhtheme = null;
       

        List<Int32> vID = new List<Int32>();
        List<Int32> hID = new List<Int32>();
        List<HudImageStack> vst = new List<HudImageStack>();
        List<HudImageStack> hst = new List<HudImageStack>();



        public class QuickSlotData
        {
            public string Name;
            public int Guid;
            public ObjectClass ObjectClass;
            public int ImbueId;
            public int Icon;
            public int IconUnderlay;
            public int IconOverlay;
        }

        private void RenderVerticalQuickSlots()
        {
            WriteToChat("I am in the function to render vertical quickslots");
 
            if (quickiesvHud != null)
            {
                DisposeVerticalQuickSlots();
            }
            if (!File.Exists(quickSlotsvFilename))
            {
                XDocument tempDoc = new XDocument(new XElement("Objs"));
                tempDoc.Save(quickSlotsvFilename);
                tempDoc = null;
            }

            xdocQuickSlotsv = XDocument.Load(quickSlotsvFilename);

            quickiesvHud = new VirindiViewService.HudView("", 30, 360, new ACImage(Color.Transparent),false,"quickiesvhud");
            quickiesvHud.ShowInBar = false;
            quickiesvHud.UserAlphaChangeable = false;
            quickiesvHud.Visible = true;
            quickiesvHud.UserClickThroughable = false;
            quickiesvHud.UserGhostable = true;
            quickiesvHud.UserMinimizable = false;
            quickiesvHud.UserResizeable = false;
            quickiesvHud.LoadUserSettings();
            quickiesvHud_Head = new HudFixedLayout();
            quickiesvHud.Controls.HeadControl = quickiesvHud_Head;
            quickiesvTabView = new HudTabView();
            quickiesvTabFixedLayout = new HudFixedLayout();

            quickiesvHud_Head.AddControl(quickiesvTabView, new Rectangle(0, 0, 29, 359));
            quickiesvTabView.AddTab(quickiesvTabFixedLayout, "");       

                btnQuickiesvAdd = new VirindiViewService.Controls.HudButton();
                btnQuickiesvAdd.Text = "+";
                btnQuickiesvAdd.Visible = true;

                btnQuickiesvRemove = new VirindiViewService.Controls.HudButton();
                btnQuickiesvRemove.Text = "-";
                btnQuickiesvRemove.Visible = true;

                quickiesvTabFixedLayout.AddControl(btnQuickiesvAdd, new Rectangle(0, 0, 12, 12));
                quickiesvTabFixedLayout.AddControl(btnQuickiesvRemove, new Rectangle(15, 0, 12, 12));
 
            mQuickStackv0 = new HudImageStack();
            mQuickStackv1 = new HudImageStack();
            mQuickStackv2 = new HudImageStack();
            mQuickStackv3 = new HudImageStack();
            mQuickStackv4 = new HudImageStack();
            mQuickStackv5 = new HudImageStack();
            mQuickStackv6 = new HudImageStack();
            mQuickStackv7 = new HudImageStack();
            mQuickStackv8 = new HudImageStack();
            mQuickStackv9 = new HudImageStack();
            mQuickStackv10 = new HudImageStack();
            mQuickStackv11 = new HudImageStack();
            mQuickStackv12 = new HudImageStack();
            mQuickStackv13 = new HudImageStack();
            mQuickStackv14 = new HudImageStack();


            vst.Add(mQuickStackv0);
            vst.Add(mQuickStackv1);
            vst.Add(mQuickStackv2);
            vst.Add(mQuickStackv3);
            vst.Add(mQuickStackv4);
            vst.Add(mQuickStackv5);
            vst.Add(mQuickStackv6);
            vst.Add(mQuickStackv7);
            vst.Add(mQuickStackv8);
            vst.Add(mQuickStackv9);
            vst.Add(mQuickStackv10);
            vst.Add(mQuickStackv11);
            vst.Add(mQuickStackv12);
            vst.Add(mQuickStackv13);
            vst.Add(mQuickStackv14);

            vID.Add(nQuickieIDv0);
            vID.Add(nQuickieIDv1);
            vID.Add(nQuickieIDv2);
            vID.Add(nQuickieIDv3);
            vID.Add(nQuickieIDv4);
            vID.Add(nQuickieIDv5);
            vID.Add(nQuickieIDv6);
            vID.Add(nQuickieIDv7);
            vID.Add(nQuickieIDv8);
            vID.Add(nQuickieIDv9);
            vID.Add(nQuickieIDv10);
            vID.Add(nQuickieIDv11);
            vID.Add(nQuickieIDv12);
            vID.Add(nQuickieIDv13);
            vID.Add(nQuickieIDv14);

            btnQuickiesvAdd.Hit += (sender, obj) => btnQuickiesvAdd_Hit(sender, obj);
            btnQuickiesvRemove.Hit += (sender, obj) => btnQuickiesvRemove_Hit(sender, obj);


            if (xdocQuickSlotsv.Root.HasElements)
            {
                doGetData(xdocQuickSlotsv, quickSlotsvFilename);
            }
  
        }

        private void DisposeVerticalQuickSlots()
        {

            if (btnQuickiesvAdd != null) {btnQuickiesvAdd.Hit -= (sender, obj) => btnQuickiesvAdd_Hit(sender, obj);btnQuickiesvAdd.Dispose();}
            if (btnQuickiesvRemove != null) { btnQuickiesvRemove.Hit -= (sender, obj) => btnQuickiesvRemove_Hit(sender, obj); btnQuickiesvRemove.Dispose(); }

            if (mQuickStackv0 != null) { mQuickStackv0.Hit -= (sender, obj) => mQuickStackv0_Hit(sender, obj); mQuickStackv0.Dispose(); }
            if (mQuickStackv1 != null) { mQuickStackv1.Hit -= (sender, obj) => mQuickStackv0_Hit(sender, obj); mQuickStackv1.Dispose(); }
            if (mQuickStackv2 != null) { mQuickStackv2.Hit -= (sender, obj) => mQuickStackv0_Hit(sender, obj); mQuickStackv2.Dispose(); }
            if (mQuickStackv3 != null) { mQuickStackv3.Hit -= (sender, obj) => mQuickStackv0_Hit(sender, obj); mQuickStackv3.Dispose(); }
            if (mQuickStackv4 != null) { mQuickStackv4.Hit -= (sender, obj) => mQuickStackv0_Hit(sender, obj); mQuickStackv4.Dispose(); }
            if (mQuickStackv5 != null) { mQuickStackv5.Hit -= (sender, obj) => mQuickStackv0_Hit(sender, obj); mQuickStackv5.Dispose(); }
            if (mQuickStackv6 != null) { mQuickStackv6.Hit -= (sender, obj) => mQuickStackv0_Hit(sender, obj); mQuickStackv6.Dispose(); }
            if (mQuickStackv7 != null) { mQuickStackv7.Hit -= (sender, obj) => mQuickStackv0_Hit(sender, obj); mQuickStackv7.Dispose(); }
            if (mQuickStackv8 != null) { mQuickStackv8.Hit -= (sender, obj) => mQuickStackv0_Hit(sender, obj); mQuickStackv8.Dispose(); }
            if (mQuickStackv9 != null) { mQuickStackv9.Hit -= (sender, obj) => mQuickStackv0_Hit(sender, obj); mQuickStackv9.Dispose(); }
            if (mQuickStackv10 != null) { mQuickStackv10.Hit -= (sender, obj) => mQuickStackv0_Hit(sender, obj); mQuickStackv10.Dispose(); }
            if (mQuickStackv11 != null) { mQuickStackv11.Hit -= (sender, obj) => mQuickStackv0_Hit(sender, obj); mQuickStackv11.Dispose(); }
            if (mQuickStackv12 != null) { mQuickStackv12.Hit -= (sender, obj) => mQuickStackv0_Hit(sender, obj); mQuickStackv12.Dispose(); }
            if (mQuickStackv13 != null) { mQuickStackv13.Hit -= (sender, obj) => mQuickStackv0_Hit(sender, obj); mQuickStackv13.Dispose(); }
            if (mQuickStackv14 != null) { mQuickStackv14.Hit -= (sender, obj) => mQuickStackv0_Hit(sender, obj); mQuickStackv14.Dispose(); }
            
            quickiesvHud_Head.Dispose();
            quickiesvHud.Dispose();
            nquickiev = 0;
            try
            {

                for (int i = 0; i < 15; i++)
                { vst[i] = null; }
            }
            catch (Exception ex) { LogError(ex); }

            try
            {

                for (int i = 0; i < 15; i++)
                { vID[i] = 0; }
            }
            catch (Exception ex) { LogError(ex); }


        }

        private void RenderHorizontalQuickSlots()
        {
            WriteToChat("I am in the function to render horizontal quickslots");
 
            if (quickieshHud != null)
            {
                DisposeHorizontalQuickSlots();
            }

             if (!File.Exists(quickSlotshFilename))
                {
                    XDocument tempDoc = new XDocument(new XElement("Objs"));
                    tempDoc.Save(quickSlotshFilename);
                    tempDoc = null;
                }

                xdocQuickSlotsh = XDocument.Load(quickSlotshFilename);

 

                quickieshHud = new VirindiViewService.HudView("", 385, 40, new ACImage(Color.Transparent),false,"quickieshhud");
                quickieshHud.ShowInBar = false;
                quickieshHud.UserAlphaChangeable = false;
                quickieshHud.Visible = true;
                quickieshHud.UserGhostable = true;
                quickieshHud.UserMinimizable = false;
                 quickieshHud.UserClickThroughable = false;
                quickieshHud.UserResizeable = false;
                quickieshHud.LoadUserSettings();

                quickieshHud_Head = new HudFixedLayout();
                quickieshHud.Controls.HeadControl = quickieshHud_Head;

                quickieshTabView = new HudTabView();
                quickieshTabFixedLayout = new HudFixedLayout();

                quickieshHud_Head.AddControl(quickieshTabView, new Rectangle(0, 0, 385, 40));
                quickieshTabView.AddTab(quickieshTabFixedLayout, "Horizontal Switchgear");

                btnQuickieshAdd = new VirindiViewService.Controls.HudButton();
            btnQuickieshAdd.Text = "+";
            btnQuickieshAdd.Visible = true;

            btnQuickieshRemove = new VirindiViewService.Controls.HudButton();
            btnQuickieshRemove.Text = "-";
            btnQuickieshRemove.Visible = true;

            quickieshTabFixedLayout.AddControl(btnQuickieshAdd, new Rectangle(0, 0, 12, 12));
            quickieshTabFixedLayout.AddControl(btnQuickieshRemove, new Rectangle(15, 0, 12, 12));


            mQuickStackh0 = new HudImageStack();
            mQuickStackh1 = new HudImageStack();
            mQuickStackh2 = new HudImageStack();
            mQuickStackh3 = new HudImageStack();
            mQuickStackh4 = new HudImageStack();
            mQuickStackh5 = new HudImageStack();
            mQuickStackh6 = new HudImageStack();
            mQuickStackh7 = new HudImageStack();
            mQuickStackh8 = new HudImageStack();
            mQuickStackh9 = new HudImageStack();
            mQuickStackh10 = new HudImageStack();
            mQuickStackh11 = new HudImageStack();
            mQuickStackh12 = new HudImageStack();
            mQuickStackh13 = new HudImageStack();
            mQuickStackh14 = new HudImageStack();


             hst.Add(mQuickStackh0);
            hst.Add(mQuickStackh1);
            hst.Add(mQuickStackh2);
            hst.Add(mQuickStackh3);
            hst.Add(mQuickStackh4);
            hst.Add(mQuickStackh5);
            hst.Add(mQuickStackh6);
            hst.Add(mQuickStackh7);
            hst.Add(mQuickStackh8);
            hst.Add(mQuickStackh9);
            hst.Add(mQuickStackh10);
            hst.Add(mQuickStackh11);
            hst.Add(mQuickStackh12);
            hst.Add(mQuickStackh13);
            hst.Add(mQuickStackh14);

            hID.Add(nQuickieIDh0);
            hID.Add(nQuickieIDh1);
            hID.Add(nQuickieIDh2);
            hID.Add(nQuickieIDh3);
            hID.Add(nQuickieIDh4);
            hID.Add(nQuickieIDh5);
            hID.Add(nQuickieIDh6);
            hID.Add(nQuickieIDh7);
            hID.Add(nQuickieIDh8);
            hID.Add(nQuickieIDh9);
            hID.Add(nQuickieIDh10);
            hID.Add(nQuickieIDh11);
            hID.Add(nQuickieIDh12);
            hID.Add(nQuickieIDh13);
            hID.Add(nQuickieIDh14);


            btnQuickieshAdd.Hit += (sender, obj) => btnQuickieshAdd_Hit(sender, obj);
            btnQuickieshRemove.Hit += (sender, obj) => btnQuickieshRemove_Hit(sender, obj);

            if (xdocQuickSlotsh.Root.HasElements)
            {
                doGetData(xdocQuickSlotsh, quickSlotshFilename);
            }
  
        }

        private void DisposeHorizontalQuickSlots()
        {

            if (btnQuickieshAdd != null) { btnQuickieshAdd.Hit -= (sender, obj) => btnQuickieshAdd_Hit(sender, obj); btnQuickieshAdd.Dispose(); }
            if (btnQuickieshRemove != null) { btnQuickieshRemove.Hit -= (sender, obj) => btnQuickieshRemove_Hit(sender, obj); btnQuickieshRemove.Dispose(); }

            if (mQuickStackh0 != null) { mQuickStackh0.Hit -= (sender, obj) => mQuickStackh0_Hit(sender, obj); mQuickStackh0.Dispose(); }
            if (mQuickStackh1 != null) { mQuickStackh1.Hit -= (sender, obj) => mQuickStackh0_Hit(sender, obj); mQuickStackh1.Dispose(); }
            if (mQuickStackh2 != null) { mQuickStackh2.Hit -= (sender, obj) => mQuickStackh0_Hit(sender, obj); mQuickStackh2.Dispose(); }
            if (mQuickStackh3 != null) { mQuickStackh3.Hit -= (sender, obj) => mQuickStackh0_Hit(sender, obj); mQuickStackh3.Dispose(); }
            if (mQuickStackh4 != null) { mQuickStackh4.Hit -= (sender, obj) => mQuickStackh0_Hit(sender, obj); mQuickStackh4.Dispose(); }
            if (mQuickStackh5 != null) { mQuickStackh5.Hit -= (sender, obj) => mQuickStackh0_Hit(sender, obj); mQuickStackh5.Dispose(); }
            if (mQuickStackh6 != null) { mQuickStackh6.Hit -= (sender, obj) => mQuickStackh0_Hit(sender, obj); mQuickStackh6.Dispose(); }
            if (mQuickStackh7 != null) { mQuickStackh7.Hit -= (sender, obj) => mQuickStackh0_Hit(sender, obj); mQuickStackh7.Dispose(); }
            if (mQuickStackh8 != null) { mQuickStackh8.Hit -= (sender, obj) => mQuickStackh0_Hit(sender, obj); mQuickStackh8.Dispose(); }
            if (mQuickStackh9 != null) { mQuickStackh9.Hit -= (sender, obj) => mQuickStackh0_Hit(sender, obj); mQuickStackh9.Dispose(); }
            if (mQuickStackh10 != null) { mQuickStackh10.Hit -= (sender, obj) => mQuickStackh0_Hit(sender, obj); mQuickStackh10.Dispose(); }
            if (mQuickStackh11 != null) { mQuickStackh11.Hit -= (sender, obj) => mQuickStackh0_Hit(sender, obj); mQuickStackh11.Dispose(); }
            if (mQuickStackh12 != null) { mQuickStackh12.Hit -= (sender, obj) => mQuickStackh0_Hit(sender, obj); mQuickStackh12.Dispose(); }
            if (mQuickStackh13 != null) { mQuickStackh13.Hit -= (sender, obj) => mQuickStackh0_Hit(sender, obj); mQuickStackh13.Dispose(); }
            if (mQuickStackh14 != null) { mQuickStackh14.Hit -= (sender, obj) => mQuickStackh0_Hit(sender, obj); mQuickStackh14.Dispose(); }
            quickieshHud_Head.Dispose();
            quickieshHud.Dispose();

             nquickieh = 0;
              try
                    {
                        for (int i = 0; i < 15; i++)
                        { hst[i] = null; }
                    }
               catch (Exception ex) { LogError(ex); }

               try
                    {
                        for (int i = 0; i < 15; i++)
                        { hID[i] = 0; }
                    }
           
            catch (Exception ex) { LogError(ex); }
        }

        

 

        private void doGetData(XDocument xdoc, string filename)
        {

            try
            {

                if (xdoc == xdocQuickSlotsv) { nquickiev = 0; }
                else if (xdoc == xdocQuickSlotsh) { nquickieh = 0; }

                IEnumerable<XElement> elements = xdoc.Element("Objs").Descendants("Obj");

                foreach (XElement elem in elements)
                {
                    thisQuickie = new QuickSlotData();
                    thisQuickie.Guid = Convert.ToInt32(elem.Element("QID").Value);
                    thisQuickie.Icon = Convert.ToInt32(elem.Element("QIcon").Value);
                    thisQuickie.IconOverlay = Convert.ToInt32(elem.Element("QIconOverlay").Value);
                    thisQuickie.IconUnderlay = Convert.ToInt32(elem.Element("QIconUnderlay").Value);
                    fillHud(xdoc, filename, thisQuickie);

                }
            }
            catch (Exception ex) { LogError(ex); }

        }

        private void btnQuickiesvAdd_Hit(object sender, System.EventArgs e)
        {
            btnAdd(quickiesvHud);
        }

        private void btnQuickieshAdd_Hit(object sender, System.EventArgs e)
        {
            btnAdd(quickieshHud);
        }


        private void btnAdd(VirindiViewService.HudView hud)
        {
            try
            {
                quickiesHud = hud;
                baddItem = true;
                bremoveItem = false;
            }

            catch (Exception ex) { LogError(ex); }

        }

        private void btnQuickiesvRemove_Hit(object sender, System.EventArgs e)
        {
            btnRemove(quickiesvHud);
        }

        private void btnQuickieshRemove_Hit(object sender, System.EventArgs e)
        {
            btnRemove(quickieshHud);
        }

        private void btnRemove(VirindiViewService.HudView hud)
        {
            try
            {
                bremoveItem = true;
                baddItem = false;
            }

            catch (Exception ex) { LogError(ex); }

        }


        private void Current_ItemSelected(object sender, ItemSelectedEventArgs e)
        {

            if (baddItem)
            {
                try
                {
                    // The following adds the icon of an item selected to the hudview

                    int objSelectedID = e.ItemGuid;

                    foreach (Decal.Adapter.Wrappers.WorldObject obj in Core.WorldFilter.GetInventory())
                    {
                        if (obj.Id == objSelectedID)
                        {
                            quickie = obj;

                            break;

                        }

                    }

                    QuickSlotData thisQuickie = new QuickSlotData();
                    thisQuickie.Guid = objSelectedID;
                    thisQuickie.Icon = quickie.Icon;
                    thisQuickie.IconOverlay = quickie.Values(LongValueKey.IconOverlay);
                    thisQuickie.IconUnderlay = quickie.Values(LongValueKey.IconUnderlay);
                    if (quickiesHud == quickiesvHud)
                    {
                        fillHud(xdocQuickSlotsv, quickSlotsvFilename, thisQuickie);
                        writeToQuickSlots(xdocQuickSlotsv, quickSlotsvFilename, thisQuickie);
                    }
                    else if (quickiesHud == quickieshHud)
                    {
                        fillHud(xdocQuickSlotsh, quickSlotshFilename, thisQuickie);
                        writeToQuickSlots(xdocQuickSlotsh, quickSlotshFilename, thisQuickie);
                    }

                }
                catch (Exception ex) { LogError(ex); }
                baddItem = false;
            }

        }


        private void doQuickieChkWork(Int32 qid, XDocument xdoc, string filename, Int32 n, VirindiViewService.HudView hud)
        {
            if (bremoveItem)
            {
                try
                {
                    IEnumerable<XElement> elements = xdoc.Element("Objs").Descendants("Obj");

                    xdoc.Descendants("Obj").Where(x => x.Element("QID").Value == qid.ToString()).Remove();
                    xdoc.Save(filename);
                }
                catch (Exception ex) { LogError(ex); }
                bremoveItem = false;

                if (hud == quickiesvHud)
                {
                    DisposeVerticalQuickSlots();
                    RenderVerticalQuickSlots();
                }
                else if (hud == quickieshHud)
                {
                    DisposeHorizontalQuickSlots();
                    RenderHorizontalQuickSlots();
                }
            }
            else if (!bremoveItem)
            {

                CoreManager.Current.Actions.UseItem(qid, 0);

            }

        }

        private void mQuickStackv0_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDv0, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
            }
            catch (Exception ex) { LogError(ex); }

        }

        private void mQuickStackv1_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDv1, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackv2_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDv2, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
            }
            catch (Exception ex) { LogError(ex); }

        }

        private void mQuickStackv3_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDv3, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackv4_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDv4, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackv5_Hit(object sender, System.EventArgs e)
        {
            try
            {

                doQuickieChkWork(nQuickieIDv5, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackv6_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDv6, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackv7_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDv7, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackv8_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDv8, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackv9_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDv9, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackv10_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDv10, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackv11_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDv11, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackv12_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDv12, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackv13_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDv13, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackv14_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDv14, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
            }
            catch (Exception ex) { LogError(ex); }

        }

        private void mQuickStackh0_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDh0, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
            }
            catch (Exception ex) { LogError(ex); }

        }

        private void mQuickStackh1_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDh1, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackh2_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDh2, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
            }
            catch (Exception ex) { LogError(ex); }

        }

        private void mQuickStackh3_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDh3, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackh4_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDh4, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackh5_Hit(object sender, System.EventArgs e)
        {
            try
            {

                doQuickieChkWork(nQuickieIDh5, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackh6_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDh6, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackh7_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDh7, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackh8_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDh8, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackh9_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDh9, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackh10_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDh10, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackh11_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDh11, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackh12_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDh12, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackh13_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDh13, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mQuickStackh14_Hit(object sender, System.EventArgs e)
        {
            try
            {
                doQuickieChkWork(nQuickieIDh14, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
            }
            catch (Exception ex) { LogError(ex); }

        }

        private void fillHud(XDocument xdoc, string filename, QuickSlotData thisQuickie)
        {
            ACImage mQuickSlots;
            Rectangle rec = new Rectangle(0, 0, 20, 20);

            HudImageStack mQuickStacks = new HudImageStack();
            try
            {
                if (thisQuickie.IconUnderlay != 0)
                {
                    mQuickSlots = new ACImage(thisQuickie.IconUnderlay);

                    mQuickStacks.Add(rec, mQuickSlots);
                }

                mQuickSlots = new ACImage(thisQuickie.Icon);
                mQuickStacks.Add(rec, mQuickSlots);

                if (thisQuickie.IconOverlay != 0)
                {
                    mQuickSlots = new ACImage(0x6000000 + thisQuickie.IconOverlay);
                    mQuickStacks.Add(rec, mQuickSlots);
                }
            }
            catch (Exception ex) { LogError(ex); }


            if (xdoc == xdocQuickSlotsv)
            {
                switch (nquickiev)
                {
                    case 0:
                        mQuickStackv0 = mQuickStacks;
                        quickiesvTabFixedLayout.AddControl(mQuickStackv0, new Rectangle(2, 15, 25, 25));
                        nQuickieIDv0 = thisQuickie.Guid;
                        nquickiev++;
                        mQuickStackv0.Hit += (sender, obj) => mQuickStackv0_Hit(sender, obj);
                        break;
                    case 1:
                        mQuickStackv1 = mQuickStacks;
                        quickiesvTabFixedLayout.AddControl(mQuickStackv1, new Rectangle(2, 37, 25, 25));
                        nQuickieIDv1 = thisQuickie.Guid;
                        nquickiev++;
                        mQuickStackv1.Hit += (sender, obj) => mQuickStackv1_Hit(sender, obj);
                        break;
                    case 2:
                        mQuickStackv2 = mQuickStacks;
                        quickiesvTabFixedLayout.AddControl(mQuickStackv2, new Rectangle(2, 59, 25, 25));
                        nQuickieIDv2 = thisQuickie.Guid;
                        nquickiev++;
                        mQuickStackv2.Hit += (sender, obj) => mQuickStackv2_Hit(sender, obj);
                        break;
                    case 3:
                        mQuickStackv3 = mQuickStacks;
                        quickiesvTabFixedLayout.AddControl(mQuickStackv3, new Rectangle(2, 81, 25, 25));
                        nQuickieIDv3 = thisQuickie.Guid;
                        nquickiev++;
                        mQuickStackv3.Hit += (sender, obj) => mQuickStackv3_Hit(sender, obj);
                        break;
                    case 4:
                        mQuickStackv4 = mQuickStacks;
                        quickiesvTabFixedLayout.AddControl(mQuickStackv4, new Rectangle(2, 103, 25, 25));
                        nQuickieIDv4 = thisQuickie.Guid;
                        mQuickStackv4.Hit += (sender, obj) => mQuickStackv4_Hit(sender, obj);
                        nquickiev++;
                        break;
                    case 5:
                        mQuickStackv5 = mQuickStacks;
                        quickiesvTabFixedLayout.AddControl(mQuickStackv5, new Rectangle(2, 125, 25, 25));
                        nQuickieIDv5 = thisQuickie.Guid;
                        mQuickStackv5.Hit += (sender, obj) => mQuickStackv5_Hit(sender, obj);
                        nquickiev++;
                        break;
                    case 6:
                        mQuickStackv6 = mQuickStacks;
                        quickiesvTabFixedLayout.AddControl(mQuickStackv6, new Rectangle(2, 147, 25, 25));
                        nQuickieIDv6 = thisQuickie.Guid;
                        nquickiev++;
                        mQuickStackv6.Hit += (sender, obj) => mQuickStackv6_Hit(sender, obj);
                        break;
                    case 7:
                        mQuickStackv7 = mQuickStacks;
                        quickiesvTabFixedLayout.AddControl(mQuickStackv7, new Rectangle(2, 169, 25, 25));
                        nQuickieIDv7 = thisQuickie.Guid;
                        nquickiev++;
                        mQuickStackv7.Hit += (sender, obj) => mQuickStackv7_Hit(sender, obj);
                        break;
                    case 8:
                        mQuickStackv8 = mQuickStacks;
                        quickiesvTabFixedLayout.AddControl(mQuickStackv8, new Rectangle(2, 191, 25, 25));
                        nQuickieIDv8 = thisQuickie.Guid;
                        nquickiev++;
                        mQuickStackv8.Hit += (sender, obj) => mQuickStackv8_Hit(sender, obj);
                        break;
                    case 9:
                        mQuickStackv9 = mQuickStacks;
                        quickiesvTabFixedLayout.AddControl(mQuickStackv9, new Rectangle(2, 213, 25, 25));
                        nQuickieIDv9 = thisQuickie.Guid;
                        nquickiev++;
                        mQuickStackv9.Hit += (sender, obj) => mQuickStackv9_Hit(sender, obj);
                        break;

                    case 10:
                        mQuickStackv10 = mQuickStacks;
                        quickiesvTabFixedLayout.AddControl(mQuickStackv10, new Rectangle(2, 234, 25, 25));
                        nQuickieIDv10 = thisQuickie.Guid;
                        nquickiev++;
                        mQuickStackv10.Hit += (sender, obj) => mQuickStackv10_Hit(sender, obj);
                        break;
                    case 11:
                        mQuickStackv11 = mQuickStacks;
                        quickiesvTabFixedLayout.AddControl(mQuickStackv11, new Rectangle(5, 256, 25, 25));
                        nQuickieIDv11 = thisQuickie.Guid;
                        nquickiev++;
                        mQuickStackv11.Hit += (sender, obj) => mQuickStackv11_Hit(sender, obj);
                        break;
                    case 12:
                        mQuickStackv12 = mQuickStacks;
                        quickiesvTabFixedLayout.AddControl(mQuickStackv12, new Rectangle(2, 278, 25, 25));
                        nQuickieIDv12 = thisQuickie.Guid;
                        nquickiev++;
                        mQuickStackv12.Hit += (sender, obj) => mQuickStackv12_Hit(sender, obj);
                        break;

                    case 13:
                        mQuickStackv13 = mQuickStacks;
                        quickiesvTabFixedLayout.AddControl(mQuickStackv13, new Rectangle(2, 300, 25, 25));
                        nQuickieIDv13 = thisQuickie.Guid;
                        nquickiev++;
                        mQuickStackv13.Hit += (sender, obj) => mQuickStackv13_Hit(sender, obj);
                        break;
                    case 14:
                        mQuickStackv14 = mQuickStacks;
                        quickiesvTabFixedLayout.AddControl(mQuickStackv14, new Rectangle(2, 322, 25, 25));
                        nQuickieIDv14 = thisQuickie.Guid;
                        nquickiev++;
                        mQuickStackv14.Hit += (sender, obj) => mQuickStackv14_Hit(sender, obj);
                        break;
                    default:
                            GearFoundry.PluginCore.WriteToChat("There are no more slots available.");
                        break;

                }
            }
            else if (xdoc == xdocQuickSlotsh)
            {
                try
                {
                    switch (nquickieh)
                    {
                        case 0:
                            mQuickStackh0 = mQuickStacks;
                            quickieshTabFixedLayout.AddControl(mQuickStackh0, new Rectangle(30, 0, 25, 30));
                            nQuickieIDh0 = thisQuickie.Guid;
                            nquickieh++;
                            mQuickStackh0.Hit += (sender, obj) => mQuickStackh0_Hit(sender, obj);
                            break;
                        case 1:
                            mQuickStackh1 = mQuickStacks;
                            quickieshTabFixedLayout.AddControl(mQuickStackh1, new Rectangle(52, 0, 25, 30));
                            nQuickieIDh1 = thisQuickie.Guid;
                            nquickieh++;
                            mQuickStackh1.Hit += (sender, obj) => mQuickStackh1_Hit(sender, obj);
                            break;
                        case 2:
                            mQuickStackh2 = mQuickStacks;
                            quickieshTabFixedLayout.AddControl(mQuickStackh2, new Rectangle(74, 0, 25, 25));
                            nQuickieIDh2 = thisQuickie.Guid;
                            nquickieh++;
                            mQuickStackh2.Hit += (sender, obj) => mQuickStackh2_Hit(sender, obj);
                            break;
                        case 3:
                            mQuickStackh3 = mQuickStacks;
                            quickieshTabFixedLayout.AddControl(mQuickStackh3, new Rectangle(96, 0, 25, 25));
                            nQuickieIDh3 = thisQuickie.Guid;
                            mQuickStackh3.Hit += (sender, obj) => mQuickStackh3_Hit(sender, obj);
                            nquickieh++;
                            break;
                        case 4:
                            mQuickStackh4 = mQuickStacks;
                            quickieshTabFixedLayout.AddControl(mQuickStackh4, new Rectangle(118, 0, 25, 25));
                            nQuickieIDh4 = thisQuickie.Guid;
                            nquickieh++;
                            mQuickStackh4.Hit += (sender, obj) => mQuickStackh4_Hit(sender, obj);
                            break;
                        case 5:
                            mQuickStackh5 = mQuickStacks;
                            quickieshTabFixedLayout.AddControl(mQuickStackh5, new Rectangle(140, 0, 25, 25));
                            nQuickieIDh5 = thisQuickie.Guid;
                            nquickieh++;
                            mQuickStackh5.Hit += (sender, obj) => mQuickStackh5_Hit(sender, obj);
                            break;
                        case 6:
                            mQuickStackh6 = mQuickStacks;
                            quickieshTabFixedLayout.AddControl(mQuickStackh6, new Rectangle(162, 0, 25, 25));
                            nQuickieIDh6 = thisQuickie.Guid;
                            nquickieh++;
                            mQuickStackh6.Hit += (sender, obj) => mQuickStackh6_Hit(sender, obj);
                            break;
                        case 7:
                            mQuickStackh7 = mQuickStacks;
                            quickieshTabFixedLayout.AddControl(mQuickStackh7, new Rectangle(184, 0, 25, 25));
                            nQuickieIDh7 = thisQuickie.Guid;
                            nquickieh++;
                            mQuickStackh7.Hit += (sender, obj) => mQuickStackh7_Hit(sender, obj);
                            break;
                        case 8:
                            mQuickStackh8 = mQuickStacks;
                            quickieshTabFixedLayout.AddControl(mQuickStackh8, new Rectangle(206, 0, 25, 25));
                            nQuickieIDh8 = thisQuickie.Guid;
                            nquickieh++;
                            mQuickStackh8.Hit += (sender, obj) => mQuickStackh8_Hit(sender, obj);
                            break;
                        case 9:
                            mQuickStackh9 = mQuickStacks;
                            quickieshTabFixedLayout.AddControl(mQuickStackh9, new Rectangle(228, 0, 25, 25));
                            nQuickieIDh9 = thisQuickie.Guid;
                            nquickieh++;
                            mQuickStackh9.Hit += (sender, obj) => mQuickStackh9_Hit(sender, obj);
                            break;

                        case 10:
                            mQuickStackh10 = mQuickStacks;
                            quickieshTabFixedLayout.AddControl(mQuickStackh10, new Rectangle(250, 0, 25, 25));
                            nQuickieIDh10 = thisQuickie.Guid;
                            mQuickStackh10.Hit += (sender, obj) => mQuickStackh10_Hit(sender, obj);
                            nquickieh++;
                            break;
                        case 11:
                            mQuickStackh11 = mQuickStacks;
                            quickieshTabFixedLayout.AddControl(mQuickStackh11, new Rectangle(272, 0, 25, 25));
                            nQuickieIDh11 = thisQuickie.Guid;
                            nquickieh++;
                            mQuickStackh11.Hit += (sender, obj) => mQuickStackh11_Hit(sender, obj);
                            break;
                        case 12:
                            mQuickStackh12 = mQuickStacks;
                            quickieshTabFixedLayout.AddControl(mQuickStackh12, new Rectangle(294, 0, 25, 25));
                            nQuickieIDh12 = thisQuickie.Guid;
                            nquickieh++;
                            mQuickStackh12.Hit += (sender, obj) => mQuickStackh12_Hit(sender, obj);
                            break;

                        case 13:
                            mQuickStackh13 = mQuickStacks;
                            quickieshTabFixedLayout.AddControl(mQuickStackh13, new Rectangle(316, 0, 25, 25));
                            nQuickieIDh13 = thisQuickie.Guid;
                            nquickieh++;
                            mQuickStackh13.Hit += (sender, obj) => mQuickStackh13_Hit(sender, obj);
                            break;
                        case 14:
                            mQuickStackh14 = mQuickStacks;
                            quickieshTabFixedLayout.AddControl(mQuickStackh14, new Rectangle(338, 0, 25, 25));
                            nQuickieIDh14 = thisQuickie.Guid;
                            nquickieh++;
                            mQuickStackh14.Hit += (sender, obj) => mQuickStackh14_Hit(sender, obj);
                            break;

                        default:
                            GearFoundry.PluginCore.WriteToChat("There are no more slots available.");
                            break;

                    }
                }
                catch (Exception ex) { LogError(ex); }
            }
        }


        private void writeToQuickSlots(XDocument xdoc, string filename, QuickSlotData thisQuickie)
        {
            xdoc.Element("Objs").Add(new XElement("Obj",
                new XElement("QID", thisQuickie.Guid),
                new XElement("QIcon", thisQuickie.Icon),
                new XElement("QIconOverlay", thisQuickie.IconOverlay),
                new XElement("QIconUnderlay", thisQuickie.IconUnderlay)));
            xdoc.Save(filename);
        }



 
    }
}//end of namespace


