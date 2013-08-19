using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Decal;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Decal.Filters;
using Decal.Adapter.NetParser;
using Decal.Adapter.Messages;
using System.Xml.Serialization;
using VirindiViewService;
using MyClasses.MetaViewWrappers;
using WindowsTimer = System.Windows.Forms.Timer;


namespace GearFoundry
{
    public partial class PluginCore : PluginBase
    {

        private void doGetInventory()
        {
            try
            {
            	if(programinv.Contains("armor"))
            	{
            		WriteToChat("Cannot run general inventory until armor inventory is completed.");
            	}
            	else{
            	programinv = "inventory";
                xdocToonInventory = new XDocument(new XElement("Objs"));
                xdocGenInventory = XDocument.Load(genInventoryFilename);
                //Need a list to hold the inventory
                mWaitingForIDTimer = new WindowsTimer();
                mWaitingForID = new List<WorldObject>();
                mCurrID = new List<string>();

                if (!File.Exists(genInventoryFilename))
                {
                    XDocument tempGIDoc = new XDocument(new XElement("Objs"));
                    tempGIDoc.Save(genInventoryFilename);
                    tempGIDoc = null;
                }


                foreach (Decal.Adapter.Wrappers.WorldObject obj in Core.WorldFilter.GetInventory())
                {
                    try
                    {
                        
                        objID = obj.Id;
                        string sobjID = objID.ToString();
                        mCurrID.Add(sobjID);
                        Globals.Host.Actions.RequestId(obj.Id);
                        mWaitingForID.Add(obj);
                    }
                    catch (Exception ex) { LogError(ex); }


                } // endof foreach world object

                ProcessDataInventory(); //This one in the doget inventory

                // initialize event timer for processing inventory
                mWaitingForIDTimer.Tick += new EventHandler(TimerEventProcessor);

                // Sets the timer interval to 5 seconds.
                mWaitingForIDTimer.Interval = 10000;



                //Now need to start routines that will continue to get data as becomes available or will end the search and save the files
                mIsFinished();  
            	}
            } //end of try
            catch (Exception ex) { LogError(ex); }
        } // end of dogetinventory




        public void mIsFinished()
        {
            try
            {
                int n;
                if (mWaitingForID != null){ n = mWaitingForID.Count; }
                else {  n = 0; }
                string s = n.ToString();
                if (n == 0)
                {
                    try
                    {
                        if (mWaitingForIDTimer != null) { mWaitingForIDTimer.Tick -= new EventHandler(TimerEventProcessor); mWaitingForIDTimer = null; }
                        removeExcessObjsfromFile();
                        xdocGenInventory.Element("Objs").Descendants("Obj").Where(x => x.Element("ToonName").Value == toonName).Remove();
                        
                        xdocGenInventory.Root.Add(XDocument.Load(inventoryFilename).Root.Elements());

                        xdocGenInventory.Save(genInventoryFilename);
                        GearFoundry.PluginCore.WriteToChat("General Inventory file has been saved. ");
                        m = 500;
                        if (mWaitingForID != null) {mWaitingForID = null;}
                        if (xdocGenInventory != null) {xdocGenInventory = null;}
                        if (xdocToonInventory != null) {xdocToonInventory = null;}
                        if (programinv != null) { programinv = "";}
                    }
                    catch (Exception ex) { LogError(ex); }
                 }
                 else if (n < m )
                 {
                        GearFoundry.PluginCore.WriteToChat("Inventory remaining to be ID'd: " + s);
                        m = n;
                       // string mname = null;
                        

                        if (mWaitingForID.Count > 0)
                        {
                            //if (binventoryWaitingEnabled)
                            //{
                            //    for (int i = 0; i < n; i++)
                            //    {
                            //        mname = mWaitingForID[i].Name;
                            //        GearFoundry.PluginCore.WriteToChat(mname);

                            //    }
                            //}
                            mDoWait();
                        }
                    }


 
                
                }
           
            catch (Exception ex) { LogError(ex); }
        }



         public void mDoWait()
        {
            try
            {
                mWaitingForIDTimer.Start();
           }
            catch (Exception ex) { LogError(ex); }

        }

        public void TimerEventProcessor(Object Sender, EventArgs mWaitingForIDTimer_Tick)

        {
            try
            {


                if (mWaitingForIDTimer != null) { mWaitingForIDTimer.Stop(); }
 
                if (mWaitingForID.Count > 0)
                {
                    for (int n = 0; n < mWaitingForID.Count; n++)
                    {
                         if (mWaitingForID[n] != null && mWaitingForID[n].HasIdData)
                        {

                            ProcessDataInventory();
                            mIsFinished();

                        }
                        else { mDoWait(); }

                    }
                }
                }

                catch (Exception ex) { LogError(ex); }
        }


        //This is routine that puts the data of an obj into the inventory file xml
        private void ProcessDataInventory()
        {
 
            for (int n = 0; n < mWaitingForID.Count; n++)
            {
                try
                {
                    if (mWaitingForID[n] != null && mWaitingForID[n].HasIdData)
                    {
                        currentobj = mWaitingForID[n];
                        mWaitingForID.Remove(mWaitingForID[n]);
                        objClassName = currentobj.ObjectClass.ToString();
                        objName = currentobj.Name;
                        objID = currentobj.Id;
                        objIcon = currentobj.Icon;
                        LootObject whatsmygearscore = new LootObject(currentobj);
                        objGearScore = whatsmygearscore.GearScore;
                        long objDesc = currentobj.Values(LongValueKey.DescriptionFormat);
                        long objMat = currentobj.Values(LongValueKey.Material);
                        long objCatType = (int)currentobj.Values(LongValueKey.Category);
                        long objAtt = (int)currentobj.Values(LongValueKey.Attuned);
                        long objBnd = (int)currentobj.Values(LongValueKey.Bonded);
                        long objToonLevel = (int)currentobj.Values(LongValueKey.MinLevelRestrict);
                        long objLore = (int)currentobj.Values(LongValueKey.LoreRequirement);
                        long objAl = (int)currentobj.Values(LongValueKey.ArmorLevel);
                        long objType = (int)currentobj.Values(LongValueKey.Type);
                        long objTinks = (int)currentobj.Values(LongValueKey.NumberTimesTinkered);
                        long objWork = (int)currentobj.Values(LongValueKey.Workmanship);
                        long objSet = (int)currentobj.Values(LongValueKey.ArmorSet);
                        long objCovers = currentobj.Values(LongValueKey.Coverage);
                        long objEqSlot = currentobj.Values(LongValueKey.EquipableSlots);
                        long objCleaveType = (int)currentobj.Values(LongValueKey.CleaveType);
                        long objElemDmg = (int)currentobj.Values(LongValueKey.ElementalDmgBonus);
                        long objEmbue = (int)currentobj.Values(LongValueKey.Imbued);
                        long objSlayer = (int)currentobj.Values(LongValueKey.SlayerSpecies);
                        long objWieldAttrInt = (int)currentobj.Values(LongValueKey.WieldReqAttribute);
                        long objWieldType = (int)currentobj.Values(LongValueKey.WieldReqType);
                        long objWieldValue = (int)currentobj.Values(LongValueKey.WieldReqValue);
                        long objDamage = (int)currentobj.Values(LongValueKey.DamageType);
                        long objMissType = (int)currentobj.Values(LongValueKey.MissileType);
                        long objSkillLevReq = (int)currentobj.Values(LongValueKey.SkillLevelReq);
                        double objElemvsMons = currentobj.Values(DoubleValueKey.ElementalDamageVersusMonsters);
                        double objMelD = currentobj.Values(DoubleValueKey.MeleeDefenseBonus);
                        double objMagicD = currentobj.Values(DoubleValueKey.MagicDBonus);
                        double objManaC = currentobj.Values(DoubleValueKey.ManaCBonus);
                        double objMissileD = currentobj.Values(DoubleValueKey.MissileDBonus);
                        double objSalvWork = currentobj.Values(DoubleValueKey.SalvageWorkmanship);
                        double objAttack = currentobj.Values(DoubleValueKey.AttackBonus);
                        double objDamageBonus = currentobj.Values(DoubleValueKey.DamageBonus);
                        double objAcid = currentobj.Values(DoubleValueKey.AcidProt);
                        double objLight = currentobj.Values(DoubleValueKey.LightningProt);
                        double objFire = currentobj.Values(DoubleValueKey.FireProt);
                        double objCold = currentobj.Values(DoubleValueKey.ColdProt);
                        double objBludg = currentobj.Values(DoubleValueKey.BludgeonProt);
                        double objSlash = currentobj.Values(DoubleValueKey.SlashProt);
                        double objPierce = currentobj.Values(DoubleValueKey.PierceProt);
                        long objMastery = currentobj.Values(LongValueKey.ActivationReqSkillId);
                        long objMaxDamage = currentobj.Values(LongValueKey.MaxDamage);
                        double objVariance = currentobj.Values(DoubleValueKey.Variance);
                        objSpellXml = GoGetSpells(currentobj);
                        long objMagicDam = currentobj.Values(LongValueKey.WandElemDmgType);
                        long objRareID = currentobj.Values(LongValueKey.RareId);
                        long objBurden = currentobj.Values(LongValueKey.Burden);
                        long objStackCount = currentobj.Values(LongValueKey.StackCount);
                        long objModel = currentobj.Values(LongValueKey.Model);
                        long iconUnderlay = currentobj.Values(LongValueKey.IconUnderlay);
                        long iconOverlay = currentobj.Values(LongValueKey.IconUnderlay);
                        long iconOutline = currentobj.Values(LongValueKey.IconOutline);
                        long objFlags = currentobj.Values(LongValueKey.Flags);
                        long objCreateFlag1 = currentobj.Values(LongValueKey.CreateFlags1);
                        long objCreateFlag2 = currentobj.Values(LongValueKey.CreateFlags2);
                        long objUnknown10 = currentobj.Values(LongValueKey.Unknown10);
                        long objUnknown100000 = currentobj.Values(LongValueKey.Unknown100000);
                        long objUnknown800000 = currentobj.Values(LongValueKey.Unknown800000);
                        long objUnknown8000000 = currentobj.Values(LongValueKey.Unknown8000000);
                        long objUsageMask = currentobj.Values(LongValueKey.UsageMask);

                            xdocToonInventory.Element("Objs").Add(new XElement("Obj",
                            new XElement("ObjName", objName),
                            new XElement("ObjID", objID),
                            new XElement("ToonName", toonName),
                            new XElement("ObjIcon", objIcon),
                            new XElement("GearScore",objGearScore),
                            new XElement("ObjClass", objClassName),
                            new XElement("ObjDesc", objDesc),
                            new XElement("ObjMaterial", objMat),
                            new XElement("ObjAl", objAl),
                            new XElement("ObjSet", objSet),
                            new XElement("ObjCovers", objCovers),
                            new XElement("ObjEqSlot", objEqSlot),
                            new XElement("ObjToonLevel", objToonLevel),
                            new XElement("ObjLoreReq", objLore),
                            new XElement("ObjSkillLevReq", objSkillLevReq),
                            new XElement("ObjWork", objWork),
                            new XElement("ObjTink", objTinks),
                            new XElement("ObjCatType", objCatType),
                            new XElement("ObjCleaveType", objCleaveType),
                            new XElement("ObjMissType", objMissType),
                            new XElement("ObjType", objType),
                            new XElement("ObjElemDmg", objElemDmg),
                            new XElement("ObjAtt", objAtt),
                            new XElement("ObjBnd", objBnd),
                            new XElement("ObjEmbue", objEmbue),
                            new XElement("ObjSlayer", objSlayer),
                            new XElement("ObjWieldAttr", objWieldAttrInt),
                            new XElement("ObjWieldType", objWieldType),
                            new XElement("ObjWieldValue", objWieldValue),
                            new XElement("ObjDamage", objDamage),
                            new XElement("ObjElemvsMons", objElemvsMons),
                            new XElement("ObjMelD", objMelD),
                            new XElement("ObjMagicD", objMagicD),
                            new XElement("ObjManaC", objManaC),
                            new XElement("ObjMissileD", objMissileD),
                            new XElement("ObjSalvWork", objSalvWork),
                            new XElement("ObjAttack", objAttack),
                            new XElement("ObjDamageBonus", objDamageBonus),
                            new XElement("ObjMaxDamage", objMaxDamage),
                            new XElement("ObjVariance", objVariance),
                            new XElement("ObjMastery", objMastery),
                            new XElement("ObjSpellXml", objSpellXml),
                            new XElement("ObjMagicDamage", objMagicDam),
                            new XElement("ObjBurden", objBurden),
                            new XElement("ObjStackCount", objStackCount),
                            new XElement("ObjAcid", objAcid),
                            new XElement("ObjLight", objLight),
                            new XElement("ObjFire", objFire),
                            new XElement("ObjCold", objCold),
                            new XElement("ObjBludg", objBludg),
                            new XElement("ObjSlash", objSlash),
                            new XElement("ObjPierce", objPierce),
                            new XElement("ObjRareID", objRareID),
                            new XElement("IconOverlay", iconOverlay),
                            new XElement("IconOutline", iconOutline),
                            new XElement("IconUnderlay", iconUnderlay),
                            new XElement("ObjFlags", objFlags),
                            new XElement("ObjCreateFlag1", objCreateFlag1),
                            new XElement("ObjCreateFlag2", objCreateFlag2),
                            new XElement("ObjUnknown10", objUnknown10),
                            new XElement("ObjUnknown100000", objUnknown100000),
                            new XElement("ObjUnknown800000", objUnknown800000),
                            new XElement("ObjUnknown8000000", objUnknown8000000),
                            new XElement("ObjUsageMask", objUsageMask)));
                        
                            currentobj = null;
                            objClassName = null;
                            objName = null;
                            objDesc = 0;
                            objID = 0;
                            objIcon = 0;
                            objGearScore = 0;

                            objAl = 0;
                            objSet = 0;
                            objMat = 0;
                            objCovers = 0;
                            objToonLevel = 0;
                            objLore = 0;
                            objSkillLevReq = 0;
                            objTinks = 0;
                            objWork = 0;
                            objCatType = 0;
                            objCleaveType = 0;
                            objMissType = 0;
                            objType = 0;
                            objElemDmg = 0;
                            objMastery = 0;

                            objAtt = 0;
                            objBnd = 0;
                            objEmbue = 0;
                            objSlayer = 0;
                            objWieldAttrInt = 0;
                            objWieldType = 0;
                            objWieldValue = 0;
                            objDamage = 0;
                            objElemvsMons = 0;
                            objMelD = 0;
                            objMagicD = 0;
                            objManaC = 0;
                            objMissileD = 0;
                            objSalvWork = 0;
                            objAttack = 0;
                            objDamageBonus = 0;
                            objEqSlot = 0;
                            objVariance = 0;
                            objMaxDamage = 0;
                            objSpellXml = null;

                            objMagicDam = 0;
                            objRareID = 0;
                            objBurden = 0;
                            objStackCount = 0;
                            objAcid = 0;
                            objLight = 0;
                            objFire = 0;
                            objCold = 0;
                            objBludg = 0;
                            objSlash = 0;
                            objPierce = 0;

                            objModel = 0;


                            iconOverlay = 0;
                            iconUnderlay = 0;
                            iconOutline = 0;

                            objFlags = 0;
                            objCreateFlag1 = 0;
                            objCreateFlag2 = 0;
                            objUnknown10 = 0;
                            objUnknown100000 = 0;
                            objUnknown800000 = 0;
                            objUnknown8000000 = 0;
                            objUsageMask = 0;
                        }
                    } // end of try



                catch (Exception ex) { LogError(ex);  }

            } // end of for



        } // end of process data

        private string GoGetSpells(Decal.Adapter.Wrappers.WorldObject o)
        {
            FileService fs = (FileService)Core.FileService;
            int intspellcnt = o.SpellCount;
            string oXmlSpells = "";
            for (int i = 0; i < intspellcnt; i++)
            {
                int spellId = o.Spell(i);

                Spell spell = fs.SpellTable.GetById(spellId);

                string spellName = spell.Name;
                if (spellName.Contains("Major") || spellName.Contains("Epic") ||
                  spellName.Contains("Incantation") || spellName.Contains("Surge")
                    || spellName.Contains("Cloaked in Skill") || spellName.Contains("Legendary"))
                {
                    oXmlSpells = oXmlSpells + ", " + spellName;

                }

            }
            return oXmlSpells;
        }  //endof gogetspells



        public void removeExcessObjsfromFile()
        {
            try
            {
                int oldCount = 0;
                int newCount = 0;
                if (xdocToonInventory != null)
                {
                   if (xdocToonInventory.Element("Objs").Descendants("Obj") != null && xdocToonInventory.Element("Objs").Descendants("Obj").Count() > 0)
                    {

                        IEnumerable<XElement> elements = xdocToonInventory.Element("Objs").Descendants("Obj");
                        oldCount = elements.Count();
                        WriteToChat("oldCount: " + oldCount.ToString());
                        var obj = from o in xdocToonInventory.Descendants("Obj")
                                  where !mCurrID.Contains(o.Element("ObjID").Value)
                                  select o;
                        obj.Remove();
                    }
                    else
                    {
                        oldCount = 0;
                    }
                }
                if (xdocToonInventory.Element("Objs").Descendants("Obj") != null && xdocToonInventory.Element("Objs").Descendants("Obj").Count() > 0)
                    {
                        newCount = (int)(xdocToonInventory.Element("Objs").Elements("Obj").Count());
                        WriteToChat("newCount: " + newCount.ToString());

                    }
                    else { newCount = 0; }
                    int count = oldCount - newCount;
                    GearFoundry.PluginCore.WriteToChat(count.ToString() + " objects removed from inventory of " + toonName);
                    xdocToonInventory.Save(inventoryFilename);
                    GearFoundry.PluginCore.WriteToChat(toonName + " inventory file has been saved.");
                    if (xdocToonInventory != null) { xdocToonInventory = null; }
                    if (moldObjsID != null) { moldObjsID = null; }
                    if (mWaitingForID != null) { mWaitingForID = null; }
                    if (mCurrID != null) { mCurrID = null; }
                }
            
            catch (Exception ex) { LogError(ex); }
        }

    }
}// end of namespace




