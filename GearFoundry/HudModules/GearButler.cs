
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

		List<WorldObject> ButlerInventory;
		private bool bBulterTradeOpen = false;
		
		private static int GB_USE_ICON = 0x6000FB7;
		private static int GB_GIVE_ICON = 0x60011F7;
		private static int GB_TRADEVENDOR_ICON = 0x6001080;
		private static int GB_EQUIPPED_ICON = 0x600136F;
				
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
		
		//CurrentSelection:
		//Use
		//Drop
		//
		
		private HudTextBox ButlerHudSearchBox = null;
		private HudStaticText ButlerPackSpacesAvailable = null;
		private HudStaticText ButlerBurden = null;
		private HudList.HudListRowAccessor ButlerHudListRow = null;
		
		
		private void SubscribeButlerEvents()
		{
			try
			{
				Core.CharacterFilter.LoginComplete += new EventHandler(ButlerLoginComplete);
				Core.ItemSelected += new EventHandler<ItemSelectedEventArgs>(ButlerItemSelected);
				Core.WorldFilter.EnterTrade += new EventHandler<EnterTradeEventArgs>(ButlerTradeOpened);
				Core.WorldFilter.EndTrade += new EventHandler<EndTradeEventArgs>(ButlerTradeEnd);
			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeButlerEvents()
		{
			try
			{
				Core.CharacterFilter.LoginComplete -= new EventHandler(ButlerLoginComplete);
				Core.ItemSelected -= new EventHandler<ItemSelectedEventArgs>(ButlerItemSelected);
				Core.WorldFilter.EnterTrade -= new EventHandler<EnterTradeEventArgs>(ButlerTradeOpened);
				Core.WorldFilter.EndTrade -= new EventHandler<EndTradeEventArgs>(ButlerTradeEnd);
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
    			ButlerHudCurrentSelectionIcon.Image = Core.WorldFilter[e.ItemGuid].Icon;
    			ButlerHudCurrentSelectionText.Text = Core.WorldFilter[e.ItemGuid].Name; 
    			UpdateButlerHudList();
    		}catch{}
		}
		
		private void ButlerTradeOpened(object sender, EnterTradeEventArgs e)
		{
			try
			{
				bBulterTradeOpen = true;		
			}catch{}
		}
		
		private void ButlerTradeEnd(object sender, EndTradeEventArgs e)
		{
			try
			{
				bBulterTradeOpen = false;
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
    			
    			ButlerHudView = new HudView("GearButler", 300, 600, new ACImage(0x1F88));
    			ButlerHudView.Theme = VirindiViewService.HudViewDrawStyle.GetThemeByName("Minimalist Transparent");
    			ButlerHudView.UserAlphaChangeable = false;
    			ButlerHudView.ShowInBar = false;
    			ButlerHudView.Visible = true;
                ButlerHudView.UserClickThroughable = true;
    			
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
    			ButlerHudTabLayout.AddControl(ButlerHudSearchBox, new Rectangle(0,80,200,20));
    			
    			ButlerHudSearchButton = new HudButton();
    			ButlerHudSearchButton.Text = "Search";
    			ButlerHudTabLayout.AddControl(ButlerHudSearchButton, new Rectangle(205,80,40,20));
    			
    			ButlerHudClearSearchButton = new HudButton();
    			ButlerHudClearSearchButton.Text = "Reset";
    			ButlerHudTabLayout.AddControl(ButlerHudClearSearchButton, new Rectangle(250,80,40,20));
    			

    			
				ButlerHudList = new HudList();
				ButlerHudLayout.AddControl(ButlerHudList, new Rectangle(0,120,300,400));
				ButlerHudList.ControlHeight = 16;	
				ButlerHudList.AddColumn(typeof(HudPictureBox), 15, null);
				ButlerHudList.AddColumn(typeof(HudStaticText), 175, null);
				ButlerHudList.AddColumn(typeof(HudPictureBox), 15, null);
				ButlerHudList.AddColumn(typeof(HudPictureBox), 15, null);
				ButlerHudList.AddColumn(typeof(HudPictureBox), 15, null);
				ButlerHudList.AddColumn(typeof(HudPictureBox), 15, null);						
								
				ButlerHudList.Click += (sender, row, col) => ButlerHudList_Click(sender, row, col);
				ButlerHudSearchBox.LostFocus += (sender, e) => ButlerHudSearchBox_End(sender, e);
				ButlerHudSearchButton.Hit += (sender, e) => ButlerHudSearchButton_Click(sender, e);
				ButlerHudClearSearchButton.Hit += (sender, e) => ButlerHudClearSearchButton_Click(sender, e);	
			  							
    		}catch(Exception ex) {LogError(ex);}
    		return;
    	}
    	
    	private void DisposeButlerHud()
    	{	
    		try
    		{
    			ButlerHudList.Click -= (sender, row, col) => ButlerHudList_Click(sender, row, col);				
				ButlerHudSearchBox.LostFocus -= (sender, e) => ButlerHudSearchBox_End(sender, e);
				ButlerHudSearchButton.Hit -= (sender, e) => ButlerHudSearchButton_Click(sender, e);
				ButlerHudClearSearchButton.Hit -= (sender, e) => ButlerHudClearSearchButton_Click(sender, e);

    			ButlerHudList.Dispose();
    			ButlerHudClearSearchButton.Dispose();
    			ButlerHudSearchButton.Dispose();
    			ButlerHudTabLayout.Dispose();
    			ButlerHudTabView.Dispose(); 		
				ButlerHudLayout.Dispose();
    			ButlerHudView.Dispose();		
    		}catch(Exception ex) {LogError(ex);}
    		return;
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
    			ButlerInventory = Core.WorldFilter.GetInventory().OrderBy(x => x.Name).ToList();
    			UpdateButlerHudList();
    		}catch{}	
    	}
    	
    	private void UpdateButlerHudList()
	    {  	
	    	try
	    	{    		
	    		ButlerHudList.ClearRows();	  	    		

	    	    foreach(WorldObject wo in ButlerInventory)
	    	    {
	    	    	ButlerHudListRow = ButlerHudList.AddRow();
	    	    	
	    	    	((HudPictureBox)ButlerHudListRow[0]).Image = wo.Icon + 0x6000000;
	    	    	((HudStaticText)ButlerHudListRow[1]).Text = wo.Name;
	    	    	if(wo.Values(LongValueKey.EquippedSlots) > 0) {((HudPictureBox)ButlerHudListRow[2]).Image = GB_EQUIPPED_ICON;}
	    	    	
	    	    	((HudPictureBox)ButlerHudListRow[3]).Image = GB_USE_ICON;
	    	    	((HudPictureBox)ButlerHudListRow[4]).Image = GB_GIVE_ICON;
	    	    	((HudPictureBox)ButlerHudListRow[5]).Image = GB_TRADEVENDOR_ICON;    	    	
	    	    }

	    	}catch(Exception ex){LogError(ex);}
	    	return;	    	
	    }
    	
    	private void ButlerHudSearchBox_End(object sender, System.EventArgs e)
    	{
    		try
    		{
    			string ButlerSearchString = ButlerHudSearchBox.Text;
    			WriteToChat(ButlerSearchString);
    		}catch{}
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
    					Host.Actions.UseItem(ButlerInventory[row].Id, 0);
    				}
    				else
    				{
    					if(!ButlerInventory[row].Name.Contains("Mana Stone") || !ButlerInventory[row].Name.Contains("Dessicant"))
    					{
    						Host.Actions.UseItem(ButlerInventory[row].Id, 1);
    					}
    				}
    			}
    			if(col == 4)
    			{
    				Host.Actions.GiveItem(ButlerInventory[row].Id, Host.Actions.CurrentSelection);
    			}
    			if(col == 5)
    			{
    				if(bBulterTradeOpen)
    				{
    					Core.Actions.TradeAdd(ButlerInventory[row].Id);
    				}
    				else if(Core.WorldFilter.OpenVendor.MerchantId != 0)
    				{
    					Core.Actions.VendorAddSellList(ButlerInventory[row].Id);
    				}
    			}
				
			}
			catch (Exception ex) { LogError(ex); }
			return;			
    	}
    		
	}
}


//UNKNOWN10 == 8 specifies single use item
//UNKNKNOWN10 != 8 are items that have to be used on something else

//[VTank] ObjectClass: Gem
//[VTank] (S) Name: Spirit of Izexi Gem
//[VTank] (I) Unknown10: 8
//[VTank] (I) UsageMask: 32768
//[VTank] (I) IconOutline: 1
//[VTank] (I) StackCount: 6
//[VTank] (I) StackMax: 100
//[VTank] (I) Container: 1342600506
//[VTank] (I) PhysicsDataFlags: 137217


//[VTank] ObjectClass: ManaStone
//[VTank] (S) Name: Major Mana Stone
//[VTank] (I) Unknown10: 655368
//[VTank] (I) UsageMask: 35103
//[VTank] (I) Container: 1342600506
//[VTank] (I) PhysicsDataFlags: 131073

//[VTank] (S) Name: Encapsulated Spirit
//[VTank] (I) CreateFlags1: 2650136
//[VTank] (I) Behavior: 16
//[VTank] (I) Value: 5000
//[VTank] (I) Unknown10: 524296
//[VTank] (I) UsageMask: 128
//[VTank] (I) PhysicsDataFlags: 399361

//[VTank] (S) Name: Lightning Spectre Essence (150)
//[VTank] (I) Unknown10: 8
//[VTank] (I) UsageMask: 16
//[VTank] (I) IconOutline: 64
//[VTank] (I) UsesRemaining: 50
//[VTank] (I) UsesTotal: 50
//[VTank] (I) PhysicsDataFlags: 137345



