using UnityEngine;

public class PlayerActionState : MonoBehaviour
{
    public bool IsBusy { get; private set; }

    public void SetBusy(bool v)
    {
        IsBusy = v;
    }
}
