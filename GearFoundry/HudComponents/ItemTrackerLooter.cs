﻿using System;
using System.Drawing;
using System.IO;
using Decal.Adapter;
using Decal.Filters;
using Decal.Adapter.Wrappers;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using VirindiViewService;
using MyClasses.MetaViewWrappers;
using VirindiViewService.Controls;
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;
using VirindiViewService.Themes;
using System.Xml.Serialization;
using System.Xml;

namespace GearFoundry
{
	public partial class PluginCore
	{		
		private void SubscribeItemEvents()
		{
			try
			{	
				GearInspectorReadWriteSettings(true);
								
				Core.ContainerOpened += LootContainerOpened;
				Core.ItemDestroyed += ItemTracker_ItemDestroyed;
				Core.WorldFilter.ReleaseObject += ItemTracker_ObjectReleased; 
				Core.WorldFilter.ChangeObject += ItemTrackerActions_ObjectChanged;
				Core.WorldFilter.CreateObject += InventoryCreated;
				Core.CharacterFilter.Logoff += ItemHudLogOff;  

				SetKnownSpellSchools();
			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void ItemHudLogOff(object sender, EventArgs e)
		{
			try
			{
				UnsubscribeItemEvents();
				DisposeItemHud();
				ClearKnownSpellSchools();
				
				WaitingVTIOs.Clear();
				
				if(WaitingVTIOs.Count == 0) 
				{
					Core.RenderFrame -= DoesVTIOHaveID;
					return;
				}
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnsubscribeItemEvents()
		{
			try
			{	
				Core.ContainerOpened -= LootContainerOpened;
				Core.ItemDestroyed -= ItemTracker_ItemDestroyed;
				Core.WorldFilter.ReleaseObject -= ItemTracker_ObjectReleased;
				Core.WorldFilter.ChangeObject -= ItemTrackerActions_ObjectChanged;
				Core.WorldFilter.CreateObject -= InventoryCreated;
				Core.CharacterFilter.Logoff -= ItemHudLogOff;			

				LOList.Clear();
				
				GearInspectorReadWriteSettings(false);
			
			}catch(Exception ex){LogError(ex);}
		}
		
//		private void ItemTracker_ServerDispatch(object sender, NetworkMessageEventArgs e)
//		{
//			try
//			{
//				if(e.Message.Type == AC_CREATE_OBJECT
//				
//				
//				e.Message.Type == AC_GAME_EVENT;
//				
//				
//			}catch(Exception ex){LogError(ex);}
//		}
		
		//		Deadeye's Color System palette Code.			
//		private void Echo_CreateObject(IMessage2 msg) // F745
//		{
//			string test = "Start";
//            object o = null;
//			try 
//			{
//                int guid = (int)msg.get_Value("object");
//                //ITEM_CLASS eClass;
//				test = "paletteCount";
//                IMessageMember mem = msg.get_Struct(test = "model");
//				int i;
//				for (i=0; i< m_nKnownItems; i++)
//				{
//					if (KnownItemArray[i].guid == guid) return;
//				}
//                o = mem.get_Value(test = "paletteCount");
//                test = "cast paletteCount";
//                int paletteCount = (byte)o;
//                o = mem.get_Value(test = "textureCount");
//                test = "case textureCount";
//                int textureCount = (byte)o;
//				IMessageIterator aPalettes = (IMessageIterator)mem.get_Struct(test = "palettes");
//                IMessageIterator aTextures = (IMessageIterator)mem.get_Struct(test = "textures");
//
//                mem = msg.get_Struct(test = "game");
//                o = mem.get_Value(test = "name");
//                string name = o.ToString();
//				
//                o = mem.get_Value(test = "type");
//				int model = (int)o;
//
//                o = mem.get_Value(test = "icon");
//                int icon = (int)o;
//
//                o = mem.get_Value(test = "category");
//                int nTypeFlags = (int)o;
//				if ((nTypeFlags & 0x06)==0) return;   // Only Armor and Clothing
//
//				int pyrealvalue;
//				int coverage;
//				int burden;      
//				try { pyrealvalue  = (int)mem.get_Value("value");    }catch{pyrealvalue=0;}
//				try { coverage     = (int)mem.get_Value("coverage1");}catch{coverage   =0;}
//				try { burden       = (int)mem.get_Value("burden");   }catch{burden     =0;}
//			
//				int[]  Color = new int[paletteCount];
//				for(i=0; i<paletteCount; i++)
//				{
//					if (m_bStats)
//					{
//						test = "palette iteration #"+i.ToString();
//						IMessageIterator palette = aPalettes.NextObjectIndex;
//						int iOS = palette.get_NextInt(test="palette");
//                        int iOffset = palette.get_NextInt(test="offset");
//                        int iSize = palette.get_NextInt(test ="length");
//                        Color[i] = iOS; //  + palette.get_NextInt("length") * 256;
//						//if (ColorTable.Contains(Color[i]) == false)
//						{
//							test = Color[i].ToString()
//								+ "," + iOffset.ToString()
//								+ "," + iSize.ToString();
//                            m_Hooks.AddChatText("DCS: Color " + test + " on " + name, 14, 1);
//							StreamWriter sw = new StreamWriter(/*m_sAssemblyPath+*/"F:\\ColorTrap.csv",true);
//							sw.Write(name+","+model.ToString()+",#"+coverage.ToString("X8")+","+i.ToString()+","+test+m_sEOL);
//							sw.Flush();
//							sw.Close();
//						}
//					}
//					else
//					{
//						test = "palette iteration #"+i.ToString();
//						IMessageIterator palette = aPalettes.NextObjectIndex;
//                        Color[i] = palette.get_NextInt(test = "palette");
//					}
//				}
//
//				COLOR_INFO NewColor;
//				//AC_MODEL   acModel;
//				ushort     iModel = (ushort)model;
//				/*
//				eClass = ITEM_CLASS.NONE;
//				switch (coverage)
//				{
//					case 0x00200000:  // Shield
//						return;
//					case 0x00000400:  // Girth
//						eClass |= ITEM_CLASS.GIRTH;
//						acModel = new AC_MODEL(iModel, "AB", name);
//						break;
//					case 0x00000200:  // Breastplate
//						eClass |= ITEM_CLASS.BREASTPLATE;
//						acModel = new AC_MODEL(iModel, "AB", name);
//						break;
//					case 0x00001800:  // Sleeves
//						eClass |= ITEM_CLASS.SLEEVES;
//						acModel = new AC_MODEL(iModel, "AC", name);
//						break;
//					case 0x00000600:  // Curaiss
//						eClass |= ITEM_CLASS.CURAISS;
//						acModel = new AC_MODEL(iModel, "AC", name);
//						break;
//					case 0x00000E00:  // Short Sleeve Shirt
//						eClass |= ITEM_CLASS.OVERSHIRT;
//						acModel = new AC_MODEL(iModel, "AD", name);
//						break;
//					case 0x00001A00:  // Coat
//						eClass |= ITEM_CLASS.COAT;
//						acModel = new AC_MODEL(iModel, "ABD", name);
//						break;
//					case 0x00002000:  // Tassets (have no color)
//						acModel = new AC_MODEL(iModel, "", name);
//						break;
//					case 0x00007F00:  // Robe
//					case 0x00007F01:  // Hooded Robe
//						eClass |= ITEM_CLASS.ROBE;
//						acModel = new AC_MODEL(iModel, "ABDE", name);
//						break;
//					case 0x00001E00:  // Hauberk  
//						eClass |= ITEM_CLASS.HAUBERK;
//						acModel = new AC_MODEL(iModel, "AE", name);
//						break;
//					case 0x00006400:  // Pants
//						eClass |= ITEM_CLASS.OVERPANTS;
//						if (model == 6004)
//							acModel = new AC_MODEL(iModel, "ACDE", name);
//						else
//							acModel = new AC_MODEL(iModel, "AC", name);
//						break;
//					default: 
//						acModel = new AC_MODEL(iModel, "ABCD", name);
//						break;
//				}
//				*/
//
//				
//				/*if (ModelTable.Contains(model))
//				{
//					AC_MODEL acModel = (AC_MODEL)ModelTable[model];
//					NewColor = new COLOR_INFO(guid,name,model,icon,coverage,acModel.Colors);
//					for (i=0; i < acModel.Colors; i++)
//					{
//						NewColor.SetColor(i,Color[acModel.GetColor(i)]);
//					}
//				}
//				else 
//				{ */
//					NewColor = new COLOR_INFO(guid,name,model,icon,coverage,4);
//					string sColors = "";
//					string sColorCodes = ",";
//					if (paletteCount > 0)
//					{
//						sColors = "A";
//						sColorCodes = ","+Color[0].ToString();
//						NewColor.SetColor(0,Color[0]);
//						for (i=1; i < paletteCount; i++)
//						{
//							sColorCodes += ","+Color[i].ToString();
//							if (sColors.Length < 4)
//							{
//								if (Color[i] != Color[i-1])
//								{
//									NewColor.SetColor(sColors.Length,Color[i]);
//									sColors += (char)('A' + i);
//								}
//							}
//						}
//					}
//					if (m_bDebug)
//					{
//						ModelTable[model] = new AC_MODEL(iModel,sColors,name);
//						StreamWriter sw = new StreamWriter(m_sAssemblyPath+"\\ModelTrap.csv",true);
//						sw.WriteLine(model.ToString()+",\""+name+"\",#"+coverage.ToString("X8")+sColorCodes);
//						sw.Flush();
//						sw.Close();
//						m_Hooks.AddChatText(model.ToString()+",\""+name+"\",#"+coverage.ToString("X8")+","+sColors+sColorCodes,7,1);
//					}
//				/*}*/
//				
//				if (m_nKnownItems > MAX_ITEMS)
//				{
//					CheckKnown();
//				}
//				else if (m_nKnownItems > MAX_ITEMS)
//				{
//					m_Hooks.AddChatText("DCS: ****ERROR*** Inventory Overflow",10,1);
//					return;
//				}
//				int ins = m_nKnownItems;
//				while (ins>0)
//				{
//					ins--;
//					if (KnownItemArray[ins].coverage > coverage) {ins++; break;}
//					if (KnownItemArray[ins].coverage == coverage)
//					{
//						if( KnownItemArray[ins].model < model) {ins++; break;}
//						if( KnownItemArray[ins].model == model)
//						{
//							if( KnownItemArray[ins].icon < icon) {ins++; break;}
//							if( KnownItemArray[ins].icon == icon)
//							{
//								if( KnownItemArray[ins].name.CompareTo(name)<=0) {ins++; break;}
//							}
//						}
//					}
//				}
//				int iPos;
//				for (iPos=m_nKnownItems-1;iPos>=ins;iPos--)
//				{
//					KnownItemArray[iPos+1] = KnownItemArray[iPos];
//				}
//				//				m_Hooks.AddChatText(name,4);
//				//				for (iPos--;iPos>=0;iPos--)
//				//				{
//				//					m_Hooks.AddChatText(KnownItemArray[iPos].name,7);
//				//				}
//
//				KnownItemArray[ins] = NewColor;
//				m_nKnownItems++;    
//				if (cbAll.Checked)
//				{
//					m_Hooks.IDQueueAdd(guid);
//				}
//			}
//			catch (Exception ex)
//			{
//				/*if (m_bDebug)*/ m_Hooks.AddChatText("DCS: Error on " + test + "--" + ex.Message + "( object is "+o.GetType().ToString()+")",7,1);
//			}
//		}
			
		private void ItemTrackerActions_ObjectChanged(object sender, ChangeObjectEventArgs e)
		{
			try
			{	
				if(e.Change != WorldChangeType.IdentReceived) {return;}
						
				if(e.Changed.Id == Host.Actions.CurrentSelection)
        		{
        			ManualCheckItemForMatches(new LootObject(e.Changed));
        			return;
        		}  		
				else if(LOList.Any(x => x.Id == e.Changed.Id && x.Listen))
        		{
					LootObject lo = LOList.Find(x => x.Id == e.Changed.Id);
					lo.Listen = false;
        			CheckItemForMatches(lo.Id);
        			return;
        		}							
			}catch(Exception ex){LogError(ex);}
		}
		
		
		private void ItemTracker_ItemDestroyed(object sender, ItemDestroyedEventArgs e)
		{
			try
			{
				if(LOList.Any(x => x.Id == e.ItemGuid))
				{
					LOList.RemoveAll(x => x.Id == e.ItemGuid || x.Container == e.ItemGuid);
					UpdateItemHud();
				}
								
				if(WaitingVTIOs.Any(x => x.Id == e.ItemGuid))
				{
					WaitingVTIOs.RemoveAll(x => x.Id == e.ItemGuid);
					UpdateItemHud();
				}
				
				return;
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ItemTracker_ObjectReleased(object sender, ReleaseObjectEventArgs e)
		{
			try
			{
				if(LOList.Any(x => x.Id == e.Released.Id))
				{
					LOList.RemoveAll(x => x.Id == e.Released.Id || x.Container == e.Released.Id);
					UpdateItemHud();
					return;
				}
				
				if(WaitingVTIOs.Any(x => x.Id == e.Released.Id))
				{
					WaitingVTIOs.RemoveAll(x => x.Id == e.Released.Id);
					UpdateItemHud();
				}
			}catch(Exception ex){LogError(ex);}
		}

		
		private int LastLootContainer = 0;
		private void LootContainerOpened(object sender, ContainerOpenedEventArgs e)
		{
			try
			{					
				if(Core.WorldFilter[e.ItemGuid] == null){return;}
				
				if(Core.WorldFilter[e.ItemGuid].Values(LongValueKey.Burden) == 0) {return;}
				
				if(Core.WorldFilter.GetByContainer(e.ItemGuid).Count == 0)
				{
					WriteToChat("Loot Tapback hit");
					LastLootContainer = e.ItemGuid;
					Core.RenderFrame += LootContainerDelay;	
				}
				
				WriteToChat("Passed into Loot Container");
				LootByContainer(e.ItemGuid);												
			}
			catch(Exception ex){LogError(ex);}
		}
		
		private void LootContainerDelay(object sender, EventArgs e)
		{
			try
			{
				if(Core.WorldFilter.GetByContainer(LastLootContainer).Count() != 0)
				{
					WriteToChat("Loot Container Tapback cleared.");
					Core.RenderFrame -= LootContainerDelay;	
					
					LootByContainer(LastLootContainer);
					LastLootContainer = 0;
				}	
			}catch(Exception ex){LogError(ex);}
		}
		
		private void LootByContainer(int ContainerId)
		{
			try
			{
				LootObject container = new LootObject(Core.WorldFilter[ContainerId]);
				
				if(container.Name.Contains(Core.CharacterFilter.Name)){return;}
				//TODO:  Protect permitted corpses.

				if(container.Name.Contains("Chest") || container.Name.Contains("Vault") || 
				   container.Name.Contains("Reliquary") || container.ObjectClass == ObjectClass.Corpse)
				{
					foreach(WorldObject wo in Core.WorldFilter.GetByContainer(container.Id))
					{
						if(!LOList.Any(x => x.Id == wo.Id))
						{
							LootObject lo = new LootObject(wo);
							LOList.Add(lo);
							SeparateItemsToID(lo.Id);
						}
					}	
				}
				
			}catch(Exception ex){LogError(ex);}
		}
			
		private void SeparateItemsToID(int loId)
		{
				try
				{
					
					LootObject IOItem = LOList.Find(x => x.Id == loId);
					if(IOItem == null) {return;}
					
					
					//Flag items that need additional info to ID...
					if(!IOItem.HasIdData)
					{
						//This should remove items which require identifications to match and queue them for listening for IDs.  All other items should pass through default.
						switch(IOItem.ObjectClass)
						{
							case ObjectClass.Armor:
							case ObjectClass.Clothing:
							case ObjectClass.Jewelry:
								if(IOItem.LValue(LongValueKey.IconOutline) > 0)
								{
									IdqueueAdd(IOItem.Id);
									IOItem.Listen = true;
									return;	
								}
								break;	
							case ObjectClass.Gem:
								IdqueueAdd(IOItem.Id);
								IOItem.Listen = true;
								return;	
							case ObjectClass.Scroll:
							case ObjectClass.MeleeWeapon:
							case ObjectClass.MissileWeapon:
							case ObjectClass.WandStaffOrb:								
								IdqueueAdd(IOItem.Id);
								IOItem.Listen = true;
								return;	
							case ObjectClass.Misc:
								if(IOItem.Name.Contains("Essence"))
								{
									IdqueueAdd(IOItem.Id);
									IOItem.Listen = true;
									return;
								}
								break;								
						}
						if(!IOItem.Listen){CheckItemForMatches(IOItem.Id);}
					}
				} catch (Exception ex) {LogError(ex);} 
				return;
			}
		

		
		private void EvaluateItemMatches(int loId)
		{
			try
			{
				LootObject IOItem = LOList.Find(x => x.Id == loId);
				//Keep those duplicates out
				if(IOItem.Exclude) {return;}
				
				switch(IOItem.IOR)
				{
					case IOResult.trophy:
						if(IOItem.Name.ToLower() == "pyreal mote" ||  IOItem.Name.ToLower() == "pyreal sliver" || IOItem.Name.ToLower() == "pyreal nugget")
						{
							IOItem.InspectList = true;
							IOItem.FoundryProcess = FoundryActionTypes.MoteCombine;
						}
						break;
					case IOResult.rare:
					case IOResult.rule:
						if(IOItem.Aetheriacheck) {IOItem.FoundryProcess = FoundryActionTypes.Reveal;}
						IOItem.InspectList = true;
						break;
					case IOResult.salvage:
						IOItem.InspectList = true;
						IOItem.FoundryProcess = FoundryActionTypes.Salvage;
						break;
					case IOResult.dessicate:
						IOItem.InspectList = true;
						IOItem.FoundryProcess = FoundryActionTypes.Desiccate;
						break;
					case IOResult.manatank:
						IOItem.InspectList = true;
						IOItem.FoundryProcess = FoundryActionTypes.ManaStone;
						break;
					case IOResult.spell:
						IOItem.InspectList = true;
						IOItem.FoundryProcess = FoundryActionTypes.Read;
						break;
					case IOResult.val:
						IOItem.InspectList = true;
						if(GISettings.SalvageHighValue) 
						{
							IOItem.FoundryProcess = FoundryActionTypes.Salvage;
						}
						break;
				}
				
				if(IOItem.ObjectClass == ObjectClass.Key)
				{
					IOItem.FoundryProcess = FoundryActionTypes.Ring;
				}
				if(IOItem.Name.ToLower() == "pyreal mote" || IOItem.Name.ToLower() == "pyreal sliver" || IOItem.Name.ToLower() == "pyreal nugget")
				{
					IOItem.FoundryProcess = FoundryActionTypes.MoteCombine;
				}
				
				if(GISettings.AutoLoot)
				{
					FoundryLoadAction(FoundryActionTypes.MoveToPack, IOItem.Id);
					InitiateFoundryActions();
				}
				
				IOItem.Exclude = true;
				UpdateItemHud();
				return;
			}catch(Exception ex){LogError(ex);}
		}		
		
		//Virindi Tank Looting..............................................................................................	
		private List<LootObject> WaitingVTIOs = new List<LootObject>();
		public int VTLinkDecision(int id, int reserved1, int reserved2)
		{
			try
			{	
				//If VT shoots in a corpse ID, check if it has a rare on it.  If so, Loot, if not, skip.				
				if(reserved2 == 1)
				{
					if(CorpseTrackingList.Any(x => x.IOR == IOResult.corpsewithrare) && reserved1 == CorpseTrackingList.Find(x => x.IOR == IOResult.corpsewithrare).Id)
					{
						return 1;
					}
					else
					{
						return 0;
					}
				}
				
				try
				{
					if(LOList.Any(x => x.Id == id && x.InspectList))
					{
						switch(LOList.Find(x => x.Id == id).IOR)
						{
							case IOResult.rule:
							case IOResult.manatank:
							case IOResult.rare:
							case IOResult.spell:
							case IOResult.trophy:	
							case IOResult.dessicate:
								return 1;						
							case IOResult.salvage:
								return 2;
							case IOResult.val:
								if(GISettings.SalvageHighValue) {return 2;}
								else{return 1;}
							default:
								return 0;
						}
					}
				}catch(Exception ex){LogError(ex);}
				
				LootObject VTIO = new LootObject(Core.WorldFilter[id]);	
				
				if(!VTIO.HasIdData)
				{
					Core.RenderFrame += DoesVTIOHaveID;
					WaitingVTIOs.Add(VTIO);
					SendVTIOtoCallBack(VTIO);
					
				}
				
				CheckRulesItem(ref VTIO);
				
				if(VTIO.IOR == IOResult.unknown) {CheckRare(ref VTIO);}
				if(VTIO.ObjectClass == ObjectClass.Scroll){CheckUnknownScrolls(ref VTIO);}
				if(VTIO.IOR == IOResult.unknown) {TrophyListCheckItem(ref VTIO);}
				if(VTIO.IOR == IOResult.unknown && GISettings.IdentifySalvage) {CheckSalvageItem(ref VTIO);}
				if(VTIO.IOR == IOResult.unknown) {CheckManaItem(ref VTIO);}
				if(VTIO.IOR == IOResult.unknown) {CheckValueItem(ref VTIO);}
				if(VTIO.IOR == IOResult.unknown) {VTIO.IOR = IOResult.nomatch;}
				
													
				switch(VTIO.IOR)
				{
					case IOResult.rule:
					case IOResult.manatank:
					case IOResult.rare:
					case IOResult.spell:
					case IOResult.trophy:
					case IOResult.dessicate:
						return 1;						
					case IOResult.salvage:
						return 2;
					case IOResult.val:
						return 1;
					default:
						return 0;
				}						
			}catch(Exception ex){LogError(ex); return 0;}
		}
		
		private void SendVTIOtoCallBack(LootObject VTIO)
		{	
			try
			{
				if(WaitingVTIOs.Count == 0) 
				{
					Core.RenderFrame -= DoesVTIOHaveID;
					return;
				}
			}catch(Exception ex){LogError(ex);}
		}
		
		private void DoesVTIOHaveID(object sender, EventArgs e)
		{
			try
			{
				if(WaitingVTIOs.Any(x => x.HasIdData == true)){WaitingVTIOs.RemoveAll(x => x.HasIdData == true);}
			}catch(Exception ex){LogError(ex);}
		}
		
		public bool VTSalvageCombineDecision(double work1, double work2, int material)
		{
			try
			{
				var salvagerules =  from allrules in SalvageRulesList
					where (allrules.material == material) && 
					(allrules.minwork <= work1) && 
					(allrules.minwork <= work2) &&                       
					(allrules.maxwork >= work1) && 
					(allrules.maxwork >= work2)
				    select allrules;
				                           
				if(salvagerules.Count() > 0) {return true;}
				else {return false;}
			}catch(Exception ex){LogError(ex); return  false;}
		}
		
	}
}
