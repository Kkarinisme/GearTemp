
using System;
using System.Drawing;
using System.IO;
using Decal.Adapter;
using Decal.Filters;
using Decal.Adapter.Wrappers;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using VirindiViewService;
using MyClasses.MetaViewWrappers;
using VirindiViewService.Controls;
using VirindiHUDs;
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;

namespace GearFoundry
{

	public partial class PluginCore
	{

		private List<WorldObject> ButlerInventory;
		private List<WorldObject> MaidKeyRings;
		private List<WorldObject> MaidKeys;
		private List<WorldObject> MaidSalvage;
		
		private bool bButlerTradeOpen = false;
		private bool bButlerDoSometing = false;
		
		private static int GB_USE_ICON = 0x6000FB7;
		private static int GB_GIVE_ICON = 0x60011F7;
		private static int GB_TRADEVENDOR_ICON = 0x6001080;
		private static int GB_EQUIPPED_ICON = 0x600136F;
		private static int GB_UNEQUIPPED_ICON = 0x600127E;
		private static int GB_MELEE_ICON = 0x60010BC;
		private static int GB_MISSILE_ICON = 0x6001302;
		private static int GB_ARMOR_ICON = 0x6000FC7;
		private static int GB_CASTER_ICON = 0x6001066;
		private static int GB_KEY_ICON = 0x6001ED3;
		private static int GB_KEYRING_ICON = 0x6002C3F;
		private static int GB_LOCKPICK_ICON = 0x6001D6E;
		private static int GB_MANASTONE_ICON = 0x60032D4;
		private static int GB_HEALKIT_ICON = 0X60032F3;
		private static int GB_POTION_ICON = 0x60019FD;
				
	    private HudView ButlerHudView = null;
		private HudFixedLayout ButlerHudLayout = null;
		private HudTabView ButlerHudTabView = null;
		private HudFixedLayout ButlerHudTabLayout = null;
		private HudList ButlerHudList = null;
		private HudButton ButlerHudSearchButton = null;
		private HudButton ButlerHudClearSearchButton = null;
		private HudStaticText ButlerHudCurrentSelectionLabel = null;
		private HudStaticText ButlerHudCurrentSelectionText = null;
		private HudPictureBox ButlerHudCurrentSelectionIcon = null;
		private HudStaticText ButlerHudSelectedLabel = null;
		private HudStaticText ButlerHudSelectedCount = null;
			
		private HudTextBox ButlerHudSearchBox = null;
		private HudStaticText ButlerPackSpacesAvailable = null;
		private HudStaticText ButlerPackSpaceAvailableLabel = null;
		private HudStaticText ButlerBurden = null;
		private HudStaticText ButlerBurdenLabel = null;
		private HudList.HudListRowAccessor ButlerHudListRow = null;
		
		private HudStaticText ButlerQuickSortLabel = null;
		private HudImageButton ButlerQuickSortEquipped = null;
		private HudImageButton ButlerQuickSortUnequipped = null;
		private HudImageButton ButlerQuickSortMelee = null;
		private HudImageButton ButlerQuickSortMissile = null;
		private HudImageButton ButlerQuickSortCaster = null;
		private HudImageButton ButlerQuickSortArmor = null;
		private HudImageButton ButlerQuickSortKeys = null;
		private HudImageButton ButlerQuickSortKeyrings = null;
		private HudImageButton ButlerQuickSortLockpicks = null;
		private HudImageButton ButlerQuickSortManastones = null;
		private HudImageButton ButlerQuickSortHealKit = null;
		private HudImageButton ButlerQuickSortPotion = null;	
		
		private HudFixedLayout MaidTabLayout = null;
		private HudButton MaidStackInventory = null;
		private HudButton MaidRingKeys = null;
		private HudButton MaidTradeAllSalvage = null;
		private HudButton MaidTradeParialSalvage = null;
		private HudButton MaidTradeFilledSalvage = null;
		private HudButton MaidSalvageCombine = null;
		
		
		
		
		private void SubscribeButlerEvents()
		{
			try
			{
				Core.CharacterFilter.LoginComplete += ButlerLoginComplete;
				Core.ItemSelected += ButlerItemSelected;
				Core.WorldFilter.EnterTrade += ButlerTradeOpened;
				Core.WorldFilter.EndTrade += ButlerTradeEnd;
				MasterTimer.Tick += ButlerTimerDo;
				
				RenderButlerHud();
			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeButlerEvents()
		{
			try
			{
				DisposeButlerHud();
				
				Core.CharacterFilter.LoginComplete -= ButlerLoginComplete;
				Core.ItemSelected -= ButlerItemSelected;
				Core.WorldFilter.EnterTrade -= ButlerTradeOpened;
				Core.WorldFilter.EndTrade -= ButlerTradeEnd;
				MasterTimer.Tick -= ButlerTimerDo;
			}
			catch(Exception ex){LogError(ex);}
		}
		
		
		private void ButlerLoginComplete(object sender, System.EventArgs e)
		{
			try
			{
				ButlerInventory = Core.WorldFilter.GetInventory().OrderBy(x => x.Name).ToList();
	    		UpdateButlerHudList();
    		}
			catch(Exception ex){LogError(ex);}
		}
		
		private void ButlerItemSelected(object sender, ItemSelectedEventArgs e)
		{
			try
    		{	
				if(Core.WorldFilter[Core.Actions.CurrentSelection] != null)
				{
					ButlerHudCurrentSelectionIcon.Image = Core.WorldFilter[Core.Actions.CurrentSelection].Icon;
					ButlerHudCurrentSelectionText.Text = Core.WorldFilter[Core.Actions.CurrentSelection].Name;
				}
				else
				{
					ButlerHudCurrentSelectionIcon.Image = null;
					ButlerHudCurrentSelectionText.Text = "Nothing Selected";
				}
				UpdateButlerHudList();
    		}catch{}
		}
		
		private void ButlerTradeOpened(object sender, EnterTradeEventArgs e)
		{
			try
			{
				bButlerTradeOpen = true;		
			}catch{}
		}
		
		private void ButlerTradeEnd(object sender, EndTradeEventArgs e)
		{
			try
			{	
				bButlerTradeOpen = false;
				ButlerInventory = Core.WorldFilter.GetInventory().ToList();
				UpdateButlerHudList();
			}catch{}

		}
				
    	public void RenderButlerHud()
    	{
    		try
    		{
    			    			
    			if(ButlerHudView != null)
    			{
    				DisposeButlerHud();
    			}			
    			
    			ButlerHudView = new HudView("GearButler", 300, 600, new ACImage(0x2D13));
    			ButlerHudView.Theme = VirindiViewService.HudViewDrawStyle.GetThemeByName("Minimalist Transparent");
    			ButlerHudView.UserAlphaChangeable = false;
    			ButlerHudView.ShowInBar = false;
    			ButlerHudView.Visible = true;
                ButlerHudView.UserClickThroughable = false;
                ButlerHudView.UserMinimizable = false;
    			
    			ButlerHudLayout = new HudFixedLayout();
    			ButlerHudView.Controls.HeadControl = ButlerHudLayout;
    			
    			ButlerHudTabView = new HudTabView();
    			ButlerHudLayout.AddControl(ButlerHudTabView, new Rectangle(0,0,300,600));
    		
    			ButlerHudTabLayout = new HudFixedLayout();
    			ButlerHudTabView.AddTab(ButlerHudTabLayout, "GearButler");
    					
    			ButlerHudCurrentSelectionLabel = new HudStaticText();
    			ButlerHudCurrentSelectionLabel.Text = "[Current Selection]";
    			ButlerHudTabLayout.AddControl(ButlerHudCurrentSelectionLabel, new Rectangle(0,0,200,16));
    			    			    			
    			ButlerHudCurrentSelectionIcon = new HudPictureBox();
    			ButlerHudCurrentSelectionIcon.Image = null;
    			ButlerHudTabLayout.AddControl(ButlerHudCurrentSelectionIcon, new Rectangle(0,20,25,25));
    			
    			ButlerHudCurrentSelectionText = new HudStaticText();
    			ButlerHudCurrentSelectionText.Text = null;
    			ButlerHudTabLayout.AddControl(ButlerHudCurrentSelectionText, new Rectangle(0,50,150,16));
    			  			
    			ButlerHudSearchBox = new HudTextBox();
    			ButlerHudSearchBox.Text = "";
    			ButlerHudTabLayout.AddControl(ButlerHudSearchBox, new Rectangle(0,80,200,20));
    			
    			ButlerHudSearchButton = new HudButton();
    			ButlerHudSearchButton.Text = "Search";
    			ButlerHudTabLayout.AddControl(ButlerHudSearchButton, new Rectangle(205,80,40,20));
    			
    			ButlerHudClearSearchButton = new HudButton();
    			ButlerHudClearSearchButton.Text = "Reset";
    			ButlerHudTabLayout.AddControl(ButlerHudClearSearchButton, new Rectangle(250,80,40,20));
    			
    			//Start at 120
    			ButlerQuickSortLabel = new HudStaticText();
    			ButlerQuickSortLabel.Text = "QuickSort: ";
    			ButlerHudTabLayout.AddControl(ButlerQuickSortLabel, new Rectangle(0,110,50,16));
    			
    			ButlerQuickSortEquipped = new HudImageButton();
    			ButlerQuickSortEquipped.Image_Up = GB_EQUIPPED_ICON;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortEquipped, new Rectangle(60,110,16,16));
    			
    			ButlerQuickSortUnequipped = new HudImageButton();
    			ButlerQuickSortUnequipped.Image_Up = GB_UNEQUIPPED_ICON;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortUnequipped, new Rectangle(80,110,16,16));
    			
    			ButlerQuickSortMelee = new HudImageButton();
    			ButlerQuickSortMelee.Image_Up = GB_MELEE_ICON;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortMelee, new Rectangle(100,110,16,16));
    			
    			ButlerQuickSortMissile = new HudImageButton();
    			ButlerQuickSortMissile.Image_Up = GB_MISSILE_ICON;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortMissile, new Rectangle(120,110,16,16));
    			
    			ButlerQuickSortCaster = new HudImageButton();
    			ButlerQuickSortCaster.Image_Up = GB_CASTER_ICON;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortCaster, new Rectangle(140,110,16,16));
    			
    			ButlerQuickSortArmor = new HudImageButton();
    			ButlerQuickSortArmor.Image_Up = GB_ARMOR_ICON;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortArmor, new Rectangle(160,110,16,16));
    			
    			ButlerQuickSortKeys = new HudImageButton();
    			ButlerQuickSortKeys.Image_Up = GB_KEY_ICON;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortKeys, new Rectangle(180,110,16,16));
    			
    			ButlerQuickSortKeyrings = new HudImageButton();
    			ButlerQuickSortKeyrings.Image_Up = GB_KEYRING_ICON;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortKeyrings, new Rectangle(200,110,16,16));
    			
    			ButlerQuickSortLockpicks = new HudImageButton();
    			ButlerQuickSortLockpicks.Image_Up = GB_LOCKPICK_ICON;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortLockpicks, new Rectangle(220,110,16,16));
    			
    			ButlerQuickSortManastones = new HudImageButton();
    			ButlerQuickSortManastones.Image_Up = GB_MANASTONE_ICON;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortManastones, new Rectangle(240,110,16,16));
    			
    			ButlerQuickSortHealKit = new HudImageButton();
    			ButlerQuickSortHealKit.Image_Up = GB_HEALKIT_ICON;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortHealKit, new Rectangle(260,110,16,16));
    			
    			ButlerQuickSortPotion = new HudImageButton();
    			ButlerQuickSortPotion.Image_Up = GB_POTION_ICON;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortPotion, new Rectangle(280,110,16,16));
								
				ButlerHudSelectedLabel = new HudStaticText();
				ButlerHudSelectedLabel.Text = "Items Selected: ";
				ButlerHudSelectedCount = new HudStaticText();
				ButlerHudSelectedCount.Text = "0";
				ButlerHudTabLayout.AddControl(ButlerHudSelectedLabel, new Rectangle(0,520,100,16));
				ButlerHudTabLayout.AddControl(ButlerHudSelectedCount, new Rectangle(110,520,150,16));
				
				ButlerPackSpacesAvailable = new HudStaticText();
				ButlerPackSpacesAvailable.Text = "0";
				ButlerPackSpaceAvailableLabel = new HudStaticText();
				ButlerPackSpaceAvailableLabel.Text = "Inventory status: ";
				ButlerHudTabLayout.AddControl(ButlerPackSpaceAvailableLabel, new Rectangle(0,540,100,16));
				ButlerHudTabLayout.AddControl(ButlerPackSpacesAvailable, new Rectangle(110,540,150,16));
				
				ButlerBurdenLabel = new HudStaticText();
				ButlerBurdenLabel.Text = "Current Burden: ";
				ButlerBurden = new HudStaticText();
				ButlerBurden.Text = "0";
				ButlerHudTabLayout.AddControl(ButlerBurdenLabel, new Rectangle(0,560,100,16));
				ButlerHudTabLayout.AddControl(ButlerBurden, new Rectangle(110,560, 150, 16));
				
				ButlerHudList = new HudList();
				ButlerHudList.ControlHeight = 16;	
				ButlerHudList.AddColumn(typeof(HudPictureBox), 15, null);
				ButlerHudList.AddColumn(typeof(HudStaticText), 175, null);
				ButlerHudList.AddColumn(typeof(HudPictureBox), 15, null);
				ButlerHudList.AddColumn(typeof(HudPictureBox), 15, null);
				ButlerHudList.AddColumn(typeof(HudPictureBox), 15, null);
				ButlerHudList.AddColumn(typeof(HudPictureBox), 15, null);	
				
				ButlerHudLayout.AddControl(ButlerHudList, new Rectangle(0,150,300,375));
				
												
				MaidTabLayout = new HudFixedLayout();
    			ButlerHudTabView.AddTab(MaidTabLayout, "GearMaid");
				
    			MaidStackInventory = new HudButton();
    			MaidStackInventory.Text = "Stack Inventory";
    			MaidTabLayout.AddControl(MaidStackInventory, new Rectangle(0,0,150,20));
    			
    			MaidRingKeys = new HudButton();
    			MaidRingKeys.Text = "Ring Keys";
    			MaidTabLayout.AddControl(MaidRingKeys, new Rectangle(0,30,150,20));
    			
    			MaidTradeAllSalvage = new HudButton();
    			MaidTradeAllSalvage.Text = "Window All Salvage";    			
    			MaidTabLayout.AddControl(MaidTradeAllSalvage, new Rectangle(0,60,150,20));
    			
    			MaidTradeFilledSalvage = new HudButton();
    			MaidTradeFilledSalvage.Text = "Window Filled Salvage";
    			MaidTabLayout.AddControl(MaidTradeFilledSalvage, new Rectangle(0,90,150,20));
    			
    			MaidTradeParialSalvage = new HudButton();
    			MaidTradeParialSalvage.Text = "Window Partial Salvage";
    			MaidTabLayout.AddControl(MaidTradeParialSalvage, new Rectangle(0,120,150,20));
    			
    			MaidSalvageCombine = new HudButton();
    			MaidSalvageCombine.Text = "Combine Salvage Bags";
    			MaidTabLayout.AddControl(MaidSalvageCombine, new Rectangle(0,150,150,20));
    			
    			
    			ButlerQuickSortEquipped.Hit += (sender, e) => ButlerQuickSortEquipped_Hit(sender, e);
    			ButlerQuickSortUnequipped.Hit += (sender, e) => ButlerQuickSortUnequipped_Hit(sender, e);
    			ButlerQuickSortMelee.Hit += (sender, e) => ButlerQuickSortMelee_Hit(sender, e);
    			ButlerQuickSortMissile.Hit += (sender, e) => ButlerQuickSortMissile_Hit(sender, e);
    			ButlerQuickSortCaster.Hit += (sender, e) => ButlerQuickSortCaster_Hit(sender, e);
    			ButlerQuickSortArmor.Hit += (sender, e) => ButlerQuickSortArmor_Hit(sender, e);
    			ButlerQuickSortKeys.Hit += (sender, e) => ButlerQuickSortKeys_Hit(sender, e);
    			ButlerQuickSortKeyrings.Hit += (sender, e) => ButlerQuickSortKeyrings_Hit(sender, e);
    			ButlerQuickSortLockpicks.Hit += (sender, e) => ButlerQuickSortLockpicks_Hit(sender, e);
    			ButlerQuickSortManastones.Hit += (sender, e) => ButlerQuickSortManastones_Hit(sender, e);
    			ButlerQuickSortHealKit.Hit += (sender, e) => ButlerQuickSortHealKit_Hit(sender, e);
    			ButlerQuickSortPotion.Hit += (sender, e) => ButlerQuickSortPotion_Hit(sender, e);
    			
    			ButlerHudList.Click += (sender, row, col) => ButlerHudList_Click(sender, row, col);
				ButlerHudSearchButton.Hit += (sender, e) => ButlerHudSearchButton_Click(sender, e);
				ButlerHudClearSearchButton.Hit += (sender, e) => ButlerHudClearSearchButton_Click(sender, e);	
    				
				ButlerHudSearchBox.LostFocus += ButlerHudSearchBox_Lostfocus;    			
				MaidStackInventory.Hit += MaidStackInventory_Hit;
    			MaidRingKeys.Hit += MaidRingKeys_Hit;
    			MaidTradeAllSalvage.Hit += MaidTradeAllSalvage_Hit;
    			MaidTradeFilledSalvage.Hit += MaidTradeFilledSalvage_Hit;
    			MaidTradeParialSalvage.Hit += MaidTradeParialSalvage_Hit;
    			MaidSalvageCombine.Hit += MaidSalvageCombine_Hit;
				
			  							
    		}catch(Exception ex) {LogError(ex);}
    		return;
    	}
    	
    	private void DisposeButlerHud()
    	{	
    		try
    		{
    			
    			ButlerQuickSortPotion.Hit -= (sender, e) => ButlerQuickSortPotion_Hit(sender, e);
    			ButlerQuickSortHealKit.Hit -= (sender, e) => ButlerQuickSortHealKit_Hit(sender, e);
    			ButlerQuickSortManastones.Hit -= (sender, e) => ButlerQuickSortManastones_Hit(sender, e);
    			ButlerQuickSortLockpicks.Hit -= (sender, e) => ButlerQuickSortLockpicks_Hit(sender, e);
    			ButlerQuickSortKeyrings.Hit -= (sender, e) => ButlerQuickSortKeyrings_Hit(sender, e);
    			ButlerQuickSortKeys.Hit -= (sender, e) => ButlerQuickSortKeys_Hit(sender, e);
    			ButlerQuickSortArmor.Hit -= (sender, e) => ButlerQuickSortArmor_Hit(sender, e);
    			ButlerQuickSortCaster.Hit -= (sender, e) => ButlerQuickSortCaster_Hit(sender, e);
    			ButlerQuickSortMissile.Hit -= (sender, e) => ButlerQuickSortMissile_Hit(sender, e);
    			ButlerQuickSortMelee.Hit -= (sender, e) => ButlerQuickSortMelee_Hit(sender, e);
    			ButlerQuickSortUnequipped.Hit -= (sender, e) => ButlerQuickSortUnequipped_Hit(sender, e);
    			ButlerQuickSortEquipped.Hit -= (sender, e) => ButlerQuickSortEquipped_Hit(sender, e);
    			ButlerHudList.Click -= (sender, row, col) => ButlerHudList_Click(sender, row, col);
				ButlerHudSearchButton.Hit -= (sender, e) => ButlerHudSearchButton_Click(sender, e);
				ButlerHudClearSearchButton.Hit -= (sender, e) => ButlerHudClearSearchButton_Click(sender, e);
								
				ButlerBurden.Dispose();
				ButlerBurdenLabel.Dispose();
				ButlerPackSpaceAvailableLabel.Dispose();
				ButlerPackSpacesAvailable.Dispose();
				ButlerHudSelectedCount.Dispose();
				ButlerHudSelectedLabel.Dispose();
				ButlerHudList.Dispose();
				ButlerQuickSortPotion.Dispose();
				ButlerQuickSortHealKit.Dispose();
				ButlerQuickSortManastones.Dispose();
    			ButlerQuickSortLockpicks.Dispose();
    			ButlerQuickSortKeyrings.Dispose();
    			ButlerQuickSortKeys.Dispose();
    			ButlerQuickSortArmor.Dispose();
    			ButlerQuickSortCaster.Dispose();
    			ButlerQuickSortMissile.Dispose();
    			ButlerQuickSortMelee.Dispose();
    			ButlerQuickSortUnequipped.Dispose();
    			ButlerQuickSortEquipped.Dispose();
    			ButlerQuickSortLabel.Dispose();
    			ButlerHudClearSearchButton.Dispose();
       			ButlerHudSearchButton.Dispose();
    			ButlerHudSearchBox.Dispose();
    			ButlerHudCurrentSelectionText.Dispose();
    			ButlerHudCurrentSelectionIcon.Dispose();
    			ButlerHudCurrentSelectionLabel.Dispose();
    			ButlerHudTabLayout.Dispose();
    			ButlerHudTabView.Dispose(); 		
				ButlerHudLayout.Dispose();
    			ButlerHudView.Dispose();	
  			
    		}catch(Exception ex) {LogError(ex);}
    		return;
    	}
    	
    	private void ButlerHudSearchBox_Lostfocus(object sender, System.EventArgs e)
    	{
    		try
    		{
    			UpdateButlerHudList();
    		}catch{}
    	}
    	
    	private void  ButlerHudSearchButton_Click(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ButlerInventory = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower().Contains(ButlerHudSearchBox.Text.ToLower())).OrderBy(x => x.Name).ToList();
    			UpdateButlerHudList();
    		}catch{}
    	}
    	
    	private void ButlerHudClearSearchButton_Click(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ButlerHudSearchBox.Text = "";
    			ButlerInventory = Core.WorldFilter.GetInventory().OrderBy(x => x.Name).ToList();
    			UpdateButlerHudList();
    		}catch{}	
    	}
    	
    	private void ButlerQuickSortEquipped_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ButlerInventory = Core.WorldFilter.GetInventory().OrderBy(x => x.Name).ToList();
    			ButlerInventory.RemoveAll(x => x.Values(LongValueKey.EquippedSlots) ==  0);               			
    			UpdateButlerHudList();
    		}catch{}
    	}
    	
    	private void ButlerQuickSortUnequipped_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ButlerInventory = Core.WorldFilter.GetInventory().OrderBy(x => x.Name).ToList();
    			ButlerInventory.RemoveAll(x => x.Values(LongValueKey.EquippedSlots) !=  0);
				ButlerInventory.RemoveAll(x => x.Values(LongValueKey.Unknown10) ==  56);                 			
    			UpdateButlerHudList();
    		}catch{}
    	}
    	
    	private void ButlerQuickSortMelee_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ButlerInventory = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.MeleeWeapon).OrderBy(x => x.Name).ToList();               			
    			UpdateButlerHudList();
    		}catch{}
    	}
    	
    	private void ButlerQuickSortMissile_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ButlerInventory = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.MissileWeapon).OrderBy(x => x.Name).ToList();               			
    			UpdateButlerHudList();
    		}catch{}
    	}
    	    	
  		private void ButlerQuickSortCaster_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ButlerInventory = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.WandStaffOrb).OrderBy(x => x.Name).ToList();               			
    			UpdateButlerHudList();
    		}catch{}
    	}
  		
  		private void ButlerQuickSortArmor_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ButlerInventory = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Armor).OrderBy(x => x.Name).ToList();               			
    			UpdateButlerHudList();
    		}catch{}
    	}
  			
  		private void  ButlerQuickSortKeys_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ButlerInventory = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Key).OrderBy(x => x.Name).ToList();
    			UpdateButlerHudList();
    		}catch{}
    	}
  		
  		private void ButlerQuickSortKeyrings_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ButlerInventory = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower().Contains("keyring")).OrderBy(x => x.Name).ToList();
    			UpdateButlerHudList();
    		}catch{}
    	}
  		
  		private void ButlerQuickSortLockpicks_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ButlerInventory = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower().Contains("lockpick")).OrderBy(x => x.Values(LongValueKey.LockpickSkillBonus)).ToList();
    			UpdateButlerHudList();
    		}catch{}
    	}
  		
  		private void ButlerQuickSortManastones_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ButlerInventory = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.ManaStone).OrderBy(x => x.Values(LongValueKey.CurrentMana)).ToList();
    			UpdateButlerHudList();
    		}catch{}
    	}
  		
  		private void ButlerQuickSortHealKit_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ButlerInventory = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.HealingKit).OrderBy(x => x.Values(LongValueKey.HealKitSkillBonus)).ToList();
    			UpdateButlerHudList();
    		}catch{}
    	}
  		
  		private void ButlerQuickSortPotion_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ButlerInventory = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Food).OrderBy(x => x.Name).ToList();
    			UpdateButlerHudList();
    		}catch{}
    	}
  		
    	
    	private void UpdateButlerHudList()
	    {  	
	    	try
	    	{    	
	    		if(ButlerHudTabView.CurrentTab != 0) 
	    		{
		    	    ButlerHudList.ClearRows();
	    			return;
	    		}
	    		else 
		    	{
	    			ButlerHudList.ClearRows();
	    		}
	    		    		
    	    	ButlerHudSelectedCount.Text = ButlerInventory.Count().ToString();
	    	    ButlerBurden.Text = Core.CharacterFilter.Burden.ToString() + "%";
	    	    if(Core.CharacterFilter.Burden < 100){ButlerBurden.TextColor = Color.Green;}
	    	    if(Core.CharacterFilter.Burden >= 100){ButlerBurden.TextColor = Color.Yellow;}
	    	    if(Core.CharacterFilter.Burden >= 200){ButlerBurden.TextColor = Color.Red;}
	    	    ButlerPackSpacesAvailable.Text = CalculateAvailableSpace();
	    		
	    			  	    		

	    	    foreach(WorldObject wo in ButlerInventory)
	    	    {
	    	    	ButlerHudListRow = ButlerHudList.AddRow();
	    	    	
	    	    	((HudPictureBox)ButlerHudListRow[0]).Image = wo.Icon + 0x6000000;
	    	    	((HudStaticText)ButlerHudListRow[1]).Text = wo.Name;
	    	    	if(wo.Values(LongValueKey.EquippedSlots) > 0 || wo.Values(LongValueKey.Unknown10) == 56) {((HudPictureBox)ButlerHudListRow[2]).Image = GB_EQUIPPED_ICON;}
	    	    	
	    	    	((HudPictureBox)ButlerHudListRow[3]).Image = GB_USE_ICON;
	    	    	((HudPictureBox)ButlerHudListRow[4]).Image = GB_GIVE_ICON;
	    	    	((HudPictureBox)ButlerHudListRow[5]).Image = GB_TRADEVENDOR_ICON;    	    	
	    	    }
	    	}catch(Exception ex){LogError(ex);}
	    	return;	    	
	    }
    	
    	
    	private string CalculateAvailableSpace()
    	{
    		try
    		{
	    		int BasePackSpace = 17*6;
	    		int BagSpaces = 0;
	    		
	    		var Bags = from all in Core.WorldFilter.GetInventory()
	    					where all.Values(LongValueKey.ItemSlots) > 0
	    					select all;
	    		
	    		foreach(var bag in Bags)
	    		{
	    			BagSpaces += bag.Values(LongValueKey.ItemSlots);
	    		}
    			
	    		var SlotsFilled = from items in Core.WorldFilter.GetInventory()
	    			where items.Values(LongValueKey.EquippedSlots) == 0 && items.Values(LongValueKey.Unknown10) != 56
	    			select items;
	    		
	    		return "Using " + SlotsFilled.Count().ToString() + " of " + (BasePackSpace + BagSpaces).ToString() + " spaces.";
    		
    		
    		}catch{return "Error";}

    		
    	}
    	
    	private void ButlerHudList_Click(object sender, int row, int col)
    	{
    		try
			{
    			if(col == 0)
    			{
    				Host.Actions.SelectItem(ButlerInventory[row].Id);
    			}
    			if(col == 1)
    			{
    				try{HudToChat(new IdentifiedObject(ButlerInventory[row]).LinkString(), 2);}catch{}
    			}
    			if(col == 3)
    			{    				
    				if(ButlerInventory[row].Values(LongValueKey.Unknown10) == 8)
    				{
    					if(!ButlerInventory[row].Name.Contains("Mana Stone") || !ButlerInventory[row].Name.Contains("Dessicant"))
    					{
    						Host.Actions.UseItem(ButlerInventory[row].Id, 1);
    					}
    				}
    				else
    				{
						Host.Actions.UseItem(ButlerInventory[row].Id, 0);
    				}
    				ButlerInventory = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower().Contains(ButlerHudSearchBox.Text.ToLower())).OrderBy(x => x.Name).ToList();
    				UpdateButlerHudList();
    			}
    			if(col == 4)
    			{
    				if(Core.WorldFilter[Core.Actions.CurrentSelection].ObjectClass == ObjectClass.Npc ||  Core.WorldFilter[Core.Actions.CurrentSelection].ObjectClass == ObjectClass.Player)
    				{
    				   if(ButlerInventory[row].Values(LongValueKey.EquippedSlots) > 0 || ButlerInventory[row].Values(LongValueKey.Unknown10) == 56)
    				   {
	    				   	WriteToChat("Unequip the item first.");	   	
    				   }		
    				   else
    				   {
    				   		Host.Actions.GiveItem(ButlerInventory[row].Id, Host.Actions.CurrentSelection);
    				   }
    				}
    				else
    				{
    					WriteToChat("Select an NPC or Player.");
    				}
    				ButlerInventory = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower().Contains(ButlerHudSearchBox.Text.ToLower())).OrderBy(x => x.Name).ToList();
    				UpdateButlerHudList();
    			}
    			if(col == 5)
    			{
    				if(bButlerTradeOpen)
    				{
    					Core.Actions.TradeAdd(ButlerInventory[row].Id);
    				}
    				else if(Core.WorldFilter.OpenVendor.MerchantId != 0)
    				{
    				   if(ButlerInventory[row].Values(LongValueKey.EquippedSlots) > 0 || ButlerInventory[row].Values(LongValueKey.Unknown10) == 56)
    				   {
	    				   	WriteToChat("Unequip the item first.");	   	
    				   }		
    				   else
    				   {
    				   		Core.Actions.VendorAddSellList(ButlerInventory[row].Id);
    				   }
    				}
    			}
			}
			catch (Exception ex) { LogError(ex); }
			return;			
    	}
    	
    	int ButlerSeconds = 0;
    	private void ButlerTimerDo(object sender, EventArgs e)
    	{
    		try
    		{
    			if(ButlerSeconds < 29){return;}
    			else
    			{
    				ButlerSeconds = 0;	
    				if(bAutoRingKeys){AutoRingKeys();}
    			}
    		}catch{}
    	}
    	
    	//Irquk:  TODO:  Feature
		private void AutoRingKeys()
		{
			try
			{
				//Keyring to Key mathching for the non-obvious ones
				//Black Coral = Mana Forge
				//Burning Sands = Legendary
				
				string[] RingableKeysArray = {"legendary", "black marrow", "directive", "granite", "mana forge", "master", "marble", "singularity",	"skeletal falatacot"};
				string[] KeyringMatchingArray = {"burning sands", "black marrow", "directive", "granite", "black coral", "master", "marble", "singularity", "skeletal falatacot"};
							
				var Keyrings = from keyrings in Core.WorldFilter.GetInventory()
					where keyrings.Name.ToLower().Contains("keyring") && keyrings.Values(LongValueKey.UsesRemaining) > 0 && keyrings.Values(LongValueKey.KeysHeld) < 24
					select keyrings;
				
				MaidKeyRings = Keyrings.ToList();
				
				var Keys = from items in Core.WorldFilter.GetInventory()
					where items.ObjectClass == ObjectClass.Key
					select items;
				
				
				
				foreach(var key in Keys)
				{
					if(key.Name.ToLower() == "mana forge key")
					{
						
					}
				}
				

//[VTank] (S) Name: Sturdy Steel Keyring
//[VTank] (S) FullDescription: A crude keyring roughly carved out of a coral golem heart. 
//[VTank] (S) UsageInstructions: Use this ring on a sturdy steel key to add the key to the ring. Use an intricate carving tool on the keyring to pop a key off again. Adding a key uses up one of the ring's remaining uses, but removing a key does not.
//[VTank] (B) CanBeSold: False
//[VTank] (I) CreateFlags1: 271088664
//[VTank] (I) Type: 24887
//[VTank] (I) Icon: 11327
//[VTank] (I) Category: 128
//[VTank] (I) Behavior: 18
//[VTank] (I) Value: 10
//[VTank] (I) Unknown10: 524296
//[VTank] (I) UsageMask: 16384
//[VTank] (I) UsesRemaining: 28
//[VTank] (I) UsesTotal: 50
//[VTank] (I) StackCount: 1
//[VTank] (I) StackMax: 1
//[VTank] (I) Container: 1343190719
//[VTank] (I) Burden: 40
//[VTank] (I) HookMask: 2
//[VTank] (I) PhysicsDataFlags: 137345
//[VTank] (I) KeysHeld: 22



				
				
			}catch{}

			
		}
		
		private void MaidStackInventory_Hit(object sender, System.EventArgs e)
		{
			WriteToChat("works");
		}
		
		private void MaidRingKeys_Hit(object sender, System.EventArgs e)
		{
			WriteToChat("works");
		}
		
		private void MaidTradeAllSalvage_Hit(object sender, System.EventArgs e)
		{
			try
			{
				ScanInventoryForSalvageBags();
				if(bButlerTradeOpen)
				{
					TradeSalvageBags(1);
					return;
				}
				else if(Core.Actions.VendorId != 0)
				{
					SellSalvageBags(1);
				}
				else
				{
					WriteToChat("Open a trade or vendor window.");
				}
			}catch{}
		}
		private void MaidTradeFilledSalvage_Hit(object sender, System.EventArgs e)
		{
			try
			{
				ScanInventoryForSalvageBags();
				if(bButlerTradeOpen)
				{
					TradeSalvageBags(0);
					return;
				}
				else if(Core.Actions.VendorId != 0)
				{
					SellSalvageBags(0);
				}
				else
				{
					WriteToChat("Open a trade or vendor window.");
				}
			}catch{}
		}
		private void MaidTradeParialSalvage_Hit(object sender, System.EventArgs e)
		{
			try
			{
				ScanInventoryForSalvageBags();
				if(bButlerTradeOpen)
				{
					TradeSalvageBags(2);
					return;
				}
				else if(Core.Actions.VendorId != 0)
				{
					SellSalvageBags(2);
				}
				else
				{
					WriteToChat("Open a trade or vendor window.");
				}
			}catch{}
		}
		
		private void MaidSalvageCombine_Hit(object sender, System.EventArgs e)
		{
			WriteToChat("works");
		}
		
		private void SellSalvageBags(int bagtype)
		{
			try
			{
				MaidScanInventoryForSalvageBags();
				
				List <WorldObject> tradelist;
				
				if(bagtype == 0)
				{
					tradelist = MaidSalvage.Where(x => x.Values(LongValueKey.UsesRemaining) == 100).OrderBy(x => x.Name).ToList();
				}
				else if(bagtype == 1)
				{
					tradelist = MaidSalvage.ToList();
				}
				else if(bagtype == 2)
				{
					tradelist = MaidSalvage.Where(x => x.Values(LongValueKey.UsesRemaining) < 100).OrderBy(x => x.Name).ToList();
				}
				else
				{
					tradelist = new List<WorldObject>();
				}
				

				
				foreach(WorldObject sb in tradelist)
				{
					Core.Actions.VendorAddSellList(sb.Id);
				}
				
			}
			catch
			{
			}
		}
		
		private void TradeSalvageBags(int bagtype)
		{
			try
			{
				
				MaidScanInventoryForSalvageBags();
				
				List<WorldObject> tradelist;
				
				if(bagtype == 0)
				{
					tradelist = MaidSalvage.Where(x => x.Values(LongValueKey.UsesRemaining) == 100).OrderBy(x => x.Name).ToList();
				}
				else if(bagtype == 1)
				{
					tradelist = MaidSalvage.ToList();
				}
				else if(bagtype == 2)
				{
					tradelist = MaidSalvage.Where(x => x.Values(LongValueKey.UsesRemaining) < 100).OrderBy(x => x.Name).ToList();
				}
				else
				{
					tradelist = new List<WorldObject>();
				}
				
				foreach(WorldObject bags in tradelist)
				{
					Core.Actions.TradeAdd(bags.Id);
				}
				

			}catch{}
		}
		
		
		private void MaidScanInventoryForSalvageBags()
		{
			try
			{
				MaidSalvage = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower().Contains("salvage")).ToList();
			}catch{}
		}
    		
	}
}



