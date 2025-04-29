using TMPro;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    // 各クラスのインスタンス（インスペクターで設定）
    [SerializeField] FieldCard  _fieldCard;
    [SerializeField] Deck       _deck;
    [SerializeField] SelfPlayer _selfPlayer;

    // 各テキスト設定
    public TextMeshProUGUI FieldText;
    public TextMeshProUGUI PassText;
    public TextMeshProUGUI HandTotalText;
    public TextMeshProUGUI TurnText;
    public TextMeshProUGUI PointText;
    public TextMeshProUGUI DobonText;
    public TextMeshProUGUI JokerText;
    public TextMeshProUGUI CardText;
    public TextMeshProUGUI RateText;

    // ポイントだけResultSceneで使うため静的にする
    public static TextMeshProUGUI pointText { get; private set; }

    void Start()
    {
        // 静的変数に代入
        pointText = PointText;
    }

    void Update()
    {
        // 毎フレーム描画するテキスト
        FieldText.text      = "Number : " + _fieldCard._fieldNumber.ToString() + "\nSuit : " + _fieldCard._suitText.ToString();
        HandTotalText.text  = "HandTotal : " + _selfPlayer._handTotal.ToString();
        PointText.text      = "Your Point : " + Game.SelfPoint.ToString() + "\n\nCPU Point : " + Game.EnemyPoint.ToString();
        JokerText.text      = "Joker : " + BasePlayer.OllJoker.ToString();
        CardText.text       = "Card : " + _deck.ListCount.ToString();
        RateText.text       = "Rate : " + Game.Instance._bonus.ToString();

        // どちらのターンか表示
        if (BasePlayer.SelfPlayerTurn)
        {
            TurnText.text = "Your Turn";
        }
        else
        {
            TurnText.text = "CPU Turn";
        }
    }

    // 相手がパスしたときのテキスト
    public void PassNotice()
    {
        PassText.text = "CPU Passed !";
    }

    // パステキストクリア用
    public void PassClearText()
    { 
        PassText.text = "";
    }

    // ドボンしたときのテキスト
    public void ShowDobonText()
    {
        // どちらがドボンしたか判定
        if (Game.Instance.SelfWinner)
        {
            DobonText.text = "Player Dobon!";
        }
        else
        {
            DobonText.text = "CPU Dobon!";
        }
    }

    // ドボンテキストクリア用
    public void DobonClearText()
    {
        DobonText.text = "";
    }
}
