using UnityEngine;
using UnityEngine.UI;

namespace Utility.Map.SpeechBubble
{
    public class SpeechBubble : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private Text speakerText;
        [SerializeField]
        private Text contextText;
#pragma warning restore 0649

        public void SetSpeaker(string speaker)
        {
            speakerText.text = speaker;
        }

        public void SetContext(string context)
        {
            contextText.text = context;
        }
    }
}