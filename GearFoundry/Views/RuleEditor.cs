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
    	
    	     
        //From: Karin.  Lists for use with XDocuments composed of XElements
        private List<XElement> mSortedMobsList = new List<XElement>();
        private List<XElement> mSortedTrophiesList = new List<XElement>();
        private List<XElement> mSortedSalvageList = new List<XElement>();
        private List<XElement> mSortedMobsListChecked = new List<XElement>();
        private List<XElement> mSortedTrophiesListChecked = new List<XElement>();
        private List<XElement> mSortedSalvageListChecked = new List<XElement>();
        private List<XElement> mPrioritizedRulesList = new List<XElement>();
        private List<XElement> mPrioritizedRulesListEnabled = new List<XElement>();
        private List<XElement> mGenSettingsList = new List<XElement>();
        private List<XElement> mSwitchGearSettingsList = new List<XElement>();

        //From: Karin.  Lists for use with cboboxes using IDNameLoadable class
        private static List<IDNameLoadable> ClassInvList= new List<IDNameLoadable>();
        private static List<IDNameLoadable> MeleeTypeInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> ArmorSetsInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> MaterialInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> ElementalInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> ArmorLevelInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> SalvageWorkInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> WeaponWieldInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> CoverageInvList = new List<IDNameLoadable>();
        private static List<IDNameLoadable> EmbueInvList = new List<IDNameLoadable>();
    	    	

        [ControlEvent("btnRuleClear", "Click")]
        private void btnRuleClear_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
                clearRule();
            }

            catch (Exception ex) { LogError(ex); }

        }

        [ControlEvent("btnRuleNew", "Click")]
        private void btnRuleNew_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
           try 
            {
                WriteToChat("I have clicked button to add a new rule");

                sRuleAppliesTo = mFindList(lstRuleApplies, AppliesToList);
                sRuleArmorSet = mFindList(lstRuleSets, ArmorSetsList);
                sRuleDamageTypes = mFindList(lstDamageTypes, ElementalList);
                sRuleArmorCoverage = mFindList(lstRuleArmorCoverages, ArmorCoverageList);
                sRuleArmorType = mFindList(lstRuleArmorTypes, ArmorIndex);
               // sRuleCloakSets = mFindList(lstRuleCloakSets, CloakSetsList);
               // sRuleCloakSpells = mFindList(lstRuleCloakSpells, CloakSpellList);
                mMakeStrings();
                writeToXdocRules(xdocRules);
                xdocRules.Save(rulesFilename);
                populateRulesListBox();
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void mMakeStrings()
        {
            try
            {
                sRuleMinMaxa = txtRuleMinMaxa.Text.ToString().Trim();
                sRuleReqSkilla = txtRuleReqSkilla.Text.ToString().Trim();
                if (sRuleReqSkilla != "")
                {
                    sRuleMinMax = sRuleMinMaxa;
                    sRuleReqSkill = sRuleReqSkilla;
                    sRuleMSCleave = sRuleMSCleavea;
                    sRuleWeapons = sRuleWeaponsa;
                }
                if (sRuleReqSkillb != "")
                {
                    sRuleMinMax = sRuleMinMax + "," + sRuleMinMaxb;
                    sRuleReqSkill = sRuleReqSkill + "," + sRuleReqSkillb;
                    sRuleMSCleave = sRuleMSCleave + "," + sRuleMSCleaveb;
                    sRuleWeapons = sRuleWeapons + "," + sRuleWeaponsb;
                }
                if (sRuleReqSkillc != "")
                {
                    sRuleMinMax = sRuleMinMax + "," + sRuleMinMaxc;
                    sRuleReqSkill = sRuleReqSkill + "," + sRuleReqSkillc;
                    sRuleMSCleave = sRuleMSCleave + "," + sRuleMSCleavec;
                    sRuleWeapons = sRuleWeapons + "," + sRuleWeaponsc;
                }

                if (sRuleReqSkilld != "")
                {
                    sRuleMinMax = sRuleMinMax + "," + sRuleMinMaxd;
                    sRuleReqSkill = sRuleReqSkill + "," + sRuleReqSkilld;
                    sRuleMSCleave = sRuleMSCleave + "," + sRuleMSCleaved;
                    sRuleWeapons = sRuleWeapons + "," + sRuleWeaponsd;
                }

            }
            catch (Exception ex) { LogError(ex); }


        }

        private void writeToXdocRules(XDocument xdoc)
        {

            xdoc.Element("Rules").Add(new XElement("Rule",
                new XElement("Enabled", bRuleEnabled),
                new XElement("Priority", nRulePriority),
                new XElement("AppliesToFlag", sRuleAppliesTo),
                new XElement("Name", sRuleName),
                new XElement("NameContains", sRuleKeyWords),
                new XElement("NameNotContains", sRuleKeyWordsNot),
                new XElement("Descr", sRuleDescr),
                new XElement("ArcaneLore", nRuleArcaneLore),
                new XElement("Value", nRuleValue),
                new XElement("Burden", nRuleBurden),
                new XElement("Work", nRuleWork),
                new XElement("WieldReqValue", nRuleWieldReqValue),
                new XElement("WieldLevel", nRuleWieldLevel),
                new XElement("ItemLevel", nRuleItemLevel),
                new XElement("MinArmorLevel", nRuleMinArmorLevel),
                new XElement("WieldAttribute", nRuleWieldAttribute),
                new XElement("MasteryType", nRuleMasteryType),
                new XElement("DamageType", sRuleDamageTypes),
                new XElement("McModAttack", nRuleMcModAttack),
                new XElement("MeleeDef", nRuleMeleeD),
                new XElement("MagicDef", nRuleMagicD),
                new XElement("WieldEnabled", sRuleWeapons),
                new XElement("ReqSkill", sRuleReqSkill),
                new XElement("MinMax", sRuleMinMax),
                new XElement("MSCleave", sRuleMSCleave),
                new XElement("Coverage", sRuleArmorCoverage),
               new XElement("ArmorType", sRuleArmorType),
                 new XElement("ArmorSet", sRuleArmorSet),
                new XElement("Unenchantable", bRuleMustBeUnEnchantable),
                new XElement("MustHaveSpell", nRuleMustHaveSpell),
                 new XElement("CloakSets", sRuleCloakSets),
                 new XElement("CloakSpells", sRuleCloakSpells),
                 new XElement("Red", bRuleRed),
                 new XElement("Yellow", bRuleYellow),
                 new XElement("Blue", bRuleBlue),
                 new XElement("EssMastery", nRuleEssMastery),
               new XElement("EssElements", sRuleEssElements),
               new XElement("EssLevel", nRuleEssLevel),
               new XElement("EssSummLevel", nRuleEssSummLevel),
               new XElement("EssDamageLevel", nRuleEssDamageLevel),
               new XElement("EssCDLevel", nRuleEssCDLevel),
               new XElement("EssCRLevel", nRuleEssCRLevel),
               new XElement("EssDRLevel", nRuleEssDRLevel),
               new XElement("EssCritLevel", nRuleEssCritLevel),
                new XElement("FilterLegend", bRuleFilterLegend),
                new XElement("FilterEpic", bRuleFilterEpic),
                new XElement("FilterMajor", bRuleFilterMajor),
                new XElement("FilterLvl8", bRuleFilterlvl8),
                new XElement("FilterLvl7", bRuleFilterlvl7),
                new XElement("FilterLvl6", bRuleFilterlvl6),
                new XElement("Spells", sRuleSpells),
                new XElement("NumSpells", nRuleNumSpells)));

        }

        private void getVariables(XElement el)
        {
            try
            {
                bRuleEnabled = Convert.ToBoolean(el.Element("Enabled").Value);
                nRulePriority = Convert.ToInt32(el.Element("Priority").Value);
                sRuleAppliesTo = el.Element("AppliesToFlag").Value.ToString();
                sRuleName = el.Element("Name").Value.ToString();
                sRuleKeyWords = el.Element("NameContains").Value.ToString();
                sRuleKeyWordsNot = el.Element("NameNotContains").Value.ToString();
                sRuleDescr = el.Element("Descr").Value.ToString();
                nRuleArcaneLore = Convert.ToInt32(el.Element("ArcaneLore").Value);
                nRuleValue = Convert.ToInt32(el.Element("Value").Value);
                nRuleBurden = Convert.ToInt32(el.Element("Burden").Value);
                nRuleWork = Convert.ToInt32(el.Element("Work").Value);
                nRuleWieldReqValue = Convert.ToInt32(el.Element("WieldReqValue").Value);
                nRuleWieldLevel = Convert.ToInt32(el.Element("WieldLevel").Value);
                nRuleItemLevel = Convert.ToInt32(el.Element("ItemLevel").Value);
                nRuleMinArmorLevel = Convert.ToInt32(el.Element("MinArmorLevel").Value);
                //bRuleTradeBotOnly = Convert.ToBoolean(el.Element("TradeBotOnly").Value);
                //bRuleTradeBot = Convert.ToBoolean(el.Element("UseTradeBot").Value);
                nRuleWieldAttribute = Convert.ToInt32(el.Element("WieldAttribute").Value);
                nRuleMasteryType = Convert.ToInt32(el.Element("MasteryType").Value);
                sRuleDamageTypes = el.Element("DamageType").Value;
                nRuleMcModAttack = Convert.ToInt32(el.Element("McModAttack").Value);
                nRuleMeleeD = Convert.ToInt32(el.Element("MeleeDef").Value);
                nRuleMagicD = Convert.ToInt32(el.Element("MagicDef").Value);
                sRuleMinMax = el.Element("MinMax").Value;
                sRuleMSCleave = el.Element("MSCleave").Value;
                sRuleReqSkill = el.Element("ReqSkill").Value;
                sRuleWeapons = el.Element("WieldEnabled").Value;
                sRuleArmorType = el.Element("ArmorType").Value.ToString();
                sRuleArmorSet = el.Element("ArmorSet").Value.ToString();
                sRuleArmorCoverage = (el.Element("Coverage").Value).ToString();
                bRuleMustBeUnEnchantable = Convert.ToBoolean(el.Element("Unenchantable").Value);
                bRuleRed = Convert.ToBoolean(el.Element("Red").Value);
                bRuleYellow = Convert.ToBoolean(el.Element("Yellow").Value);
                bRuleBlue = Convert.ToBoolean(el.Element("Blue").Value);
                sRuleCloakSets = (el.Element("CloakSets").Value.ToString());
                sRuleCloakSpells = (el.Element("CloakSpells").Value.ToString());
                nRuleEssMastery = Convert.ToInt32(el.Element("EssMastery").Value);
                sRuleEssElements = el.Element("EssElements").Value.ToString();
                nRuleEssLevel = Convert.ToInt32(el.Element("EssLevel").Value);
                nRuleEssSummLevel = Convert.ToInt32(el.Element("EssSummLevel").Value);
                nRuleEssDamageLevel = Convert.ToInt32(el.Element("EssDamageLevel").Value);
                nRuleEssCDLevel = Convert.ToInt32(el.Element("EssCDLevel").Value);
                nRuleEssCRLevel = Convert.ToInt32(el.Element("EssCRLevel").Value);
                nRuleEssDRLevel = Convert.ToInt32(el.Element("EssDRLevel").Value);
                nRuleEssCritLevel = Convert.ToInt32(el.Element("EssCritLevel").Value);
                nRuleMustHaveSpell = Convert.ToInt32(el.Element("MustHaveSpell").Value);
                sRuleSpells = el.Element("Spells").Value;
                nRuleNumSpells = Convert.ToInt32(el.Element("NumSpells").Value);
                bRuleFilterLegend = Convert.ToBoolean(el.Element("FilterLegend").Value);
                bRuleFilterEpic = Convert.ToBoolean(el.Element("FilterEpic").Value);
                bRuleFilterMajor = Convert.ToBoolean(el.Element("FilterMajor").Value);
                bRuleFilterlvl8 = Convert.ToBoolean(el.Element("FilterLvl8").Value);
                bRuleFilterlvl7 = Convert.ToBoolean(el.Element("FilterLvl7").Value);
                bRuleFilterlvl6 = Convert.ToBoolean(el.Element("FilterLvl6").Value);

            }
            catch (Exception ex) { LogError(ex); }

        }



        [ControlEvent("btnRuleupdate", "Click")]
        private void btnRuleUpdate_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            try
            {
                IEnumerable<XElement> elements = xdocRules.Element("Rules").Descendants("Rule");
                xdocRules.Descendants("Rule").Where(x => x.Element("Name").Value.ToString().Trim().Contains(sRuleName.Trim())).Remove();
                sRuleAppliesTo = mFindList(lstRuleApplies, AppliesToList);
                sRuleArmorSet = mFindList(lstRuleSets, ArmorSetsList);
                sRuleDamageTypes = mFindList(lstDamageTypes, ElementalList);
                sRuleArmorCoverage = mFindList(lstRuleArmorCoverages, ArmorCoverageList);
                sRuleArmorType = mFindList(lstRuleArmorTypes, ArmorIndex);
                sRuleCloakSets = mFindList(lstRuleCloakSets, CloakSetsList);
                sRuleCloakSpells = mFindList(lstRuleCloakSpells, CloakSpellList);
                sRuleEssElements = mFindList(lstRuleEssElements, EssElementsList);
                mMakeStrings();

                writeToXdocRules(xdocRules);
                xdocRules.Save(rulesFilename);
                GearFoundry.PluginCore.WriteToChat("xdocRules Updated file added.");
                populateRulesListBox();
                FillItemRules();
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

        [ControlEvent("txtRuleDescr", "End")]
        private void txtRuleDescr_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e) //Decal.Adapter.TextBoxEndEventArgs e)
        {
            sRuleDescr = txtRuleDescr.Text.ToString().Trim();
        }

        [ControlEvent("txtRuleMaxBurden", "End")]
        private void txtRuleMaxBurden_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  // Decal.Adapter.TextBoxEndEventArgs e)
        {
            int result = 0;
            if (int.TryParse(txtRuleMaxBurden.Text, out result))
            {
                nRuleBurden = result;
            }
            else
            {
                txtRuleMaxBurden.Text = string.Empty;
                nRuleBurden = -1;
            }

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

        [ControlEvent("txtRulePrice", "End")]
        private void txtRulePrice_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            int result = 0;
            if (int.TryParse(txtRulePrice.Text, out result))
            {
                nRuleValue = result;
            }
            else
            {
                txtRulePrice.Text = string.Empty;
                nRuleValue = -1;
            }

        }


        [ControlEvent("txtRuleWieldReqValue", "End")]
        private void txtRuleWieldReqValue_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {

            int result = 0;
            if (int.TryParse(txtRuleWieldReqValue.Text, out result))
            {
                nRuleWieldReqValue = result;
            }
            else
            {
                txtRuleWieldReqValue.Text = string.Empty;
                nRuleWieldReqValue = -1;
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

        [ControlEvent("txtRuleItemLevel", "End")]
        private void txtRuleItemLevel_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            int result = 0;
            if (int.TryParse(txtRuleItemLevel.Text, out result))
            {
                nRuleItemLevel = result;
            }
            else
            {
                txtRuleItemLevel.Text = string.Empty;
                nRuleItemLevel = -1;
            }

        }

        [ControlEvent("txtRuleMinArmorLevel", "End")]
        private void txtRuleMinArmorLevel_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            int result = 0;
            if (int.TryParse(txtRuleMinArmorLevel.Text, out result))
            {
                nRuleMinArmorLevel = result;
            }
            else
            {
                txtRuleMinArmorLevel.Text = string.Empty;
                nRuleMinArmorLevel = -1;
            }

        }


        [ControlEvent("txtRuleKeyWordsNot", "End")]
        private void txtRuleKeyWordsNot_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            sRuleKeyWordsNot = txtRuleKeyWordsNot.Text.Trim();
        }


        [ControlEvent("txtRuleKeywords", "End")]
        private void txtRuleKeywords_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            sRuleKeyWords = txtRuleKeywords.Text;
        }

        [ControlEvent("chkRuleMustBeUnenchantable", "Change")]
        private void chkRuleMustBeUnenchantable_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {

            bRuleMustBeUnEnchantable = e.Checked;
        }

        [ControlEvent("chkRuleCloakMustHaveSpell", "Change")]
        private void chkRuleCloakMustHaveSpell_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {

            bRuleCloakMustHaveSpell = e.Checked;
        }

        [ControlEvent("chkRuleRed", "Change")]
        private void chkRuleRed_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {

            bRuleRed = e.Checked;
        }
        [ControlEvent("chkRuleYellow", "Change")]
        private void chkRuleYellow_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {

            bRuleYellow = e.Checked;
        }
        [ControlEvent("chkRuleBlue", "Change")]
        private void chkRuleBlue_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {

            bRuleBlue = e.Checked;
        }

        [ControlEvent("txtRuleEssLevel", "End")]
        private void txtRuleEssLevel_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  
        {
            int result = 0;
            if (int.TryParse(txtRuleEssLevel.Text, out result))
            {
                nRuleEssLevel = result;
            }
            else
            {
                txtRuleEssLevel.Text = string.Empty;
                nRuleEssLevel = -1;
            }

        }

        [ControlEvent("txtRuleEssSummLevel", "End")]
        private void txtRuleEssSummLevel_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
            int result = 0;
            if (int.TryParse(txtRuleEssSummLevel.Text, out result))
            {
                nRuleEssSummLevel = result;
            }
            else
            {
                txtRuleEssSummLevel.Text = string.Empty;
                nRuleEssSummLevel = -1;
            }

        }
        [ControlEvent("txtRuleEssDamageLevel", "End")]
        private void txtRuleEssDamageLevel_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
            int result = 0;
            if (int.TryParse(txtRuleEssDamageLevel.Text, out result))
            {
                nRuleEssDamageLevel = result;
            }
            else
            {
                txtRuleEssDamageLevel.Text = string.Empty;
                nRuleEssDamageLevel = -1;
            }

        }
        [ControlEvent("txtRuleEssCDLevel", "End")]
        private void txtRuleEssCDLevel_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
            int result = 0;
            if (int.TryParse(txtRuleEssCDLevel.Text, out result))
            {
                nRuleEssCDLevel = result;
            }
            else
            {
                txtRuleEssCDLevel.Text = string.Empty;
                nRuleEssCDLevel = -1;
            }

        }
        [ControlEvent("txtRuleEssCRLevel", "End")]
        private void txtRuleEssCRLevel_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
            int result = 0;
            if (int.TryParse(txtRuleEssCRLevel.Text, out result))
            {
                nRuleEssCRLevel = result;
            }
            else
            {
                txtRuleEssCRLevel.Text = string.Empty;
                nRuleEssCRLevel = -1;
            }

        }
        [ControlEvent("txtRuleEssDRLevel", "End")]
        private void txtRuleEssDRLevel_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
            int result = 0;
            if (int.TryParse(txtRuleEssDRLevel.Text, out result))
            {
                nRuleEssDRLevel = result;
            }
            else
            {
                txtRuleEssDRLevel.Text = string.Empty;
                nRuleEssDRLevel = -1;
            }

        }
        [ControlEvent("txtRuleEssCritLevel", "End")]
        private void txtRuleEssCritLevel_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
            int result = 0;
            if (int.TryParse(txtRuleEssCritLevel.Text, out result))
            {
                nRuleEssCritLevel = result;
            }
            else
            {
                txtRuleEssCritLevel.Text = string.Empty;
                nRuleEssCritLevel = -1;
            }

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

        private void chkRuleFilterlvl7_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            if (chkRuleFilterlvl7.Checked)
            { bRuleFilterlvl7 = true; }
            else
            { bRuleFilterlvl7 = false; }
            populateSpellListBox();

        }

        private void chkRuleFilterlvl6_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            if (chkRuleFilterlvl6.Checked)
            { bRuleFilterlvl6 = true; }
            else
            { bRuleFilterlvl6 = false; }
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

                nRuleWieldAttribute = (WeaponTypeList[cboWeaponAppliesTo.Selected].ID);
                GearFoundry.PluginCore.WriteToChat("Index of weaponappliesto: " + cboWeaponAppliesTo.Selected.ToString());
                GearFoundry.PluginCore.WriteToChat("Guid of weaponappliesto: " + WeaponTypeList[cboWeaponAppliesTo.Selected].ID.ToString());
                GearFoundry.PluginCore.WriteToChat("Name of weaponappliesto: " + WeaponTypeList[cboWeaponAppliesTo.Selected].name);


                switch (nRuleWieldAttribute)
                {
                    case 0: // eRuleWeaponTypes.notapplicable:
                        lblRuleMcModAttack.Text = "mc/mod%/attack";
                        lblRuleMinMax_ElvsMons.Text = "Damage";
                        break;
                    case 47: //eRuleWeaponTypes.missile:
                        lblRuleMcModAttack.Text = "Damage Mod:";
                        lblRuleMinMax_ElvsMons.Text = "ElemDam";
                        break;
                    case 34: //eRuleWeaponTypes.mage:
                    case 43:
                        //wand
                        lblRuleMcModAttack.Text = "Mana Conversion:";
                        lblRuleMinMax_ElvsMons.Text = "ElemvsMons";
                        break;
                    default:
                        //melee
                        lblRuleMcModAttack.Text = "Attack Mod:";
                        lblRuleMinMax_ElvsMons.Text = "Min-Max";
                        break;
                }

            }
            catch (Exception ex) { LogError(ex); }


        }


        [ControlEvent("cboMasteryType", "Change")]
        private void cboMasteryType_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            nRuleMasteryType = cboMasteryType.Selected;
            GearFoundry.PluginCore.WriteToChat("Index of mastery: " + cboMasteryType.Selected.ToString());
            GearFoundry.PluginCore.WriteToChat("Guid of mastery: " + MasteryIndex[cboMasteryType.Selected].ID.ToString());
            GearFoundry.PluginCore.WriteToChat("Name of weaponappliesto: " + MasteryIndex[cboMasteryType.Selected].name);

        }


        [ControlEvent("txtRuleMcModAttack", "End")]
        private void txtRuleMcModAttack_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
            int d = 0;
            nRuleMcModAttack = 0;
            if (int.TryParse(txtRuleMcModAttack.Text, out d))
            {
                nRuleMcModAttack = d;
            }
            else
            {
                nRuleMcModAttack = 0;

            }

            //	}
        }

        [ControlEvent("txtRuleMeleeD", "End")]
        private void txtRuleMeleeD_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            //if (mSelectedRule != null) {
            int d = 0;
            nRuleMeleeD = 0;
            if (int.TryParse(txtRuleMeleeD.Text, out d))
            {
                //mSelectedRule.minmeleebonus = d;
                nRuleMeleeD = d;
            }
            else
            {
                nRuleMeleeD = 0;
            }

            //}
        }

        [ControlEvent("txtRuleMagicD", "End")]
        private void txtRuleMagicD_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            //	if (mSelectedRule != null) {
            int d = 0;
            nRuleMagicD = 0;
            if (int.TryParse(txtRuleMagicD.Text, out d))
            {
                //mSelectedRule.minmagicdbonus = d;
                nRuleMagicD = d;
            }
            else
            {
                //mSelectedRule.minmagicdbonus = -1;
                nRuleMagicD = d;
            }
        }

        [ControlEvent("chkRuleWeaponsa", "Change")]
        private void chkRuleWeaponsa_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            sRuleWeaponsa = "false";
            if (chkRuleWeaponsa.Checked)
            { sRuleWeaponsa = "true"; }
        }


        [ControlEvent("chkRuleWeaponsb", "Change")]
        private void chkRuleWeaponsb_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            sRuleWeaponsb = "false";
            if (chkRuleWeaponsb.Checked)
            { sRuleWeaponsb = "true"; }
        }

        [ControlEvent("chkRuleWeaponsc", "Change")]
        private void chkRuleWeaponsc_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            sRuleWeaponsc = "false";
            if (chkRuleWeaponsc.Checked)
            { sRuleWeaponsc = "true"; }
        }

        [ControlEvent("chkRuleWeaponsd", "Change")]
        private void chkRuleWeaponsd_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            sRuleWeaponsd = "false";
            if (chkRuleWeaponsa.Checked)
            { sRuleWeaponsd = "true"; }
        }

        [ControlEvent("chkRuleMSCleavea", "Change")]
        private void chkRuleMSCleavea_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            sRuleMSCleavea = "false";

            if (chkRuleMSCleavea.Checked)
            { sRuleMSCleavea = "true"; }

        }

        [ControlEvent("chkRuleMSCleaveb", "Change")]
        private void chkRuleMSCleaveb_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            sRuleMSCleaveb = "false";
            if (chkRuleMSCleaveb.Checked)
            { sRuleMSCleaveb = "true"; }


        }

        [ControlEvent("chkRuleMSCleavec", "Change")]
        private void chkRuleMSCleavec_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            sRuleMSCleavec = "false";
            if (chkRuleMSCleavec.Checked)
            { sRuleMSCleavec = "true"; }

        }

        [ControlEvent("chkRuleMSCleaved", "Change")]
        private void chkRuleMSCleaved_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)  //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            sRuleMSCleaved = "false";
            if (chkRuleMSCleaved.Checked)
            { sRuleMSCleaved = "true"; }

        }

        [ControlEvent("cboRuleEssMastery", "Change")]
        private void cboRuleEssMastery_Change(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        {
            nRuleEssMastery = cboRuleEssMastery.Selected;
            //GearFoundry.PluginCore.WriteToChat("Index of essence mastery: " + cboruleEssMastery.Selected.ToString());
            //GearFoundry.PluginCore.WriteToChat("Guid of mastery: " + MasteryIndex[cboMasteryType.Selected].ID.ToString());
            //GearFoundry.PluginCore.WriteToChat("Name of weaponappliesto: " + MasteryIndex[cboMasteryType.Selected].name);

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
        [ControlEvent("txtRuleMinMaxa", "End")]

        private void txtRuleMinMaxa_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {

            sRuleMinMaxa = txtRuleMinMaxa.Text.ToString().Trim();

        }

        [ControlEvent("txtRuleMinMaxb", "End")]
        private void txtRuleMinMaxb_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            sRuleMinMaxb = txtRuleMinMaxb.Text.Trim();
        }

        [ControlEvent("txtRuleMinMaxc", "End")]
        private void txtRuleMinMaxc_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            sRuleMinMaxc = txtRuleMinMaxc.Text.Trim();

        }

        [ControlEvent("txtRuleMinMaxd", "End")]

        private void txtRuleMinMaxd_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            sRuleMinMaxd = txtRuleMinMaxd.Text.Trim();

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
            { var = var.Substring(0, mLength - 1); }


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


        [ControlEvent("txtRuleSpellMatches", "End")]
        private void txtRuleSpellMatches_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {

            if (!sRuleSpells.Contains(txtRuleSpellMatches.Text))
            {
                foreach (spellinfo spell in FilteredSpellIndex)
                {
                    try
                    {
                        if (spell.spellname.ToLower().Contains(txtRuleSpellMatches.Text.ToLower()))
                        {
                            string name = spell.spellname;
                            int id = spell.spellid;
                            nRuleMustHaveSpell = id;
                            //if (!sRuleSpells.Contains(nRuleMustHaveSpell.ToString()))
                            //{

                            WriteEnabledSpellsList(id, name);
                            populateRuleSpellEnabledListBox();
                            // }
                        }
                    }
                    catch (Exception ex) { LogError(ex); }
                }
            }
        }
    }
}