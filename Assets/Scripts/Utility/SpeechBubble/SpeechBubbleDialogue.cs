using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Core;
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

        private SpeechBubble _speechBubble;

        [Header("말풍선")] [Header("말풍선 대사 입력")] [SerializeField]
        public Script[] bubbleScripts;

        [Header("말풍선 위치 조절")] [SerializeField] private Vector2 speechPos;

        [SerializeField] private int _bubbleIndex;
        private bool _isBubble;
        private bool _isCharacterInRange;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
            _speechBubble =
                Instantiate(Resources.Load<GameObject>("SpeechBubble"), DataController.instance.currentMap.ui)
                    .GetComponent<SpeechBubble>();
            _speechBubble.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_isBubble)
            {
                SetSpeechBubblePosition();
            }
        }

        private void UpdateDialogue()
        {
            if (randomSecond == RandomSecond.Random)
            {
                bubbleScripts[_bubbleIndex].appearSec = Random.Range(1, bubbleScripts[_bubbleIndex].randomRange);
                bubbleScripts[_bubbleIndex].disappearSec = Random.Range(1, bubbleScripts[_bubbleIndex].randomRange);
            }
            _speechBubble.SetSpeaker(bubbleScripts[_bubbleIndex].speaker);
            _speechBubble.SetContext(bubbleScripts[_bubbleIndex].dialogue);
        }

        private void SetSpeechBubblePosition()
        {
            transform.localPosition += (Vector3)speechPos;

            Vector3 screenPoint =
                DataController.instance.cam.WorldToScreenPoint(transform.position);
            _speechBubble.transform.position = screenPoint;


            transform.localPosition -= (Vector3)speechPos;
        }

        private IEnumerator StartSpeechBubble()
        {
            _bubbleIndex %= bubbleScripts.Length;
            
            _isBubble = true;
            _speechBubble.gameObject.SetActive(true);

            UpdateDialogue();

            var bubbleScript = bubbleScripts[_bubbleIndex];

            _bubbleIndex++;

            yield return new WaitForSeconds(bubbleScript.appearSec);
            _speechBubble.gameObject.SetActive(false);

            _isBubble = false;

            yield return new WaitForSeconds(bubbleScript.disappearSec);

            yield return new WaitWhile(() => DialogueController.instance.IsTalking);

            if (_isCharacterInRange && IsBubbleEnable())
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

            _isCharacterInRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            _isCharacterInRange = false;
        }

        private bool IsBubbleEnable()
        {
            return !_isBubble && (speechBubbleType == SpeechBubbleType.Loop ||
                   (speechBubbleType == SpeechBubbleType.Once &&
                    _bubbleIndex != bubbleScripts.Length));
        }
    }
}