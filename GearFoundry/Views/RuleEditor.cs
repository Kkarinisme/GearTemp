using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using VirindiViewService;
using MyClasses.MetaViewWrappers;
using System.Xml;
using System.Xml.Linq;


//Contains most of functions for editing rules Karin 4/16/13
namespace GearFoundry
{

    public partial class PluginCore
    {
    	
    	     


       // [ControlEvent", "Click")]
        private void btnRuleClear_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
        	try
            {
        		
                mSelectedRule.Element("Enabled").Value = "false";
                mSelectedRule.Element("Priority").Value = "999";
                mSelectedRule.Element("AppliesToFlag").Value = String.Empty;
                mSelectedRule.Element("ArcaneLore").Value = "-1";
                mSelectedRule.Element("Work").Value = "-1";
                mSelectedRule.Element("WieldLevel").Value = "-1";
                mSelectedRule.Element("WieldSkill").Value = "0";
                mSelectedRule.Element("MasteryType").Value = "0";
                mSelectedRule.Element("DamageType").Value = "0";
                mSelectedRule.Element("GearScore").Value = "-1";
                mSelectedRule.Element("WieldEnabled").Value = "false,false,false,false";
                mSelectedRule.Element("ReqSkill").Value = "-1,-1,-1,-1";
                mSelectedRule.Element("NumSpells").Value = "-1";
                mSelectedRule.Element("Slots").Value = String.Empty;
                mSelectedRule.Element("ArmorType").Value = String.Empty;
                mSelectedRule.Element("ArmorSet").Value = String.Empty;
                mSelectedRule.Element("Spells").Value = String.Empty;
                mSelectedRule.Element("Palettes").Value = String.Empty;
        		                      
                _UpdateRulesTabs();    
            }catch (Exception ex) { LogError(ex); };
        }

        [ControlEvent("btnRuleNew", "Click")]
        private void btnRuleNew_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
           try 
            {
           	
           		mSelectedRule = CreateRulesXElement();          		                    
           		mPrioritizedRulesList.Add(mSelectedRule);	
           		SyncAndSaveRules();
           		_UpdateRulesTabs();		
            }
            catch (Exception ex) { LogError(ex); }
        }

        [ControlEvent("btnRuleupdate", "Click")]
        private void btnRuleUpdate_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
                int HoldScrollPostion = lstRules.ScrollPosition;
                
                SyncAndSaveRules();
            	_UpdateRulesTabs();

                lstRules.ScrollPosition = HoldScrollPostion;
            }
            catch (Exception ex) { LogError(ex); }


        }
        
        private void InitRules()
        {
        	try
        	{               		
                mPrioritizedRulesList.Clear();
                mPrioritizedRulesList = xdocRules.Element("Rules").Descendants("Rule").OrderBy(x => x.Element("Enabled").Value).ToList();                
                FillItemRules();

        	}catch(Exception ex){LogError(ex);}
        }

        private void SyncAndSaveRules()
        {
        	try
        	{      		
	            xdocRules = new XDocument(new XElement("Rules"));
	            
	            string saverulenumber = mSelectedRule.Element("RuleNum").Value;
	
	            foreach(XElement el in mPrioritizedRulesList)
	            {
	                xdocRules.Root.Add(el);
	            }
	            
           		xdocRules.Save(rulesFilename);
           		
                mPrioritizedRulesList.Clear();
                mPrioritizedRulesList = xdocRules.Element("Rules").Descendants("Rule").OrderBy(x => x.Element("Enabled").Value).ToList();
                
                FillItemRules();
                
                mSelectedRule = mPrioritizedRulesList.Find(x => x.Element("RuleNum").Value == saverulenumber);

        	}catch(Exception ex){LogError(ex);}
        }

        [ControlEvent("txtRuleName", "End")]
        private void txtRuleName_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
        	mSelectedRule.Element("Name").Value = txtRuleName.Text.ToString().Trim();
        }

        [ControlEvent("txtRulePriority", "End")]
        private void txtRulePriority_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
        	try
        	{
        		mSelectedRule.Element("Priority").Value = txtRulePriority.Text;
        	}catch(Exception ex){mSelectedRule.Element("Priority").Value = "-1"; LogError(ex);}
        }
      
        [ControlEvent("txtRuleMaxCraft", "End")]
        private void txtRuleMaxCraft_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)   //Decal.Adapter.TextBoxEndEventArgs e)
        {
        	try
        	{
        		mSelectedRule.Element("Work").Value = txtRuleMaxCraft.Text;
        	}catch(Exception ex){mSelectedRule.Element("Work").Value = "-1"; LogError(ex);}
        }

        [ControlEvent("txtRuleArcaneLore", "End")]
        private void txtRuleArcaneLore_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)   //Decal.Adapter.TextBoxEndEventArgs e)
        {
        	try
        	{
        		mSelectedRule.Element("ArcaneLore").Value = txtRuleArcaneLore.Text;
        	}catch(Exception ex){mSelectedRule.Element("ArcaneLore").Value = "-1"; LogError(ex);}
        }
        
        [ControlEvent("txtGearScore", "End")]
        private void txtGearScore_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)   //Decal.Adapter.TextBoxEndEventArgs e)
        {
        	try
        	{
        		mSelectedRule.Element("GearScore").Value = txtGearScore.Text;
        	}catch(Exception ex){mSelectedRule.Element("GearScore").Value = "-1"; LogError(ex);}
        }

        [ControlEvent("txtRuleWieldLevel", "End")]
        private void txtRuleWieldLevel_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
        	try
        	{
        		mSelectedRule.Element("WieldLevel").Value = txtRuleWieldLevel.Text;
        	}catch(Exception ex){mSelectedRule.Element("WieldLevel").Value = "-1";LogError(ex);}

        }

        private void chkRuleFilterLegend_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            _UpdateRulesTabs();
        }

        private void chkRuleFilterEpic_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            _UpdateRulesTabs();
        }
        
        private void chkRuleFilterCloak_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            _UpdateRulesTabs();
        }
        
        private void chkRuleFilterMajor_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            _UpdateRulesTabs();
        }

        private void chkRuleFilterlvl8_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
        	_UpdateRulesTabs();
        }

        private void chkRuleEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
        	try
        	{
        		mSelectedRule.Element("Enabled").Value = chkRuleEnabled.Checked.ToString().ToLower();
        	}catch(Exception ex){LogError(ex);}

        }
        [ControlEvent("cboWeaponAppliesTo", "Change")]
        private void cboWeaponAppliesTo_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
            	mSelectedRule.Element("WieldSkill").Value = WeaponTypeList[cboWeaponAppliesTo.Selected].ID.ToString();
                if(mSelectedRule.Element("WieldSkill").Value == "54") {lblRuleReqSkilla.Text = "Essence Level";}
                else{lblRuleReqSkilla.Text = "Skill Level";}
            }
            catch (Exception ex) { LogError(ex); }
        }


        [ControlEvent("cboMasteryType", "Change")]
        private void cboMasteryType_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
        	mSelectedRule.Element("MasteryType").Value = cboMasteryType.Selected.ToString();
        }

        [ControlEvent("chkRuleWeaponsa", "Change")]
        private void chkRuleWeaponsa_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
           mSelectedRule.Element("WieldEnabled").Value = chkRuleWeaponsa.Checked.ToString() + "," + chkRuleWeaponsb.Checked.ToString() + "," +
        		chkRuleWeaponsc.Checked.ToString() + "," + chkRuleWeaponsd.Checked.ToString();
        }

        [ControlEvent("chkRuleWeaponsb", "Change")]
        private void chkRuleWeaponsb_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            mSelectedRule.Element("WieldEnabled").Value = chkRuleWeaponsa.Checked.ToString() + "," + chkRuleWeaponsb.Checked.ToString() + "," +
        		chkRuleWeaponsc.Checked.ToString() + "," + chkRuleWeaponsd.Checked.ToString();
        }

        [ControlEvent("chkRuleWeaponsc", "Change")]
        private void chkRuleWeaponsc_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
           mSelectedRule.Element("WieldEnabled").Value = chkRuleWeaponsa.Checked.ToString() + "," + chkRuleWeaponsb.Checked.ToString() + "," +
        		chkRuleWeaponsc.Checked.ToString() + "," + chkRuleWeaponsd.Checked.ToString();
        }

        [ControlEvent("chkRuleWeaponsd", "Change")]
        private void chkRuleWeaponsd_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
        	mSelectedRule.Element("WieldEnabled").Value = chkRuleWeaponsa.Checked.ToString() + "," + chkRuleWeaponsb.Checked.ToString() + "," +
        		chkRuleWeaponsc.Checked.ToString() + "," + chkRuleWeaponsd.Checked.ToString();
        }

        [ControlEvent("txtRuleReqSkilla", "End")]
        private void txtRuleReqSkilla_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
           mSelectedRule.Element("ReqSkill").Value = txtRuleReqSkilla.Text.Trim() + "," + txtRuleReqSkillb.Text.Trim() + "," + 
        							txtRuleReqSkillc.Text.Trim() + "," + txtRuleReqSkilld.Text.Trim();

        }

        [ControlEvent("txtRuleReqSkillb", "End")]
        private void txtRuleReqSkillb_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            mSelectedRule.Element("ReqSkill").Value = txtRuleReqSkilla.Text.Trim() + "," + txtRuleReqSkillb.Text.Trim() + "," + 
        							txtRuleReqSkillc.Text.Trim() + "," + txtRuleReqSkilld.Text.Trim();
        }
        [ControlEvent("txtRuleReqSkillc", "End")]
        private void txtRuleReqSkillc_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            mSelectedRule.Element("ReqSkill").Value = txtRuleReqSkilla.Text.Trim() + "," + txtRuleReqSkillb.Text.Trim() + "," + 
        							txtRuleReqSkillc.Text.Trim() + "," + txtRuleReqSkilld.Text.Trim();
        }

        [ControlEvent("txtRuleReqSkilld", "End")]
        private void txtRuleReqSkilld_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
        	mSelectedRule.Element("ReqSkill").Value = txtRuleReqSkilla.Text.Trim() + "," + txtRuleReqSkillb.Text.Trim() + "," + 
        							txtRuleReqSkillc.Text.Trim() + "," + txtRuleReqSkilld.Text.Trim();
        }

        [ControlEvent("txtRuleNumSpells", "End")]
        private void txtRuleNumSpells_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
        	try
        	{
        		Convert.ToInt32(txtRuleNumSpells.Text);
        		mSelectedRule.Element("NumSpells").Value = txtRuleNumSpells.Text;
        	}
        	catch{mSelectedRule.Element("NumSpells").Value = "-1";}
        }
        
        
        private string[] KeyTypes = {"Double","Long"};
        private string[] KeyCompare = {"Equals","Not Equals","Equals or Greater","Equals or Less"};
        private string[] KeyLink = {"End","And","Or"};

        
        private void chkAdvEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
        	try
        	{
	        	if(e.Checked)
	        	{
					cboAdv1KeyType.Clear();
	        		cboAdv1KeyCompare.Clear();
	        		cboAdv1Key.Clear();
	        		cboAdv1Link.Clear();
	        		txtAdv1KeyValue.Text = String.Empty;
	        		foreach(string item in KeyTypes) {cboAdv1KeyType.Add(item);}
	        		FillAdvancedKeyList(cboAdv1KeyType.Selected, cboAdv1Key);
	        		foreach(string item in KeyCompare) {cboAdv1KeyCompare.Add(item);}
	        		foreach(string item in KeyLink) {cboAdv1Link.Add(item);}	        		
	        	}
	        	else
	        	{
	        		cboAdv1KeyType.Clear();
	        		cboAdv1Key.Clear();
	        		cboAdv1KeyCompare.Clear();
	        		cboAdv1Link.Clear();
	        		txtAdv1KeyValue.Text = String.Empty;
	        	}
	        	Update_mSelected_Advanced();
        	}catch(Exception ex){LogError(ex);}
        }
        

         
		private void cboAdv1KeyType_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
			try
			{
				FillAdvancedKeyList(cboAdv1KeyType.Selected, cboAdv1Key);
				Update_mSelected_Advanced();
			}catch(Exception ex){LogError(ex);}
        }
		
		private void cboAdv1Key_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
        	Update_mSelected_Advanced();
        }
		
		private void cboAdv1KeyCompare_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
        	Update_mSelected_Advanced();
        }
		
		private void txtAdv1KeyValue_Change(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
			Update_mSelected_Advanced();
        }   

		private void cboAdv1Link_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
			try
        	{
	        	if(cboAdv1Link.Selected != 0)
	        	{
	        		foreach(string item in KeyTypes) {cboAdv2KeyType.Add(item);}
	        		foreach(string item in KeyCompare) {cboAdv2KeyCompare.Add(item);}
	        		FillAdvancedKeyList(cboAdv2KeyType.Selected, cboAdv2Key);
	        		foreach(string item in KeyLink) {cboAdv2Link.Add(item);}	        		
	        	}
	        	else
	        	{
	        		cboAdv2KeyType.Clear();
	        		cboAdv2KeyCompare.Clear();
	        		cboAdv2KeyType.Clear();
	        		cboAdv2Link.Clear();
	        		txtAdv2KeyValue.Text = String.Empty;
	        	}
	        	Update_mSelected_Advanced();
        	}catch(Exception ex){LogError(ex);}
        }
		
		private void cboAdv2KeyType_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
			try
			{
				if(cboAdv2KeyType.Selected == 0) {cboAdv2Key.Clear();}
				else
				{
					FillAdvancedKeyList(cboAdv2KeyType.Selected, cboAdv2Key);
				}
				Update_mSelected_Advanced();
			}catch(Exception ex){LogError(ex);}
        }
		
		private void cboAdv2Key_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
        	Update_mSelected_Advanced();
        }
		
		private void cboAdv2KeyCompare_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
        	Update_mSelected_Advanced();
        }
		
		private void txtAdv2KeyValue_Change(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
			Update_mSelected_Advanced();
        }   

		private void cboAdv2Link_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
			try
        	{
	        	if(cboAdv2Link.Selected != 0)
	        	{
	        		foreach(string item in KeyTypes) {cboAdv3KeyType.Add(item);}
	        		foreach(string item in KeyCompare) {cboAdv3KeyCompare.Add(item);}
	        		FillAdvancedKeyList(cboAdv3KeyType.Selected, cboAdv3Key);
	        		foreach(string item in KeyLink) {cboAdv3Link.Add(item);}	        		
	        	}
	        	else
	        	{
	        		cboAdv3KeyType.Clear();
	        		cboAdv3KeyCompare.Clear();
	        		cboAdv3KeyType.Clear();
	        		cboAdv3Link.Clear();
	        		txtAdv3KeyValue.Text = String.Empty;
	        	}
	        	Update_mSelected_Advanced();
        	}catch(Exception ex){LogError(ex);}
        }
		
		private void cboAdv3KeyType_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
			try
			{
				if(cboAdv3KeyType.Selected == 0) {cboAdv3Key.Clear();}
				else
				{
					FillAdvancedKeyList(cboAdv3KeyType.Selected, cboAdv3Key);
				}
				Update_mSelected_Advanced();
			}catch(Exception ex){LogError(ex);}
        }
		
		private void cboAdv3Key_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
        	Update_mSelected_Advanced();
        }
		
		private void cboAdv3KeyCompare_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
        	Update_mSelected_Advanced();
        }
		
		private void txtAdv3KeyValue_Change(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
			Update_mSelected_Advanced();
        }   

		private void cboAdv3Link_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
			try
        	{
	        	if(cboAdv3Link.Selected != 0)
	        	{
	        		foreach(string item in KeyTypes) {cboAdv4KeyType.Add(item);}
	        		foreach(string item in KeyCompare) {cboAdv4KeyCompare.Add(item);}
	        		FillAdvancedKeyList(cboAdv4KeyType.Selected, cboAdv4Key);
	        		foreach(string item in KeyLink) {cboAdv4Link.Add(item);}	        		
	        	}
	        	else
	        	{
	        		cboAdv4KeyType.Clear();
	        		cboAdv4KeyCompare.Clear();
	        		cboAdv4KeyType.Clear();
	        		cboAdv4Link.Clear();
	        		txtAdv4KeyValue.Text = String.Empty;
	        	}
	        	Update_mSelected_Advanced();
        	}catch(Exception ex){LogError(ex);}
        }
		
		private void cboAdv4KeyType_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
			try
			{
				if(cboAdv4KeyType.Selected == 0) {cboAdv4Key.Clear();}
				else
				{
					FillAdvancedKeyList(cboAdv4KeyType.Selected, cboAdv4Key);
				}
				Update_mSelected_Advanced();
			}catch(Exception ex){LogError(ex);}
        }
		
		private void cboAdv4Key_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
        	Update_mSelected_Advanced();
        }
		
		private void cboAdv4KeyCompare_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
        	Update_mSelected_Advanced();
        }
		
		private void txtAdv4KeyValue_Change(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
			Update_mSelected_Advanced();
        }   

		private void cboAdv4Link_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
			try
        	{
	        	if(cboAdv4Link.Selected != 0)
	        	{
	        		foreach(string item in KeyTypes) {cboAdv5KeyType.Add(item);}
	        		foreach(string item in KeyCompare) {cboAdv5KeyCompare.Add(item);}  
					FillAdvancedKeyList(cboAdv5KeyType.Selected, cboAdv5Key);	        		
	        	}
	        	else
	        	{
	        		cboAdv5KeyType.Clear();
	        		cboAdv5KeyCompare.Clear();
	        		cboAdv5KeyType.Clear();
	        		txtAdv5KeyValue.Text = String.Empty;
	        	}
	        	Update_mSelected_Advanced();
        	}catch(Exception ex){LogError(ex);}
        	
        }
		
		private void cboAdv5KeyType_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
			try
			{
				if(cboAdv5KeyType.Selected == 0) {cboAdv5Key.Clear();}
				else
				{
					FillAdvancedKeyList(cboAdv5KeyType.Selected, cboAdv5Key);
				}
				Update_mSelected_Advanced();
			}catch(Exception ex){LogError(ex);}
        }
		
		private void cboAdv5Key_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
        	Update_mSelected_Advanced();
        }
		
		private void cboAdv5KeyCompare_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
        	Update_mSelected_Advanced();
        }
		
		private void txtAdv5KeyValue_Change(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
			Update_mSelected_Advanced();
        }  

		private void FillAdvancedKeyList(int selection, MyClasses.MetaViewWrappers.ICombo cboList)
		{
			try
			{
				switch(selection)
				{
					case 0:
						cboList.Clear();
						foreach(IDNameLoadable idl in DoubleKeyList) {cboList.Add(idl.name);}
						break;
					case 1:
						cboList.Clear();
						foreach(IDNameLoadable idl in LongKeyList) {cboList.Add(idl.name);}
						break;
				}
				
			}catch(Exception ex){LogError(ex);}
		}		
        
		private void Update_mSelected_Advanced()
		{
			try
			{
				string savestring = String.Empty;
				if(!chkAdvEnabled.Checked){savestring = "false";}
				else
				{
					savestring = "true," + AdvancedStringSegement(cboAdv1KeyType.Selected, cboAdv1Key.Selected) +
						KeyCompare[cboAdv1KeyCompare.Selected] + ":" + txtAdv1KeyValue.Text + ":" + KeyLink[cboAdv1Link.Selected];
					
					if(cboAdv1Link.Selected != 0)
					{
						savestring += "," + AdvancedStringSegement(cboAdv2KeyType.Selected, cboAdv2Key.Selected) + 
						KeyCompare[cboAdv2KeyCompare.Selected] + ":" + txtAdv2KeyValue.Text + ":" + KeyLink[cboAdv2Link.Selected];
						
						if(cboAdv2Link.Selected != 0)
						{
							savestring += "," + AdvancedStringSegement(cboAdv3KeyType.Selected, cboAdv3Key.Selected) +
							KeyCompare[cboAdv3KeyCompare.Selected] + ":" + txtAdv3KeyValue.Text + ":" + KeyLink[cboAdv3Link.Selected];
							
							if(cboAdv3Link.Selected != 0)
							{
								savestring += "," + AdvancedStringSegement(cboAdv4KeyType.Selected, cboAdv4Key.Selected) + 
								KeyCompare[cboAdv4KeyCompare.Selected] + ":" + txtAdv4KeyValue.Text + ":" + KeyLink[cboAdv4Link.Selected];
								
								if(cboAdv4Link.Selected != 0)
								{
									savestring += "," + AdvancedStringSegement(cboAdv5KeyType.Selected, cboAdv5Key.Selected) + 
									KeyCompare[cboAdv5KeyCompare.Selected] + ":" + txtAdv5KeyValue.Text + ":" + KeyLink[0];		
								}
							}
						}
					}		
				}
				mSelectedRule.Element("Advanced").Value = savestring;
			}catch(Exception ex){LogError(ex);}
			
		}
		
		private string AdvancedStringSegement(int AdvKeyType, int AdvKeyListValue)
		{
			string result = String.Empty;
			switch(AdvKeyType)
			{
				case 0:
					result = "Double:" + DoubleKeyList[AdvKeyListValue].ID.ToString() + ":";
					break;
				case 1:
					result = "Long:" + LongKeyList[AdvKeyListValue].ID.ToString() + ":";
					break;
			}
			return result;
		}
    }
}