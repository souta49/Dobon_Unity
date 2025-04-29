using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class ResultScene : MonoBehaviour
{
    // テキストとボタンの設定
    public TextMeshProUGUI pointText;
    public TextMeshProUGUI ResultText;
    public Button ReturnTitle;

    void Start()
    {
        // ポイントのテキスト表示
        pointText.text = TextManager.pointText.text;

        // プレイヤーの勝敗表示
        if (Game.SelfWin)
        {
            ResultText.text = "Your Win !";
        }
        else
        {
            ResultText.text = "Your Loss";
        }

        // Titleボタンのリスナー追加
        ReturnTitle.onClick.AddListener(ReturnToTitle);
    }

    // Titleボタンの設定
    void ReturnToTitle()
    {
        // Titleシーンへ遷移
        SceneManager.LoadScene("Title");
    }
}
