using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Utility.Character;
using Utility.Core;

namespace Utility.Timeline
{
    public class CharacterBindingHelper : MonoBehaviour
    {
        public CharacterType who;
        [SerializeField] private PlayableDirector[] playableDirectors;

        private void Awake()
        {
            var animator = GetComponent<Animator>();

            foreach (var playableDirector in playableDirectors)
            {
                var timelineAsset = playableDirector.playableAsset as TimelineAsset;
                if (timelineAsset == null)
                {
                    continue;
                }
                
                var trackAssets = timelineAsset.GetOutputTracks();
                foreach (var trackAsset in trackAssets)
                {
                    if (trackAsset is AnimationTrack)
                    {
                        var binding = playableDirector.GetGenericBinding(trackAsset);
                        if (binding == animator)
                        {
                            playableDirector.SetGenericBinding(trackAsset,
                                DataController.Instance.GetCharacter(who).CharacterAnimator);
                        }
                    }
                }

                playableDirector.stopped += _ =>
                {
                    foreach (var trackAsset in trackAssets)
                    {
                        if (trackAsset is AnimationTrack)
                        {
                            var binding = playableDirector.GetGenericBinding(trackAsset);
                            if (binding && ((Animator)binding).gameObject.TryGetComponent(out CharacterManager characterManager))
                            {
                                playableDirector.SetGenericBinding(trackAsset, null);
                            }
                        }
                    }
                };
            }
        }
    }
}
