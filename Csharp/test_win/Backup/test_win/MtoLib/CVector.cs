using System;

namespace MtoLib
{
    /*-------------------------------------------------------------------
    【機能】CVector3クラス
    -------------------------------------------------------------------*/
    /// <summary>
    /// float型の三次元ベクトル
    /// </summary>
    class CVector3
    {
        public float x;
        public float y;
        public float z;

        // コンストラクタ
        public CVector3()
        {
            this.x = 0.0f;
            this.y = 0.0f;
            this.z = 0.0f;
        }

        public CVector3(float fx, float fy, float fz)
        {
            this.x = fx;
            this.y = fy;
            this.z = fz;
        }

        public CVector3(CVector3 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        // 値設定
        public void Set(float fx, float fy, float fz)
        {
            this.x = fx;
            this.y = fy;
            this.z = fz;
        }
        
        public void Set(CVector3 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        // 符号変化
        public static CVector3 operator +(CVector3 v)
        {
            return new CVector3(v.x, v.y, v.z);
        }

        public static CVector3 operator -(CVector3 v)
        {
            return new CVector3(-v.x, -v.y, -v.z);
        }

        // 演算
        public static CVector3 operator +(CVector3 v1, CVector3 v2)
        {
            return new CVector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }
        
        public static CVector3 operator -(CVector3 v1, CVector3 v2)
        {
            return new CVector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static CVector3 operator +(CVector3 v1, float fnum)
        {
            return new CVector3(v1.x + fnum, v1.y + fnum, v1.z + fnum);
        }

        public static CVector3 operator -(CVector3 v1, float fnum)
        {
            return new CVector3(v1.x - fnum, v1.y - fnum, v1.z - fnum);
        }

        public static CVector3 operator *(CVector3 v1, float fnum)
        {
            return new CVector3(v1.x * fnum, v1.y * fnum, v1.z * fnum);
        }

        public static CVector3 operator /(CVector3 v1, float fnum)
        {
            if (fnum == 0)
            {
                throw new DivideByZeroException("Zero Division");
            }
            return new CVector3(v1.x / fnum, v1.y / fnum, v1.z / fnum);
        }

        // 評価
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            CVector3 v = obj as CVector3;
            if ((object)v == null) return false;

            return (this.x == v.x) && (this.y == v.y) && (this.z == v.z);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(CVector3 v1, CVector3 v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(CVector3 v1, CVector3 v2)
        {
            return !v1.Equals(v2);
        }

        /// <summary>
        /// ベクトルの長さを取得
        /// </summary>
        /// <returns>ベクトルの長さ</returns>
        public float Length()
        {
            return (float)Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
        }


        /// <summary>
        /// ベクトル長の2乗を取得
        /// </summary>
        /// <returns>ベクトルの長さの2乗</returns>
        public float LengthSq()
        {
            return (this.x * this.x + this.y * this.y + this.z * this.z);
        }

        /// <summary>
        /// 単位ベクトルを取得
        /// </summary>
        /// <returns>単位ベクトル</returns>
        public CVector3 Normalize()
        {
            float len;

            len = this.Length();
            len = len > 0.0f ? (1.0f / len) : 0.0f;

            return new CVector3(this.x * len, this.y * len, this.z * len);
        }

        /// <summary>
        /// 外積を求める
        /// </summary>
        /// <param name="v1">入力ベクトル1</param>
        /// <param name="v2">入力ベクトル2</param>
        /// <returns>外積ベクトル</returns>
        public static CVector3 Cross(CVector3 v1, CVector3 v2)
        {
            CVector3 v = new CVector3();
            v.x = v1.y * v2.z - v1.z * v2.y;
            v.y = v1.z * v2.x - v1.x * v2.z;
            v.z = v1.x * v2.y - v1.y * v2.x;
            return v;
        }

        /// <summary>
        /// 内積を求める
        /// </summary>
        /// <param name="v1">入力ベクトル1</param>
        /// <param name="v2">入力ベクトル2</param>
        /// <returns>内積値</returns>
        public static float Dot(CVector3 v1, CVector3 v2)
        {
            return (v1.x * v2.x + v1.y * v2.y + v1.z * v2.z);
        }

        /// <summary>
        /// 法線ベクトルを求める
        /// </summary>
        /// <param name="v1">入力ベクトル1</param>
        /// <param name="v2">入力ベクトル2</param>
        /// <param name="v3">入力ベクトル3</param>
        /// <returns>法線ベクトル</returns>
        public static CVector3 Normal(CVector3 v1, CVector3 v2, CVector3 v3)
        {
            CVector3 v = new CVector3();
            CVector3 a = new CVector3();
            CVector3 b = new CVector3();

            a = v2 - v1;
            b = v3 - v2;
            v = CVector3.Cross(a, b);
            v = v.Normalize();
            return v;
        }
    }

    /*-------------------------------------------------------------------
    【機能】CVector4クラス
    -------------------------------------------------------------------*/
    /// <summary>
    /// float型の四次元ベクトル
    /// </summary>
    class CVector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        // コンストラクタ
        public CVector4()
        {
            this.x = 0.0f;
            this.y = 0.0f;
            this.z = 0.0f;
            this.w = 0.0f;
        }

        public CVector4(float fx, float fy, float fz, float fw)
        {
            this.x = fx;
            this.y = fy;
            this.z = fz;
            this.w = fw;
        }

        public CVector4(CVector3 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
            this.w = 1.0f;
        }

        public CVector4(CVector4 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
            this.w = v.w;
        }

        // 値設定
        public void Set(float fx, float fy, float fz, float fw)
        {
            this.x = fx;
            this.y = fy;
            this.z = fz;
            this.w = fw;
        }

        public void Set(CVector3 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
            this.w = 1.0f;
        }

        public void Set(CVector4 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
            this.w = v.w;
        }

        // 符号変化
        public static CVector4 operator +(CVector4 v)
        {
            return new CVector4(v.x, v.y, v.z, v.w);
        }

        public static CVector4 operator -(CVector4 v)
        {
            return new CVector4(-v.x, -v.y, -v.z, -v.w);
        }

        // 演算
        public static CVector4 operator +(CVector4 v1, CVector4 v2)
        {
            return new CVector4(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
        }

        public static CVector4 operator -(CVector4 v1, CVector4 v2)
        {
            return new CVector4(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z, v1.w - v2.w);
        }

        public static CVector4 operator +(CVector4 v1, float fnum)
        {
            return new CVector4(v1.x + fnum, v1.y + fnum, v1.z + fnum, v1.w + fnum);
        }

        public static CVector4 operator -(CVector4 v1, float fnum)
        {
            return new CVector4(v1.x - fnum, v1.y - fnum, v1.z - fnum, v1.w - fnum);
        }

        public static CVector4 operator *(CVector4 v1, float fnum)
        {
            return new CVector4(v1.x * fnum, v1.y * fnum, v1.z * fnum, v1.w * fnum);
        }

        public static CVector4 operator /(CVector4 v1, float fnum)
        {
            if (fnum == 0)
            {
                throw new DivideByZeroException("Zero Division");
            }
            return new CVector4(v1.x / fnum, v1.y / fnum, v1.z / fnum, v1.w / fnum);
        }

        // 評価
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            CVector4 v = obj as CVector4;
            if ((object)v == null) return false;

            return (this.x == v.x) && (this.y == v.y) && (this.z == v.z) && (this.w == v.w);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(CVector4 v1, CVector4 v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(CVector4 v1, CVector4 v2)
        {
            return !v1.Equals(v2);
        }

        /// <summary>
        /// ベクトルの長さを求める
        /// </summary>
        /// <returns>ベクトルの長さ</returns>
        public float Length()
        {
            return (float)Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
        }

        /// <summary>
        /// ベクトル長の2乗を求める
        /// </summary>
        /// <returns>ベクトル長の2乗</returns>
        public float LengthSq()
        {
            return (this.x * this.x + this.y * this.y + this.z * this.z);
        }

        /// <summary>
        /// 単位ベクトルを求める
        /// </summary>
        /// <returns>単位ベクトル</returns>
        public CVector4 Normalize()
        {
            float len;

            len = this.Length();
            len = len > 0.0f ? (1.0f / len) : 0.0f;

            return new CVector4(this.x * len, this.y * len, this.z * len, this.w);
        }

        /// <summary>
        /// 外積を求める
        /// </summary>
        /// <param name="v1">入力ベクトル1</param>
        /// <param name="v2">入力ベクトル2</param>
        /// <returns>外積ベクトル</returns>
        public static CVector4 Cross(CVector4 v1, CVector4 v2)
        {
            CVector4 v = new CVector4();
            v.x = v1.y * v2.z - v1.z * v2.y;
            v.y = v1.z * v2.x - v1.x * v2.z;
            v.z = v1.x * v2.y - v1.y * v2.x;
            v.w = 0.0f;
            return v;
        }

        /// <summary>
        /// 内積を求める
        /// </summary>
        /// <param name="v1">入力ベクトル1</param>
        /// <param name="v2">入力ベクトル2</param>
        /// <returns>内積値</returns>
        public static float Dot(CVector4 v1, CVector4 v2)
        {
            return (v1.x * v2.x + v1.y * v2.y + v1.z * v2.z);
        }

        /// <summary>
        ///  法線ベクトルを求める
        /// </summary>
        /// <param name="v1">入力ベクトル1</param>
        /// <param name="v2">入力ベクトル2</param>
        /// <param name="v3">入力ベクトル3</param>
        /// <returns>法線ベクトル</returns>
        public static CVector4 Normal(CVector4 v1, CVector4 v2, CVector4 v3)
        {
            CVector4 v = new CVector4();
            CVector4 a = new CVector4();
            CVector4 b = new CVector4();

            a = v2 - v1;
            b = v3 - v2;
            v = CVector4.Cross(a, b);
            v = v.Normalize();
            return v;
        }
    }
}
