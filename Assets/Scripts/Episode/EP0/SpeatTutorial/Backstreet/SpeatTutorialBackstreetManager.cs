using System;
using System.Collections;
using System.Linq;
using CommonScript;
using Play;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using static Data.CustomEnum;
using Random = UnityEngine.Random;

namespace Episode.EP0.SpeatTutorial.Backstreet
{
    public class SpeatTutorialBackstreetManager : MonoBehaviour, IGamePlayable
    {
        public bool IsPlay { get; set; }
        public Action ONEndPlay { get; set; }

        [Header("End Dialogue")] [SerializeField]
        private TextAsset jsonFile;

        [Header("#UI")] public Slider speatSlider;
        public Text endText;
        public Slider pimpSlider;
        public Text distanceText;

        [Header("#Objects")] public Transform startPosition;
        public GameObject pimp;
        public Transform[] trailer;

        [Header("#Buttons")] [SerializeField] private Button abilityButton;
        [SerializeField] private Button jumpButton;

        [SerializeField] private float runSpeed;

        private float speatDistance;
        private float pimpDistance;
        private float speatAccelator;
        private float pimpAccelator;

        private bool jumpEnable;
        private bool abilityEnable;

        private GameObject[][] patterns;
        
        private AssetBundle obstacleAssetBundle;

        private const string Path = "/AssetBundles/backstreetrun";

        private void Start()
        {
            InitRunGame();
        }

        private void InitRunGame()
        {
            obstacleAssetBundle = AssetBundle.LoadFromFile(Application.dataPath + Path);

            Debug.Log(obstacleAssetBundle == null ? "Fail to load" : "Success to load");
            var obstacleAssets = obstacleAssetBundle.LoadAllAssets<GameObject>();

            patterns = new GameObject[4][];
            for (var idx = 0; idx < patterns.Length; idx++)
            {
                var obstacles = obstacleAssets.Where(item => item.name.Substring(7, 1) == idx.ToString()).Distinct().ToArray();
                patterns[idx] = obstacles;
            }
            abilityButton.onClick.AddListener(UseAbility);
            jumpButton.onClick.AddListener(UseJump);
        }

        public void Play()
        {
            IsPlay = true;
            StartCoroutine(StartRunGame());
        }

        public void EndPlay()
        {
            IsPlay = false;

            ONEndPlay?.Invoke();

            if (jsonFile != null)
            {
                DialogueController.instance.SetDialougueEndAction(() =>
                {
                    DataController.Instance.CurrentMap.MapClear();
                });
                DialogueController.instance.StartConversation(jsonFile.text);
            }
            else
            {
                DataController.Instance.CurrentMap.MapClear();
            }
        }

        private IEnumerator StartRunGame()
        {
            CharacterManager mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
            
            JoystickController.instance.InitializeJoyStick(false);
            JoystickController.instance.inputDegree = 1;
            JoystickController.instance.inputDirection.x = 1;
            
            DataController.Instance.CurrentMap.isJoystickInputUse = false;

            mainCharacter.jumpForce = 7;
            
            while (speatSlider.value < speatSlider.maxValue)
            {
                var speatPercentage = 100f / speatSlider.maxValue;
                speatDistance = (speatSlider.maxValue - speatSlider.value) * speatPercentage; // 종료 지점과 스핏의 거리
                pimpDistance = (speatSlider.value - pimpSlider.value) * speatPercentage; // 스핏과 포주의 거리
            
                if (!pimp.activeSelf && speatSlider.value > speatSlider.maxValue * 0.1f)
                {
                    pimp.SetActive(true);
                    pimpSlider.fillRect.gameObject.SetActive(true);
                    pimpSlider.handleRect.gameObject.SetActive(true);
                }

                if (pimp.activeSelf)
                {
                    Vector3 localPosition = pimp.transform.localPosition;
                    localPosition.x = 20 - pimpDistance * 2;
                    pimp.transform.localPosition = localPosition;
                }

                // 남은 거리, 포주와의 거리 텍스트 입력
                endText.text = $"{speatDistance:0}m";
                distanceText.text = $"{pimpDistance:0}m";

                if (speatSlider.value >= speatSlider.maxValue)
                {
                    EndPlay();
                }
                else if (speatSlider.value >= 10 && speatSlider.value <= pimpSlider.value + 1)
                {
                    DataController.Instance.ChangeMap(DataController.Instance.mapCode);
                }
                
                
                
                // 스핏 달리기(장애물에 막히지 않았을 때만)
                if (startPosition.position.x < mainCharacter.transform.position.x)
                {
                    speatSlider.value += 0.8f * Time.fixedDeltaTime + speatAccelator;
                    speatAccelator += 0.0009f * Time.fixedDeltaTime;
                }
                else
                {
                    speatAccelator *= 0.97f;
                }
                
                if (pimp.activeSelf)
                {
                    pimpSlider.value += 0.85f * Time.fixedDeltaTime + pimpAccelator;
                    pimpAccelator += 0.0005f * Time.fixedDeltaTime;
                }

                if (startPosition.position.x < mainCharacter.transform.position.x)
                {
                    for (int i = 1; i < trailer.Length; i++)
                    {
                        // 트레일러 이동
                        trailer[i].position = trailer[0].position.x - trailer[i].position.x >= 18f * 2
                            ? trailer[0].position + 18.5f * 2 * Vector3.right
                            : // 앞 위치로 이동
                            trailer[i].position -
                            Vector3.right * (runSpeed + speatAccelator) * Time.fixedDeltaTime; // 뒤로 밀기
                        // 제거 및 생성
                        if (trailer[0].position.x - trailer[i].position.x >= 18f * 2)
                        {
                            Destroy(trailer[i].GetChild(0).gameObject); // 현재 장애물 패턴 제거

                            if (speatSlider.value < speatSlider.maxValue * 0.8f)
                            {
                                int index = speatDistance > 66 ? 1 : speatDistance > 33 ? 2 : 3;
                                Instantiate(patterns[index][Random.Range(0, patterns[index].Length)], trailer[i])
                                    .SetActive(true); // 장애물 패턴 랜덤 생성
                            }
                            else
                            {
                                Instantiate(patterns[0][1], trailer[i]).SetActive(true); // 장애물 없는 길 생성
                            }
                        }
                    }
                }

                yield return waitForFixedUpdate;
            }

            mainCharacter.jumpForce = 4;
            mainCharacter.UseJoystickCharacter();
        }

        private void UseJump()
        {
            if (!jumpEnable)
            {
                return;
            }

            jumpEnable = false;
            JoystickController.instance.inputJump = true;
            StartCoroutine(JumpCooldown());
        }

        private IEnumerator JumpCooldown()
        {
            var jumpImage = jumpButton.GetComponentsInChildren<Image>()[1];
            jumpImage.fillAmount = 1;
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (jumpImage.fillAmount > 0)
            {
                jumpImage.fillAmount -= Time.fixedDeltaTime * 20f;
                if (jumpImage.fillAmount < 0.9f)
                {
                    JoystickController.instance.inputJump = false;
                }

                yield return waitForFixedUpdate;
            }

            jumpEnable = true;
        }

        private void UseAbility()
        {
            if (!abilityEnable)
            {
                return;
            }

            abilityEnable = false;
            StartCoroutine(AbilityCooldown());
        }

        private IEnumerator AbilityCooldown()
        {
            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            mainCharacter.gameObject.layer = LayerMask.NameToLayer("SpeatWallPass");
            var waitForFixedUpdate = new WaitForFixedUpdate();

            var abilityImage = abilityButton.GetComponentsInChildren<Image>()[1];
            while (abilityImage.fillAmount < 1)
            {
                abilityImage.fillAmount += Time.fixedDeltaTime;
                yield return waitForFixedUpdate;
            }

            mainCharacter.gameObject.layer = 0;

            while (abilityImage.fillAmount > 0)
            {
                abilityImage.fillAmount -= Time.fixedDeltaTime;
                yield return waitForFixedUpdate;
            }

            abilityEnable = true;
        }

        private void OnDestroy()
        {
            if (obstacleAssetBundle)
            {
                obstacleAssetBundle.Unload(true);
            }
        }
    }
}
