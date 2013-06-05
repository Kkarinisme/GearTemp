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
        XDocument xdocRemoteGear = null;
        int nRemoteGear = 0;

        private static VirindiViewService.HudView remoteGearHud = null;
        private static VirindiViewService.Controls.HudFixedLayout remoteGear_Head = null;
        private static VirindiViewService.Controls.HudTabView remoteGearTabView = null;
        private static VirindiViewService.Controls.HudFixedLayout remoteGearTabFixedLayout = null;

        private HudPictureBox mRemoteGear0 = null;
        private HudPictureBox mRemoteGear1 = null;
        private HudPictureBox mRemoteGear2 = null;
        private HudPictureBox mRemoteGear3 = null;
        private HudPictureBox mRemoteGear4 = null;
        private HudPictureBox mRemoteGear5 = null;
        private HudPictureBox mRemoteGear6 = null;
        private HudPictureBox mRemoteGear7 = null;
        private HudPictureBox mRemoteGear8 = null;
        private HudPictureBox mRemoteGear9 = null;


        int nRemoteGear0 = 0;
        int nRemoteGear1 = 0;
        int nRemoteGear2 = 0;
        int nRemoteGear3 = 0;
        int nRemoteGear4 = 0;
        int nRemoteGear5 = 0;
        int nRemoteGear6 = 0;
        int nRemoteGear7 = 0;
        int nRemoteGear8 = 0;
        int nRemoteGear9 = 0;


        List<Int32> remoteGearID = new List<Int32>();
        List<HudPictureBox> remoteGearPB = new List<HudPictureBox>();

        private void RenderRemoteGearHud()
        {

            if (remoteGearHud != null)
            {
                DisposeRemoteGearHud();
            }
            if (!File.Exists(remoteGearFilename))
            {
                XDocument tempDoc = new XDocument(new XElement("Huds"));
                tempDoc.Save(remoteGearFilename);
                tempDoc = null;
            }

            xdocRemoteGear = XDocument.Load(remoteGearFilename);

            remoteGearHud = new VirindiViewService.HudView("", 30, 360, new ACImage(Color.Transparent), false, "RemoteGear");
            remoteGearHud.ShowInBar = false;
            remoteGearHud.UserAlphaChangeable = false;
            remoteGearHud.Visible = true;
            remoteGearHud.UserClickThroughable = false;
            remoteGearHud.UserGhostable = true;
            remoteGearHud.UserMinimizable = false;
            remoteGearHud.UserResizeable = false;
            remoteGearHud.LoadUserSettings();
            remoteGear_Head = new HudFixedLayout();
            remoteGearHud.Controls.HeadControl = remoteGear_Head;
            remoteGearTabView = new HudTabView();
            remoteGearTabFixedLayout = new HudFixedLayout();

            remoteGear_Head.AddControl(remoteGearTabView, new Rectangle(0, 0, 29, 359));
            remoteGearTabView.AddTab(remoteGearTabFixedLayout, "");

 
            mRemoteGear0 = new HudPictureBox();
            mRemoteGear1 = new HudPictureBox();
            mRemoteGear2 = new HudPictureBox();
            mRemoteGear3 = new HudPictureBox();
            mRemoteGear4 = new HudPictureBox();
            mRemoteGear5 = new HudPictureBox();
            mRemoteGear6 = new HudPictureBox();
            mRemoteGear7 = new HudPictureBox();
            mRemoteGear8 = new HudPictureBox();
            mRemoteGear9 = new HudPictureBox();

            remoteGearPB.Add(mRemoteGear0);
            remoteGearPB.Add(mRemoteGear2);
            remoteGearPB.Add(mRemoteGear3);
            remoteGearPB.Add(mRemoteGear4);
            remoteGearPB.Add(mRemoteGear5);
            remoteGearPB.Add(mRemoteGear6);
            remoteGearPB.Add(mRemoteGear7);
            remoteGearPB.Add(mRemoteGear8);
            remoteGearPB.Add(mRemoteGear9);


            if (xdocRemoteGear.Root.HasElements)
            {
                doGetRemoteData(xdocRemoteGear, remoteGearFilename);
            }

        }

        private void DisposeRemoteGearHud()
        {

            if (mRemoteGear0 != null) { mRemoteGear0.Hit -= (sender, obj) => mRemoteGear0_Hit(sender, obj); mRemoteGear0.Dispose(); }
            if (mRemoteGear1 != null) { mRemoteGear1.Hit -= (sender, obj) => mRemoteGear1_Hit(sender, obj); mRemoteGear1.Dispose(); }
            if (mRemoteGear2 != null) { mRemoteGear2.Hit -= (sender, obj) => mRemoteGear2_Hit(sender, obj); mRemoteGear2.Dispose(); }
            if (mRemoteGear3 != null) { mRemoteGear3.Hit -= (sender, obj) => mRemoteGear3_Hit(sender, obj); mRemoteGear3.Dispose(); }
            if (mRemoteGear4 != null) { mRemoteGear4.Hit -= (sender, obj) => mRemoteGear4_Hit(sender, obj); mRemoteGear4.Dispose(); }
            if (mRemoteGear5 != null) { mRemoteGear5.Hit -= (sender, obj) => mRemoteGear5_Hit(sender, obj); mRemoteGear5.Dispose(); }
            if (mRemoteGear6 != null) { mRemoteGear6.Hit -= (sender, obj) => mRemoteGear6_Hit(sender, obj); mRemoteGear6.Dispose(); }
            if (mRemoteGear7 != null) { mRemoteGear7.Hit -= (sender, obj) => mRemoteGear7_Hit(sender, obj); mRemoteGear7.Dispose(); }
            if (mRemoteGear8 != null) { mRemoteGear8.Hit -= (sender, obj) => mRemoteGear8_Hit(sender, obj); mRemoteGear8.Dispose(); }
            if (mRemoteGear9 != null) { mRemoteGear9.Hit -= (sender, obj) => mRemoteGear9_Hit(sender, obj); mRemoteGear9.Dispose(); }

            remoteGear_Head.Dispose();
            remoteGearHud.Dispose();
            nRemoteGear = 0;
            try
            {

                for (int i = 0; i <  remoteGearPB.Count; i++)
                { remoteGearPB[i] = null; }
            }
            catch (Exception ex) { LogError(ex); }

        }

 



        private void doGetRemoteData(XDocument xdoc, string filename)
        {

            try
            {

                nRemoteGear = 0; 
                IEnumerable<XElement> elements = xdoc.Element("Huds").Descendants("Hud");

                foreach (XElement elem in elements)
                {
                    fillHud(xdoc, filename, thisQuickie);

                }
            }
            catch (Exception ex) { LogError(ex); }

        }



        private void mRemoteGear0_Hit(object sender, System.EventArgs e)
        {
            try
            {
            }
            catch (Exception ex) { LogError(ex); }

        }

        private void mRemoteGear1_Hit(object sender, System.EventArgs e)
        {
            try
            {
             }
            catch (Exception ex) { LogError(ex); }

        }
        private void mRemoteGear2_Hit(object sender, System.EventArgs e)
        {
            try
            {
            }
            catch (Exception ex) { LogError(ex); }

        }

        private void mRemoteGear3_Hit(object sender, System.EventArgs e)
        {
            try
            {
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mRemoteGear4_Hit(object sender, System.EventArgs e)
        {
            try
            {
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mRemoteGear5_Hit(object sender, System.EventArgs e)
        {
            try
            {
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mRemoteGear6_Hit(object sender, System.EventArgs e)
        {
            try
            {
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mRemoteGear7_Hit(object sender, System.EventArgs e)
        {
            try
            {
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mRemoteGear8_Hit(object sender, System.EventArgs e)
        {
            try
            {
            }
            catch (Exception ex) { LogError(ex); }

        }
        private void mRemoteGear9_Hit(object sender, System.EventArgs e)
        {
            try
            {
            }
            catch (Exception ex) { LogError(ex); }

        }


        private void fillHud(XDocument xdoc, string filename, ACImage thisImage)
        {
            HudPictureBox mRemoteGearPB = new HudPictureBox();
            try
            {
                    mRemoteGearPB.Image = thisImage;
              }
            catch (Exception ex) { LogError(ex); }
            try{
                switch (nRemoteGear)
                {
                    case 0:
                        mRemoteGear0 = mRemoteGearPB;
                        remoteGearTabFixedLayout.AddControl(mRemoteGear0, new Rectangle(2, 30, 25, 25));
                        nRemoteGear++;
                        mRemoteGear0.Hit += (sender, obj) => mRemoteGear0_Hit(sender, obj);
                        break;
                    case 1:
                        mRemoteGear1 = mRemoteGearPB;
                        remoteGearTabFixedLayout.AddControl(mRemoteGear1, new Rectangle(2, 30, 25, 25));
                        nRemoteGear++;
                        mRemoteGear1.Hit += (sender, obj) => mRemoteGear1_Hit(sender, obj);
                        break;
                    case 2:
                        mRemoteGear2 = mRemoteGearPB;
                        remoteGearTabFixedLayout.AddControl(mRemoteGear2, new Rectangle(2, 30, 25, 25));
                        nRemoteGear++;
                        mRemoteGear2.Hit += (sender, obj) => mRemoteGear2_Hit(sender, obj);
                        break;
                    case 3:
                        mRemoteGear3 = mRemoteGearPB;
                        remoteGearTabFixedLayout.AddControl(mRemoteGear3, new Rectangle(2, 30, 25, 25));
                        nRemoteGear++;
                        mRemoteGear3.Hit += (sender, obj) => mRemoteGear3_Hit(sender, obj);
                        break;
                    case 4:
                        mRemoteGear4 = mRemoteGearPB;
                        remoteGearTabFixedLayout.AddControl(mRemoteGear4, new Rectangle(2, 30, 25, 25));
                        nRemoteGear++;
                        mRemoteGear4.Hit += (sender, obj) => mRemoteGear4_Hit(sender, obj);
                        break;
                    case 5:
                        mRemoteGear5 = mRemoteGearPB;
                        remoteGearTabFixedLayout.AddControl(mRemoteGear5, new Rectangle(2, 30, 25, 25));
                        nRemoteGear++;
                        mRemoteGear5.Hit += (sender, obj) => mRemoteGear5_Hit(sender, obj);
                        break;
                    case 6:
                        mRemoteGear6 = mRemoteGearPB;
                        remoteGearTabFixedLayout.AddControl(mRemoteGear6, new Rectangle(2, 30, 25, 25));
                        nRemoteGear++;
                        mRemoteGear6.Hit += (sender, obj) => mRemoteGear6_Hit(sender, obj);
                        break;
                    case 7:
                        mRemoteGear7 = mRemoteGearPB;
                        remoteGearTabFixedLayout.AddControl(mRemoteGear7, new Rectangle(2, 30, 25, 25));
                        nRemoteGear++;
                        mRemoteGear7.Hit += (sender, obj) => mRemoteGear7_Hit(sender, obj);
                        break;
                    case 8:
                        mRemoteGear8 = mRemoteGearPB;
                        remoteGearTabFixedLayout.AddControl(mRemoteGear8, new Rectangle(2, 30, 25, 25));
                        nRemoteGear++;
                        mRemoteGear8.Hit += (sender, obj) => mRemoteGear8_Hit(sender, obj);
                        break;
                    case 9:
                        mRemoteGear9 = mRemoteGearPB;
                        remoteGearTabFixedLayout.AddControl(mRemoteGear9, new Rectangle(2, 30, 25, 25));
                        nRemoteGear++;
                        mRemoteGear9.Hit += (sender, obj) => mRemoteGear9_Hit(sender, obj);
                        break;
 

                    }
                }
                catch (Exception ex) { LogError(ex); }
            
        }


        //private void writeToRemoteGear(XDocument xdoc, string filename, QuickSlotData thisQuickie)
        //{
        //    xdoc.Element("Objs").Add(new XElement("Obj",
        //        new XElement("QID", thisQuickie.Guid),
        //        new XElement("QIcon", thisQuickie.Icon),
        //        new XElement("QIconOverlay", thisQuickie.IconOverlay),
        //        new XElement("QIconUnderlay", thisQuickie.IconUnderlay)));
        //    xdoc.Save(filename);
        //}




    }
}//end of namespace


