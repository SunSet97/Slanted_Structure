using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.Dialogue;
using Utility.UI;
using Utility.Utils;
using Random = UnityEngine.Random;

namespace Utility.Map.SpeechBubble
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
        private enum SpeechBubblePlayType
        {
            Loop,
            Once
        }

        private enum SpeechDelayType
        {
            Custom,
            Random
        }

#pragma warning disable 0649
        [FormerlySerializedAs("speechBubbleType")]
        [FormerlySerializedAs("speechBubbleDialogueType")]
        [Header("말풍선 대화 타입 선택")]
        [SerializeField]
        private SpeechBubblePlayType speechBubblePlayType;

        [FormerlySerializedAs("randomSecond")] [Header("말풍선 보이는, 안보이는 시간 타입 선택")] [SerializeField]
        private SpeechDelayType speechDelayType;

        [Header("말풍선")] [Header("말풍선 대사 입력")] [SerializeField]
        public Script[] bubbleScripts;

        [SerializeField] private bool isWorld;
        [Header("말풍선 위치 조절")] [SerializeField] private Vector2 speechPos;

        [SerializeField] private int bubbleIndex;
#pragma warning restore 0649

        private SpeechBubble speechBubble;
        private bool isCharacterInRange;

        /// <summary>
        /// 추후에 Resource말고 다른거로 가져오게 변경
        /// </summary>
        private GameObject prefab;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
            if (isWorld)
            {
                prefab = Resources.Load<GameObject>("SpeechBubbleWorld");
            }
            else
            {
                prefab = Resources.Load<GameObject>("SpeechBubbleCanvas");
            }
        }

        private void Update()
        {
            if (speechBubble)
            {
                SetSpeechBubblePosition();
            }
        }

        private void UpdateDialogue()
        {
            if (speechDelayType == SpeechDelayType.Random)
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
            var bubbleScript = bubbleScripts[bubbleIndex];

            ActiveSpeechBubble(true);

            UpdateDialogue();
            yield return new WaitForSeconds(bubbleScript.appearSec);
            ActiveSpeechBubble(false);
            bubbleIndex++;

            yield return new WaitForSeconds(bubbleScript.disappearSec);

            if (DialogueController.Instance.IsDialogue)
            {
                DialogueController.Instance.AddDialogueEndAction(() =>
                {
                    if (IsBubbleEnable())
                    {
                        StartCoroutine(StartSpeechBubble());
                    }
                });
            }
            else
            {
                if (IsBubbleEnable())
                {
                    StartCoroutine(StartSpeechBubble());
                }
            }
        }

        private void ActiveSpeechBubble(bool isActive)
        {
            if (isActive)
            {
                speechBubble = ObjectPoolHelper.Get(prefab).GetComponent<SpeechBubble>();
                if (isWorld)
                {
                    speechBubble.transform.parent = PlayUIController.Instance.worldSpaceUI;
                }
                else
                {
                    speechBubble.transform.parent = PlayUIController.Instance.mapUi;
                }

                speechBubble.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                speechBubble.gameObject.SetActive(true);
            }
            else
            {
                speechBubble.gameObject.SetActive(false);
                speechBubble.transform.parent = null;
                ObjectPoolHelper.Release(prefab, speechBubble.gameObject);
                speechBubble = null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            isCharacterInRange = true;

            if (IsBubbleEnable())
            {
                StopAllCoroutines();
                StartCoroutine(StartSpeechBubble());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            isCharacterInRange = false;
        }

        private bool IsBubbleEnable()
        {
            return isCharacterInRange && !speechBubble &&
                   (speechBubblePlayType == SpeechBubblePlayType.Loop ||
                    (speechBubblePlayType == SpeechBubblePlayType.Once &&
                     bubbleIndex != bubbleScripts.Length));
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying || !speechBubble)
                return;

            ActiveSpeechBubble(false);
        }
    }
}