using UnityEngine;
using Utility.Core;
using Utility.Game;

namespace Episode.EP1.DalgonaGame
{
    public class DalgonaMiniGame : MiniGame
    {
#pragma warning disable 0649
        public GameObject dalgonaPanel;
        
        [SerializeField] private DalgonaDragger[] dalgonaDragger;
#pragma warning restore 0649
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
            dalgonaDragger[index].Init(() =>
            {
                index++;
                if (index < dalgonaDragger.Length)
                {
                    dalgonaDragger[index].Play();
                }
                else
                {
                    EndPlay(true);
                }
            });
            dalgonaDragger[index].Play();
        }

        public override void EndPlay(bool isSuccess)
        {
            base.EndPlay(isSuccess);
            JoystickController.Instance.StopSaveLoadJoyStick(false);
            dalgonaPanel.SetActive(false);
        }
    }
}