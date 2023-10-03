using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.Preference;
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
        private enum SpeechBubbleType
        {
            Loop,
            Once
        }

        private enum RandomSecond
        {
            Custom,
            Random
        }

        [FormerlySerializedAs("speechBubbleDialogueType")] [Header("말풍선 대화 타입 선택")] [SerializeField]
        private SpeechBubbleType speechBubbleType;

        [Header("말풍선 보이는, 안보이는 시간 타입 선택")] [SerializeField]
        private RandomSecond randomSecond;

        private SpeechBubble speechBubble;

        [Header("말풍선")] [Header("말풍선 대사 입력")] [SerializeField]
        public Script[] bubbleScripts;

        [SerializeField] private bool isWorld;
        [Header("말풍선 위치 조절")] [SerializeField] private Vector2 speechPos;

        [SerializeField] private int bubbleIndex;
        private bool isBubble;
        private bool isCharacterInRange;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
            if (isWorld)
            {
                speechBubble =
                    Instantiate(Resources.Load<GameObject>("SpeechBubbleWorld"), PlayUIController.Instance.worldSpaceUI)
                        .GetComponent<SpeechBubble>();
            }
            else
            {
                speechBubble =
                    Instantiate(Resources.Load<GameObject>("SpeechBubbleCanvas"), PlayUIController.Instance.mapUi)
                        .GetComponent<SpeechBubble>();
            }

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
            if (randomSecond == RandomSecond.Random)
            {
                bubbleScripts[bubbleIndex].appearSec = Random.Range(1, bubbleScripts[bubbleIndex].randomRange);
                bubbleScripts[bubbleIndex].disappearSec = Random.Range(1, bubbleScripts[bubbleIndex].randomRange);
            }

            speechBubble.SetSpeaker(bubbleScripts[bubbleIndex].speaker);
            speechBubble.SetContext(bubbleScripts[bubbleIndex].dialogue);
        }

        private void SetSpeechBubblePosition()
        {
            if (isWorld)
            {
                speechBubble.transform.position = (Vector3)speechPos + transform.position;
            }
            else
            {
                var screenPoint =
                    DataController.Instance.Cam.WorldToScreenPoint(transform.position + (Vector3)speechPos);
                speechBubble.transform.position = screenPoint;
            }
        }

        private IEnumerator StartSpeechBubble()
        {
            bubbleIndex %= bubbleScripts.Length;

            isBubble = true;
            speechBubble.gameObject.SetActive(true);

            UpdateDialogue();

            var bubbleScript = bubbleScripts[bubbleIndex];

            bubbleIndex++;

            yield return new WaitForSeconds(bubbleScript.appearSec);
            speechBubble.gameObject.SetActive(false);

            isBubble = false;

            yield return new WaitForSeconds(bubbleScript.disappearSec);

            yield return new WaitWhile(() => DialogueController.Instance.IsDialogue);

            if (isCharacterInRange && IsBubbleEnable())
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
            if (IsBubbleEnable())
            {
                StopAllCoroutines();
                StartCoroutine(StartSpeechBubble());
            }

            isCharacterInRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            isCharacterInRange = false;
        }

        private bool IsBubbleEnable()
        {
            return !isBubble && (speechBubbleType == SpeechBubbleType.Loop ||
                                 (speechBubbleType == SpeechBubbleType.Once &&
                                  bubbleIndex != bubbleScripts.Length));
        }

        private void OnDestroy()
        {
            if (speechBubble.gameObject)
            {
                Destroy(speechBubble.gameObject);
            }
        }
    }
}