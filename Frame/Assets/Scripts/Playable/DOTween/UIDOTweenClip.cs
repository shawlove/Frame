using System;
using System.Collections.Generic;
using System.ComponentModel;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace GameFrame
{
    [DisplayName("DOTween")]
    public class UIDOTweenClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedReference<GameObject> target;

        public override double duration => _duration;

        private double _duration = 5;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ScriptPlayable<UIDOTweenBehavior> p = ScriptPlayable<UIDOTweenBehavior>.Create(graph);
#if UNITY_EDITOR

            //同步dotween时长
            GameObject t = target.Resolve(graph.GetResolver());
            if (t != null)
            {
                _duration = 0;
                foreach (IDOTweenUI component in t.GetComponents<IDOTweenUI>())
                {
                    try
                    {
                        _duration = Math.Max(component.Tween.Duration(), _duration);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }

                foreach (ABSAnimationComponent component in t.GetComponents<ABSAnimationComponent>())
                {
                    var tween = component.tween;
                    if (tween != null)
                    {
                        _duration = Math.Max(tween.Duration(), _duration);
                    }
                }

                Invoker.InvokeWithSelectedClips<UIDOTweenClipAction>();
            }
#endif

            UIDOTweenBehavior behaviour = p.GetBehaviour();
            behaviour.SetTarget(target.Resolve(graph.GetResolver()));
            return p;
        }

        public ClipCaps clipCaps => ClipCaps.Blending;
    }

    public class UIDOTweenClipAction : ClipAction
    {
        public override bool Execute(IEnumerable<TimelineClip> clips)
        {
            foreach (TimelineClip clip in clips)
            {
                //同步dotween时长
                if (clip.asset is UIDOTweenClip uidoTweenClip && uidoTweenClip.duration != 0)
                {
                    clip.duration = uidoTweenClip.duration;
                }
            }

            return true;
        }

        public override ActionValidity Validate(IEnumerable<TimelineClip> clips)
        {
            return ActionValidity.Valid;
        }
    }
}