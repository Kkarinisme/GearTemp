using System.IO;
using System;

namespace GearFoundry
{



    public partial class PluginCore
    {


//        public int CurrentContainer
//        {
//            get { return mCurrentContainer; }
//        }

        public int GetLootDecision(int id, int reserved1, int reserved2)
        {
            //Loot connector injects wo.ID from vtank.  This sub-routine accesses loot rules for matching
            try
            {
                //Reserved2 = 1 check for rare
                if (reserved2 == 1)
                {
                    //corpse w/rare?  Loot
//                    if (mCorpseWithRareId != 0 && (reserved1 == mCorpseWithRareId))
//                    {
//                        return 1;
//                    }
//                    else
//                    {
//                        return 0;
//                        //else don't loot
//                    }
                }

                //check against notified items list
//                if (mNotifiedItems.ContainsKey(id))
//                {
//                    notify n = (global::GearFoundry.PluginCore.notify)mNotifiedItems[id];
//                    if (n.scantype == IOResult.salvage)
//                    {
//                        if (mPluginConfig.AutoUst)
//                        {
//                            return 1;
//                            // 2 = salvage but let alinco do the salvaging
//                        }
//                        else
//                        {
//                            return 2;
//                        }
//                    }
//                    else
//                    {
//                        return 1;
//                    }
//                }
                else
                {
                    Decal.Adapter.Wrappers.WorldObject wo = null;
                    wo = Core.WorldFilter[id];
                    //add item(id) to w
                    if (wo != null)
                    {
                        IdentifiedObject no = new IdentifiedObject(wo);
                        //makes a new IO type from no
                        //CheckObjectForMatch(no, false);
                        //calls CheckObject for Match
//                        if (result == IOResult.salvage)
//                        {
//                            if (mPluginConfig.AutoUst)
//                            {
//                                return 1;
//                                // 2 = salvage but let alinco do the salvaging
//                            }
//                            else
//                            {
//                                return 2;
//                            }
//                        }
//                        else if (result != IOResult.nomatch)
//                        {
//                            return 1;
//                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            //HACK:  Not all paths return a value
            return 0;
        }

//        public int LootNext(bool corpse, bool landscape, double maxrange, int maxz)
//        {
//            try
//            {
//                if (mwaitonopen)
//                {
//                    if (!((DateTime.Now.Second - mtryopenstart.Second) > 4))
//                    {
//                        //HACK:  Was return false, inconsistent return type
//                        //TODO:  Test functionality
//                        return 0;
//                    }
//                    mwaitonopen = false;
//                }
//
//                if (hotkeyloot(landscape, maxrange, maxz))
//                {
//                    return 1;
//                }
//
//                if (mPluginConfig.AutoUst)
//                {
////                    if (AutoUst())
////                    {
////                        return 1;
////                    }
//                }
//
//
//            }
//            catch (Exception ex)
//            {
//                LogError(ex);
//            }
//            //HACK:  Not all code paths return a value
//            return 0;
//        }

    }
}
