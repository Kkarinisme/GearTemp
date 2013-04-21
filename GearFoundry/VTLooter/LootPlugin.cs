using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using uTank2.LootPlugins;
using System.IO;

public class LootPlugin : LootPluginBase
{

	private Alinco.Plugin mAlincoBase;

	private bool mAlincoLoaded;
	private void TryGetAlincoInstance()
	{
		mAlincoLoaded = false;

		try {
			mAlincoBase = Alinco.Plugin.Instance;

			if (mAlincoBase != null) {
				mAlincoLoaded = true;
			}

		} catch (Exception ex) {
			//LogToFile("log.txt", ex.Message)
		}
	}

	private uTank2.LootPlugins.LootAction AlincoRules(int id, int r1, int r2)
	{
		try {
			//      LogToFile("log.txt", "AlincoRules " & Hex(id))
			int i = mAlincoBase.GetLootDecision(id, r1, r2);
			//       LogToFile("log.txt", name & " AlincoRules=>GetLootDecision " & Hex(id) & " returned " & i)
			switch (i) {
				case 0:
					return uTank2.LootPlugins.LootAction.NoLoot;
				case 1:
					return uTank2.LootPlugins.LootAction.Keep;
				case 2:
					return uTank2.LootPlugins.LootAction.Salvage;
			}
		} catch (Exception ex) {
			//        LogToFile("log.txt", "AlincoRules")
		}
		return uTank2.LootPlugins.LootAction.NoLoot;
	}



	public override void CloseEditorForProfile()
	{
	}

	public override bool DoesPotentialItemNeedID(uTank2.LootPlugins.GameItemInfo item)
	{

		try {
			// HACK check item.container = corpswithrareid
			// TODO add a public function DoesPotentialItemNeedID to alinco
			int result = mAlincoBase.GetLootDecision(item.Id, item.GetValueInt(IntValueKey.Container, 0), 1);
			if (result == 1) {
				return true;
			}

			switch (item.ObjectClass) {
				case ObjectClass.MissileWeapon:
					int t = item.GetValueInt(IntValueKey.MissileType, 0);
					if (t == 1 || t == 2 || t == 4) {
						return true;
					}
					break;
				case uTank2.LootPlugins.ObjectClass.Gem:
				case uTank2.LootPlugins.ObjectClass.Armor:
				case uTank2.LootPlugins.ObjectClass.Clothing:
				case uTank2.LootPlugins.ObjectClass.Jewelry:
				case uTank2.LootPlugins.ObjectClass.MeleeWeapon:
				case uTank2.LootPlugins.ObjectClass.MissileWeapon:
				case uTank2.LootPlugins.ObjectClass.WandStaffOrb:
					return true;
			}

		} catch (Exception ex) {
		}

		return false;
	}

	public override uTank2.LootPlugins.LootAction GetLootDecision(uTank2.LootPlugins.GameItemInfo item)
	{


		try {
			if (mAlincoLoaded) {
				// LogToFile("log.txt", "GetLootDecision1")

				return AlincoRules(item.Id, 0, 0);


			}
		} catch (Exception ex) {
			// LogToFile("log.txt", "Error GetLootDecision")
		}

		return uTank2.LootPlugins.LootAction.NoLoot;
	}

	public override void LoadProfile(string filename, bool newprofile)
	{
		// LogToFile("log.txt", "LoadProfile " & vbNewLine & filename)
		try {
			if (!mAlincoLoaded) {
				Host.AddChatText("Error: Alinco plugin not loaded.", 14, 1);
				return;
			}

			if (newprofile) {
				Host.AddChatText("Profiles not implemented: Only need one dummy profile for Alinco", 14, 1);

				try {
					System.IO.FileStream fs = new System.IO.FileStream(filename, FileMode.Append, FileAccess.Write);
					byte[] info = new System.Text.UTF8Encoding(true).GetBytes("Dummy Profile" + Environment.NewLine);
					fs.Write(info, 0, info.Length);

					fs.Close();

				} catch (Exception exo) {
					// empty catch
				}
			}
		} catch (Exception ex) {
			//LogToFile("log.txt", ex.Message)
		}

	}

	public override void OpenEditorForProfile()
	{
		try {
			Host.AddChatText("No editor.", 14, 1);

		} catch (Exception ex) {
		}

	}


	public override void Shutdown()
	{
	}

	public override uTank2.LootPlugins.LootPluginInfo Startup()
	{
		try {
			mAlincoLoaded = false;
			TryGetAlincoInstance();


		} catch (Exception ex) {
		}

		if (mAlincoLoaded) {
			return new LootPluginInfo("los");
		}

		return null;
	}


	public override void UnloadProfile()
	{
	}
}
