using System;
using System.Collections;
using UnityEngine;
using Utility.System;
using Random = UnityEngine.Random;

namespace Utility.SpeechBubble
{
    [Serializable]
    public struct Script
    {
        public string speaker;
        public float appearSec;
        public float disappearSec;
        public float randomRange;
        [TextArea] public string dialogue;
    }

    public class SpeechBubbleDialogue : MonoBehaviour
    {
        private enum SpeechBubbleDialogueType
        {
            Loop,
            Once
        }

        private enum RandomSecond
        {
            Custom,
            Random
        }

        [Header("말풍선 대화 타입 선택")] [SerializeField]
        private SpeechBubbleDialogueType speechBubbleDialogueType;

        [Header("말풍선 보이는, 안보이는 시간 타입 선택")] [SerializeField]
        private RandomSecond randomSecond;

        private SpeechBubble _speechBubble;

        [Header("말풍선")]
        [Header("말풍선 대사 입력")] [SerializeField] public Script[] bubbleScripts;

        [Header("말풍선 위치 조절")] [SerializeField] private Vector2 speechPos;

        private int bubbleIndex;
        private bool isExcuted;
        private bool isBubble;
        private bool isCharacterInRange;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
            _speechBubble = Instantiate(Resources.Load<GameObject>("SpeechBubble")).GetComponent<SpeechBubble>();
        }

        private void Update()
        {
            if (isBubble)
            {
                SetSpeechBubblePosition();
            }
        }

        private void UpdateDialogue()
        {
            _speechBubble.SetSpeaker(bubbleScripts[bubbleIndex].speaker);
            _speechBubble.SetContext(bubbleScripts[bubbleIndex].dialogue);

            if (randomSecond == RandomSecond.Random)
            {
                bubbleScripts[bubbleIndex].appearSec = Random.Range(1, bubbleScripts[bubbleIndex].randomRange);
                bubbleScripts[bubbleIndex].disappearSec = Random.Range(1, bubbleScripts[bubbleIndex].randomRange);
            }
            
            bubbleIndex = (bubbleIndex + 1) % bubbleScripts.Length;
        }

        private void SetSpeechBubblePosition()
        {
            Vector3 screenPoint = DataController.instance.cam.WorldToScreenPoint(transform.position);
            _speechBubble.transform.position = screenPoint + new Vector3(speechPos.x, speechPos.y, 0);
        }

        private IEnumerator StartSpeechBubble()
        {
            isBubble = true;
            _speechBubble.gameObject.SetActive(true);

            UpdateDialogue();
            

            yield return new WaitForSeconds(bubbleScripts[bubbleIndex].appearSec);
            _speechBubble.gameObject.SetActive(false);
            
            isBubble = false;
            
            yield return new WaitForSeconds(bubbleScripts[bubbleIndex].disappearSec);

            yield return new WaitWhile(() => DialogueController.instance.IsTalking);

            if (isCharacterInRange)
            {
                StartCoroutine(StartSpeechBubble());
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(gameObject.transform.position, 5);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isBubble && speechBubbleDialogueType == SpeechBubbleDialogueType.Loop || !isExcuted)
            {
                StopAllCoroutines();
                StartCoroutine(StartSpeechBubble());
                isExcuted = true;
            }

            isCharacterInRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            isCharacterInRange = false;
        }
    }
}