using System.Collections;
using UnityEngine.Audio;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    //array of sounds, sound class let's me easily set sounds up in the inspector
    public Sound[] sounds;
    public Sound[] BackgroundTracks;

    public AudioMixerGroup musicMixerGroup, SFXMixerGroup;

    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = SFXMixerGroup;
        }

        foreach (Sound bgTrack in BackgroundTracks)
        {
            bgTrack.source = gameObject.AddComponent<AudioSource>();
            bgTrack.source.clip = bgTrack.clip;
            
            bgTrack.source.volume = bgTrack.volume;
            bgTrack.source.pitch = bgTrack.pitch;
            bgTrack.source.loop = bgTrack.loop;
            bgTrack.source.outputAudioMixerGroup = musicMixerGroup;
        }
    }

    private void Start()
    {
        if (BackgroundTracks.Length > 0)
        {
            StartCoroutine("PlayBackgroundMusic");
        }
    }

    private void Update()
    {
        //This is for debuggin because my audio sources randomly starting throwing a null reference, even though they're all their....................
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            foreach (Sound s in sounds)
            {
                Debug.Log(s.name + s.source);
            }
        }
    }

    //function used to play a sound
    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Sound '" + name + "' not found.");
            return;
        }
        if (s.source == null)
        {
            Debug.Log("Audio Source for sound: '" + name + "' Not found;");
            return;
        }

        //set the pitch of the source back to original in case the function below was used
        s.source.pitch = s.pitch;

        s.source.Play();
    }

    //play function with varying pitch;
    public void Play(string name, float minPitch, float maxPitch)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Sound '" + name + "' not found.");
            return;
        }
        if (s.source == null)
        {
            Debug.Log("Audio Source for sound: '" + name + "' Not found;");
            return;
        }

        //randomize the pitch between specified values
        s.source.pitch = Random.Range(minPitch, maxPitch);

        s.source.Play();
    }

    //function used to play a sound from s specified array
    public void Play(string name, Sound[] sArr)
    {
        Sound s = Array.Find(sArr, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Sound '" + name + "' not found.");
            return;
        }
        if (s.source == null)
        {
            Debug.Log("Audio Source for sound: '" + name + "' Not found;");
            return;
        }

        //set the pitch of the source back to original in case the function below was used
        s.source.pitch = s.pitch;

        s.source.Play();
    }

    private IEnumerator PlayBackgroundMusic()
    {
        int trackIndex = -1;        //both set to -1 cause i want a song to be randomly selected to play first.
        int previousTrack = -1;
        while (true)
        {
            //while currently rolled track is the same as previous track reroll
            while(trackIndex == previousTrack)      trackIndex = UnityEngine.Random.Range(0, BackgroundTracks.Length);
            //save the index of currently played track into previousTrack variable 
            previousTrack = trackIndex;
            //fetch the track and its length
            Sound s = BackgroundTracks[trackIndex];
            float trackLength = s.clip.length;

            Play(s.name, BackgroundTracks);

            yield return new WaitForSeconds(trackLength);
        }
    }

}
