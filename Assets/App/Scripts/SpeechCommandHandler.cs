using CubeBouncer.Messages;
using UnityEngine;
using HoloToolkitExtensions.Messaging;

namespace CubeBouncer
{
    public class SpeechCommandHandler : MonoBehaviour
    {
        public void CreateNewGrid()
        {
            Messenger.Instance.Broadcast(new CreateNewGridMessage());
        }

        public void Drop(bool all)
        {
            Messenger.Instance.Broadcast(new DropMessage { All = all });
        }

        public void Revert(bool all)
        {
            Messenger.Instance.Broadcast(new RevertMessage { All = all });
        }
    }
}
