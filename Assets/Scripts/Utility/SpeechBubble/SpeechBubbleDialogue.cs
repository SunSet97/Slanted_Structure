using System;
using System.Collections;
using UnityEngine;
using Utility.Property;
using Utility.System;
using Random = UnityEngine.Random;

namespace Utility.SpeechBubble
{
    [Serializable]
    public struct Script
    {
        public string speaker;
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

        [ConditionalHideInInspector("randomSecond", RandomSecond.Custom)] [SerializeField]
        private float visibleSecond = 5;

        [ConditionalHideInInspector("randomSecond", RandomSecond.Custom)] [SerializeField]
        private float invisibleSecond = 5;

        [ConditionalHideInInspector("randomSecond", RandomSecond.Random)] [SerializeField]
        private float randomRange = 5;

        [Header("말풍선")] [SerializeField] private SpeechBubble speechBubble;

        [Header("말풍선 대사 입력")] [SerializeField] public Script[] bubbleScripts;

        [Header("말풍선 위치 조절")] [SerializeField] private Vector2 speechPos;

        private int bubbleIndex;
        private bool isExcuted;
        private bool isBubble;
        private bool isCharacterInRange;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
            speechBubble.gameObject.SetActive(false);
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
            speechBubble.SetSpeaker(bubbleScripts[bubbleIndex].speaker);
            speechBubble.SetContext(bubbleScripts[bubbleIndex].dialogue);
            bubbleIndex = (bubbleIndex + 1) % bubbleScripts.Length;
        }

        private void SetSpeechBubblePosition()
        {
            Vector3 screenPoint = DataController.instance.cam.WorldToScreenPoint(transform.position);
            speechBubble.transform.position = screenPoint + new Vector3(speechPos.x, speechPos.y, 0);
        }

        private IEnumerator StartSpeechBubble()
        {
            isBubble = true;
            speechBubble.gameObject.SetActive(true);

            UpdateDialogue();
            if (randomSecond == RandomSecond.Random)
            {
                visibleSecond = Random.Range(1, randomRange);
                invisibleSecond = Random.Range(1, randomRange);
            }

            yield return new WaitForSeconds(visibleSecond);
            speechBubble.gameObject.SetActive(false);
            
            isBubble = false;
            
            yield return new WaitForSeconds(invisibleSecond);

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