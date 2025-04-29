using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenController : MonoBehaviour
{
    // プレイボタンがクリックされたときに呼び出されるメソッド
    public void StartGame()
    {
        // Gameシーンに遷移
        SceneManager.LoadScene("Game");
    }

    // ルールボタンがクリックされたときに呼び出されるメソッド
    public void ShowRules()
    {
        // Ruleシーンに遷移
        SceneManager.LoadScene("Rule");
    }
}
