
using System;
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
using VirindiHUDs;
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;
using System.Xml.Serialization;
using System.Xml;

namespace GearFoundry
{
	/// <summary>
	/// Description of GearValet.
	/// </summary>
	public partial class PluginCore
	{
		private HudFixedLayout ValetTabLayout = null;
		private HudButton ValetDisrobe = null;
		private HudButton ValetEquipSuit = null;		
		private HudButton ValetCreateSuit = null;
		private HudList ValetSuitList = null;
		private HudList ValetSuitPiecesList = null;
		private HudStaticText ValetTextBoxLabel = null;
		private HudStaticText ValetSuitListLabel = null;
		private HudStaticText ValetSuitPiecesListLabel = null;
		
//		private HudButton ValetEquipSuit3 = null;
//		private HudButton ValetEquipSuit0 = null;
//		private HudButton ValetSuit1 = null;
//		private HudButton ValetSuit2 = null;
//		private HudButton ValetSuit3 = null;
//		private HudButton ValetSuit0 = null;
//		private HudButton ValetClearSuit1 = null;
//		private HudButton ValetClearSuit2 = null;
//		private HudButton ValetClearSuit3 = null;
//		private HudButton ValetClearSuit0 = null;
//		private HudList ValetSuit1List = null;
//		private HudList ValetSuit2List= null;
//		private HudList ValetSuit3List = null;
//		private HudList ValetSuit0List = null;
		private HudList.HudListRowAccessor ValetRow = null;
		private HudTextBox ValetNameBox = null;
		
		public class ValetTicket
		{
			public int ItemId = 0;
			public int SlotId = 0;
		}
		
		private void RenderButlerHudValetTab()
		{
			try
			{   
				int splithorizontal = Convert.ToInt32((double)GearButlerSettings.ButlerHudWidth /(double)3);
				
				ValetDisrobe = new HudButton();
				ValetDisrobe.Text = "Disrobe";
				ValetTabLayout.AddControl(ValetDisrobe, new Rectangle(10,5,splithorizontal-20,20));
				
				ValetEquipSuit = new HudButton();
				ValetEquipSuit.Text = "Equip";
				ValetTabLayout.AddControl(ValetEquipSuit, new Rectangle(100,5,splithorizontal-20,20));
				
				ValetCreateSuit = new HudButton();
				ValetCreateSuit.Text = "Create";
				ValetTabLayout.AddControl(ValetCreateSuit, new Rectangle(200,5,splithorizontal-20,20));
				
				ValetTextBoxLabel = new HudStaticText();
				ValetTextBoxLabel.FontHeight = 8;
				ValetTextBoxLabel.Text = "Suit Label:";
				ValetTabLayout.AddControl(ValetTextBoxLabel, new Rectangle(0,30,50,10));
				
				ValetNameBox = new HudTextBox();
				ValetNameBox.Text = String.Empty;
				ValetTabLayout.AddControl(ValetNameBox, new Rectangle(10,40,GearButlerSettings.ButlerHudWidth -20, 20));
				
			
				
				
//						
//		private HudStaticText ValetSuitListLabel = null;
//		private HudStaticText ValetSuitPiecesListLabel = null;
				

				
				ValetSuitList = new HudList();
				ValetSuitList.AddColumn(typeof(HudCheckBox), 16, null);
				ValetSuitList.AddColumn(typeof(HudStaticText), GearButlerSettings.ButlerHudWidth - 60, null);
				ValetSuitList.AddColumn(typeof(HudPictureBox), 16, null);
				ValetSuitList.AddColumn(typeof(HudStaticText), 1, null);
				ValetTabLayout.AddControl(ValetSuitList, new Rectangle(0,60,GearButlerSettings.ButlerHudWidth - 20,100));
				
				ValetSuitPiecesList = new HudList();
				ValetSuitPiecesList.AddColumn(typeof(HudPictureBox), 16, null);
				ValetSuitPiecesList.AddColumn(typeof(HudStaticText), GearButlerSettings.ButlerHudWidth - 60, null);
				ValetSuitPiecesList.AddColumn(typeof(HudPictureBox), 16, null);
				ValetSuitPiecesList.AddColumn(typeof(HudStaticText), 1, null);
				ValetTabLayout.AddControl(ValetSuitPiecesList, new Rectangle(0,GearButlerSettings.ButlerHudHeight - 180,GearButlerSettings.ButlerHudWidth - 20,100));
				                        
//				ValetSuit1List = new HudList();
//				ValetSuit1List.AddColumn(typeof(HudPictureBox), 16, null);
//				ValetSuit1List.AddColumn(typeof(HudStaticText),200,null);
//				ValetTabLayout.AddControl(ValetSuit1List, new Rectangle(0,150,300,80));
				
				
//				ValetEquipSuit3 = new HudButton();
//				ValetEquipSuit3.Text = "Jump Suit";
//				ValetTabLayout.AddControl(ValetEquipSuit3, new Rectangle(10,70,130,20));
//				
//				ValetEquipSuit0 = new HudButton();
//				ValetEquipSuit0.Text = "Zoot Suit";
//				ValetTabLayout.AddControl(ValetEquipSuit0, new Rectangle(160,70,130,20));
//							
//				ValetSuit1 = new HudButton();
//				ValetSuit1.Text = "Set Business Suit";
//				ValetTabLayout.AddControl(ValetSuit1, new Rectangle(10,120,130,20));
//				
//				ValetClearSuit1 = new HudButton();
//				ValetClearSuit1.Text = "Clear Business Suit";
//				ValetTabLayout.AddControl(ValetClearSuit1, new Rectangle(160,120,130,20));
//				
//				ValetSuit1List = new HudList();
//				ValetSuit1List.AddColumn(typeof(HudPictureBox), 16, null);
//				ValetSuit1List.AddColumn(typeof(HudStaticText),200,null);
//				ValetTabLayout.AddControl(ValetSuit1List, new Rectangle(0,150,300,80));
//				
//				ValetSuit2 = new HudButton();
//				ValetSuit2.Text = "Set Leisure Suit";
//				ValetTabLayout.AddControl(ValetSuit2, new Rectangle(10,240,130,20));
//				
//				ValetClearSuit2 = new HudButton();
//				ValetClearSuit2.Text = "Clear Leisure Suit";
//				ValetTabLayout.AddControl(ValetClearSuit2, new Rectangle(160,240,130,20));
//				
//				ValetSuit2List = new HudList();
//				ValetSuit2List.AddColumn(typeof(HudPictureBox), 16, null);
//				ValetSuit2List.AddColumn(typeof(HudStaticText),200,null);
//				ValetTabLayout.AddControl(ValetSuit2List, new Rectangle(0,270,300,80));
//				
//				ValetSuit3 = new HudButton();
//				ValetSuit3.Text = "Set Jump Suit";
//				ValetTabLayout.AddControl(ValetSuit3, new Rectangle(10,360,130,20));
//				
//				ValetClearSuit3 = new HudButton();
//				ValetClearSuit3.Text = "Clear Jump Suit";
//				ValetTabLayout.AddControl(ValetClearSuit3, new Rectangle(160,360,130,20));
//				
//				ValetSuit3List = new HudList();
//				ValetSuit3List.AddColumn(typeof(HudPictureBox), 16, null);
//				ValetSuit3List.AddColumn(typeof(HudStaticText),200,null);
//				ValetTabLayout.AddControl(ValetSuit3List, new Rectangle(0,390,300,80));
//				
//				ValetSuit0 = new HudButton();
//				ValetSuit0.Text = "Set Zoot Suit";
//				ValetTabLayout.AddControl(ValetSuit0, new Rectangle(10,480,130,20));
//				
//				ValetClearSuit0 = new HudButton();
//				ValetClearSuit0.Text = "Clear Zoot Suit";
//				ValetTabLayout.AddControl(ValetClearSuit0, new Rectangle(160,480,130,20));
//				
//				ValetSuit0List = new HudList();
//				ValetSuit0List.AddColumn(typeof(HudPictureBox), 16, null);
//				ValetSuit0List.AddColumn(typeof(HudStaticText),200,null);
//				ValetTabLayout.AddControl(ValetSuit0List, new Rectangle(0,510,300,80));
				
				
				ValetDisrobe.Hit += ValetDisrobe_Hit;
				ValetEquipSuit.Hit += ValetEquipSuit_Hit;
				ValetCreateSuit.Hit += ValetCreateSuit_Hit;
				
//				ValetEquipSuit2.Hit += ValetEquipSuit2_Hit;
//				ValetEquipSuit3.Hit += ValetEquipSuit3_Hit;
//				ValetEquipSuit0.Hit += ValetEquipSuit0_Hit;
//				
//				ValetSuit1.Hit += ValetSuit1_Hit;
//				ValetSuit2.Hit += ValetSuit2_Hit;
//				ValetSuit3.Hit += ValetSuit3_Hit;
//				ValetSuit0.Hit += ValetSuit0_Hit;
//				
//				ValetClearSuit1.Hit += ValetClearSuit1_Hit;
//				ValetClearSuit2.Hit += ValetClearSuit2_Hit;
//				ValetClearSuit3.Hit += ValetClearSuit3_Hit;
//				ValetClearSuit0.Hit += ValetClearSuit0_Hit;
				
				ValetTab = true;
				
				UpdateValetHud();
				
				
			}catch(Exception ex){LogError(ex);}
		}
			
		private void DisposeValetTabLayout()
		{
			try
			{
				if(!ValetTab) { return;}
				
				ValetDisrobe.Hit -= ValetDisrobe_Hit;
				ValetEquipSuit.Hit -= ValetEquipSuit_Hit;
				ValetCreateSuit.Hit -= ValetCreateSuit_Hit;
				
//				ValetSuit1.Hit -= ValetSuit1_Hit;
//				ValetSuit2.Hit -= ValetSuit2_Hit;
//				ValetSuit3.Hit -= ValetSuit3_Hit;
//				ValetSuit0.Hit -= ValetSuit0_Hit;
//				
//				ValetClearSuit1.Hit -= ValetClearSuit1_Hit;
//				ValetClearSuit2.Hit -= ValetClearSuit2_Hit;
//				ValetClearSuit3.Hit -= ValetClearSuit3_Hit;
//				ValetClearSuit0.Hit -= ValetClearSuit0_Hit;
//				
				
//				ValetDisrobe.Dispose();
//				ValetEquipSuit.Dispose();
//				ValetCreateSuit.Dispose();
//				
//				ValetSuit1.Dispose();
//				ValetClearSuit1.Dispose();	
//				ValetSuit1List.Dispose();
//				ValetSuit1List = null;
//				
//				ValetSuit2.Dispose();
//				ValetClearSuit2.Dispose();
//				ValetSuit2List.Dispose();
//				ValetSuit2List = null;
//				
//				ValetSuit3.Dispose();
//				ValetClearSuit3.Dispose();
//				ValetSuit3List.Dispose();
//				ValetSuit3List = null;
//				
//				ValetSuit0.Dispose();
//				ValetClearSuit0.Dispose();
//				ValetSuit0List.Dispose();
//				ValetSuit0List = null;
//				
//				ValetEquipSuit1.Dispose();
//				ValetEquipSuit2.Dispose();
//				ValetEquipSuit3.Dispose();
//				ValetEquipSuit0.Dispose();
				
				ValetTab = false;
				
			}catch(Exception ex){LogError(ex);}
		}
				
		private void UpdateValetHud()
		{
			try
			{
//				ValetSuit0List.ClearRows();
//				ValetSuit1List.ClearRows();
//				ValetSuit2List.ClearRows();
//				ValetSuit3List.ClearRows();
//				
//				foreach(ValetTicket vt in GearButlerSettings.SuitList1)
//				{
//					ValetRow = ValetSuit1List.AddRow();
//					((HudPictureBox)ValetRow[0]).Image = Core.WorldFilter[vt.ItemId].Icon;
//                    ((HudStaticText)ValetRow[1]).FontHeight = nitemFontHeight;
//                    ((HudStaticText)ValetRow[1]).Text = Core.WorldFilter[vt.ItemId].Name;
//                }
//				
//				foreach(ValetTicket vt in GearButlerSettings.SuitList2)
//				{
//					ValetRow = ValetSuit2List.AddRow();
//					((HudPictureBox)ValetRow[0]).Image = Core.WorldFilter[vt.ItemId].Icon;
//                    ((HudStaticText)ValetRow[1]).FontHeight = nitemFontHeight;
//                    ((HudStaticText)ValetRow[1]).Text = Core.WorldFilter[vt.ItemId].Name;
//                }
//				
//				foreach(ValetTicket vt in GearButlerSettings.SuitList3)
//				{
//					ValetRow = ValetSuit3List.AddRow();
//					((HudPictureBox)ValetRow[0]).Image = Core.WorldFilter[vt.ItemId].Icon;
//                    ((HudStaticText)ValetRow[1]).FontHeight = nitemFontHeight;
//                    ((HudStaticText)ValetRow[1]).Text = Core.WorldFilter[vt.ItemId].Name;
//				}
//				
//				foreach(ValetTicket vt in GearButlerSettings.SuitList0)
//				{
//					ValetRow = ValetSuit0List.AddRow();
//					((HudPictureBox)ValetRow[0]).Image = Core.WorldFilter[vt.ItemId].Icon;
//					((HudStaticText)ValetRow[1]).Text = Core.WorldFilter[vt.ItemId].Name;
//                    ((HudStaticText)ValetRow[1]).FontHeight = nitemFontHeight;
//                }
				
			}catch(Exception ex){LogError(ex);}
		}
		
				
		private void ValetDisrobe_Hit(object sender, System.EventArgs e)
		{
			try
			{
				ValetRemoveList = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();	
				UpdateValetHud();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ValetEquipSuit_Hit(object sender, System.EventArgs e)
		{
			try
			{
//				GearButlerSettings.SuitList1.Clear();
//				
//				List<WorldObject> vs = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();	
//				foreach(WorldObject wo in vs)
//				{
//					ValetTicket vt = new ValetTicket();
//					vt.ItemId = wo.Id;
//					vt.SlotId = wo.Values(LongValueKey.EquippedSlots);
//					GearButlerSettings.SuitList1.Add(vt);
//				}
//				GearButlerReadWriteSettings(false);
//				UpdateValetHud();
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ValetCreateSuit_Hit(object sender, System.EventArgs e)
		{
			try
			{
//				GearButlerSettings.SuitList2.Clear();
//				
//				List<WorldObject> vs = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();	
//				foreach(WorldObject wo in vs)
//				{
//					ValetTicket vt = new ValetTicket();
//					vt.ItemId = wo.Id;
//					vt.SlotId = wo.Values(LongValueKey.EquippedSlots);
//					GearButlerSettings.SuitList2.Add(vt);
//				}
//				GearButlerReadWriteSettings(false);
//				UpdateValetHud();
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ValetSuit3_Hit(object sender, System.EventArgs e)
		{
			try
			{
				GearButlerSettings.SuitList3.Clear();
				
				List<WorldObject> vs = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();	
				foreach(WorldObject wo in vs)
				{
					ValetTicket vt = new ValetTicket();
					vt.ItemId = wo.Id;
					vt.SlotId = wo.Values(LongValueKey.EquippedSlots);
					GearButlerSettings.SuitList3.Add(vt);
				}
				GearButlerReadWriteSettings(false);
				UpdateValetHud();
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ValetSuit0_Hit(object sender, System.EventArgs e)
		{
			try
			{
				GearButlerSettings.SuitList0.Clear();
				
				List<WorldObject> vs = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();	
				foreach(WorldObject wo in vs)
				{
					ValetTicket vt = new ValetTicket();
					vt.ItemId = wo.Id;
					vt.SlotId = wo.Values(LongValueKey.EquippedSlots);
					GearButlerSettings.SuitList0.Add(vt);
				}
				GearButlerReadWriteSettings(false);
				UpdateValetHud();
				
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ValetClearSuit1_Hit(object sender, System.EventArgs e)
		{
			try
			{
				GearButlerSettings.SuitList1.Clear();
				GearButlerReadWriteSettings(false);
				UpdateValetHud();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ValetClearSuit2_Hit(object sender, System.EventArgs e)
		{
			try
			{
				GearButlerSettings.SuitList2.Clear();
				GearButlerReadWriteSettings(false);
				UpdateValetHud();
			}catch(Exception ex){LogError(ex);}
		}
				
		private void ValetClearSuit3_Hit(object sender, System.EventArgs e)
		{
			try
			{
				GearButlerSettings.SuitList3.Clear();
				GearButlerReadWriteSettings(false);
				UpdateValetHud();
			}catch(Exception ex){LogError(ex);}
		}
						
		private void ValetClearSuit0_Hit(object sender, System.EventArgs e)
		{
			try
			{
				GearButlerSettings.SuitList0.Clear();
				GearButlerReadWriteSettings(false);
				UpdateValetHud();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ValetEquipSuit1_Hit(object sender, System.EventArgs e)
		{
			try
			{
				ValetRemoveList = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();
				ValetEquipList = GearButlerSettings.SuitList1.ToList();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ValetEquipSuit2_Hit(object sender, System.EventArgs e)
		{
			try
			{
				ValetRemoveList = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();
				ValetEquipList = GearButlerSettings.SuitList2.ToList();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ValetEquipSuit3_Hit(object sender, System.EventArgs e)
		{
			try
			{
				ValetRemoveList = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();
				ValetEquipList = GearButlerSettings.SuitList3.ToList();
			}catch(Exception ex){LogError(ex);}
		}
		
		private void ValetEquipSuit0_Hit(object sender, System.EventArgs e)
		{
			try
			{
				ValetRemoveList = Core.WorldFilter.GetInventory().Where(x => x.Values(LongValueKey.EquippedSlots) !=  0).OrderBy(x => x.Name).ToList();
				ValetEquipList = GearButlerSettings.SuitList0.ToList();
			}catch(Exception ex){LogError(ex);}
		}
			
			private void ValetProcessRemove()
			{
				try
				{
					if(ValetRemoveList.Count > 0)
					{
						if(ValetRemoveList.First().Values(LongValueKey.EquippedSlots) == 0)
						{
							ValetRemoveList.RemoveAt(0);
						}
						Core.Actions.MoveItem(ValetRemoveList.First().Id, Core.CharacterFilter.Id,0, false);
						return;
					}
						
				}catch(Exception ex){LogError(ex);}
			}
			
			private void ValetProcessEquip()
			{
				try
				{
					if(ValetEquipList.Count > 0)
					{	
						if(Core.WorldFilter[ValetEquipList.First().ItemId].Values(LongValueKey.EquippedSlots) > 0)
						{
								ValetEquipList.RemoveAt(0);
						}
						if(ValetEquipList.First().SlotId == 0x200000 && Core.WorldFilter[ValetEquipList.First().ItemId].ObjectClass == ObjectClass.MeleeWeapon)
						{
							Core.Actions.AutoWield(ValetEquipList.First().ItemId, 1, 0, 1, 0, 0);
						}
						else
						{
							Core.Actions.UseItem(ValetEquipList.First().ItemId,0);
						}
						return;
					}
				}catch(Exception ex){LogError(ex);}
			}
		
	}
}
