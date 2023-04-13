using System.Collections;
using Data;
using UnityEngine;
using Utility.Core;

namespace Episode.EP1
{
    public class EventMoveTo : MonoBehaviour
    {
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        
        [SerializeField] private float sec = 5f;
        [Range(5, 20)]
        [SerializeField] private int moveSpeed = 10;

        private static readonly int Speed = Animator.StringToHash("Speed");
        
        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("OnlyPlayerCheck");
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out CharacterManager character) &&
                character != DataController.Instance.GetCharacter(CustomEnum.Character.Main))
            {
                return;
            }
            
            StartCoroutine(MoveTo(character));
        }

        private IEnumerator MoveTo(CharacterManager character)
        {
            character.PickUpCharacter();
            JoystickController.Instance.StopSaveLoadJoyStick(true);
            
            character.transform.position = startPoint.position;
            character.transform.LookAt(endPoint);
            
            var waitForFixedUpdate = new WaitForFixedUpdate();
            character.CharacterAnimator.SetFloat(Speed, 1f);
            var t = 0f;
            while (t <= 1f)
            {
                t += Time.fixedDeltaTime / sec * moveSpeed;
                character.transform.position = Vector3.Lerp(startPoint.position, endPoint.position, t);
                yield return waitForFixedUpdate;
            }
            character.PutDownCharacter();
            character.CharacterAnimator.SetFloat(Speed, 0f);
            yield return new WaitForSeconds(0.5f);
            JoystickController.Instance.StopSaveLoadJoyStick(false);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isEditor || Application.isPlaying)
            {
                return;
            }
            Gizmos.color = Color.blue * 0.7f;
            Gizmos.DrawSphere(startPoint.position, 0.5f);
            Gizmos.DrawLine(startPoint.position, endPoint.position);
            Gizmos.DrawSphere(endPoint.position, 0.5f);
        }
    }
}
