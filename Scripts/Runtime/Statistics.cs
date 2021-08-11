using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity_ImageStatistics {

    public static class Statistics {
        public const string PATH = "Statistics";

        public static readonly int P_Result = Shader.PropertyToID("Result");

        public static readonly int P_MainTex = Shader.PropertyToID("MainTex");
        public static readonly int P_MainTex_Len = Shader.PropertyToID("MainTex_Len");

        public static readonly int P_MainBuf = Shader.PropertyToID("MainBuf");
        public static readonly int P_MainBuf_Len = Shader.PropertyToID("MainBuf_Len");

        public static readonly int Sz_Vector4 = Marshal.SizeOf<Vector4>();

        public static readonly int K_ReduceTextureH;
        public static readonly int K_ReduceBuffer;

        static ComputeShader cs;

        static Statistics() {
            cs = Resources.Load<ComputeShader>(PATH);
            K_ReduceTextureH = cs.FindKernel("ReduceTextureH");
            K_ReduceBuffer = cs.FindKernel("ReduceBuffer");
        }

        #region interface
        public static StatResult Analyze(this Texture src) {
            using (var vbuf = new ComputeBuffer(src.height * 2, Sz_Vector4)) {

                src.ReduceTextureHorizontal(vbuf);

                var vdata = new Vector4[vbuf.count];
                vbuf.GetData(vdata);
                Debug.Log($"{vdata.DebugString()}");

                using (var hbuf = new ComputeBuffer(2, Sz_Vector4)) {

                    vbuf.ReduceBuffer(hbuf);

                    var n = src.height * src.width;
                    var stat = hbuf.ToStatResult(n);
                    return stat;
                }
            }
        }

        public static ComputeBuffer ReduceBuffer(this ComputeBuffer vbuf, ComputeBuffer hbuf) {
            cs.SetBuffer(K_ReduceBuffer, P_Result, hbuf);

            cs.SetBuffer(K_ReduceBuffer, P_MainBuf, vbuf);
            cs.SetInt(P_MainBuf_Len, vbuf.count);

            cs.Dispatch(K_ReduceBuffer, 1, 1, 1);

            return hbuf;
        }

        public static ComputeBuffer ReduceTextureHorizontal(this Texture src, ComputeBuffer vbuf) {
            cs.SetBuffer(K_ReduceTextureH, P_Result, vbuf);

            cs.SetTexture(K_ReduceTextureH, P_MainTex, src);
            cs.SetInts(P_MainTex_Len, src.width, src.height);

            cs.GetKernelThreadGroupSizes(K_ReduceTextureH, out uint x, out uint y, out uint z);
            var groups = (int)((src.height + x - 1) / x);
            cs.Dispatch(K_ReduceTextureH, groups, 1, 1);

            return vbuf;
        }

        public static StatResult ToStatResult(this ComputeBuffer hbuf, int n) {
            var hdata = new Vector4[hbuf.count];
            hbuf.GetData(hdata);
            var stat = new StatResult(n, hdata[0], hdata[1]);
            return stat;
        }
        #endregion



        #region definitions
        public struct StatResult {
            public readonly int n;
            public readonly Vector4 m;
            public readonly Vector4 s;

            public StatResult(int n, Vector4 m, Vector4 s) {
                this.n = n;
                this.m = m;
                this.s = s;
            }

            #region interface
            public Vector4 Mean {
                get => m;
            }
            public Vector4 UnbiasedVariance {
                get => s / (n - 1);
            }
            public Vector4 PopulationVariance {
                get => s / n;
            }

            public override string ToString() {
                return $"<{GetType().Name} : mean={Mean}, p-variance={PopulationVariance}>\nm={m}, s={s}";
            }
            #endregion
        }
        #endregion
    }
}