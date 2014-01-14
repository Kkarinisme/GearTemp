
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
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;
using System.Xml.Serialization;
using System.Xml;

namespace GearFoundry
{

	public partial class PluginCore
	{

		ButlerFilters mButlerFilters = new ButlerFilters();
		private Queue<WorldObject> UnchargedManaStones = new Queue<WorldObject>();
		
		private List<ValetTicket> ValetEquipList = new List<ValetTicket>();
		private List<WorldObject> ValetRemoveList = new List<WorldObject>();
		
		private bool bButlerTradeOpen = false;
		private int MaidKeyToRing = 0;
		private int MatchedKeyRingId = 0;
		
		internal class ButlerFilters
		{
			internal string name = String.Empty;
			internal bool equipped = false;
			internal bool notquipped = false;
			internal bool melee = false;
			internal bool missile = false;
			internal bool caster = false;
			internal bool armor = false;
			internal bool keys = false;
			internal bool keyrings = false;
			internal bool healkits = false;
			internal bool lockpicks = false;
			internal bool manastones = false;
			internal bool potion = false;	
		}
			
		public class ButlerSettings
		{
			public List<ValetSuit> SuitList = new List<ValetSuit>();
            public int ButlerHudWidth = 300;
            public int ButlerHudHeight = 500;
            public List<ValetSuit> ValetSuitList = new List<ValetSuit>();
		}
			
		public ButlerSettings GearButlerSettings;
		
		private void SubscribeButlerEvents()
		{
			try
			{
				GearButlerReadWriteSettings(true);
				
				Core.CharacterFilter.Logoff += ButlerHud_LogOff;
				Core.ItemSelected += ButlerItemSelected;
				Core.WorldFilter.EnterTrade += ButlerTradeOpened;
				Core.WorldFilter.EndTrade += ButlerTradeEnd;
				Core.ItemDestroyed += ButlerDestroyed;
				Core.WorldFilter.ReleaseObject += ButlerReleased;
				MasterTimer.Tick += ButlerTimerDo;
				Core.EchoFilter.ServerDispatch += ButlerServerDispatch;					
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ButlerHud_LogOff(object sender, EventArgs e)
		{
			try
			{
				DisposeButlerHud();
				UnsubscribeButlerEvents();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeButlerEvents()
		{
			try
			{	
				GearButlerReadWriteSettings(false);
				
				Core.CharacterFilter.Logoff -= ButlerHud_LogOff;
				Core.ItemSelected -= ButlerItemSelected;
				Core.WorldFilter.EnterTrade -= ButlerTradeOpened;
				Core.WorldFilter.EndTrade -= ButlerTradeEnd;
				Core.ItemDestroyed -= ButlerDestroyed;
				Core.WorldFilter.ReleaseObject -= ButlerReleased;
				MasterTimer.Tick -= ButlerTimerDo;
				Core.EchoFilter.ServerDispatch -= ButlerServerDispatch;
			}
			catch(Exception ex){LogError(ex);}
		}	
		
		
		//Butler View		
		Rectangle CurrentSelectionRectangle = new Rectangle(0,0,30,30);
	    private HudView ButlerHudView = null;
		private HudTabView ButlerHudTabView = null;
		
		//Butler Tab
		private HudFixedLayout ButlerHudTabLayout = null;
		private HudList ButlerHudList = null;
		private HudButton ButlerHudSearchButton = null;
		private HudButton ButlerHudClearSearchButton = null;
		private HudStaticText ButlerHudCurrentSelectionLabel = null;
		private HudStaticText ButlerHudCurrentSelectionText = null;
		private HudImageStack ButlerHudCurrentSelectionIcon = null;
		private HudButton ButlerHudPickCurrentSelection = null;
		private HudButton ButlerHudSalvageCurrentSelection = null;
		private HudButton ButlerHudUseCurrentSelection = null;
		private HudButton ButlerHudDestoryCurrentSelection = null;		
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
		
		//MaidTab
		private HudFixedLayout MaidTabLayout = null;
		private HudButton MaidStackInventory = null;
		private HudButton MaidRingKeys = null;
		private HudButton MaidTradeAllSalvage = null;
		private HudButton MaidTradeParialSalvage = null;
		private HudButton MaidTradeFilledSalvage = null;
		private HudButton MaidTradeAllEightComps = null;
		private HudButton MaidSalvageCombine = null;
		private HudButton MaidCannibalizeInventory = null;
		private HudCheckBox MaidCannibalizeEnable = null;
		
		//Valet Tab
		private HudFixedLayout ValetTabLayout = null;
		private HudButton ValetDisrobe = null;
		private HudButton ValetEquipSuit = null;		
		private HudButton ValetCreateSuit = null;
		private HudList ValetSuitList = null;
		private HudList ValetSuitPiecesList = null;
		private HudStaticText ValetTextBoxLabel = null;
		private HudStaticText ValetSuitListLabel = null;
		private HudStaticText ValetSuitPiecesListLabel = null;
		private HudList.HudListRowAccessor ValetRow = null;
		private HudTextBox ValetNameBox = null;
		
		public void RenderButlerHud()
    	{
    		try
    		{
    			if(ButlerHudView != null)
    			{
    				DisposeButlerHud();
    			}
 			
    			ButlerHudView = new HudView("GearButler", GearButlerSettings.ButlerHudWidth, GearButlerSettings.ButlerHudHeight, new ACImage(0x6AA3));
    			ButlerHudView.UserAlphaChangeable = false;
    			ButlerHudView.ShowInBar = false;
    			ButlerHudView.Visible = true;
                ButlerHudView.UserClickThroughable = false;
                ButlerHudView.UserMinimizable = true;
                ButlerHudView.UserResizeable = true;
                ButlerHudView.LoadUserSettings();
    			
    			ButlerHudTabView = new HudTabView();
                ButlerHudView.Controls.HeadControl = ButlerHudTabView;
    		
                //ButlerTab
    			ButlerHudTabLayout = new HudFixedLayout();
    			ButlerHudTabView.AddTab(ButlerHudTabLayout, "Butler");
    			
    			ButlerHudCurrentSelectionLabel = new HudStaticText();
                ButlerHudCurrentSelectionLabel.FontHeight = nmenuFontHeight;
    			ButlerHudCurrentSelectionLabel.Text = "Current Selection";
    			ButlerHudCurrentSelectionLabel.TextAlignment = VirindiViewService.WriteTextFormats.Center;
                ButlerHudTabLayout.AddControl(ButlerHudCurrentSelectionLabel, new Rectangle(75, 0, 150, 16));
				
    			ButlerHudUseCurrentSelection = new HudButton();
    			ButlerHudUseCurrentSelection.Text = "Use";
    			ButlerHudTabLayout.AddControl(ButlerHudUseCurrentSelection, new Rectangle(5,5,50,20));
    				
    			ButlerHudDestoryCurrentSelection = new HudButton();
    			ButlerHudDestoryCurrentSelection.Text = "Destroy";
    			ButlerHudTabLayout.AddControl(ButlerHudDestoryCurrentSelection, new Rectangle(245,5,50,20));
    			
    			ButlerHudSalvageCurrentSelection = new HudButton();
    			ButlerHudSalvageCurrentSelection.Text = "Salvage";
    			ButlerHudTabLayout.AddControl(ButlerHudSalvageCurrentSelection, new Rectangle(245,30,50,20));
    			    			
    			try
    			{
    				Decal.Interop.Filters.SkillInfo lockpickinfo = Core.CharacterFilter.Underlying.get_Skill((Decal.Interop.Filters.eSkillID)0x17);
    			
	    			if(lockpickinfo.Training.ToString() == "eTrainSpecialized" || lockpickinfo.Training.ToString() == "eTrainTrained")
	    			{
	    				ButlerHudPickCurrentSelection = new HudButton();
	    				ButlerHudPickCurrentSelection.Text = "Pick";
	    				ButlerHudTabLayout.AddControl(ButlerHudPickCurrentSelection, new Rectangle(5,30,50,20));
	    			}
    			}catch(Exception ex){LogError(ex);}
    			    			    			
    			ButlerHudCurrentSelectionIcon = new HudImageStack();
    			ButlerHudTabLayout.AddControl(ButlerHudCurrentSelectionIcon, new Rectangle(135,20,30,30));

                ButlerHudCurrentSelectionText = new HudStaticText();
                ButlerHudCurrentSelectionText.FontHeight = nmenuFontHeight; 
                ButlerHudCurrentSelectionText.Text = null;
    			ButlerHudCurrentSelectionText.TextAlignment = VirindiViewService.WriteTextFormats.Center;
    			ButlerHudTabLayout.AddControl(ButlerHudCurrentSelectionText, new Rectangle(0,50,300,16));
    			  			
    			ButlerHudSearchBox = new HudTextBox();
    			ButlerHudSearchBox.Text = String.Empty;
    			ButlerHudTabLayout.AddControl(ButlerHudSearchBox, new Rectangle(0,80,200,20));
    			
    			ButlerHudSearchButton = new HudButton();
    			ButlerHudSearchButton.Text = "Search";
    			ButlerHudTabLayout.AddControl(ButlerHudSearchButton, new Rectangle(205,80,40,20));
    			
    			ButlerHudClearSearchButton = new HudButton();
    			ButlerHudClearSearchButton.Text = "Reset";
    			ButlerHudTabLayout.AddControl(ButlerHudClearSearchButton, new Rectangle(250,80,40,20));
    			
    			ButlerQuickSortLabel = new HudStaticText();
                ButlerQuickSortLabel.FontHeight = 8;
    			ButlerQuickSortLabel.Text = "QSort:";
    			ButlerHudTabLayout.AddControl(ButlerQuickSortLabel, new Rectangle(0,110,30,16));
    			
    			ButlerQuickSortEquipped = new HudImageButton();
    			ButlerQuickSortEquipped.Image_Down = GearGraphics.GB_SELECT;
    			ButlerQuickSortEquipped.Image_Background = GearGraphics.GB_BACKGROUND;
    			ButlerQuickSortEquipped.Image_Up = GearGraphics.GB_EQUIPPED_ICON;
    			ButlerQuickSortEquipped.CanSticky = true;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortEquipped, new Rectangle(40,110,16,16));
				VirindiViewService.TooltipSystem.AssociateTooltip(ButlerQuickSortEquipped, "Equipped");
    			
    			ButlerQuickSortUnequipped = new HudImageButton();
    			ButlerQuickSortUnequipped.Image_Down = GearGraphics.GB_SELECT;
    			ButlerQuickSortUnequipped.Image_Up = GearGraphics.GB_UNEQUIPPED_ICON;
    			ButlerQuickSortUnequipped.Image_Background = GearGraphics.GB_BACKGROUND;
    			ButlerQuickSortUnequipped.CanSticky = true;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortUnequipped, new Rectangle(60,110,16,16));
				VirindiViewService.TooltipSystem.AssociateTooltip(ButlerQuickSortUnequipped, "Unequipped");
    			
    			ButlerQuickSortMelee = new HudImageButton();
    			ButlerQuickSortMelee.Image_Down = GearGraphics.GB_SELECT;
    			ButlerQuickSortMelee.Image_Up = GearGraphics.GB_MELEE_ICON;
    			ButlerQuickSortMelee.Image_Background = GearGraphics.GB_BACKGROUND;
    			ButlerQuickSortMelee.CanSticky = true;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortMelee, new Rectangle(100,110,16,16));
				VirindiViewService.TooltipSystem.AssociateTooltip(ButlerQuickSortMelee, "Melee Weapons");
    			
    			ButlerQuickSortMissile = new HudImageButton();
    			ButlerQuickSortMissile.Image_Down = GearGraphics.GB_SELECT;
    			ButlerQuickSortMissile.Image_Up = GearGraphics.GB_MISSILE_ICON;
    			ButlerQuickSortMissile.Image_Background = GearGraphics.GB_BACKGROUND;
    			ButlerQuickSortMissile.CanSticky = true;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortMissile, new Rectangle(120,110,16,16));
				VirindiViewService.TooltipSystem.AssociateTooltip(ButlerQuickSortMissile, "Missile Weapons");
    			
    			ButlerQuickSortCaster = new HudImageButton();
    			ButlerQuickSortCaster.Image_Down = GearGraphics.GB_SELECT;
    			ButlerQuickSortCaster.Image_Up = GearGraphics.GB_CASTER_ICON;
    			ButlerQuickSortCaster.Image_Background = GearGraphics.GB_BACKGROUND;
    			ButlerQuickSortCaster.CanSticky = true;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortCaster, new Rectangle(140,110,16,16));
				VirindiViewService.TooltipSystem.AssociateTooltip(ButlerQuickSortCaster, "Magical Casters");
    			
    			ButlerQuickSortArmor = new HudImageButton();
    			ButlerQuickSortArmor.Image_Down = GearGraphics.GB_SELECT;
    			ButlerQuickSortArmor.Image_Up = GearGraphics.GB_ARMOR_ICON;
    			ButlerQuickSortArmor.Image_Background = GearGraphics.GB_BACKGROUND;
    			ButlerQuickSortArmor.CanSticky = true;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortArmor, new Rectangle(160,110,16,16));
				VirindiViewService.TooltipSystem.AssociateTooltip(ButlerQuickSortArmor, "Armor");
    			
    			ButlerQuickSortKeys = new HudImageButton();
    			ButlerQuickSortKeys.Image_Down = GearGraphics.GB_SELECT;
    			ButlerQuickSortKeys.Image_Up = GearGraphics.GB_KEY_ICON;
    			ButlerQuickSortKeys.Image_Background = GearGraphics.GB_BACKGROUND;
    			ButlerQuickSortKeys.CanSticky = true;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortKeys, new Rectangle(180,110,16,16));
				VirindiViewService.TooltipSystem.AssociateTooltip(ButlerQuickSortKeys, "Keys");
    			
    			ButlerQuickSortKeyrings = new HudImageButton();
    			ButlerQuickSortKeyrings.Image_Down = GearGraphics.GB_SELECT;
    			ButlerQuickSortKeyrings.Image_Up = GearGraphics.GB_KEYRING_ICON;
    			ButlerQuickSortKeyrings.Image_Background = GearGraphics.GB_BACKGROUND;
    			ButlerQuickSortKeyrings.CanSticky = true;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortKeyrings, new Rectangle(200,110,16,16));
				VirindiViewService.TooltipSystem.AssociateTooltip(ButlerQuickSortKeyrings, "Keyrings");
    			
    			ButlerQuickSortLockpicks = new HudImageButton();
    			ButlerQuickSortLockpicks.Image_Down = GearGraphics.GB_SELECT;
    			ButlerQuickSortLockpicks.Image_Up = GearGraphics.GB_LOCKPICK_ICON;
    			ButlerQuickSortLockpicks.Image_Background = GearGraphics.GB_BACKGROUND;
    			ButlerQuickSortLockpicks.CanSticky = true;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortLockpicks, new Rectangle(220,110,16,16));
				VirindiViewService.TooltipSystem.AssociateTooltip(ButlerQuickSortLockpicks, "Lockpicks");
    			
    			ButlerQuickSortManastones = new HudImageButton();
    			ButlerQuickSortManastones.Image_Down = GearGraphics.GB_SELECT;
    			ButlerQuickSortManastones.Image_Up = GearGraphics.GB_MANASTONE_ICON;
    			ButlerQuickSortManastones.Image_Background = GearGraphics.GB_BACKGROUND;
    			ButlerQuickSortManastones.CanSticky = true;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortManastones, new Rectangle(240,110,16,16));
				VirindiViewService.TooltipSystem.AssociateTooltip(ButlerQuickSortManastones, "Mana Stones");
    			
    			ButlerQuickSortHealKit = new HudImageButton();
    			ButlerQuickSortHealKit.Image_Down = GearGraphics.GB_SELECT;
    			ButlerQuickSortHealKit.Image_Up = GearGraphics.GB_HEALKIT_ICON;
    			ButlerQuickSortHealKit.Image_Background = GearGraphics.GB_BACKGROUND;
    			ButlerQuickSortHealKit.CanSticky = true;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortHealKit, new Rectangle(260,110,16,16));
				VirindiViewService.TooltipSystem.AssociateTooltip(ButlerQuickSortHealKit, "Healing Kits");
    			
    			ButlerQuickSortPotion = new HudImageButton();
    			ButlerQuickSortPotion.Image_Down = GearGraphics.GB_SELECT;
    			ButlerQuickSortPotion.Image_Up = GearGraphics.GB_POTION_ICON;
    			ButlerQuickSortPotion.Image_Background = GearGraphics.GB_BACKGROUND;
    			ButlerQuickSortPotion.CanSticky = true;
    			ButlerHudTabLayout.AddControl(ButlerQuickSortPotion, new Rectangle(280,110,16,16));
				VirindiViewService.TooltipSystem.AssociateTooltip(ButlerQuickSortPotion, "Potions");
    			
    			ButlerHudList = new HudList();
				ButlerHudList.ControlHeight = 16;	
				ButlerHudList.AddColumn(typeof(HudPictureBox), 15, null);
				ButlerHudList.AddColumn(typeof(HudStaticText), 175, null);
				ButlerHudList.AddColumn(typeof(HudPictureBox), 15, null);
				ButlerHudList.AddColumn(typeof(HudPictureBox), 15, null);
				ButlerHudList.AddColumn(typeof(HudPictureBox), 15, null);
				ButlerHudList.AddColumn(typeof(HudPictureBox), 15, null);
				ButlerHudList.AddColumn(typeof(HudStaticText), 1, null);
                ButlerHudTabLayout.AddControl(ButlerHudList, new Rectangle(0, 150, 300, 375));
								
				ButlerHudSelectedLabel = new HudStaticText();
                ButlerHudSelectedLabel.FontHeight = nmenuFontHeight;
				ButlerHudSelectedLabel.Text = "Items Selected: ";
				ButlerHudSelectedCount = new HudStaticText();
                ButlerHudSelectedCount.FontHeight = nmenuFontHeight;
				ButlerHudTabLayout.AddControl(ButlerHudSelectedLabel, new Rectangle(0,520,100,16));
				ButlerHudTabLayout.AddControl(ButlerHudSelectedCount, new Rectangle(110,520,150,16));

                ButlerPackSpacesAvailable = new HudStaticText();
                ButlerPackSpacesAvailable.FontHeight = nmenuFontHeight;
                ButlerPackSpaceAvailableLabel = new HudStaticText();
                ButlerPackSpaceAvailableLabel.FontHeight = nmenuFontHeight;
				ButlerPackSpaceAvailableLabel.Text = "Inventory status: ";
				ButlerHudTabLayout.AddControl(ButlerPackSpaceAvailableLabel, new Rectangle(0,540,100,16));
				ButlerHudTabLayout.AddControl(ButlerPackSpacesAvailable, new Rectangle(110,540,150,16));
				
				ButlerBurdenLabel = new HudStaticText();
                ButlerBurdenLabel.FontHeight = nmenuFontHeight;
				ButlerBurdenLabel.Text = "Current Burden: ";
                ButlerBurden = new HudStaticText();
                ButlerBurden.FontHeight = nmenuFontHeight;
                ButlerHudTabLayout.AddControl(ButlerBurdenLabel, new Rectangle(0, 560, 100, 16));
				ButlerHudTabLayout.AddControl(ButlerBurden, new Rectangle(110,560, 150, 16));
				
				if(ButlerHudPickCurrentSelection != null) {ButlerHudPickCurrentSelection.Hit += ButlerHudPickCurrentSelection_Hit;}
				ButlerHudUseCurrentSelection.Hit += ButlerHudUseCurrentSelection_Hit;
				ButlerHudDestoryCurrentSelection.Hit += ButlerHudDestoryCurrenSelection_Hit;
				ButlerHudSalvageCurrentSelection.Hit += ButlerHudSalvageCurrentSelection_Hit;
				
				ButlerQuickSortEquipped.StickyDownStateChanged += ButlerQuickSortEquipped_Hit;
    			ButlerQuickSortUnequipped.StickyDownStateChanged += ButlerQuickSortUnequipped_Hit;
    			ButlerQuickSortMelee.StickyDownStateChanged += ButlerQuickSortMelee_Hit;
    			ButlerQuickSortMissile.StickyDownStateChanged += ButlerQuickSortMissile_Hit;
    			ButlerQuickSortCaster.StickyDownStateChanged += ButlerQuickSortCaster_Hit;
    			ButlerQuickSortArmor.StickyDownStateChanged += ButlerQuickSortArmor_Hit;
    			ButlerQuickSortKeys.StickyDownStateChanged += ButlerQuickSortKeys_Hit;
    			ButlerQuickSortKeyrings.StickyDownStateChanged += ButlerQuickSortKeyrings_Hit;
    			ButlerQuickSortLockpicks.StickyDownStateChanged += ButlerQuickSortLockpicks_Hit;
    			ButlerQuickSortManastones.StickyDownStateChanged += ButlerQuickSortManastones_Hit;
    			ButlerQuickSortHealKit.StickyDownStateChanged += ButlerQuickSortHealKit_Hit;
    			ButlerQuickSortPotion.StickyDownStateChanged += ButlerQuickSortPotion_Hit;
    			
    			ButlerHudList.Click += (sender, row, col) => ButlerHudList_Click(sender, row, col);
				ButlerHudSearchButton.Hit += ButlerHudSearchButton_Click;
				ButlerHudClearSearchButton.Hit += ButlerHudClearSearchButton_Click;	
				
				//MaidTab    			
    			MaidTabLayout = new HudFixedLayout();
    			ButlerHudTabView.AddTab(MaidTabLayout, "Maid");
    			
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
    			
    			MaidTradeAllEightComps = new HudButton();
    			MaidTradeAllEightComps.Text = "Window L8 Components";
    			MaidTabLayout.AddControl(MaidTradeAllEightComps, new Rectangle(0, 180, 150,20));
    			
    			MaidCannibalizeEnable = new HudCheckBox();
    			MaidCannibalizeEnable.Text = "Enable Cannibalize Button";
    			MaidTabLayout.AddControl(MaidCannibalizeEnable, new Rectangle(0,210,150,20));
    			
    			MaidStackInventory.Hit += MaidStackInventory_Hit;
    			MaidRingKeys.Hit += MaidRingKeys_Hit;
    			MaidTradeAllSalvage.Hit += MaidTradeAllSalvage_Hit;
    			MaidTradeFilledSalvage.Hit += MaidTradeFilledSalvage_Hit;
    			MaidTradeParialSalvage.Hit += MaidTradeParialSalvage_Hit;
    			MaidSalvageCombine.Hit += MaidSalvageCombine_Hit;
    			MaidCannibalizeEnable.Hit += MaidCannibalizeEnable_Hit;
    			MaidTradeAllEightComps.Hit += MaidTradeAllEightComps_Hit;
    			
    			
    			//ValetTab
    			ValetTabLayout = new HudFixedLayout();
    			ButlerHudTabView.AddTab(ValetTabLayout, "Valet");
    			
    							int split3horizontal = Convert.ToInt32((double)GearButlerSettings.ButlerHudWidth /(double)3);
				int splithalf = Convert.ToInt32((double)GearButlerSettings.ButlerHudWidth/(double)2);
				int halfsplit3horizontal = Convert.ToInt32((double)split3horizontal/(double)2);
				int splitbottomvertical = Convert.ToInt32(((double)100 - GearButlerSettings.ButlerHudHeight) /2);
				
				
				ValetDisrobe = new HudButton();
				ValetDisrobe.Text = "Disrobe";
				ValetTabLayout.AddControl(ValetDisrobe, new Rectangle(10,5,split3horizontal-20,20));
				
				ValetEquipSuit = new HudButton();
				ValetEquipSuit.Text = "Equip";
				ValetTabLayout.AddControl(ValetEquipSuit, new Rectangle(splithalf - halfsplit3horizontal ,5,split3horizontal-20,20));
				
				ValetCreateSuit = new HudButton();
				ValetCreateSuit.Text = "Create";
				ValetTabLayout.AddControl(ValetCreateSuit, new Rectangle(splithalf + halfsplit3horizontal,5,split3horizontal-20,20));
				
				ValetTextBoxLabel = new HudStaticText();
				ValetTextBoxLabel.Text = "Suit Label:";
				ValetTabLayout.AddControl(ValetTextBoxLabel, new Rectangle(0,30,50,16));
				
				ValetNameBox = new HudTextBox();
				ValetNameBox.Text = String.Empty;
				ValetTabLayout.AddControl(ValetNameBox, new Rectangle(10,55,GearButlerSettings.ButlerHudWidth -20, 20));
				
				ValetSuitListLabel = new HudStaticText();
				ValetSuitListLabel.Text = "Suits:";
				ValetTabLayout.AddControl(ValetSuitListLabel, new Rectangle(0,80,50,16));			
	
				ValetSuitList = new HudList();
				ValetSuitList.AddColumn(typeof(HudPictureBox), 16, null);
				ValetSuitList.AddColumn(typeof(HudStaticText), GearButlerSettings.ButlerHudWidth - 80, null);
				ValetSuitList.AddColumn(typeof(HudPictureBox), 16, null);
				ValetSuitList.AddColumn(typeof(HudStaticText), 1, null);
				ValetTabLayout.AddControl(ValetSuitList, new Rectangle(0,100,GearButlerSettings.ButlerHudWidth - 20,100));
				
				ValetSuitPiecesListLabel = new HudStaticText();
				ValetSuitPiecesListLabel.Text = "Pieces:";
				ValetTabLayout.AddControl(ValetSuitPiecesListLabel, new Rectangle(0,210,50,16));	
				
				ValetSuitPiecesList = new HudList();
				ValetSuitPiecesList.AddColumn(typeof(HudPictureBox), 16, null);
				ValetSuitPiecesList.AddColumn(typeof(HudStaticText), GearButlerSettings.ButlerHudWidth - 80, null);
				ValetSuitPiecesList.AddColumn(typeof(HudPictureBox), 16, null);
				ValetSuitPiecesList.AddColumn(typeof(HudStaticText), 1, null);
				ValetTabLayout.AddControl(ValetSuitPiecesList, new Rectangle(0, 230 ,GearButlerSettings.ButlerHudWidth - 20,100));
				
				ValetDisrobe.Hit += ValetDisrobe_Hit;
				ValetEquipSuit.Hit += ValetEquipSuit_Hit;
				ValetCreateSuit.Hit += ValetCreateSuit_Hit;
				ValetSuitList.Click += ValetSuitList_Click;
				ValetSuitPiecesList.Click += ValetSuitPiecesList_Click;
    			
    			
    			
    			
    			
    			
    			
		
 				
               
                ButlerHudView.Resize += ButlerHudView_Resize; 
                ButlerHudView.VisibleChanged += ButlerHudView_VisibleChanged;

                UpdateButlerHudList();
                UpdateValetHud();
			  							
    		}catch(Exception ex) {LogError(ex);}
    		return;
    	}
		
		
		private void ButlerHudView_VisibleChanged(object sender, EventArgs e)
		{
			try
			{
				DisposeButlerHud();
			}catch(Exception ex){LogError(ex);}
		}
		
		
    	private void DisposeButlerHud()
    	{	
    		try
    		{
    			if(ButlerHudView == null) {return;}
    			
    			
				//Butler Hud
    			ButlerHudView.Resize -= ButlerHudView_Resize; 
    			ButlerHudView.VisibleChanged -= ButlerHudView_VisibleChanged;
    			
    			ValetTabLayout.Dispose();
    			MaidTabLayout.Dispose();
    			ButlerHudTabLayout.Dispose();
    			ButlerHudTabView.Dispose();
    			ButlerHudView.Dispose();
    			
    			
    			try{ButlerHudPickCurrentSelection.Hit -= ButlerHudPickCurrentSelection_Hit;}catch{}
    			ButlerHudUseCurrentSelection.Hit -= ButlerHudUseCurrentSelection_Hit;
				ButlerHudDestoryCurrentSelection.Hit -= ButlerHudDestoryCurrenSelection_Hit;
				ButlerHudSalvageCurrentSelection.Hit -= ButlerHudSalvageCurrentSelection_Hit;
				
				ButlerQuickSortEquipped.StickyDownStateChanged -= ButlerQuickSortEquipped_Hit;
    			ButlerQuickSortUnequipped.StickyDownStateChanged -= ButlerQuickSortUnequipped_Hit;
    			ButlerQuickSortMelee.StickyDownStateChanged -= ButlerQuickSortMelee_Hit;
    			ButlerQuickSortMissile.StickyDownStateChanged -= ButlerQuickSortMissile_Hit;
    			ButlerQuickSortCaster.StickyDownStateChanged -= ButlerQuickSortCaster_Hit;
    			ButlerQuickSortArmor.StickyDownStateChanged -= ButlerQuickSortArmor_Hit;
    			ButlerQuickSortKeys.StickyDownStateChanged -= ButlerQuickSortKeys_Hit;
    			ButlerQuickSortKeyrings.StickyDownStateChanged -= ButlerQuickSortKeyrings_Hit;
    			ButlerQuickSortLockpicks.StickyDownStateChanged -= ButlerQuickSortLockpicks_Hit;
    			ButlerQuickSortManastones.StickyDownStateChanged -= ButlerQuickSortManastones_Hit;
    			ButlerQuickSortHealKit.StickyDownStateChanged -= ButlerQuickSortHealKit_Hit;
    			ButlerQuickSortPotion.StickyDownStateChanged -= ButlerQuickSortPotion_Hit;
    			
    			ButlerHudList.Click -= (sender, row, col) => ButlerHudList_Click(sender, row, col);
				ButlerHudSearchButton.Hit -= ButlerHudSearchButton_Click;
				ButlerHudClearSearchButton.Hit -= ButlerHudClearSearchButton_Click;		
    			
    			
    			ButlerHudSalvageCurrentSelection.Dispose();
    			ButlerHudDestoryCurrentSelection.Dispose();
    			ButlerHudUseCurrentSelection.Dispose();
    			ButlerHudCurrentSelectionLabel.Dispose();
    			try{ButlerHudPickCurrentSelection.Dispose();}catch{}
    			ButlerHudCurrentSelectionIcon.Dispose();
    			ButlerHudCurrentSelectionText.Dispose();
    			ButlerHudSearchBox.Dispose();
    			ButlerHudSearchButton.Dispose();
    			ButlerHudClearSearchButton.Dispose();
    			ButlerQuickSortLabel.Dispose();
    			ButlerQuickSortEquipped.Dispose();
    			ButlerQuickSortUnequipped.Dispose();
    			ButlerQuickSortMelee.Dispose();
    			ButlerQuickSortMissile.Dispose();
    			ButlerQuickSortCaster.Dispose();
    			ButlerQuickSortArmor.Dispose();
    			ButlerQuickSortKeys.Dispose();
    			ButlerQuickSortKeyrings.Dispose();
    			ButlerQuickSortLockpicks.Dispose();
    			ButlerQuickSortManastones.Dispose();
    			ButlerQuickSortHealKit.Dispose();
    			ButlerQuickSortPotion.Dispose();
    			ButlerHudList.Dispose();
    			ButlerHudList = null;
    			ButlerHudSelectedLabel.Dispose();
    			ButlerPackSpacesAvailable.Dispose();
    			ButlerBurdenLabel.Dispose();
    			
    			//Maid Tab
    			MaidStackInventory.Hit -= MaidStackInventory_Hit;
    			MaidRingKeys.Hit -= MaidRingKeys_Hit;
    			MaidTradeAllSalvage.Hit -= MaidTradeAllSalvage_Hit;
    			MaidTradeFilledSalvage.Hit -= MaidTradeFilledSalvage_Hit;
    			MaidTradeParialSalvage.Hit -= MaidTradeParialSalvage_Hit;
    			MaidSalvageCombine.Hit -= MaidSalvageCombine_Hit;
    			MaidTradeAllEightComps.Hit -= MaidTradeAllEightComps_Hit;
    			if(MaidCannibalizeInventory != null) {MaidCannibalizeInventory.Hit -= MaidCannibalizeInventory_Hit;}
    			
    			if(MaidCannibalizeInventory != null){MaidCannibalizeInventory.Dispose();}
    			    			
    			MaidCannibalizeEnable.Dispose();
    			MaidSalvageCombine.Dispose();
    			MaidTradeParialSalvage.Dispose();
    			MaidTradeFilledSalvage.Dispose();
    			MaidTradeAllSalvage.Dispose();
    			MaidRingKeys.Dispose();
    			MaidStackInventory.Dispose();
    			
    			//Valet Tab
    			ValetDisrobe.Hit -= ValetDisrobe_Hit;
				ValetEquipSuit.Hit -= ValetEquipSuit_Hit;
				ValetCreateSuit.Hit -= ValetCreateSuit_Hit;
				ValetSuitList.Click -= ValetSuitList_Click;
				ValetSuitPiecesList.Click -= ValetSuitPiecesList_Click;
				
				ValetDisrobe.Dispose();
				ValetEquipSuit.Dispose();
				ValetCreateSuit.Dispose();
				ValetTextBoxLabel.Dispose();
				ValetNameBox.Dispose();				
				ValetSuitListLabel.Dispose();			
				ValetSuitList.Dispose();				
				ValetSuitPiecesListLabel.Dispose();				
				ValetSuitPiecesList.Dispose();
		
				ButlerHudView = null;    			
  			
    		}catch(Exception ex) {LogError(ex);}
    		return;
    	}
		
		
		
		
		
		private void GearButlerReadWriteSettings(bool read)
		{
			try
			{
				FileInfo GearButlerSettingsFile = new FileInfo(toonDir + @"\GearButler.xml");
								
				if (read)
				{
					try
					{
						if (!GearButlerSettingsFile.Exists)
		                {
		                    try
		                    {
		                    	GearButlerSettings = new ButlerSettings();
		                    	using (XmlWriter writer0 = XmlWriter.Create(GearButlerSettingsFile.ToString()))
								{
						   			XmlSerializer serializer0 = new XmlSerializer(typeof(ButlerSettings));
						   			serializer0.Serialize(writer0, GearButlerSettings);
						   			writer0.Close();
								}
		                    }
		                    catch (Exception ex) { LogError(ex); }
		                }
						
						using (XmlReader reader = XmlReader.Create(GearButlerSettingsFile.ToString()))
						{	
							XmlSerializer serializer = new XmlSerializer(typeof(ButlerSettings));
							GearButlerSettings = (ButlerSettings)serializer.Deserialize(reader);
							reader.Close();
						}
					}
					catch
					{
						GearButlerSettings = new ButlerSettings();
					}
				}
				
				if(!read)
				{
					if(GearButlerSettingsFile.Exists)
					{
						GearButlerSettingsFile.Delete();
					}
					
					using (XmlWriter writer = XmlWriter.Create(GearButlerSettingsFile.ToString()))
					{
			   			XmlSerializer serializer2 = new XmlSerializer(typeof(ButlerSettings));
			   			serializer2.Serialize(writer, GearButlerSettings);
			   			writer.Close();
					}
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		
		
		
		
		private void ButlerServerDispatch(object sender, Decal.Adapter.NetworkMessageEventArgs e)
        {
        	int iEvent = 0;
            try
            {
            	if(e.Message.Type == EchoConstants.AC_GAME_EVENT)
            	{
            		try
                    {
                    	iEvent = Convert.ToInt32(e.Message["event"]);
                    }
                    catch{}
                    if(iEvent == EchoConstants.GE_READY_PREV_ACTION_COMPLETE)
                    {
                    	if(MaidKeyList != null)
                    	{	
                    		if(MaidKeyList.Count > 0) 
                    		{
                    			if(MaidKeyToRing == MaidKeyList.First().Id)
                    			{
                    				MaidKeyList.RemoveAt(0);
                    			}
                    			MaidProcessRingKeys();
                    		}
                    	}
                    }
            	}
            }
            catch (Exception ex){LogError(ex);}
        }  
		
		
		
		private void ButlerDestroyed(object sender, ItemDestroyedEventArgs e)
		{
			try
			{
				UpdateButlerHudList();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void  ButlerReleased(object sender, ReleaseObjectEventArgs e)
		{
			try
			{
				UpdateButlerHudList();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ButlerItemSelected(object sender, ItemSelectedEventArgs e)
		{
			try
    		{	
				if(ButlerHudView == null) {return;}
				if(Core.WorldFilter[Core.Actions.CurrentSelection] != null)
				{		
					ButlerHudCurrentSelectionIcon.Clear();

					if(Core.WorldFilter[Core.Actions.CurrentSelection].Values(LongValueKey.IconUnderlay) != 0)
					{
						ButlerHudCurrentSelectionIcon.Add(CurrentSelectionRectangle, Core.WorldFilter[Core.Actions.CurrentSelection].Values(LongValueKey.IconUnderlay));
					}
					
					ButlerHudCurrentSelectionIcon.Add(CurrentSelectionRectangle, Core.WorldFilter[Core.Actions.CurrentSelection].Values(LongValueKey.Icon));
					
					if(Core.WorldFilter[Core.Actions.CurrentSelection].Values(LongValueKey.IconOverlay) != 0)
					{
						ButlerHudCurrentSelectionIcon.Add(CurrentSelectionRectangle, Core.WorldFilter[Core.Actions.CurrentSelection].Values(LongValueKey.IconOverlay));
					}
					
					                                  
					ButlerHudCurrentSelectionText.Text = Core.WorldFilter[Core.Actions.CurrentSelection].Name;
				}
				else
				{
					ButlerHudCurrentSelectionIcon.Clear();
					ButlerHudCurrentSelectionText.Text = "Nothing Selected";
				}
				UpdateButlerHudList();
    		}catch(Exception ex){LogError(ex);}
		}
		
		private void ButlerTradeOpened(object sender, EnterTradeEventArgs e)
		{
			try
			{
				bButlerTradeOpen = true;		
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ButlerTradeEnd(object sender, EndTradeEventArgs e)
		{
			try
			{	
				bButlerTradeOpen = false;
			}catch(Exception ex){LogError(ex);}

		}
				
    	

        private void ButlerHudView_Resize(object sender, System.EventArgs e)
        {
            try
            {
            	GearButlerSettings.ButlerHudWidth = ButlerHudView.Width;
	            GearButlerSettings.ButlerHudHeight = ButlerHudView.Height;
				GearButlerReadWriteSettings(false);
            }
            catch (Exception ex) { LogError(ex); }
        }
        
        private void AlterButlerHud()
        {
        	try
        	{
        		//TODO:  Make any adjustments to butler rendering here.
        	}catch(Exception ex){LogError(ex);}
        }
                  	
    	private void ButlerHudPickCurrentSelection_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			if(Core.WorldFilter[Core.Actions.CurrentSelection].ObjectClass == ObjectClass.Door || Core.WorldFilter[Core.Actions.CurrentSelection].ObjectClass == ObjectClass.Container)
    			{
    				WorldObject[] lockpicks = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Lockpick).OrderByDescending(x => x.Values(LongValueKey.LockpickSkillBonus)).ToArray();
    			
	    			if(lockpicks.Count() > 0)
	    			{
	    				Core.Actions.UseItem(lockpicks[0].Id, 1);
	    			}
	    			else
	    			{	
	    				WriteToChat("You are out of lockpicks!");
	    			}
    			}
    			if(Core.WorldFilter[Core.Actions.CurrentSelection].ObjectClass == ObjectClass.Misc)
    			{
    				WorldObject carvetool = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower() == "intricate carving tool").FirstOrDefault();
    				if(carvetool != null)
    				{
    					Core.Actions.UseItem(carvetool.Id, 1);
    				}
    				else
    				{
    					WriteToChat("No intricate carving tool!");
    				}
    				
    			}
    			UpdateButlerHudList();
    		}catch(Exception ex){LogError(ex);}
    	}   	
    	
    	private void ButlerHudUseCurrentSelection_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			if(Core.WorldFilter[Core.Actions.CurrentSelection].Values(LongValueKey.Unknown10) == 8)
    			{
    				Host.Actions.UseItem(Core.WorldFilter[Core.Actions.CurrentSelection].Id, 1);
    			}
    			else
    			{
					Host.Actions.UseItem(Core.WorldFilter[Core.Actions.CurrentSelection].Id, 0);
    			}
    			UpdateButlerHudList();
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void ButlerHudDestoryCurrenSelection_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			if(Core.WorldFilter[Core.Actions.CurrentSelection].Name.ToLower().Contains("aetheria"))
    			{
    				WorldObject[] dessicants = Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower() == "aetheria desiccant").ToArray();
    				if(dessicants.Count() > 0)
    				{
    					Core.Actions.UseItem(dessicants[0].Id, 1);
    				}
    				else
    				{
    					WriteToChat("Buy more aetheria dessicant!");
    				}
    			}
    			if(Core.WorldFilter[Core.Actions.CurrentSelection].Values(LongValueKey.CurrentMana) > 0)
    			{
    				WorldObject[] stones = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.ManaStone && x.Values(LongValueKey.IconOutline) == 0).OrderBy(x => x.Values(DoubleValueKey.ManaTransferEfficiency)).ToArray();
    				foreach(var stone in stones)
    				{
    					if(!UnchargedManaStones.Any(x => x.Id == stone.Id)) {UnchargedManaStones.Enqueue(stone);}
    				}
    				
    				if(UnchargedManaStones.Count > 0)
    				{
    					Core.Actions.UseItem(UnchargedManaStones.Dequeue().Id,1); 					
    				}
    				else
    				{
    					WriteToChat("No uncharged mana stones available!");
    				}
    				
    			}

    			UpdateButlerHudList();
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private DateTime ButlerLastAction = DateTime.MinValue;
    	private void ButlerHudSalvageCurrentSelection_Hit(object sender, System.EventArgs e)
    	{
    		try
    		{
    			if(Core.WorldFilter[Core.Actions.CurrentSelection].Values(DoubleValueKey.SalvageWorkmanship) > 0)
    			{
    				if(Core.WorldFilter.GetInventory().Any(x => x.Name.ToLower() == "ust"))
    				{
    					Core.Actions.UseItem(Core.WorldFilter.GetInventory().Where(x => x.Name.ToLower() == "ust").First().Id, 0);
	    				ButlerLastAction = DateTime.Now;
	    				Core.RenderFrame += RenderFrame_BulterSalvage;
    				}
    				else
    				{
    					WriteToChat("Character has no ust!");
    				}
    			}	
    			UpdateButlerHudList();
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void RenderFrame_BulterSalvage(object sender, EventArgs e)
    	{
    		try
    		{
	    		if((DateTime.Now - ButlerLastAction).TotalMilliseconds < 100) {return;}
	    		else
	    		{
	    			Core.RenderFrame -= RenderFrame_BulterSalvage;	
	    		}
	    		Core.Actions.SalvagePanelAdd(Core.WorldFilter[Core.Actions.CurrentSelection].Id);
		    	Core.Actions.SalvagePanelSalvage();
    		}catch(Exception ex){LogError(ex);}    	
    	}
    	
    	private void  ButlerHudSearchButton_Click(object sender, System.EventArgs e)
    	{
    		try
    		{
    			mButlerFilters.name = @ButlerHudSearchBox.Text.ToLower();
    			UpdateButlerHudList();
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void ButlerHudClearSearchButton_Click(object sender, System.EventArgs e)
    	{
    		try
    		{
    			ButlerHudSearchBox.Text = String.Empty;
    			mButlerFilters.name = String.Empty;
    			UpdateButlerHudList();
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void ButlerToggleFilters(string filtername, bool filteron)
    	{
    		try
    		{
    			mButlerFilters.melee = false;
				mButlerFilters.missile = false;
				mButlerFilters.caster = false;
				mButlerFilters.armor= false;
				mButlerFilters.keys = false;
				mButlerFilters.keyrings = false;
				mButlerFilters.healkits = false;
				mButlerFilters.lockpicks = false;
				mButlerFilters.manastones = false;
				mButlerFilters.potion = false;
    		
				if(filteron)
				{
	    			switch(filtername)
	    			{
	    				case "melee":
	    					mButlerFilters.melee = true;
	    					WriteToChat("Toggled " + filtername);
	    					break;
	    				case "missile":
	    					mButlerFilters.missile = true;
	    					WriteToChat("Toggled " + filtername);
	    					break;
	    				case "caster":
	    					mButlerFilters.caster = true;
	    					WriteToChat("Toggled " + filtername);
	    					break;
	    				case "armor":
	    					mButlerFilters.armor = true;
	    					WriteToChat("Toggled " + filtername);
	    					break;
	    				case "keys":
	    					mButlerFilters.keys = true;
	    					WriteToChat("Toggled " + filtername);
	    					break;
	    				case "keyrings":
	    					mButlerFilters.keyrings = true;
	    					WriteToChat("Toggled " + filtername);
	    					break;
	    				case "healkits":
	    					mButlerFilters.healkits = true;
	    					WriteToChat("Toggled " + filtername);
	    					break;
	    				case "lockpicks":
	    					mButlerFilters.lockpicks = true;
	    					WriteToChat("Toggled " + filtername);
	    					break;
	    				case "manastones":
	    					mButlerFilters.manastones = true;
	    					WriteToChat("Toggled " + filtername);
	    					break;
	   					case "potions":
	    					mButlerFilters.potion = true;
	    					WriteToChat("Toggled " + filtername);
	    					break;
	    			}
				}
    			UpdateButlerTumblers();
				    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	
    	private void UpdateButlerTumblers()
    	{
    		try
    		{			
    			ButlerQuickSortEquipped.StickyDown = mButlerFilters.equipped;
				ButlerQuickSortUnequipped.StickyDown = mButlerFilters.notquipped;
				ButlerQuickSortMelee.StickyDown = mButlerFilters.melee;
				ButlerQuickSortMissile.StickyDown = mButlerFilters.missile;
				ButlerQuickSortCaster.StickyDown = mButlerFilters.caster;
				ButlerQuickSortArmor.StickyDown = mButlerFilters.armor;
				ButlerQuickSortKeys.StickyDown = mButlerFilters.keys;
				ButlerQuickSortKeyrings.StickyDown = mButlerFilters.keyrings;
				ButlerQuickSortHealKit.StickyDown = mButlerFilters.healkits;
				ButlerQuickSortLockpicks.StickyDown = mButlerFilters.lockpicks;
				ButlerQuickSortManastones.StickyDown = mButlerFilters.manastones;
				ButlerQuickSortPotion.StickyDown = mButlerFilters.potion;
				
				UpdateButlerHudList();
    			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void ButlerQuickSortEquipped_Hit(object sender, bool down)
    	{
    		try
    		{
    			mButlerFilters.equipped = down;				
    			if(mButlerFilters.notquipped)
    			{
    				mButlerFilters.notquipped = false;
    			}
    			UpdateButlerTumblers();              			
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void ButlerQuickSortUnequipped_Hit(object sender, bool down)
    	{
    		try
    		{
    			mButlerFilters.notquipped = down;
    			if(mButlerFilters.equipped)
    			{
    				mButlerFilters.equipped = false;	
    			}   
				UpdateButlerTumblers();
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void ButlerQuickSortMelee_Hit(object sender, bool down)
    	{
    		try
    		{
    			ButlerToggleFilters("melee", down); 
    		}catch(Exception ex){LogError(ex);}
    	}
    	
    	private void ButlerQuickSortMissile_Hit(object sender, bool down)
    	{
    		try
    		{
    			ButlerToggleFilters("missile", down);
    		}catch(Exception ex){LogError(ex);}
    	}
    	    	
  		private void ButlerQuickSortCaster_Hit(object sender, bool down)
    	{
    		try
    		{
    			ButlerToggleFilters("caster", down); 
    		}catch(Exception ex){LogError(ex);}
    	}
  		
  		private void ButlerQuickSortArmor_Hit(object sender, bool down)
    	{
    		try
    		{
    			ButlerToggleFilters("armor", down);
    		}catch(Exception ex){LogError(ex);}
    	}
  			
  		private void  ButlerQuickSortKeys_Hit(object sender, bool down)
    	{
    		try
    		{
    			ButlerToggleFilters("keys", down);
    		}catch(Exception ex){LogError(ex);}
    	}
  		
  		private void ButlerQuickSortKeyrings_Hit(object sender, bool down)
    	{
    		try
    		{
    			ButlerToggleFilters("keyrings", down);
    		}catch(Exception ex){LogError(ex);}
    	}
  		
  		private void ButlerQuickSortLockpicks_Hit(object sender, bool down)
    	{
    		try
    		{
    			ButlerToggleFilters("lockpicks", down);
    		}catch(Exception ex){LogError(ex);}
    	}
  		
  		private void ButlerQuickSortManastones_Hit(object sender, bool down)
    	{
    		try
    		{
    			ButlerToggleFilters("manastones", down);
    		}catch(Exception ex){LogError(ex);}
    	}
  		
  		private void ButlerQuickSortHealKit_Hit(object sender, bool down)
    	{
    		try
    		{
    			ButlerToggleFilters("healkits", down);
    		}catch(Exception ex){LogError(ex);}
    	}
  		
  		private void ButlerQuickSortPotion_Hit(object sender, bool down)
    	{
    		try
    		{
    			ButlerToggleFilters("potions", down);
    		}catch(Exception ex){LogError(ex);}
    	}
  		    	
    	private void UpdateButlerHudList()
	    {  	
	    	try
	    	{ 
	    		if (ButlerHudView == null) {return;}
	    		int scroll = ButlerHudList.ScrollPosition;
	    		

	    	    
	    	    List<WorldObject> ButlerInventory = new List<WorldObject>();
	    	    
	    	    if(mButlerFilters.name != String.Empty)
	    	    {
	    	    	ButlerInventory = Core.WorldFilter.GetInventory().Where(x => @x.Name.ToLower().Contains(@mButlerFilters.name)).OrderBy(x => @x.Name).ToList();
	    	    }
	    	    else
	    	    {
	    	    	ButlerInventory = Core.WorldFilter.GetInventory().OrderBy(x => @x.Name).ToList();
	    	    }
	    	    
	    	    if(mButlerFilters.equipped)
	    	    {
	    	    	ButlerInventory.RemoveAll(x => x.Values(LongValueKey.EquippedSlots) == 0);
	    	    }
	    	    else if(mButlerFilters.notquipped)
	    	    {
	    			ButlerInventory.RemoveAll(x => x.Values(LongValueKey.EquippedSlots) !=  0);
					ButlerInventory.RemoveAll(x => x.Values(LongValueKey.Unknown10) ==  56);    
	    	    }
	    	    
	    	    if(mButlerFilters.melee) ButlerInventory.RemoveAll(x => x.ObjectClass != ObjectClass.MeleeWeapon);
	    	    else if(mButlerFilters.missile) ButlerInventory.RemoveAll(x => x.ObjectClass != ObjectClass.MissileWeapon);
	    	    else if(mButlerFilters.caster) ButlerInventory.RemoveAll(x => x.ObjectClass != ObjectClass.WandStaffOrb);
	    	    else if(mButlerFilters.armor) ButlerInventory.RemoveAll(x => x.ObjectClass != ObjectClass.Armor);
	    	    else if(mButlerFilters.keys) ButlerInventory.RemoveAll(x => x.ObjectClass != ObjectClass.Key);
	    	    else if(mButlerFilters.keyrings) ButlerInventory.RemoveAll(x => !x.Name.ToLower().Contains("keyring"));
	    	    else if(mButlerFilters.manastones) ButlerInventory.RemoveAll(x => x.ObjectClass != ObjectClass.ManaStone);
	    	    else if(mButlerFilters.healkits) ButlerInventory.RemoveAll(x => x.ObjectClass != ObjectClass.HealingKit);
	    	    else if(mButlerFilters.potion) ButlerInventory.RemoveAll(x => x.ObjectClass != ObjectClass.Food);
	    	    else if(mButlerFilters.lockpicks) ButlerInventory.RemoveAll(x => x.ObjectClass != ObjectClass.Lockpick);
				
	    	    
	    			    		
	    	    ButlerHudList.ClearRows();
	    	    foreach(WorldObject wo in ButlerInventory)
	    	    {
	    	    	ButlerHudListRow = ButlerHudList.AddRow();
	    	    	
	    	    	((HudPictureBox)ButlerHudListRow[0]).Image = wo.Icon + 0x6000000;
                    ((HudStaticText)ButlerHudListRow[1]).FontHeight = nitemFontHeight;
                    ((HudStaticText)ButlerHudListRow[1]).Text = wo.Name;
                    
                    if(wo.Values(LongValueKey.EquippedSlots) > 0 || wo.Values(LongValueKey.Unknown10) == 56)
	    	    	{
                    	((HudStaticText)ButlerHudListRow[1]).TextColor = Color.Gold;
	    	    	}
  
                    if (wo.Id == Core.Actions.CurrentSelection)
	    	    	{
	    	    		((HudPictureBox)ButlerHudListRow[0]).Image = 0x6006119;
                        ((HudStaticText)ButlerHudListRow[1]).TextColor = Color.Red;
                        ((HudStaticText)ButlerHudListRow[1]).FontHeight = nitemFontHeight;
                    }
	    	    	
	    	    	
                    ((HudPictureBox)ButlerHudListRow[2]).Image = GearGraphics.GB_UNEQUIPPED_ICON;
	    	    	((HudPictureBox)ButlerHudListRow[3]).Image = GearGraphics.GB_USE_ICON;
	    	    	((HudPictureBox)ButlerHudListRow[4]).Image = GearGraphics.GB_GIVE_ICON;
	    	    	((HudPictureBox)ButlerHudListRow[5]).Image = GearGraphics.GB_TRADEVENDOR_ICON;  
	    	    	((HudStaticText)ButlerHudListRow[6]).Text = wo.Id.ToString();
	    	    }
	    	    
	    	    ButlerHudList.ScrollPosition = scroll;
	    	    
	    	   	ButlerHudSelectedCount.Text = Core.WorldFilter.GetInventory().Count.ToString();
	    	    ButlerBurden.Text = Core.CharacterFilter.Burden.ToString() + "%";
	    	    if(Core.CharacterFilter.Burden < 100){ButlerBurden.TextColor = Color.Green;}
	    	    if(Core.CharacterFilter.Burden >= 100){ButlerBurden.TextColor = Color.Yellow;}
	    	    if(Core.CharacterFilter.Burden >= 200){ButlerBurden.TextColor = Color.Red;}
	    	    ButlerPackSpacesAvailable.Text = CalculateAvailableSpace();

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
    	
    	private HudList.HudListRowAccessor ButlerRow;
    	private void ButlerHudList_Click(object sender, int row, int col)
    	{
    		try
			{
    			ButlerRow = ButlerHudList[row];
    			int ItemGuid = Convert.ToInt32(((HudStaticText)ButlerRow[6]).Text);
    			LootObject lo = new LootObject(Core.WorldFilter[ItemGuid]);
    			
    			if(col == 0)
    			{
    				Host.Actions.SelectItem(lo.Id);
    			}
    			if(col == 1)
    			{
    				try
    				{
    					if(GISettings.GSStrings){HudToChat(lo.GSReportString(), 2);}
    					if(GISettings.AlincoStrings){HudToChat(lo.LinkString(),2);}
    				}catch{}
    			}
    			if(col == 2)
    			{
    				Core.Actions.MoveItem(lo.Id, Core.CharacterFilter.Id, 0, false);
    			}
    			if(col == 3)
    			{    				
    				if(lo.LValue(LongValueKey.Unknown10) == 8)
    				{
    					if(!lo.Name.Contains("Mana Stone") || !lo.Name.Contains("Dessicant"))
    					{
    						Host.Actions.UseItem(lo.Id, 1);
    					}
    				}
    				else
    				{
						Host.Actions.UseItem(lo.Id, 0);
    				}
    			}
    			if(col == 4)
    			{
    				if(Core.WorldFilter[Core.Actions.CurrentSelection].ObjectClass == ObjectClass.Npc ||  Core.WorldFilter[Core.Actions.CurrentSelection].ObjectClass == ObjectClass.Player)
    				{
    				   if(lo.LValue(LongValueKey.EquippedSlots) > 0 || lo.LValue(LongValueKey.Unknown10) == 56)
    				   {
	    				   	WriteToChat("Unequip the item first.");	   	
    				   }		
    				   else
    				   {
    				   		Host.Actions.GiveItem(lo.Id, Host.Actions.CurrentSelection);
    				   }
    				}
    				else
    				{
    					WriteToChat("First select an NPC, Player, or yourself.");
    				}
    			}
    			if(col == 5)
    			{
    				if(bButlerTradeOpen)
    				{
    					Core.Actions.TradeAdd(lo.Id);
    				}
    				else if(Core.WorldFilter.OpenVendor.MerchantId != 0)
    				{
    				   if(lo.LValue(LongValueKey.EquippedSlots) > 0 || lo.LValue(LongValueKey.Unknown10) == 56)
    				   {
	    				   	WriteToChat("Unequip the item first.");	   	
    				   }		
    				   else
    				   {
    				   		Core.Actions.VendorAddSellList(lo.Id);
    				   }
    				}
    			}
			}
			catch (Exception ex) { LogError(ex); }
			return;			
    	}
				
			private void ButlerTimerDo(object sender, System.EventArgs e)
			{
				try
				{
					if(ValetRemoveList.Count > 0) {
						ValetProcessRemove();
						return;
					}
					else if(ValetEquipList.Count > 0) {
						ValetProcessEquip();
						return;
					}	
					else if(MaidCannibalizeQueue.Count > 0){
						MaidProcessCannibalize();
						return;
					}
					
				}catch(Exception ex){LogError(ex);}
			}
    		
	}
}



