using UnityEngine;
using System;
using System.Threading;
namespace ArkWalk {
    internal sealed class ArkTravelTone : MonoBehaviour {
        AudioSource source;
        AudioClip accepted, arrived;
        ArkTravelToneProbe probe;
        internal int Accepted {get;private set;}
        internal int Arrived {get;private set;}
        internal ArkTravelToneProbe Probe=>probe;
        internal const float Gain=.10f;
        internal void Setup(){
            var emitter=new GameObject("Travel instrument feedback");
            emitter.transform.SetParent(transform,false);
            source=emitter.AddComponent<AudioSource>();
            source.playOnAwake=false;source.spatialBlend=0;source.volume=Gain;
            source.ignoreListenerPause=false;source.ignoreListenerVolume=false;
            accepted=Tone("Route selection",.061f,475f,573f);
            arrived=Tone("Route arrival",.087f,685f,610f);
            var args=Environment.GetCommandLineArgs();
            if(Array.IndexOf(args,"-arkVerify")>=0&&Array.IndexOf(args,"-arkTravelAudio")>=0)
                probe=emitter.AddComponent<ArkTravelToneProbe>();
        }
        AudioClip Tone(string label,float seconds,float start,float finish){
            const int rate=48000;
            var wave=new float[Mathf.RoundToInt(rate*seconds)];float phase=0;
            for(int i=0;i<wave.Length;i++){
                float t=(float)i/(wave.Length-1),s=(float)i/rate;
                phase+=2*Mathf.PI*Mathf.Lerp(start,finish,t)/rate;
                float a=Mathf.Clamp01(s/.007f)*Mathf.Pow(1-t,1.7f);
                wave[i]=a*(.34f*Mathf.Sin(phase)+.07f*Mathf.Sin(phase*2));
            }
            var clip=AudioClip.Create(label,wave.Length,1,rate,false);
            clip.SetData(wave,0);return clip;
        }
        internal void Play(bool destination){
            if(!source||!source.isActiveAndEnabled||source.mute||AudioListener.volume<=0||AudioListener.pause)return;
            if(destination)Arrived++;else Accepted++;
            if(probe)probe.Stage=destination?2:1;
            source.PlayOneShot(destination?arrived:accepted);
        }
        void OnDestroy(){if(accepted)Destroy(accepted);if(arrived)Destroy(arrived);}
    }
    internal sealed class ArkTravelToneProbe : MonoBehaviour {
        int stage,acceptPeak,arrivePeak;
        internal int Stage {set=>Volatile.Write(ref stage,value);}
        internal float AcceptPeak=>Volatile.Read(ref acceptPeak)/1000000f;
        internal float ArrivePeak=>Volatile.Read(ref arrivePeak)/1000000f;
        void OnAudioFilterRead(float[] data,int channels){
            int w=Volatile.Read(ref stage),peak=0;
            for(int i=0;i<data.Length;i++){
                float sample=data[i];int v=(int)((sample<0?-sample:sample)*1000000f);
                if(v>peak)peak=v;
                data[i]=0; // QA exclusively: quiet monitoring before this source reaches output.
            }
            if(w==1&&peak>acceptPeak)Volatile.Write(ref acceptPeak,peak);
            else if(w==2&&peak>arrivePeak)Volatile.Write(ref arrivePeak,peak);
        }
    }
}
