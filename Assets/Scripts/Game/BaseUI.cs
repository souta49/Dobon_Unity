using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BaseUI : SingletonMonoBehaviour<BaseUI>// シングルトンクラス
{
    // ボタン（インスペクターで設定）
    [SerializeField] Button drawButton;
    [SerializeField] Button passButton;
    [SerializeField] Button dobonButton;
    [SerializeField] Button HeartButton;
    [SerializeField] Button DiamondButton;
    [SerializeField] Button SpadeButton;
    [SerializeField] Button ClubButton;

    // フラグ
    public bool _canDobon; 
    public bool _canDraw;  
    public bool _canPass;
    public bool _canSuite;
    public bool _onCardClick;
    public bool _onMouseDown;
    public bool isDrawButtonClicked;
    public bool isDobonButtonClicked;
    public bool isPassButtonClicked;

    // 変数
    public int PressedButton { get; set; } = 0;

    void Start()
    {
        // ボタンにリスナーを追加
        drawButton.onClick.AddListener(OnDrawClicked);
        dobonButton.onClick.AddListener(OnDobonClicked);
        passButton.onClick.AddListener(OnPassClicked);

        // ボタンにリスナーを追加しリスナーで変数を変更
        HeartButton.onClick.AddListener(() => ButtonPressed(1));
        DiamondButton.onClick.AddListener(() => ButtonPressed(2));
        SpadeButton.onClick.AddListener(() => ButtonPressed(3));
        ClubButton.onClick.AddListener(() => ButtonPressed(4));
    }

    // ボタンの状態とマウスのクリックを常に監視
    void Update()
    {
        // ボタンの状態を更新
        UpdateButtonStates();

        // フラグがオンで自分のターンで左クリックされたら
        if (Input.GetMouseButtonUp(0) && BasePlayer.SelfPlayerTurn && _onMouseDown)
        {
            OnMouseClick();
        }
    }

    // ボタンの状態
    void UpdateButtonStates()
    {
        // ボタンを押していいか判定
        drawButton.interactable = _canDraw;
        passButton.interactable = _canPass;
        dobonButton.interactable = _canDobon;

        // フラグでボタンの表示（オブジェクト）を管理
        if (_canSuite)
        {
            HeartButton.gameObject.SetActive(true);
            DiamondButton.gameObject.SetActive(true);
            SpadeButton.gameObject.SetActive(true);
            ClubButton.gameObject.SetActive(true);
        }
        else
        {
            HeartButton.gameObject.SetActive(false);
            DiamondButton.gameObject.SetActive(false);
            SpadeButton.gameObject.SetActive(false);
            ClubButton.gameObject.SetActive(false);
        }

        // ボタンの明るさを判定
        UpdateButtonColor(drawButton, _canDraw);
        UpdateButtonColor(passButton, _canPass);
        UpdateButtonColor(dobonButton, _canDobon);
    }

    // ボタンの明るさ設定処理
    void UpdateButtonColor(Button button, bool isEnabled)
    {
        // 現在のボタンのカラーブロックを取得
        ColorBlock colors = button.colors;
        
        // ボタンの状況がtrueなら白、falseならグレー
        colors.normalColor = isEnabled ? Color.white : Color.gray;

        // 更新されたカラーブロックをボタンに適用
        button.colors = colors;
    }

    // マウスクリック処理
    bool OnMouseClick()
    {
        // マウスの位置からRayを飛ばしてオブジェクトを取得
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        // オブジェクトにヒットたら
        if (hit.collider != null)
        {
            // ヒットしたオブジェクトを代入
            GameObject clickedObj = hit.collider.gameObject;

            // 自分の手札かつ、出せる手札（暗くなってない）なら
            if (clickedObj.tag == "Self" &&
                CardPrefabs.Instance.spriteRendererCache.TryGetValue(clickedObj, out var spriteRenderer) &&
                spriteRenderer.color != Color.gray)
            {
                // クリックしたオブジェクトを渡す
                SelfPlayer.ClickObject = clickedObj;
                _onCardClick = true;
                return true;
            }
        }
        return false;
    }

    // ボタンが押されたらフラグをオンにする
    public void OnDrawClicked() => isDrawButtonClicked = true;
    public void OnPassClicked() => isPassButtonClicked = true;
    public void OnDobonClicked() => isDobonButtonClicked = true;

    // 押されたボタンの数字（スート）を代入
    void ButtonPressed(int buttonNumber)
    {
        PressedButton = buttonNumber;
    }
}
