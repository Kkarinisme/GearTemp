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

        //private static Point vpt = new Point();
        //private static Point hpt = new Point();

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


        private HudCheckBox chkQuickiev0 = null;
        private HudCheckBox chkQuickiev1 = null;
        private HudCheckBox chkQuickiev2 = null;
        private HudCheckBox chkQuickiev3 = null;
        private HudCheckBox chkQuickiev4 = null;
        private HudCheckBox chkQuickiev5 = null;
        private HudCheckBox chkQuickiev6 = null;
        private HudCheckBox chkQuickiev7 = null;
        private HudCheckBox chkQuickiev8 = null;
        private HudCheckBox chkQuickiev9 = null;
        private HudCheckBox chkQuickiev10 = null;
        private HudCheckBox chkQuickiev11 = null;
        private HudCheckBox chkQuickiev12 = null;
        private HudCheckBox chkQuickiev13 = null;
        private HudCheckBox chkQuickiev14 = null;



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

        private HudCheckBox chkQuickieh0 = null;
        private HudCheckBox chkQuickieh1 = null;
        private HudCheckBox chkQuickieh2 = null;
        private HudCheckBox chkQuickieh3 = null;
        private HudCheckBox chkQuickieh4 = null;
        private HudCheckBox chkQuickieh5 = null;
        private HudCheckBox chkQuickieh6 = null;
        private HudCheckBox chkQuickieh7 = null;
        private HudCheckBox chkQuickieh8 = null;
        private HudCheckBox chkQuickieh9 = null;
        private HudCheckBox chkQuickieh10 = null;
        private HudCheckBox chkQuickieh11 = null;
        private HudCheckBox chkQuickieh12 = null;
        private HudCheckBox chkQuickieh13 = null;
        private HudCheckBox chkQuickieh14 = null;

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
       

        List<HudCheckBox> vchk = new List<HudCheckBox>();
        List<HudCheckBox> hchk = new List<HudCheckBox>();
        List<Int32> vID = new List<Int32>();
        List<Int32> hID = new List<Int32>();
        List<HudImageStack> vst = new List<HudImageStack>();
        List<HudImageStack> hst = new List<HudImageStack>();



        //private VirindiViewService.Controls.HudButton btnQuickiesAdd = new HudButton();
        //private VirindiViewService.Controls.HudButton btnQuickiesRemove = new HudButton();


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

            if (vpt.X == 0) { vpt.X = 200; }
            if (vpt.Y == 0) { vpt.Y = 200; }

            quickiesvHud = new VirindiViewService.HudView("", 30, 340, new ACImage(Color.Transparent));
            quickiesvHud.ShowInBar = false;
            quickiesvHud.UserAlphaChangeable = false;
            quickiesvHud.Visible = true;
            quickiesvHud.UserGhostable = true;
            quickiesvHud.UserMinimizable = false;
            quickiesvHud.UserResizeable = false;
            quickiesvHud.Location = vpt;
            if (mvtheme == null)
                {mvtheme = HudViewDrawStyle.GetThemeByName("Minimalist Transparent");}
            quickiesvHud.Theme = mvtheme;
            quickiesvHud_Head = new HudFixedLayout();
            quickiesvHud.Controls.HeadControl = quickiesvHud_Head;

            quickiesvTabView = new HudTabView();
            quickiesvTabFixedLayout = new HudFixedLayout();

            quickiesvHud_Head.AddControl(quickiesvTabView, new Rectangle(0,0,30,300));
            quickiesvTabView.AddTab(quickiesvTabFixedLayout,null);


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


            chkQuickiev0 = new HudCheckBox();
            chkQuickiev1 = new HudCheckBox();
            chkQuickiev2 = new HudCheckBox();
            chkQuickiev3 = new HudCheckBox();
            chkQuickiev4 = new HudCheckBox();
            chkQuickiev5 = new HudCheckBox();
            chkQuickiev6 = new HudCheckBox();
            chkQuickiev7 = new HudCheckBox();
            chkQuickiev8 = new HudCheckBox();
            chkQuickiev9 = new HudCheckBox();
            chkQuickiev10 = new HudCheckBox();
            chkQuickiev11 = new HudCheckBox();
            chkQuickiev12 = new HudCheckBox();
            chkQuickiev13 = new HudCheckBox();
            chkQuickiev14 = new HudCheckBox();

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

            chkQuickiev0 = new HudCheckBox(); vchk.Add(chkQuickiev0);
            chkQuickiev1 = new HudCheckBox(); vchk.Add(chkQuickiev1);
            chkQuickiev2 = new HudCheckBox(); vchk.Add(chkQuickiev2);
            chkQuickiev3 = new HudCheckBox(); vchk.Add(chkQuickiev3);
            chkQuickiev4 = new HudCheckBox(); vchk.Add(chkQuickiev4);
            chkQuickiev5 = new HudCheckBox(); vchk.Add(chkQuickiev5);
            chkQuickiev6 = new HudCheckBox(); vchk.Add(chkQuickiev6);
            chkQuickiev7 = new HudCheckBox(); vchk.Add(chkQuickiev7);
            chkQuickiev8 = new HudCheckBox(); vchk.Add(chkQuickiev8);
            chkQuickiev9 = new HudCheckBox(); vchk.Add(chkQuickiev9);
            chkQuickiev10 = new HudCheckBox(); vchk.Add(chkQuickiev10);
            chkQuickiev11 = new HudCheckBox(); vchk.Add(chkQuickiev11);
            chkQuickiev12= new HudCheckBox(); vchk.Add(chkQuickiev12);
            chkQuickiev13 = new HudCheckBox(); vchk.Add(chkQuickiev13);
            chkQuickiev14 = new HudCheckBox(); vchk.Add(chkQuickiev14);

            quickiesvHud_Head.AddControl(chkQuickiev0, new Rectangle(0, 15, 10, 20));
            quickiesvHud_Head.AddControl(chkQuickiev1, new Rectangle(0, 35, 10, 20));
            quickiesvHud_Head.AddControl(chkQuickiev2, new Rectangle(0, 55, 10, 20));
            quickiesvHud_Head.AddControl(chkQuickiev3, new Rectangle(0, 75, 10, 20));
            quickiesvHud_Head.AddControl(chkQuickiev4, new Rectangle(0, 95, 10, 20));
            quickiesvHud_Head.AddControl(chkQuickiev5, new Rectangle(0, 115, 10, 20));
            quickiesvHud_Head.AddControl(chkQuickiev6, new Rectangle(0, 135, 10, 20));
            quickiesvHud_Head.AddControl(chkQuickiev7, new Rectangle(0, 150, 10, 20));
            quickiesvHud_Head.AddControl(chkQuickiev8, new Rectangle(0, 175, 10, 20));
            quickiesvHud_Head.AddControl(chkQuickiev9, new Rectangle(0, 195, 10, 20));
            quickiesvHud_Head.AddControl(chkQuickiev10, new Rectangle(0, 215, 10, 20));
            quickiesvHud_Head.AddControl(chkQuickiev11, new Rectangle(0, 235, 10, 20));
            quickiesvHud_Head.AddControl(chkQuickiev12, new Rectangle(0, 255, 10, 20));
            quickiesvHud_Head.AddControl(chkQuickiev13, new Rectangle(0, 275, 10, 20));
            quickiesvHud_Head.AddControl(chkQuickiev14, new Rectangle(0, 295, 10, 20));

            mQuickStackv0.Hit += (sender,obj) => mQuickStackv0_Hit(sender,obj);
            quickiesvHud.ThemeChanged += (sender, obj) => quickiesvHud_ThemeChanged(sender, obj);
            quickiesvHud.Moved += (sender, obj) => quickiesvHud_Moved(sender, obj);
            btnQuickiesvAdd.Hit += (sender, obj) => btnQuickiesvAdd_Hit(sender, obj);
            btnQuickiesvRemove.Hit += (sender, obj) => btnQuickiesvRemove_Hit(sender, obj);
           chkQuickiev0.Change += (sender, obj) => chkQuickiev0_Change(sender, obj);
            chkQuickiev1.Change += (sender, obj) => chkQuickiev1_Change(sender, obj);
            chkQuickiev2.Change += (sender, obj) => chkQuickiev2_Change(sender, obj);
            chkQuickiev3.Change += (sender, obj) => chkQuickiev3_Change(sender, obj);
            chkQuickiev4.Change += (sender, obj) => chkQuickiev4_Change(sender, obj);
            chkQuickiev5.Change += (sender, obj) => chkQuickiev5_Change(sender, obj);
            chkQuickiev6.Change += (sender, obj) => chkQuickiev6_Change(sender, obj);
            chkQuickiev7.Change += (sender, obj) => chkQuickiev7_Change(sender, obj);
            chkQuickiev8.Change += (sender, obj) => chkQuickiev8_Change(sender, obj);
            chkQuickiev9.Change += (sender, obj) => chkQuickiev9_Change(sender, obj);
            chkQuickiev10.Change += (sender, obj) => chkQuickiev10_Change(sender, obj);
            chkQuickiev11.Change += (sender, obj) => chkQuickiev11_Change(sender, obj);
            chkQuickiev12.Change += (sender, obj) => chkQuickiev12_Change(sender, obj);
            chkQuickiev13.Change += (sender, obj) => chkQuickiev13_Change(sender, obj);
            chkQuickiev14.Change += (sender, obj) => chkQuickiev14_Change(sender, obj);
            mQuickStackv0.Hit += (sender, obj) => mQuickStackv0_Hit(sender, obj);


            if (xdocQuickSlotsv.Root.HasElements)
            {
                doGetData(xdocQuickSlotsv, quickSlotsvFilename);
            }
  
        }

        private void DisposeVerticalQuickSlots()
        {
            quickiesvHud.ThemeChanged -= (sender, obj) => quickiesvHud_ThemeChanged(sender, obj);
            quickiesvHud.Moved -= (sender, obj) => quickiesvHud_Moved(sender, obj);
            btnQuickiesvAdd.Hit -= (sender, obj) => btnQuickiesvAdd_Hit(sender, obj);
            btnQuickiesvRemove.Hit -= (sender, obj) => btnQuickiesvRemove_Hit(sender, obj);
            chkQuickiev0.Change -= (sender, obj) => chkQuickiev0_Change(sender, obj);
            chkQuickiev1.Change -= (sender, obj) => chkQuickiev1_Change(sender, obj);
            chkQuickiev2.Change -= (sender, obj) => chkQuickiev2_Change(sender, obj);
            chkQuickiev3.Change -= (sender, obj) => chkQuickiev3_Change(sender, obj);
            chkQuickiev4.Change -= (sender, obj) => chkQuickiev4_Change(sender, obj);
            chkQuickiev5.Change -= (sender, obj) => chkQuickiev5_Change(sender, obj);
            chkQuickiev6.Change -= (sender, obj) => chkQuickiev6_Change(sender, obj);
            chkQuickiev7.Change -= (sender, obj) => chkQuickiev7_Change(sender, obj);
            chkQuickiev8.Change -= (sender, obj) => chkQuickiev8_Change(sender, obj);
            chkQuickiev9.Change -= (sender, obj) => chkQuickiev9_Change(sender, obj);
            chkQuickiev10.Change -= (sender, obj) => chkQuickiev10_Change(sender, obj);
            chkQuickiev11.Change -= (sender, obj) => chkQuickiev11_Change(sender, obj);
            chkQuickiev12.Change -= (sender, obj) => chkQuickiev12_Change(sender, obj);
            chkQuickiev13.Change -= (sender, obj) => chkQuickiev13_Change(sender, obj);
            chkQuickiev14.Change -= (sender, obj) => chkQuickiev14_Change(sender, obj);

            chkQuickiev0.Dispose();
            chkQuickiev1.Dispose();
            chkQuickiev2.Dispose();
            chkQuickiev3.Dispose();
            chkQuickiev4.Dispose();
            chkQuickiev5.Dispose();
            chkQuickiev6.Dispose();
            chkQuickiev7.Dispose();
            chkQuickiev8.Dispose();
            chkQuickiev9.Dispose();
            chkQuickiev10.Dispose();
            chkQuickiev11.Dispose();
            chkQuickiev12.Dispose();
            chkQuickiev13.Dispose();
            chkQuickiev14.Dispose();


            mQuickStackv0.Dispose();
            mQuickStackv1.Dispose();
            mQuickStackv2.Dispose();
            mQuickStackv3.Dispose();
            mQuickStackv4.Dispose();
            mQuickStackv5.Dispose();
            mQuickStackv6.Dispose();
            mQuickStackv7.Dispose();
            mQuickStackv8.Dispose();
            mQuickStackv9.Dispose();
            mQuickStackv10.Dispose();
            mQuickStackv11.Dispose();
            mQuickStackv12.Dispose();
            mQuickStackv13.Dispose();
            mQuickStackv14.Dispose();

            btnQuickiesvAdd.Dispose();
            btnQuickiesvRemove.Dispose();
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


            for (int i = 0; i < 15; i++)
            { vchk[i] = null; }


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

                if (hpt.X == 0) { hpt.X = 200; }
                if (hpt.Y == 0) { hpt.Y = 200; }


            quickieshHud = new VirindiViewService.HudView("", 340, 30, new ACImage(Color.Transparent));
            quickieshHud.ShowInBar = false;
            quickieshHud.UserAlphaChangeable = false;
            quickieshHud.Visible = true;
            quickieshHud.UserGhostable = true;
            quickieshHud.UserMinimizable = false;
            quickieshHud.UserResizeable = false;
            quickieshHud.Location = hpt;
            //if (mhtheme == null)
            //    mhtheme = HudViewDrawStyle.GetThemeByName("Minimalist Transparent");
            //quickieshHud.Theme = mhtheme;
 

            //  quickieshHud.Theme = HudViewDrawStyle.GetThemeByName("Minimalist Transparent");

            quickieshHud_Head = new HudFixedLayout();
            quickieshHud.Controls.HeadControl = quickieshHud_Head;

            btnQuickieshAdd = new VirindiViewService.Controls.HudButton();
            btnQuickieshAdd.Text = "+";
            btnQuickieshAdd.Visible = true;

            btnQuickieshRemove = new VirindiViewService.Controls.HudButton();
            btnQuickieshRemove.Text = "-";
            btnQuickieshRemove.Visible = true;

            quickieshHud_Head.AddControl(btnQuickieshAdd, new Rectangle(0, 0, 12, 12));
            quickieshHud_Head.AddControl(btnQuickieshRemove, new Rectangle(15, 0, 12, 12));


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


            chkQuickieh0 = new HudCheckBox();
            chkQuickieh1 = new HudCheckBox();
            chkQuickieh2 = new HudCheckBox();
            chkQuickieh3 = new HudCheckBox();
            chkQuickieh4 = new HudCheckBox();
            chkQuickieh5 = new HudCheckBox();
            chkQuickieh6 = new HudCheckBox();
            chkQuickieh7 = new HudCheckBox();
            chkQuickieh8 = new HudCheckBox();
            chkQuickieh9 = new HudCheckBox();
            chkQuickieh10 = new HudCheckBox();
            chkQuickieh11 = new HudCheckBox();
            chkQuickieh12 = new HudCheckBox();
            chkQuickieh13 = new HudCheckBox();
            chkQuickieh14 = new HudCheckBox();

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

            chkQuickieh0 = new HudCheckBox(); hchk.Add(chkQuickieh0);
            chkQuickieh1 = new HudCheckBox(); hchk.Add(chkQuickieh1);
            chkQuickieh2 = new HudCheckBox(); hchk.Add(chkQuickieh2);
            chkQuickieh3 = new HudCheckBox(); hchk.Add(chkQuickieh3);
            chkQuickieh4 = new HudCheckBox(); hchk.Add(chkQuickieh4);
            chkQuickieh5 = new HudCheckBox(); hchk.Add(chkQuickieh5);
            chkQuickieh6 = new HudCheckBox(); hchk.Add(chkQuickieh6);
            chkQuickieh7 = new HudCheckBox(); hchk.Add(chkQuickieh7);
            chkQuickieh8 = new HudCheckBox(); hchk.Add(chkQuickieh8);
            chkQuickieh9 = new HudCheckBox(); hchk.Add(chkQuickieh9);
            chkQuickieh10 = new HudCheckBox(); hchk.Add(chkQuickieh10);
            chkQuickieh11 = new HudCheckBox(); hchk.Add(chkQuickieh11);
            chkQuickieh12= new HudCheckBox(); hchk.Add(chkQuickieh12);
            chkQuickieh13 = new HudCheckBox(); hchk.Add(chkQuickieh13);
            chkQuickieh14 = new HudCheckBox(); hchk.Add(chkQuickieh14);

            quickieshHud_Head.AddControl(chkQuickieh0, new Rectangle(30, 20, 20, 10));
            quickieshHud_Head.AddControl(chkQuickieh1, new Rectangle(50, 20, 20, 10));
            quickieshHud_Head.AddControl(chkQuickieh2, new Rectangle(70, 20, 20, 10));
            quickieshHud_Head.AddControl(chkQuickieh3, new Rectangle(90, 20, 20, 10));
            quickieshHud_Head.AddControl(chkQuickieh4, new Rectangle(110, 20, 20, 10));
            quickieshHud_Head.AddControl(chkQuickieh5, new Rectangle(130, 20, 20, 10));
            quickieshHud_Head.AddControl(chkQuickieh6, new Rectangle(150, 20, 20, 10));
            quickieshHud_Head.AddControl(chkQuickieh7, new Rectangle(170, 20, 20, 10));
            quickieshHud_Head.AddControl(chkQuickieh8, new Rectangle(190, 20, 20, 10));
            quickieshHud_Head.AddControl(chkQuickieh9, new Rectangle(210, 20, 20, 10));
            quickieshHud_Head.AddControl(chkQuickieh10, new Rectangle(230, 20, 20, 10));
            quickieshHud_Head.AddControl(chkQuickieh11, new Rectangle(250, 20, 20, 10));
            quickieshHud_Head.AddControl(chkQuickieh12, new Rectangle(270, 20, 20, 10));
            quickieshHud_Head.AddControl(chkQuickieh13, new Rectangle(290, 20, 20, 10));
            quickieshHud_Head.AddControl(chkQuickieh14, new Rectangle(310, 20, 20, 10));

            quickieshHud.ThemeChanged += (sender, obj) => quickieshHud_ThemeChanged(sender, obj);
            quickieshHud.Moved += (sender, obj) => quickieshHud_Moved(sender, obj);
            btnQuickieshAdd.Hit += (sender, obj) => btnQuickieshAdd_Hit(sender, obj);
            btnQuickieshRemove.Hit += (sender, obj) => btnQuickieshRemove_Hit(sender, obj);
            chkQuickieh0.Change += (sender, obj) => chkQuickieh0_Change(sender, obj);
            chkQuickieh1.Change += (sender, obj) => chkQuickieh1_Change(sender, obj);
            chkQuickieh2.Change += (sender, obj) => chkQuickieh2_Change(sender, obj);
            chkQuickieh3.Change += (sender, obj) => chkQuickieh3_Change(sender, obj);
            chkQuickieh4.Change += (sender, obj) => chkQuickieh4_Change(sender, obj);
            chkQuickieh5.Change += (sender, obj) => chkQuickieh5_Change(sender, obj);
            chkQuickieh6.Change += (sender, obj) => chkQuickieh6_Change(sender, obj);
            chkQuickieh7.Change += (sender, obj) => chkQuickieh7_Change(sender, obj);
            chkQuickieh8.Change += (sender, obj) => chkQuickieh8_Change(sender, obj);
            chkQuickieh9.Change += (sender, obj) => chkQuickieh9_Change(sender, obj);
            chkQuickieh10.Change += (sender, obj) => chkQuickieh10_Change(sender, obj);
            chkQuickieh11.Change += (sender, obj) => chkQuickieh11_Change(sender, obj);
            chkQuickieh12.Change += (sender, obj) => chkQuickieh12_Change(sender, obj);
            chkQuickieh13.Change += (sender, obj) => chkQuickieh13_Change(sender, obj);
            chkQuickieh14.Change += (sender, obj) => chkQuickieh14_Change(sender, obj);

            if (xdocQuickSlotsh.Root.HasElements)
            {
                doGetData(xdocQuickSlotsh, quickSlotshFilename);
            }
  
        }

        private void DisposeHorizontalQuickSlots()
        {

            quickieshHud.ThemeChanged -= (sender, obj) => quickieshHud_ThemeChanged(sender, obj);
            quickieshHud.Moved -= (sender, obj) => quickieshHud_Moved(sender, obj);
            btnQuickieshAdd.Hit -= (sender, obj) => btnQuickieshAdd_Hit(sender, obj);
            btnQuickieshRemove.Hit -= (sender, obj) => btnQuickieshRemove_Hit(sender, obj);
            chkQuickieh0.Change -= (sender, obj) => chkQuickieh0_Change(sender, obj);
            chkQuickieh1.Change -= (sender, obj) => chkQuickieh1_Change(sender, obj);
            chkQuickieh2.Change -= (sender, obj) => chkQuickieh2_Change(sender, obj);
            chkQuickieh3.Change -= (sender, obj) => chkQuickieh3_Change(sender, obj);
            chkQuickieh4.Change -= (sender, obj) => chkQuickieh4_Change(sender, obj);
            chkQuickieh5.Change -= (sender, obj) => chkQuickieh5_Change(sender, obj);
            chkQuickieh6.Change -= (sender, obj) => chkQuickieh6_Change(sender, obj);
            chkQuickieh7.Change -= (sender, obj) => chkQuickieh7_Change(sender, obj);
            chkQuickieh8.Change -= (sender, obj) => chkQuickieh8_Change(sender, obj);
            chkQuickieh9.Change -= (sender, obj) => chkQuickieh9_Change(sender, obj);
            chkQuickieh10.Change -= (sender, obj) => chkQuickieh10_Change(sender, obj);
            chkQuickieh11.Change -= (sender, obj) => chkQuickieh11_Change(sender, obj);
            chkQuickieh12.Change -= (sender, obj) => chkQuickieh12_Change(sender, obj);
            chkQuickieh13.Change -= (sender, obj) => chkQuickieh13_Change(sender, obj);
            chkQuickieh14.Change -= (sender, obj) => chkQuickieh14_Change(sender, obj);


            chkQuickieh0.Dispose();
            chkQuickieh1.Dispose();
            chkQuickieh2.Dispose();
            chkQuickieh3.Dispose();
            chkQuickieh4.Dispose();
            chkQuickieh5.Dispose();
            chkQuickieh6.Dispose();
            chkQuickieh7.Dispose();
            chkQuickieh8.Dispose();
            chkQuickieh9.Dispose();
            chkQuickieh10.Dispose();
            chkQuickieh11.Dispose();
            chkQuickieh12.Dispose();
            chkQuickieh13.Dispose();
            chkQuickieh14.Dispose();

            mQuickStackh0.Dispose();
            mQuickStackh1.Dispose();
            mQuickStackh2.Dispose();
            mQuickStackh3.Dispose();
            mQuickStackh4.Dispose();
            mQuickStackh5.Dispose();
            mQuickStackh6.Dispose();
            mQuickStackh7.Dispose();
            mQuickStackh8.Dispose();
            mQuickStackh9.Dispose();
            mQuickStackh10.Dispose();
            mQuickStackh11.Dispose();
            mQuickStackh12.Dispose();
            mQuickStackh13.Dispose();
            mQuickStackh14.Dispose();

            btnQuickieshAdd.Dispose();
            btnQuickieshRemove.Dispose();
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

              try{
                    for (int i = 0; i < 12; i++)
                    { hchk[i] = null; }
                    nquickieh = 0;
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

        private void chkQuickiev0_Change(object sender, System.EventArgs e)
        {
            if (chkQuickiev0.Checked && nQuickieIDv0 != 0)
            {
                doQuickieChkWork(nQuickieIDv0, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
                chkQuickiev0.Checked = false;
            }
        }
        private void mQuickStackv0_Hit(object sender, System.EventArgs e)
        {
                doQuickieChkWork(nQuickieIDv0, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
        }

        private void chkQuickiev1_Change(object sender, System.EventArgs e)
        {
            if (chkQuickiev1.Checked && nQuickieIDv1 != 0)
            {
                doQuickieChkWork(nQuickieIDv1, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
                chkQuickiev1.Checked = false;
            }

        }

        private void chkQuickiev2_Change(object sender, System.EventArgs e)
        {
            if (chkQuickiev2.Checked && nQuickieIDv2 != 0)
            {
                doQuickieChkWork(nQuickieIDv2, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
                chkQuickiev2.Checked = false;
            }

        }

        private void chkQuickiev3_Change(object sender, System.EventArgs e)
        {
            if (chkQuickiev3.Checked && nQuickieIDv3 != 0)
            {
                doQuickieChkWork(nQuickieIDv3, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
                chkQuickiev3.Checked = false;
            }

        }

        private void chkQuickiev4_Change(object sender, System.EventArgs e)
        {
            if (chkQuickiev4.Checked && nQuickieIDv4 != 0)
            {
                doQuickieChkWork(nQuickieIDv4, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
                chkQuickiev4.Checked = false;
            }

        }
        private void chkQuickiev5_Change(object sender, System.EventArgs e)
        {
            if (chkQuickiev5.Checked && nQuickieIDv5 != 0)
            {
                doQuickieChkWork(nQuickieIDv5, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
                chkQuickiev5.Checked = false;
            }

        }
        private void chkQuickiev6_Change(object sender, System.EventArgs e)
        {
            if (chkQuickiev6.Checked && nQuickieIDv6 != 0)
            {
                doQuickieChkWork(nQuickieIDv6, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
                chkQuickiev6.Checked = false;
            }

        }
        private void chkQuickiev7_Change(object sender, System.EventArgs e)
        {
            if (chkQuickiev7.Checked && nQuickieIDv7 != 0)
            {
                doQuickieChkWork(nQuickieIDv7, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
                chkQuickiev7.Checked = false;
            }
        }
        private void chkQuickiev8_Change(object sender, System.EventArgs e)
        {
            if (chkQuickiev8.Checked && nQuickieIDv8 != 0)
            {
                doQuickieChkWork(nQuickieIDv8, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
                chkQuickiev8.Checked = false;
            }

        }
        private void chkQuickiev9_Change(object sender, System.EventArgs e)
        {
            if (chkQuickiev9.Checked && nQuickieIDv9 != 0)
            {
                doQuickieChkWork(nQuickieIDv9, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
                chkQuickiev9.Checked = false;
            }

        }
        private void chkQuickiev10_Change(object sender, System.EventArgs e)
        {
            if (chkQuickiev10.Checked && nQuickieIDv10 != 0)
            {
                doQuickieChkWork(nQuickieIDv10, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
                chkQuickiev10.Checked = false;
            }

        }
        private void chkQuickiev11_Change(object sender, System.EventArgs e)
        {
            if (chkQuickiev11.Checked && nQuickieIDv11 != 0)
            {
                doQuickieChkWork(nQuickieIDv11, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
                chkQuickiev11.Checked = false;
            }

        }

        private void chkQuickiev12_Change(object sender, System.EventArgs e)
        {
            if (chkQuickiev12.Checked && nQuickieIDv12 != 0)
            {
                doQuickieChkWork(nQuickieIDv12, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
                chkQuickiev12.Checked = false;
            }

        }
        private void chkQuickiev13_Change(object sender, System.EventArgs e)
        {
            if (chkQuickiev13.Checked && nQuickieIDv13 != 0)
            {
                doQuickieChkWork(nQuickieIDv13, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
                chkQuickiev13.Checked = false;
            }

        }
        private void chkQuickiev14_Change(object sender, System.EventArgs e)
        {
            if (chkQuickiev14.Checked && nQuickieIDv14 != 0)
            {
                doQuickieChkWork(nQuickieIDv14, xdocQuickSlotsv, quickSlotsvFilename, nquickiev, quickiesvHud);
                chkQuickiev14.Checked = false;
            }

        }

        private void chkQuickieh0_Change(object sender, System.EventArgs e)
        {
            if (chkQuickieh0.Checked && nQuickieIDh0 != 0)
            {
                doQuickieChkWork(nQuickieIDh0, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
                chkQuickieh0.Checked = false;
            }
        }

        private void chkQuickieh1_Change(object sender, System.EventArgs e)
        {
            if (chkQuickieh1.Checked && nQuickieIDh1 != 0)
            {
                doQuickieChkWork(nQuickieIDh1, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
                chkQuickieh1.Checked = false;
            }

        }

        private void chkQuickieh2_Change(object sender, System.EventArgs e)
        {
            if (chkQuickieh2.Checked && nQuickieIDh2 != 0)
            {
                doQuickieChkWork(nQuickieIDh2, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
                chkQuickieh2.Checked = false;
            }

        }

        private void chkQuickieh3_Change(object sender, System.EventArgs e)
        {
            if (chkQuickieh3.Checked && nQuickieIDh3 != 0)
            {
                doQuickieChkWork(nQuickieIDh3, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
                chkQuickieh3.Checked = false;
            }

        }

        private void chkQuickieh4_Change(object sender, System.EventArgs e)
        {
            if (chkQuickieh4.Checked && nQuickieIDh4 != 0)
            {
                doQuickieChkWork(nQuickieIDh4, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
                chkQuickieh4.Checked = false;
            }

        }
        private void chkQuickieh5_Change(object sender, System.EventArgs e)
        {
            if (chkQuickieh5.Checked && nQuickieIDh5 != 0)
            {
                doQuickieChkWork(nQuickieIDh5, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
                chkQuickieh5.Checked = false;
            }

        }
        private void chkQuickieh6_Change(object sender, System.EventArgs e)
        {
            if (chkQuickieh6.Checked && nQuickieIDh6 != 0)
            {
                doQuickieChkWork(nQuickieIDh6, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
                chkQuickieh6.Checked = false;
            }

        }
        private void chkQuickieh7_Change(object sender, System.EventArgs e)
        {
            if (chkQuickieh7.Checked && nQuickieIDh7 != 0)
            {
                doQuickieChkWork(nQuickieIDh7, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
                chkQuickiev7.Checked = false;
            }
        }
        private void chkQuickieh8_Change(object sender, System.EventArgs e)
        {
            if (chkQuickieh8.Checked && nQuickieIDh8 != 0)
            {
                doQuickieChkWork(nQuickieIDh8, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
                chkQuickieh8.Checked = false;
            }

        }
        private void chkQuickieh9_Change(object sender, System.EventArgs e)
        {
            if (chkQuickieh9.Checked && nQuickieIDh9 != 0)
            {
                doQuickieChkWork(nQuickieIDh9, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
                chkQuickieh9.Checked = false;
            }

        }
        private void chkQuickieh10_Change(object sender, System.EventArgs e)
        {
            if (chkQuickieh10.Checked && nQuickieIDh10 != 0)
            {
                doQuickieChkWork(nQuickieIDh10, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
                chkQuickieh10.Checked = false;
            }

        }
        private void chkQuickieh11_Change(object sender, System.EventArgs e)
        {
            if (chkQuickieh11.Checked && nQuickieIDh11 != 0)
            {
                doQuickieChkWork(nQuickieIDh11, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
                chkQuickieh11.Checked = false;
            }

        }

        private void chkQuickieh12_Change(object sender, System.EventArgs e)
        {
            if (chkQuickieh12.Checked && nQuickieIDh12 != 0)
            {
                doQuickieChkWork(nQuickieIDh12, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
                chkQuickieh12.Checked = false;
            }

        }
        private void chkQuickieh13_Change(object sender, System.EventArgs e)
        {
            if (chkQuickieh13.Checked && nQuickieIDh13 != 0)
            {
                doQuickieChkWork(nQuickieIDh13, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
                chkQuickieh13.Checked = false;
            }

        }
        private void chkQuickieh14_Change(object sender, System.EventArgs e)
        {
            if (chkQuickieh14.Checked && nQuickieIDh14 != 0)
            {
                doQuickieChkWork(nQuickieIDh14, xdocQuickSlotsh, quickSlotshFilename, nquickieh, quickieshHud);
                chkQuickieh14.Checked = false;
            }

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
                        quickiesvTabFixedLayout.AddControl(mQuickStackv0, new Rectangle(0,15, 20, 20));
                        nQuickieIDv0 = thisQuickie.Guid;
                        nquickiev++;
                        
                        break;
                    case 1:
                        mQuickStackv1 = mQuickStacks;
                        quickiesvHud_Head.AddControl(mQuickStackv1, new Rectangle(10, 35, 20, 20));
                        nQuickieIDv1 = thisQuickie.Guid;
                        nquickiev++;
                        break;
                    case 2:
                        mQuickStackv2 = mQuickStacks;
                        quickiesvHud_Head.AddControl(mQuickStackv2, new Rectangle(10, 55, 20, 20));
                        nQuickieIDv2 = thisQuickie.Guid;
                        nquickiev++;
                        break;
                    case 3:
                        mQuickStackv3 = mQuickStacks;
                        quickiesvHud_Head.AddControl(mQuickStackv3, new Rectangle(10, 75, 20, 20));
                        nQuickieIDv3 = thisQuickie.Guid;
                        nquickiev++;
                        break;
                    case 4:
                        mQuickStackv4 = mQuickStacks;
                        quickiesvHud_Head.AddControl(mQuickStackv4, new Rectangle(10, 95, 20, 20));
                        nQuickieIDv4 = thisQuickie.Guid;
                        nquickiev++;
                        break;
                    case 5:
                        mQuickStackv5 = mQuickStacks;
                        quickiesvHud_Head.AddControl(mQuickStackv5, new Rectangle(10, 115, 20, 20));
                        nQuickieIDv5 = thisQuickie.Guid;
                        nquickiev++;
                        break;
                    case 6:
                        mQuickStackv6 = mQuickStacks;
                        quickiesvHud_Head.AddControl(mQuickStackv6, new Rectangle(10, 135, 20, 20));
                        nQuickieIDv6 = thisQuickie.Guid;
                        nquickiev++;
                        break;
                    case 7:
                        mQuickStackv7 = mQuickStacks;
                        quickiesvHud_Head.AddControl(mQuickStackv7, new Rectangle(10, 155, 20, 20));
                        nQuickieIDv7 = thisQuickie.Guid;
                        nquickiev++;
                        break;
                    case 8:
                        mQuickStackv8 = mQuickStacks;
                        quickiesvHud_Head.AddControl(mQuickStackv8, new Rectangle(10, 175, 20, 20));
                        nQuickieIDv8 = thisQuickie.Guid;
                        nquickiev++;
                        break;
                    case 9:
                        mQuickStackv9 = mQuickStacks;
                        quickiesvHud_Head.AddControl(mQuickStackv9, new Rectangle(10, 195, 20, 20));
                        nQuickieIDv9 = thisQuickie.Guid;
                        nquickiev++;
                        break;

                    case 10:
                        mQuickStackv10 = mQuickStacks;
                        quickiesvHud_Head.AddControl(mQuickStackv10, new Rectangle(10, 215, 20, 20));
                        nQuickieIDv10 = thisQuickie.Guid;
                        nquickiev++;
                        break;
                    case 11:
                        mQuickStackv11 = mQuickStacks;
                        quickiesvHud_Head.AddControl(mQuickStackv11, new Rectangle(10, 235, 20, 20));
                        nQuickieIDv11 = thisQuickie.Guid;
                        nquickiev++;
                        break;
                    case 12:
                        mQuickStackv12 = mQuickStacks;
                        quickiesvHud_Head.AddControl(mQuickStackv12, new Rectangle(10, 255, 20, 20));
                        nQuickieIDv12 = thisQuickie.Guid;
                        nquickiev++;
                        break;

                    case 13:
                        mQuickStackv13 = mQuickStacks;
                        quickiesvHud_Head.AddControl(mQuickStackv13, new Rectangle(10, 275, 20, 20));
                        nQuickieIDv13 = thisQuickie.Guid;
                        nquickiev++;
                        break;
                    case 14:
                        mQuickStackv14 = mQuickStacks;
                        quickiesvHud_Head.AddControl(mQuickStackv14, new Rectangle(10, 295, 20, 20));
                        nQuickieIDv14 = thisQuickie.Guid;
                        nquickiev++;
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
                            quickieshHud_Head.AddControl(mQuickStackh0, new Rectangle(30, 0, 20, 20));
                            nQuickieIDh0 = thisQuickie.Guid;
                            nquickieh++;
                            break;
                        case 1:
                            mQuickStackh1 = mQuickStacks;
                            quickieshHud_Head.AddControl(mQuickStackh1, new Rectangle(50, 0, 20, 20));
                            nQuickieIDh1 = thisQuickie.Guid;
                            nquickieh++;
                            break;
                        case 2:
                            mQuickStackh2 = mQuickStacks;
                            quickieshHud_Head.AddControl(mQuickStackh2, new Rectangle(70, 0, 20, 20));
                            nQuickieIDh2 = thisQuickie.Guid;
                            nquickieh++;
                            break;
                        case 3:
                            mQuickStackh3 = mQuickStacks;
                            quickieshHud_Head.AddControl(mQuickStackh3, new Rectangle(90, 0, 20, 20));
                            nQuickieIDh3 = thisQuickie.Guid;
                            nquickieh++;
                            break;
                        case 4:
                            mQuickStackh4 = mQuickStacks;
                            quickieshHud_Head.AddControl(mQuickStackh4, new Rectangle(110, 0, 20, 20));
                            nQuickieIDh4 = thisQuickie.Guid;
                            nquickieh++;
                            break;
                        case 5:
                            mQuickStackh5 = mQuickStacks;
                            quickieshHud_Head.AddControl(mQuickStackh5, new Rectangle(130, 0, 20, 20));
                            nQuickieIDh5 = thisQuickie.Guid;
                            nquickieh++;
                            break;
                        case 6:
                            mQuickStackh6 = mQuickStacks;
                            quickieshHud_Head.AddControl(mQuickStackh6, new Rectangle(150, 0, 20, 20));
                            nQuickieIDh6 = thisQuickie.Guid;
                            nquickieh++;
                            break;
                        case 7:
                            mQuickStackh7 = mQuickStacks;
                            quickieshHud_Head.AddControl(mQuickStackh7, new Rectangle(170, 0, 20, 20));
                            nQuickieIDh7 = thisQuickie.Guid;
                            nquickieh++;
                            break;
                        case 8:
                            mQuickStackh8 = mQuickStacks;
                            quickieshHud_Head.AddControl(mQuickStackh8, new Rectangle(190, 0, 20, 20));
                            nQuickieIDh8 = thisQuickie.Guid;
                            nquickieh++;
                            break;
                        case 9:
                            mQuickStackh9 = mQuickStacks;
                            quickieshHud_Head.AddControl(mQuickStackh9, new Rectangle(210, 0, 20, 20));
                            nQuickieIDh9 = thisQuickie.Guid;
                            nquickieh++;
                            break;

                        case 10:
                            mQuickStackv10 = mQuickStacks;
                            quickieshHud_Head.AddControl(mQuickStackh10, new Rectangle(230, 0, 20, 20));
                            nQuickieIDv10 = thisQuickie.Guid;
                            nquickieh++;
                            break;
                        case 11:
                            mQuickStackv11 = mQuickStacks;
                            quickieshHud_Head.AddControl(mQuickStackh11, new Rectangle(250, 0, 20, 20));
                            nQuickieIDh11 = thisQuickie.Guid;
                            nquickieh++;
                            break;
                        case 12:
                            mQuickStackh9 = mQuickStacks;
                            quickieshHud_Head.AddControl(mQuickStackh12, new Rectangle(270, 0, 20, 20));
                            nQuickieIDh12 = thisQuickie.Guid;
                            nquickieh++;
                            break;

                        case 13:
                            mQuickStackv13 = mQuickStacks;
                            quickieshHud_Head.AddControl(mQuickStackh13, new Rectangle(290, 0, 20, 20));
                            nQuickieIDv13 = thisQuickie.Guid;
                            nquickieh++;
                            break;
                        case 14:
                            mQuickStackv14 = mQuickStacks;
                            quickieshHud_Head.AddControl(mQuickStackh14, new Rectangle(310, 0, 20, 20));
                            nQuickieIDh14 = thisQuickie.Guid;
                            nquickieh++;
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



        private void quickiesvHud_Moved(object sender, System.EventArgs e)
        {
            try
            {

                vpt = quickiesvHud.Location;
                SaveSwitchGearSettings();

            }
            catch (Exception ex) { LogError(ex); }


        }

        private void quickiesvHud_ThemeChanged(object sender, System.EventArgs e)
        {
            try
            {

                mvtheme = quickiesvHud.Theme;
                SaveSwitchGearSettings();
            }
            catch (Exception ex) { LogError(ex); }


        }
        private void quickieshHud_Moved(object sender, System.EventArgs e)
        {
            try
            {
                hpt = quickieshHud.Location;
                SaveSwitchGearSettings();
            }
            catch (Exception ex) { LogError(ex); }


        }

        private void quickieshHud_ThemeChanged(object sender, System.EventArgs e)
        {
            try
            {

                mhtheme = quickieshHud.Theme;
                SaveSwitchGearSettings();
            }
            catch (Exception ex) { LogError(ex); }


        }
 
    }
}//end of namespace


