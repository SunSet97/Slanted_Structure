using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Audio;
using Utility.Character;
using Utility.Core;
using Utility.UI.Preference;
using Utility.Utils.Property;
using Utility.WayPoint;

namespace Utility.Map
{
    public enum CharacterMoveType
    {
        OneDirection,
        AllDirection,
        Other,
        Waypoint
    }

    public class MapData : MonoBehaviour
    {
        [Serializable]
        public class AnimationCharacterSet
        {
            public Animator characterAnimator;
            public CharacterType who;
        }

        [Serializable]
        public struct CharacterPositionSet
        {
            public CharacterType who;
            public Transform startPosition;
            public bool isMain;
            public bool isFollow;
        }

        #region 맵 설정

        [Header("#Map Setting")] [Tooltip("맵의 코드이며 변경시 오브젝트의 이름도 같이 변경 됩니다.(코드는 반드시 6자리)")]
        public string mapCode = "000000"; // auto setting

        public string location;
        public string date;
        public string time;

        [FormerlySerializedAs("moveType")]
        [FormerlySerializedAs("method")]
        [Space(15)]
        [ConditionalHideInInspector("isJoystickInputUse")]
        [Tooltip("이 맵의 조이스틱 입력 방식입니다.")]
        public CharacterMoveType characterMoveType;

        [Tooltip("클리어시 넘어갈 다음 맵의 맵 코드입니다.")] public string nextMapcode = "000000";

        //[ConditionalHideInInspector("characterMoveType", JoystickInputMethod.OneDirection)]
        [ConditionalHideInInspector("isJoystickInputUse")]
        public bool rightIsForward;

        [ConditionalHideInInspector("characterMoveType", CharacterMoveType.Waypoint)]
        [ConditionalHideInInspector("isJoystickInputUse")]
        public Waypoint waypoint;

        [Space(15)] [Tooltip("맵의 이름은 사용자가 원하는 대로 변경하면 되며 맵 구성 어셋들은 이 오브젝트의 자식으로 설정해주면 됩니다.")]
        public GameObject map; // auto setting

        [Tooltip("이 맵의 전용 UI를 넣어주시면 됩니다.")] public RectTransform ui;

        [Tooltip("이 맵의 전용 SkyBox를 넣어주시면 됩니다.")]
        public Material skyboxSetting;

        [Tooltip("카메라의 orthographic 뷰를 제어할 수 있습니다.")]
        public bool isOrthographic;

        [ConditionalHideInInspector("isOrthographic")]
        public float orthographicSize;

        [Header("FadeOut")] [Space(10)] public bool useFadeOut;

        [ConditionalHideInInspector("useFadeOut")]
        public float fadeOutSec = 1f;

        public float fadeInSec = 1f;

        [FormerlySerializedAs("BGM")] [Header("BGM")]
        public AudioClip bgm;

        [Header("애니메이션 실행되는 캐릭터 넣으세요")] public List<AnimationCharacterSet> characters;

        [Header("#클리어 박스")] [SerializeField] private List<CheckMapClear> clearBoxList = new List<CheckMapClear>();

        #endregion

        [Header("캐릭터 세팅")] public List<CharacterPositionSet> positionSets;

        [Header("맵 세팅")] [Space(20)] public bool isCustomJumpForce;

        [ConditionalHideInInspector("isCustomJumpForce")]
        public float jumpForce;

        public bool isCustomGravityScale;

        [ConditionalHideInInspector("isCustomGravityScale")]
        public float gravityScale;

        [Space(20)] [Header("카메라")] public CameraViewType cameraViewType;
        public Vector3 camDis;
        public Vector3 camRot;
        public bool useFieldOfView;

        [ConditionalHideInInspector("useFieldOfView")]
        public float fieldOfView;

        public bool useClippingPlanes;

        [ConditionalHideInInspector("useClippingPlanes")]
        public float farClipPlane;

        public bool usePostProcessing;

        [Header("조이스틱")] [Space(20)] [Header("조이스틱 인풋 사용 유무")]
        public bool isJoystickInputUse;

        [Header("조이스틱 존재 유무")] public bool isJoystickNone;
        [Header("조이스틱 컨트롤 불가능 유무")] public bool isJoystickControlDisable;

        [ConditionalHideInInspector("isJoystickInputUse")]
        public JoystickType joystickType;

        private CharacterManager mainCharacter;
        private CharacterManager[] followCharacters;

        private void Awake()
        {
            if (Application.isEditor)
            {
                if (name != mapCode)
                {
                    name = mapCode;
                }

                if (ui != null)
                {
                    ui.name = map.name;
                }
            }

            if (ui != null)
            {
                ui.SetParent(PlayUIController.Instance.mapUi);
                ui.offsetMax = new Vector2(0, 0);
                ui.offsetMin = new Vector2(0, 0);
                ui.localScale = new Vector3(1, 1, 1);
            }

            AudioController.Instance.PlayBgm(bgm);

            mainCharacter = DataController.Instance.GetCharacter(CharacterType.Main);
            followCharacters = DataController.Instance.GetFollowCharacters(positionSets);
        }

        private void FixedUpdate()
        {
            PlayerFixedUpdate();
        }

        private void PlayerFixedUpdate()
        {
            if (mainCharacter == null)
            {
                return;
            }

            if (isJoystickInputUse)
            {
                JoystickController.Instance.UpdateJoystickInput(characterMoveType);
            }

            mainCharacter.MoveCharacter(characterMoveType, isJoystickInputUse);

            if (followCharacters != null)
            {
                foreach (var followCharacter in followCharacters)
                {
                    followCharacter.FollowMainCharacter(characterMoveType);
                }
            }
        }

        public void SetClearMapCode(string nextMapCode)
        {
            clearBoxList[0].nextSelectMapCode = nextMapCode;
        }

        public void ClearMap(float waitTime)
        {
            Invoke("ClearMap", waitTime);
        }

        public void ClearMap()
        {
            if (clearBoxList?.Count > 0)
            {
                clearBoxList[0].Clear();
            }
            else
            {
                Debug.LogError("클리어 박스 오류");
            }
        }

        public void ResetMap()
        {
            DataController.Instance.ChangeMap(mapCode);
        }

        public void DestroyMap()
        {
            if (ui)
            {
                Destroy(ui.gameObject);
            }

            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isEditor || Application.isPlaying)
            {
                return;
            }

            Gizmos.color = Color.red;
            foreach (var posSet in positionSets)
            {
                Gizmos.DrawSphere(posSet.startPosition.position, 1f);
            }
        }
    }
}