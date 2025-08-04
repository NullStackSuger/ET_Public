using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace ET.Client;

[EntitySystemOf(typeof(AudioComponent))]
public static partial class AudioComponentSystem
{
    [EntitySystem]
    private static void Awake(this AudioComponent self)
    {

    }

    [EntitySystem]
    private static void Serialize(this AudioComponent self)
    {

    }

    [EntitySystem]
    private static void Deserialize(this AudioComponent self)
    {

    }

    [EntitySystem]
    private static void LateUpdate(this AudioComponent self)
    {
        /*InputComponent inputComponent = self.Scene().GetComponent<InputComponent>();
        if (inputComponent.Get(Key.A) == InputState.Down)
        {
            self.Pause();
        }
        if (inputComponent.Get(Key.D) == InputState.Down)
        {
            self.Play();
        }
        if (inputComponent.Get(Key.W) == InputState.Down)
        {
            self.Stop();
        }*/
        
        if (!(self.current == null || self.current.wasapiOut.PlaybackState == PlaybackState.Stopped)) return;

        if (!self.waitPlay.TryDequeue(out self.current)) return;
        
        self.current.isInit = true;
        self.current.wasapiOut.Init(self.current.clip);
        self.current.wasapiOut.Play();
    }

    // 不推荐用ETTask, 因为有Stop PlayImmediately这些可能导致播放顺序混乱,导致tcs超时报错
    public static void Play(this AudioComponent self, ISampleProvider clip, AudioClientShareMode mode = AudioClientShareMode.Shared, int latency = 200)
    {
        var audioClip = new AudioComponent.AudioClip() { clip = clip, wasapiOut = new WasapiOut(mode, latency) };
        self.current ??= audioClip;
        self.waitPlay.Enqueue(audioClip);
    }

    public static void Play(this AudioComponent self, string path, AudioClientShareMode mode = AudioClientShareMode.Shared, int latency = 200)
    {
        using var waveFileReader = new WaveFileReader(path);
        var audioClip = new AudioComponent.AudioClip() { clip = waveFileReader.ToSampleProvider(), wasapiOut = new WasapiOut(mode, latency) };
        self.current ??= audioClip;
        self.waitPlay.Enqueue(audioClip);
    }

    public static void PlayImmediately(this AudioComponent self, ISampleProvider clip, AudioClientShareMode mode = AudioClientShareMode.Shared, int latency = 200)
    {
        self.current = null;
        self.waitPlay.Clear();
        
        var audioClip = new AudioComponent.AudioClip() { clip = clip, wasapiOut = new WasapiOut(mode, latency) };
        self.current = audioClip;
        audioClip.isInit = true;
        audioClip.wasapiOut.Init(clip);
        audioClip.wasapiOut.Play();
    }

    public static void PlayImmediately(this AudioComponent self, string path, AudioClientShareMode mode = AudioClientShareMode.Shared, int latency = 200)
    {
        self.current = null;
        self.waitPlay.Clear();
        
        using var waveFileReader = new WaveFileReader(path);
        var audioClip = new AudioComponent.AudioClip() { clip = waveFileReader.ToSampleProvider(), wasapiOut = new WasapiOut(mode, latency) };
        self.current = audioClip;
        audioClip.isInit = true;
        audioClip.wasapiOut.Init(audioClip.clip);
        audioClip.wasapiOut.Play();
    }
    
    public static void Play(this AudioComponent self)
    {
        if (self.current == null || self.current.isInit == false || self.current.wasapiOut.PlaybackState == PlaybackState.Playing) return;
        
        self.current.wasapiOut.Play();
    }
    
    public static void Stop(this AudioComponent self)
    {
        if (self.current == null || self.current.isInit == false || self.current.wasapiOut.PlaybackState == PlaybackState.Stopped) return;
        
        self.current.wasapiOut.Stop();
    }

    public static void Pause(this AudioComponent self)
    {
        if (self.current == null || self.current.isInit == false || self.current.wasapiOut.PlaybackState == PlaybackState.Paused) return;
        
        self.current.wasapiOut.Pause();
    }
}