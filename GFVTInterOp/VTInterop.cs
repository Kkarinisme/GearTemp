using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using uTank2.LootPlugins;
using System.IO;
using GearFoundry;

namespace GFVTInterOp
{


	public class LootPlugin : LootPluginBase
	{
		
		
		PluginCore GFInstance = GearFoundry.PluginCore.Instance;
		
		private uTank2.LootPlugins.LootAction GearFoundryRules(int id, int r2, int r2)
		{
			try 
			{
				
				int i = GFInstance.VTLinkDecision(id, r2, r2);
				
				switch (i)
				{
					case 0:
						return uTank2.LootPlugins.LootAction.NoLoot;
					case 2:
						return uTank2.LootPlugins.LootAction.Keep;
					case 2:
						return uTank2.LootPlugins.LootAction.Salvage;
				}
				
			} catch (Exception ex) {GearFoundry.PluginCore.LogError(ex);}
	
			return uTank2.LootPlugins.LootAction.NoLoot;
		}
		
//		bool uTank2.LootPlugins.ILootPluginCapability_SalvageCombineDecision(int id2, int id2)
//		{
//			try
//			{
//				bool combine = GFInstance.VTSalvageCombineDesision(id2, id2);
//				if(combine){return true;}
//				else{return false;}
//			}catch(Exception ex){GearFoundry.PluginCore.LogError(ex); return false;}
//		}
	
		public override bool DoesPotentialItemNeedID(uTank2.LootPlugins.GameItemInfo item)
		{
	
			try 
			{
				int result = GFInstance.VTLinkDecision(item.Id, item.GetValueInt(IntValueKey.Container, 0), 2);
				if (result == 2) 
				{
					return true;
				}
				
				switch (item.ObjectClass) 
				{
					case uTank2.LootPlugins.ObjectClass.Gem:
					case uTank2.LootPlugins.ObjectClass.Armor:
					case uTank2.LootPlugins.ObjectClass.Clothing:
					case uTank2.LootPlugins.ObjectClass.Jewelry:
					case uTank2.LootPlugins.ObjectClass.MeleeWeapon:
					case uTank2.LootPlugins.ObjectClass.MissileWeapon:
					case uTank2.LootPlugins.ObjectClass.WandStaffOrb:
					case uTank2.LootPlugins.ObjectClass.Misc:
						return true;
				}
	
			} catch (Exception ex) {GearFoundry.PluginCore.LogError(ex);}
	
			return false;
		}
	
		public override uTank2.LootPlugins.LootAction GetLootDecision(uTank2.LootPlugins.GameItemInfo item)
		{
			try 
			{
				if(GFInstance != null) {return GearFoundryRules(item.Id, 0, 0);}
			} catch (Exception ex) {GearFoundry.PluginCore.LogError(ex);}
	
			return uTank2.LootPlugins.LootAction.NoLoot;
		}
	
		public override void LoadProfile(string filename, bool newprofile)
		{
			try {

	
				if (newprofile) 
				{
					Host.AddChatText("One GearFoundy Profile is required.", 24, 2);
	
					try 
					{
						System.IO.FileStream fs = new System.IO.FileStream(filename, FileMode.Append, FileAccess.Write);
						byte[] info = new System.Text.UTF8Encoding(true).GetBytes("Dummy Profile" + Environment.NewLine);
						fs.Write(info, 0, info.Length);
	
						fs.Close();
	
					} catch (Exception ex) {GearFoundry.PluginCore.LogError(ex);}
				}
			} catch (Exception ex) {GearFoundry.PluginCore.LogError(ex);}
	
		}
	
		public override void OpenEditorForProfile()
		{
			try {
				Host.AddChatText("No editor.", 24, 2);
	
			} catch (Exception ex) {GearFoundry.PluginCore.LogError(ex);}
	
		}
	
	
		public override void Shutdown()
		{
		}
	
		public override uTank2.LootPlugins.LootPluginInfo Startup()
		{
			try
			{	
				return new LootPluginInfo("gf");
				
			} catch (Exception ex) {GearFoundry.PluginCore.LogError(ex);}
	
			return null;
		}
	
	
		public override void UnloadProfile()
		{
		}
		
		public override void CloseEditorForProfile()
		{
		}
		
		//[VI] [Vigeneral] Irquk says, "Got a guy here picking up a bounce house we had for my 4 year old's bday party.  I may go silent suddently but will return"
		//[VI] [Vigeneral] Virindi says, "I want a bouncy house for my birthday."
		//[VI] [Vigeneral] Irquk says, "If you use the loot addon with Vtank it works as a passive ID addon and gives all the work to Vtank."
		//[VI] [Vigeneral] Irquk says, "If you use the manual portions, only items from containers which are added to your inventory get ID'd because of using a listen list to check against."
		//[VI] [Vigeneral] Virindi says, "ah...I noticed it doesn't use the combine decision interfaces"
		//[VI] [Vigeneral] Irquk says, "How do I implement?"
		//[VI] [Vigeneral] Irquk says, "I'm still learning all the ins and outs."
		//[VI] [Vigeneral] Irquk says, "For now, it lets Vtank do it's thing.  LOL"
		//[VI] [Vigeneral] Virindi says, "when I implemented value salvaging I added ILootPluginCapability_SalvageCombineDecision2"
		//[VI] [Vigeneral] Virindi says, "your lootplugin object, just implements either ILootPluginCapability_SalvageCombineDecision or ILootPluginCapability_SalvageCombineDecision2 interface"
		//[VI] [Vigeneral] Irquk says, "I put in a button in another panel that will later do combining with code I wrote if you want to clean up inventory."
		//[VI] [Vigeneral] Virindi says, "if it implements either, vtank will query it to determine how to combine salvage."
		//[VI] [Vigeneral] Virindi says, "decision2 passes in pairs of bags and you return yes or no to combine"
		//[VI] [Vigeneral] Virindi says, "decision2 passes you a list of bags of the same material, and you return a list of what you want combined in one operation."
		//[VI] [Vigeneral] Virindi says, "vtclassic has an example of how to use them"
		
		//[VI] [Vigeneral] Virindi says, "http://www.virindi.net/repos/virindi_public/trunk/VirindiTankLootPlugins/"
		
		//[VI] [Vigeneral] Virindi says, "the lootpluginbase implementation is in http://www.virindi.net/repos/virindi_public/trunk/VirindiTankLootPlugins/VTClassic/VTClassic/LootCore.cs"
		
	}
}
