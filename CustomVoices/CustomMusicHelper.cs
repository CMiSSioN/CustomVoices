using BattleTech;
using Harmony;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomVoices {
  [HarmonyPatch(typeof(AudioEventManager))]
  [HarmonyPatch("SetMusicState")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AudioState_Music_State), typeof(AudioSwitch_Mission_Status), typeof(AudioState_Player_State), typeof(bool) })]
  public static class AudioEventManager_SetMusicState {
    public static bool Prefix(AudioState_Music_State newMusicState, AudioSwitch_Mission_Status newMissionStatus, AudioState_Player_State newPlayerState, bool stopOldMusic, ref bool __result) {
      Log.M?.TWL(0, $"AudioEventManager.SetMusicState {newMusicState} {newMissionStatus} {newPlayerState} stopOldMusic:{stopOldMusic} custMusicIsPlayed:{CustomMusicHelper.isMusicPlayed}");
      try {
        if(CustomMusicHelper.hasMusicFiles(newMusicState, newMissionStatus, newPlayerState) == false) {
          if (CustomMusicHelper.isMusicPlayed) { CustomMusicHelper.StopMusic(); }; return true;
        }
        string customSample = CustomMusicHelper.getMusicFile(newMusicState, newMissionStatus, newPlayerState, out bool needtochange);
        Log.M?.WL(1,$"change:{needtochange} sample:{customSample}");
        if (string.IsNullOrEmpty(customSample)) { if (CustomMusicHelper.isMusicPlayed) { CustomMusicHelper.StopMusic(); }; return true; }
        if (needtochange || stopOldMusic) {
          if (CustomMusicHelper.isMusicPlayed) { CustomMusicHelper.StopMusic(); };
        }
        AudioEventManager.StopMusic();
        AudioEventManager.musicCurrentMusicState = newMusicState;
        AudioEventManager.musicCurrentMissionStatus = newMissionStatus;
        AudioEventManager.musicCurrentPlayerState = newPlayerState;
        AudioEventManager.musicLastPlayedID = 0;
        CustomMusicHelper.musicCurrentMusicState = newMusicState;
        CustomMusicHelper.musicCurrentMissionStatus = newMissionStatus;
        CustomMusicHelper.musicCurrentPlayerState = newPlayerState;
        if (CustomMusicHelper.isMusicPlayed == false) {
          CustomMusicHelper.Play(customSample);
        }
        __result = true;
        return false;
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
      return true;
    }
  }
  //[HarmonyPatch(typeof(AudioEventManager))]
  //[HarmonyPatch("StopMusic")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] {  })]
  //public static class AudioEventManager_StopMusic {
  //  public static void Postfix() {
  //    try {
  //      CustomMusicHelper.StopMusic();
  //    } catch (Exception e) {
  //      Log.M?.TWL(0, e.ToString(), true);
  //    }
  //  }
  //}
  public static class CustomMusicHelper {
    public static bool isMusicPlayed { get { return currentMusic != null; } }
    public static void StopMusic() {
      if (currentMusic != null) {
        Log.M?.TWL(0, "CustomMusicHelper.StopMusic " + currentMusic.parent.name,true);
        Log.M?.WL(0, Environment.StackTrace);
        currentMusic.Stop();
        currentMusic = null;
      }
    }
    public static AudioState_Music_State musicCurrentMusicState = AudioState_Music_State.None;
    public static AudioSwitch_Mission_Status musicCurrentMissionStatus = AudioSwitch_Mission_Status.ambient;
    public static AudioState_Player_State musicCurrentPlayerState = AudioState_Player_State.None;
    private static HashSet<string> lastMusicList = new HashSet<string>();
    public static void MusicSampleIsOver() {
      if (isMusicPlayed == false) { return; }
      Log.M?.TWL(0, "CustomMusicHelper.MusicSampleIsOver event", true);
      string customSample = CustomMusicHelper.getMusicFile(musicCurrentMusicState, musicCurrentMissionStatus, musicCurrentPlayerState, out bool needtochange);
      if (string.IsNullOrEmpty(customSample)) { AudioEventManager.SetMusicState(musicCurrentMusicState, musicCurrentMissionStatus, musicCurrentPlayerState, true); return; }
      Play(customSample);
    }
    private static AudioEngine.AudioChannel currentMusic { get; set; } = null;
    public static void Play(string path) {
      Log.M?.TWL(0, "CustomMusicHelper.Play "+path, true);
      currentMusic = AudioEngine.Instance.MusicBus.PlayFile(path, false, new List<AudioEngine.AudioChannelEvent>() { new AudioEngine.AudioChannelEvent(0.0f, MusicSampleIsOver) });
      lastPlayed = path;
    }
    public static bool isExists(AudioState_Music_State newMusicState, AudioSwitch_Mission_Status newMissionStatus, AudioState_Player_State newPlayerState) {
      return musicFiles[newMusicState][newMissionStatus][newPlayerState].Count > 0;
    }
    private static string lastPlayed { get; set; } = string.Empty;
    public static bool hasMusicFiles(AudioState_Music_State newMusicState, AudioSwitch_Mission_Status newMissionStatus, AudioState_Player_State newPlayerState) {
      return musicFiles[newMusicState][newMissionStatus][newPlayerState].Count > 0;
    }
    public static string getMusicFile(AudioState_Music_State newMusicState, AudioSwitch_Mission_Status newMissionStatus, AudioState_Player_State newPlayerState, out bool needtochange) {
      needtochange = false;
      if (musicFiles[newMusicState][newMissionStatus][newPlayerState].Count == 0) { return string.Empty; }
      needtochange = (lastMusicList.SetEquals(musicFiles[newMusicState][newMissionStatus][newPlayerState]) == false);
      lastMusicList = musicFiles[newMusicState][newMissionStatus][newPlayerState];
      if (musicFiles[newMusicState][newMissionStatus][newPlayerState].Count == 1) { return musicFiles[newMusicState][newMissionStatus][newPlayerState].First(); }
      string result = string.Empty;
      int watchdog = 0;
      List<string> list = musicFiles[newMusicState][newMissionStatus][newPlayerState].ToList();
      do {
        result = list[UnityEngine.Random.Range(0, list.Count)];
        ++watchdog;
        if (watchdog > 10) { break; }
      } while (result == lastPlayed);
      return result;
    }
    //public class MusicFiles {
    //  public Dictionary<AudioState_Music_State, Dictionary<AudioSwitch_Mission_Status, Dictionary<AudioState_Player_State, List<string>>>> musicFiles = new Dictionary<AudioState_Music_State, Dictionary<AudioSwitch_Mission_Status, Dictionary<AudioState_Player_State, List<string>>>>();
    //  public Dictionary<AudioState_Music_State, Dictionary<AudioSwitch_Mission_Status, Dictionary<AudioState_Player_State, AudioState_Player_State>>> linkage3 = new Dictionary<AudioState_Music_State, Dictionary<AudioSwitch_Mission_Status, Dictionary<AudioState_Player_State, AudioState_Player_State>>>();
    //}
    private static Dictionary<AudioState_Music_State, Dictionary<AudioSwitch_Mission_Status, Dictionary<AudioState_Player_State, HashSet<string>>>> musicFiles = new Dictionary<AudioState_Music_State, Dictionary<AudioSwitch_Mission_Status, Dictionary<AudioState_Player_State, HashSet<string>>>>();
    public static string SearchForBaseDir(string modDir) {
      string cur_dir = modDir;
      while (string.IsNullOrEmpty(cur_dir) == false) {
        string cache_dir = Path.Combine(cur_dir, "Mods");
        if (Directory.Exists(cache_dir)) { return cur_dir; }
        cur_dir = Path.GetDirectoryName(cur_dir);
      }
      return string.Empty;
    }
    public static string SearchForModTek(string modDir) {
      string cur_dir = modDir;
      while (string.IsNullOrEmpty(cur_dir) == false) {
        string cache_dir = Path.Combine(cur_dir, ".modtek");
        if (Directory.Exists(cache_dir)) { return cache_dir; }
        cur_dir = Path.GetDirectoryName(cur_dir);
      }
      return string.Empty;
    }
    public static void Init(string modDir) {
      string musicDir = SearchForBaseDir(modDir);
      if (string.IsNullOrEmpty(musicDir)) {
        Log.M?.TWL(0,"Can't find game base directory");
        return;
      }
      musicDir = Path.Combine(musicDir, "Music");
      if (Directory.Exists(musicDir) == false) { Directory.CreateDirectory(musicDir); }
      musicFiles.Clear();
      Log.M?.TWL(0, "CustomMusicHelper.Init start");
      string modtekDir = SearchForModTek(modDir);
      string cacheFile = Path.Combine(string.IsNullOrEmpty(modtekDir)? musicDir: modtekDir, "music_cache.json");
      if (File.Exists(cacheFile)) {
        musicFiles = JsonConvert.DeserializeObject<Dictionary<AudioState_Music_State, Dictionary<AudioSwitch_Mission_Status, Dictionary<AudioState_Player_State, HashSet<string>>>>>(File.ReadAllText(cacheFile));
        Log.M?.TWL(0, "CustomMusicHelper.Init finish");
        return;
      }
      List<AudioState_Music_State> musicStates = Enum.GetValues(typeof(AudioState_Music_State)).Cast<AudioState_Music_State>().ToList();
      List<AudioSwitch_Mission_Status> missionStatuses = Enum.GetValues(typeof(AudioSwitch_Mission_Status)).Cast<AudioSwitch_Mission_Status>().ToList();
      List<AudioState_Player_State> playerStates = Enum.GetValues(typeof(AudioState_Player_State)).Cast<AudioState_Player_State>().ToList();
      foreach (AudioState_Music_State musicState in musicStates) {
        string musicStateDir = Path.Combine(musicDir, "MusicState_"+ musicState);
        if (Directory.Exists(musicStateDir) == false) { Directory.CreateDirectory(musicStateDir); }
        Dictionary<AudioSwitch_Mission_Status, Dictionary<AudioState_Player_State, HashSet<string>>> musicStateFiles = new Dictionary<AudioSwitch_Mission_Status, Dictionary<AudioState_Player_State, HashSet<string>>>();
        musicFiles.Add(musicState, musicStateFiles);
        foreach (AudioSwitch_Mission_Status missionStatus in missionStatuses) {
          string missionStatusDir = Path.Combine(musicStateDir, "MissionStatus_" + missionStatus);
          if (Directory.Exists(missionStatusDir) == false) { Directory.CreateDirectory(missionStatusDir); }
          Dictionary<AudioState_Player_State, HashSet<string>> missionStatusFiles = new Dictionary<AudioState_Player_State, HashSet<string>>();
          musicStateFiles.Add(missionStatus, missionStatusFiles);
          foreach (AudioState_Player_State playerState in playerStates) {
            string playerStateDir = Path.Combine(missionStatusDir, "PlayerState_" + playerState);
            if (Directory.Exists(playerStateDir) == false) { Directory.CreateDirectory(playerStateDir); }
            HashSet<string> playerStateFiles = new HashSet<string>();
            missionStatusFiles.Add(playerState, playerStateFiles);
            string[] musicSamples = Directory.GetFiles(playerStateDir, "*.mp3", SearchOption.AllDirectories);
            foreach(string f in musicSamples) playerStateFiles.Add(f);
            musicSamples = Directory.GetFiles(playerStateDir, "*.ogg", SearchOption.AllDirectories);
            foreach (string f in musicSamples) playerStateFiles.Add(f);
            musicSamples = Directory.GetFiles(playerStateDir, "*.wav", SearchOption.AllDirectories);
            foreach (string f in musicSamples) playerStateFiles.Add(f);
          }
        }
      }

      foreach (AudioState_Music_State musicState in musicStates) {
        string msdir = Path.Combine(musicDir, "MusicState_" + musicState);
        foreach(AudioSwitch_Mission_Status missionStatus in missionStatuses) {
          string mstdir = Path.Combine(msdir, "MissionStatus_" + missionStatus);
          foreach (AudioState_Player_State playerState in playerStates) {
            string psDir = Path.Combine(mstdir, "PlayerState_" + playerState);
            foreach (AudioState_Player_State ps in playerStates) {
              if (ps == playerState) { continue; }
              string psfile = Path.Combine(psDir, "PlayerState_" + ps+".txt");
              if (File.Exists(psfile)) {
                foreach (string f in musicFiles[musicState][missionStatus][ps]) musicFiles[musicState][missionStatus][playerState].Add(f);
              }
            }
          }
        }
      }

      foreach (AudioState_Music_State musicState in musicStates) {
        string msdir = Path.Combine(musicDir, "MusicState_" + musicState);
        foreach (AudioSwitch_Mission_Status missionStatus in missionStatuses) {
          string mstdir = Path.Combine(msdir, "MissionStatus_" + missionStatus);
          foreach (AudioSwitch_Mission_Status mst in missionStatuses) {
            if (mst == missionStatus) { continue; }
            string mstfile = Path.Combine(mstdir, "MissionStatus_" + mst + ".txt");
            if (File.Exists(mstfile) == false) { continue; }
            foreach (AudioState_Player_State ps in playerStates) {
              foreach (string f in musicFiles[musicState][mst][ps]) musicFiles[musicState][missionStatus][ps].Add(f);
            }
          }
        }
      }

      foreach (AudioState_Music_State musicState in musicStates) {
        string msdir = Path.Combine(musicDir, "MusicState_" + musicState);
        foreach (AudioState_Music_State ms in musicStates) {
          if (ms == musicState) { continue; }
          string msfile = Path.Combine(msdir, "MusicState_" + ms + ".txt");
          if (File.Exists(msfile) == false) { continue; }
          foreach (AudioSwitch_Mission_Status mst in missionStatuses) {
            foreach (AudioState_Player_State ps in playerStates) {
              foreach (string f in musicFiles[ms][mst][ps]) musicFiles[musicState][mst][ps].Add(f);
            }
          }
        }
      }
      File.WriteAllText(cacheFile, JsonConvert.SerializeObject(musicFiles,Formatting.Indented));
      Log.M?.TWL(0, "CustomMusicInited.Init finish");
      //Log.M?.TWL(0, "musicFiles dump");
      //foreach (var musicState in musicFiles) {
      //  Log.M?.WL(1, "MusicState_"+ musicState.Key);
      //  foreach(var missionStatus in musicState.Value) {
      //    Log.M?.WL(2, "MissionStatus_" + missionStatus.Key);
      //    foreach (var playerState in missionStatus.Value) {
      //      Log.M?.WL(3, "PlayerState_" + playerState.Key);
      //      foreach(var path in playerState.Value) {
      //        Log.M?.WL(4, path);
      //      }
      //    }
      //  }
      //}
      //Log.M?.WL(0,"",true);
    }
  }
}