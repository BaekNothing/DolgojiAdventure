using UnityEngine;

namespace CoreSystem
{
    public interface IDataContainor
    {
        UISystem.DialogueData DialogueData { get; }
    }

    [CreateAssetMenu(fileName = "DataContainor", menuName = "ScriptableObjects/DataContainor", order = 1)]
    public class DataContainor : ScriptableObject, IDataContainor
    {
        public static bool IsInitialized { get; private set; } = false;

        [SerializeField] UISystem.DialogueData _dialogueData;
        public UISystem.DialogueData DialogueData { get => _dialogueData; }

        public void Initialized()
        {
            DialogueData.Initialized();
            IsInitialized = true;
        }
    }
}
