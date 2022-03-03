using System;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.Playables;

namespace GameFrame
{
    public class UIDOTweenBehavior : PlayableBehaviour
    {
        private GameObject _target;

        private Sequence _sequence;

        public double startTime;
        public double duration;

        public double endTime => startTime + duration;

        public void SetTarget(GameObject target)
        {
            _target = target;
            if (_target == null) return;
            _sequence = DOTween.Sequence();
            _sequence.SetUpdate(UpdateType.Manual);
            _sequence.SetAutoKill(false);
            foreach (IDOTweenUI component in _target.GetComponents<IDOTweenUI>())
            {
                try
                {
                    _sequence.Append(component.Tween);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            foreach (ABSAnimationComponent component in _target.GetComponents<ABSAnimationComponent>())
            {
                var tween = component.tween;
                if (tween == null)
                {
#if UNITY_EDITOR
                    if (Application.isEditor)
                    {
                        if (component is DOTweenAnimation animation)
                        {
                            tween = animation.CreateEditorPreview();
                        }
                    }
#endif
                }

                if (tween != null)
                {
                    _sequence.Append(tween);
                }
            }
        }

        public void ToEnd()
        {
            _sequence?.Goto(1);
        }

        public void ToStart()
        {
            _sequence?.Goto(0);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
            SetSequencePosition(playable);
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            base.OnPlayableDestroy(playable);
            _sequence?.Kill();
        }

        private void SetSequencePosition(Playable playable)
        {
            _sequence?.Goto((float) (playable.GetTime() / playable.GetDuration()));
        }
    }
}