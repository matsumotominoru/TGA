//#define CLEAR_TRANS   // 平行移動の値をクリアする？

using System;

namespace MtoLib
{ 
    /*-------------------------------------------------------------------
    【機能】CMatrix4クラス
    -------------------------------------------------------------------*/
    /// <summary>
    /// float型の4x4行列
    /// </summary>
    class CMatrix4
    {
        public float[,] m = new float[4, 4];

        // コンストラクタ
        public CMatrix4() {}

        public CMatrix4(CMatrix4 mat) 
        {
            m[0, 0] = mat.m[0, 0]; m[0, 1] = mat.m[0, 1]; m[0, 2] = mat.m[0, 2]; m[0, 3] = mat.m[0, 3];
            m[1, 0] = mat.m[1, 0]; m[1, 1] = mat.m[1, 1]; m[1, 2] = mat.m[1, 2]; m[1, 3] = mat.m[1, 3];
            m[2, 0] = mat.m[2, 0]; m[2, 1] = mat.m[2, 1]; m[2, 2] = mat.m[2, 2]; m[2, 3] = mat.m[2, 3];
            m[3, 0] = mat.m[3, 0]; m[3, 1] = mat.m[3, 1]; m[3, 2] = mat.m[3, 2]; m[3, 3] = mat.m[3, 3];
	    }

        public CMatrix4(float m11, float m12, float m13, float m14,
			     float m21, float m22, float m23, float m24,
			     float m31, float m32, float m33, float m34,
			     float m41, float m42, float m43, float m44) 
        {
		    m[0, 0] = m11; m[0, 1] = m12; m[0, 2] = m13; m[0, 3] = m14;
		    m[1, 0] = m21; m[1, 1] = m22; m[1, 2] = m23; m[1, 3] = m24;
		    m[2, 0] = m31; m[2, 1] = m32; m[2, 2] = m33; m[2, 3] = m34;
		    m[3, 0] = m41; m[3, 1] = m42; m[3, 2] = m43; m[3, 3] = m44;
	    }

	    // 値設定
	    public void Set(float m11, float m12, float m13, float m14,
			     float m21, float m22, float m23, float m24,
			     float m31, float m32, float m33, float m34,
			     float m41, float m42, float m43, float m44) 
        {
		    m[0, 0] = m11; m[0, 1] = m12; m[0, 2] = m13; m[0, 3] = m14;
		    m[1, 0] = m21; m[1, 1] = m22; m[1, 2] = m23; m[1, 3] = m24;
		    m[2, 0] = m31; m[2, 1] = m32; m[2, 2] = m33; m[2, 3] = m34;
		    m[3, 0] = m41; m[3, 1] = m42; m[3, 2] = m43; m[3, 3] = m44;
	    }

        public void Set(float[] mat)
        {
		    for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    m[i, j] = mat[i * 4 + j];
                }
            }
	    }

        public void Set(float[,] mat) 
        {
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    m[i, j] = mat[i, j];
                }
            }
        }

        // 符号変更
	    public static CMatrix4 operator +(CMatrix4 tm) 
        {
		    return new CMatrix4(tm);
        }

        public static CMatrix4 operator -(CMatrix4 tm)
        {
            return new CMatrix4(-tm.m[0, 0], -tm.m[0, 1], -tm.m[0, 2], -tm.m[0, 3],
                                -tm.m[1, 0], -tm.m[1, 1], -tm.m[1, 2], -tm.m[1, 3],
                                -tm.m[2, 0], -tm.m[2, 1], -tm.m[2, 2], -tm.m[2, 3],
                                -tm.m[3, 0], -tm.m[3, 1], -tm.m[3, 2], -tm.m[3, 3]);
        }

        // 演算
	    public static CMatrix4 operator + (CMatrix4 m1, CMatrix4 m2) 
        {
		    return new CMatrix4(m1.m[0, 0] + m2.m[0, 0], m1.m[0, 1] + m2.m[0, 1], m1.m[0, 2] + m2.m[0, 2], m1.m[0, 3] + m2.m[0, 3],
                                m1.m[1, 0] + m2.m[1, 0], m1.m[1, 1] + m2.m[1, 1], m1.m[1, 2] + m2.m[1, 2], m1.m[1, 3] + m2.m[1, 3],
                                m1.m[2, 0] + m2.m[2, 0], m1.m[2, 1] + m2.m[2, 1], m1.m[2, 2] + m2.m[2, 2], m1.m[2, 3] + m2.m[2, 3],
                                m1.m[3, 0] + m2.m[3, 0], m1.m[3, 1] + m2.m[3, 1], m1.m[3, 2] + m2.m[3, 2], m1.m[3, 3] + m2.m[3, 3]);
	    }

	    public static CMatrix4 operator - (CMatrix4 m1, CMatrix4 m2) 
        {
		    return new CMatrix4(m1.m[0, 0] - m2.m[0, 0], m1.m[0, 1] - m2.m[0, 1], m1.m[0, 2] - m2.m[0, 2], m1.m[0, 3] - m2.m[0, 3],
                                m1.m[1, 0] - m2.m[1, 0], m1.m[1, 1] - m2.m[1, 1], m1.m[1, 2] - m2.m[1, 2], m1.m[1, 3] - m2.m[1, 3],
                                m1.m[2, 0] - m2.m[2, 0], m1.m[2, 1] - m2.m[2, 1], m1.m[2, 2] - m2.m[2, 2], m1.m[2, 3] - m2.m[2, 3],
                                m1.m[3, 0] - m2.m[3, 0], m1.m[3, 1] - m2.m[3, 1], m1.m[3, 2] - m2.m[3, 2], m1.m[3, 3] - m2.m[3, 3]);
	    }

	    public static CMatrix4 operator * (CMatrix4 m1, CMatrix4 m2) {
		    CMatrix4 tm = new CMatrix4();

            for (int i = 0; i < 4; i++) {
		        for (int j = 0; j < 4; j++) {
			        tm.m[i, j] = m1.m[i, 0] * m2.m[0, j] + m1.m[i, 1] * m2.m[1, j] +
                                 m1.m[i, 2] * m2.m[2, j] + m1.m[i, 3] * m2.m[3, j];
		        }
	        }
		    return tm;
	    }

	    public static CMatrix4 operator + (CMatrix4 m1, float f) 
        {
		    return new CMatrix4(m1.m[0, 0] + f, m1.m[0, 1] + f, m1.m[0, 2] + f, m1.m[0, 3] + f,
                                m1.m[1, 0] + f, m1.m[1, 1] + f, m1.m[1, 2] + f, m1.m[1, 3] + f,
                                m1.m[2, 0] + f, m1.m[2, 1] + f, m1.m[2, 2] + f, m1.m[2, 3] + f,
                                m1.m[3, 0] + f, m1.m[3, 1] + f, m1.m[3, 2] + f, m1.m[3, 3] + f);
	    }

	    public static CMatrix4 operator - (CMatrix4 m1, float f)
        {
		    return new CMatrix4(m1.m[0, 0] - f, m1.m[0, 1] - f, m1.m[0, 2] - f, m1.m[0, 3] - f,
                                m1.m[1, 0] - f, m1.m[1, 1] - f, m1.m[1, 2] - f, m1.m[1, 3] - f,
                                m1.m[2, 0] - f, m1.m[2, 1] - f, m1.m[2, 2] - f, m1.m[2, 3] - f,
                                m1.m[3, 0] - f, m1.m[3, 1] - f, m1.m[3, 2] - f, m1.m[3, 3] - f);
	    }

	    public static CMatrix4 operator * (CMatrix4 m1, float f)
        {
		    return new CMatrix4(m1.m[0, 0] * f, m1.m[0, 1] * f, m1.m[0, 2] * f, m1.m[0, 3] * f,
                                m1.m[1, 0] * f, m1.m[1, 1] * f, m1.m[1, 2] * f, m1.m[1, 3] * f,
                                m1.m[2, 0] * f, m1.m[2, 1] * f, m1.m[2, 2] * f, m1.m[2, 3] * f,
                                m1.m[3, 0] * f, m1.m[3, 1] * f, m1.m[3, 2] * f, m1.m[3, 3] * f);
	    }

	    public static CMatrix4 operator / (CMatrix4 m1, float f) {
            if (f == 0) {
                throw new DivideByZeroException("Zero Division");
            }

            float fInv = 1.0f / f;
		    return new CMatrix4(m1.m[0, 0] * fInv, m1.m[0, 1] * fInv, m1.m[0, 2] * fInv, m1.m[0, 3] * fInv,
                                m1.m[1, 0] * fInv, m1.m[1, 1] * fInv, m1.m[1, 2] * fInv, m1.m[1, 3] * fInv,
                                m1.m[2, 0] * fInv, m1.m[2, 1] * fInv, m1.m[2, 2] * fInv, m1.m[2, 3] * fInv,
                                m1.m[3, 0] * fInv, m1.m[3, 1] * fInv, m1.m[3, 2] * fInv, m1.m[3, 3] * fInv);
	    }

	    // 評価
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            CMatrix4 tm = obj as CMatrix4;
            if ((object)tm == null) return false;

		    return ((m[0, 0] == tm.m[0, 0]) && (m[0, 1] == tm.m[0, 1]) && (m[0, 2] == tm.m[0, 2]) && (m[0, 3] == tm.m[0, 3]) &&
				    (m[1, 0] == tm.m[1, 0]) && (m[1, 1] == tm.m[1, 1]) && (m[1, 2] == tm.m[1, 2]) && (m[1, 3] == tm.m[1, 3]) &&
				    (m[2, 0] == tm.m[2, 0]) && (m[2, 1] == tm.m[2, 1]) && (m[2, 2] == tm.m[2, 2]) && (m[2, 3] == tm.m[2, 3]) &&
				    (m[3, 0] == tm.m[3, 0]) && (m[3, 1] == tm.m[3, 1]) && (m[3, 2] == tm.m[3, 2]) && (m[3, 3] == tm.m[3, 3]));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(CMatrix4 m1, CMatrix4 m2)
        {
            return m1.Equals(m2);
        }

        public static bool operator !=(CMatrix4 m1, CMatrix4 m2)
        {
            return !m1.Equals(m2);
        }

        /// <summary>
        /// 単位行列に設定
        /// </summary>
        public void Identity()
        {
            m[0, 0] = 1.0f; m[0, 1] = 0.0f; m[0, 2] = 0.0f; m[0, 3] = 0.0f;
            m[1, 0] = 0.0f; m[1, 1] = 1.0f; m[1, 2] = 0.0f; m[1, 3] = 0.0f;
            m[2, 0] = 0.0f; m[2, 1] = 0.0f; m[2, 2] = 1.0f; m[2, 3] = 0.0f;
            m[3, 0] = 0.0f; m[3, 1] = 0.0f; m[3, 2] = 0.0f; m[3, 3] = 1.0f;
        }

        /// <summary>
        ///  平行移動行列に設定
        /// </summary>
        /// <param name="x">Xの移動値</param>
        /// <param name="y">Yの移動値</param>
        /// <param name="z">Zの移動値</param>
        public void Translate(float x, float y, float z)
        {
            this.Identity();
            m[3, 0] = x;
            m[3, 1] = y;
            m[3, 2] = z;
        }

        /// <summary>
        /// X軸回転行列に設定
        /// </summary>
        /// <param name="rad">回転角度（ラジアン）</param>
        public void RotationX(float rad)
        {
            this.Identity();
            m[1, 1] =  (float)Math.Cos(rad);
            m[1, 2] = -(float)Math.Sin(rad);
            m[2, 1] =  (float)Math.Sin(rad);
            m[2, 2] =  (float)Math.Cos(rad);
        }

        /// <summary>
        /// Y軸回転行列に設定
        /// </summary>
        /// <param name="rad">回転角度（ラジアン）</param>
        public void RotationY(float rad)
        {
            this.Identity();
            m[0, 0] =  (float)Math.Cos(rad);
            m[2, 0] =  (float)Math.Sin(rad);
            m[0, 2] = -(float)Math.Sin(rad);
            m[2, 2] =  (float)Math.Cos(rad);
        }

        /// <summary>
        /// Z軸回転行列に設定
        /// </summary>
        /// <param name="rad">回転角度（ラジアン）</param>
        public void RotationZ(float rad)
        {
            this.Identity();
            m[0, 0] =  (float)Math.Cos(rad);
            m[0, 1] = -(float)Math.Sin(rad);
            m[1, 0] =  (float)Math.Sin(rad);
            m[1, 1] =  (float)Math.Cos(rad);
        }

        /// <summary>
        /// 転置行列の取得
        /// </summary>
        /// <returns>転置行列</returns>
        public CMatrix4 Transpose()
        {
            CMatrix4 tm = new CMatrix4();

            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    tm.m[i, j] = m[j, i];
                }
            }
            return tm;
        }

        /// <summary>
        /// 逆行列の取得
        /// </summary>
        /// <param name="mat">出力先</param>
        /// <returns>逆行列がない場合は単位行列が代入され、falseが返る</returns>
        public bool Invers(CMatrix4 mat)
        {
	        int i, j, row;
            float tmp;
            float[,] mat84 = new float[4, 8];

	        // 8x4行列に4x4行列と単位行列入れる
	        for (i = 0; i < 4; i++) {
		        for (j = 0; j < 4; j++) {
			        mat84[i, j] = m[i, j];

			        if (i == j) {
				        mat84[i, (j + 4)] = 1.0f;
			        } else {
				        mat84[i, (j + 4)] = 0.0f;
			        }
		        }
	        }

	        for (row = 0; row < 4; row++) {
		        tmp = mat84[row, row];
		        if (tmp != 1.0f) {
			        if (tmp == 0.0f) {
				        for (i = row + 1; i < 4; i++) {
					        tmp = mat84[i, row];
					        if (tmp != 0.0f) break;
				        }

                        // 全て0なら逆行列なし
				        if (i >= 4) {
                            mat.Identity(); // 単位行列を入れておく（保険）
					        return false;
				        }

				        // 行を入れ替える
				        for (j = 0; j < 8; j++) {
					        tmp = mat84[i, j];
					        mat84[i, j] = mat84[row, j];
					        mat84[row, j] = tmp;
				        }
				        tmp = mat84[row, row];
			        }

			        for (i = 0; i < 8; i++) {
				        mat84[row, i] /= tmp;
			        }
		        }

		        // mat84[i][row]が1になるよう計算
		        for (i = 0; i < 4; i++) {
			        if (i != row) {
				        tmp = mat84[i, row];
				        if (tmp != 0) {
					        for (j = 0; j < 8; j++) {
						        mat84[i, j] -= mat84[row, j] * tmp;
					        }
				        }
			        }
		        }
	        }

	        // 求まった逆行列を4x4行列にコピー
	        for (i = 0; i < 4; i++) {
		        for (j = 0; j < 4; j++) {
			        mat.m[i, j] = mat84[i, (j + 4)];
		        }
	        }

            return true;
        }

        /// <summary>
        /// 逆行列の取得
        /// 備考：正則行列であることが前提
        /// </summary>
        /// <returns>逆行列</returns>
        public CMatrix4 InversTrans()
        {
            CMatrix4 tm = this.Transpose();
            tm.m[0, 3] = -(m[0, 3] * tm.m[0, 0] + m[1, 3] * tm.m[0, 1] + m[2, 3] * tm.m[0, 2]);
            tm.m[1, 3] = -(m[0, 3] * tm.m[1, 0] + m[1, 3] * tm.m[1, 1] + m[2, 3] * tm.m[1, 2]);
            tm.m[2, 3] = -(m[0, 3] * tm.m[2, 0] + m[1, 3] * tm.m[2, 1] + m[2, 3] * tm.m[2, 2]);
#if CLEAR_TRANS
	        tm.m[3, 0] = tm.m[3, 1] = tm.m[3, 2] = 0.0f;
#else
            tm.m[3, 0] = -tm.m[3, 0];
            tm.m[3, 1] = -tm.m[3, 1];
            tm.m[3, 2] = -tm.m[3, 2];
#endif
            tm.m[3, 3] = 1.0f;

            return tm;
        }
    }
}
