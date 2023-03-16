using System.Collections;
using Data.GamePlay;
using UnityEngine;
using Utility.Core;

namespace Episode.EP1.MarketGame.Dalgona
{
    public class DalgonaGame : Game
    {
        public GameObject dalgonaPanel;
        public DalgonaDrager[] dalgona;
        
        private int index;

        public override void Play()
        {
            if (index >= dalgona.Length)
            {
                Debug.LogError("dalgona index Error");
                return;
            }
            base.Play();

            dalgonaPanel.SetActive(true);
            JoystickController.Instance.StopSaveLoadJoyStick(true);
            dalgona[index].Init();
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
