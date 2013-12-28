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
       
        private List<PortalActions> PortalActionList = new List<PortalActions>();
        private System.Windows.Forms.Timer PortalActionTimer = new System.Windows.Forms.Timer();
        
     	private class PortalActions
     	{
     		public bool fireaction = false;
			public bool pending = false;
			public DateTime StartAction = DateTime.MinValue;
			public int ItemId = 0;	
			public RecallTypes RecallSpell = RecallTypes.none;	
			public int Retries = 0;			
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
            tiesecondary
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
				
				for(int i = 0; i < 4; i++)
				{
					PortalActionList.Add(new PortalActions());
				}
				
				Core.CharacterFilter.ActionComplete += PortalCast_ListenComplete;
				Core.CharacterFilter.ChangePortalMode += PortalCast_Listen;
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
		
		private void PortalGear_Logoff(object sender, EventArgs e)
		{
			DisposePortalGearHud();
			UnsubscribePortalEvents();
		}
		
		private void UnsubscribePortalEvents()
		{
			try
			{
				PortalActionList.Clear();
				savePortalSettings();
				Core.CharacterFilter.ActionComplete -= PortalCast_ListenComplete;
				Core.CharacterFilter.ChangePortalMode -= PortalCast_Listen;
				Core.CharacterFilter.Logoff -= PortalGear_Logoff;
			}catch(Exception ex){LogError(ex);}
		}
				
		private HudView DynamicPortalGearView = null;
		private HudTabView DynamicPortalGearTabView = null;
		private HudFixedLayout DynamicPortalGearLayout = null;	

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
                if(TextRecallList.Count + PortalSpellList.Count  > MiscRecallList.Count + RecallSpellList.Count)
                {
                	PortalHudWidth = (TextRecallList.Count + PortalSpellList.Count) * 30;
                }
                else
                {
                	PortalHudWidth = (MiscRecallList.Count + RecallSpellList.Count) * 30;
                }
                int PortalHudHeight = 70;
                
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
                DynamicPortalGearTabView.AddTab(DynamicPortalGearLayout, "");
                
               	int shiftbase = 0;		
                //Text based recall Tiles
                for(int i = 0; i < TextRecallList.Count; i++)
                {
                	DynamicPortalGearLayout.AddControl(TextRecallList[i].PortalIcon, new Rectangle(i * 30, 0, 25, 25));
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
                
                shiftbase = 0;
                //Misc tiles
                for(int i = 0; i < MiscRecallList.Count; i++)
                {
                	DynamicPortalGearLayout.AddControl(MiscRecallList[i].PortalIcon, new Rectangle(i*30, 30, 25, 25));
                	if(MiscRecallList[i].Identifier == "caster") VirindiViewService.TooltipSystem.AssociateTooltip(MiscRecallList[i].PortalIcon, "Select Caster");
                	if(MiscRecallList[i].Identifier == "hubgem") VirindiViewService.TooltipSystem.AssociateTooltip(MiscRecallList[i].PortalIcon, "Facility Hub Gem");
                }
                shiftbase += MiscRecallList.Count * 30;
                //Recall spell tiles
                if(RecallSpellList.Count > 0)
                {
                	for(int i = 0; i < RecallSpellList.Count; i++)
                	{
                		DynamicPortalGearLayout.AddControl(RecallSpellList[i].PortalIcon, new Rectangle(shiftbase + i * 30, 30, 25, 25));
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
                	}
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
     
        private void PortalActionsLoad(RecallTypes recall)
        {
        	
        	try
        	{
        		if(PortalActionsPending) 
        		{
        			WriteToChat("Portal action pending.  Please wait for completion.");
        			return;
        		}
        		
        		if(mDynamicPortalGearSettings.nOrbGuid == 0)
        		{
        			mDynamicPortalGearSettings.nOrbGuid = Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.WandStaffOrb && 
        			           (x.Values(LongValueKey.WieldReqValue) == 0 || (x.Values(LongValueKey.WieldReqValue) == 150 && Core.CharacterFilter.Level >= 150) ||
        			           (x.Values(LongValueKey.WieldReqValue) == 180 && Core.CharacterFilter.Level >= 180))).ToList().OrderByDescending(x => x.Values(DoubleValueKey.MeleeDefenseBonus)).First().Id;
        			
        			mDynamicPortalGearSettings.nOrbIcon = Core.WorldFilter[mDynamicPortalGearSettings.nOrbGuid].Icon;
                }
        		
        		//Not holding a caster
        		if(Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) == 0x1000000).Count() == 0)
        		{
        			if(Core.Actions.CombatMode != CombatState.Peace)
        			{
        				PortalActionList[0].fireaction = true;
        			}
        			PortalActionList[1].fireaction = true;
        			PortalActionList[2].fireaction = true;
        		}
        		else if(Core.Actions.CombatMode != CombatState.Magic)
        		{
        			PortalActionList[2].fireaction = true;
          		}
        		PortalActionList[3].fireaction = true;
        		PortalActionList[3].RecallSpell = recall; 
				PortalCastSuccess = false;        		

        		InitiatePortalActions();
		
        	}catch(Exception ex){LogError(ex);}
        }
        
        private bool PortalActionsPending = false;
        private void InitiatePortalActions()
        {
        	try
        	{
        		if(!PortalActionsPending) 
        		{
        			PortalActionsPending = true;
					PortalActionTimer.Interval = 250;
					PortalActionTimer.Start();
					
					PortalActionTimer.Tick += PortalActionInitiator;
					return;
        		}
        	}catch(Exception ex){LogError(ex);}
        }

        private void PortalActionInitiator(object sender, EventArgs e)
		{
			try
			{
				FirePortalActions();
			}catch(Exception ex){LogError(ex);}
		}
        
        
        private void FirePortalActions()
		{
			try
			{				
				if(PortalActionList[0].fireaction)
				{
					if(PortalActionList[0].pending && (DateTime.Now - PortalActionList[0].StartAction).TotalMilliseconds < 600)
					{
						return;
					}
					else if(Core.Actions.CombatMode != CombatState.Peace)
					{
						PortalActionList[0].pending = true;
						PortalActionList[0].StartAction = DateTime.Now;
						Core.Actions.SetCombatMode(CombatState.Peace);
						return;
					}
					else
					{
						PortalActionList[0].pending = false;
						PortalActionList[0].StartAction = DateTime.MinValue;
						PortalActionList[0].fireaction = false;
					}
				}
				else if(PortalActionList[1].fireaction)
				{
					if(PortalActionList[1].pending && (DateTime.Now - PortalActionList[1].StartAction).TotalMilliseconds < 300)
					{
						return;
					}
					else if(Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) == 0x1000000).Count() == 0)
					{	
						PortalActionList[1].pending = true;
						PortalActionList[1].StartAction = DateTime.Now;
						PortalActionEquip();
						return;
					}
					else
					{
						PortalActionList[1].pending = false;
						PortalActionList[1].StartAction = DateTime.MinValue;
						PortalActionList[1].fireaction = false;
					}
				}
				else if(PortalActionList[2].fireaction)
				{
					if(PortalActionList[2].pending && (DateTime.Now - PortalActionList[2].StartAction).TotalMilliseconds < 600)
					{
						return;
					}
					else if(Core.Actions.CombatMode != CombatState.Magic)
					{
						PortalActionList[2].pending = true;
						PortalActionList[2].StartAction = DateTime.Now;
						Core.Actions.SetCombatMode(CombatState.Magic);
						return;
					}
					else
					{
						PortalActionList[2].pending = false;
						PortalActionList[2].StartAction = DateTime.MinValue;
						PortalActionList[2].fireaction = false;
					}
				}
				else if(PortalActionList[3].fireaction)
				{
					if(PortalCastSuccess)
					{
						PortalActionsPending = false;
						PortalActionList[3].pending = false;
						PortalActionTimer.Tick -= PortalActionInitiator;	
						PortalActionTimer.Stop();
						return;	
					}
					else if(PortalActionList[3].pending && (DateTime.Now - PortalActionList[3].StartAction).TotalSeconds < 5)
					{
						return;
					}
					else if(!PortalCastSuccess && PortalActionList[3].Retries < 3)
					{
						PortalActionList[3].pending = true;
						PortalActionList[3].StartAction = DateTime.Now;
						PortalActionList[3].Retries++;
						PortalActionsCastSpell();
						return;
					}	
					else if(!PortalCastSuccess && PortalActionList[3].Retries >= 3)
					{
						if(PortalActionList[3].Retries >= 3) {WriteToChat("Recall/Summon Failed. Check ties and other recall requirements.");}
						PortalActionList[3].pending = false;
						PortalActionList[3].StartAction = DateTime.MinValue;
						PortalActionList[3].fireaction = false;
						PortalActionList[3].Retries = 0;	
					}

				}
				else
				{
					PortalActionsPending = false;
					PortalActionList[3].pending = false;
					PortalActionTimer.Tick -= PortalActionInitiator;	
					PortalActionTimer.Stop();
					return;
				}
			}catch(Exception ex){LogError(ex);}
		}     
     
            
        private void PortalActionEquip()
        {
        	try
			{	

				if(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Armor && x.Values(LongValueKey.EquippedSlots) == 0x200000).Count() > 0 &&
				   Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.MeleeWeapon && x.LongKeys.Contains((int)LongValueKey.EquippedSlots)).Count() == 0)
				{
					Core.Actions.UseItem(Core.WorldFilter.GetInventory().Where(x => x.ObjectClass == ObjectClass.Armor && x.Values(LongValueKey.EquippedSlots) == 0x200000).First().Id,0);
					return;
				}
				else
				{
					Core.Actions.UseItem(mDynamicPortalGearSettings.nOrbGuid, 0);
					return;
				}
					
			}catch(Exception ex){LogError(ex);}
		}
        
        private void PortalActionsCastSpell()
        {
        	try
        	{
        		//Clean up listens in cast

					
				switch(PortalActionList[3].RecallSpell)
				{
					case RecallTypes.lifestone:
						Core.Actions.CastSpell(1635, Core.CharacterFilter.Id);
						return;
						
					case RecallTypes.portal:
						Core.Actions.CastSpell(2645, Core.CharacterFilter.Id);
						return;
						
					case RecallTypes.primaryporal:
						Core.Actions.CastSpell(48, Core.CharacterFilter.Id);
						return;
						
					case RecallTypes.summonprimary:
						Core.Actions.CastSpell(157, Core.CharacterFilter.Id);
						return;
						
					case RecallTypes.secondaryportal:
						Core.Actions.CastSpell(2647, Core.CharacterFilter.Id);
						return;
						
					case RecallTypes.summonsecondary:
						Core.Actions.CastSpell(2648, Core.CharacterFilter.Id);
						return;
						
                    case RecallTypes.sanctuary:
                        Core.Actions.CastSpell(2023, Core.CharacterFilter.Id);
                        return;
                        
                    case RecallTypes.bananaland:
                        Core.Actions.CastSpell(2931, Core.CharacterFilter.Id);
                        return;
                        
                    case RecallTypes.col:
                        Core.Actions.CastSpell(4213, Core.CharacterFilter.Id);
                        return;
                        
                    case RecallTypes.aerlinthe:
                        Core.Actions.CastSpell(2041, Core.CharacterFilter.Id);
                        return;
                        
                    case RecallTypes.caul:
                        Core.Actions.CastSpell(2943, Core.CharacterFilter.Id);
                        return;
                        
                    case RecallTypes.bur:
                        Core.Actions.CastSpell(4084, Core.CharacterFilter.Id);
                        return;
                        
                    case RecallTypes.olthoi_north:
                        Core.Actions.CastSpell(4198, Core.CharacterFilter.Id);
                        return;
                        
                    case RecallTypes.facilityhub:
                        Core.Actions.CastSpell(5175, Core.CharacterFilter.Id);
                        return;
                    case RecallTypes.gearknight:
                        Core.Actions.CastSpell(5330, Core.CharacterFilter.Id);
                        return;
                        
                    case RecallTypes.neftet:
                        Core.Actions.CastSpell(5541, Core.CharacterFilter.Id);
                        return;
                        
                    case RecallTypes.rynthid:
                        Core.Actions.CastSpell(6150, Core.CharacterFilter.Id);
                        return;
                        
                    case RecallTypes.mhoire: 
                        Core.Actions.CastSpell(4128, Core.CharacterFilter.Id);
                        return;
                        
                    case RecallTypes.lifestonetie:
                        Core.Actions.CastSpell(2644, Core.Actions.CurrentSelection);
                        return;
                        
                    case RecallTypes.tieprimary:
                        Core.Actions.CastSpell(47, Core.Actions.CurrentSelection);
                        return;
                        
                    case RecallTypes.tiesecondary:
                        Core.Actions.CastSpell(2646, Core.Actions.CurrentSelection);
                        return;
                       
													
				}	
        	}catch(Exception ex){LogError(ex);}
        }
        
        private bool PortalCastSuccess = false;
        private void PortalCast_Listen(object sender, EventArgs e)
        {
        	try
        	{
        		if(!PortalActionsPending) {return;}
        		PortalCastSuccess = true;
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void PortalCast_ListenComplete(object sender, EventArgs e)
        {
        	try
        	{
        		if(!PortalActionsPending) {return;}
        		PortalCastSuccess = true;
        		
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
        		if(Core.CharacterFilter.IsSpellKnown(2644))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = new ACImage(Color.Blue);
        			picon.PortalIcon.Hit += LifestoneTie_Hit;
        			picon.Identifier = "lifestonetie";
        			
        			PortalSpellList.Add(picon);	
        		}
        		if(Core.CharacterFilter.IsSpellKnown(1635))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("lsrecall.gif");
        			picon.PortalIcon.Hit += LifestoneRecall_Hit;
        			picon.Identifier = "lifestonerecall";
        			
        			PortalSpellList.Add(picon);	
        		}
        		if(Core.CharacterFilter.IsSpellKnown(47))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = new ACImage(Color.MediumPurple);
        			picon.PortalIcon.Hit += TiePortalOne_Hit;
        			picon.Identifier = "tieportalone";
        			
        			PortalSpellList.Add(picon);
        		}
          		if(Core.CharacterFilter.IsSpellKnown(48))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("recallP1.gif");
        			picon.PortalIcon.Hit += RecallPortalOne_Hit;
        			picon.Identifier = "recallportalone";
        			
        			PortalSpellList.Add(picon);
        		}
        		if(Core.CharacterFilter.IsSpellKnown(157))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("summonP1.gif");
        			picon.PortalIcon.Hit += SummonPortalOne_Hit;
        			picon.Identifier = "summonportalone";
        			
        			PortalSpellList.Add(picon);
        		}
        		if(Core.CharacterFilter.IsSpellKnown(2646))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = new ACImage(Color.MediumPurple);
        			picon.PortalIcon.Hit += TiePortalTwo_Hit;
        			picon.Identifier = "tieportaltwo";
        			
        			PortalSpellList.Add(picon);
        		}
        		if(Core.CharacterFilter.IsSpellKnown(2647))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("recallP2.gif");
        			picon.PortalIcon.Hit += RecallPortalTwo_Hit;
        			picon.Identifier = "recallportaltwo";
        			
        			PortalSpellList.Add(picon);
        		}
        		if(Core.CharacterFilter.IsSpellKnown(2648))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("summonP2.gif");
        			picon.PortalIcon.Hit += SummonPortalTwo_Hit;
        			picon.Identifier = "summonportaltwo";
        			
        			PortalSpellList.Add(picon);
        		}
        		//Recall Spells
        		if(Core.CharacterFilter.IsSpellKnown(2023))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("sanctuary.gif");
        			picon.PortalIcon.Hit += Sanctuary_Hit;
        			picon.Identifier = "sanctuary";
        			
        			RecallSpellList.Add(picon);
        		}
        		if(Core.CharacterFilter.IsSpellKnown(2931))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("bananaland.gif");
        			picon.PortalIcon.Hit += BananaLand_Hit;
        			picon.Identifier = "bananaland";
        			
        			RecallSpellList.Add(picon);
        		}
        		if(Core.CharacterFilter.IsSpellKnown(4213))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("col.gif");
        			picon.PortalIcon.Hit += Colo_Hit;
        			picon.Identifier = "colo";
        			
        			RecallSpellList.Add(picon);
        		}
        		if(Core.CharacterFilter.IsSpellKnown(2041))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("aerlinthe.gif");
        			picon.Identifier = "aerlinthe";
        			picon.PortalIcon.Hit +=  Aerlinthe_Hit;
        			
        			RecallSpellList.Add(picon);
        		}
        		if(Core.CharacterFilter.IsSpellKnown(2943))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("caul.gif");
        			picon.Identifier = "caul";
        			picon.PortalIcon.Hit += Caul_Hit;
        			
        			RecallSpellList.Add(picon);
        		}
				if(Core.CharacterFilter.IsSpellKnown(4084))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("bur.gif");
        			picon.Identifier = "bur";
        			picon.PortalIcon.Hit += Bur_Hit;
        			
        			RecallSpellList.Add(picon);
        		}
				if(Core.CharacterFilter.IsSpellKnown(4198))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("olthoi_north.gif");
        			picon.Identifier = "olthoi";
        			picon.PortalIcon.Hit += Olthoi_Hit;
        			
        			RecallSpellList.Add(picon);
        		}
				if(Core.CharacterFilter.IsSpellKnown(5175))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("facility.gif");
        			picon.Identifier = "facility";
        			picon.PortalIcon.Hit += Facility_Hit;
        			
        			RecallSpellList.Add(picon);
        		}
				if(Core.CharacterFilter.IsSpellKnown(5330))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("gearknight.gif");
        			picon.Identifier = "gearknight";
        			picon.PortalIcon.Hit += GearKnight_Hit;
        			
        			RecallSpellList.Add(picon);
        		}
				if(Core.CharacterFilter.IsSpellKnown(5541))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("neftet.gif");
        			picon.Identifier = "neftet";
        			picon.PortalIcon.Hit += Neftet_Hit;
        			
        			RecallSpellList.Add(picon);
        		}
				if(Core.CharacterFilter.IsSpellKnown(6150))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("rynthid.gif");
        			picon.Identifier = "rynthid";
        			picon.PortalIcon.Hit += Rynthid_Hit;
        			
        			RecallSpellList.Add(picon);
        		}
				if(Core.CharacterFilter.IsSpellKnown(4128))
        		{
        			picon = new PortalIcons();
        			picon.PortalIcon.Image = CreateIconFromResource("mhoire.gif");
        			picon.Identifier = "mhoire";
        			picon.PortalIcon.Hit += Mhoire_Hit;
        			
        			RecallSpellList.Add(picon);
        		}	
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
        			picon.PortalIcon.Dispose();
        		}
        		RecallSpellList.Clear();
	
        	}catch(Exception ex){LogError(ex);}
        }
    }
}//end of namespace




