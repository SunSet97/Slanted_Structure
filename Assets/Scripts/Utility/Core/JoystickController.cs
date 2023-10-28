﻿using System;
using Data;
using UnityEngine;
using UnityEngine.UI;
using Utility.Character;
using Utility.Map;

namespace Utility.Core
{
    public class JoystickController : MonoBehaviour
    {
        public static JoystickController Instance { get; private set; }
        
#pragma warning disable 0649
        [Header("조이스틱")] [SerializeField] private Joystick dynamicJoystick;
        [SerializeField] private Joystick fixedJoyStick;
        [SerializeField] private Button jumpButton;
#pragma warning restore 0649
        
        [Header("For Debug")]
        public Vector2 inputDirection;
        public float inputDegree;

        [NonSerialized] public Joystick Joystick;
        
        public bool InputJump
        {
            get => inputJump;
            set
            {
                if (value)
                {
                    var character = DataController.Instance.GetCharacter(CharacterType.Main);
                    if (character != null && character.IsJumpEnable())
                    {
                        // 점프 중이 아닌 경우에만 True 가능
                        inputJump = true;
                        Debug.Log("Jump On");
                    }
                }
                else
                {
                    inputJump = false;
                }
            }
        }

        private bool inputJump;

        private bool isAlreadySave;
        private bool wasJoystickUse;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        private void Start()
        {
            jumpButton.onClick.AddListener(() =>
            {
                var mainCharacter = DataController.Instance.GetCharacter(CharacterType.Main);
                mainCharacter.TryJump();
            });
        }

        public void Initialize(JoystickType joystickType)
        {
            Debug.Log("조이스틱 초기화 - " + joystickType);
            isAlreadySave = false;

            if (Joystick)
            {
                Joystick.gameObject.SetActive(false);
            }

            jumpButton.gameObject.SetActive(false);

            if (joystickType == JoystickType.Dynamic)
            {
                Joystick = dynamicJoystick;
            }
            else if (joystickType == JoystickType.Fixed)
            {
                Joystick = fixedJoyStick;
            }
        }
        
        public void ResetJoyStickState()
        {
            if (!Joystick)
            {
                return;
            }
            
            Joystick.gameObject.SetActive(Joystick.gameObject.activeSelf);
            if (Joystick.GetType() == typeof(DynamicJoystick))
            {
                Joystick.transform.GetChild(0).gameObject.SetActive(false);
                Joystick.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().position = default;
            }
            else if (Joystick.GetType() == typeof(FixedJoystick))
            {
                jumpButton.gameObject.SetActive(jumpButton.gameObject.activeSelf);
                Joystick.transform.GetChild(0).gameObject.SetActive(false);
            }

            Joystick.OnEndDrag();
            inputDegree = 0;
            inputDirection = Vector2.zero;
            Joystick.input = Vector2.zero;
        }

        /// <summary>
        /// 조이스틱 상태 초기화하는 함수
        /// </summary>
        /// <param name="isOn">JoyStick On/Off</param>
        public void SetJoyStickState(bool isOn)
        {
            if (!Joystick)
            {
                return;
            }

            Joystick.gameObject.SetActive(isOn);
            if (Joystick.GetType() == typeof(DynamicJoystick))
            {
                Joystick.transform.GetChild(0).gameObject.SetActive(false);
                Joystick.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().position = default;
            }
            else if (Joystick.GetType() == typeof(FixedJoystick))
            {
                jumpButton.gameObject.SetActive(isOn);
                Joystick.transform.GetChild(0).gameObject.SetActive(false);
            }

            Joystick.OnEndDrag();
            inputDegree = 0;
            inputDirection = Vector2.zero;
            Joystick.input = Vector2.zero;
            InputJump = false;
        }

        /// <summary>
        /// 조이스틱 멈추고 이전 상태에 따라 키거나 끄는 함수
        /// ex) 대화 이전에 조이스틱을 사용하지 않으면 계속 사용하지 않는다.
        /// </summary>
        /// <param name="isSave">Save여부   true - save, false - load</param>
        public void StopSaveLoadJoyStick(bool isSave)
        {
            Debug.Log((isSave ? "Save" : "Load") + ", " + (isAlreadySave ? "저장된 상태" : "저장되지 않은 상태") + ", 이전 상태" +
                      wasJoystickUse);

            if (isSave)
            {
                if (isAlreadySave) return;

                isAlreadySave = true;
                wasJoystickUse = Joystick.gameObject.activeSelf;
                SetJoyStickState(false);
            }
            else
            {
                isAlreadySave = false;
                SetJoyStickState(wasJoystickUse);
            }
        }

        public void SetJoystickArea(CustomEnum.JoystickAreaType joystickAreaType)
        {
            var rect = Joystick.GetComponent<RectTransform>();
            if (joystickAreaType == CustomEnum.JoystickAreaType.Default)
            {
                rect.anchorMax = new Vector2(.5f, .5f);
            }
            else if (joystickAreaType == CustomEnum.JoystickAreaType.Full)
            {
                rect.anchorMax = Vector2.one;
            }
            else if (joystickAreaType == CustomEnum.JoystickAreaType.None)
            {
                rect.anchorMax = Vector2.zero;
            }

            Debug.Log("조이스틱 영역 조절" + rect.anchorMax);
        }

        public void UpdateJoystickInput(CharacterMoveType method)
        {
            if (method.Equals(CharacterMoveType.OneDirection))
            {
                inputDirection.Set(Joystick.Horizontal, 0);
                inputDegree = Mathf.Abs(Joystick.Horizontal);
                if(Mathf.Approximately(inputDegree, 0f)){
                    return;
                }
                
                if (Joystick.GetType() == typeof(DynamicJoystick))
                {
                    var angle = -1 * (Vector2.Angle(Vector2.up, new Vector2(Joystick.Horizontal, Joystick.Vertical)) -
                                      90f);
                    
                    InputJump = angle >= 30f;
                }
            }   
            else if (method.Equals(CharacterMoveType.Waypoint))
            {
                //inputDirection = changedDir;
                inputDegree = Mathf.Abs(Joystick.Horizontal);
                if(Mathf.Approximately(inputDegree, 0f)){
                    return;
                }

                if (Joystick.GetType() == typeof(DynamicJoystick))
                {
                    var angle = -1 * (Vector2.Angle(Vector2.up, new Vector2(Joystick.Horizontal, Joystick.Vertical)) -
                                      90f);

                    InputJump = angle >= 30f;
                }
            }
            else
            {
                inputDirection.Set(Joystick.Horizontal, Joystick.Vertical);
                inputDegree = Vector2.Distance(Vector2.zero, inputDirection);
            }
        }
        
        public void SetVisible(bool isVisible)
        {
            var joystickChildren = Joystick.transform.GetChild(0).GetComponentsInChildren<Image>(true);
            foreach (var image in joystickChildren)
            {
                if (isVisible)
                {
                    image.color = Color.white - Color.black * 0.5f;
                }
                else
                {
                    image.color = Color.clear;
                }
            }
        }
    }
}