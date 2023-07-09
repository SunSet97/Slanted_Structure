using System;
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
        [Serializable]
        private class TrailData
        {
            public Transform transform;
            public Trail trail;
        }
        
        [Header("#UI")] [SerializeField] private Slider speatSlider;
        [SerializeField] private Text remainingDistanceText;
        [SerializeField] private Slider pimpSlider;
        [SerializeField] private Text pimpDistanceText;

        [Header("#Objects")] [SerializeField] private Transform startPosition;
        [SerializeField] private GameObject pimp;
        // [SerializeField] private Trail endTrailer;
        [SerializeField] private TrailData[] trailerData;
        [SerializeField] private Transform resetPoint;

        [Header("#Buttons")] [SerializeField] private Button abilityButton;
        [SerializeField] private Button jumpButton;

        [Header("#Play")] [SerializeField] private float jumpCooldownSec = 0.05f;
        [SerializeField] private float abilityCooldownSec = 0.05f;
        [SerializeField] private float runSpeed;
        [SerializeField] private float freeDistance;
        // [SerializeField] private float endDistance;

        private bool JumpEnable
        {
            get => jumpEnable;
            set
            {
                jumpEnable = value;
                jumpButton.enabled = jumpEnable;
            }
        }        
        
        private float speatAcceleration;
        private float pimpAcceleration;
        private bool jumpEnable;
        private bool abilityEnable;
        private Trail[][] patterns;
        private AssetBundle obstacleAssetBundle;
        
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        
        private const string Path = "/AssetBundles/backstreetrun";
        private const int Length = 100;

        private void Start()
        {
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(false);

            obstacleAssetBundle = AssetBundle.LoadFromFile(Application.dataPath + Path);
            Debug.Log(obstacleAssetBundle == null ? "Fail to load" : "Success to load");
            var obstacleAssets = obstacleAssetBundle.LoadAllAssets<GameObject>();

            patterns = new Trail[4][];
            for (var idx = 0; idx < patterns.Length; idx++)
            {
                var obstacles = obstacleAssets
                    .Where(item => item.name.Substring(7, 1) == idx.ToString()).Distinct()
                    .ToArray();
                patterns[idx] = obstacles.Select(item => item.GetComponent<Trail>()).ToArray();
            }

            abilityButton.onClick.AddListener(UseAbility);
            jumpButton.onClick.AddListener(UseJump);
        }

        public override void Play()
        {
            base.Play();
            StartCoroutine(WaitPimp());
            StartCoroutine(StartRunGame());
        }

        public override void EndPlay()
        {
            base.EndPlay();
            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            mainCharacter.CharacterAnimator.SetFloat(SpeedHash, 0f);
            StopAllCoroutines();
            
            DataController.Instance.CurrentMap.MapClear();
        }

        private IEnumerator WaitPimp()
        {
            yield return new WaitUntil(() => speatSlider.value > speatSlider.maxValue * 0.1f);

            pimp.SetActive(true);
            pimpSlider.gameObject.SetActive(true);
            pimp.GetComponent<Animator>().SetFloat(SpeedHash, 1f);
        }

        private IEnumerator StartRunGame()
        {
            speatSlider.value = 0f;
            pimpSlider.value = 0f;
            JumpEnable = true;
            abilityEnable = true;
            
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(true);
            var mainCharacter = DataController.Instance.GetCharacter(Character.Main);

            const float deltaTime = 0.01f;
            var waitForSeconds = new WaitForSeconds(deltaTime);
            
            mainCharacter.CharacterAnimator.SetFloat(SpeedHash, 1f);

            while (true)
            {
                // 총 100m
                var percentage = 100f / speatSlider.maxValue;
                var remainingDistance = (speatSlider.maxValue - speatSlider.value) * percentage;
                var pimpDistance = (speatSlider.value - pimpSlider.value) * percentage;

                remainingDistanceText.text = $"{remainingDistance * Length / 100f:0}m";
                pimpDistanceText.text = $"{pimpDistance * Length / 100f:0}m";

                if (Mathf.Approximately(remainingDistance, 0f))
                {
                    EndPlay();
                }

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

                    pimpSlider.value += 0.85f * deltaTime + pimpAcceleration;
                    pimpAcceleration += 0.0003f * deltaTime;
                }

                if (startPosition.position.x < mainCharacter.transform.position.x)
                {
                    speatSlider.value += 0.8f * deltaTime + speatAcceleration;
                    speatAcceleration += 0.0009f * deltaTime;

                    UpdateObstacle(remainingDistance, deltaTime);
                }
                else
                {
                    speatAcceleration *= 0.98f;
                }

                yield return waitForSeconds;
            }
        }

        private void UpdateObstacle(float remainingDistance, float deltaTime)
        {
            for (var index = 0; index < trailerData.Length; index++)
            {
                var trailData = trailerData[index];
                // if (resetPoint.position.x >= trailData.transform.position.x && !endTrailer.gameObject.activeSelf)
                if (resetPoint.position.x >= trailData.transform.position.x)
                {
                    Destroy(trailData.trail.gameObject);

                    Debug.Log(remainingDistance);
                    if (remainingDistance > freeDistance)
                    {
                        var level = 3;
                        var ratio = Mathf.InverseLerp(freeDistance, 100, remainingDistance);
                        Debug.Log(ratio);
                        if (ratio > .66f)
                        {
                            level = 1;
                        }
                        else if (ratio > .33f)
                        {
                            level = 2;
                        }

                        trailData.trail = Instantiate(patterns[level][Random.Range(0, patterns[level].Length)],
                            trailData.transform);
                        trailData.trail.gameObject.SetActive(true);
                    }
                    // else if (remainingDistance > endDistance)
                    else
                    {
                        trailData.trail = Instantiate(patterns[0][0], trailData.transform);
                        trailData.trail.gameObject.SetActive(true);
                        Debug.Log($"남은거리: {remainingDistance}, 생성");
                    }
                    // else
                    // {
                    //     endTrailer.gameObject.SetActive(true);
                    //     endTrailer.transform.SetParent(trailData.transform);
                    //     trailData.trail = endTrailer;
                    //     endTrailer.transform.localPosition = new Vector3(-215f, 0, 0);
                    // }

                    var leftIndex = (index - 1 + trailerData.Length) % trailerData.Length;
                    trailData.transform.position = trailerData[leftIndex].transform.position +
                                                   (trailerData[leftIndex].trail.length / 2 +
                                                    trailerData[index].trail.length / 2) * Vector3.right;
                }

                trailData.transform.position -= Vector3.right * ((runSpeed + speatAcceleration) * deltaTime);
            }
        }

        private void UseJump()
        {
            if (!JumpEnable)
            {
                return;
            }

            JumpEnable = false;
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

            JumpEnable = true;
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