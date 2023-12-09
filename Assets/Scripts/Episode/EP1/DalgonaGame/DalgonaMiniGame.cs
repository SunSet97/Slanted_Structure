using UnityEngine;
using UnityEngine.Serialization;
using Utility.Core;
using Utility.Game;

namespace Episode.EP1.DalgonaGame
{
    public class DalgonaMiniGame : MiniGame
    {
#pragma warning disable 0649
        [FormerlySerializedAs("dalgonaPanel")] public GameObject dalgonaGamePanel;

        [SerializeField] private DalgonaDragger[] dalgonaDragger;
#pragma warning restore 0649
        private int playIndex;

        public override void Play()
        {
            base.Play();

            dalgonaGamePanel.SetActive(true);
            JoystickController.Instance.StopSaveLoadJoyStick(true);

            foreach (var dragger in dalgonaDragger)
            {
                dragger.Init(isSuccess =>
                {
                    playIndex++;
                    if (isSuccess && playIndex < dalgonaDragger.Length)
                    {
                        dalgonaDragger[playIndex].Play();
                    }
                    else
                    {
                        EndPlay(isSuccess);
                    }
                });
            }

            dalgonaDragger[playIndex].Play();
        }

        public override void EndPlay(bool isSuccess)
        {
            base.EndPlay(isSuccess);
            JoystickController.Instance.StopSaveLoadJoyStick(false);
            dalgonaGamePanel.SetActive(false);
        }
    }
}