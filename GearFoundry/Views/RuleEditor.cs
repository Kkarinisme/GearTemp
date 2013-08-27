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
                nRuleRow = lstRules.ScrollPosition;
                clearRule();
                lstRules.ScrollPosition = nRuleRow;
            }

            catch (Exception ex) { LogError(ex); }

        }

        [ControlEvent("btnRuleNew", "Click")]
        private void btnRuleNew_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
           try 
            {

                nRuleNum = nNextRuleNum;
                nNextRuleNum++;
                setUpForFindingLists();
                populateRulesListBox();
                nRuleRow =  lstRules.RowCount;
                lstRules.ScrollPosition = nRuleRow;
            }
            catch (Exception ex) { LogError(ex); }
        }

               

        private void setUpForFindingLists()
        {
            sRuleAppliesTo = mFindList(lstRuleApplies, AppliesToList);
            sRuleArmorSet = mFindList(lstRuleSets, ArmorSetsList);
            sRuleDamageTypes = mFindList(lstDamageTypes, ElementalList);
            sRuleSlots = mFindList(lstRuleSlots, SlotList);
            sRuleArmorType = mFindList(lstRuleArmorTypes, ArmorIndex);
            mMakeStrings();
            writeToXdocRules(xdocRules);
            xdocRules.Save(rulesFilename);
        }



        [ControlEvent("btnRuleupdate", "Click")]
        private void btnRuleUpdate_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
                nRuleRow = lstRules.ScrollPosition;
                IEnumerable<XElement> elements = xdocRules.Element("Rules").Descendants("Rule");
                xdocRules.Descendants("Rule").Where(x => x.Element("RuleNum").Value.ToString().Equals(nRuleNum.ToString())).Remove();
                setUpForFindingLists();

                FillItemRules();
                setUpRulesLists(xdocRules, mPrioritizedRulesList, mPrioritizedRulesListEnabled);
 
                lstRules.ScrollPosition = nRuleRow;
            }
            catch (Exception ex) { LogError(ex); }


        }


        private void mMakeStrings()
        {
            try
            {

           
                sRuleReqSkilla = txtRuleReqSkilla.Text.ToString().Trim();
                if (sRuleReqSkilla != String.Empty)
                {
                    sRuleReqSkill = sRuleReqSkilla;
                    sRuleWeapons = sRuleWeaponsa;                    
                }

                else if (sRuleReqSkilla == "")
                {

                    try
                    {
                        sRuleReqSkill = "";
                        sRuleWeapons = "false";
                    }
                    catch (Exception ex) { LogError(ex); }
                }



                if (sRuleReqSkillb != "")
                {
                    sRuleReqSkill = sRuleReqSkill + "," + sRuleReqSkillb;
                    sRuleWeapons = sRuleWeapons + "," + sRuleWeaponsb;
                }
                else if (sRuleReqSkillb == "")
                {
                    setDefaultReqSkill();
                }

                if (sRuleReqSkillc != "")
                {
                    sRuleReqSkill = sRuleReqSkill + "," + sRuleReqSkillc;
                    sRuleWeapons = sRuleWeapons + "," + sRuleWeaponsc;
                }

                else if (sRuleReqSkillc == "")
                {
                    setDefaultReqSkill();
                }
                if (sRuleReqSkilld != "")
                {
                    sRuleReqSkill = sRuleReqSkill + "," + sRuleReqSkilld;
                    sRuleWeapons = sRuleWeapons + "," + sRuleWeaponsd;
                }
                else if (sRuleReqSkilld == "")
                {
                    setDefaultReqSkill();
                }
 


            }
            catch (Exception ex) { LogError(ex); }


        }

        private void setDefaultReqSkill()
        {
            try
            {

                sRuleReqSkill = sRuleReqSkill + "," + "";
                sRuleWeapons = sRuleWeapons + "," + "false";
            }
            catch (Exception ex) { LogError(ex); }

        }


        private void writeToXdocRules(XDocument xdoc)
        {

            xdoc.Element("Rules").Add(new XElement("Rule",
                new XElement("RuleNum",nRuleNum),
                new XElement("Enabled", bRuleEnabled),
                new XElement("Priority", nRulePriority),
                new XElement("AppliesToFlag", sRuleAppliesTo),
                new XElement("Name", sRuleName),
                new XElement("ArcaneLore", nRuleArcaneLore),
                new XElement("Work", nRuleWork),
                new XElement("WieldLevel", nRuleWieldLevel),
                new XElement("WieldSkill", nRuleWieldSkill),
                new XElement("MasteryType", nRuleMasteryType),
                new XElement("DamageType", sRuleDamageTypes),
                new XElement("GearScore", nGearScore),
                new XElement("WieldEnabled", sRuleWeapons),
                new XElement("ReqSkill", sRuleReqSkill),
                new XElement("Slots", sRuleSlots),
               new XElement("ArmorType", sRuleArmorType),
                 new XElement("ArmorSet", sRuleArmorSet),
                new XElement("Unenchantable", bRuleMustBeUnEnchantable),
                new XElement("Spells", sRuleSpells),
                new XElement("NumSpells", nRuleNumSpells),
                new XElement("Palettes", sRulePalettes)));

        }

        private void getVariables(XElement el)
        {
            try
            {
                nRuleNum = Convert.ToInt32(el.Element("RuleNum").Value);
                bRuleEnabled = Convert.ToBoolean(el.Element("Enabled").Value);
                nRulePriority = Convert.ToInt32(el.Element("Priority").Value);
                sRuleAppliesTo = el.Element("AppliesToFlag").Value.ToString();
                sRuleName = (string)el.Element("Name").Value;
                nRuleArcaneLore = Convert.ToInt32(el.Element("ArcaneLore").Value);
                nRuleWork = Convert.ToInt32(el.Element("Work").Value);
                nRuleWieldLevel = Convert.ToInt32(el.Element("WieldLevel").Value);
                nRuleWieldSkill = Convert.ToInt32(el.Element("WieldSkill").Value);
                nRuleMasteryType = Convert.ToInt32(el.Element("MasteryType").Value);
                sRuleDamageTypes = el.Element("DamageType").Value;
                nGearScore = Convert.ToInt32(el.Element("GearScore").Value);
                 sRuleWeapons = el.Element("WieldEnabled").Value;
                sRuleReqSkill = el.Element("ReqSkill").Value;
                sRuleSlots = (string)el.Element("Slots").Value;
                sRuleArmorType = (string)el.Element("ArmorType").Value;
                sRuleArmorSet = (string)el.Element("ArmorSet").Value;
                bRuleMustBeUnEnchantable = Convert.ToBoolean(el.Element("Unenchantable").Value);
                sRuleSpells = el.Element("Spells").Value;
                nRuleNumSpells = Convert.ToInt32(el.Element("NumSpells").Value);
                sRulePalettes = (string)el.Element("Palettes").Value;
 
            }
            catch (Exception ex) { LogError(ex); }

        }

         [ControlEvent("txtRuleName", "End")]
        private void txtRuleName_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            sRuleName = txtRuleName.Text.ToString().Trim();
        }

        [ControlEvent("txtRulePriority", "End")]
        private void txtRulePriority_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            string snum = txtRulePriority.Text;
            int result = 0;
            if (Int32.TryParse(txtRulePriority.Text, out result))
            { nRulePriority = result; }
            else
            { nRulePriority = 0; }

        }


        [ControlEvent("txtRuleMaxCraft", "End")]
        private void txtRuleMaxCraft_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)   //Decal.Adapter.TextBoxEndEventArgs e)
        {
            int result = 0;
            if (int.TryParse(txtRuleMaxCraft.Text, out result))
            {
                nRuleWork = result;
            }
            else
            {
                txtRuleMaxCraft.Text = string.Empty;
                nRuleWork = -1;
            }

        }

        [ControlEvent("txtRuleArcaneLore", "End")]
        private void txtRuleArcaneLore_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)   //Decal.Adapter.TextBoxEndEventArgs e)
        {
            int result = 0;
            if (int.TryParse(txtRuleArcaneLore.Text, out result))
            {
                nRuleArcaneLore = result;
            }
            else
            {
                txtRuleArcaneLore.Text = string.Empty;
                nRuleArcaneLore = -1;
            }

        }
        
        [ControlEvent("txtGearScore", "End")]
        private void txtGearScore_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)   //Decal.Adapter.TextBoxEndEventArgs e)
        {
            int result = 0;
            if (int.TryParse(txtGearScore.Text, out result))
            {
                nGearScore = result;
            }
            else
            {
                txtRuleArcaneLore.Text = string.Empty;
                nGearScore = -1;
            }

        }


        [ControlEvent("txtRuleWieldLevel", "End")]
        private void txtRuleWieldLevel_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {

            int result = 0;
            if (int.TryParse(txtRuleWieldLevel.Text, out result))
            {
                nRuleWieldLevel = result;
            }
            else
            {
                txtRuleWieldLevel.Text = string.Empty;
                nRuleWieldLevel = -1;
            }

        }

        [ControlEvent("chkRuleMustBeUnenchantable", "Change")]
        private void chkRuleMustBeUnenchantable_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {

            bRuleMustBeUnEnchantable = e.Checked;
        }




        private void chkRuleFilterLegend_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            if (chkRuleFilterLegend.Checked)
            {
                bRuleFilterLegend = true;
            }
            else
            { bRuleFilterLegend = false; }
            populateSpellListBox();

        }


        private void chkRuleFilterEpic_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            if (chkRuleFilterEpic.Checked)
            {
                bRuleFilterEpic = true;
            }
            else
            { bRuleFilterEpic = false; }
            populateSpellListBox();

        }
        
        private void chkRuleFilterCloak_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            if (chkRuleFilterCloak.Checked)
            {
                bRuleFilterCloak = true;
            }
            else
            { bRuleFilterCloak = false; }
            populateSpellListBox();

        }
        

        private void chkRuleFilterMajor_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            if (chkRuleFilterMajor.Checked)
            { bRuleFilterMajor = true; }
            else
            { bRuleFilterMajor = false; }
            populateSpellListBox();
        }

        private void chkRuleFilterlvl8_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            if (chkRuleFilterlvl8.Checked)
            { bRuleFilterlvl8 = true; }
            else
            { bRuleFilterlvl8 = false; }
            populateSpellListBox();

        }

        //[ControlEvent("chkRuleEnabled", "Change")]
        private void chkRuleEnabled_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            if (chkRuleEnabled.Checked)
            { bRuleEnabled = true; }
            else
            { bRuleEnabled = false; }

        }
        [ControlEvent("cboWeaponAppliesTo", "Change")]
        private void cboWeaponAppliesTo_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {

                nRuleWieldSkill = (WeaponTypeList[cboWeaponAppliesTo.Selected].ID);
                if(nRuleWieldSkill == 54) {lblRuleReqSkilla.Text = "Essence Level";}
                else{lblRuleReqSkilla.Text = "Skill Level";}
            }
            catch (Exception ex) { LogError(ex); }


        }


        [ControlEvent("cboMasteryType", "Change")]
        private void cboMasteryType_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            nRuleMasteryType = cboMasteryType.Selected;

        }



        [ControlEvent("chkRuleWeaponsa", "Change")]
        private void chkRuleWeaponsa_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            sRuleWeaponsa = "false";
            if (chkRuleWeaponsa.Checked)
            { sRuleWeaponsa = "true"; }
            else
            { sRuleWeaponsa = "false"; }
        }


        [ControlEvent("chkRuleWeaponsb", "Change")]
        private void chkRuleWeaponsb_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            sRuleWeaponsb = "false";
            if (chkRuleWeaponsb.Checked)
            { sRuleWeaponsb = "true"; }
            else
            { sRuleWeaponsb = "false"; }
        }

        [ControlEvent("chkRuleWeaponsc", "Change")]
        private void chkRuleWeaponsc_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            sRuleWeaponsc = "false";
            if (chkRuleWeaponsc.Checked)
            { sRuleWeaponsc = "true"; }
            else
            { sRuleWeaponsc = "false"; }
        }

        [ControlEvent("chkRuleWeaponsd", "Change")]
        private void chkRuleWeaponsd_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            sRuleWeaponsd = "false";
            if (chkRuleWeaponsa.Checked)
            { sRuleWeaponsd = "true"; }
            else
            { sRuleWeaponsd = "false"; }
        }




        [ControlEvent("txtRuleReqSkilla", "End")]
        private void txtRuleReqSkilla_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            sRuleReqSkilla = txtRuleReqSkilla.Text.Trim();

        }

        [ControlEvent("txtRuleReqSkillb", "End")]
        private void txtRuleReqSkillb_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            sRuleReqSkillb = txtRuleReqSkillb.Text.Trim();
        }
        [ControlEvent("txtRuleReqSkillc", "End")]
        private void txtRuleReqSkillc_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            sRuleReqSkillc = txtRuleReqSkillc.Text.Trim();
        }

        [ControlEvent("txtRuleReqSkilld", "End")]
        private void txtRuleReqSkilld_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            sRuleReqSkilld = txtRuleReqSkilld.Text.Trim();
        }
      

      
        //Creates a string of integers separated by columns in listviews in which more than one chosen
        private string mFindList(MyClasses.MetaViewWrappers.IList lstvue, List<IDNameLoadable> lst)
        {
            int id = 0;
            string var;
            bool @checked = false;
            IListRow row;
            id = 0;
            string sid = "";
            @checked = false;
            var = "";

            //need the length of the list to determine how many items to check to determine if chosen
            int n = lst.Count;

            for (int i = 0; i < n; i++)
            {
                   row = lstvue[i];
                    @checked = Convert.ToBoolean(row[0][0]);
                    if (@checked)
                    {
                        id = Convert.ToInt32(row[2][0]);
                        sid = id.ToString();
                        var = var + sid + ",";
                    }
 
            }

            int mLength = var.Length;

            if (mLength > 0)
            {
                var = var.Substring(0, mLength - 1); 
            }


            return var;
        }

        private string mFindList(MyClasses.MetaViewWrappers.IList lstvue, List<spellinfo> lst)
        {
            int id = 0;
            string var;
            bool @checked = false;
            IListRow row;
            id = 0;
            string sid = "";
            @checked = false;
            var = "";

            //need the length of the list to determine how many items to check to determine if chosen
            int n = lst.Count;

            for (int i = 0; i < n; i++)
            {

                row = lstvue[i];
                @checked = Convert.ToBoolean(row[0][0]);
                if (@checked)
                {
                    id = Convert.ToInt32(row[2][0]);
                    sid = id.ToString();
                    var = var + sid + ",";
                }

            }

            int mLength = var.Length;

            if (mLength > 0)
            { var = var.Substring(0, mLength - 1); }


            return var;
        }

        private void mFindAppliestoList()
        {
            int id = 0;
            bool @checked = false;
            IListRow row;
            id = 0;
            string sid = "";
            @checked = false;
            int n = AppliesToList.Count;
            sRuleAppliesTo = "";
            for (int i = 0; i < n; i++)
            {
                row = lstRuleApplies[i];
                @checked = Convert.ToBoolean(row[0][0]);
                if (@checked)
                {
                    id = Convert.ToInt32(row[2][0]);
                    sid = id.ToString();
                    sRuleAppliesTo = sRuleAppliesTo + sid + ",";

                }
            }

            int mLength = sRuleAppliesTo.Length;

            if (mLength > 0)
            { sRuleAppliesTo = sRuleAppliesTo.Substring(0, mLength - 1); }

        }

        [ControlEvent("txtRuleNumSpells", "End")]
        private void txtRuleNumSpells_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
            int d = 0;
            nRuleNumSpells = 0;
            if (int.TryParse(txtRuleNumSpells.Text, out d))
            {
                nRuleNumSpells = d;
            }
            else
            {
                nRuleNumSpells = 0;

            }

            
        }


        //[ControlEvent("txtRuleSpellMatches", "End")]
        //private void txtRuleSpellMatches_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        //{
        //    if (!sRuleSpells.Contains(txtRuleSpellMatches.Text))
        //    {
        //        foreach (spellinfo spell in FilteredSpellIndex)
        //        {
        //            try
        //            {
        //                if (spell.spellname.ToLower().Contains(txtRuleSpellMatches.Text.ToLower()))
        //                {
        //                    string name = spell.spellname;
        //                    int id = spell.spellid;
        //                    nRuleMustHaveSpell = id;
        //                    //if (!sRuleSpells.Contains(nRuleMustHaveSpell.ToString()))
        //                    //{

        //                    WriteEnabledSpellsList(id, name);
        //                    populateRuleSpellEnabledListBox();
        //                    // }
        //                }
        //            }
        //            catch (Exception ex) { LogError(ex); }
        //        }
        //    }
        //}
    }
}