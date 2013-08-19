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
using MyClasses.MetaViewWrappers.VirindiViewServiceHudControls;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace GearFoundry
{

	public partial class PluginCore
	{
		private List<FellowMember> FellowMemberList = new List<FellowMember>();
		
		public class FellowMember
		{
			public int Id = 0;
			public string Name = String.Empty;
			public bool Looting = false;	
			public int MaxHealth = 0;
			public int CurrentHealth = 0;
			public int MaxStamina = 0;
			public int CurrentStamina = 0;
			public int MaxMana = 0;
			public int CurrentMana = 0;
		}
		
		private void SubscribeFellowshipEvents()
		{
			try
			{
				Core.EchoFilter.ServerDispatch += ServerDispatchFellow;	
			}catch(Exception ex){LogError(ex);}
		}
		
		private void UnSubscribeFellowshipEvents()
		{
			try
			{
				Core.EchoFilter.ServerDispatch -= ServerDispatchFellow;	
			}catch(Exception ex){LogError(ex);}
		}
		private void ServerDispatchFellow(object sender, NetworkMessageEventArgs e)
		{
		int iEvent = 0;
            try
            {
            	if(e.Message.Type == AC_GAME_EVENT)
            	{
            		try
                    {
            			iEvent = (int)e.Message["event"];
                    }
                    catch{}
                    if(iEvent == GE_ADD_FELLOWMEMBER)
                    {
                    	AddFellowLandscape(e);
                    }
                    if(iEvent == GE_FELLOWSHIP_MEMBER_QUIT || iEvent == GE_FELLOWSHIP_MEMBER_DISMISSED)
                    {
                    	RemoveFellowLandscape(e);
                    }
                    if(iEvent == GE_DISBAND_FELLOWSHIP)
                    {
                    	ClearFellowLandscape(e);
                    }
                    if(iEvent == GE_CREATE_FELLOWSHIP)
                    {
                    	CreateorJoinFellowLandscape(e);
                    }                    
            	}
            }
            catch (Exception ex){LogError(ex);}
		}
		
		private void AddFellowLandscape(NetworkMessageEventArgs e)
	    {
	    	try
	    	{
	    		int fellow = (int)e.Message.Struct("fellow")["fellow"];
	    		if(fellow != Core.CharacterFilter.Id)
	    		{
	    			if(!FellowMemberList.Any(x => x.Id == fellow))
	    			{
	    				FellowMember fella = new FellowMember();
	    				fella.Id = fellow;
	    				fella.Name = (string)e.Message.Struct("fellow")["name"];
	    				FellowMemberList.Add(fella);
	    			}
	    		}    
	    	}catch(Exception ex) {LogError(ex);}
	    }
	    
	   	private void RemoveFellowLandscape(NetworkMessageEventArgs e)
	    {
	    	try
	    	{
	    		int fellow = (int)e.Message.Value<int>("fellow");
	    		
	    		if(fellow == Core.CharacterFilter.Id)
	    		{
	    			FellowMemberList.Clear();
	    		}
	    		else
	    		{
	    			FellowMemberList.RemoveAll(x => x.Id == fellow);
	    		}	    
	    	} catch(Exception ex){LogError(ex);}
	    }
	   	
	   	private void ClearFellowLandscape(NetworkMessageEventArgs e)
	    {
	    	try
	    	{
	   			FellowMemberList.Clear();	    
	    	} catch(Exception ex) {LogError(ex);}
	    }
	   	
	   	private void CreateorJoinFellowLandscape(NetworkMessageEventArgs e)
	   	{
	   		try
	   		{
	   			FellowMemberList.Clear();
	   			var fellowmembers = e.Message.Struct("fellows");
	   			int fellowcount = (int)e.Message.Value<int>("fellowCount");
	   			for(int i = 0; i < fellowcount; i++)
	   			{
	   				var fellow = fellowmembers.Struct(i).Struct("fellow");
	   				if((string)fellow.Value<string>("name") != Core.CharacterFilter.Name)
	   				{
	   					FellowMember fella = new FellowMember();
	   					fella.Name = (string)fellow.Value<string>("name");
	   					fella.Id = (int)fellow.Value<int>("fellow");
	   					if((int)fellow.Value<int>("shareLoot") == 0x10) {fella.Looting = true;}
	   					FellowMemberList.Add(fella);
	   				}
	   			}	   			
	   		}catch(Exception ex) {LogError(ex);}
	   	}
	   	
	   	
	   	
	   	
	   	private void DebugFellow()
	   	{
	   		try
	   		{
	   			foreach(var item in FellowMemberList)
	   			{
	   				WriteToChat("Name " + item.Name + ", Id " + item.Id);
	   			}
	   		}catch{}
	   	}
	   	
//private Decal.Interop.Filters.EchoFilter mNetEcho;
//
//protected override void Startup() {
//   mNetEcho = (Decal.Interop.Filters.EchoFilter)Host.Decal.GetObject(@"services\DecalNet.NetService\DecalFilters.EchoFilter");
//   mNetEcho.EchoMessage += new Decal.Interop.Filters.IEchoSink_EchoMessageEventHandler(NetEcho_EchoMessage);
//}
//
//// Keep track of who's in your fellowship
//private void NetEcho_EchoMessage(Decal.Interop.Net.IMessage pMsg) {
//   switch (pMsg.Type) {
//      case 0xF7B0: // Game Event
//         Decal.Interop.Net.IMessageMember fellowMembers, fellow;
//
//         switch ((int)pMsg.get_Value("event")) {
//            case 0x02BE: // Create Or Join Fellowship
//               mFellowship.Clear();
//               fellowMembers = pMsg.get_Struct("fellows");
//               int fellowCount = (int)pMsg.get_Value("fellowCount");
//               for (int i = 0; i < fellowCount; i++) {
//                  fellow = fellowMembers.get_Struct(i).get_Struct("fellow");
//                  mFellowship[(int)fellow.get_Value("fellow")] = (string)fellow.get_Value("name");
//               }
//               break;
//
//            case 0x02C0: // Add Fellowship Member
//               fellow = pMsg.get_Struct("fellow");
//               mFellowship[(int)fellow.get_Value("fellow")] = (string)fellow.get_Value("name");
//               break;
//
//            case 0x00A4: // Fellowship Member Quit Or Dismissed
//               guid = (int)pMsg.get_Value("fellow");
//               if (guid == Core.CharacterFilter.Id) {
//                  mFellowship.Clear();
//               }
//               else {
//                  mFellowship.Remove(guid);
//               }
//               break;
//
//            case 0x02BF: // Disband Fellowship
//               mFellowship.Clear();
//               break;
//         }
//         break;
//   }
//}
	    
		
		
		
	}
}
