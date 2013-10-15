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

                if (e.Index == 2)
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

        private void lstRules_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e)  // Decal.Adapter.ListSelectEventArgs e)
        {
            try
            {
            	
                MyClasses.MetaViewWrappers.IListRow row = lstRules[e.Row];
                string RuleNumber = (row[4][0]).ToString();
                
                if(e.Column == 3)
                {
                	mPrioritizedRulesList.RemoveAll(x => x.Element("RuleNum").Value == RuleNumber);
                	mSelectedRule = mPrioritizedRulesList.First();
                }
                else
                {
                	mSelectedRule = mPrioritizedRulesList.Find(x => x.Element("RuleNum").Value == RuleNumber);
                }
                
                if(e.Column == 0)
                {
                	WriteToChat(row[0][0].ToString());
                	mSelectedRule.Element("Enabled").Value = row[0][0].ToString().ToLower();
                }
                
                _UpdateRulesTabs();
                
            }catch (Exception ex) { LogError(ex); }
        }
        
        private void lstRuleApplies_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e)  // Decal.Adapter.ListSelectEventArgs e)
        {
            try
            {
            	WriteToChat("Rule applies selected.");
                MyClasses.MetaViewWrappers.IListRow row = lstRuleApplies[e.Row];
                
                List<int> AppliesList = _ConvertCommaStringToIntList(mSelectedRule.Element("AppliesToFlag").Value);
                
                bool selected = (bool)row[0][0];
                int AppliesTo = Convert.ToInt32(row[2][0]);
                
                if(AppliesList.Contains(AppliesTo) && !selected)
                {
                	AppliesList.RemoveAll(x => x  == AppliesTo);
                }
                
                if(!AppliesList.Contains(AppliesTo) && selected)
                {
                	AppliesList.Add(AppliesTo);
                }
                
                mSelectedRule.Element("AppliesToFlag").Value = _ConvertIntListToCommaString(AppliesList);
                          
                _UpdateRulesTabs();
                
            }catch (Exception ex) { LogError(ex); }
        }
        
        private void lstRuleSlots_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e)  // Decal.Adapter.ListSelectEventArgs e)
        {
            try
            {
            	WriteToChat("Slot List selected.");
                MyClasses.MetaViewWrappers.IListRow row = lstRuleSlots[e.Row];
                
                List<int> sList = _ConvertCommaStringToIntList(mSelectedRule.Element("Slots").Value);
                
                bool selected = (bool)row[0][0];
                int Slot = Convert.ToInt32(row[2][0]);
                
                if(sList.Contains(Slot) && !selected)
                {
                	sList.RemoveAll(x => x  == Slot);
                }
                
                if(!sList.Contains(Slot) && selected)
                {
                	sList.Add(Slot);
                }
                
                mSelectedRule.Element("Slots").Value = _ConvertIntListToCommaString(sList);
                          
                _UpdateRulesTabs();
                
            }catch (Exception ex) { LogError(ex); }
        }
        
        private void lstRuleSets_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e)  // Decal.Adapter.ListSelectEventArgs e)
        {
            try
            {
            	WriteToChat("Set List selected.");
                MyClasses.MetaViewWrappers.IListRow row = lstRuleSets[e.Row];
                
                List<int> sList = _ConvertCommaStringToIntList(mSelectedRule.Element("ArmorSet").Value);
                
                bool selected = (bool)row[0][0];
                int armorset = Convert.ToInt32(row[2][0]);
                
                if(sList.Contains(armorset) && !selected)
                {
                	sList.RemoveAll(x => x  == armorset);
                }
                
                if(!sList.Contains(armorset) && selected)
                {
                	sList.Add(armorset);
                }
                
                mSelectedRule.Element("ArmorSet").Value = _ConvertIntListToCommaString(sList);
                          
                _UpdateRulesTabs();
                
            }catch (Exception ex) { LogError(ex); }
        }

        private void lstRuleArmorTypes_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e)  // Decal.Adapter.ListSelectEventArgs e)
        {
            try
            {

                MyClasses.MetaViewWrappers.IListRow row = lstRuleArmorTypes[e.Row];
                
                List<int> aList = _ConvertCommaStringToIntList(mSelectedRule.Element("ArmorType").Value);
                
                bool selected = (bool)row[0][0];
                int armortype = Convert.ToInt32(row[2][0]);
                
                if(aList.Contains(armortype) && !selected)
                {
                	aList.RemoveAll(x => x  == armortype);
                }
                
                if(!aList.Contains(armortype) && selected)
                {
                	aList.Add(armortype);
                }
                
                mSelectedRule.Element("ArmorType").Value = _ConvertIntListToCommaString(aList);
                          
                _UpdateRulesTabs();
                
            }catch (Exception ex) { LogError(ex); }
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

        private void lstRuleSpellsEnabled_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e)  
        {
            try
            {
                MyClasses.MetaViewWrappers.IListRow row = lstRuleSpellsEnabled[e.Row];
                List<int> spellist = _ConvertCommaStringToIntList(mSelectedRule.Element("Spells").Value);

                int spel = Convert.ToInt32(row[1][0]);
                
                if(spellist.Contains(spel))
                {
                	spellist.RemoveAll(x => x == spel);
                }
                
                mSelectedRule.Element("Spells").Value = _ConvertIntListToCommaString(spellist);
                
                _UpdateRulesTabs();
            }
            catch (Exception ex) { LogError(ex); }
        }

        private void lstRuleSpells_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e) 
        {
            try
            {
                MyClasses.MetaViewWrappers.IListRow row = lstRuleSpells[e.Row];
                List<int> spellist = _ConvertCommaStringToIntList(mSelectedRule.Element("Spells").Value);

                int spel = Convert.ToInt32(row[1][0]);
                
                if(!spellist.Contains(spel))
                {
                	spellist.Add(spel);
                }
                
                mSelectedRule.Element("Spells").Value = _ConvertIntListToCommaString(spellist);
                
                _UpdateRulesTabs();
                
            }
            catch (Exception ex) { LogError(ex); }

        }
        
        private void lstNotifySalvage_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e)  // Decal.Adapter.ListSelectEventArgs e)
        {
        	try
        	{
        		MyClasses.MetaViewWrappers.IListRow row = lstNotifySalvage[e.Row];
                
        		string Material = (row[3][0]).ToString();
                mSelectedSalvage = mSalvageList.Find(x => x.Element("intvalue").Value == Material);
                                
                if(e.Column == 0)
                {
                	mSelectedSalvage.Element("checked").Value = row[0][0].ToString().ToLower();
                	SaveSalvage();
                }
                
                _UpdateSalvagePanel();                          
        	}catch(Exception ex){LogError(ex);}


        }
        
        private void lstDamageTypes_Selected(object sender, MyClasses.MetaViewWrappers.MVListSelectEventArgs e) 
        {
        	try
        	{
                MyClasses.MetaViewWrappers.IListRow row = lstDamageTypes[e.Row];
                
                List<int> dList = _ConvertCommaStringToIntList(mSelectedRule.Element("DamageType").Value);
                
                bool selected = (bool)row[0][0];
                int damagetype = Convert.ToInt32(row[2][0]);
                
                if(dList.Contains(damagetype) && !selected)
                {
                	dList.RemoveAll(x => x  == damagetype);
                }
                
                if(!dList.Contains(damagetype) && selected)
                {
                	dList.Add(damagetype);
                }
                
                mSelectedRule.Element("DamageType").Value = _ConvertIntListToCommaString(dList);
                          
                _UpdateRulesTabs();	
        	}catch(Exception ex){LogError(ex);}
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
                }

                xdoc.Save(filename);

                if (xdoc == xdocTrophies)
                { populateTrophysListBox(); }
                else if (xdoc == xdocMobs)
                { populateMobsListBox(); }
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

        private void populateTrophysListBox()
        {
            try
            {
                int mList = 1;

                setUpLists(xdocTrophies, mSortedTrophiesList);
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
                setUpLists(xdocMobs, mSortedMobsList);
                populateLst(lstmyMobs, mSortedMobsList, mList);

            }

            catch (Exception ex) { LogError(ex); }

        }

        private void _UpdateSalvagePanel()
        {
        	try
        	{        		
        		List<XElement> SortedSalvageList = mSalvageList.OrderByDescending(x => x.Element("checked").Value).ThenBy(y => y.Element("key").Value).ToList();
        		
        		lstNotifySalvage.Clear();
        		
        		if(mSelectedSalvage == null) {mSelectedSalvage = mSalvageList.Find(x => x.Element("intvalue").Value == SortedSalvageList.First().Element("intvalue").Value);}
        		
				foreach (XElement element in SortedSalvageList)
                {
					MyClasses.MetaViewWrappers.IListRow newRow = lstNotifySalvage.AddRow();
					newRow[0][0] = Convert.ToBoolean(element.Element("checked").Value);
					newRow[1][0] = element.Element("key").Value;
					newRow[2][0] = element.Element("combine").Value;
                    newRow[3][0] = element.Element("intvalue").Value;
				}	
				
				lblSalvageName.Text = mSelectedSalvage.Element("key").Value;
				txtSalvageString.Text = mSelectedSalvage.Element("combine").Value;							
        	}catch(Exception ex){LogError(ex);}
        }
        
        private void btnUpdateSalvage_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)  //Decal.Adapter.ControlEventArgs e)
        {
			try
			{
				
				mSelectedSalvage.Element("combine").Value = txtSalvageString.Text;
				SaveSalvage();
				_UpdateSalvagePanel();
				
			}catch(Exception ex){LogError(ex);}
        }
        
        private void SaveSalvage()
        {
        	try
        	{
        		xdocSalvage = new XDocument(new XElement("GameItems"));
				
				foreach(XElement xe in mSalvageList)
				{
					xdocSalvage.Root.Add(xe);
				}
				
				xdocSalvage.Save(salvageFilename);
				
				FillSalvageRules();
        	}catch(Exception ex){LogError(ex);}
        }
        
        
    }
}
