using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
using Utility.Character;
using Utility.Core;
using Utility.Dialogue;
using Utility.Game;
using Utility.Interaction.Click;
using Utility.Map;
using Utility.Preference;
using Utility.Utils;
using Utility.Utils.Json;
using Utility.Utils.Property;
using Task = Utility.Dialogue.Task;

namespace Utility.Interaction
{
    [ExecuteInEditMode]
    public class InteractionObject : MonoBehaviour, IClickable
    {
#pragma warning disable 0649
        public string id;
        [FormerlySerializedAs("interactions")] public List<InteractionData> interactionData;

        [Header("아웃라인 설정")] [Space(20)] [SerializeField]
        private bool useOutline;

        [ConditionalHideInInspector("useOutline")] [SerializeField]
        private OutlineColor color;

        [ConditionalHideInInspector("useOutline")] [SerializeField]
        private Outline outline;

        [Header("기즈모")] [Space(20)] [SerializeField]
        private bool useGizmos;

        [ConditionalHideInInspector("useGizmos")] [SerializeField]
        private int gizmosRadius = 5;

        [ConditionalHideInInspector("useGizmos")] [SerializeField]
        private Color gizmosColor;

        [NonSerialized] public GameObject ExclamationMark;

        [NonSerialized] public int InteractIndex;
#pragma warning restore 0649
        
        private void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            DataController.Instance.AddInteractor(this);

            if (GetInteractionData().interactionMethod == InteractionMethod.OnChangeMap)
            {
                Debug.Log("Add OnChange Map");
                DataController.Instance.AddOnLoadMap(StartInteraction);
            }
        }

        protected virtual void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (interactionData != null && interactionData.Count > 0)
                {
                    foreach (var interaction in interactionData)
                    {
                        if (interaction.interactionPlayType == InteractionPlayType.Dialogue)
                        {
                            interaction.dialogueData = new DialogueData();
                            interaction.dialogueData.Init(interaction.jsonFile.text);
                        }
                        else if (interaction.interactionPlayType == InteractionPlayType.Task)
                        {
                            interaction.debugTaskData = LoadTaskData();
                        }
                    }
                }
            }
            else
#endif
            {
                gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
                if (interactionData != null)
                {
                    var t = interactionData.Where(interaction => Mathf.Approximately(interaction.dialoguePrintSec, 0))
                        .ToArray();
                    foreach (var interaction in t)
                    {
                        interaction.dialoguePrintSec = DataController.Instance.dialoguePrintSec;
                    }
                }
            }

            if (useOutline)
            {
                if (TryGetComponent(out Outline t))
                {
                    outline = t;
                }
                else
                {
                    outline = gameObject.AddComponent<Outline>();
                }

                outline.OutlineMode = Outline.Mode.OutlineAll;
                outline.OutlineWidth = 8f;
                outline.enabled = false;

                if (color == OutlineColor.Red) outline.OutlineColor = Color.red;
                else if (color == OutlineColor.Magenta) outline.OutlineColor = Color.magenta;
                else if (color == OutlineColor.Yellow) outline.OutlineColor = Color.yellow;
                else if (color == OutlineColor.Green) outline.OutlineColor = Color.green;
                else if (color == OutlineColor.Blue) outline.OutlineColor = Color.blue;
                else if (color == OutlineColor.Grey) outline.OutlineColor = Color.grey;
                else if (color == OutlineColor.Black) outline.OutlineColor = Color.black;
                else if (color == OutlineColor.White) outline.OutlineColor = Color.white;
            }
        }
        
        private void StackNewTask(string jsonRoute)
        {
            DialogueController.Instance.debugTaskData = null;
            GetInteractionData().serializedInteractionData.JsonTask = new Stack<TaskData>();
            GC.Collect();
            PushTask(jsonRoute);
            StopAllCoroutines();
            StartInteraction();
        }

        private void PushTask(string jsonString)
        {
            Debug.Log("Push Task");
            var taskData = new TaskData
            {
                tasks = JsontoString.FromJsonList<Task>(jsonString)
            };
            var interactionDatum = GetInteractionData();
            interactionDatum.serializedInteractionData.JsonTask.Push(taskData);
            DialogueController.Instance.debugTaskData = taskData;
        }

        /// <summary>
        /// 인터랙션을 처음으로 실행할 때 (ex - 터치)
        /// </summary>
        public void StartInteraction()
        {
            var interactionDatum = GetInteractionData();
            Debug.Log($"Start Interaction {gameObject.name}, {InteractIndex}, {interactionDatum.interactionPlayType}");

            if (interactionDatum.isWait && interactionDatum.waitInteractionData.waitInteractions.Any(waitInteraction =>
                    !waitInteraction.waitInteraction.GetInteractionData(waitInteraction.interactionIndex)
                        .serializedInteractionData.isInteracted))
            {
                return;
            }

            if (!interactionDatum.serializedInteractionData.isInteractable ||
                interactionDatum.serializedInteractionData.isInteracted && !interactionDatum.isLoop)
            {
                Debug.Log($"인터랙션 시작 전 중지\n" +
                          $"isInteractable - {interactionDatum.serializedInteractionData.isInteractable}\n" +
                          $"isInteracted - {interactionDatum.serializedInteractionData.isInteracted}\n" +
                          $"isLoop - {interactionDatum.isLoop}");
                return;
            }

            interactionDatum.StartAction(transform);

            switch (interactionDatum.interactionPlayType)
            {
                case InteractionPlayType.Animation:
                    Debug.Log("애니메이션 스타트");
                    interactionDatum.animator.SetTrigger("Start");
                    StartCoroutine(WaitAnimationEnd(interactionDatum));
                    break;
                case InteractionPlayType.Dialogue:
                {
                    if (!interactionDatum.jsonFile)
                    {
                        Debug.LogError("오류");
                    }

                    DialogueController.Instance.AddDialogueEndAction(() => { EndInteraction(interactionDatum); });
                    DialogueController.Instance.StartConversation(interactionDatum.jsonFile.text,
                        interactionDatum.dialoguePrintSec);
                    break;
                }
                case InteractionPlayType.Portal:
                    if (!gameObject.TryGetComponent(out CheckMapClear mapClear))
                    {
                        Debug.LogError("포탈 오류");
                        return;
                    }

                    EndInteraction(interactionDatum);
                    mapClear.Clear();
                    break;
                case InteractionPlayType.Task:
                {
                    if (interactionDatum.jsonFile && interactionDatum.serializedInteractionData.JsonTask == null)
                    {
                        interactionDatum.serializedInteractionData.JsonTask = new Stack<TaskData>();
                        PushTask(interactionDatum.jsonFile.text);
                    }

                    if (interactionDatum.serializedInteractionData.JsonTask == null ||
                        interactionDatum.serializedInteractionData.JsonTask.Count == 0)
                    {
                        Debug.LogError("task파일 없음 오류오류");
                    }

                    StartTaskAction(interactionDatum);
                    break;
                }
                case InteractionPlayType.Game:
                    interactionDatum.miniGame.OnEndPlay = isSuccess => { EndInteraction(interactionDatum, isSuccess); };
                    interactionDatum.miniGame.Play();
                    break;
                case InteractionPlayType.Cinematic:
                {
                    if (!interactionDatum.playableDirector)
                    {
                        Debug.LogError("타임라인 세팅 오류");
                        return;
                    }

                    BindPlayableDirector(interactionDatum.playableDirector);

                    PlayUIController.Instance.SetMenuActive(false);
                    // DataController.Instance.GetCharacter(Character.Main)?.PickUpCharacter();
                    JoystickController.Instance.StopSaveLoadJoyStick(true);
                    foreach (var interactionInGame in interactionDatum.inGames)
                    {
                        interactionInGame.SetActive(false);
                    }

                    foreach (var interactionCinematic in interactionDatum.cinematics)
                    {
                        interactionCinematic.SetActive(true);
                    }
                    
                    interactionDatum.playableDirector.Play();

                    StartCoroutine(WaitCinematicEnd(interactionDatum, () =>
                    {
                        JoystickController.Instance.StopSaveLoadJoyStick(false);
                        DataController.Instance.GetCharacter(CharacterType.Main)?.UseJoystickCharacter();
                        PlayUIController.Instance.SetMenuActive(true);
                        Debug.Log("타임라인 끝");
                        
                        foreach (var interactionInGame in interactionDatum.inGames)
                        {
                            interactionInGame.SetActive(true);
                        }

                        foreach (var interactionCinematic in interactionDatum.cinematics)
                        {
                            interactionCinematic.SetActive(false);
                        }

                        EndInteraction(interactionDatum);
                    }));

                    return;
                }
            }
        }
        
        private void StartTaskAction(InteractionData interactionDatum)
        {
            var taskData = interactionDatum.serializedInteractionData.JsonTask.Peek();

            // If (!Task End)
            if (taskData.tasks.Count > taskData.taskIndex &&
                taskData.tasks[taskData.taskIndex].order.Equals(taskData.taskOrder))
            {
                DialogueController.Instance.debugTaskData = taskData;

                var currentTask = taskData.tasks[taskData.taskIndex];
                Debug.Log("Task 실행\n" +
                          $"Task 길이: {taskData.tasks.Count}\n" +
                          $"TaskContentType - {string.Join(", ", taskData.tasks.Select(item => item.taskContentType))}\n" +
                          $"taskIndex - {taskData.taskIndex}\n" +
                          $"InteractionType - {currentTask.taskContentType}");

                switch (currentTask.taskContentType)
                {
                    case TaskContentType.Dialogue:
                        var path = currentTask.nextFile;
                        var jsonString = DialogueController.ConvertPathToJson(path);
                        Debug.Log($"대화 파일 경로 - {path}");
                        DialogueController.Instance.AddDialogueEndAction(() =>
                        {
                            taskData.taskIndex++;
                            StartTaskAction(interactionDatum);
                        });
                        DialogueController.Instance.StartConversation(jsonString);
                        break;
                    case TaskContentType.Animation:
                        //세팅된 애니메이션 실행
                        interactionDatum.animator.SetTrigger("Start");
                        Debug.Log("애니메이션 스타트");
                        StartCoroutine(WaitAnimationEnd(interactionDatum));
                        taskData.taskIndex++;
                        StartTaskAction(interactionDatum);
                        break;
                    case TaskContentType.Play:
                        var gamePlayable = GameObject.Find(currentTask.nextFile).GetComponent<MiniGame>();
                        gamePlayable.OnEndPlay = isSuccess =>
                        {
                            PlayUIController.Instance.SetMenuActive(true);
                            taskData.taskIndex++;
                            StartTaskAction(interactionDatum);
                        };

                        PlayUIController.Instance.SetMenuActive(false);
                        gamePlayable.Play();
                        break;
                    case TaskContentType.Choice:
                        Debug.Log("선택지 열기");
                        DialogueController.Instance.SetChoiceAction(ExecuteChoiceAction);
                        DialogueController.Instance.OpenChoicePanel(taskData);
                        break;
                    case TaskContentType.ChoiceDialogue:
                        //선택지에서 대화를 고른 경우
                        Debug.Log("선택지 선택 - 단순 대화");
                        interactionDatum.serializedInteractionData.JsonTask.Peek().taskIndex++;
                        StartTaskAction(interactionDatum);
                        break;
                    case TaskContentType.ImmediateChoice:
                        ExecuteChoiceByRelationship();
                        break;
                    case TaskContentType.DialogueElement:
                        // asdad
                        // 대화 실행
                        break;
                    // TaskEnd 보다는 TaskReset이라는 말이 어울린다
                    // 애매하네
                    //TaskEnd, TaskReset - TaskEnd를 할 때 Task를 재사용 가능하도록 하냐 Task를 재사용하지 못하도록 하냐..
                    case TaskContentType.TaskReset:
                        if (interactionDatum.interactionMethod == InteractionMethod.Touch)
                        {
                            interactionDatum.serializedInteractionData.isInteracted = false;
                        }

                        interactionDatum.serializedInteractionData.JsonTask.Pop();
                        if (interactionDatum.serializedInteractionData.JsonTask.Count > 0)
                        {
                            Debug.LogError("Task 엑셀 관련 오류");
                        }

                        break;
                    case TaskContentType.New:
                    {
                        StackNewTask(currentTask.nextFile);
                        break;
                    }
                    case TaskContentType.FadeOut:
                    {
                        FadeEffect.Instance.OnFadeOver += () =>
                        {
                            taskData.taskIndex++;
                            StartTaskAction(interactionDatum);
                        };
                        FadeEffect.Instance.FadeOut();
                        break;
                    }
                    case TaskContentType.FadeIn:
                    {
                        FadeEffect.Instance.OnFadeOver += () =>
                        {
                            taskData.taskIndex++;
                            StartTaskAction(interactionDatum);
                        };
                        FadeEffect.Instance.FadeIn();
                        break;
                    }
                    case TaskContentType.TheEnd:
                        //게임 엔딩
                        if (int.TryParse(currentTask.nextFile, out var endingIndex))
                        {
                            EndingHelper.Instance.StartEnd(endingIndex);
                        }
                        else
                        {
                            Debug.LogError($"엔딩 세팅 오류, {currentTask.nextFile}을 변경해주세요.");
                        }

                        break;
                    case TaskContentType.Timeline:
                        var episodeIndex = int.Parse(currentTask.nextFile.Substring(0, 1));
                        var timelineAsset = Resources.Load<TimelineAsset>($"Timeline/Episode {episodeIndex}/{currentTask.nextFile}");
                        
                        if(timelineAsset == null)
                        {
                            Debug.LogError("타임라인 오류");
                            return;
                        }
                        
                        DialogueController.Instance.playableDirector.playableAsset = timelineAsset;

                        BindPlayableDirector(DialogueController.Instance.playableDirector);

                        foreach (var interactionInGame in interactionDatum.inGames)
                        {
                            interactionInGame.SetActive(false);
                        }

                        foreach (var interactionCinematic in interactionDatum.cinematics)
                        {
                            interactionCinematic.SetActive(true);
                        }
                        
                        DialogueController.Instance.playableDirector.Play();

                        JoystickController.Instance.StopSaveLoadJoyStick(true);
                        // DataController.Instance.GetCharacter(Character.Main)?.PickUpCharacter();
                        PlayUIController.Instance.SetMenuActive(false);

                        StartCoroutine(WaitTimelineEnd(DialogueController.Instance.playableDirector, () =>
                        {
                            PlayUIController.Instance.SetMenuActive(true);
                            JoystickController.Instance.StopSaveLoadJoyStick(false);
                            DataController.Instance.GetCharacter(CharacterType.Main)?.UseJoystickCharacter();

                            foreach (var cinematic in interactionDatum.cinematics)
                            {
                                cinematic.SetActive(false);
                            }

                            foreach (var inGame in interactionDatum.inGames)
                            {
                                inGame.SetActive(true);
                            }
                            
                            interactionDatum.serializedInteractionData.JsonTask.Peek().taskIndex++;
                            StartTaskAction(interactionDatum);
                        }));

                        break;
                    case TaskContentType.ClearMap:
                        DataController.Instance.CurrentMap.ClearMap();
                        break;
                    default:
                    {
                        Debug.LogError($"{currentTask.taskContentType}은 존재하지 않는 type입니다.");
                        break;
                    }
                }

                Debug.Log("Task Action 종료 후 대기 중 - " + currentTask.taskContentType + ", Index - " + taskData.taskIndex);
            }
            else
            {
                Debug.LogWarning($"Task 종료, Order - {taskData.taskOrder}");
                taskData.taskOrder++;

                if (taskData.tasks.Count == taskData.taskIndex)
                {
                    DialogueController.Instance.debugTaskData = null;
                    interactionDatum.serializedInteractionData.JsonTask.Pop();

                    if (interactionDatum.serializedInteractionData.JsonTask.Count > 1)
                    {
                        interactionDatum.serializedInteractionData.JsonTask.Peek().taskIndex++;
                        StartTaskAction(interactionDatum);
                    }
                    else
                    {
                        EndInteraction(interactionDatum);
                    }
                }
            }
        }

        private void EndInteraction(InteractionData interactionDatum, bool isSuccess = false)
        {
            if (interactionDatum.isNextInteractable)
            {
                InteractIndex = (InteractIndex + 1) % interactionData.Count;
            }

            interactionDatum.EndAction(isSuccess);

            if (interactionDatum.isNextInteract)
            {
                StartInteraction();
            } 
            else if (GetInteractionData().interactionMethod != InteractionMethod.Trigger)
            {
                OnColEnter();
            }
        }

        /// <summary>
        /// 선택지를 눌렀을 때 부르는 함수, Task로 사용한다
        /// </summary>
        /// <param name="index">선택지 번호, 1번부터 시작</param>
        private void ExecuteChoiceAction(int index)
        {
            var interaction = GetInteractionData();
            var taskData = interaction.serializedInteractionData.JsonTask.Peek();
            var originTaskIndex = taskData.taskIndex;
            var choiceTargetIndex = originTaskIndex + index;
            var curTask = taskData.tasks[choiceTargetIndex];

            // taskIndex 변화
            var choiceLen = int.Parse(taskData.tasks[originTaskIndex].nextFile);
            taskData.taskIndex += choiceLen;
            Debug.Log($"next taskIndex - {taskData.taskIndex}");

            // 성향 변화
            curTask.increaseVar = curTask.increaseVar.Replace("m", "-");
            var changeVal = Array.ConvertAll(curTask.increaseVar.Split(','), int.Parse);
            DataController.Instance.UpdateLikeable(changeVal);

            //다음 맵 코드 변경
            if (curTask.order != 0)
            {
                DataController.Instance.CurrentMap.SetClearMapCode($"{curTask.order,000000}");
            }

            ChoiceAction(interaction, choiceTargetIndex);
        }

        /// <summary>
        /// 관계값에 따라 ChoiceEvent 즉각 실행
        /// </summary>
        private void ExecuteChoiceByRelationship()
        {
            var interaction = GetInteractionData();
            var currentTaskData = interaction.serializedInteractionData.JsonTask.Peek();
            var originTaskIndex = currentTaskData.taskIndex;
            var taskCount = int.Parse(currentTaskData.tasks[originTaskIndex].nextFile);

            if (currentTaskData.tasks.Count <= originTaskIndex + taskCount)
            {
                Debug.LogError("즉시 실행 개수 오류 - IndexOverFlow");
            }

            Debug.Log(taskCount);
            currentTaskData.taskIndex += taskCount;
            Debug.Log(currentTaskData.taskIndex);

            var likeable = DataController.Instance.GetRelationshipData();

            for (var index = 0; index < taskCount; index++)
            {
                var targetTaskIndex = originTaskIndex + index;
                var targetTask = currentTaskData.tasks[targetTaskIndex];

                //변화값
                targetTask.condition = targetTask.increaseVar.Replace("m", "-");
                Debug.Log($"{index}번째 선택지 조건 - {targetTask.condition}");
                var condition = Array.ConvertAll(targetTask.condition.Split(','), int.Parse);


                if (targetTask.order >= 0)
                {
                    if (condition[0] < likeable[0] || condition[1] < likeable[1] || condition[2] < likeable[2])
                    {
                        continue;
                    }
                }
                else
                {
                    if (condition[0] > likeable[0] || condition[1] > likeable[1] || condition[2] > likeable[2])
                    {
                        continue;
                    }
                }

                ChoiceAction(interaction, targetTaskIndex);
            }

            Debug.LogError($"관계도에 따른 선택지 실행 안됨");
        }

        private void ChoiceAction(InteractionData interactionDatum, int targetTaskIndex)
        {
            var taskData = interactionDatum.serializedInteractionData.JsonTask.Peek();
            var targetTask = taskData.tasks[targetTaskIndex];

            Debug.Log($"선택지 실행 - {targetTask.taskContentType}");
            switch (targetTask.taskContentType)
            {
                // 조건에 따라 바로 실행하기
                case TaskContentType.Dialogue:
                    taskData.tasks.Insert(taskData.taskIndex + 1, new Task
                    {
                        taskContentType = TaskContentType.ChoiceDialogue,
                        order = taskData.taskOrder
                    });
                    taskData.taskIndex++;

                    var jsonString = DialogueController.ConvertPathToJson(targetTask.nextFile);

                    DialogueController.Instance.AddDialogueEndAction(() =>
                    {
                        taskData.taskIndex++;
                        StartTaskAction(interactionDatum);
                    });
                    DialogueController.Instance.StartConversation(jsonString);
                    //다음 인덱스의 타입
                    break;
                case TaskContentType.Choice:
                    //새로운 task 실행
                    jsonString = DialogueController.ConvertPathToJson(targetTask.nextFile);
                    PushTask(jsonString);
                    StartInteraction();
                    break;
                case TaskContentType.New:
                    Debug.LogWarning("사용하지 마세요");
                    StackNewTask(targetTask.nextFile);
                    break;
            }
        }

        private static IEnumerator WaitCinematicEnd(InteractionData interactionDatum, Action onEndAction)
        {
            yield return new WaitUntil(() =>
            {
                var playableDirector = interactionDatum.playableDirector;

                return Math.Abs(playableDirector.time - playableDirector.duration) <=
                       1 / ((TimelineAsset)playableDirector.playableAsset).editorSettings.fps ||
                       playableDirector.state == PlayState.Paused && !playableDirector.playableGraph.IsValid() &&
                       !DialogueController.Instance.IsDialogue;
            });

            onEndAction?.Invoke();
        }

        private static IEnumerator WaitTimelineEnd(PlayableDirector playableDirector, Action onEndAction)
        {
            yield return new WaitUntil(() => Math.Abs(playableDirector.time - playableDirector.duration) <=
                                             1 / ((TimelineAsset)playableDirector.playableAsset).editorSettings.fps ||
                                             playableDirector.state == PlayState.Paused &&
                                             !playableDirector.playableGraph.IsValid() &&
                                             !DialogueController.Instance.IsDialogue);

            onEndAction?.Invoke();
        }

        private IEnumerator WaitAnimationEnd(InteractionData interactionDatum)
        {
            yield return new WaitUntil(() => interactionDatum.animator.GetCurrentAnimatorStateInfo(0).IsName("Finish"));
            Debug.Log("애니메이션 종료");
            var clipInfos = interactionDatum.animator.GetCurrentAnimatorClipInfo(0);
            var alreadyDone = clipInfos.Any(item =>
            {
                return item.clip.events.Any(t => t.functionName == nameof(EndAnimation));
            });
            if (alreadyDone)
            {
                yield break;
            }

            foreach (var animatorClipInfo in clipInfos)
            {
                var animationEvent = new AnimationEvent
                {
                    time = animatorClipInfo.clip.length,
                    intParameter = interactionData.FindIndex(item => item == interactionDatum),
                    functionName = nameof(EndAnimation)
                };
                animatorClipInfo.clip.AddEvent(animationEvent);
            }
        }

        /// <summary>
        /// Use by AnimationEvent
        /// </summary>
        /// <param name="interactionIndex"></param>
        public void EndAnimation(int interactionIndex)
        {
            Debug.Log("실행");
            EndInteraction(GetInteractionData(interactionIndex));
        }

        /// <summary>
        /// Dialogue가 시작할 때 사용하는 1회성 Event, Task - Dialogue인 경우에 실행되지 않는다.
        /// </summary>
        /// <param name="unityAction">사용할 함수를 만들어서 넣으세요</param>
        public void SetInteractionStartEvent(UnityAction unityAction, int index = -1)
        {
            if (index != -1)
            {
                GetInteractionData(index).interactionStartActions.AddInteraction(unityAction);
            }
            else
            {
                GetInteractionData().interactionStartActions.AddInteraction(unityAction);
            }
        }

        /// <summary>
        /// Dialogue가 끝날 때 사용하는 1회성 Event, Task - Dialogue인 경우에 실행되지 않는다.
        /// </summary>
        /// <param name="unityAction">사용할 함수를 만들어서 넣으세요</param>
        public void SetInteractionEndEvent(UnityAction unityAction, int index = -1)
        {
            if (index != -1)
            {
                GetInteractionData(index).interactionEndActions.AddInteraction(unityAction);
            }
            else
            {
                GetInteractionData().interactionEndActions.AddInteraction(unityAction);
            }
        }

        private static void BindPlayableDirector(PlayableDirector playableDirector)
        {
            if (playableDirector == null)
            {
                Debug.LogError($"PlayableDirector 오류 - {playableDirector}");
                return;
            }
            
            var timelineAsset = playableDirector.playableAsset as TimelineAsset;
            if (timelineAsset == null)
            {
                Debug.LogError($"타임라인 세팅 오류 {playableDirector}, {playableDirector.playableAsset}");
                return;
            }

            var trackAssets = timelineAsset.GetOutputTracks().ToList();
            while (trackAssets.Count > 0)
            {
                var trackAsset = trackAssets.First();

                var binding = playableDirector.GetGenericBinding(trackAsset);
                if (binding == null)
                {
                    if (trackAsset is CinemachineTrack)
                    {
                        playableDirector.SetGenericBinding(trackAsset,
                            DataController.Instance.Cam.GetComponent<CinemachineBrain>());
                    }
                    else if (trackAsset is AnimationTrack)
                    {
                        playableDirector.SetGenericBinding(trackAsset,
                            DialogueController.Instance.dialogueBindAnimator);
                    }   
                }

                if (trackAsset.isSubTrack)
                {
                    trackAssets.AddRange(trackAsset.GetChildTracks());
                }

                trackAssets.Remove(trackAsset);
            }
        }

        public void LoadData(InteractionSaveData saveData)
        {
            transform.position = saveData.pos;
            transform.rotation = saveData.rot;
            InteractIndex = saveData.interactIndex;

            foreach (var interaction in interactionData)
            {
                var serializedData = saveData.serializedInteractionData.Find(item =>
                    item.id == interaction.serializedInteractionData.id);

                interaction.serializedInteractionData = serializedData;
            }
        }

        public InteractionData GetInteractionData(int index)
        {
            if (interactionData == null || interactionData.Count <= index)
            {
                Debug.LogError("인터랙션 데이터 설정 오류");
                return null;
            }

            return interactionData[index];
        }

        public InteractionData GetInteractionData()
        {
            if (interactionData == null || interactionData.Count == 0 || interactionData.Count <= InteractIndex)
            {
                Debug.LogError("인터랙션 데이터 설정 오류");
                return null;
            }

            return interactionData[InteractIndex];
        }

        public InteractionSaveData GetInteractionSaveData()
        {
            var interactionSaveData = new InteractionSaveData
            {
                id = id,
                pos = transform.position,
                rot = transform.rotation,
                interactIndex = InteractIndex,
                serializedInteractionData = new List<SerializedInteractionData>()
            };

            for (var index = 0; index < interactionData.Count; index++)
            {
                var interaction = interactionData[index];
                interaction.serializedInteractionData.id = index;
                interactionSaveData.serializedInteractionData.Add(interaction.serializedInteractionData);
            }

            return interactionSaveData;
        }

        bool IClickable.IsClickEnable
        {
            get
            {
                var interactionDatum = GetInteractionData();
                if ((interactionDatum.interactionPlayType == InteractionPlayType.Dialogue ||
                     interactionDatum.interactionPlayType == InteractionPlayType.Task) && !interactionDatum.jsonFile)
                {
                    Debug.LogWarning("Json 세팅 오류");
                    return false;
                }

                Debug.Log($"Interactable: {interactionDatum.serializedInteractionData.isInteractable}\n" +
                          $"Enable:  {enabled}\n" +
                          $"Method: {interactionDatum.interactionMethod}\n" +
                          $"Is Interacted: {interactionDatum.serializedInteractionData.isInteracted}");

                if (interactionDatum.isWait && interactionDatum.waitInteractionData.waitInteractions.Any(item =>
                        !item.waitInteraction.GetInteractionData(item.interactionIndex).serializedInteractionData
                            .isInteracted))
                {
                    return false;
                }

                return interactionDatum.serializedInteractionData.isInteractable && enabled &&
                       interactionDatum.interactionMethod == InteractionMethod.Touch &&
                       (!interactionDatum.serializedInteractionData.isInteracted || interactionDatum.isLoop);
            }
            set
            {
                if (value)
                {
                    var interactionDatum = GetInteractionData();
                    interactionDatum.interactionMethod = InteractionMethod.Touch;
                    interactionDatum.serializedInteractionData.isInteracted = false;
                    interactionDatum.serializedInteractionData.isInteractable = true;
                }
                else
                {
                    var interactionDatum = GetInteractionData();
                    interactionDatum.interactionMethod = InteractionMethod.No;
                }
            }
        }

        bool IClickable.IsClicked
        {
            get
            {
                var interactionDatum = GetInteractionData();
                return interactionDatum.serializedInteractionData.isInteracted;
            }
            set
            {
                var interactionDatum = GetInteractionData();
                interactionDatum.serializedInteractionData.isInteracted = value;
            }
        }

        void IClickable.ActiveObjectClicker(bool isActive)
        {
            if (useOutline && outline)
            {
                outline.enabled = isActive;
            }

            Debug.Log(ExclamationMark);
            if (ExclamationMark)
            {
                ExclamationMark.SetActive(isActive);
            }

            if (GetInteractionData().interactionMethod == InteractionMethod.Touch)
            {
                ObjectClicker.Instance.UpdateClick(this, isActive);
            }
        }

        bool IClickable.GetIsClicked()
        {
            var interactionDatum = GetInteractionData();
            return interactionDatum.serializedInteractionData.isInteracted;
        }

        void IClickable.Click()
        {
            if (!((IClickable)this).IsClickEnable)
            {
                return;
            }

            ((IClickable)this).ActiveObjectClicker(false);

            StartInteraction();
        }

        public void OnColEnter()
        {
            Debug.Log("Trigger Enter");
            if (((IClickable)this).IsClickEnable)
            {
                ((IClickable)this).ActiveObjectClicker(true);
            }

            if (GetInteractionData().interactionMethod == InteractionMethod.Trigger)
            {
                StartInteraction();
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying || !ObjectClicker.Instance)
            {
                return;
            }

            ObjectClicker.Instance.UpdateClick(this, false);
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying || !ObjectClicker.Instance)
            {
                return;
            }

            ObjectClicker.Instance.UpdateClick(this, false);
        }

        public void OnExit()
        {
            Debug.Log("Exit");
            if (((IClickable)this).IsClickEnable)
            {
                ((IClickable)this).ActiveObjectClicker(false);
            }
        }

        private void OnDrawGizmos()
        {
            if (!useGizmos)
            {
                return;
            }

            Gizmos.color = gizmosColor;
            Gizmos.DrawWireSphere(transform.position, gizmosRadius);
        }

#if UNITY_EDITOR
        //For Debugging
        private List<TaskData> LoadTaskData()
        {
            var taskDataDebug = new List<TaskData>
            {
                new TaskData
                {
                    tasks = JsontoString.FromJsonList<Task>(GetInteractionData().jsonFile.text)
                }
            };

            return taskDataDebug;
        }
#endif
    }
}