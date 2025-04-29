using UnityEngine.SceneManagement;
using UnityEngine;

public class RuleScene : MonoBehaviour
{
    public void ChangeScene()
    {
        SceneManager.LoadScene("Title");
    }
}
