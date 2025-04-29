using UnityEngine;
using System;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour // 抽象クラス
{
    // Singletonインスタンスの保持
    static T instance;
    public static T Instance
    {
        get
        {
            // インスタンスが未作成の場合は探す
            if (instance == null)
            {
                Type t = typeof(T);

                // シーン内のオブジェクトからインスタンスを探す
                instance = (T)FindObjectOfType(t);
                if (instance == null)
                {
                    Debug.LogError(t + " をアタッチしているGameObjectはありません");
                }
            }
            return instance;
        }
    }

    // Awakeメソッドでインスタンスのチェックを行う
    virtual protected void Awake()
    {
        // 他のゲームオブジェクトにアタッチされているか調べる
        // アタッチされている場合は破棄する。
        CheckInstance();
    }

    // インスタンスのチェックを行うメソッド
    protected bool CheckInstance()
    {
        if (instance == null)
        {
            // インスタンスが未設定の場合は現在のインスタンスを設定
            instance = this as T;
            return true;
        }
        else if (Instance == this)
        {
            // 現在のインスタンスが既存のインスタンスと同じ場合
            return true;
        }

        // 既存のインスタンスがある場合は現在のオブジェクトを破棄
        Destroy(this);
        return false;
    }
}
