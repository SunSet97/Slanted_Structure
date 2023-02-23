using System;
using System.Collections;
using System.Linq;
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
        public Action OnEndPlay { get; set; }

        [Header("End Dialogue")] [SerializeField]
        private TextAsset jsonFile;

        [Header("#UI")] public Slider speatSlider;
        public Text remainingDistanceText;
        public Slider pimpSlider;
        public Text pimpDistanceText;

        [Header("#Objects")] public Transform startPosition;
        public GameObject pimp;
        public Transform[] trailer;

        [Header("#Buttons")] [SerializeField] private Button abilityButton;
        [SerializeField] private Button jumpButton;

        [SerializeField] private float runSpeed;
        [SerializeField] private float jumpCooldownSec = 0.05f;
        
        private float remainingDistance;
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
            Initialize();
        }

        private void Initialize()
        {
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(false);
            
            obstacleAssetBundle = AssetBundle.LoadFromFile(Application.dataPath + Path);

            Debug.Log(obstacleAssetBundle == null ? "Fail to load" : "Success to load");
            var obstacleAssets = obstacleAssetBundle.LoadAllAssets<GameObject>();

            speatSlider.value = 0f;
            pimpSlider.value = 0f;

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
            StartCoroutine(WaitPimp());
            StartCoroutine(StartRunGame());
        }

        public void EndPlay()
        {
            IsPlay = false;

            OnEndPlay?.Invoke();

            if (jsonFile != null)
            {
                DialogueController.Instance.SetDialougueEndAction(() =>
                {
                    DataController.Instance.CurrentMap.MapClear();
                });
                DialogueController.Instance.StartConversation(jsonFile.text);
            }
            else
            {
                DataController.Instance.CurrentMap.MapClear();
            }
        }

        private IEnumerator WaitPimp()
        {
            yield return new WaitUntil(() => speatSlider.value > speatSlider.maxValue * 0.1f);
            
            pimp.SetActive(true);
            pimpSlider.gameObject.SetActive(true);
        }

        private IEnumerator StartRunGame()
        {
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(true);
            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            var waitForFixedUpdate = new WaitForFixedUpdate();

            var originJumpForce = mainCharacter.jumpForce;
            mainCharacter.jumpForce = 7f;
            mainCharacter.CharacterAnimator.SetFloat("Speed", JoystickController.Instance.inputDegree);
            
            while (speatSlider.value < speatSlider.maxValue)
            {
                var percentage = 100f / speatSlider.maxValue;
                remainingDistance = (speatSlider.maxValue - speatSlider.value) * percentage;
                pimpDistance = (speatSlider.value - pimpSlider.value) * percentage;

                remainingDistanceText.text = $"{remainingDistance:0}m";
                pimpDistanceText.text = $"{pimpDistance:0}m";

                
                if (speatSlider.value >= speatSlider.maxValue)
                {
                    EndPlay();
                }
                else if (speatSlider.value >= 10 && speatSlider.value <= pimpSlider.value + 1)
                {
                    DataController.Instance.ChangeMap(DataController.Instance.mapCode);
                }
                
                
                if (pimp.activeSelf)
                {
                    var localPosition = pimp.transform.localPosition;
                    localPosition.x = 20f - pimpDistance * 2f;
                    pimp.transform.localPosition = localPosition;
                    
                    pimpSlider.value += 0.85f * Time.fixedDeltaTime + pimpAccelator;
                    pimpAccelator += 0.0005f * Time.fixedDeltaTime;
                }

                if (startPosition.position.x < mainCharacter.transform.position.x)
                {
                    speatSlider.value += 0.8f * Time.fixedDeltaTime + speatAccelator;
                    speatAccelator += 0.0009f * Time.fixedDeltaTime;

                    InstantiateObstacle();
                }
                else
                {
                    speatAccelator *= 0.97f;
                }

                yield return waitForFixedUpdate;
            }

            mainCharacter.jumpForce = originJumpForce;
            mainCharacter.CharacterAnimator.SetFloat("Speed", 0f);
        }

        private void InstantiateObstacle()
        {
            for (var idx = 1; idx < trailer.Length; idx++)
            {
                if (trailer[0].position.x - trailer[idx].position.x >= 18f * 2)
                {
                    trailer[idx].position = trailer[0].position + 18.5f * 2 * Vector3.right;
                }
                else
                {
                    // 앞 위치로 이동
                    trailer[idx].position -= Vector3.right * (runSpeed + speatAccelator) * Time.fixedDeltaTime; // 뒤로 밀기
                }
                
                // 제거 및 생성
                if (trailer[0].position.x - trailer[idx].position.x >= 18f * 2)
                {
                    Destroy(trailer[idx].GetChild(0).gameObject); // 현재 장애물 패턴 제거

                    if (speatSlider.value < speatSlider.maxValue * 0.8f)
                    {
                        var level = 3;
                        if (remainingDistance > 66f)
                        {
                            level = 1;
                        }
                        else if (remainingDistance > 33f)
                        {
                            level = 2;
                        }
                        Instantiate(patterns[level][Random.Range(0, patterns[level].Length)], trailer[idx])
                            .SetActive(true);
                    }
                    else
                    {
                        Instantiate(patterns[0][1], trailer[idx]).SetActive(true);
                    }
                }
            }
        }

        private void UseJump()
        {
            if (!jumpEnable)
            {
                return;
            }

            jumpEnable = false;
            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            mainCharacter.Jump();
            StartCoroutine(JumpCooldown());
        }

        private IEnumerator JumpCooldown()
        {
            var jumpImage = jumpButton.GetComponentsInChildren<Image>()[1];
            jumpImage.fillAmount = 1;
            var waitForFixedUpdate = new WaitForFixedUpdate();
            while (jumpImage.fillAmount > 0)
            {
                jumpImage.fillAmount -= Time.fixedDeltaTime / jumpCooldownSec;
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

            mainCharacter.gameObject.layer = LayerMask.NameToLayer("Player");

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
