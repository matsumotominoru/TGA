using System;
using System.Collections.Generic;

namespace MtoLib
{
    /*-------------------------------------------------------------------
    【機能】キーフレームクラス
    -------------------------------------------------------------------*/
    /// <summary>
    /// キーフレームデータ
    /// </summary>
    public class KeyData
    {
        /// <summary>
        /// 制御キー・終了
        /// </summary>
        public const float KEY_END = -1.0f;

        /// <summary>
        /// 制御キー・ループ
        /// </summary>
        public const float KEY_LOOP = -2.0f;

        public float key;   // 0.0～1.0(持続時間に対する割合)
        public float val;   // そのときの値

        public KeyData()
        {
            key = val = 0.0f;
        }

        public KeyData(float key, float val)
        {
            this.key = key;
            this.val = val;
        }
    }

    /// <summary>
    /// キーフレーム本体
    /// </summary>
    class CKeyframe
    {
        public UInt32 baseTime;         // 開始時間
        public UInt32 duration;         // 全体の持続時間
        public bool bLoop;              // ループする？（強制）
        public bool keyLoop;            // キーフレームデータのループ
        public List<KeyData> keyList;   // キーフレームデータリスト

        public CKeyframe()
        {
            baseTime = 0;
            duration = 1000;
            bLoop = false;
            keyLoop = false;
            keyList = new List<KeyData>();
        }

        /// <summary>
        /// 実行時間取得
        /// </summary>
        /// <param name="nowTime">現在の時間（ミリ秒）</param>
        /// <returns>実行時間</returns>
        private UInt32 GetRunTime(UInt32 nowTime)
        {
            UInt32 ntime = 0;

            // タイマーが１周した？
            if (nowTime < baseTime)
            {
                ntime = UInt32.MaxValue - baseTime + nowTime;
            }
            else
            {
                ntime = nowTime - baseTime;
            }

            return ntime;
        }

        /// <summary>
        /// 持続時間の設定
        /// </summary>
        /// <param name="duration">持続時間（ミリ秒）</param>
        public void SetDuration(UInt32 duration)
        {
            this.duration = duration;
        }

        /// <summary>
        /// ループの設定
        /// </summary>
        /// <param name="bLoop">ループ false:しない true:する</param>
        public void SetLoop(bool bLoop)
        {
            this.bLoop = bLoop;
        }

        /// <summary>
        /// キーデータの登録
        /// </summary>
        /// <param name="kDat">登録するキーデータ</param>
        public void SetKeyData(KeyData kDat)
        {
            // 制御キーなら登録しない
            if (kDat.key == MtoLib.KeyData.KEY_LOOP)
            {
                bLoop = keyLoop = true;
            }
            else if (kDat.key == MtoLib.KeyData.KEY_END)
            {
                bLoop = keyLoop = false;
            }
            else
            {
                // キーデータの登録
                keyList.Add(kDat);
            }
        }

        /// <summary>
        /// キーデータの登録
        /// </summary>
        /// <param name="datArray">登録するキーデータ</param>
        public void SetKeyData(KeyData[] datArray)
        {
            foreach (KeyData dat in datArray)
            {
                this.SetKeyData(dat);
            }
        }

        /// <summary>
        /// キーフレーム開始
        /// </summary>
        /// <param name="baseTime">開始時間（ミリ秒）</param>
        public void Start(UInt32 baseTime)
        {
            this.baseTime = baseTime;
        }

        /// <summary>
        /// キーフレームデータのインデックス取得
        /// </summary>
        /// <param name="nowTime">現在の時間（ミリ秒）</param>
        /// <param name="nkey">現在のキーフレームの割合</param>
        /// <returns>キーフレームデータのインデックス</returns>
        public int GetIndex(UInt32 nowTime, ref float nkey)
        {
            UInt32 nTime = this.GetRunTime(nowTime);
            nTime %= (UInt32)duration; // ループ実行対策

            // 割合に変換
            nkey = (float)nTime / (float)duration;

            // 対応するキーフレームインデックスを探す
            int i, idx;
            for (i = idx = 0; i < keyList.Count; i++)
            {
                if (keyList[i].key <= nkey)
                {
                    idx = i;
                }
                else
                {
                    break;
                }
            }

            return idx;
        }

        /// <summary>
        /// キーフレームデータのインデックス取得
        /// </summary>
        /// <param name="nowTime">現在の時間（ミリ秒）</param>
        /// <returns>キーフレームデータのインデックス</returns>
        public int GetIndex(UInt32 nowTime)
        {
            float nkey = 0.0f;
            return this.GetIndex(nowTime, ref nkey);
        }

        /// <summary>
        /// キーフレームデータの取得
        /// </summary>
        /// <param name="idx">キーフレームデータのインデックス番号</param>
        /// <returns>キーフレームデータ</returns>
        public KeyData GetKeyData(int idx)
        {
            return keyList[idx];

        }

        /// <summary>
        /// キーフレームの値取得
        /// </summary>
        /// <param name="val">値の保存先</param>
        /// <param name="nowTime">現在の時間（ミリ秒）</param>
        /// <param name="bFlg">線形補間を行う？</param>
        /// <returns>キーフレーム true:終了 false:実行中</returns>
        public bool GetValue(ref float val, UInt32 nowTime, bool bFlg)
        {
            int sidx, eidx;
            float nkey;
            
            // アニメーション終了チェック
            if (!bLoop || !keyLoop)
            {
                if (duration <= this.GetRunTime(nowTime))
                {
                    val = keyList[keyList.Count - 1].val;
                    return true;
                }
            }

            // キーフレームインデックス取得
            nkey = 0.0f;
            sidx = this.GetIndex(nowTime, ref nkey);
            eidx = sidx + 1;

            // 線形補間を行う？
            if (bFlg)
            {
                if (sidx == (keyList.Count - 1))
                { // 最後のキー？
                    val = keyList[(keyList.Count - 1)].val;
                }
                else
                {
                    float dkey, dval, slope;

                    // キーフレーム間の値を求める
                    dkey  = keyList[eidx].key - keyList[sidx].key;
                    dval  = keyList[eidx].val - keyList[sidx].val;
                    slope = dval / dkey; // sidx-didx間の増減量

                    // 現在値を求める
                    val = slope * (nkey - keyList[sidx].key) + keyList[sidx].val;
                }
            }
            else
            {
                val = keyList[sidx].val;
            }

            return false;
        }
    }
}
