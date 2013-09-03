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
            	mSelectedRule = new XElement("Rule");
            	
            	chkRuleFilterMajor.Checked = false;
            	chkRuleFilterCloak.Checked = false;
            	chkRuleFilterEpic.Checked = false;
            	chkRuleFilterlvl8.Checked = false;
            	chkRuleFilterLegend.Checked = false;

                _UpdateRulesTabs();    
            }catch (Exception ex) { LogError(ex); };
        }

        [ControlEvent("btnRuleNew", "Click")]
        private void btnRuleNew_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
           try 
            {
           		List<int> NumList = new List<int>();
           		foreach(XElement xe in mPrioritizedRulesList)
           		{
           			NumList.Add(Convert.ToInt32(xe.Element("RuleNum").Value));
           		}
           		
           		int NewRuleNumber = 0;
           		for(NewRuleNumber = 0; NewRuleNumber < NumList.Count; )
           		{
           			if(!NumList.Contains(NewRuleNumber)){break;}
           			else{NewRuleNumber++;}
           		}
           		
           		mSelectedRule = new XElement("Rule");
           		mSelectedRule.Element("RuleNum").Value = NewRuleNumber.ToString();
           		                    
           		mPrioritizedRulesList.Add(mSelectedRule);
           		
           		writeToXdocRules(xdocRules);
           		xdocRules.Save(rulesFilename);
  		
            }
            catch (Exception ex) { LogError(ex); }
        }

        [ControlEvent("btnRuleupdate", "Click")]
        private void btnRuleUpdate_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
                int HoldScrollPostion = lstRules.ScrollPosition;
                
                IEnumerable<XElement> elements = xdocRules.Element("Rules").Descendants("Rule");
                //xdocRules.Descendants("Rule").Where(x => x.Element("RuleNum").Value.ToString().Equals(nRuleNum.ToString())).Remove();
                
                writeToXdocRules(xdocRules);
            	xdocRules.Save(rulesFilename);
            	_UpdateRulesTabs();

                lstRules.ScrollPosition = HoldScrollPostion;
            }
            catch (Exception ex) { LogError(ex); }


        }

        //TODO:  Make mPrioritizedRulesLIst reconcile with XDocRules
        private void MirrorToXdocRules()
        {
        	try
        	{
        	
//        		xdoc.Element("Rules").Add(new XElement("Rule",
//                new XElement("RuleNum", xdocRules),
//                new XElement("Enabled", bRuleEnabled),
//                new XElement("Priority", nRulePriority),
//                new XElement("AppliesToFlag", sRuleAppliesTo),
//                new XElement("Name", sRuleName),
//                new XElement("ArcaneLore", nRuleArcaneLore),
//                new XElement("Work", nRuleWork),
//                new XElement("WieldLevel", nRuleWieldLevel),
//                new XElement("WieldSkill", nRuleWieldSkill),
//                new XElement("MasteryType", nRuleMasteryType),
//                new XElement("DamageType", sRuleDamageTypes),
//                new XElement("GearScore", nGearScore),
//                new XElement("WieldEnabled", sRuleWeapons),
//                new XElement("ReqSkill", sRuleReqSkill),
//                new XElement("Slots", sRuleSlots),
//               new XElement("ArmorType", sRuleArmorType),
//                 new XElement("ArmorSet", sRuleArmorSet),
//                new XElement("Spells", sRuleSpells),
//                new XElement("NumSpells", nRuleNumSpells),
//                new XElement("Palettes", sRulePalettes)));	
        	}catch(Exception ex){LogError(ex);}
        }

        
        
        private void writeToXdocRules(XDocument xdoc)
        {
//            xdoc.Element("Rules").Add(new XElement("Rule",
//                new XElement("RuleNum", xdocRules),
//                new XElement("Enabled", bRuleEnabled),
//                new XElement("Priority", nRulePriority),
//                new XElement("AppliesToFlag", sRuleAppliesTo),
//                new XElement("Name", sRuleName),
//                new XElement("ArcaneLore", nRuleArcaneLore),
//                new XElement("Work", nRuleWork),
//                new XElement("WieldLevel", nRuleWieldLevel),
//                new XElement("WieldSkill", nRuleWieldSkill),
//                new XElement("MasteryType", nRuleMasteryType),
//                new XElement("DamageType", sRuleDamageTypes),
//                new XElement("GearScore", nGearScore),
//                new XElement("WieldEnabled", sRuleWeapons),
//                new XElement("ReqSkill", sRuleReqSkill),
//                new XElement("Slots", sRuleSlots),
//               new XElement("ArmorType", sRuleArmorType),
//                 new XElement("ArmorSet", sRuleArmorSet),
//                new XElement("Spells", sRuleSpells),
//                new XElement("NumSpells", nRuleNumSpells),
//                new XElement("Palettes", sRulePalettes)));

        }
//
//        private void getVariables(XElement el)
//        {
//            try
//            {
//                nRuleNum = Convert.ToInt32(el.Element("RuleNum").Value);
//                bRuleEnabled = Convert.ToBoolean(el.Element("Enabled").Value);
//                nRulePriority = Convert.ToInt32(el.Element("Priority").Value);
//                sRuleAppliesTo = el.Element("AppliesToFlag").Value.ToString();
//                sRuleName = (string)el.Element("Name").Value;
//                nRuleArcaneLore = Convert.ToInt32(el.Element("ArcaneLore").Value);
//                nRuleWork = Convert.ToInt32(el.Element("Work").Value);
//                nRuleWieldLevel = Convert.ToInt32(el.Element("WieldLevel").Value);
//                nRuleWieldSkill = Convert.ToInt32(el.Element("WieldSkill").Value);
//                nRuleMasteryType = Convert.ToInt32(el.Element("MasteryType").Value);
//                sRuleDamageTypes = el.Element("DamageType").Value;
//                nGearScore = Convert.ToInt32(el.Element("GearScore").Value);
//                 sRuleWeapons = el.Element("WieldEnabled").Value;
//                sRuleReqSkill = el.Element("ReqSkill").Value;
//                sRuleSlots = (string)el.Element("Slots").Value;
//                sRuleArmorType = (string)el.Element("ArmorType").Value;
//                sRuleArmorSet = (string)el.Element("ArmorSet").Value;
//                sRuleSpells = el.Element("Spells").Value;
//                nRuleNumSpells = Convert.ToInt32(el.Element("NumSpells").Value);
//                sRulePalettes = (string)el.Element("Palettes").Value;
// 
//            }
//            catch (Exception ex) { LogError(ex); }
//
//        }

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