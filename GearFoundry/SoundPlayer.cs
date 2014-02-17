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
using System.Text.RegularExpressions;
using System.Text;
using System.Media;

namespace GearFoundry
{
	public partial class PluginCore
	{
		//gearmisc variables		
        
        private List<Sounds> SoundList = new List<Sounds>();
        public SoundSettings mSoundsSettings = new SoundSettings();
        
        public class SoundSettings
        {
        	public bool MuteSounds = false;
        	
        	public int LandscapeTrophies = 0;
        	public int LandscapeMobs = 0;
        	public int LandscapePlayers = 0;
        	
        	public int CorpseRare = 0;
        	public int CorpseSelfKill = 0;
        	public int CorpseFellowKill = 0;
        	public int DeadMe = 0;
        	public int DeadPermitted = 0;
        	
        	public int CorpseTrophy = 0;
        	public int CorpseRule = 0;
        	public int CorpseSalvage = 0;
        }
        
		private void SoundsReadWriteSettings(bool read)
		{
			try
			{
				FileInfo SoundSettingsFile = new FileInfo(GearDir + @"\Sounds.xml");
								
				if (read)
				{
					try
					{
						if (!SoundSettingsFile.Exists)
		                {
		                    try
		                    {
		                    	mSoundsSettings = new SoundSettings();
		                    	
		                    	using (XmlWriter writer = XmlWriter.Create(SoundSettingsFile.ToString()))
								{
						   			XmlSerializer serializer2 = new XmlSerializer(typeof(SoundSettings));
						   			serializer2.Serialize(writer, mSoundsSettings);
						   			writer.Close();
								}
		                    }
		                    catch (Exception ex) { LogError(ex); }
		                }
						
						using (XmlReader reader = XmlReader.Create(SoundSettingsFile.ToString()))
						{	
							XmlSerializer serializer = new XmlSerializer(typeof(SoundSettings));
							mSoundsSettings = (SoundSettings)serializer.Deserialize(reader);
							reader.Close();
						}
					}
					catch
					{
						mSoundsSettings = new SoundSettings();
					}
				}
				
				if(!read)
				{
					if(SoundSettingsFile.Exists)
					{
						SoundSettingsFile.Delete();
					}
					
					using (XmlWriter writer = XmlWriter.Create(SoundSettingsFile.ToString()))
					{
			   			XmlSerializer serializer2 = new XmlSerializer(typeof(SoundSettings));
			   			serializer2.Serialize(writer, mSoundsSettings);
			   			writer.Close();
					}
				}
			}catch(Exception ex){LogError(ex);}
		}
        
        public class Sounds
        {
        	public string name = string.Empty;
        	public System.IO.Stream SoundStream;
        	public int SoundId = 0;
        }
        
        private void InitSoundsFunctions()
        {
        	
        	System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
        		
        	Sounds tsound;
        	tsound = new Sounds();
        	tsound.name = "None";
        	tsound.SoundId = 0;
        	SoundList.Add(tsound);
        	
        	tsound = new Sounds();
        	tsound.name = "Blip";
        	tsound.SoundId = 1;
        	tsound.SoundStream = a.GetManifestResourceStream("blip.wav");
        	SoundList.Add(tsound);
        	
        	tsound = new Sounds();
        	tsound.name = "Cork";
        	tsound.SoundId = 2;
        	tsound.SoundStream = a.GetManifestResourceStream("cork.wav");
        	SoundList.Add(tsound);
        	
        	tsound = new Sounds();
        	tsound.name = "Click 1";
        	tsound.SoundId = 3;
        	tsound.SoundStream = a.GetManifestResourceStream("click1.wav");      	
        	SoundList.Add(tsound);
        		
        	tsound = new Sounds();
        	tsound.name = "Click 2";
        	tsound.SoundId = 4;
        	tsound.SoundStream = a.GetManifestResourceStream("click2.wav");
        	SoundList.Add(tsound);
        		
        	tsound = new Sounds();
        	tsound.name = "Oop";
        	tsound.SoundId = 5;
        	tsound.SoundStream = a.GetManifestResourceStream("oop.wav");
        	SoundList.Add(tsound);
        		
        	tsound = new Sounds();
        	tsound.name = "Pluck";
        	tsound.SoundId = 6;
        	tsound.SoundStream = a.GetManifestResourceStream("pluck.wav");
        	SoundList.Add(tsound);
        		
        	tsound = new Sounds();
        	tsound.name = "Splooge";
        	tsound.SoundId = 7;
        	tsound.SoundStream = a.GetManifestResourceStream("splooge.wav");
        	SoundList.Add(tsound);
        	
        	tsound = new Sounds();
        	tsound.name = "Till";
        	tsound.SoundId = 8;
        	tsound.SoundStream = a.GetManifestResourceStream("till.wav");
        	SoundList.Add(tsound);
        		
        	tsound = new Sounds();
        	tsound.name = "Womp 1";
        	tsound.SoundId = 9;
        	tsound.SoundStream = a.GetManifestResourceStream("womp.wav");
        	SoundList.Add(tsound);
        		
        	tsound = new Sounds();
        	tsound.name = "Womp 2";
        	tsound.SoundId = 10;
        	tsound.SoundStream = a.GetManifestResourceStream("womp2.wav");
        	SoundList.Add(tsound);

            foreach (Sounds s in SoundList)
            {
                cboTrophyLandscape.AddItem(s.name,0);
                cboMobLandscape.AddItem(s.name, 0);
                cboPlayerLandscape.AddItem(s.name, 0);
                cboCorpseRare.AddItem(s.name, 0);
                cboCorpseSelfKill.AddItem(s.name, 0);
                cboCorpseFellowKill.AddItem(s.name, 0);
                cboDeadMe.AddItem(s.name, 0);
                cboDeadPermitted.AddItem(s.name, 0);
                cboTrophyCorpse.AddItem(s.name, 0);
                cboRuleCorpse.AddItem(s.name, 0);
                cboSalvageCorpse.AddItem(s.name, 0);
            }

            SoundsReadWriteSettings(true);
            UpdateSoundPanel();
        }

        private void UpdateSoundPanel()
        {
            try
            {
                chkMuteSounds.Checked = mSoundsSettings.MuteSounds;
                cboTrophyLandscape.Current = mSoundsSettings.LandscapeTrophies;
                cboMobLandscape.Current = mSoundsSettings.LandscapeMobs;
                cboPlayerLandscape.Current = mSoundsSettings.LandscapePlayers;
                cboCorpseRare.Current = mSoundsSettings.CorpseRare;
                cboCorpseSelfKill.Current = mSoundsSettings.CorpseSelfKill;
                cboCorpseFellowKill.Current = mSoundsSettings.CorpseFellowKill;
                cboDeadMe.Current = mSoundsSettings.DeadMe;
                cboDeadPermitted.Current = mSoundsSettings.DeadPermitted;
                cboTrophyCorpse.Current = mSoundsSettings.CorpseTrophy;
                cboRuleCorpse.Current = mSoundsSettings.CorpseRule;
                cboSalvageCorpse.Current = mSoundsSettings.CorpseSalvage;
            }
            catch (Exception ex) { LogError(ex); }
        }
        
        private void playSoundFromResource(int SoundFileId)
		{
        	try
        	{
	            if (mSoundsSettings.MuteSounds || SoundFileId == 0) { return; }
	            else
	            {
	            	//WriteToChat("Sound ID = " + SoundFileId.ToString());
	            	SoundPlayer player = new SoundPlayer(SoundList.Find(x => x.SoundId == SoundFileId).SoundStream);
	                player.Play();
	                player.Dispose();
	                SoundList.Find(x => x.SoundId == SoundFileId).SoundStream.Position = 0;  
	            }
        	}catch(Exception ex){LogError(ex);}
		}
        
        //Sound Settings
        void chkMuteSounds_Change()
        {
            try
            {
                mSoundsSettings.MuteSounds = chkMuteSounds.Checked;
                SoundsReadWriteSettings(false);
                UpdateSoundPanel();
            }
            catch (Exception ex) { LogError(ex); }
        }
        void cboTrophyLandscape_Change(object sender, EventArgs e)
        {
            try
            {
                mSoundsSettings.LandscapeTrophies = cboTrophyLandscape.Current;
                SoundsReadWriteSettings(false);
                UpdateSoundPanel();
                playSoundFromResource(mSoundsSettings.LandscapeTrophies);
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboMobLandscape_Change(object sender, EventArgs e)
        {
            try
            {
                mSoundsSettings.LandscapeMobs = cboMobLandscape.Current;
                SoundsReadWriteSettings(false);
                UpdateSoundPanel();
                playSoundFromResource(mSoundsSettings.LandscapeMobs);
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboPlayerLandscape_Change(object sender, EventArgs e)
        {
            try
            {
                mSoundsSettings.LandscapePlayers = cboPlayerLandscape.Current;
                SoundsReadWriteSettings(false);
                UpdateSoundPanel();
                playSoundFromResource(mSoundsSettings.LandscapePlayers);
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboCorpseRare_Change(object sender, EventArgs e)
        {
            try
            {
                mSoundsSettings.CorpseRare = cboCorpseRare.Current;
                SoundsReadWriteSettings(false);
                UpdateSoundPanel();
                playSoundFromResource(mSoundsSettings.CorpseRare);
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboCorpseSelfKill_Change(object sender, EventArgs e)
        {
            try
            {
                mSoundsSettings.CorpseSelfKill = cboCorpseSelfKill.Current;
                SoundsReadWriteSettings(false);
                UpdateSoundPanel();
                playSoundFromResource(mSoundsSettings.CorpseSelfKill);
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboCorpseFellowKill_Change(object sender, EventArgs e)
        {
            try
            {
                mSoundsSettings.CorpseFellowKill = cboCorpseFellowKill.Current;
                SoundsReadWriteSettings(false);
                UpdateSoundPanel();
                playSoundFromResource(mSoundsSettings.CorpseFellowKill);
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboDeadMe_Change(object sender, EventArgs e)
        {
            try
            {
                mSoundsSettings.DeadMe = cboDeadMe.Current;
                SoundsReadWriteSettings(false);
                UpdateSoundPanel();
                playSoundFromResource(mSoundsSettings.DeadMe);
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboDeadPermitted_Change(object sender, EventArgs e)
        {
            try
            {
                mSoundsSettings.DeadPermitted = cboDeadPermitted.Current;
                SoundsReadWriteSettings(false);
                UpdateSoundPanel();
                playSoundFromResource(mSoundsSettings.DeadPermitted);
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboTrophyCorpse_Change(object sender, EventArgs e)
        {
            try
            {
                mSoundsSettings.CorpseTrophy = cboTrophyCorpse.Current;
                SoundsReadWriteSettings(false);
                UpdateSoundPanel();
                playSoundFromResource(mSoundsSettings.CorpseTrophy);
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboRuleCorpse_Change(object sender, EventArgs e)
        {
            try
            {
                mSoundsSettings.CorpseRule = cboRuleCorpse.Current;
                SoundsReadWriteSettings(false);
                UpdateSoundPanel();
                playSoundFromResource(mSoundsSettings.CorpseRule);
            }
            catch (Exception ex) { LogError(ex); }
        }

        void cboSalvageCorpse_Change(object sender, EventArgs e)
        {
            try
            {
                mSoundsSettings.CorpseSalvage = cboSalvageCorpse.Current;
                SoundsReadWriteSettings(false);
                UpdateSoundPanel();
                playSoundFromResource(mSoundsSettings.CorpseSalvage);
            }
            catch (Exception ex) { LogError(ex); }
        }



    }
}
