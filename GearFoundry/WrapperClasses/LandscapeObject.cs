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
						return "(Rare) ";
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
			
			public string MiniIORString()
			{
				switch(IOR)
				{
					case IOResult.portal:
						return "(P) ";
					case IOResult.players:
						return "(Pl) ";
					case IOResult.lifestone:
						return "(L) ";
					case IOResult.trophy:
						return "(T) ";
					case IOResult.rare:
						return "(R) ";
					case IOResult.monster:
						return "(M) ";
					case IOResult.corpseselfkill:
						return "(C) ";
					case IOResult.corpsepermitted:
						return "(CP) ";
					case IOResult.corpsewithrare:
						return "(CR) ";
					case IOResult.corpseofself:
						return "(CS) ";
					case IOResult.corpsefellowkill:
						return "(CF) ";
					case IOResult.allegplayers:
						return "(A) ";
					case IOResult.npc:
						return "(N) ";
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
					if(wo != null) {return true;}
					else{return false;}
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
			
			public string TruncateName()
			{
				try
				{
					if(wo.Name.Length > 8)
					{
						string ReturnString = wo.Name.Replace("Corpse of ", "");
						if(ReturnString.Contains("of "))
						{
							ReturnString = ReturnString.Replace("of ","");
						}
						if(ReturnString.Length > 8)
						{
							if(ReturnString.Contains("a"))
							{
								ReturnString = ReturnString.Replace("a", "");
							}
							if(ReturnString.Contains("e"))
							{
								ReturnString = ReturnString.Replace("e", "");
							}
							if(ReturnString.Contains("i"))
							{
								ReturnString = ReturnString.Replace("i", "");
							}
							if(ReturnString.Contains("o"))
							{
								ReturnString = ReturnString.Replace("o", "");
							}
							if(ReturnString.Contains("u"))
							{
								ReturnString = ReturnString.Replace("u", "");
							}
							if(ReturnString.Length > 8)
							{
								if(ReturnString.Contains(" "))
								{
									
									string[] splitstring = ReturnString.Split(' ');
									ReturnString = String.Empty;
									foreach(string piece in splitstring)
									{
										if(piece.Length > 2)
										{
											ReturnString += piece.Substring(0,2);
										}
										else
										{
											ReturnString += piece;
										}
									}
									return ReturnString;
								}
								else
								{
									return ReturnString.Substring(0,10) + ".";
								}
							}
							else
							{
								return ReturnString;
							}	
						}
						else
						{
							return ReturnString;
						}
					}
					else
					{
						return wo.Name;
					}
				}catch(Exception ex){LogError(ex); return String.Empty;}
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
				}catch(Exception ex){LogError(ex); return String.Empty;}
			}
			
			public string MiniHudString()
			{
				try
				{
					return TruncateName() + DistanceString();		
				}catch(Exception ex){LogError(ex); return String.Empty;}
			}
		}
	}
}


		
		

