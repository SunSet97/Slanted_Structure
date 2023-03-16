using System;
using System.Collections;
using Data.GamePlay;
using UnityEngine;
using UnityEngine.UI;
using Utility.Core;
using Utility.Preference;

namespace Episode.EP2.PlayGroundGame
{
    public class PlayGroundManager : Game
    {
        [Serializable]
        public struct ThrowCan
        {
            public float power;
            [Range(0, 1)] public float plusY;
            public ResultState resultType;
        }

        public enum ResultState
        {
            Perfect,
            Good,
            NotBad,
            Bad
        }
        
        public GameObject canPrefab;

        public GameObject rangeParent;
        public Button clearButton;

        public Transform startPoint;
        public Transform destPoint;

        public ThrowCan[] throwCans;

        [SerializeField] private CreateRing ring;

        private void Start()
        {
            clearButton.onClick.AddListener(ClearCheck);
        }

        public override void Play()
        {
            base.Play();
            StartTimingGame();
        }

        private IEnumerator StartTestThrow()
        {
            while (true)
            {
                StartThrowing(ResultState.Bad);
                yield return new WaitForSeconds(2f);
            }
        }

        public override void EndPlay()
        {
            base.EndPlay();
        
            DataController.Instance.camInfo.camDis = DataController.Instance.CurrentMap.camDis;
            DataController.Instance.camInfo.camRot = DataController.Instance.CurrentMap.camRot;
        }

        private void StartTimingGame()
        {
            JoystickController.Instance.InitializeJoyStick(false);
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(true);

            var canvas = PlayUIController.Instance.Canvas;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = DataController.Instance.Cam;
            canvas.planeDistance = 0.8f;

            rangeParent.SetActive(true);

            ring.Setup();
            ring.CreatePoints();

            StartCoroutine(PlayTimingGame());
        }
        
        private IEnumerator PlayTimingGame()
        {
            while (rangeParent.transform.GetChild(2).localScale.x * 0.3 *
                   rangeParent.transform.GetChild(2).GetComponent<RectTransform>().rect.width <= ring.Radius * 2)
            {
                ring.DisplayUpdate();
                yield return null;
            }

            EndTimingGame(ResultState.Bad);
        }

        private void EndTimingGame(ResultState result)
        {
            rangeParent.SetActive(false);
            ring.Reset();
            PlayUIController.Instance.Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            DataController.Instance.CurrentMap.ui.gameObject.SetActive(false);
            StartThrowing(result);
        }

        private void StartThrowing(ResultState type)
        {
            var can = Instantiate(canPrefab);
            can.GetComponent<Can>().OnCollisionEnter.AddListener(() => { Invoke(nameof(EndPlay), 1f); });
            can.transform.position = startPoint.position;

            var index = Array.FindIndex(throwCans, item => item.resultType == type);
        
            var canDir = (destPoint.position - startPoint.position).normalized;
            canDir.y = throwCans[index].plusY;

            can.GetComponent<Collider>().isTrigger = false;

            var throwRigid = can.GetComponent<Rigidbody>();
            throwRigid.AddForce(canDir * throwCans[index].power, ForceMode.Impulse);
            throwRigid.AddTorque(canDir * throwCans[index].power, ForceMode.Impulse);
        }

        private void ClearCheck()
        {
            if (!IsPlay)
            {
                return;
            }

            ResultState result;
            if (ring.Radius * 2 <= rangeParent.transform.GetChild(2).localScale.x *
                rangeParent.transform.GetChild(2).GetComponent<RectTransform>().rect.width)
            {
                Debug.Log("유후! 바로 들어감");
                result = ResultState.Perfect;
            }
            else if (ring.Radius * 2 <= rangeParent.transform.GetChild(1).localScale.x *
                     rangeParent.transform.GetChild(1).GetComponent<RectTransform>().rect.width)
            {
                Debug.Log("아싸! 한번 튕기고 들어감");
                result = ResultState.Good;
            }
            else if (ring.Radius * 2 <= rangeParent.transform.GetChild(0).localScale.x *
                     rangeParent.transform.GetChild(0).GetComponent<RectTransform>().rect.width)
            {
                Debug.Log("아깝다! 한번 튕기고 안들어감");
                result = ResultState.NotBad;
            }
            else
            {
                Debug.Log("앗! 안들어감");
                result = ResultState.Bad;
            }

            StopAllCoroutines();
            EndTimingGame(result);
        }
    }
}