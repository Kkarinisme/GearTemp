using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Decal.Interop.Net;
using Decal.Interop.Filters;
using Decal.Filters;
using System;
using System.Data;
using System.Drawing;
using System.Xml;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using WindowsTimer = System.Windows.Forms.Timer;
using VirindiViewService;
using MyClasses.MetaViewWrappers;
using System.IO;

namespace GearFoundry
{
    public partial class PluginCore : PluginBase
    {

        [ControlEvent("btnGetToonStats", "Click")]
        void btnGetToonStats_Click(object sender, MyClasses.MetaViewWrappers.MVControlEventArgs e)
        { getStats(); }

        void chkStats_Change(object sender, MyClasses.MetaViewWrappers.MVCheckBoxChangeEventArgs e)
        {
            try
            {
                btoonStatsEnabled = e.Checked;

                SaveSettings();
            }
            catch (Exception ex) { LogError(ex); }

        }



        private void getStats()
        {
            try
            {
               string account = Core.CharacterFilter.AccountName;
                string allegiance = Core.CharacterFilter.Allegiance.Name;
                string server = Core.CharacterFilter.Server;
                string monarch = Core.CharacterFilter.Monarch.Name;
                string patron = Core.CharacterFilter.Patron.Name;
                int level = Core.CharacterFilter.Level;
                string race = Core.CharacterFilter.Race;
                string gender = Core.CharacterFilter.Gender;
                int skillCredits = Core.CharacterFilter.SkillPoints;
                string xpToSpend = Core.CharacterFilter.UnassignedXP.ToString();
                string xpToLevel = Core.CharacterFilter.XPToNextLevel.ToString();
                xpToLevel = addCommas(xpToLevel);
                xpToSpend = addCommas(xpToSpend);


                if (!File.Exists(allStatsFilename))
                {

                    XDocument tempDoc = new XDocument(new XElement("Toons"));
                    tempDoc.Save(allStatsFilename);
                    tempDoc = null;


                }



                xDocStats = new XDocument(new XElement("Toons",
                      new XElement("Toon",
                        new XElement("Statistics",
                          new XElement("ToonName", toonName),
                          new XElement("Level", level),
                          new XElement("Account", account),
                          new XElement("Allegiance", allegiance),
                          new XElement("Server", server),
                          new XElement("Monarch", monarch),
                          new XElement("Patron", patron),
                          new XElement("Race", race),
                          new XElement("Gender", gender),
                          new XElement("SkillCredits", skillCredits),
                          new XElement("XpToSpend", xpToSpend),
                          new XElement("XpToLevel", xpToLevel)),
                         new XElement("Attributes"))));


                foreach (CharFilterAttributeType attrib in Enum.GetValues(typeof(CharFilterAttributeType)))
                {
                    string attrName = Core.CharacterFilter.Attributes[attrib].Name;
                    int attrBase = Core.CharacterFilter.Attributes[attrib].Base;
                    int attrBuff = Core.CharacterFilter.Attributes[attrib].Buffed;

                    xDocStats.Element("Toons").Element("Toon").Element("Attributes").Add(new XElement("Attribute",
                       new XElement("AttrName", attrName),
                       new XElement("AttrBase", attrBase),
                       new XElement("AttrBuff", attrBuff)));
                }

                // Need to get skills
                xDocStats.Element("Toons").Element("Toon").Add(new XElement("Skills",
                    new XElement("Specialized"),
                    new XElement("Trained")));

                Decal.Filters.FileService fs = CoreManager.Current.FileService as Decal.Filters.FileService;
                Decal.Interop.Filters.SkillInfo skillinfo = null;
                for (int i = 0; i < fs.SkillTable.Length; ++i)
                {
                    skillinfo = Core.CharacterFilter.Underlying.get_Skill((Decal.Interop.Filters.eSkillID)fs.SkillTable[i].Id);
                    string sklltype = skillinfo.Training.ToString();
                    string skllname = skillinfo.Name.ToString();
                    int skllbs = skillinfo.Base;
                    int skllbf = skillinfo.Buffed;
                    int skllbonus = skillinfo.Bonus;

                    if (sklltype == "eTrainSpecialized")
                    {
                        xDocStats.Element("Toons").Element("Toon").Element("Skills").Element("Specialized")
                           .Add(new XElement("SkllName", skllname,
                               new XElement("SkllBase", skllbs),
                               new XElement("SkllBuff", skllbf),
                               new XElement("SkllBonus", skllbonus)));


                    }
                    else
                    {
                        if (sklltype == "eTrainTrained")
                        {
                            xDocStats.Element("Toons").Element("Toon").Element("Skills").Element("Trained")
                              .Add(new XElement("SkllName", skllname,
                                  new XElement("SkllBase", skllbs),
                                  new XElement("SkllBuff", skllbf),
                                  new XElement("SkllBonus", skllbonus)));
                        }
                    }


                } // end of for i ....
                if (skillinfo != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(skillinfo);
                    skillinfo = null;
                }


                // need to get spells toon does not have in own spell book

                xDocStats.Element("Toons").Element("Toon").Add(new XElement("Spells",
                    new XElement("Creature"),
                    new XElement("Life"),
                    new XElement("Item"),
                    new XElement("War"),
                    new XElement("Void")));

                string spellName = null;
                int spellID = 0;
                foreach (int spellId in Core.CharacterFilter.SpellBook)
                {
                    Spell s = fs.SpellTable.GetById(spellId);
                    spellID = s.Id;
                    //      if (!Core.CharacterFilter.SpellBook.Contains(spellID))
                    //     {

                    spellName = s.Name;
                    string spellSchool = s.School.ToString();

                    if (spellName.Contains("III") || spellName.Contains("IV") || spellName.Contains("V") ||
                        spellName.Contains("VI"))
                    { continue; }
                    else
                    {

                        Decal.Filters.SpellComponentIDs spellcomps = s.ComponentIDs;
                        int complength = spellcomps.Length;

                        if (((spellcomps[0] == 112 || spellcomps[0] == 110 || spellcomps[0] == 193
                        || spellcomps[0] == 119035422 || spellcomps[7] == 63 || spellcomps[7] == 64
                        || spellcomps[3] == 66) || spellSchool.Contains("Void Magic")))
                        {

                            if (spellSchool == "Creature Enchantment")
                            {
                                xDocStats.Element("Toons").Element("Toon").Element("Spells").Element("Creature")
                                 .Add(new XElement("Spell", spellName));
                            }
                            if (spellSchool == "Life Magic")
                            {
                                xDocStats.Element("Toons").Element("Toon").Element("Spells").Element("Life")
                                 .Add(new XElement("Spell", spellName));
                            }
                            if (spellSchool == "Item Enchantment")
                            {
                                xDocStats.Element("Toons").Element("Toon").Element("Spells").Element("Item")
                                 .Add(new XElement("Spell", spellName));
                            }
                            if (spellSchool == "War Magic")
                            {
                                xDocStats.Element("Toons").Element("Toon").Element("Spells").Element("War")
                                 .Add(new XElement("Spell", spellName));
                            }
                            if (spellSchool == "Void Magic")
                            {
                                xDocStats.Element("Toons").Element("Toon").Element("Spells").Element("Void")
                                 .Add(new XElement("Spell", spellName));
                            }
                        } // if spellcomps[]
                    }// end of if spell contains
                    //  } // end of if not spellbook contains
                } // end of for


                xDocStats.Save(statsFilename);
                xDocStats = null;
                // removeToonfromFile();
                var MyDoc = XDocument.Load(allStatsFilename);
                MyDoc.Root.Add(XDocument.Load(statsFilename).Root.Elements());
                MyDoc.Save(holdingStatsFilename);
                MyDoc = null;
                xDocAllStats = XDocument.Load(holdingStatsFilename);
                xDocAllStats.Save(allStatsFilename);
                GearFoundry.PluginCore.WriteToChat("General statistics file has been saved. ");




                GearFoundry.PluginCore.WriteToChat("xDocStats saved");
            } //end of try

            catch (Exception ex) { LogError(ex); }
        } // end of getstats

        private string addCommas(string stringnum)
        {
            int initnumber = 0;
            int strnumberlength = stringnum.Length;
            int numgrpsthree = strnumberlength / 3;
            int numgrpbeg = strnumberlength - (numgrpsthree * 3);
            string temp = "";
            if (numgrpbeg > 0)
            {
                temp = stringnum.Substring(0, numgrpbeg);
            }
            for (int i = 0; i < numgrpsthree; i++)
            {
                initnumber = numgrpbeg + i * 3;
                if (temp.Length > 0)
                    temp = temp + "," + stringnum.Substring(initnumber, 3);
                else
                    temp = stringnum.Substring(0, 3);
            }
            return temp;

        }
    }
}

 

 