using JetBrains.Annotations;
using UnityEngine;

public class PracticeButtonScript : MonoBehaviour 
{
    [UsedImplicitly]
    void OnClick()
    {
        Debug.Log("PracticeButtonScript OnClick");

        Application.LoadLevel("PracticeGameScene");
    }
}
