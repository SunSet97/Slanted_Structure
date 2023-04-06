using System;
using System.Collections;
using Data.GamePlay;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Core;

namespace Episode.EP1.DalgonaGame
{
    public class DalgonaMiniGame : MiniGame
    {
        public GameObject dalgonaPanel;
        [FormerlySerializedAs("dalgona")] [SerializeField] private DalgonaDrager[] dalgonaDragger;
        
        private int index;

        public override void Play()
        {
            if (index >= dalgonaDragger.Length)
            {
                Debug.LogError("dalgona index Error");
                return;
            }
            base.Play();

            dalgonaPanel.SetActive(true);
            JoystickController.Instance.StopSaveLoadJoyStick(true);
            dalgonaDragger[index].Init();
            StartCoroutine(WaitDalgonaEnd());
        }

        public override void EndPlay()
        {
            base.EndPlay();
            JoystickController.Instance.StopSaveLoadJoyStick(false);
            dalgonaPanel.SetActive(false);
        }

        private IEnumerator WaitDalgonaEnd()
        {
            yield return new WaitUntil(() => dalgonaDragger[index].isEnd);
            if (index + 1 < dalgonaDragger.Length)
            {
                index++;
                dalgonaDragger[index].Init();
                StartCoroutine(WaitDalgonaEnd());
            }
            else
            {
                EndPlay();
            }
        }
    }
}
