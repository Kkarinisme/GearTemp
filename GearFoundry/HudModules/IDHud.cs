/*
 * Created by SharpDevelop.
 * User: Paul
 * Date: 12/27/2012
 * Time: 11:33 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using VirindiViewService;
using VirindiViewService.Controls;
using System.Drawing;

namespace AlincoVVS.HUD
{

	public partial class PluginCore
	{
		public void IDHud()
		{
			try
	        {
               VirindiViewService.HudView ID_Hud = new VirindiViewService.HudView(null, 60, 105, new ACImage(Color.Black), false);
	
               ID_Hud.Visible = true;
               ID_Hud.UserGhostable = false;
               ID_Hud.Ghosted = true;
               ID_Hud.UserMinimizable = false;
               ID_Hud.UserAlphaChangeable = false;
               ID_Hud.ShowIcon = false;
               ID_Hud.ClickThrough = true;
               ID_Hud.Theme = HudViewDrawStyle.GetThemeByName("Minimalist Transparent");
               //ViewR.LoadUserSettings();
	
//               ViewR_Head = new VirindiViewService.Controls.HudFixedLayout();
//               ViewR.Controls.HeadControl = ViewR_Head;
	
//               int y = 0;
//	
//               imgTaper = new HudPictureBox();
//               ViewR_Head.AddControl(imgTaper, new Rectangle(0, y, 0x10, 0x10));
//               imgTaper.Image = new ACImage(9770);
//	
//               txtTapers = new HudStaticText();
//               txtTapers.FontHeight = 9;
//               txtTapers.TextColor = Color.White;
//               ViewR_Head.AddControl(txtTapers, new Rectangle(20, y, 40, 0x10));
//	
//               y += 0x12;
//	
//               imgManaScarab = new HudPictureBox();
//               ViewR_Head.AddControl(imgManaScarab, new Rectangle(0, y, 0x10, 0x10));
//               imgManaScarab.Image = new ACImage(26533);
//
//               txtManaScarabs = new HudStaticText();
//               txtManaScarabs.FontHeight = 9;
//               txtManaScarabs.TextColor = Color.White;
//               ViewR_Head.AddControl(txtManaScarabs, new Rectangle(15, y, 18, 0x10));
//	
//               imgMassiveManaCharge = new HudPictureBox();
//               ViewR_Head.AddControl(imgMassiveManaCharge, new Rectangle(33, y, 0x10, 0x10));
//               imgMassiveManaCharge.Image = new ACImage(13107);
//	
//               txtMassiveManaCharges = new HudStaticText();
//               txtMassiveManaCharges.FontHeight = 9;
//               txtMassiveManaCharges.TextColor = Color.White;
//               ViewR_Head.AddControl(txtMassiveManaCharges, new Rectangle(46, y, 12, 0x10));
//	
//               y += 0x12;
//	
//               imgPlatinumScarab = new HudPictureBox();
//               ViewR_Head.AddControl(imgPlatinumScarab, new Rectangle(0, y, 0x10, 0x10));
//               imgPlatinumScarab.Image = new ACImage(8033);
//	
//               txtPlatinumScarabs = new HudStaticText();
//               txtPlatinumScarabs.FontHeight = 9;
//               txtPlatinumScarabs.TextColor = Color.White;
//               ViewR_Head.AddControl(txtPlatinumScarabs, new Rectangle(15, y, 18, 0x10));
//	
//               imgMajorManaStone = new HudPictureBox();
//               ViewR_Head.AddControl(imgMajorManaStone, new Rectangle(33, y, 0x10, 0x10));
//               imgMajorManaStone.Image = new ACImage(13012);
//	
//               txtManaStones = new HudStaticText();
//               txtManaStones.FontHeight = 9;
//               txtManaStones.TextColor = Color.White;
//               ViewR_Head.AddControl(txtManaStones, new Rectangle(46, y, 12, 0x10));
//	
//               y += 0x12;
//	
//               txtHoursLeft = new HudStaticText();
//               txtHoursLeft.FontHeight = 9;
//               txtHoursLeft.TextColor = Color.White;
//               ViewR_Head.AddControl(txtHoursLeft, new Rectangle(0, y, 30, 0x10));
//	
//               imgTreatedHealingKit = new HudPictureBox();
//               ViewR_Head.AddControl(imgTreatedHealingKit, new Rectangle(31, y, 0x10, 0x10));
//               imgTreatedHealingKit.Image = new ACImage(13029);
//	
//               txtHealingKits = new HudStaticText();
//               txtHealingKits.FontHeight = 9;
//               txtHealingKits.TextColor = Color.White;
//               ViewR_Head.AddControl(txtHealingKits, new Rectangle(46, y, 12, 0x10));
//	
//               y += 0x12;
//	
//               imgMMD = new HudPictureBox();
//               ViewR_Head.AddControl(imgMMD, new Rectangle(0, y, 0x10, 0x10));
//               imgMMD.Image = new ACImage(10081);
//	
//               txtMMDs = new HudStaticText();
//               txtMMDs.FontHeight = 9;
//               txtMMDs.TextColor = Color.White;
//               ViewR_Head.AddControl(txtMMDs, new Rectangle(15, y, 18, 0x10));
//	
//               txtInactiveEquipment = new HudStaticText();
//               txtInactiveEquipment.FontHeight = 9;
//               txtInactiveEquipment.TextColor = Color.White;
//               ViewR_Head.AddControl(txtInactiveEquipment, new Rectangle(46, y, 12, 0x10));
//	
//               y += 0x12;
//	
//               imgPeas = new HudPictureBox();
//               ViewR_Head.AddControl(imgPeas, new Rectangle(0, y, 0x10, 0x10));
//               imgPeas.Image = new ACImage(7788);
//	
//               txtPeas = new HudStaticText();
//               txtPeas.FontHeight = 9;
//               txtPeas.TextColor = Color.White;
//               ViewR_Head.AddControl(txtPeas, new Rectangle(15, y, 18, 0x10));
//	
//               txtFreePackSlots = new HudStaticText();
//               txtFreePackSlots.FontHeight = 9;
//	           txtFreePackSlots.TextColor = Color.White;
//	           ViewR_Head.AddControl(txtFreePackSlots, new Rectangle(46, y, 12, 0x10));
	       
	
//	            CoreManager.Current.CharacterFilter.Login += new EventHandler<Decal.Adapter.Wrappers.LoginEventArgs>(CharacterFilter_Login);
//	
//	            CoreManager.Current.WorldFilter.CreateObject += new EventHandler<Decal.Adapter.Wrappers.CreateObjectEventArgs>(WorldFilter_CreateObject);
//	            CoreManager.Current.WorldFilter.ChangeObject += new EventHandler<Decal.Adapter.Wrappers.ChangeObjectEventArgs>(WorldFilter_ChangeObject);
//	            CoreManager.Current.WorldFilter.ReleaseObject += new EventHandler<Decal.Adapter.Wrappers.ReleaseObjectEventArgs>(WorldFilter_ReleaseObject);
//	
//	            CoreManager.Current.RenderFrame += new EventHandler<EventArgs>(Current_RenderFrame);
	         }
         catch { }
      }

//      protected override void Shutdown()
//      {
//         try
//         {
//            CoreManager.Current.RenderFrame -= new EventHandler<EventArgs>(Current_RenderFrame);
//
//            CoreManager.Current.CharacterFilter.Login -= new EventHandler<Decal.Adapter.Wrappers.LoginEventArgs>(CharacterFilter_Login);
//
//            CoreManager.Current.WorldFilter.CreateObject -= new EventHandler<Decal.Adapter.Wrappers.CreateObjectEventArgs>(WorldFilter_CreateObject);
//            CoreManager.Current.WorldFilter.ChangeObject -= new EventHandler<Decal.Adapter.Wrappers.ChangeObjectEventArgs>(WorldFilter_ChangeObject);
//            CoreManager.Current.WorldFilter.ReleaseObject -= new EventHandler<Decal.Adapter.Wrappers.ReleaseObjectEventArgs>(WorldFilter_ReleaseObject);
//
//            if (ViewR != null)
//            {
//               ViewR.Dispose();
//               ViewR = null;
//            }
//
//            if (ViewR_Head != null)
//            {
//               ViewR_Head.Dispose();
//               ViewR_Head = null;
//            }
//         }
//         catch { }
//		}
	}
}
