using System;
using System.IO;
using System.Linq;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System.Collections.Generic;
using VirindiViewService;
using MyClasses.MetaViewWrappers;
using System.Xml;
using System.Xml.Linq;
using System.Windows;


namespace GearFoundry
{
    public partial class PluginCore
    {
        //Tabpage click event
        [ControlEvent("nbSetupsetup", "Change")]
        private void nbSetupsetup_Change(object sender, MyClasses.MetaViewWrappers.MVIndexChangeEventArgs e)
        {
            try
            {

                if (e.Index == 3)
                {
                    populateSalvageListBox();
                }
                else if (e.Index == 2)
                {
                    populateMobsListBox();
                }
                else if (e.Index == 1)
                {
                    //fixed spelling from thropy
                    populateTrophysListBox();
                }

            }
            catch (Exception ex) { LogError(ex); }
        }



        [ControlEvent("chkTrophyExact", "Change")]
        private void chkTrophyExact_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)    // Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            mexact = chkTrophyExact.Checked;

        }

        [ControlEvent("chkmyMobExact", "Change")]
        private void chkmyMobExact_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e) //Decal.Adapter.CheckBoxChangeEventArgs e)
        {
            mexact = chkmyMobExact.Checked;

        }

        private void lstSelect(XDocument xdoc, string filename, List<XElement> lst, MyClasses.MetaViewWrappers.IList lstvue, MyClasses.MetaViewWrappers.IStaticText mlbl, MyClasses.MetaViewWrappers.MVListSelectEventArgs margs, int mlist)
        {
            try
            {
                // Variable initiations
                mgoon = true;
                string mcomb = "";
                MyClasses.MetaViewWrappers.IListRow row = null;
                string mID = "";
                //This is gotten from the sender function which has identified event args and sent as a parameter
                row = lstvue[margs.Row];
                //If this function follows a click on the check box then mchecked will be different here from in the salvagelist (lst)
                mchecked = Convert.ToBoolean(row[0][0]);
                sname = (Convert.ToString(row[1][0]));
                mlbl.Text = sname;

                if (lst != null && filename == salvageFilename)
                {
                    //  RedoSalvageFile();
                    mitem = 3;
                    mcomb = (Convert.ToString(row[2][0]));
                    txtSalvageString.Text = mcomb;
                    mgoon = true;
                    switch (margs.Column)
                    {
                        case 0:
                            mchecked = Convert.ToBoolean(row[0][0]);
                            doSalvageUpdate();
                            break;

                        case 1:
                            mlbl.Text = sname;
                            break;
                        case 2:
                            txtSalvageString.Text = mcomb;
                            break;
                    }

                    mchecked = Convert.ToBoolean(row[0][0]);
                } //end of salvage selected
             }

            catch (Exception ex) { LogError(ex); }
        }



        private void lstSelect(XDocument xdoc, string filename, List<XElement> lst, MyClasses.MetaViewWrappers.IList lstvue, MyClasses.MetaViewWrappers.ITextBox mtxt, MyClasses.MetaViewWrappers.MVListSelectEventArgs margs, int mlist)
        {
            try
            {
 
                // Variable initiations
                mgoon = true;
                string mcomb = "";
                MyClasses.MetaViewWrappers.IListRow row = null;
                string mID = "";
                //This is gotten from the sender function which has identified event args and sent as a parameter
                row = lstvue[margs.Row];
                 //If this function follows a click on the check box then mchecked will be different here from in the salvagelist (lst)
                mchecked = Convert.ToBoolean(row[0][0]);
                sname = (Convert.ToString(row[1][0]));
                snameorig = sname;
                mtxt.Text = sname;

               if (xdoc != null && ((filename == mobsFilename) || (filename == trophiesFilename)))
                {
                    IEnumerable<XElement> elements = xdoc.Element("GameItems").Descendants("item");
                    var data = from item in lst
                               where item.Element("key").Value.ToString().Contains(sname)
                               select item;
                    mexact = Convert.ToBoolean(data.First().Element("isexact").Value);

                    if (filename == mobsFilename)
                    {
                        chkmyMobExact.Checked = mexact;
                        mitem = 1;
                    }
                    else if (filename == trophiesFilename)
                    {
                        chkTrophyExact.Checked = mexact;
                        mitem = 2;
                    }


                    switch (margs.Column)
                    {
                        case 0:
                        case 1:
                           mtxt.Text = sname;
                            break;
                        case 2:
                            mgoon = false;
                            break;
                    }
                    mchecked = Convert.ToBoolean(row[0][0]);
                }  // end of trophies or mobs selected
                    //Need to remove object being worked on before adding it back so won't have a duplication of itme.
               if (xdoc != null)
               {
                  IEnumerable<XElement> elements = xdoc.Element("GameItems").Descendants("item");
                   xdoc.Descendants("item").Where(x => x.Element("key").Value.ToString().Trim().Equals(sname.Trim())).Remove();
               }
                    //If want to keep the item above need to add it back with the new data
                    if (mgoon)
                    { addMyItem(xdoc, filename, mID, mexact, mitem); }

                    else
                    // Need to save the file without the item in it.
                    {
                        xdoc.Save(filename);
                        if (xdoc == xdocTrophies)
                        { populateTrophysListBox(); }
                        else
                        { populateMobsListBox(); }
                    }
                }
            

            catch (Exception ex) { LogError(ex); }
        }

        //overload for spells listbox which does not  have an associated textfile nor an xdocument and uses a spellinfo type list
        private void lstSelect(string filename, List<spellinfo> lst, MyClasses.MetaViewWrappers.IList lstvue, MyClasses.MetaViewWrappers.MVListSelectEventArgs margs, int mlist)
        {
            try
            {

                MyClasses.MetaViewWrappers.IListRow row = null;
                row = lstvue[margs.Row];
                mchecked = Convert.ToBoolean(row[0][0]);
                sname = (Convert.ToString(row[1][0]));
                int nID = (Convert.ToInt32(row[2][0]));


                switch (margs.Column)
                {
                    case 0:
                        break;
                    case 2:
                        mgoon = false;
                        break;
                    default:
                        break;
                }
                mchecked = Convert.ToBoolean(row[0][0]);
                if (mchecked)
                {
                    WriteEnabledSpellsList(nID, sname);
                    populateRuleSpellEnabledListBox();
                }
            }
            catch (Exception ex) { LogError(ex); }
        }



        //Provides a modified overload for lstSelect to display lists where  only purpose is to select an item for the rules part of the 
        //program.  
        private string lstSelect(List<IDNameLoadable> lst, MyClasses.MetaViewWrappers.IList lstvue, MyClasses.MetaViewWrappers.MVListSelectEventArgs margs, int mlist)
        {
            try
            {

                MyClasses.MetaViewWrappers.IListRow row = null;
                row = lstvue[margs.Row];
                mchecked = Convert.ToBoolean(row[0][0]);
                sname = (Convert.ToString(row[1][0]));
                string snum = Convert.ToString(row[2][0]);
                if (mchecked)
                { return snum; }
                else
                { return ""; }
            }
            catch (Exception ex) {LogError(ex); return null; }
        }

        //Provides a modified overload for lstSelect to display lists where  only purpose is to select an item for the rules part of the 
        //program.  This overload uses spellinfo as opposed to IDNameLoadable list
        private string lstSelect(List<spellinfo> lst, MyClasses.MetaViewWrappers.IList lstvue, MyClasses.MetaViewWrappers.MVListSelectEventArgs margs, int mlist)
        {
            try
            {

                MyClasses.MetaViewWrappers.IListRow row = null;
                row = lstvue[margs.Row];
                mchecked = Convert.ToBoolean(row[0][0]);
                sname = (Convert.ToString(row[1][0]));
                string snum = Convert.ToString(row[2][0]);
                if (mchecked)
                { return snum; }
                else
                { return ""; }
            }
            catch (Exception ex) { LogError(ex); return null; }
        }


        // [ControlEvent]("lstRules", "Selected")
        //TODO:  Revise here with pointers to list objects.
        private void lstRules_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e)  // Decal.Adapter.ListSelectEventArgs e)
        {
            //int mList = 6;
            try
            {
                //First clear previous rule
                clearRule();
                nRuleRow = e.Row;

                //Now add the information from the selected rule to  the various controls associated with the rules
                bool mdelete = false;
                MyClasses.MetaViewWrappers.IListRow row = null;
                row = lstRules[e.Row];
                bRuleEnabled = Convert.ToBoolean(row[0][0]);
                string rowname = (Convert.ToString(row[2][0]));
                string rowdescr = (Convert.ToString(row[3][0]));
                string rowprior = (Convert.ToString(row[1][0]));

                txtRuleName.Text = rowname;
                txtRulePriority.Text = rowprior;
                if (bRuleEnabled)
                    chkRuleEnabled.Checked = true;
                else
                    chkRuleEnabled.Checked = false;

                //Need to find the information associated with the selected rule
                try
                {
                    IEnumerable<XElement> elements = xdocRules.Element("Rules").Descendants("Rule");
                    foreach (XElement element in elements)
                    {
                        if (element.Element("Name").Value.ToString() == rowname)
                        {
                            el = element;
                            nRuleNum = Convert.ToInt32(element.Element("RuleNum").Value);
                            break;
                        }
                    }

                    loadControls(el);
                    lstRules.ScrollPosition = nRuleRow;
                }
                catch (Exception ex) { LogError(ex); }


                //Need to do an action depending on the column selected
                switch (e.Column)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        txtRuleName.Text = rowname;
                        break;
                    case 3:
                        break;
                    case 4:
                        mdelete = true;
                        break;
                }

                // If the checkmark is changed need to  change the  bRuleEnabled
                bRuleEnabled = Convert.ToBoolean(row[0][0]);

                if (xdocRules != null)
                {
                    try
                    {
                        // IEnumerable<XElement> myelements = xdocRules.Element("Rules").Descendants("Rule");
                        if (mdelete)
                        {
                            //Need to delete the  rule if case 4
                            xdocRules.Descendants("Rule").Where(x => x.Element("RuleNum").Value.ToString().Equals(nRuleNum.ToString())).Remove();
                            xdocRules.Save(rulesFilename);
                            populateRulesListBox();
                            clearRule();
                            lstRules.ScrollPosition = nRuleRow;
                        }
                    }
                    catch (Exception ex) { LogError(ex); }

                }
            }

            catch (Exception ex) { LogError(ex); }
        }

        //clears all associated controls of previous data when adding or selecting a new rule
        private void clearRule()
        {
            try
            {
                initRulesVariables();
                populateListBoxes();
                lstRuleSpellsEnabled.Clear();
                EnabledSpellsList.Clear();
                initRulesCtrls();
                
            }
                catch (Exception ex) { LogError(ex); }
           
        }

        //Loads the controls with the values of the current rules and clears values of previous rule in the rules listview
        private void loadControls(XElement el)
        {
            try
            {
                //Need to clear list boxes of previous data
                populateListBoxes();

                //Need to populate textbox and checkboxs controls with data for this rule
                //First need to get the variables for this rule
                getVariables(el); //works

                //Break down strings that are composites
                if (sRuleReqSkill.Length > 0)
                {
                    // strBreakUp(sRuleReqSkill, sRuleReqSkilla, sRuleReqSkillb, sRuleReqSkillc, sRuleReqSkilld);
                    string[] SplitReqSkill = sRuleReqSkill.Split(',');
                    
                    sRuleReqSkilla = SplitReqSkill[0];
                    sRuleReqSkillb = SplitReqSkill[1];
                    sRuleReqSkillc = SplitReqSkill[2];
                    sRuleReqSkilld = SplitReqSkill[3];
                    
                    string[] SplitRuleWeapons = sRuleWeapons.Split(',');

                    sRuleWeaponsa = SplitRuleWeapons[0];
                    sRuleWeaponsb = SplitRuleWeapons[1];
                    sRuleWeaponsc = SplitRuleWeapons[2];
                    sRuleWeaponsd = SplitRuleWeapons[3];
                    
                }
                //Now add variables to text and checkbox controls to clear them

                initRulesCtrls();

                string flag = "";

                //Reset appliestobox
                flag = sRuleAppliesTo;
                if (flag.Length != 0)
                {
                    int n = AppliesToList.Count;
                    MyClasses.MetaViewWrappers.IList lst = lstRuleApplies;
                    setUpForChecks(n, flag, lst);
                    lst = null;
                    n = 0;
                    flag = "";

                }

                //Reset weapondamagebox
                if (sRuleDamageTypes != "")
                {
                    flag = sRuleDamageTypes;
                    int n = ElementalList.Count;
                    MyClasses.MetaViewWrappers.IList lst = lstDamageTypes;
                    setUpForChecks(n, flag, lst);

                }
                
                if(sRulePalettes != String.Empty)
                {
                
                }

                //Reset ArmorTypebox
                if (sRuleArmorType != "")
                {
                    flag = sRuleArmorType;
                    int n = ArmorIndex.Count;
                    MyClasses.MetaViewWrappers.IList lst = lstRuleArmorTypes;
                    setUpForChecks(n, flag, lst);
                }
                
                //Reset Slot List Box
                flag = sRuleSlots;
                if(flag.Length !=0)
                {
                	int n = SlotList.Count;
                	MyClasses.MetaViewWrappers.IList lst = lstRuleSlots;
                	setUpForChecks(n, flag, lst);
                }


                //Reset ArmorSetsBox
                populateSetsListBox();
                flag = sRuleArmorSet;
                if (flag.Length != 0)
                {
                    int n = ArmorSetsList.Count;
                    MyClasses.MetaViewWrappers.IList lst = lstRuleSets;
                    setUpForChecks(n, flag, lst);
                }


               flag = sRuleSpells;
                if (flag.Length != 0)
                {
                    try
                    {
                        if (!flag.Contains(","))
                        {
                            getSpellName(flag);
                            WriteEnabledSpellsList(nid, sname);


                        }
                        else
                        {
                            List<string> flagList = new List<string>();
                            var count = flag.Count(x => x == ',');
                            int z = (int)count;

                            for (int m = 0; m < z; m++)
                            {
                                int k = flag.IndexOf(',');
                                string item = flag.Substring(0, k);
                                flag = flag.Substring(k + 1);

                                getSpellName(item);
                                WriteEnabledSpellsList(nid, sname);
                                populateRuleSpellEnabledListBox();


                            }
                            getSpellName(flag);
                            WriteEnabledSpellsList(nid, sname);



                        }
                        populateRuleSpellEnabledListBox();


                    }
                    catch (Exception ex) { LogError(ex); }

                }
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void getSpellName(string sid)
        {
            nid = Convert.ToInt32(sid);
            foreach (spellinfo spell in SpellIndex)
            {
                if (spell.spellid == nid)
                {
                    sname = spell.spellname;
                }

            }
        }


        private void setUpForChecks(int n, string flag, MyClasses.MetaViewWrappers.IList lst)
        {
            try
            {
                if (!flag.Contains(","))
                {
                    setChecked(n, flag, lst);
                }
                else
                {
                    List<string> flagList = new List<string>();
                    var count = flag.Count(x => x == ',');
                    int z = (int)count;

                    for (int m = 0; m < z; m++)
                    {
                        int k = flag.IndexOf(',');
                        string item = flag.Substring(0, k);
                        flag = flag.Substring(k + 1);

                        setChecked(n, item, lst);
                    }
                    setChecked(n, flag, lst);

                }
            }
            catch (Exception ex) { LogError(ex); }

        }

        private void setChecked(int n, string flag, MyClasses.MetaViewWrappers.IList lst)
        {
            try
            {
                bool mchecked = true;
                IListRow row;
                for (int i = 0; i < n; i++)
                {
                    row = lst[i];
                    if (row[2][0].ToString() == flag)
                    {
                        row[0][0] = mchecked;

                    }
                }
            }
            catch (Exception ex) { LogError(ex); }


        }

        // [ControlEvent]("lstmyTrophies", "Selected")
        private void lstmyTrophies_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e)  // Decal.Adapter.ListSelectEventArgs e)
        {
            nTrophyRow = e.Row;
            
            int mList = 1;
            MVListSelectEventArgs args = e;
            lstSelect(xdocTrophies, trophiesFilename, mSortedTrophiesList, lstmyTrophies, txtTrophyName, args, mList);
        }

        // [ControlEvent]("lstmyMobs", "Selected") 
        private void lstmyMobs_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e)  // Decal.Adapter.ListSelectEventArgs e)
        {
            nMobRow = e.Row;
            int mList = 1;
            MVListSelectEventArgs args = e;
            lstSelect(xdocMobs, mobsFilename, mSortedMobsList, lstmyMobs, txtmyMobName, args, mList);

        }

        // [ControlEvent]("lstRuleSpellsEnabled", "Selected") 
        private void lstRuleSpellsEnabled_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e)  
        {
            try
            {
                MyClasses.MetaViewWrappers.IListRow row = null;
                row = lstRuleSpellsEnabled[e.Row];
                //sname = (Convert.ToString(row[0][0]));
                int nID = (Convert.ToInt32(row[1][0]));
                
                if (e.Column == 2)
                {
                    try
                    {

                        List<IDNameLoadable>EnabledSpellsListTemp = EnabledSpellsList.Where(x => x.ID != nID).ToList();
                        
                        EnabledSpellsList.Clear();
                        EnabledSpellsList.AddRange(EnabledSpellsListTemp);
                        EnabledSpellsListTemp.Clear();

                    }
                    catch (Exception ex) { LogError(ex); }
                    if (EnabledSpellsList.Count == 0)
                    { lstRuleSpellsEnabled.Clear(); }
                    else 
                    {
                        sRuleSpells = "";
                        nspells = 0;

                        //Now resetup the variables nspells and sRuleSpells
                        foreach (IDNameLoadable spl in EnabledSpellsList)
                        {
                            string sid = spl.ID.ToString();
                            sRuleSpells = sRuleSpells + sid + ",";
                            nspells++;
                        }
                        //Now remove the final comma from the variable
                        sRuleSpells = sRuleSpells.Substring(0, sRuleSpells.Length - 1);

                        populateRuleSpellEnabledListBox(); 
                    }
                }
            }
            catch (Exception ex) { LogError(ex); }
        }

        // [ControlEvent]("lstRuleSpells", "Selected") 
        private void lstRuleSpells_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e) 
        {
            try
            {
                MyClasses.MetaViewWrappers.IListRow row = null;
                row = lstRuleSpells[e.Row];
                mchecked = Convert.ToBoolean(row[0][0]);
                sname = (Convert.ToString(row[1][0]));
                int nID = (Convert.ToInt32(row[2][0]));


                switch (e.Column)
                {
                    case 0:
                        break;
                    case 2:
                        mgoon = false;
                        break;
                    default:
                        break;
                }
                mchecked = Convert.ToBoolean(row[0][0]);
                if (mchecked)
                {
                    WriteEnabledSpellsList(nID, sname);
                    populateRuleSpellEnabledListBox();
                }
            }
            catch (Exception ex) { LogError(ex); }

        }


        // [ControlEvent]("lstNotifySalvage", "Selected")
        private void lstNotifySalvage_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e)  // Decal.Adapter.ListSelectEventArgs e)
        {
            int mList = 2;
            MVListSelectEventArgs args = e;
            lstSelect(xdocSalvage, salvageFilename, mSortedSalvageList, lstNotifySalvage, lblSalvageName, args, mList);

        }

        // [ControlEvent]("lstDamageTypes", "Selected")
        private void lstDamageTypes_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e) 
        {

            int mList = 3;
            MVListSelectEventArgs args = e;
            string mDamageTypeNum = lstSelect(ElementalList, lstDamageTypes, args, mList);
        }

        [ControlEvent("txtTrophyMax", "End")]
        private void txtTrophyMax_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            string snum = txtTrophyMax.Text;
            int result = 0;
            if (Int32.TryParse(txtTrophyMax.Text, out result))
            { mMaxLoot = result; }
            else
            { mMaxLoot = 0; }

        }



        [ControlEvent("txtTrophyName", "End")]
        private void txtTrophyName_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            sname = txtTrophyName.Text.Trim();

        }


        [ControlEvent("txtmyMobName", "End")]
        private void txtmyMobName_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)  //Decal.Adapter.TextBoxEndEventArgs e)
        {
            sname = txtmyMobName.Text.Trim();
        }

        //[ControlEvent("lblSalvageName", "End")]
        //private void lblSalvageName_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        //{
        //    sname = lblSalvageName.Text.Trim();
        //}

        [ControlEvent("txtSalvageString", "End")]
        private void txtSalvageString_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {
            sinput = txtSalvageString.Text.Trim();
            sname = lblSalvageName.Text.Trim();
        }


        //This is a basic function that adds an item to an xdocument.  It is called by various other functions.
        //It receives the name of the XDocument, the filename that identifies where it is stored, the name of the item,
        //whether it is enabled, and the string for the combination such as in salvage or empty for mobs and trophies,
        //the GUID as a string -- which may or may not be kept, and a bool for if it is a partial name, and the number of subitems in the xml file
        //the function simply adds the item to the file and then saves the file.
        private void addMyItem(XDocument xdoc, string filename, string mid, bool mexact, int mitem)
        {

            try
            {
                switch (mitem)
                {
                    case 1:
                        xdoc.Element("GameItems").Add(new XElement("item",
                           new XElement("key", sname),
                           new XElement("checked", mchecked),
                           new XElement("isexact", mexact),
                           new XElement("Guid", mid)));
                        break;
                    case 2:
                        xdoc.Element("GameItems").Add(new XElement("item",
                            new XElement("key", sname),
                            new XElement("checked", mchecked),
                            new XElement("isexact", mexact),
                            new XElement("Guid", mMaxLoot)));
                        break;
                    case 3:
                        xdoc.Element("GameItems").Add(new XElement("item",
                          new XElement("key", sname),
                          new XElement("intvalue", mintvalue),
                          new XElement("checked", mchecked),
                          new XElement("combine", sinput),
                          new XElement("Guid", mid)));
                        break;
                }

                xdoc.Save(filename);

                if (xdoc == xdocTrophies)
                { populateTrophysListBox(); }
                else if (xdoc == xdocMobs)
                { populateMobsListBox(); }
                else if (xdoc == xdocSalvage)
                { populateSalvageListBox(); }



            }
            catch (Exception ex) { LogError(ex); }
        }

        //   [ControlEvent("btnAddTrophyItem", "Click")]
        private void btnAddTrophyItem_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)  //Decal.Adapter.ControlEventArgs e)
        {
            try
            {

                if (sname != null && sname.Trim().Length > 0)
                {

                    //                   string mcomb = "";
                    mchecked = true;
                    int mList = 1;
                    bool mexact;
                    if (chkTrophyExact.Checked)
                    { mexact = true; }
                    else
                    { mexact = false; }
                    string mID = "";
                    addMyItem(xdocTrophies, trophiesFilename, mID, mexact, mList);

                }
                else { GearFoundry.PluginCore.WriteToChat("Please give the name of a trophy or NPC to add"); }

            }
            catch (Exception ex) { LogError(ex); }
        }

        private void btnUpdateTrophyItem_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)  //Decal.Adapter.ControlEventArgs e)
        {
            try
            {
                if (sname != null && sname.Trim().Length > 0)
                {
                    nTrophyRow = lstmyTrophies.ScrollPosition;

                    try
                    {
                        if (xdocTrophies != null)
                        {
                            IEnumerable<XElement> elements = xdocTrophies.Element("GameItems").Descendants("item");
                            xdocTrophies.Descendants("item").Where(x => x.Element("key").Value.ToString().Trim().Equals(snameorig.Trim())).Remove();
                           populateTrophysListBox();
                        }

                    }

                    catch (Exception ex) { LogError(ex); }

                    mchecked = true;
                    int mList = 1;
                    bool mexact;
                    if (chkTrophyExact.Checked)
                    { mexact = true; }
                    else
                    { mexact = false; }
                    string mID = "";
                    addMyItem(xdocTrophies, trophiesFilename, mID, mexact, mList);
                    lstmyTrophies.ScrollPosition = nTrophyRow;


                }
                else { GearFoundry.PluginCore.WriteToChat("Please give the name of a trophy or NPC to add"); }

            }
            catch (Exception ex) { LogError(ex); }
        }




        //   [ControlEvent("btnAddMobItem", "Click")]
        private void btnUpdateMobItem_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)  //Decal.Adapter.ControlEventArgs e)
        {
            nMobRow = lstmyMobs.ScrollPosition;

            try
            {
                if (xdocMobs != null)
                {
                    IEnumerable<XElement> elements = xdocMobs.Element("GameItems").Descendants("item");
                    xdocMobs.Descendants("item").Where(x => x.Element("key").Value.ToString().Trim().Equals(snameorig.Trim())).Remove();
                    populateMobsListBox();
                }

            }
            catch (Exception ex) { LogError(ex); }
  

            try
            {

                if (sname != null && sname.Trim().Length > 0)
                {

                    //                string mCombine = "";
                    mchecked = true;
                    int mList = 1;
                    bool mexact;
                    if (chkmyMobExact.Checked)
                    { mexact = true; }
                    else
                    { mexact = false; }
                    string mID = "";
                    addMyItem(xdocMobs, mobsFilename, mID, mexact, mList);

                }
                else { GearFoundry.PluginCore.WriteToChat("Please give the name of a mob to add"); }

            }
            catch (Exception ex) { LogError(ex); }
        }

        private void btnAddMobItem_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)  //Decal.Adapter.ControlEventArgs e)
        {
            try
            {

                if (sname != null && sname.Trim().Length > 0)
                {

                    //                string mCombine = "";
                    mchecked = true;
                    int mList = 1;
                    bool mexact;
                    if (chkmyMobExact.Checked)
                    { mexact = true; }
                    else
                    { mexact = false; }
                    string mID = "";
                    addMyItem(xdocMobs, mobsFilename, mID, mexact, mList);

                }
                else { GearFoundry.PluginCore.WriteToChat("Please give the name of a mob to add"); }

            }
            catch (Exception ex) { LogError(ex); }
        }




        //Basic function to populate the listviews.  It receives the listview name, the 
        //list from which it is populated, and the number of columns that are displayed.
        //For each item in the list it pulls out the variables, then sets up the lists,

        private void populateLst(MyClasses.MetaViewWrappers.IList lstView, List<XElement> lst, int mlist)
        {
            try
            {

                lstView.Clear();
                string mdescr = "";
                string vcombine = "";
                string spriority = "";
                string vname = "";
                string mname = "";
                string vID = "";


                foreach (XElement element in lst)
                {
                    if (mlist == 4)
                    {
                        mchecked = Convert.ToBoolean(element.Element("Enabled").Value);
                        mname = element.Element("Name").Value.ToString().Trim();
                        spriority = element.Element("Priority").Value.ToString();

                    }
                    else if (mlist == 5)
                    {
                        //   mchecked = Convert.ToBoolean(element.Element("checked").Value);
                        vname = element.Element("Name").Value.ToString();
                        vID = element.Element("ID").Value.ToString();

                    }
                    else
                    {
                        mchecked = Convert.ToBoolean(element.Element("checked").Value);
                        vname = element.Element("key").Value.ToString();
                        if (lstView == lstNotifySalvage) { vcombine = element.Element("combine").Value.ToString(); }
                    }

                    MyClasses.MetaViewWrappers.IListRow newRow = lstView.AddRow();
                    switch (mlist)
                    {
                        case 1:
                            newRow[0][0] = mchecked;
                            newRow[1][0] = vname;
                            newRow[2][1] = 0x6005e6a;
                            break;
                        case 2:
                            newRow[0][0] = mchecked;
                            newRow[1][0] = vname;
                            newRow[2][0] = vcombine;
                            break;
                        case 4:
                            newRow[0][0] = mchecked;
                            newRow[1][0] = spriority;
                            newRow[2][0] = mname;
                            newRow[3][0] = mdescr;
                            newRow[4][1] = 0x6005e6a;
                            break;
                        case 5:
                            newRow[0][0] = vname;
                            newRow[1][1] = 0x6005e6a;
                            break;


                    }
                }
            }
            catch (Exception ex) { LogError(ex); }


        }


        // Use this for lists simply needing a selection to be made
        private void populateLst(MyClasses.MetaViewWrappers.IList lstView, List<IDNameLoadable> lst, int mlist)
        {
            lstView.Clear();

            foreach (IDNameLoadable element in lst)
            {
                try
                {

                    string vname = element.name;
                    string snum = element.ID.ToString();
                    bool mchecked = false;
                    MyClasses.MetaViewWrappers.IListRow newRow = lstView.AddRow();

                    switch (mlist)
                    {
                        case 3:
                            newRow[0][0] = mchecked;
                            newRow[1][0] = vname;
                            newRow[2][0] = snum;

                            break;
                        case 5:
                            newRow[0][0] = vname;
                            newRow[1][0] = snum;
                            newRow[2][1] = 0x6005e6a;
                            break;

                    }
                }
                catch (Exception ex) { LogError(ex); }
            }

        }

        // This is initial function to populate the lstRules (MainView listbox)
        // This function will also create a sorted mRulesListPrioritized 
        // and an mRulesListPrioritizedEnabled
        // Function uses the Xdocument xdocRules created on game initiation
        //mPrioritizedRulesList  becomes the list to work with throughout Alinco

        private void populateRulesListBox()
        {
            try
            {
                int mList = 4;

                setUpRulesLists(xdocRules, mPrioritizedRulesList, mPrioritizedRulesListEnabled);
                populateLst(lstRules, mPrioritizedRulesList, mList);
                FillItemRules();

            }

            catch (Exception ex) { LogError(ex); }

        }

        private void populateListRuleAppliesBox()
        {
            try
            {
                int mList = 3;
                populateLst(lstRuleApplies, AppliesToList, mList);

            }

            catch (Exception ex) { LogError(ex); }

        }



        // This is initial function to populate the lstmyTrophies (MainView listbox)
        // This function will also create a sorted mTrophiesList
        // Function uses the Xdocument xdocTrophies created on game initiation
        //mSortedTrophiesList becomes the list to work with throughout Alinco

        private void populateTrophysListBox()
        {
            try
            {
                int mList = 1;

                setUpLists(xdocTrophies, mSortedTrophiesList, mSortedTrophiesListChecked);
                populateLst(lstmyTrophies, mSortedTrophiesList, mList);

            }

            catch (Exception ex) { LogError(ex); }

        }

        // This is initial function to populate the lstmyMobs (MainView listbox)
        // This function will also create a sorted mobslist
        // Function uses the Xdocument xdocMobs created on game initiation
        //mSortedMobList becomes the list to work with throughout Alinco
        private void populateMobsListBox()
        {
            try
            {
                int mList = 1;
                setUpLists(xdocMobs, mSortedMobsList, mSortedMobsListChecked);
                populateLst(lstmyMobs, mSortedMobsList, mList);

            }

            catch (Exception ex) { LogError(ex); }

        }

        private void populateRuleSpellEnabledListBox()
        {
            try
            {

                if (EnabledSpellsList != null && EnabledSpellsList.Count() > 0)
                {
                    int mList = 5;
                    populateLst(lstRuleSpellsEnabled, EnabledSpellsList, mList);

                }

            }

            catch (Exception ex) { LogError(ex); }

        }


        private void populateSalvageListBox()
        {
            try
            {
                int mList = 2;
                setUpLists(xdocSalvage, mSortedSalvageList, mSortedSalvageListChecked);
                populateLst(lstNotifySalvage, mSortedSalvageList, mList);

            }

            catch (Exception ex) { LogError(ex); }

        }


        private void populateWeaponDamageListBox()
        {
            try
            {
                int mList = 3;
                populateLst(lstDamageTypes, ElementalList, mList);

            }

            catch (Exception ex) { LogError(ex); }

        }
        
        private void populateSlotListBox()
        {
            try
            {
                int mList = 3;
                populateLst(lstRuleSlots, SlotList, mList);

            }

            catch (Exception ex) { LogError(ex); }

        }

        private void populateArmorTypesListBox()
        {
            try
            {
                int mList = 3;
                populateLst(lstRuleArmorTypes, ArmorIndex, mList);

            }

            catch (Exception ex) { LogError(ex); }

        }

        private void populateSetsListBox()
        {
            try
            {

                int mList = 3;
                populateLst(lstRuleSets, ArmorSetsList, mList);

            }

            catch (Exception ex) { LogError(ex); }

        }







        private void populateSpellListBox()
        {
            FilteredSpellIndex.Clear();
            CreateFilteredSpellIndex();
            lstRuleSpells.Clear();

            foreach (spellinfo element in FilteredSpellIndex)
            {
                try
                {
                    string vname = element.spellname;
                    bool mchecked = false;
                    string snum = element.spellid.ToString();
                    MyClasses.MetaViewWrappers.IListRow newRow = lstRuleSpells.AddRow();
                    newRow[0][0] = mchecked;
                    newRow[1][0] = vname;
                    newRow[2][0] = snum;

                }
                catch (Exception ex) { LogError(ex); }
            }
        }



        //[ControlEvent("btnNewSalvage", "Click")]
        //private void btnNewSalvage_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)  //Decal.Adapter.ControlEventArgs e)
        //{

        //    if (lblSalvageName == null || txtSalvageString == null)
        //    {
        //        GearFoundry.PluginCore.WriteToChat("Please type in name of new salvage to add to list");
        //    }

        //    else
        //    {
        //        doSalvageNew();
        //    }
        //}

        //private void doSalvageNew()
        //{
        //    try
        //    {
        //        sname = lblSalvageName.Text.ToString().Trim();
        //        sinput = txtSalvageString.Text.ToString().Trim();

        //        if (sname != null && sname.Trim().Length > 0)
        //        {

        //            //string mCombine = "";
        //            string mID = "";
        //            bool mexact = false;
        //            int mitem = 3;

        //            addMyItem(xdocSalvage, salvageFilename, mID, mexact, mitem);
        //            FillSalvageRules();

        //        }
        //        else { GearFoundry.PluginCore.WriteToChat("Please give the name of a mob to add"); }

        //    }
        //    catch (Exception ex) { LogError(ex); }
        //}


        [ControlEvent("btnUpdateSalvage", "Click")]
        private void btnUpdateSalvage_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)  //Decal.Adapter.ControlEventArgs e)
        {

            if (lblSalvageName == null || txtSalvageString == null)
            {
                GearFoundry.PluginCore.WriteToChat("Please select salvage from list to update");
            }

            else
            {
                doSalvageUpdate();
            }
        }

        private void doSalvageUpdate()
        {
            try
            {
                sname = lblSalvageName.Text.ToString().Trim();
                sinput = txtSalvageString.Text.ToString().Trim();

                //  IEnumerable<XElement> elements = xdocSalvage.Element("GameItems").Descendants("item");
                var el = from item in mSortedSalvageList
                         where item.Element("key").Value.ToString().Contains(sname)
                         select item;
                mintvalue = Convert.ToInt32(el.First().Element("intvalue").Value);

                if (xdocSalvage != null)
                {

                    IEnumerable<XElement> elements = xdocSalvage.Element("GameItems").Descendants("item");
                    xdocSalvage.Descendants("item").Where(x => x.Element("key").Value.ToString().Trim().Contains(sname.Trim())).Remove();
                }
                string mID = "";
                bool mexact = false;
                int mitem = 3;

                addMyItem(xdocSalvage, salvageFilename, mID, mexact, mitem);
                FillSalvageRules();
            }
            catch (Exception ex) { LogError(ex); }
 
        }

        private void txtMaxMana_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {

        }
        private void txtMaxValue_End(object sender, MyClasses.MetaViewWrappers.MVTextBoxEndEventArgs e)
        {

        }
 
        //    #endregion





    }
}
