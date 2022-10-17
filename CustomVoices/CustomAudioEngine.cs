using BattleTech;
using BattleTech.UI;
using Harmony;
using HBS;
using ManagedBass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using UnityEngine;

namespace CustomVoices {
  [HarmonyPatch(typeof(UIManager))]
  [HarmonyPatch("Awake")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  internal static class UIManager_Awake {
    public static void Postfix(UIManager __instance) {
      Log.M?.TWL(0, "UIManager.Awake");
      try {
        AudioEngine.Init();
        if(AudioEngine.Instance == null) {
          AudioEngine.Instance = __instance.gameObject.GetComponent<AudioEngine>();
          if (AudioEngine.Instance == null) {
            AudioEngine.Instance = __instance.gameObject.AddComponent<AudioEngine>();
            //AudioEngine.Instance.MusicBus.Play("getout", true);
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(AudioEventManager))]
  [HarmonyPatch("LoadAudioSettings")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[]{})]
  internal static class AudioEventManager_LoadAudioSettings {
    public static void Postfix() {
      Log.M?.TWL(0,"AudioEventManager.LoadAudioSettings");
      try { 
        AudioEngine.Instance.MasterVolume = (AudioEventManager.MasterVolume / 100f) *Core.settings.volumes.VoiceOverBus;
        Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.MasterVolume:" + AudioEngine.Instance.MasterVolume);
        AudioEngine.Instance.MusicBus.volume = (AudioEventManager.MusicVolume / 100f) *Core.settings.volumes.MusicBus;
        Log.M?.WL(1, "AudioEngine("+ AudioEngine.Instance.GetInstanceID() + ").Instance.MusicBus.volume:" + AudioEngine.Instance.MusicBus?.volume);
        AudioEngine.Instance.VoiceOverBus.volume = (AudioEventManager.VoiceVolume / 100f) * Core.settings.volumes.VoiceOverBus;
        Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.VoiceVolume.volume:" + AudioEngine.Instance.VoiceOverBus?.volume);
        AudioEngine.Instance.CombatBus.volume = (AudioEventManager.SFXVolume / 100f) * Core.settings.volumes.CombatBus;
        Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.CombatBus.volume:" + AudioEngine.Instance.CombatBus?.volume);
        AudioEngine.Instance.AmbientBus.volume = (AudioEventManager.AmbienceVolume / 100f) * Core.settings.volumes.AmbientBus;
        Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.AmbienceVolume.volume:" + AudioEngine.Instance.AmbientBus?.volume);
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(AudioEventManager))]
  [HarmonyPatch("SaveAudioSettings")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  internal static class AudioEventManager_SaveAudioSettings {
    public static void Postfix() {
      Log.M?.TWL(0, "AudioEventManager.SaveAudioSettings");
      try { 
        AudioEngine.Instance.MasterVolume = AudioEventManager.MasterVolume / 100f;
        Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.MasterVolume:" + AudioEngine.Instance.MasterVolume);
        AudioEngine.Instance.MusicBus.volume = (AudioEventManager.MusicVolume / 100f) * Core.settings.volumes.MusicBus;
        Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.MusicBus.volume:" + AudioEngine.Instance.MusicBus?.volume);
        AudioEngine.Instance.VoiceOverBus.volume = (AudioEventManager.VoiceVolume / 100f) * Core.settings.volumes.VoiceOverBus;
        Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.VoiceVolume.volume:" + AudioEngine.Instance.VoiceOverBus?.volume);
        AudioEngine.Instance.CombatBus.volume = (AudioEventManager.SFXVolume / 100f) * Core.settings.volumes.CombatBus;
        Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.CombatBus.volume:" + AudioEngine.Instance.CombatBus?.volume);
        AudioEngine.Instance.AmbientBus.volume = (AudioEventManager.AmbienceVolume / 100f) * Core.settings.volumes.AmbientBus;
        Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.AmbienceVolume.volume:" + AudioEngine.Instance.AmbientBus?.volume);
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(WwiseManager))]
  [HarmonyPatch("SuspendSoundEngine")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  internal static class WwiseManager_SuspendSoundEngine {
    public static void Postfix(WwiseManager __instance) {
      Log.M?.TWL(0, "WwiseManager.SuspendSoundEngine");
      try { 
        if (__instance.playInBackground == false) {
          if (AudioEngine.Instance != null) {
            AudioEngine.Instance.MasterVolume = 0f;
            //Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.MasterVolume:" + AudioEngine.Instance.MasterVolume);
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(WwiseManager))]
  [HarmonyPatch("WakeupSoundEngine")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  internal static class WwiseManager_WakeupSoundEngine {
    public static void Postfix() {
      Log.M?.TWL(0, "WwiseManager.WakeupSoundEngine");
      try {
        if (AudioEngine.Instance != null) {
          AudioEngine.Instance.MasterVolume = AudioEventManager.MasterVolume / 100f;
          //Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.MasterVolume:" + AudioEngine.Instance.MasterVolume);
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(AudioSettingsModule))]
  [HarmonyPatch("ApplyUpdatedSlider")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  internal static class AudioSettingsModule_ApplyUpdatedSlider {
    public static void Postfix(AudioSettingsModule __instance,string val,
        float ___currentVolumeMaster,
        float ___currentVolumeMusic,
        float ___currentVolumeVoice,
        float ___currentVolumeSFX,
        float ___currentVolumeAmbience,
        float ___currentVolumeCinematic
      ) {
      //Log.M?.TWL(0, "AudioSettingsModule.ApplyUpdatedSlider");
      try {
        AudioEngine.Instance.MasterVolume = ___currentVolumeMaster / 100f;
        //Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.MasterVolume:" + AudioEngine.Instance.MasterVolume);
        AudioEngine.Instance.MusicBus.volume = (___currentVolumeMusic / 100f) * Core.settings.volumes.MusicBus;
        //Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.MusicBus.volume:" + AudioEngine.Instance.MusicBus?.volume);
        AudioEngine.Instance.VoiceOverBus.volume = (___currentVolumeVoice / 100f) * Core.settings.volumes.VoiceOverBus;
        //Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.VoiceVolume.volume:" + AudioEngine.Instance.VoiceOverBus?.volume);
        AudioEngine.Instance.CombatBus.volume = (___currentVolumeSFX / 100f) * Core.settings.volumes.CombatBus;
        //Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.CombatBus.volume:" + AudioEngine.Instance.CombatBus?.volume);
        AudioEngine.Instance.AmbientBus.volume = (___currentVolumeAmbience / 100f) * Core.settings.volumes.AmbientBus;
        //Log.M?.WL(1, "AudioEngine(" + AudioEngine.Instance.GetInstanceID() + ").Instance.AmbienceVolume.volume:" + AudioEngine.Instance.AmbientBus?.volume);
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  public class AudioSampleSettings {
    public float volume { get; set; } = 1f;
    public float min3D { get; set; } = 50f;
    public float max3D { get; set; } = 1000f;
  }
  public class AudioObject: MonoBehaviour {
    public class Playing {
      public int counter { get; set; } = 0;
      public AudioEngine.AudioChannel channel { get; set; } = null;
      public Playing(AudioEngine.AudioChannel channel) {
        this.channel = channel;
        counter = 1;
      }
    }
    protected Dictionary<string, Playing> currentlyPlaying = new Dictionary<string, Playing>();
    public void Start() {
      AudioEngine.Instance.RegisterAudioObject(this);
    }
    public void OnDisable() {
      foreach(var channel in this.currentlyPlaying) {
        channel.Value.channel.Pause();
      }
    }
    public void OnEnable() {
      foreach (var channel in this.currentlyPlaying) {
        channel.Value.channel.Resume();
      }
    }
    private void ClearNotPlaying() {
      HashSet<string> toDelete = new HashSet<string>();
      foreach (var playing in this.currentlyPlaying) {
        if (playing.Value.channel.detached) {
          --playing.Value.counter;
          if (playing.Value.counter <= 0) {
            toDelete.Add(playing.Key);
          } else {
            playing.Value.channel = AudioEngine.Instance.CombatBus.Play(playing.Value.channel.parent.name, playing.Value.channel.loop);
          }
        };
      }
      foreach(string name in toDelete) { currentlyPlaying.Remove(name); }
      //playingQueue.
    }
    public void LateUpdate() {
      ClearNotPlaying();
    }
    public void Play(string name, bool loop) {
      try {
        if (currentlyPlaying.TryGetValue(name, out var existingChannel)) {
          existingChannel.channel.loop = loop;
          ++existingChannel.counter;
          return;
        }
        AudioEngine.AudioChannel channel = AudioEngine.Instance.CombatBus.Play(name, loop);
        currentlyPlaying.Add(name, new Playing(channel));
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    public void Play(string name, bool loop, List<AudioEngine.AudioChannelEvent> events) {
      try {
        if(currentlyPlaying.TryGetValue(name, out var existingChannel)) {
          existingChannel.channel.loop = loop;
          ++existingChannel.counter;
          return;
        }
        AudioEngine.AudioChannel channel = AudioEngine.Instance.CombatBus.Play(name, loop, events);
        currentlyPlaying.Add(name, new Playing(channel));
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
    }
    public void Stop(string name) {
      try {
        if(currentlyPlaying.TryGetValue(name,out var playing)) {
          --playing.counter;
          if (playing.counter <= 0) {
            playing.channel.Stop();
            currentlyPlaying.Remove(name);
          } else {
            if (playing.channel.loop == false) {
              if (playing.channel.detached) {
                playing.channel = AudioEngine.Instance.CombatBus.Play(playing.channel.parent.name, false);
              } else {
                playing.channel.Play();
              }
            }
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    public void OnDestroy() {
      foreach (var channel in this.currentlyPlaying) {
        channel.Value.channel.Stop();
      }
      currentlyPlaying.Clear();
      AudioEngine.Instance.UnregisterAudioObject(this);
    }
    public void Update3DPosition(Vector3D mbPosition) {
      foreach (var playing in this.currentlyPlaying) {
        playing.Value.channel.UpdatePosition(mbPosition);
      }
    }
  }
  public class AudioEngine: MonoBehaviour {
    public static AudioEngine Instance = null;
    public static readonly int MAX_CHANNELS_PER_SAMPLE = 32;
    private static Dictionary<string, VersionManifestEntry> samplesManifest = new Dictionary<string, VersionManifestEntry>();
    private static Dictionary<string, VersionManifestEntry> samplesInfoManifest = new Dictionary<string, VersionManifestEntry>();
    private static Dictionary<string, AudioSampleSettings> cachedSettings = new Dictionary<string, AudioSampleSettings>();
    public static string ResetSettings() {
      cachedSettings.Clear();
      foreach(var sample in samplesManifest) {
        AudioSampleSettings info = null;
        if (samplesInfoManifest.TryGetValue(sample.Key, out var infoEntry)) {
          using (StreamReader stream = new StreamReader(infoEntry.FilePath)) {
            info = JsonConvert.DeserializeObject<AudioSampleSettings>(stream.ReadToEnd());
          }
        }
        if (info == null) { info = new AudioSampleSettings(); }
        cachedSettings.Add(sample.Key, info);
      }
      return JsonConvert.SerializeObject(cachedSettings, Formatting.Indented);
    }
    public static void ReadSettings(string json) {
      cachedSettings = JsonConvert.DeserializeObject<Dictionary<string, AudioSampleSettings>>(json);
    }
    public void RegisterAudioObject(AudioObject audioObject) {
      this.CombatBus.audioObjects3D.Add(audioObject);
    }
    public void UnregisterAudioObject(AudioObject audioObject) {
      this.CombatBus.audioObjects3D.Remove(audioObject);
    }
    public static bool isInAudioManifest(string name) {
      return samplesManifest.ContainsKey(name);
    }
    public static void getAudioFiles(Dictionary<string, VersionManifestEntry> manifest) {
      foreach(var item in manifest) {
        if (samplesManifest.ContainsKey(item.Key)) { samplesManifest[item.Key] = item.Value; } else { samplesManifest.Add(item.Key, item.Value); }
      }
    }
    public static void getAudioFilesSettings(Dictionary<string, VersionManifestEntry> manifest) {
      foreach (var item in manifest) {
        if (samplesInfoManifest.ContainsKey(item.Key)) { samplesInfoManifest[item.Key] = item.Value; } else { samplesInfoManifest.Add(item.Key, item.Value); }
      }
    }
    private float f_MasterVolume = 0f;
    public float MasterVolume {
      get {
        return f_MasterVolume;
      }
      set {
        f_MasterVolume = value;
        this.VoiceOverBus.updateVolume();
        this.MusicBus.updateVolume();
        this.CombatBus.updateVolume();
      }
    }
    private AudioBus f_VoiceOverBus = null;
    private AudioBus f_MusicBus = null;
    private AudioBus f_AmbientBus = null;
    private AudioBus f_CombatBus = null;
    public AudioBus VoiceOverBus { get { if (f_VoiceOverBus == null) { f_VoiceOverBus = new AudioBus(this, "voice", false); }; return f_VoiceOverBus; } }
    public AudioBus MusicBus { get { if (f_MusicBus == null) { f_MusicBus = new AudioBus(this, "music", false); }; return f_MusicBus; } }
    public AudioBus AmbientBus { get { if (f_AmbientBus == null) { f_AmbientBus = new AudioBus(this, "ambient", false); }; return f_AmbientBus; } }
    public AudioBus CombatBus { get { if (f_CombatBus == null) { f_CombatBus = new AudioBus(this, "sfx", true); }; return f_CombatBus; } }
    //public static AudioEngine Instance { get { return LazySingletonBehavior<AudioEngine>.Instance; } }
    public void Update() {
      if (EngineInited == false) { return; }
      //Log.M?.TWL(0, "AudioEngine.Update");
      this.f_CombatBus?.Update();
    }
    public void updateVolume() {

    }
    public void LateUpdate() {
      if (AudioEngine.EngineInited == false) { return; }
      //Log.M?.TWL(0, "AudioEngine.LateUpdate");
      //try {
      {
        this.f_VoiceOverBus?.LateUpdate(Time.deltaTime);
        this.f_MusicBus?.LateUpdate(Time.deltaTime);
        this.f_CombatBus?.LateUpdate(Time.deltaTime);
        this.f_CombatBus?.position3DUpdate();
      }
      //}catch(Exception e) {
        //Log.M?.TWL(0,e.ToString());
      //}
    }
    private static bool EngineInited = false;
    public static void Init() {
      Log.M?.TWL(0, "AudioEngine.Init");
      if(AudioEngine.EngineInited == false) {
        if (Bass.Init(-1,44100, DeviceInitFlags.Device3D)) {
          AudioEngine.EngineInited = true;
        } else {
          throw new BassException();
        }
      }
    }
    public class AudioSample {
      public int handle { get; private set; }
      public string name { get; private set; }
      public SampleInfo info { get; private set; }
      public AudioSampleSettings settings { get; private set; }
      public AudioSample(string filepath, bool pos3Dengine = false, AudioSampleSettings settings = null) {
        BassFlags flags = BassFlags.Default;
        if (pos3Dengine) { flags |= (BassFlags.Mono | BassFlags.Bass3D ); }
        using (StreamReader stream = new StreamReader(filepath)) {
          using (var memstream = new MemoryStream()) {
            stream.BaseStream.CopyTo(memstream);
            byte[] data = memstream.ToArray();
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, unmanagedPointer, data.Length);
            handle = Bass.SampleLoad(unmanagedPointer, 0, data.Length, MAX_CHANNELS_PER_SAMPLE, flags);
            Marshal.FreeHGlobal(unmanagedPointer);            
          }
        }          
        if(this.handle == 0) {
          throw new System.Exception("fail to load " + filepath + " due to " + Bass.LastError);
        }
        this.settings = settings == null ? new AudioSampleSettings() : settings;
        info = Bass.SampleGetInfo(this.handle);
        this.name = filepath;
      }
    }
    public class AudioChannelEvent {
      public float position { get; set; }
      public Action callback { get; set; }
      public AudioChannelEvent(float pos, Action callback) {
        this.position = pos; this.callback = callback;
      }
    }
    public class AudioChannel {
      public int handle { get; private set; }
      public bool detached { get; private set; } = false;
      public AudioSample parent { get; private set; }
      public AudioBus bus { get; private set; }
      public float length { get; private set; }
      public float position { get; private set; }
      public HashSet<AudioChannelEvent> events { get; private set; }
      public bool loop {
        get { return Bass.ChannelHasFlag(this.handle, BassFlags.Loop); }
        set { if (value) { Bass.ChannelAddFlag(this.handle, BassFlags.Loop); } else { Bass.ChannelRemoveFlag(this.handle, BassFlags.Loop); } }
      }
      public bool isPlaying { get; private set; } = false;
      internal float volume {
        get { return (float)Bass.ChannelGetAttribute(handle, ChannelAttribute.Volume); }
        set {
          //Log.M?.TWL(0, "AudioChannel.set_volume:"+this.parent.name+":"+value);
          Bass.ChannelSetAttribute(handle, ChannelAttribute.Volume, (double)value);
        }
      }
      public void detachFromBus() {
        detached = true;
        this.bus = null;
        this.parent = null;
        this.isPlaying = false;
        this.handle = 0;
      }
      public AudioChannel (AudioSample parent, AudioBus bus, List<AudioChannelEvent> events = null) {
        this.parent = parent;
        this.bus = bus;
        this.handle = Bass.SampleGetChannel(parent.handle, false);
        this.position = 0f;
        long len = Bass.ChannelGetLength(handle);
        this.length = (float)Bass.ChannelBytes2Seconds(this.handle, len);
        this.events = events != null ? events.ToHashSet() : null;
        if (bus.use3D) {
          this.Apply3DSettings(parent.settings.min3D, parent.settings.max3D);
        }
      }
      protected void Apply3DSettings(float min, float max) {
        ManagedBass.Bass.ChannelSet3DAttributes(this.handle, Mode3D.Normal, min, max, 360, 360, 100f);
      }
      public void Play() {
        if (this.detached) { return; }
        Bass.ChannelPlay(this.handle, true);
        this.isPlaying = true;
      }
      public void Pause() {
        if (this.detached) { return; }
        if (this.isPlaying) {
          Bass.ChannelPause(this.handle);
          this.isPlaying = false;
        }
      }
      internal void StopInternal() {
        if (this.detached) { return; }
        Bass.ChannelStop(this.handle);
        this.isPlaying = false;
      }
      public void Stop() {
        if (this.detached) { return; }
        this.isPlaying = false;
        Bass.ChannelStop(this.handle);
        this.bus?.OnChannelStop(this);
      }
      public void Resume() {
        if (this.detached) { return; }
        if (this.isPlaying == false) {
          Bass.ChannelPlay(this.handle, false);
          this.isPlaying = true;
        }
      }
      public void UpdatePosition(Vector3D pos) {
        if (this.detached) { return; }
        ManagedBass.Bass.ChannelSet3DPosition(this.handle, pos, null, null);
      }
      public void Update(float delta) {
        if (this.isPlaying == false) { return; }
        if (this.detached) { return; }
        this.position += delta;
        if (this.position >= this.length) {
          if (this.loop == false) {
            PlaybackState state = Bass.ChannelIsActive(this.handle);
            //Log.M?.TWL(0,this.parent.name+":"+state);
            if (state == PlaybackState.Stopped) {
              this.position = length; this.isPlaying = false;
              try { Bass.ChannelStop(this.handle); } finally { }
            }
          } else {
            this.position = 0; this.isPlaying = true;
          }
        }
        if(this.events != null) {
          //Log.M?.TWL(0,"channel.update "+this.parent.name+" events:"+ this.events.Count + " position:"+this.position+" isPlaying:"+ isPlaying);
          if(this.events.Count > 0) {
            HashSet<AudioChannelEvent> fired = new HashSet<AudioChannelEvent>();
            foreach (AudioChannelEvent tstEvent in this.events) {
              if(((tstEvent.position <= this.position)&&(tstEvent.position > Core.Epsilon))||(this.isPlaying == false)) { fired.Add(tstEvent); }
            }
            //Log.M?.WL(1, "fired:"+fired.Count);
            foreach (AudioChannelEvent tstEvent in fired) {
              this.events.Remove(tstEvent);
              try {
                tstEvent.callback();
              } catch (Exception e) {
                Log.M?.TWL(0, e.ToString(), true);
              }
            }
          }
        }
        if (this.isPlaying == false) { this.bus?.OnChannelStop(this); }
      }
    }
    public class AudioBus {
      public string name { get; private set; }
      public HashSet<AudioObject> audioObjects3D { get; private set; } = new HashSet<AudioObject>();
      public AudioEngine engine { get; private set; } = null;
      //private SpinLock spinlock = new SpinLock();
      private Dictionary<string, AudioEngine.AudioSample> registredSamples = new Dictionary<string, AudioEngine.AudioSample>();
      private HashSet<AudioChannel> channels = new HashSet<AudioChannel>();
      private float f_volume = 1f;
      public void OnChannelStop(AudioChannel channel) {
        if (this.channels.Remove(channel)) { channel.detachFromBus(); }
      }
      public void StopAll() {
        foreach (AudioChannel channel in channels) {
          channel.StopInternal();
          foreach(var ev in channel.events) { ev.callback(); }
          channel.detachFromBus();
        }
        channels.Clear();
      }
      public void PauseAll() {
        foreach (AudioChannel channel in channels) {
          channel.Pause();
        }
      }
      public void ResumeAll() {
        foreach (AudioChannel channel in channels) {
          channel.Resume();
        }
      }
      public float volume {
        get {
          return f_volume;
        }
        set {
          f_volume = value;
          this.updateVolume();
        }
      }
      public bool use3D { get; private set; }
      internal AudioBus(AudioEngine engine,string name,bool use3D) {
        this.engine = engine;
        this.name = name;
        this.use3D = use3D;
      }
      public void updateVolume() {
        foreach(var channel in channels) {
          channel.volume = this.engine.MasterVolume * this.volume * channel.parent.settings.volume;
        }
      }
      public void Update() {

      }
      public void LateUpdate(float delta) {
        //Log.M?.TWL(0, "AudioBus.LateUpdate "+this.name+" "+ channels.Count);
        HashSet<AudioChannel> temp = new HashSet<AudioChannel>();
        foreach (var channel in this.channels) { temp.Add(channel); }
        foreach (var channel in temp) {
          //Log.M?.WL(1, channel.parent.name + " isPlaying:" + channel.isPlaying + " position:" + channel.position + "/" + channel.length + " isloop:" + channel.loop);
          channel.Update(delta);          
        }
      }
      protected AudioChannel Play(AudioEngine.AudioSample sample, bool loop, List<AudioChannelEvent> events = null) {
        AudioChannel result = new AudioChannel(sample, this, events);
        result.volume = this.engine.MasterVolume * this.volume * sample.settings.volume;
        result.loop = loop;
        this.channels.Add(result);
        result.Play();
        Log.M?.TWL(0, "on engine: "+engine.GetInstanceID()+" on bus "+this.name+":" + this.channels.Count 
          + " " + result.parent.name + " isPlaying:" + result.isPlaying 
          + " length:" + result.length + " isloop:"+result.loop+"/"+loop
          + " volume: "+result.volume + "(master:"+ this.engine.MasterVolume + " bus:"+ this.volume + " own:"+ sample.settings.volume + ")"
          );
        return result;
      }
      public void Preload(string id) {
        if (AudioEngine.samplesManifest.TryGetValue(id, out var entry)) {
          if (registredSamples.TryGetValue(entry.FilePath, out var sample) == false) {
            AudioSampleSettings setting = null;
            if(cachedSettings.TryGetValue(id, out var info)) {
              setting = info;
            }else
            if (samplesInfoManifest.TryGetValue(id, out var settingEntry)) {
              using (StreamReader stream = new StreamReader(settingEntry.FilePath)) {
                setting = JsonConvert.DeserializeObject<AudioSampleSettings>(stream.ReadToEnd());
              }
            }
            sample = new AudioSample(entry.FilePath, this.use3D, setting);
            registredSamples.Add(entry.FilePath, sample);
          }
        } else {
          throw new System.Exception("no audio file with " + id + " in manifest");
        }
      }
      public void position3DUpdate() {
        Vector3 cameraPosition = CameraControl.HasInstance ? CameraControl.Instance.CameraPos : Vector3.zero;
        foreach(var audio3Dobject in this.audioObjects3D) {
          Vector3 position = audio3Dobject.transform.position - cameraPosition;
          ManagedBass.Vector3D mbPosition = new Vector3D(position.x, position.y, position.z);
          audio3Dobject.Update3DPosition(mbPosition);
        }
        ManagedBass.Bass.Apply3D();
      }
      public AudioChannel PlayFile(string path, bool loop, List<AudioChannelEvent> events = null) {
        if (File.Exists(path) == false) { throw new System.Exception("no audio file with " + path + " in file system"); }
        if (registredSamples.TryGetValue(path, out var sample) == false) {
          sample = new AudioSample(path, this.use3D, null);
          registredSamples.Add(path, sample);
        }
        return this.Play(sample, loop, events);
      }
      public AudioChannel Play(string id, bool loop, List<AudioChannelEvent> events = null) {
        if(AudioEngine.samplesManifest.TryGetValue(id, out var entry)) {
          if (registredSamples.TryGetValue(entry.FilePath, out var sample) == false) {
            AudioSampleSettings setting = null;
            if (samplesInfoManifest.TryGetValue(id, out var settingEntry)) {
              Log.M?.TWL(0,"settings for "+id+" found");
              using (StreamReader stream = new StreamReader(settingEntry.FilePath)) {
                setting = JsonConvert.DeserializeObject<AudioSampleSettings>(stream.ReadToEnd());
              }
            }
            sample = new AudioSample(entry.FilePath, this.use3D, setting);
            registredSamples.Add(entry.FilePath, sample);
          }
          return this.Play(sample, loop, events);
        } else {
          throw new System.Exception("no audio file with "+id+" in manifest");
        }
      }
    }
  }
}