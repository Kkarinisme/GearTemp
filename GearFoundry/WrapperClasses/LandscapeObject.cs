using System;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using System.IO;
using System.Collections.ObjectModel;
using System.Text;
using Decal.Filters;
using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GearFoundry
{

	public partial class PluginCore
	{	
	
				
		public class LandscapeObject
		{
			private WorldObject wo;
		
			public LandscapeObject(WorldObject obj)
			{
				wo = obj;
			}
		
			public LandscapeObject()
			{
			}
		
			//wrappers: the overloads of function Values in wrappers.worldobject makes me type too much -Losado
			//wrappers below allow direct access to all world object properties not expressly
			//included in the IdentifiedObject (IO) data class.  Many properties are expressly defined and loaded.  -Irquk
			
			public int LValue(Decal.Adapter.Wrappers.LongValueKey eval) {
				return wo.Values(eval); 
			}
			public double DValue(Decal.Adapter.Wrappers.DoubleValueKey eval) {
				return wo.Values(eval); 
			}
			public string SValue(Decal.Adapter.Wrappers.StringValueKey eval) {
				return wo.Values(eval);
			}
			public bool BValue(Decal.Adapter.Wrappers.BoolValueKey eval) {
				return wo.Values(eval); 
			}
			
			internal IOResult IOR = IOResult.unknown;
			public bool addtoloot;
			public bool notify;
			public string rulename;
			public double DistanceAway;							
			
			public string IORString()
			{
				switch(IOR)
				{
					case IOResult.portal:
						return "(Portal) ";
					case IOResult.players:
						return "(Player) ";
					case IOResult.lifestone:
						return "(Lifestone) ";
					case IOResult.trophy:
						return "(Trophy) ";
					case IOResult.rare:
						return "(Rrare) ";
					case IOResult.spell:
						return "(Spell) ";
					case IOResult.rule:
						return "(" + rulename + ") ";
					case IOResult.monster:
						return "(Mob) ";
					case IOResult.corpseselfkill:
						return "(Corpse) ";
					case IOResult.corpsepermitted:
						return "(Corpse:Permitted) ";
					case IOResult.corpsewithrare:
						return "(Corpse:Rare) ";
					case IOResult.corpseofself:
						return "(Corpse:Self) ";
					case IOResult.corpsefellowkill:
						return "(Corpse:Fellow) ";
					case IOResult.val:
						return "(Value) ";
					case IOResult.manatank:
						return "(Manatank) ";
					case IOResult.allegplayers:
						return "(Allegence) ";
					case IOResult.npc:
						return "(NPC) ";
					case IOResult.salvage:
						return "(" + rulename + ") ";
					default:
						return String.Empty;
				}  
			}
			
			public string DistanceString()
			{
				return " <" + (DistanceAway * 100).ToString("N0") + ">";
			}
				
			public int SpellCount 
			{
				get { return wo.SpellCount; }
			}
			public int Id 
			{
				get { return wo.Id; }
			}
			public bool HasIdData 
			{
				get { return wo.HasIdData; }
			}
			public int Icon 
			{
				get { return wo.Icon; }
			}	
			public string Name
			{
				get { return wo.Name; }
			}
			public Decal.Adapter.Wrappers.ObjectClass ObjectClass 
			{
				get { return wo.ObjectClass; }
			}
			public int Container 
			{
				get { return wo.Container; }
			}
		
			public bool isvalid
			{
				get
				{
					return host.Underlying.Hooks.IsValidObject(wo.Id);
				}
			}
			

	

			public string CoordsStringLink(string inputcoords)
			{
				return " (" + "<Tell:IIDString:" + GOARROWLINK_ID + ":" + inputcoords + ">" + inputcoords + "<\\Tell>" + ")";
			}
			
			public string LinkString()
			{
				//builds result string with appropriate goodies to report
				string result = string.Empty;
				try {
					if (wo != null) 
					{
						switch(wo.ObjectClass)
						{

							case ObjectClass.Player:
								result = IORString() + wo.Name;
								break;
							case ObjectClass.Portal:
								result = IORString() + wo.Values(StringValueKey.PortalDestination) + CoordsStringLink(wo.Coordinates().ToString());
								break;
							default:
								result = IORString() + wo.Name + CoordsStringLink(wo.Coordinates().ToString());
								break;
						}
						
					}
				} catch (Exception ex) {
					LogError(ex);
				}
				return result;
			}
			
			public string HudString()
			{
				try
				{
					if(wo.Name.Contains("Corpse of"))
					{
						return wo.Name.Replace("Corpse of", "") + DistanceString();
					}
					else
					{
						return wo.Name + DistanceString();
					}
//					if(splitstring.Count() == 1)
//					{
//						return wo.Name + DistanceString();
//					}
//					else
//					{
//						string returnstring = String.Empty;
//						foreach(string piece in splitstring)
//						{
//							if(piece != "Corpse" || piece != "of")
//							{
//								returnstring += piece[0];
//							}
//						}
//						return returnstring + DistanceString();
//					}
				}catch(Exception ex){LogError(ex); return String.Empty;}
			}
		}
	}
}


		
		

