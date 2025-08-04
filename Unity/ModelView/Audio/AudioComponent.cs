using NAudio.Wave;

namespace ET.Client;

public class AudioComponent : Entity, IAwake, ISerialize, IDeserialize, ILateUpdate
{
    public Queue<AudioClip> waitPlay = new();
    public AudioClip current;
    
    public class AudioClip
    {
        public ISampleProvider clip;
        public WasapiOut wasapiOut;
        public bool isInit;
    }
}