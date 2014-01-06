using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VirindiViewService;
using VirindiViewService.Controls;
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
using System.Reflection;

namespace GearFoundry
{

    public partial class PluginCore : PluginBase
    {
    	private List<PortalIcons> MiscRecallList = new List<PortalIcons>();
    	private List<PortalIcons> TextRecallList = new List<PortalIcons>();
    	private List<PortalIcons> PortalSpellList = new List<PortalIcons>();
    	private List<PortalIcons> RecallSpellList = new List<PortalIcons>();
    	private List<string> MissingSpellsList = new List<string>();
    	
    	internal class PortalIcons
    	{
    		internal HudPictureBox PortalIcon = new HudPictureBox();
    		internal string Identifier = String.Empty;
    	}
    	    	
        XDocument xdocPortalGear = null;

        private PortalGearSettings mDynamicPortalGearSettings = new PortalGearSettings();
        
        public class PortalGearSettings
        {
        	public int nOrbGuid = 0;
        	public int nOrbIcon = 0x2A38;
        	public int nFacilityHubGemID = 0;
        }
       
      

     	
     	public enum RecallTypes
		{
			none,
			portal,
			lifestone,
			primaryporal,
			summonprimary,
			secondaryportal,
			summonsecondary,
            sanctuary,
            bananaland,
            col,
            aerlinthe,
            mhoire,
            neftet,
            rynthid,
            gearknight,
            caul,
            bur,
            olthoi_north,
            facilityhub,
            lifestonetie,
            tieprimary,
            tiesecondary,
            lethe,
            ulgrim,
            candeth,
            glendenwood
		}
		
		private void SubscribePortalEvents()
		{
			try
			{
				if (!File.Exists(portalGearFilename))
                {
                    savePortalSettings();
                }
				
				xdocPortalGear = XDocument.Load(portalGearFilename);
				
	            if(xdocPortalGear.Root.Element("Setting") == null)
	            {
	            	savePortalSettings();
	            	xdocPortalGear = XDocument.Load(portalGearFilename);
	            }
	            
    	        Int32.TryParse(xdocPortalGear.Root.Element("Setting").Element("OrbGuid").Value, out mDynamicPortalGearSettings.nOrbGuid);
    	        Int32.TryParse(xdocPortalGear.Root.Element("Setting").Element("OrbIcon").Value, out mDynamicPortalGearSettings.nOrbIcon);
    	        Int32.TryParse(xdocPortalGear.Root.Element("Setting").Element("FacilityHubGemID").Value, out mDynamicPortalGearSettings.nFacilityHubGemID);   
    	        
    	        if(mDynamicPortalGearSettings.nFacilityHubGemID == 0)
	             {
	             	if(Core.WorldFilter.GetInventory().Where(x => x.Name.Contains("Facility Hub Portal Gem")).Count() > 0)
	             	{
	             		mDynamicPortalGearSettings.nFacilityHubGemID = Core.WorldFilter.GetInventory().Where(x => x.Name.Contains("Facility Hub Portal Gem")).First().Id;
	             		savePortalSettings();
	             	}
	             }
    	        
    	        if(mDynamicPortalGearSettings.nOrbGuid == 0)
    	        {
    	        	SelectDefaultCaster();
    	        }
				
				Core.CharacterFilter.Logoff += PortalGear_Logoff;
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void savePortalSettings()
        {
           try
           {
                XDocument xdocPortalSettings = new XDocument(new XElement("Settings"));
                xdocPortalSettings.Element("Settings").Add(new XElement("Setting",
                        new XElement("OrbGuid", mDynamicPortalGearSettings.nOrbGuid),
                         new XElement("OrbIcon", mDynamicPortalGearSettings.nOrbIcon),
                        new XElement("FacilityHubGemID",mDynamicPortalGearSettings.nFacilityHubGemID)));
                xdocPortalSettings.Save(portalGearFilename);
            }
            catch (Exception ex) { LogError(ex); }
        }
		
		private void SelectDefaultCaster()
		{
			try
			{
				if(mDynamicPortalGearSettings.nOrbGuid == 0)
        		{
        			mDynamicPortalGearSettings.nOrbGuid = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.WandStaffOrb && 
        			           (x.Values(LongValueKey.WieldReqValue) == 0 || (x.Values(LongValueKey.WieldReqValue) == 150 && Core.CharacterFilter.Level >= 150) ||
        			           (x.Values(LongValueKey.WieldReqValue) == 180 && Core.CharacterFilter.Level >= 180))).ToList().OrderByDescending(x => x.Values(DoubleValueKey.MeleeDefenseBonus)).First().Id;
        			
        			mDynamicPortalGearSettings.nOrbIcon = Core.WorldFilter[mDynamicPortalGearSettings.nOrbGuid].Icon;
                }
				savePortalSettings();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void PortalGear_Logoff(object sender, EventArgs e)
		{
			DisposePortalGearHud();
			UnsubscribePortalEvents();
		}
		
		private void UnsubscribePortalEvents()
		{
			try
			{
				savePortalSettings();
				Core.CharacterFilter.Logoff -= PortalGear_Logoff;
			}catch(Exception ex){LogError(ex);}
		}
				
		private HudView DynamicPortalGearView = null;
		private HudTabView DynamicPortalGearTabView = null;
		private HudFixedLayout DynamicPortalGearLayout = null;
		private HudFixedLayout DynamicPortalGearMissingLayout = null;
		private HudList DynaicPortalGearMissingSpellsList = null;

        private void RenderPortalGearHud()
        {
            try
            {
                if (DynamicPortalGearView != null)
                {
                	DisposePortalGearHud();
                }
                
                AssembleRecallSpellsList();
                
                int PortalHudWidth = 0;
                if(TextRecallList.Count + PortalSpellList.Count + MiscRecallList.Count > RecallSpellList.Count)
                {
                	PortalHudWidth = (TextRecallList.Count + PortalSpellList.Count + MiscRecallList.Count) * 30;
                }
                else
                {
                	PortalHudWidth = (RecallSpellList.Count) * 30;
                }
                int PortalHudHeight = 70;
                if(RecallSpellList.Count == 0)
                {
                	PortalHudHeight = 35;
                }
                
                DynamicPortalGearView = new VirindiViewService.HudView("", PortalHudWidth, PortalHudHeight, new ACImage(0x6AA2), false, "PortalGear");
 				DynamicPortalGearView.ShowInBar = false;
                DynamicPortalGearView.UserAlphaChangeable = false;
                DynamicPortalGearView.Visible = true;
                DynamicPortalGearView.UserClickThroughable = false;
                DynamicPortalGearView.UserGhostable = true;
                DynamicPortalGearView.UserMinimizable = true;
                DynamicPortalGearView.UserResizeable = false;
                DynamicPortalGearView.LoadUserSettings();
                
                DynamicPortalGearTabView = new HudTabView();
                DynamicPortalGearView.Controls.HeadControl = DynamicPortalGearTabView;
                
                DynamicPortalGearLayout = new HudFixedLayout();
                DynamicPortalGearTabView.AddTab(DynamicPortalGearLayout, "Portals");
                
               	int shiftbase = 0;	
               	for(int i = 0; i < MiscRecallList.Count; i++)
                {
                	DynamicPortalGearLayout.AddControl(MiscRecallList[i].PortalIcon, new Rectangle(i*30, 0, 25, 25));
                	if(MiscRecallList[i].Identifier == "caster") VirindiViewService.TooltipSystem.AssociateTooltip(MiscRecallList[i].PortalIcon, "Select Caster");
                	if(MiscRecallList[i].Identifier == "hubgem") VirindiViewService.TooltipSystem.AssociateTooltip(MiscRecallList[i].PortalIcon, "Facility Hub Gem");
                }
               	shiftbase = MiscRecallList.Count * 30;
               	
                //Text based recall Tiles
                for(int i = 0; i < TextRecallList.Count; i++)
                {
                	DynamicPortalGearLayout.AddControl(TextRecallList[i].PortalIcon, new Rectangle(shiftbase + i * 30, 0, 25, 25));
                	if(TextRecallList[i].Identifier == "atlifestone") VirindiViewService.TooltipSystem.AssociateTooltip(TextRecallList[i].PortalIcon, "Lifestone Recall (/@ls)");
                	if(TextRecallList[i].Identifier == "athouse") VirindiViewService.TooltipSystem.AssociateTooltip(TextRecallList[i].PortalIcon, "House Recall (/@hr)");
                	if(TextRecallList[i].Identifier == "atmansion") VirindiViewService.TooltipSystem.AssociateTooltip(TextRecallList[i].PortalIcon, "Mansion recall (/@hom)");
                	if(TextRecallList[i].Identifier == "atallegiance") VirindiViewService.TooltipSystem.AssociateTooltip(TextRecallList[i].PortalIcon, "Allegiance Hometown (/@ah)");
                	if(TextRecallList[i].Identifier == "atmarket") VirindiViewService.TooltipSystem.AssociateTooltip(TextRecallList[i].PortalIcon, "Marketplace (/@mp)");
                }
                shiftbase += TextRecallList.Count * 30;
                
                //Portal spell tiles
                if(PortalSpellList.Count > 0)
                {
	                for(int i = 0; i < PortalSpellList.Count; i++)
	                {
	                	DynamicPortalGearLayout.AddControl(PortalSpellList[i].PortalIcon, new Rectangle(shiftbase + i * 30, 0, 25, 25));
	                	
	                	if(PortalSpellList[i].Identifier == "portalrecall") VirindiViewService.TooltipSystem.AssociateTooltip(PortalSpellList[i].PortalIcon, "Portal Recall");
	                	if(PortalSpellList[i].Identifier == "lifestonetie") VirindiViewService.TooltipSystem.AssociateTooltip(PortalSpellList[i].PortalIcon, "Lifestone Tie");
	                	if(PortalSpellList[i].Identifier == "lifestonerecall") VirindiViewService.TooltipSystem.AssociateTooltip(PortalSpellList[i].PortalIcon, "Lifestone Recall");
	                	if(PortalSpellList[i].Identifier == "tieportalone")  VirindiViewService.TooltipSystem.AssociateTooltip(PortalSpellList[i].PortalIcon, "Tie Portal I");
	        			if(PortalSpellList[i].Identifier == "recallportalone") VirindiViewService.TooltipSystem.AssociateTooltip(PortalSpellList[i].PortalIcon, "Recall Portal I");
	        			if(PortalSpellList[i].Identifier == "summonportalone") VirindiViewService.TooltipSystem.AssociateTooltip(PortalSpellList[i].PortalIcon, "Summon Portal I");
	        			if(PortalSpellList[i].Identifier == "tieportaltwo")  VirindiViewService.TooltipSystem.AssociateTooltip(PortalSpellList[i].PortalIcon, "Tie Portal II");
	        			if(PortalSpellList[i].Identifier == "recallportaltwo") VirindiViewService.TooltipSystem.AssociateTooltip(PortalSpellList[i].PortalIcon, "Recall Portal II");
	        			if(PortalSpellList[i].Identifier == "summonportaltwo") VirindiViewService.TooltipSystem.AssociateTooltip(PortalSpellList[i].PortalIcon, "Summon Portal II");
	                } 
                }
                

                //Recall spell tiles
                if(RecallSpellList.Count > 0)
                {
                	for(int i = 0; i < RecallSpellList.Count; i++)
                	{
                		DynamicPortalGearLayout.AddControl(RecallSpellList[i].PortalIcon, new Rectangle(i * 30, 30, 25, 25));
                		if(RecallSpellList[i].Identifier == "sanctuary") VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "Sanctuary Recall");
                		if(RecallSpellList[i].Identifier == "bananaland") VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "BananaLand Recall");
                		if(RecallSpellList[i].Identifier == "colo")  VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "Coloseum Recall");
                		if(RecallSpellList[i].Identifier == "aerlinthe") VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "Aerlinthe Recall");
                		if(RecallSpellList[i].Identifier == "caul") VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "Singularity Caul Recall");
                		if(RecallSpellList[i].Identifier == "bur") VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "Bur Recall");
                		if(RecallSpellList[i].Identifier == "olthoi") VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "Olthoi North Recall");
                		if(RecallSpellList[i].Identifier == "facility")  VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "Facility Recall");
                		if(RecallSpellList[i].Identifier == "gearknight") VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "GearKnight Resistance Camp Recall");
                		if(RecallSpellList[i].Identifier == "neftet") VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "Neftet Recall");
                		if(RecallSpellList[i].Identifier == "rynthid")  VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "Rynthid Recall");
                		if(RecallSpellList[i].Identifier == "mhoire") VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "Mhoire Recall");
                		if(RecallSpellList[i].Identifier == "glendenwood") VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "Glendenwood Recall");
                		if(RecallSpellList[i].Identifier == "mtlethe") VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "Mount Lethe Recall");
                		if(RecallSpellList[i].Identifier == "ulgrim") VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "Ulgrim's Recall");
                		if(RecallSpellList[i].Identifier == "candeth") VirindiViewService.TooltipSystem.AssociateTooltip(RecallSpellList[i].PortalIcon, "Candeth Keep Recall");
                	}
                }
                
                DynamicPortalGearMissingLayout = new HudFixedLayout();
                DynamicPortalGearTabView.AddTab(DynamicPortalGearMissingLayout, "Missing");
                
                DynaicPortalGearMissingSpellsList = new HudList();
                DynamicPortalGearMissingLayout.AddControl(DynaicPortalGearMissingSpellsList, new Rectangle(0,0,DynamicPortalGearView.Width, DynamicPortalGearView.Height));
                DynaicPortalGearMissingSpellsList.AddColumn(typeof(HudStaticText),DynamicPortalGearView.Width,null);
                
                HudList.HudListRowAccessor nrow;
                foreach(string spell in MissingSpellsList)
                {
                	nrow = DynaicPortalGearMissingSpellsList.AddRow();
                	((HudStaticText)nrow[0]).Text = spell;
                }    
                
            }catch(Exception ex){LogError(ex);}
 
        }
        
        private void DisposePortalGearHud()
        {
        	try
        	{
        		if(DynamicPortalGearView == null) {return;}
        		DestroyRecallSpells();
        		DynamicPortalGearView.Dispose();
        		DynamicPortalGearTabView.Dispose();
        		DynamicPortalGearLayout.Dispose();
        		DynamicPortalGearMissingLayout.Dispose();
        		DynaicPortalGearMissingSpellsList.Dispose();
        		DynamicPortalGearView = null;
        		
        	}catch(Exception ex){LogError(ex);}
        }

        private void CasterButton_Hit(object sender, System.EventArgs e)
        {
            try
            {
            	if(Core.Actions.CurrentSelection != 0 && Core.WorldFilter[Core.Actions.CurrentSelection].ObjectClass == ObjectClass.WandStaffOrb && 
				   Core.WorldFilter.GetInventory().Where(x => x.Id == Core.Actions.CurrentSelection).Count() != 0)
            	{
            		mDynamicPortalGearSettings.nOrbGuid = Core.Actions.CurrentSelection;
					mDynamicPortalGearSettings.nOrbIcon = Core.WorldFilter[mDynamicPortalGearSettings.nOrbGuid].Icon;
                	WriteToChat("Selected " + Core.WorldFilter[mDynamicPortalGearSettings.nOrbGuid].Name);
					savePortalSettings();
					RenderPortalGearHud();
            	}
            	else
            	{
                	WriteToChat("Please select a default caster to wield when one is not equipped.");
                	Core.ItemSelected += PortalItemSelected;
            	}
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void PortalItemSelected(object sender, ItemSelectedEventArgs e)
		{
			try
    		{	
				Core.ItemSelected -= PortalItemSelected;
				if(Core.Actions.CurrentSelection != 0 && Core.WorldFilter[Core.Actions.CurrentSelection].ObjectClass == ObjectClass.WandStaffOrb && 
				   Core.WorldFilter.GetInventory().Where(x => x.Id == Core.Actions.CurrentSelection).Count() != 0)
				{		
					mDynamicPortalGearSettings.nOrbGuid = Core.Actions.CurrentSelection;
					mDynamicPortalGearSettings.nOrbIcon = Core.WorldFilter[mDynamicPortalGearSettings.nOrbGuid].Icon;
                	WriteToChat("Selected " + Core.WorldFilter[mDynamicPortalGearSettings.nOrbGuid].Name);
					savePortalSettings();
					RenderPortalGearHud();
                }
				else
				{
					WriteToChat("No caster selected, try again.");
				}
            }catch(Exception ex){LogError(ex);}
        }
        
        private void HubGem_Hit(object sender, System.EventArgs e)
        {
            try{Core.Actions.UseItem(mDynamicPortalGearSettings.nFacilityHubGemID, 0);}catch(Exception ex){LogError(ex);}
        }
	
        //Text Recalls
		private void AtLifestone_Hit(object sender, System.EventArgs e)
        {
            try{Host.Actions.InvokeChatParser("/ls");}catch (Exception ex) {LogError(ex);}
        }
		private void AtHouse_Hit(object sender, System.EventArgs e)
        {
            try{Host.Actions.InvokeChatParser("/hr");}catch (Exception ex) {LogError(ex);}
        }
		private void AtMansion_Hit(object sender, System.EventArgs e)
        {
            try{Host.Actions.InvokeChatParser("/hom");}catch (Exception ex) {LogError(ex);}
        }
		private void AtMarket_Hit(object sender, System.EventArgs e)
        {
            try{Host.Actions.InvokeChatParser("/mp");}catch (Exception ex) {LogError(ex);}
        }
		private void AtAllegiance_Hit(object sender, System.EventArgs e)
        {
            try{Host.Actions.InvokeChatParser("/ah");}catch (Exception ex) {LogError(ex);}
        }

        //Portal Recalls
        private void PortalRecall_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.portal);}catch(Exception ex){LogError(ex);}
        }
        private void LifestoneTie_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.lifestonetie);}catch(Exception ex){LogError(ex);}
        }
        private void LifestoneRecall_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.lifestone);}catch(Exception ex){LogError(ex);}
        }
        private void TiePortalOne_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.tieprimary);}catch(Exception ex){LogError(ex);}
        }
        private void RecallPortalOne_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.primaryporal);}catch(Exception ex){LogError(ex);}
        }
        private void SummonPortalOne_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.summonprimary);}catch(Exception ex){LogError(ex);}
        }
        private void TiePortalTwo_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.tiesecondary);}catch(Exception ex){LogError(ex);}
        }
        private void RecallPortalTwo_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.secondaryportal);}catch(Exception ex){LogError(ex);}
        }
        private void SummonPortalTwo_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.summonsecondary);}catch (Exception ex){LogError(ex);}
        }

        
     	//Recall Spells
        private void Sanctuary_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.sanctuary);}catch(Exception ex){LogError(ex);}
        }
        private void BananaLand_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.bananaland);}catch(Exception ex){LogError(ex);}
        }
        private void Facility_Hit(object sender, System.EventArgs e)
        {
        	try{PortalActionsLoad(RecallTypes.facilityhub);}catch(Exception ex){LogError(ex);}
        }
        private void Colo_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.col);}catch (Exception ex) {LogError(ex);}
        }
        private void Aerlinthe_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.aerlinthe);}catch(Exception ex){LogError(ex);}
        }
        private void Caul_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.caul);}catch(Exception ex){LogError(ex);}
        }
        private void Bur_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.bur);}catch(Exception ex){LogError(ex);}
        }
        private void Olthoi_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.olthoi_north);}catch(Exception ex){LogError(ex);}
        }
        private void Mhoire_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.mhoire);}catch(Exception ex){LogError(ex);}
        }
        private void GearKnight_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.gearknight);}catch(Exception ex){LogError(ex);}
        }
        private void Neftet_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.neftet);}catch(Exception ex){LogError(ex);}
        }
        private void Rynthid_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.rynthid);}catch (Exception ex){LogError(ex);}
        }
     	private void Glendenwood_Hit(object sender, System.EventArgs e)
        {
     		try{PortalActionsLoad(RecallTypes.glendenwood);}catch (Exception ex){LogError(ex);}
        }
     	private void MtLethe_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.lethe);}catch (Exception ex){LogError(ex);}
        }
     	private void Ulgrim_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.ulgrim);}catch (Exception ex){LogError(ex);}
        }
     	private void Candeth_Hit(object sender, System.EventArgs e)
        {
            try{PortalActionsLoad(RecallTypes.candeth);}catch (Exception ex){LogError(ex);}
        }
     	
        private void PortalActionsLoad(RecallTypes recall)
        {
        	
        	try
        	{        		
          		//Not holding a caster
        		if(Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) == 0x1000000).Count() == 0)
        		{
        			FoundryToggleAction(FoundryActionTypes.Peace);
        			FoundryToggleAction(FoundryActionTypes.EquipWand);
        		}
        		
        		FoundryToggleAction(FoundryActionTypes.Magic);
        		FoundryToggleAction(FoundryActionTypes.Portal);
        		         		
        		switch(recall)
				{
					case RecallTypes.lifestone:
        				LoadPortalActionToFoundry(1635);
						break;
						
					case RecallTypes.portal:
						LoadPortalActionToFoundry(2645);
						break;
						
					case RecallTypes.primaryporal:
						LoadPortalActionToFoundry(48);
						break;
						
					case RecallTypes.summonprimary:
						LoadPortalActionToFoundry(157);
						break;
						
					case RecallTypes.secondaryportal:
						LoadPortalActionToFoundry(2647);
						break;
						
					case RecallTypes.summonsecondary:
						LoadPortalActionToFoundry(2648);
						break;
						
                    case RecallTypes.sanctuary:
                       LoadPortalActionToFoundry(2023);
                        break;
                        
                    case RecallTypes.bananaland:
                        LoadPortalActionToFoundry(2931);
                        break;
                        
                    case RecallTypes.col:
                        LoadPortalActionToFoundry(4213);
                        break;
                        
                    case RecallTypes.aerlinthe:
                        LoadPortalActionToFoundry(2041);
                        break;
                        
                    case RecallTypes.caul:
                        LoadPortalActionToFoundry(2943);
                        break;
                        
                    case RecallTypes.bur:
                        LoadPortalActionToFoundry(4084);
                        break;
                        
                    case RecallTypes.olthoi_north:
                        LoadPortalActionToFoundry(4198);
                        break;
                        
                    case RecallTypes.facilityhub:
                        LoadPortalActionToFoundry(5175);
                        break;
                    case RecallTypes.gearknight:
                        LoadPortalActionToFoundry(5330);
                        break;
                        
                    case RecallTypes.neftet:
                        LoadPortalActionToFoundry(5541);
                        break;
                        
                    case RecallTypes.rynthid:
                        LoadPortalActionToFoundry(6150);
                        break;
                        
                    case RecallTypes.mhoire: 
                        LoadPortalActionToFoundry(4128);
                        break;
                        
                    case RecallTypes.lifestonetie:
                        LoadPortalActionToFoundry(2644);
                        break;
                        
                    case RecallTypes.tieprimary:
                        LoadPortalActionToFoundry(47);
                        break;
                        
                    case RecallTypes.tiesecondary:
                        LoadPortalActionToFoundry(2646);
                        break;
                    
                    case RecallTypes.glendenwood:
                       LoadPortalActionToFoundry(3865);
                        break;
                        
                    case RecallTypes.lethe:
                        LoadPortalActionToFoundry(2813);
                        break;
                        
                    case RecallTypes.ulgrim:
                        LoadPortalActionToFoundry(2941);
                        break;
                    
                    case RecallTypes.candeth:
                        LoadPortalActionToFoundry(4214);
                        break;													
				}	
			
        		InitiateFoundryActions();
        		
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void LoadPortalActionToFoundry(int SpellId)
        {
        	try
        	{
        		int index = FoundryActionList.FindIndex(x => x.Action == FoundryActionTypes.Portal);
        		FoundryActionList[index].ToDoList.Clear();
        		FoundryActionList[index].ToDoList.Add(SpellId);
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void AssembleRecallSpellsList()
        {
        	try
        	{
        		PortalIcons picon;
        		
        		picon = new PortalIcons();
        		picon.PortalIcon.Image = mDynamicPortalGearSettings.nOrbIcon;
        		picon.PortalIcon.Hit += CasterButton_Hit;
        		picon.Identifier = "caster";
        		MiscRecallList.Add(picon);
        		
        		if(Core.WorldFilter.GetInventory().Where(x => x.Name.Contains("Facility Hub Portal Gem")).Count() > 0)
        		{
	        		picon = new PortalIcons();
	        		picon.PortalIcon.Image = CreateIconFromResource("facilityhubgem.gif");
	        		picon.PortalIcon.Hit += HubGem_Hit;
	        		picon.Identifier = "hubgem";
	        		MiscRecallList.Add(picon);
        		}
        		else
        		{
        			MissingSpellsList.Add("Facility Hub Gem");
        		}
        		
        		picon = new PortalIcons();
        		picon.PortalIcon.Image = GearGraphics.GR_LifestoneRecall_ICON;
        		picon.PortalIcon.Hit += AtLifestone_Hit;
        		picon.Identifier = "atlifestone";
        		TextRecallList.Add(picon);
        		
        		picon = new PortalIcons();
        		picon.PortalIcon.Image = GearGraphics.GR_HouseRecall_ICON;
        		picon.PortalIcon.Hit += AtHouse_Hit;
        		picon.Identifier = "athouse";
        		TextRecallList.Add(picon);
        		
        		picon = new PortalIcons();
        		picon.PortalIcon.Image = GearGraphics.GR_MansionRecall_ICON;
        		picon.PortalIcon.Hit += AtMansion_Hit;
        		picon.Identifier = "atmansion";
        		TextRecallList.Add(picon);
        		
        		picon = new PortalIcons();
        		picon.PortalIcon.Image = GearGraphics.GR_AHRecall_ICON;
        		picon.PortalIcon.Hit += AtAllegiance_Hit;
        		picon.Identifier = "atallegiance";
        		TextRecallList.Add(picon);
        		
        		picon = new PortalIcons();
        		picon.PortalIcon.Image = GearGraphics.GR_Market_ICON;
        		picon.PortalIcon.Hit += AtMarket_Hit;
        		picon.Identifier = "atmarket";
        		TextRecallList.Add(picon);
        		      		
        		//Portal Spells
        		if(Core.CharacterFilter.IsSpellKnown(2645))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("recall.gif");
        			picon.PortalIcon.Hit += PortalRecall_Hit;
        			picon.Identifier = "portalrecall";
        			PortalSpellList.Add(picon);
        		}
        		else MissingSpellsList.Add("Portal Recall");
        		if(Core.CharacterFilter.IsSpellKnown(2644))
        		{
        			picon = new PortalIcons();
                    picon.PortalIcon.Image = CreateIconFromResource("lstie.gif");
        			picon.PortalIcon.Hit += LifestoneTie_Hit;
        			picon.Identifier = "lifestonetie";
        			PortalSpellList.Add(picon);	
        		}
        		else MissingSpellsList.Add("Lifestone Tie");
        		if(Core.CharacterFilter.IsSpellKnown(1635))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("lsrecall.gif");
        			picon.PortalIcon.Hit += LifestoneRecall_Hit;
        			picon.Identifier = "lifestonerecall";
        			PortalSpellList.Add(picon);	
        		}
        		else MissingSpellsList.Add("Lifestone Recall");
        		if(Core.CharacterFilter.IsSpellKnown(47))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("tieP1.gif");
        			picon.PortalIcon.Hit += TiePortalOne_Hit;
        			picon.Identifier = "tieportalone";
        			PortalSpellList.Add(picon);
        		}
        		else MissingSpellsList.Add("Primary Portal Tie");
          		if(Core.CharacterFilter.IsSpellKnown(48))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("recallP1.gif");
        			picon.PortalIcon.Hit += RecallPortalOne_Hit;
        			picon.Identifier = "recallportalone";
        			PortalSpellList.Add(picon);
        		}
          		else MissingSpellsList.Add("Primary Portal Recall");
        		if(Core.CharacterFilter.IsSpellKnown(157))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("summonP1.gif");
        			picon.PortalIcon.Hit += SummonPortalOne_Hit;
        			picon.Identifier = "summonportalone";
        			PortalSpellList.Add(picon);
        		}
        		else MissingSpellsList.Add("Summon Primary Portal");
        		if(Core.CharacterFilter.IsSpellKnown(2646))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("tieP2.gif");
        			picon.PortalIcon.Hit += TiePortalTwo_Hit;
        			picon.Identifier = "tieportaltwo";		
        			PortalSpellList.Add(picon);
        		}
        		else MissingSpellsList.Add("Secondary Portal Tie");
        		if(Core.CharacterFilter.IsSpellKnown(2647))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("recallP2.gif");
        			picon.PortalIcon.Hit += RecallPortalTwo_Hit;
        			picon.Identifier = "recallportaltwo";
        			PortalSpellList.Add(picon);
        		}
        		else MissingSpellsList.Add("Secondary Portal Recall");
        		if(Core.CharacterFilter.IsSpellKnown(2648))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("summonP2.gif");
        			picon.PortalIcon.Hit += SummonPortalTwo_Hit;
        			picon.Identifier = "summonportaltwo";
        			PortalSpellList.Add(picon);
        		}
        		else MissingSpellsList.Add("Summon Secondary Portal");
        		//Recall Spells
        		if(Core.CharacterFilter.IsSpellKnown(2023))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("sanctuary.gif");
        			picon.PortalIcon.Hit += Sanctuary_Hit;
        			picon.Identifier = "sanctuary";
        			RecallSpellList.Add(picon);
        		}
        		else MissingSpellsList.Add("Recall the Sanctuary");
        		if(Core.CharacterFilter.IsSpellKnown(2931))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("bananaland.gif");
        			picon.PortalIcon.Hit += BananaLand_Hit;
        			picon.Identifier = "bananaland";
        			RecallSpellList.Add(picon);
        		}
        		else MissingSpellsList.Add("Recall Aphus Lassel");
        		if(Core.CharacterFilter.IsSpellKnown(4213))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("col.gif");
        			picon.PortalIcon.Hit += Colo_Hit;
        			picon.Identifier = "colo";
        			RecallSpellList.Add(picon);
        		}
        		else MissingSpellsList.Add("Colosseum Recall");
        		if(Core.CharacterFilter.IsSpellKnown(2041))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("aerlinthe.gif");
        			picon.Identifier = "aerlinthe";
        			picon.PortalIcon.Hit +=  Aerlinthe_Hit;
        			RecallSpellList.Add(picon);
        		}
        		else MissingSpellsList.Add("Aerlinthe Recall");
        		if(Core.CharacterFilter.IsSpellKnown(2943))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("caul.gif");
        			picon.Identifier = "caul";
        			picon.PortalIcon.Hit += Caul_Hit;
        			RecallSpellList.Add(picon);
        		}
        		else MissingSpellsList.Add("Recall to the Singularity Caul");
				if(Core.CharacterFilter.IsSpellKnown(4084))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("bur.gif");
        			picon.Identifier = "bur";
        			picon.PortalIcon.Hit += Bur_Hit;
        			RecallSpellList.Add(picon);
        		}
				else MissingSpellsList.Add("Bur Recall");
				if(Core.CharacterFilter.IsSpellKnown(4198))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("olthoi_north.gif");
        			picon.Identifier = "olthoi";
        			picon.PortalIcon.Hit += Olthoi_Hit;
        			RecallSpellList.Add(picon);
        		}
				else MissingSpellsList.Add("Paradox-touched Olthoi Infested Area Recall");
				if(Core.CharacterFilter.IsSpellKnown(5175))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("facility.gif");
        			picon.Identifier = "facility";
        			picon.PortalIcon.Hit += Facility_Hit;	
        			RecallSpellList.Add(picon);
        		}
				else MissingSpellsList.Add("Facility Hub Recall");
				if(Core.CharacterFilter.IsSpellKnown(5330))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("gearknight.gif");
        			picon.Identifier = "gearknight";
        			picon.PortalIcon.Hit += GearKnight_Hit;
        			RecallSpellList.Add(picon);
        		}
				else MissingSpellsList.Add("Gear Knight Invasion Area Camp Recall");
				if(Core.CharacterFilter.IsSpellKnown(5541))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("neftet.gif");
        			picon.Identifier = "neftet";
        			picon.PortalIcon.Hit += Neftet_Hit;
        			RecallSpellList.Add(picon);
        		}
				else MissingSpellsList.Add("Lost City of Neftet Recall");
				if(Core.CharacterFilter.IsSpellKnown(6150))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("rynthid.gif");
        			picon.Identifier = "rynthid";
        			picon.PortalIcon.Hit += Rynthid_Hit;
        			RecallSpellList.Add(picon);
        		}
				else MissingSpellsList.Add("Rynthid Recall");
				if(Core.CharacterFilter.IsSpellKnown(4128))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("mhoire.gif");
        			picon.Identifier = "mhoire";
        			picon.PortalIcon.Hit += Mhoire_Hit;
        			RecallSpellList.Add(picon);
        		}
				else MissingSpellsList.Add("Call of the Mhoire Forge");
				if(Core.CharacterFilter.IsSpellKnown(3865))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("GlendonWoods.gif");
        			picon.Identifier = "glendenwood";
        			picon.PortalIcon.Hit += Glendenwood_Hit;
        			RecallSpellList.Add(picon);
        		}
				else MissingSpellsList.Add("Glenden Wood Recall");
				if(Core.CharacterFilter.IsSpellKnown(2813))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("MtLetheRecall.gif");
        			picon.Identifier = "mtlethe";
        			picon.PortalIcon.Hit += MtLethe_Hit;
        			RecallSpellList.Add(picon);
        		}
				else MissingSpellsList.Add("Mount Lethe Recall");
				if(Core.CharacterFilter.IsSpellKnown(2941))
        		{
        			picon = new PortalIcons();
                    picon.PortalIcon.Image = CreateIconFromResource("Ulgrim.gif");
        			picon.Identifier = "ulgrim";
        			picon.PortalIcon.Hit += Ulgrim_Hit;
        			RecallSpellList.Add(picon);
        		}
				else MissingSpellsList.Add("Ulgrim's Recall");
				if(Core.CharacterFilter.IsSpellKnown(4214))
        		{
        			picon = new PortalIcons();
                    picon.PortalIcon.Image = CreateIconFromResource("candeth.gif");
        			picon.Identifier = "candeth";
        			picon.PortalIcon.Hit += Candeth_Hit;
        			RecallSpellList.Add(picon);
        		}
				else MissingSpellsList.Add("Return to the Keep");
				
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void DestroyRecallSpells()
        {
        	try
        	{
        		
        		foreach(PortalIcons picon in MiscRecallList)
        		{
        			if(picon.Identifier == "caster") picon.PortalIcon.Hit -= CasterButton_Hit;
        			if(picon.Identifier == "hubgem") picon.PortalIcon.Hit -= HubGem_Hit;
        			picon.PortalIcon.Dispose();        			
        		}
        		MiscRecallList.Clear();

        		foreach(PortalIcons picon in TextRecallList)
        		{
        			if(picon.Identifier == "atlifestone") picon.PortalIcon.Hit -= AtLifestone_Hit;
        			if(picon.Identifier == "athouse") picon.PortalIcon.Hit -= AtHouse_Hit;
        			if(picon.Identifier == "atmansion") picon.PortalIcon.Hit -= AtMansion_Hit;
        			if(picon.Identifier == "atallegiance") picon.PortalIcon.Hit -= AtAllegiance_Hit;
        			if(picon.Identifier == "atmarket") picon.PortalIcon.Hit -= AtMarket_Hit;
        			picon.PortalIcon.Dispose();
        		}
        		TextRecallList.Clear();
		
        		foreach(PortalIcons picon in PortalSpellList)
        		{
        			if(picon.Identifier == "portalrecall") picon.PortalIcon.Hit -= PortalRecall_Hit;
        			if(picon.Identifier == "lifestonetie") picon.PortalIcon.Hit -= LifestoneTie_Hit;
        			if(picon.Identifier == "lifestonerecall") picon.PortalIcon.Hit -= LifestoneRecall_Hit;
        			if(picon.Identifier == "tieportalone") picon.PortalIcon.Hit -= TiePortalOne_Hit;
        			if(picon.Identifier == "recallportalone") picon.PortalIcon.Hit -= RecallPortalOne_Hit;
        			if(picon.Identifier == "summonportalone") picon.PortalIcon.Hit -= SummonPortalOne_Hit;
        			if(picon.Identifier == "tieportaltwo") picon.PortalIcon.Hit -= TiePortalTwo_Hit;
        			if(picon.Identifier == "recallportaltwo") picon.PortalIcon.Hit -= RecallPortalTwo_Hit;
        			if(picon.Identifier == "summonportaltwo") picon.PortalIcon.Hit -= SummonPortalTwo_Hit;
        			picon.PortalIcon.Dispose();
        		}
        		PortalSpellList.Clear();
        	
        		foreach(PortalIcons picon in RecallSpellList)
        		{
        			if(picon.Identifier == "sanctuary") picon.PortalIcon.Hit -= Sanctuary_Hit;
        			if(picon.Identifier == "bananaland") picon.PortalIcon.Hit -= BananaLand_Hit;
        			if(picon.Identifier == "colo") picon.PortalIcon.Hit -= Colo_Hit;
        			if(picon.Identifier == "aerlinthe") picon.PortalIcon.Hit -=  Aerlinthe_Hit;
        			if(picon.Identifier == "caul") picon.PortalIcon.Hit -= Caul_Hit;
        			if(picon.Identifier == "bur") picon.PortalIcon.Hit -= Bur_Hit;
        			if(picon.Identifier == "olthoi") picon.PortalIcon.Hit -= Olthoi_Hit;
        			if(picon.Identifier == "facility") picon.PortalIcon.Hit -= Facility_Hit;
        			if(picon.Identifier == "gearknight") picon.PortalIcon.Hit -= GearKnight_Hit;
        			if(picon.Identifier == "neftet") picon.PortalIcon.Hit -= Neftet_Hit;
        			if(picon.Identifier == "rynthid") picon.PortalIcon.Hit -= Rynthid_Hit;
        			if(picon.Identifier == "mhoire") picon.PortalIcon.Hit -= Mhoire_Hit;
        			if(picon.Identifier == "glendenwood") picon.PortalIcon.Hit -= Glendenwood_Hit;
        			if(picon.Identifier == "mtlethe") picon.PortalIcon.Hit -= MtLethe_Hit;
        			if(picon.Identifier == "ulgrim") picon.PortalIcon.Hit -= Ulgrim_Hit;
        			if(picon.Identifier == "candeth") picon.PortalIcon.Hit -= Candeth_Hit;

        			picon.PortalIcon.Dispose();
        		}
        		RecallSpellList.Clear();
        		
        		MissingSpellsList.Clear();
	
        	}catch(Exception ex){LogError(ex);}
        }
    }
}//end of namespace




