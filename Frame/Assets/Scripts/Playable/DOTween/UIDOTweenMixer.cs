using UnityEngine.Playables;

namespace GameFrame
{
    public class UIDOTweenMixer: PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);

            int    inputCount = playable.GetInputCount();
            double time       = playable.GetTime();

            for (int i = 0; i < inputCount; i++)
            {
                Playable          input    = playable.GetInput(i);
                UIDOTweenBehavior behavior = input.GetScriptBehaviour<UIDOTweenBehavior>();
                if (behavior == null)
                {
                    continue;
                }

                if (time > behavior.endTime)
                {
                    behavior.ToEnd();
                }
                else if (time < behavior.startTime)
                {
                    behavior.ToStart();
                }
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            base.OnBehaviourPause(playable, info);
            int inputCount = playable.GetInputCount();

            for (int i = 0; i < inputCount; i++)
            {
                Playable          input    = playable.GetInput(i);
                UIDOTweenBehavior behavior = input.GetScriptBehaviour<UIDOTweenBehavior>();

                behavior?.ToStart();
            }
        }
    }
}