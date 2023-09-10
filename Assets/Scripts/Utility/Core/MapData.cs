using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Audio;
using Utility.Preference;
using Utility.Property;
using Utility.WayPoint;
using static Data.CustomEnum;

namespace Utility.Core
{
    [ExecuteInEditMode]
    public class MapData : MonoBehaviour
    {
        [Serializable]
        public class AnimationCharacterSet
        {
            public Animator characterAnimator;
            public Character who;
        }

        [Serializable]
        public struct CharacterPositionSet
        {
            public Character who;
            public Transform startPosition;
            public bool isMain;
            public bool isFollow;
        }

        #region Default

        [TextArea(7, int.MaxValue)] [SerializeField]
        private string scriptDesription =
            "맵 자동 생성 및 맵 정보 포함 스크립트이며\n인스펙터 창에서 각각의 이름에 마우스를 갖다대면 설명을 볼 수 있습니다.\n(Edit mode에서도 바로 사용 가능)";

        #endregion

        #region 맵 설정

        [Header("#Map Setting")] [Tooltip("맵의 코드이며 변경시 오브젝트의 이름도 같이 변경 됩니다.(코드는 반드시 6자리)")]
        public string mapCode = "000000"; // auto setting

        public string location;
        public string date;
        public string time;

        [Space(15)] [ConditionalHideInInspector("isJoystickInputUse")] [Tooltip("이 맵의 조이스틱 입력 방식입니다.")]
        public JoystickInputMethod method;

        [Tooltip("클리어시 넘어갈 다음 맵의 맵 코드입니다.")] public string nextMapcode = "000000";

        //[ConditionalHideInInspector("method", JoystickInputMethod.OneDirection)]
        [ConditionalHideInInspector("isJoystickInputUse")]
        public bool rightIsForward;

        [ConditionalHideInInspector("method", JoystickInputMethod.Waypoint)]
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

        [FormerlySerializedAs("BGM")] [Header("BGM입니다")]
        public AudioClip bgm;

        [Header("애니메이션 실행되는 캐릭터 넣으세요")] public List<AnimationCharacterSet> characters;

        [Header("#클리어 박스")]
        [ContextMenuItem("Create ClearBox", "CreateClearBox")]
        [Tooltip("인스펙터 설정창에서 클리어박스를 생성하세요.")]
        [SerializeField]
        private GameObject clearBoxSetting;

        [SerializeField] private List<CheckMapClear> clearBoxList = new List<CheckMapClear>();

        private void CreateDefaultSetting()
        {
            GameObject temp = new GameObject();
            if (map == null)
            {
                map = Instantiate(temp, transform.position, Quaternion.identity, transform);
                map.name = "Map Name";
            }

            if (clearBoxSetting == null)
            {
                clearBoxSetting = Instantiate(temp, transform.position, Quaternion.identity, transform);
                clearBoxSetting.name = "ClearBox Setting";
            }

            if (positionSetting == null) // 캐릭터 위치 세팅 오브젝트 생성 및 이름 설정
            {
                positionSetting = Instantiate(temp, transform.position, Quaternion.identity, transform);
                positionSetting.name = "Position Setting";
            }

            DestroyImmediate(temp);
        }

        #endregion

        #region 위치 설정

        [Header("#Position Setting")]
        [ContextMenuItem("Remove/Rau", "RemoveRauPosition")]
        [ContextMenuItem("Remove/Oun", "RemoveOunPosition")]
        [ContextMenuItem("Remove/Speat", "RemoveSpeatPosition")]
        [ContextMenuItem("Remove/SpeatAdult", "RemoveSpeatAdultPosition")]
        [ContextMenuItem("Remove/SpeatChild", "RemoveSpeatChildPosition")]
        [ContextMenuItem("Remove/SpeatAdolescene", "RemoveSpeatAdolescenePosition")]
        [ContextMenuItem("Remove/All", "RemoveAllPosition")]
        [ContextMenuItem("Create/Rau", "CreateRauPosition")]
        [ContextMenuItem("Create/Oun", "CreateOunPosition")]
        [ContextMenuItem("Create/Speat", "CreateSpeatPosition")]
        [ContextMenuItem("Create/SpeatAdult", "CreateSpeatAdultPosition")]
        [ContextMenuItem("Create/SpeatChild", "CreateSpeatChildPosition")]
        [ContextMenuItem("Create/SpeatAdolescene", "CreateSpeatAdolescenePosition")]
        [ContextMenuItem("Create/All", "CreateAllPosition")]
        [Tooltip("인스펙터를 우클릭하여 원하는 캐릭터의 시작위치와 목표위치를 생성 및 제거하세요.")]
        [SerializeField]
        private GameObject positionSetting; // auto setting

        public List<CharacterPositionSet> positionSets; // auto setting

        [Space(10)] public bool isCustomJumpForce;

        [ConditionalHideInInspectorAttribute("isCustomJumpForce")]
        public float jumpForce;

        public bool isCustomGravityScale;

        [ConditionalHideInInspector("isCustomGravityScale")]
        public float gravityScale;

        //ContextMenu 연결
        public void CreateClearBox()
        {
            Transform instant = new GameObject().transform;
            Transform clearBox = Instantiate(instant, positionSetting.transform.position, Quaternion.identity,
                clearBoxSetting.transform);
            clearBox.name = "Clear Box";
            clearBox.gameObject.AddComponent<BoxCollider>();
            clearBox.GetComponent<BoxCollider>().isTrigger = true;
            clearBox.gameObject.AddComponent<CheckMapClear>();
            DestroyImmediate(instant.gameObject);
            clearBoxList.Add(clearBox.GetComponent<CheckMapClear>());
        }

        public void CreateSpeatPosition()
        {
            CreatePositionSetting(Character.Speat);
        }

        public void CreateOunPosition()
        {
            CreatePositionSetting(Character.Oun);
        }

        public void CreateRauPosition()
        {
            CreatePositionSetting(Character.Rau);
        }

        public void CreateSpeatAdolescenePosition()
        {
            CreatePositionSetting(Character.Speat_Adolescene);
        }

        public void CreateSpeatAdultPosition()
        {
            CreatePositionSetting(Character.Speat_Adult);
        }

        public void CreateSpeatChildPosition()
        {
            CreatePositionSetting(Character.Speat_Child);
        }

        public void CreateAllPosition()
        {
            CreateSpeatPosition();
            CreateOunPosition();
            CreateRauPosition();
            CreateSpeatAdolescenePosition();
            CreateSpeatAdultPosition();
            CreateSpeatChildPosition();
        }

        void CreatePositionSetting(Character createWho)
        {
            CharacterPositionSet temp = new CharacterPositionSet();
            Transform instant = new GameObject().transform;
            temp.who = createWho;
            temp.startPosition =
                Instantiate(instant, positionSetting.transform.position, Quaternion.identity,
                    positionSetting.transform);
            temp.startPosition.name = createWho + " Start Position";
            DestroyImmediate(instant.gameObject);
            positionSets.Add(temp);
        }

        public void RemoveSpeatPosition()
        {
            RemovePositionSetting(Character.Speat);
        }

        public void RemoveOunPosition()
        {
            RemovePositionSetting(Character.Oun);
        }

        public void RemoveRauPosition()
        {
            RemovePositionSetting(Character.Rau);
        }

        public void RemoveSpeatAdolescenePosition()
        {
            RemovePositionSetting(Character.Speat_Adolescene);
        }

        public void RemoveSpeatAdultPosition()
        {
            RemovePositionSetting(Character.Speat_Adult);
        }

        public void RemoveSpeatChildPosition()
        {
            RemovePositionSetting(Character.Speat_Child);
        }

        public void RemoveAllPosition()
        {
            RemoveSpeatPosition();
            RemoveOunPosition();
            RemoveRauPosition();
            RemoveSpeatAdolescenePosition();
            RemoveSpeatAdultPosition();
            RemoveSpeatChildPosition();
        }

        private void RemovePositionSetting(Character removeWho)
        {
            if (positionSets.Exists(item => item.who == removeWho))
            {
                CharacterPositionSet temp = positionSets.Find(item => item.who == removeWho);
                positionSets.Remove(temp);
            }
        }

        private void PositionSettingUpdate()
        {
            foreach (CharacterPositionSet item in positionSets)
            {
                if (item.startPosition)
                {
                    item.startPosition.name = item.who + " Start Position";
                }
            }
        }

        #endregion

        [Space(10)] public CameraViewType cameraViewType;
        public Vector3 camDis;
        public Vector3 camRot;

        public bool useFieldOfView;

        [ConditionalHideInInspector("useFieldOfView")]
        public float fieldOfView;

        public bool useClippingPlanes;

        [ConditionalHideInInspector("useClippingPlanes")]
        public float farClipPlane;

        public bool usePostProcessing;

        private void Start()
        {
            if (!Application.isPlaying)
            {
                CreateDefaultSetting();
            }
        }

        private void FixedUpdate()
        {
            if (!Application.isPlaying)
            {
                PositionSettingUpdate();
            }
            else
            {
                PlayerFixedUpdate();
            }
        }

        public void Initialize()
        {
            if (name != mapCode)
            {
                name = mapCode;
            }

            if (ui != null)
            {
                ui.name = map.name;

                ui.SetParent(PlayUIController.Instance.mapUi);
                ui.offsetMax = new Vector2(0, 0);
                ui.offsetMin = new Vector2(0, 0);
                ui.localScale = new Vector3(1, 1, 1);
            }

            AudioController.Instance.PlayBgm(bgm);

            mainCharacter = DataController.Instance.GetCharacter(Character.Main);
            followCharacters = DataController.Instance.GetFollowCharacters();
        }

        public void SetNextMapCode(string nextMapCode)
        {
            clearBoxList[0].nextSelectMapcode = nextMapCode;
        }

        public void MapClear(float waitTime)
        {
            Invoke("MapClear", waitTime);
        }

        public void MapClear()
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

        [Header("조이스틱 인풋 사용 유무")] public bool isJoystickInputUse;

        [Header("조이스틱 존재 유무")] public bool isJoystickNone;

        [ConditionalHideInInspector("isJoystickInputUse")]
        public JoystickType joystickType;

        private CharacterManager mainCharacter;
        private CharacterManager[] followCharacters;

        private void PlayerFixedUpdate()
        {
            if (mainCharacter == null)
            {
                return;
            }

            if (isJoystickInputUse)
            {
                JoystickController.Instance.UpdateJoystickInput(method);
            }

            mainCharacter.MoveCharacter(method, isJoystickInputUse);

            if (followCharacters != null)
            {
                foreach (var followCharacter in followCharacters)
                {
                    followCharacter.FollowMainCharacter(method);
                }
            }
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