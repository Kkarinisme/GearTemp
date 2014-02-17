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




        private void btnRuleClear_Hit(object sender, EventArgs e)
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


        private void btnRuleNew_Hit(object sender, EventArgs e)
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

        private void btnRuleClone_Hit(object sender, EventArgs e)
        {
           try 
            {
           	
           		XElement XENew = CreateRulesXElement();
           		string rulenumber = XENew.Element("RuleNum").Value;
           		
           		XENew.Element("Enabled").Value = mSelectedRule.Element("Enabled").Value;
                XENew.Element("Priority").Value = mSelectedRule.Element("Priority").Value;
                XENew.Element("AppliesToFlag").Value  = mSelectedRule.Element("AppliesToFlag").Value;
                XENew.Element("ArcaneLore").Value = mSelectedRule.Element("ArcaneLore").Value;
                XENew.Element("Work").Value = mSelectedRule.Element("Work").Value;
                XENew.Element("WieldLevel").Value = mSelectedRule.Element("WieldLevel").Value;
                XENew.Element("WieldSkill").Value = mSelectedRule.Element("WieldSkill").Value;
                XENew.Element("MasteryType").Value = mSelectedRule.Element("MasteryType").Value;
                XENew.Element("DamageType").Value = mSelectedRule.Element("DamageType").Value;
                XENew.Element("GearScore").Value = mSelectedRule.Element("GearScore").Value;
                XENew.Element("WieldEnabled").Value = mSelectedRule.Element("WieldEnabled").Value;
                XENew.Element("ReqSkill").Value = mSelectedRule.Element("ReqSkill").Value;
                XENew.Element("NumSpells").Value = mSelectedRule.Element("NumSpells").Value;
                XENew.Element("Slots").Value = mSelectedRule.Element("Slots").Value;
                XENew.Element("ArmorType").Value = mSelectedRule.Element("ArmorType").Value;
                XENew.Element("ArmorSet").Value = mSelectedRule.Element("ArmorSet").Value;
                XENew.Element("Spells").Value = mSelectedRule.Element("Spells").Value;
                XENew.Element("Palettes").Value = mSelectedRule.Element("Palettes").Value;
                mPrioritizedRulesList.Add(XENew);
           	
                mSelectedRule = mPrioritizedRulesList.Find(x => x.Element("RuleNum").Value == rulenumber);
           		
           		SyncAndSaveRules();
           		_UpdateRulesTabs();		
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void btnRuleUpdate_Hit(object sender, EventArgs e)
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

   //     [ControlEvent("txtRuleName", "End")]
        private void txtRuleName_LostFocus(object sender, System.EventArgs e)
        {
        	mSelectedRule.Element("Name").Value = txtRuleName.Text.ToString().Trim();
        }

   //     [ControlEvent("txtRulePriority", "End")]
        private void txtRulePriority_LostFocus(object sender, System.EventArgs e)
        {
        	try
        	{
        		mSelectedRule.Element("Priority").Value = txtRulePriority.Text;
        	}catch(Exception ex){mSelectedRule.Element("Priority").Value = "-1"; LogError(ex);}
        }
      
    //    [ControlEvent("txtRuleMaxCraft", "End")]
        private void txtRuleMaxCraft_LostFocus(object sender, System.EventArgs e)
        {
        	try
        	{
        		mSelectedRule.Element("Work").Value = txtRuleMaxCraft.Text;
        	}catch(Exception ex){mSelectedRule.Element("Work").Value = "-1"; LogError(ex);}
        }

   //     [ControlEvent("txtRuleArcaneLore", "End")]
        private void txtRuleArcaneLore_LostFocus(object sender, System.EventArgs e)
        {
        	try
        	{
        		mSelectedRule.Element("ArcaneLore").Value = txtRuleArcaneLore.Text;
        	}catch(Exception ex){mSelectedRule.Element("ArcaneLore").Value = "-1"; LogError(ex);}
        }
        
   //     [ControlEvent("txtGearScore", "End")]
        private void txtGearScore_LostFocus(object sender, System.EventArgs e)
        {
        	try
        	{
        		mSelectedRule.Element("GearScore").Value = txtGearScore.Text;
        	}catch(Exception ex){mSelectedRule.Element("GearScore").Value = "-1"; LogError(ex);}
        }

     //   [ControlEvent("txtRuleWieldLevel", "End")]
        private void txtRuleWieldLevel_LostFocus(object sender, System.EventArgs e)
        {
        	try
        	{
        		mSelectedRule.Element("WieldLevel").Value = txtRuleWieldLevel.Text;
        	}catch(Exception ex){mSelectedRule.Element("WieldLevel").Value = "-1";LogError(ex);}

        }

        private void chkRuleFilterLegend_Change(object sender, EventArgs e)
        {
            _UpdateRulesTabs();
        }

        private void chkRuleFilterEpic_Change(object sender, EventArgs e)
        {
            _UpdateRulesTabs();
        }

        private void chkRuleFilterCloak_Change(object sender, EventArgs e)
        {
            _UpdateRulesTabs();
        }

        private void chkRuleFilterMajor_Change(object sender, EventArgs e)
        {
            _UpdateRulesTabs();
        }

        private void chkRuleFilterlvl8_Change(object sender, EventArgs e)
        {
        	_UpdateRulesTabs();
        }

        private void chkRuleEnabled_Change(object sender, EventArgs e)
        {
        	try
        	{
        		mSelectedRule.Element("Enabled").Value = chkRuleEnabled.Checked.ToString().ToLower();
        	}catch(Exception ex){LogError(ex);}

        }
        //void cboArmorSet_Change(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        objArmorSet = 0;
        //        objArmorSetName = "";
        //        objArmorSet = ArmorSetsInvList[cboArmorSet.Current].ID;
        //        objArmorSetName = ArmorSetsInvList[cboArmorSet.Current].name;
        //        //    WriteToChat("objSet: " + objArmorSet.ToString());


        //    }
        //    catch (Exception ex) { LogError(ex); }
        //}

        
        private void cboWeaponAppliesTo_Change(object sender, EventArgs e)
        {
            try
            {
                objWeaponAppliesTo = 0;
                objWeaponAppliesToName = "";
                objWeaponAppliesTo = WeaponTypeList[cboWeaponAppliesTo.Current].ID;
                objWeaponAppliesToName = WeaponTypeList[cboWeaponAppliesTo.Current].name;
               // mSelectedRule.Element("WieldSkill").Value = WeaponTypeList[cboWeaponAppliesTo.Current].ID.ToString();
              //  if (objWeaponAppliesTo.ToString() == "54") { lblRuleReqSkilla.Text = "Essence Level"; }
              //  else { lblRuleReqSkilla.Text = "Skill Level"; }
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void cboMasteryType_Change(object sender, EventArgs e)
        {
            try
            {
                objMasteryType = 0;
                objMasteryTypeName = "";
                objMasteryType = MasteryIndex[cboWeaponAppliesTo.Current].ID;
                objMasteryTypeName = MasteryIndex[cboWeaponAppliesTo.Current].name;
                // mSelectedRule.Element("WieldSkill").Value = WeaponTypeList[cboWeaponAppliesTo.Current].ID.ToString();
                //  if (objWeaponAppliesTo.ToString() == "54") { lblRuleReqSkilla.Text = "Essence Level"; }
                //  else { lblRuleReqSkilla.Text = "Skill Level"; }
            }
            catch (Exception ex) { LogError(ex); }
        }

        //[ControlEvent("cboMasteryType", "Change")]
        //private void cboMasteryType_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        //{
        //    mSelectedRule.Element("MasteryType").Value = cboMasteryType.Current.ToString();
        //}

        private void chkRuleWeaponsa_Change(object sender, System.EventArgs e)  
        {
            mSelectedRule.Element("WieldEnabled").Value = chkRuleWeaponsa.Checked.ToString() + "," + chkRuleWeaponsb.Checked.ToString() + "," +
                 chkRuleWeaponsc.Checked.ToString() + "," + chkRuleWeaponsd.Checked.ToString();
        }

        private void chkRuleWeaponsb_Change(object sender, System.EventArgs e) 
        {
            mSelectedRule.Element("WieldEnabled").Value = chkRuleWeaponsa.Checked.ToString() + "," + chkRuleWeaponsb.Checked.ToString() + "," +
                chkRuleWeaponsc.Checked.ToString() + "," + chkRuleWeaponsd.Checked.ToString();
        }

        private void chkRuleWeaponsc_Change(object sender, System.EventArgs e)  
        {
            mSelectedRule.Element("WieldEnabled").Value = chkRuleWeaponsa.Checked.ToString() + "," + chkRuleWeaponsb.Checked.ToString() + "," +
                 chkRuleWeaponsc.Checked.ToString() + "," + chkRuleWeaponsd.Checked.ToString();
        }

        private void chkRuleWeaponsd_Change(object sender, System.EventArgs e) 
        {
            mSelectedRule.Element("WieldEnabled").Value = chkRuleWeaponsa.Checked.ToString() + "," + chkRuleWeaponsb.Checked.ToString() + "," +
                chkRuleWeaponsc.Checked.ToString() + "," + chkRuleWeaponsd.Checked.ToString();
        }

        private void txtRuleReqSkilla_LostFocus(object sender, EventArgs e)  
        {
            mSelectedRule.Element("ReqSkill").Value = txtRuleReqSkilla.Text.Trim() + "," + txtRuleReqSkillb.Text.Trim() + "," +
                                     txtRuleReqSkillc.Text.Trim() + "," + txtRuleReqSkilld.Text.Trim();

        }

        private void txtRuleReqSkillb_LostFocus(object sender, EventArgs e)
        {
            mSelectedRule.Element("ReqSkill").Value = txtRuleReqSkilla.Text.Trim() + "," + txtRuleReqSkillb.Text.Trim() + "," +
                                    txtRuleReqSkillc.Text.Trim() + "," + txtRuleReqSkilld.Text.Trim();
        }

        private void txtRuleReqSkillc_LostFocus(object sender, EventArgs e)
        {
            mSelectedRule.Element("ReqSkill").Value = txtRuleReqSkilla.Text.Trim() + "," + txtRuleReqSkillb.Text.Trim() + "," +
                                    txtRuleReqSkillc.Text.Trim() + "," + txtRuleReqSkilld.Text.Trim();
        }

        private void txtRuleReqSkilld_LostFocus(object sender, EventArgs e)
        {
            mSelectedRule.Element("ReqSkill").Value = txtRuleReqSkilla.Text.Trim() + "," + txtRuleReqSkillb.Text.Trim() + "," +
                                    txtRuleReqSkillc.Text.Trim() + "," + txtRuleReqSkilld.Text.Trim();
        }

        //[ControlEvent("txtRuleNumSpells", "End")]

        private void txtRuleNumSpells_LostFocus(object sender, System.EventArgs e)
        {
            try
            {
                Convert.ToInt32(txtRuleNumSpells.Text);
                mSelectedRule.Element("NumSpells").Value = txtRuleNumSpells.Text;
            }
            catch { mSelectedRule.Element("NumSpells").Value = "-1"; }
        }
        
        
        private string[] KeyTypes = {"Double","Long"};
        private string[] KeyCompare = {"Equals","Not Equals","Equals or Greater","Equals or Less"};
        private string[] KeyLink = {"End","And","Or"};

        
        //todo Paul please check the number I should have put after item in AddItem(item,?)
        private void chkAdvEnabled_Change(object sender, System.EventArgs e)  
        {
        	try
        	{
                if (chkAdvEnabled.Checked)  
	        	{
                    cboAdv1KeyType.Clear();
                    cboAdv1KeyCompare.Clear();
                    cboAdv1Key.Clear();
                    cboAdv1Link.Clear();
                    txtAdv1KeyValue.Text = String.Empty;
                    foreach (string item in KeyTypes) { cboAdv1KeyType.AddItem(item, 1); }
               //     FillAdvancedKeyList(cboAdv1KeyType.Current, cboAdv1Key);
                    foreach (string item in KeyCompare) { cboAdv1KeyCompare.AddItem(item,1); }
                    foreach (string item in KeyLink) { cboAdv1Link.AddItem(item,1); }	        		
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



        private void cboAdv1KeyType_Change(object sender, EventArgs e)
        {
            try
            {
             //   FillAdvancedKeyList(cboAdv1KeyType.Current, cboAdv1Key);
                Update_mSelected_Advanced();
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void cboAdv1Key_Change(object sender, EventArgs e)
        {
            Update_mSelected_Advanced();
        }

        private void cboAdv1KeyCompare_Change(object sender, EventArgs e)
        {
            Update_mSelected_Advanced();
        }

        private void txtAdv1KeyValue_LostFocus(object sender, EventArgs e)
        {
            Update_mSelected_Advanced();
        }   

        private void cboAdv1Link_Change(object sender, EventArgs e)
        {
            try
            {
                if (cboAdv1Link.Current != 0)
                {
                    foreach (string item in KeyTypes) { cboAdv2KeyType.AddItem(item, 1); }
                    foreach (string item in KeyCompare) { cboAdv2KeyCompare.AddItem(item, 1); }
                //    FillAdvancedKeyList(cboAdv2KeyType.Current, cboAdv2Key);
                    foreach (string item in KeyLink) { cboAdv2Link.AddItem(item,1); }
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
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void cboAdv2KeyType_Change(object sender, EventArgs e)
        {
            try
            {
                if (cboAdv2KeyType.Current == 0) { cboAdv2Key.Clear(); }
                else
                {
               //     FillAdvancedKeyList(cboAdv2KeyType.Current, cboAdv2Key);
                }
                Update_mSelected_Advanced();
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void cboAdv2Key_Change(object sender, EventArgs e)
        {
            Update_mSelected_Advanced();
        }

        private void cboAdv2KeyCompare_Change(object sender, EventArgs e)
        {
            Update_mSelected_Advanced();
        }

        private void txtAdv2KeyValue_LostFocus(object sender, EventArgs e)
        {
            Update_mSelected_Advanced();
        }

        private void cboAdv2Link_Change(object sender, EventArgs e)
        {
            try
            {
                if (cboAdv2Link.Current != 0)
                {
                    foreach (string item in KeyTypes) { cboAdv3KeyType.AddItem(item,1); }
                    foreach (string item in KeyCompare) { cboAdv3KeyCompare.AddItem(item,1); }
                 //   FillAdvancedKeyList(cboAdv3KeyType.Current, cboAdv3Key);
                    foreach (string item in KeyLink) { cboAdv3Link.AddItem(item,1); }
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
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void cboAdv3KeyType_Change(object sender, EventArgs e)
        {
            try
            {
                if (cboAdv3KeyType.Current == 0) { cboAdv3Key.Clear(); }
                else
                {
                    //FillAdvancedKeyList(cboAdv3KeyType.Current, cboAdv3Key);
                }
                Update_mSelected_Advanced();
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void cboAdv3Key_Change(object sender, EventArgs e)
        {
            Update_mSelected_Advanced();
        }

        private void cboAdv3KeyCompare_Change(object sender, EventArgs e)
        {
            Update_mSelected_Advanced();
        }

        private void txtAdv3KeyValue_LostFocus(object sender, EventArgs e)
        {
            Update_mSelected_Advanced();
        }

        private void cboAdv3Link_Change(object sender, EventArgs e)
        {
            try
            {
                if (cboAdv3Link.Current != 0)
                {
                    foreach (string item in KeyTypes) { cboAdv4KeyType.AddItem(item,1); }
                    foreach (string item in KeyCompare) { cboAdv4KeyCompare.AddItem(item,1); }
                 //   FillAdvancedKeyList(cboAdv3KeyType.Current, cboAdv3Key);
                    foreach (string item in KeyLink) { cboAdv4Link.AddItem(item,1); }

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
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void cboAdv4KeyType_Change(object sender, EventArgs e)
        {
            try
            {
                if (cboAdv4KeyType.Current == 0) { cboAdv4Key.Clear(); }
                else
                {
                  //  FillAdvancedKeyList(cboAdv4KeyType.Current, cboAdv4Key);
                }
                Update_mSelected_Advanced();
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void cboAdv4Key_Change(object sender, EventArgs e)
        {
            Update_mSelected_Advanced();
        }

        private void cboAdv4KeyCompare_Change(object sender, EventArgs e)
        {
            Update_mSelected_Advanced();
        }

        private void txtAdv4KeyValue_LostFocus(object sender, EventArgs e)
        {
            Update_mSelected_Advanced();
        }

        private void cboAdv4Link_Change(object sender, EventArgs e)
        {
            try
            {
                if (cboAdv4Link.Current != 0)
                {
                    foreach (string item in KeyTypes) { cboAdv5KeyType.AddItem(item, 1); }
                    foreach (string item in KeyCompare) { cboAdv5KeyCompare.AddItem(item, 1); }
                    //   FillAdvancedKeyList(cboAdv3KeyType.Current, cboAdv3Key);

                 }
                else
                {
                    cboAdv5KeyType.Clear();
                    cboAdv5KeyCompare.Clear();
                    cboAdv5KeyType.Clear();
                    txtAdv5KeyValue.Text = String.Empty;
                }
                Update_mSelected_Advanced();
            }
            catch (Exception ex) { LogError(ex); }

        }

        private void cboAdv5KeyType_Change(object sender, EventArgs e)
        {
            try
            {
                if (cboAdv5KeyType.Current == 0) { cboAdv5Key.Clear(); }
                else
                {
                  //  FillAdvancedKeyList(cboAdv5KeyType.Current, cboAdv5Key);
                }
                Update_mSelected_Advanced();
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void cboAdv5Key_Change(object sender, EventArgs e)
        {
            Update_mSelected_Advanced();
        }

        private void cboAdv5KeyCompare_Change(object sender, EventArgs e)
        {
            Update_mSelected_Advanced();
        }

        private void txtAdv5KeyValue_LostFocus(object sender, EventArgs e)
        {
            Update_mSelected_Advanced();
        }
      
        //
     //  Paul canot figure out what I am adding to list
       // private void FillAdvancedKeyList(int selection, MyClasses.MetaViewWrappers.ICombo cboList)
        private void FillAdvancedKeyList(int selection, List< IDNameLoadable> cboList)
        {
            try
            {
                switch (selection)
                {
                    case 0:
                        cboList.Clear();
                 //       foreach (IDNameLoadable idl in DoubleKeyList) { cboList.Add(idl.name,0); }
                        break;
                    case 1:
                        cboList.Clear();
                  //      foreach (IDNameLoadable idl in LongKeyList) { cboList.Add(idl.name,1); }
                        break;
                }

            }
            catch (Exception ex) { LogError(ex); }
        }

        private void Update_mSelected_Advanced()
        {
            try
            {
                string savestring = String.Empty;
                if (!chkAdvEnabled.Checked) { savestring = "false"; }
                else
                {
                    savestring = "true," + AdvancedStringSegement(cboAdv1KeyType.Current, cboAdv1Key.Current) +
                        KeyCompare[cboAdv1KeyCompare.Current] + ":" + txtAdv1KeyValue.Text + ":" + KeyLink[cboAdv1Link.Current];

                    if (cboAdv1Link.Current != 0)
                    {
                        savestring += "," + AdvancedStringSegement(cboAdv2KeyType.Current, cboAdv2Key.Current) +
                        KeyCompare[cboAdv2KeyCompare.Current] + ":" + txtAdv2KeyValue.Text + ":" + KeyLink[cboAdv2Link.Current];

                        if (cboAdv2Link.Current != 0)
                        {
                            savestring += "," + AdvancedStringSegement(cboAdv3KeyType.Current, cboAdv3Key.Current) +
                            KeyCompare[cboAdv3KeyCompare.Current] + ":" + txtAdv3KeyValue.Text + ":" + KeyLink[cboAdv3Link.Current];

                            if (cboAdv3Link.Current != 0)
                            {
                                savestring += "," + AdvancedStringSegement(cboAdv4KeyType.Current, cboAdv4Key.Current) +
                                KeyCompare[cboAdv4KeyCompare.Current] + ":" + txtAdv4KeyValue.Text + ":" + KeyLink[cboAdv4Link.Current];

                                if (cboAdv4Link.Current != 0)
                                {
                                    savestring += "," + AdvancedStringSegement(cboAdv5KeyType.Current, cboAdv5Key.Current) +
                                    KeyCompare[cboAdv5KeyCompare.Current] + ":" + txtAdv5KeyValue.Text + ":" + KeyLink[0];
                                }
                            }
                        }
                    }
                }
                mSelectedRule.Element("Advanced").Value = savestring;
            }
            catch (Exception ex) { LogError(ex); }

        }

        private string AdvancedStringSegement(int AdvKeyType, int AdvKeyListValue)
        {
            string result = String.Empty;
            switch (AdvKeyType)
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