using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace GameFrame
{
    [TrackColor(0.65f, 0.93f, 0.70f)]
    [TrackClipType(typeof(UIDOTweenClip))]
    [TrackBindingType(typeof(GameObject))]
    [DisplayName("DOTween轨道")]
    public class UITrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<UIDOTweenMixer>.Create(graph, inputCount);
        }

        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            Playable p = base.CreatePlayable(graph, gameObject, clip);

            UIDOTweenBehavior behavior = p.GetScriptBehaviour<UIDOTweenBehavior>();
            if (behavior != null)
            {
                behavior.startTime = clip.start;
                behavior.duration  = clip.duration;
            }

            return p;
        }
    }
}