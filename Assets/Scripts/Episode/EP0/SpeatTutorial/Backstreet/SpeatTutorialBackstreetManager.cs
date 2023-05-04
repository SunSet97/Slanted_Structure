using System.Collections;
using System.Linq;
using Data.GamePlay;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using static Data.CustomEnum;
using Random = UnityEngine.Random;

namespace Episode.EP0.SpeatTutorial.Backstreet
{
    public class SpeatTutorialBackstreetManager : MiniGame
    {
        [Header("#UI")] [SerializeField] private Slider speatSlider;
        [SerializeField] private Text remainingDistanceText;
        [SerializeField] private Slider pimpSlider;
        [SerializeField] private Text pimpDistanceText;

        [Header("#Objects")] [SerializeField] private Transform startPosition;
        [SerializeField] private GameObject pimp;
        [SerializeField] private GameObject endTrailer;
        [SerializeField] private Transform[] trailers;

        [Header("#Buttons")] [SerializeField] private Button abilityButton;
        [SerializeField] private Button jumpButton;

        [SerializeField] private float runSpeed;
        [SerializeField] private float jumpCooldownSec = 0.05f;
        [SerializeField] private float abilityCooldownSec = 0.05f;

        private float originJumpForce;
        private float speatAccelator;
        private float pimpAccelator;
        private bool jumpEnable;
        private bool abilityEnable;
        private GameObject[][] patterns;
        private AssetBundle obstacleAssetBundle;
        
        private static readonly int Speed = Animator.StringToHash("Speed");

        private const string Path = "/AssetBundles/backstreetrun";
        private const int Length = 100;
        public float TrailerDistance = 18.65f;

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
                var obstacles = obstacleAssets
                    .Where(item => item.name.Substring(7, 1) == idx.ToString()).Distinct()
                    .ToArray();
                patterns[idx] = obstacles;
                foreach (var obstacle in obstacles)
                {
                    Debug.Log(obstacle);
                }
            }

            abilityButton.onClick.AddListener(UseAbility);
            jumpButton.onClick.AddListener(UseJump);

            jumpEnable = true;
            abilityEnable = true;
        }

        public override void Play()
        {
            base.Play();
            Debug.Log("플레이!");
            StartCoroutine(WaitPimp());
            StartCoroutine(StartRunGame());
        }

        public override void EndPlay()
        {
            base.EndPlay();
            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            mainCharacter.jumpForce = originJumpForce;
            mainCharacter.CharacterAnimator.SetFloat(Speed, 0f);
            StopAllCoroutines();
        }

        private IEnumerator WaitPimp()
        {
            yield return new WaitUntil(() => speatSlider.value > speatSlider.maxValue * 0.1f);

            pimp.SetActive(true);
            pimpSlider.gameObject.SetActive(true);
            pimp.GetComponent<Animator>().SetFloat(Speed, 1f);
        }

        private IEnumerator StartRunGame()
        {
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(true);
            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            var waitForFixedUpdate = new WaitForFixedUpdate();

            originJumpForce = mainCharacter.jumpForce;
            mainCharacter.jumpForce = 7f;
            mainCharacter.CharacterAnimator.SetFloat(Speed, 1f);

            while (true)
            {
                // 총 100m
                var percentage = 100f / speatSlider.maxValue;
                var remainingDistance = (speatSlider.maxValue - speatSlider.value) * percentage;
                var pimpDistance = (speatSlider.value - pimpSlider.value) * percentage;

                remainingDistanceText.text = $"{remainingDistance * Length / 100f:0}m";
                pimpDistanceText.text = $"{pimpDistance * Length / 100f:0}m";


                if (pimp.activeSelf && speatSlider.value <= pimpSlider.value)
                {
                    // DialogueController.Instance.SetDialougueEndAction(() =>
                    // {
                    //     if (speatSlider.value + speatSlider.maxValue * 0.1f >= speatSlider.maxValue * 0.8f)
                    //     {
                    //         // speatSlider.value
                    //     }
                    //     else
                    //     {
                    //         speatSlider.value += speatSlider.maxValue * 0.1f;
                    //     }
                    // });
                    // DialogueController.Instance.StartConversation(); 
                    DataController.Instance.CurrentMap.ResetMap();

                    break;
                }

                if (pimp.activeSelf)
                {
                    var localPosition = pimp.transform.localPosition;
                    localPosition.x = 20f - pimpDistance * 2f;
                    pimp.transform.localPosition = localPosition;

                    pimpSlider.value += 0.85f * Time.fixedDeltaTime + pimpAccelator;
                    pimpAccelator += 0.0003f * Time.fixedDeltaTime;
                }

                if (startPosition.position.x < mainCharacter.transform.position.x)
                {
                    speatSlider.value += 0.8f * Time.fixedDeltaTime + speatAccelator;
                    speatAccelator += 0.0009f * Time.fixedDeltaTime;

                    InstantiateObstacle(remainingDistance);
                }
                else
                {
                    speatAccelator *= 0.98f;
                }

                yield return waitForFixedUpdate;
            }
        }

        private void InstantiateObstacle(float remainingDistance)
        {
            for (var index = 1; index < trailers.Length; index++)
            {
                var trailer = trailers[index];
                if (trailers[0].position.x >= trailer.position.x && !endTrailer.activeSelf)
                {
                    Destroy(trailer.GetChild(0).gameObject);

                    if (speatSlider.value < speatSlider.maxValue * 0.9f)
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

                        Instantiate(patterns[level][Random.Range(0, patterns[level].Length)], trailer).SetActive(true);
                    }
                    else if(remainingDistance <= 4f)
                    {
                        endTrailer.SetActive(true);
                        endTrailer.transform.SetParent(trailer);
                        endTrailer.transform.localPosition = new Vector3(-204.17f, 0, 0);
                        Instantiate(patterns[0][0], trailer).SetActive(true);
                    }
                    else
                    {
                        Instantiate(patterns[0][1], trailer).SetActive(true);
                        Debug.Log($"남은거리: {remainingDistance}, 생성");
                    }
                    
                    var leftIndex = ((index - 1) + (trailers.Length - 1) - 1) % (trailers.Length - 1) + 1;
                    trailer.position = trailers[leftIndex].position + TrailerDistance * Vector3.right;
                }
                else
                {
                    Debug.Log("무브");
                    trailer.position -= Vector3.right * ((runSpeed + speatAccelator) * Time.fixedDeltaTime);
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
            mainCharacter.TryJump();
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
                abilityImage.fillAmount -= Time.fixedDeltaTime / abilityCooldownSec;
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

            StopAllCoroutines();
        }
    }
}