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
           		MirrorToXdocRules();
				setUpRulesLists();           		
           		_UpdateRulesTabs();
           		FillItemRules();  		
            }
            catch (Exception ex) { LogError(ex); }
        }

        [ControlEvent("btnRuleupdate", "Click")]
        private void btnRuleUpdate_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
                int HoldScrollPostion = lstRules.ScrollPosition;
                
                MirrorToXdocRules();
                setUpRulesLists();
            	_UpdateRulesTabs();
            	FillItemRules();

                lstRules.ScrollPosition = HoldScrollPostion;
            }
            catch (Exception ex) { LogError(ex); }


        }

        private void MirrorToXdocRules()
        {
        	try
        	{       
	            xdocRules = new XDocument(new XElement("Rules"));
	
	            try
	            {
		            foreach(XElement el in mPrioritizedRulesList)
		            {
		                xdocRules.Root.Add(el);
		            }
	            }catch (Exception ex) { LogError(ex); }

           		xdocRules.Save(rulesFilename);

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
        		mSelectedRule.Element("Enabled").Value = chkRuleEnabled.Checked.ToString();
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
    }
}