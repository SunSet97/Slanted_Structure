using System;
using System.Collections;
using Play;
using UnityEngine;
using Utility.Core;

namespace Episode.EP1.MarketGame.Dalgona
{
    public class DalgonaGame : MonoBehaviour, IGamePlayable
    {
        public GameObject dalgonaPanel;

        public DalgonaDrager[] dalgona;
        private int index = 0;

        public bool IsPlay { get; set; }
        public Action OnEndPlay { get; set; }

        public void Play()
        {
            if (index >= dalgona.Length)
            {
                Debug.LogError("dalgona index Error");
                return;
            }

            dalgonaPanel.SetActive(true);
            JoystickController.Instance.StopSaveLoadJoyStick(true);
            IsPlay = true;
            dalgona[index].Init();
            StartCoroutine(WaitDalgonaEnd());
        }

        public void EndPlay()
        {
            JoystickController.Instance.StopSaveLoadJoyStick(false);
            OnEndPlay?.Invoke();
            IsPlay = false;
            dalgonaPanel.SetActive(false);
        }

        private IEnumerator WaitDalgonaEnd()
        {
            yield return new WaitUntil(() => dalgona[index].isEnd);
            if (index + 1 < dalgona.Length)
            {
                index++;
                dalgona[index].Init();
                StartCoroutine(WaitDalgonaEnd());
            }
            else
            {
                EndPlay();
            }
        }
    }
}
