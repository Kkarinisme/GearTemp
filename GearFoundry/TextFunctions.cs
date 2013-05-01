/*
 * Created by SharpDevelop.
 * User: Paul
 * Date: 12/27/2012
 * Time: 8:06 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using Decal.Adapter;
using Decal.Filters;
using Decal.Adapter.Wrappers;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace GearFoundry
{

	public partial class PluginCore
	{
		//Util iUtil = new Util();

		//works with just "ChatBoxMessage"
		[BaseEvent("ChatBoxMessage")]
        private void Plugin_ChatBoxMessage(object sender, Decal.Adapter.ChatTextInterceptEventArgs e)
        {
        	//WriteToChat("I am at ChatBoxMessage");
            try {
        		//strip /n
        		string CBMessage = e.Text.Substring(0, e.Text.Length - 1);
        
                
//
//                switch (e.Color) {
//                    case 0:
//                    case 24:
//
//                        if (msg.EndsWith("This permission will last one hour.")) {
////                            pos = msg.IndexOf(" has given you permission to loot");
////                            if (pos >= 0) {
////                                string playercorpse = "Corpse of " + msg.Substring(0, pos);
////
////                                if (mAVVS.ProtectedCorpsesList.Contains(playercorpse)) {
////                                    WriteToChat("Alinco added " + playercorpse + " to protected corpses.");
////                                    mAVVS.ProtectedCorpsesList.Add(playercorpse);
////                                }
////
////                            }
//                        } else if (msg.EndsWith("DTLN")) {
//                            msg += "= Air (West)";
//                            wtcw(msg, e.Color);
//                            e.Eat = true;
//                        } else if (msg.EndsWith("DBTNK")) {
//                            msg += " = Water (South)";
//                            wtcw(msg, e.Color);
//                            e.Eat = true;
//                        } else if (msg.EndsWith("NTLN")) {
//                            msg += " = Fire (North)";
//                            wtcw(msg, e.Color);
//                            e.Eat = true;
//                        } else if (msg.EndsWith("ZTNK")) {
//                            msg += " = Earth (East)";
//                            wtcw(msg, e.Color);
//                            e.Eat = true;
//                        }
//                        break;
//                    case 7:
//                        //spellcasting
//                        if (mPluginConfig.FilterSpellcasting && msg.StartsWith("The spell")) {
//                            e.Eat = true;
//                        } else if (mPluginConfig.FilterSpellsExpire && msg.EndsWith("has expired.")) {
//                            e.Eat = true;
//                            // resists your spell
//                        } else if (mPluginConfig.FilterChatResists && msg.StartsWith("You resist the spell")) {
//                            e.Eat = true;
//
//                        }
//                        break;
//                    case 17:
//                        if (msg.IndexOf("Cruath Quareth") >= 0) {
//                            mSpellwords = "Cruath Quareth";
//                        } else if (msg.IndexOf("Cruath Quasith") >= 0) {
//                            mSpellwords = "Cruath Quasith";
//                        } else if (msg.IndexOf("Equin Opaj") >= 0) {
//                            mSpellwords = "Equin Opaj";
//                        } else if (msg.IndexOf("Equin Ozael") >= 0) {
//                            mSpellwords = "Equin Ozael";
//                        } else if (msg.IndexOf("Equin Ofeth") >= 0) {
//                            mSpellwords = "Equin Ofeth";
//                        } else if (mPluginConfig.FilterSpellcasting) {
//                            if (msg.StartsWith("The spell")) {
//                                e.Eat = true;
//                            } else if (msg.StartsWith("You say, ")) {
//                                e.Eat = true;
//                            } else if (msg.IndexOf("says,\"") > 0) {
//                                e.Eat = true;
//                            }
//                        }
//
//                        break;
//                    case 21:
//                        //melee evades
//                        if (mPluginConfig.FilterChatMeleeEvades) {
//                            if (msg.IndexOf("You evaded") >= 0) {
//                                e.Eat = true;
//                            }
//                        }
//                        break;
//                    case 3:
//                        if (mPluginConfig.FilterTellsMerchant | mPluginConfig.notifytells) {
//                            string actorname = actornamefromtell(msg);
//
//                            WorldObject cursel = Core.WorldFilter[Host.Actions.CurrentSelection];
//                            WorldObjectCollection actors = Core.WorldFilter.GetByName(actorname);
//
//                            bool vendorTell = false;
//                            bool npcTell = false;
//
//                            //multipe actors possible?
//                            foreach (WorldObject x in actors) {
//                                if (x.ObjectClass == ObjectClass.Npc) {
//                                    npcTell = true;
//                                } else if (x.ObjectClass == ObjectClass.Vendor) {
//                                    vendorTell = true;
//                                }
//                            }
//
//
//                            if (mPluginConfig.FilterTellsMerchant && vendorTell) {
//                                e.Eat = true;
//                                return;
//                            }
//
//
//                            if (mPluginConfig.notifytells & !vendorTell & !npcTell) {
//
////                                    if (cursel == null || ((!(msg.IndexOf(cursel.Name) >= 0)))) {
////                                        PlaySoundFile("rcvtell.wav", mPluginConfig.wavVolume);
////	
////                                    }
//
//                            }
//
//                        }
//                        break;
//                    }
                
	
            } catch (Exception ex) {
                LogError(ex);
            }
        }
        
        // Confirmed Functional -- Irquk 
        // TODO:  Verify individual flags are all still working.
        // TODO:  Review usefulness of different functions.
        // TODO:  Make menu items with buttons that will also call these functions.
        [BaseEvent("CommandLineText")]
		private void Plugin_CommandLineText(object sender, Decal.Adapter.ChatParserInterceptEventArgs e)
		{
			//WriteToChat("I am at CommandLineText");
			try 
			{
				if (e.Text.StartsWith("/alinco") || e.Text.StartsWith("@alinco"))
				{
					string cmd = e.Text.Substring(7).ToLower();
					//WriteToChat(cmd);
					
//					if(cmd.Contains("combine") && cmd.Contains("salvage"))
//					{	
//						CombineSalvageBags();
//					}
//					if(cmd.Contains("trade") && cmd.Contains("salvage"))
//					{
//						//TradeSalvage States:  "1" = all bags, "2" = only partial bags, default to full bags
//						if(cmd.Contains("all")){TradeSalvageBags(1);}
//						else if(cmd.Contains("partial")){TradeSalvageBags(2);}
//						else {TradeSalvageBags(0);}
//					}
//					if(cmd.Contains("sell") && cmd.Contains("salvage"))
//					{
//						SellSalvageBags();
//					}
					
				}

//				else if (cmd.StartsWith("trackxp")) {
//						Decal.Adapter.Wrappers.WorldObject b = Core.WorldFilter[Host.Actions.CurrentSelection];
//						if (b == null) {
//							wtcw("no object selected");
//						} else if (b.Id == Core.CharacterFilter.Id) {
//							wtcw("Track item xp off");
//							mCharconfig.trackobjectxpHudId = 0;
//	
//						} else if (!b.HasIdData) {
//							wtcw("Ident the selected object first then try again");
//	
//						} else if (!IsItemInInventory(b)) {
//							wtcw("The selected object must be in inventory");
//	
//						} else {
//							wtcw("trackxp on");
//							mCharconfig.trackobjectxpHudId = b.Id;
//						}
//	
//					} else if (cmd.StartsWith("alincodebug")) {
////						mDebugRules = !mDebugRules;
////						wtcw("Debugging rules, chatwindow 2 " + mDebugRules);
//					} else if (cmd.StartsWith("save")) {
//						forcesave();
//						e.Eat = true;
//					} else if (cmd.StartsWith("inventory export")) {
//						transforminventory1();
//						e.Eat = true;
//					} else if (cmd.StartsWith("inventory rescan")) {
//						startscanInventoryforSerialize(true);
//						e.Eat = true;
//					} else if (cmd.StartsWith("inventory update")) {
//						startscanInventoryforSerialize(false);
//						e.Eat = true;
//					} else if (cmd.StartsWith("inventory find")) {
//						e.Eat = true;
//					} else if (cmd.StartsWith("tradesalvagep")) {
//						tradesalvage(true);
//						e.Eat = true;
//					} else if (cmd.StartsWith("tradesalvage")) {
//						tradesalvage(false);
//						e.Eat = true;
//					} else if (cmd.StartsWith("tradeust")) {
//						tradeust();
//						e.Eat = true;
//					}  else if (cmd.StartsWith("reset")) {
//						TotalErrors = 0;
//						mKills = 0;
//						mXPStart = Core.CharacterFilter.TotalXP;
//						mXPStartTime = DateTime.Now;
//					} else if (e.Text.StartsWith("clear")) {
////						mNotifiedCorpses.Clear();
////						mNotifiedItems.Clear();
//						mColScanInventoryItems.Clear();
//						mColStacker.Clear();
//						midInventory.Clear();
//						// TODO:  See if we care or just eliminate this functionality
//	
//					} else if (cmd.StartsWith("find ")) {
//						int p = e.Text.IndexOf(" ");
//						// HACK:  Don't know what this does, can't easily determine how to make it work.
//						//int[] Idarray = (int)Enum.GetValues(typeof(eSomeTestColorsArg));
//			
//	
//	
//						if (p > 0 & p < e.Text.Length) {
//							int ncount = 0;
//							string s = e.Text.Substring(p + 1).ToLower();
//							wtcw("searching for->" + s);
//							int id = 0;
//							WorldObjectCollection wocol = Core.WorldFilter.GetLandscape();
//							foreach (WorldObject wo in wocol) {
//								if (wo.Name.ToLower().IndexOf(s) >= 0) {
//									markobject1(wo.Id);
////									if (!mNotifiedItems.ContainsKey(wo.Id) && !mNotifiedCorpses.ContainsKey(wo.Id)) {
////										notify newobject = new notify();
////										newobject.icon = wo.Icon;
////										newobject.id = wo.Id;
////										newobject.name = wo.Name;
////										newobject.description = wo.ObjectClass.ToString();
////										newobject.ColorArgb = 0xFF0000;
////										if (wo.ObjectClass == ObjectClass.Corpse) {
////											newobject.scantype = IOResult.corpse;
////											mNotifiedCorpses.Add(wo.Id, newobject);
////										} else {
////											newobject.scantype = IOResult.other;
////											mNotifiedItems.Add(wo.Id, newobject);
////										}
////									}
//									id = wo.Id;
//									ncount += 1;
//								}
//							}
//	
//							if (ncount == 1) {
//								Host.Actions.SelectItem(id);
//							} else {
//								wtcw(ncount + " items found");
//							}
//						}
//					} else if (cmd.StartsWith("alincotest")) {
//						wtcw("Alinco version: " + dllversion);
//						wtcw(".NET Framework : " + System.Environment.Version.ToString());
//						Decal.Adapter.Wrappers.WorldObject b = Core.WorldFilter[Host.Actions.CurrentSelection];
//						if (b == null) {
//							wtcw("no object selected");
//						} else {
//							//TODO:  Replaced significant hex foratting here, verify functionality
//							wtcw("selected : " + b.Name);
//							wtcw("Category    " + b.Category.ToString("X")); //Conversion.Hex(b.Category));
//							wtcw("Id    " + b.Id.ToString("X")); //Conversion.Hex(b.Id));
//							wtcw("Icon    " + b.Icon);
//							wtcw("ObjectClass    " + b.ObjectClass.ToString());
//							wtcw("Container    " + b.Container.ToString("X")); //Conversion.Hex(b.Container));
//							wtcw("HouseOwner    " + b.Values(LongValueKey.HouseOwner).ToString("X")); //Conversion.Hex(b.Values(LongValueKey.HouseOwner)));
//							wtcw("Wielder    " + b.Values(LongValueKey.Wielder).ToString("X")); //Conversion.Hex(b.Values(LongValueKey.Wielder)));
//							wtcw(" ");
//							wtcw("HasIdData : " + b.HasIdData);
//							wtcw("EquipType    " + b.Values(LongValueKey.EquipType).ToString("X"));  //Conversion.Hex(b.Values(LongValueKey.EquipType)));
//							wtcw("Coverage    " + b.Values(LongValueKey.Coverage).ToString("X"));
//							wtcw("Behavior    " + b.Behavior.ToString("X"));
//							wtcw("WieldReqType    " + b.Values(LongValueKey.WieldReqType).ToString("X"));
//							wtcw("WieldReqValue   " + (b.Values(LongValueKey.WieldReqValue)));
//							wtcw("WieldReqAttribute   " + (b.Values(LongValueKey.WieldReqAttribute)));
//							wtcw("MissileType    " + (b.Values(LongValueKey.MissileType)));
//							wtcw("AssociatedSpell    " + (b.Values(LongValueKey.AssociatedSpell)));
//							wtcw("EquipableSlots    " + b.Values(LongValueKey.EquipableSlots).ToString("X"));
//							wtcw("EquippedSlots    " + b.Values(LongValueKey.EquippedSlots).ToString("X"));
//							wtcw("ActivationReqSkillId    " + b.Values(LongValueKey.ActivationReqSkillId).ToString("X"));
//							wtcw("EquipSkill    " + b.Values(LongValueKey.EquipSkill).ToString("X"));
//							wtcw("Type    " + b.Values(LongValueKey.Type).ToString("X"));
//							wtcw("setid    " + b.Values((LongValueKey)0x109).ToString("X"));
//							wtcw(" ");
//							wtcw("HealKitSkillBonus    " + (b.Values(LongValueKey.HealKitSkillBonus)));
//							wtcw("KeysHeld    " + (b.Values(LongValueKey.KeysHeld)));
//							wtcw("UsesRemaining    " + (b.Values(LongValueKey.UsesRemaining)));
//							wtcw("UsesTotal    " + (b.Values(LongValueKey.UsesTotal)));
////							Dim strspells As String = String.Empty
////							For i As Integer = 1 To b.SpellCount - 1
////							    Dim oSpell As Decal.Filters.Spell = Plugin.FileService.SpellTable.GetById(b.Spell(i))
////							    If Not oSpell Is Nothing Then
////							        strspells &= ", " & oSpell.Name & " 0x" & Hex(oSpell.Id)
////							    End If
////							Next
////							If strspells <> String.Empty Then
////							    wtcw("  " & strspells)
////							End If
////							For i As Integer = 0 To Plugin.FileService.SpellTable.Length - 1
////							    Try
////							        Dim oSpell As Decal.Filters.Spell = Plugin.FileService.SpellTable.Item((i))
////							        If Not oSpell Is Nothing AndAlso Not oSpell.IsDebuff Then
////	
////							            If oSpell.Name.ToLower.Contains("two handed") Then
////							                wtcw(oSpell.Name & " &H" & Hex(oSpell.Id))
////							            End If
////							        End If
////							    Catch ex As Exception
////							    End Try
////							Next
//						}
//					} else if (cmd.StartsWith("alinco")) {
//						long frequency = Stopwatch.Frequency;
//						double nanosecPerTick = (1000L * 1000L * 1000L) / frequency;
//						wtcw(string.Format("Lookup Statistics: Timer is accurate within {0} nanoseconds", nanosecPerTick.ToString("0.00")));
//						wtcw(" ");
//						wtcw("Alinco version " + dllversion + ", available commands: ");
//						wtcw(" salvage mule:");
//						wtcw(" /tradeust      => adds the items from the ust list to a open tradewindow");
//						wtcw(" /tradesalvage  => adds all salvage to a open tradewindow");
//						wtcw(" /tradesalvagep => partial bags only (< 100 units)");
//						wtcw(" ");
//						wtcw(" /inventory export => serializes inventory to userdocuments\\decal plugins\\Alinco3\\inventory");
//						wtcw(" /inventory update => update inventory");
//						wtcw(" /inventory find   => not implemented");
//						wtcw(" ");
//						wtcw(" /save         => forces to save all settings to disk");
//						wtcw(" /reset        => reset xp/h");
//						wtcw(" /find         => searches the landscape");
//						wtcw(" /clear        => clears the listboxes with matched items and corpses");
//						wtcw(" /sell         => Adds the salvage to the vendors trade window");
//						e.Eat = true;
//					}
////					else {
////						if (mPluginConfig != null && mPluginConfig.Shortcuts != null) {
////							foreach (KeyValuePair<string, string> x in mPluginConfig.Shortcuts) {
////								if (!string.IsNullOrEmpty(x.Key) && (e.Text.ToLower().StartsWith("/" + x.Key.ToLower().ToString()) | e.Text.ToLower().StartsWith("@" + x.Key.ToLower().ToString()))) {
////									e.Eat = true;
////									Host.Actions.InvokeChatParser("/" + x.Value);
////									break; // TODO: might not be correct. Was : Exit For
////								}
////							}
////						}
////	
////					}
//	
//				}
	
	
			} catch (Exception ex) {
				LogError(ex);
			}
		}
		
		[BaseEvent("ChatNameClicked")]
        private void Plugin_ChatNameClicked(object sender, Decal.Adapter.ChatClickInterceptEventArgs e)
        {

            try
            {

                switch (e.Id)
                {
                    case NOTIFYLINK_ID:

                        int Itemid = Convert.ToInt32(e.Text);
//                        if (mHudlistboxItems.ContainsKey(Itemid))
//                        {
//                            huditemclick(true, (global::GearFoundry.PluginCore.notify)mHudlistboxItems[Itemid], true);
//                        }
                        e.Eat = true;

                        break;
                    case ERRORLINK_ID:

                        string url = null;
                        url = docPath + "\\Errors.txt";


//                        if (File.Exists(url))
//                        {
//                            System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
//                            myProcess.StartInfo.FileName = "notepad.exe";
//                            myProcess.StartInfo.Arguments = url;
//                            myProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
//                            myProcess.Start();
//
//                        }
                        e.Eat = true;
                        break;
                }

            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }
        
	}
}
