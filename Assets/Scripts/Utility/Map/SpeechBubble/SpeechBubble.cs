using UnityEngine;
using UnityEngine.UI;

namespace Utility.Map.SpeechBubble
{
    public class SpeechBubble : MonoBehaviour
    {
        [SerializeField]
        private Text speakerText;
        [SerializeField]
        private Text contextText;

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