using System.Collections;
using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace Spine.Unity
{
    namespace Jobs
    {
        [BurstCompile]
        public struct NativeCurve
        {
            [ReadOnly]
            public NativeArray<float> m_curves;
            [ReadOnly]
            public NativeArray<float> m_frames;

            public int m_frameStep;
            public int m_stride;

            public int Search(float time)
            {
                int n = m_frames.Length;
                for (int i = m_frameStep; i < n; i += m_frameStep)
                    if (m_frames[i] > time) return i / m_frameStep - 1;
                return n / m_frameStep - 1;
            }

            public float GetLinearValue(float time, int frameIndex)
            {
                float startTime = m_frames[frameIndex], startVal = m_frames[frameIndex + 1];
                return startVal + (time - startTime) / (m_frames[frameIndex + m_stride] - startTime) * (m_frames[frameIndex + m_stride + 1] - startVal);
            }
            public float GetStepValue(int frameIndex)
            {
                return m_frames[frameIndex + 1];
            }

            public float GetBezierValue(float time, int frameIndex, int valueOffset, int segment)
            {
                const int BEZIER_SIZE = 18;
                int n = segment + BEZIER_SIZE;
                float prevX = m_frames[frameIndex], prevY = m_frames[frameIndex + valueOffset];
                for(int i = segment; i < n;i += 2)
                {
                    if(m_curves[i] >= time)
                    {
                        return prevY + (time - prevX) / (m_curves[i] - prevX) * (m_curves[i + 1] - prevY);
                    }
                    prevX = m_curves[i];
                    prevY = m_curves[i + 1];
                }
                return prevY + (time - prevX) / (m_frames[frameIndex + m_frameStep] - prevX) * (m_frames[frameIndex + m_frameStep + valueOffset] - prevY);
            }
            public float2 GetLinearValue2(float time, int frameIndex)
            {
                float startTime = m_frames[frameIndex], endTime = m_frames[frameIndex + m_stride];
                float2 startVal = default, endVal = default;
                startVal.x = m_frames[frameIndex + 1];
                startVal.y = m_frames[frameIndex + 2];
                endVal.x = m_frames[frameIndex + m_stride + 1];
                endVal.y = m_frames[frameIndex + m_stride + 2];
                return startVal + (endVal - startVal) * ((time - startTime) / (endTime - startTime));
            }
            public float2 GetStepValue2(int frameIndex)
            {
                float2 ret = default;
                ret.x = m_frames[frameIndex + 1];
                ret.y = m_frames[frameIndex + 2];
                return ret;
            }
            public float3 GetLinearValue3(float time, int frameIndex)
            {
                float startTime = m_frames[frameIndex], endTime = m_frames[frameIndex + m_stride];
                float3 startVal = default, endVal = default;
                startVal.x = m_frames[frameIndex + 1];
                startVal.y = m_frames[frameIndex + 2];
                startVal.z = m_frames[frameIndex + 3];
                endVal.x = m_frames[frameIndex + m_stride + 1];
                endVal.y = m_frames[frameIndex + m_stride + 2];
                endVal.z = m_frames[frameIndex + m_stride + 3];
                return startVal + (endVal - startVal) * ((time - startTime) / (endTime - startTime));
            }
            public float3 GetStepValue3(int frameIndex)
            {
                float3 ret = default;
                ret.x = m_frames[frameIndex + 1];
                ret.y = m_frames[frameIndex + 2];
                ret.z = m_frames[frameIndex + 3];
                return ret;
            }
            public float4 GetLinearValue4(float time, int frameIndex)
            {
                float startTime = m_frames[frameIndex], endTime = m_frames[frameIndex + m_stride];
                float4 startVal = default, endVal = default;
                startVal.x = m_frames[frameIndex + 1];
                startVal.y = m_frames[frameIndex + 2];
                startVal.z = m_frames[frameIndex + 3];
                startVal.w = m_frames[frameIndex + 4];
                endVal.x = m_frames[frameIndex + m_stride + 1];
                endVal.y = m_frames[frameIndex + m_stride + 2];
                endVal.z = m_frames[frameIndex + m_stride + 3];
                endVal.w = m_frames[frameIndex + m_stride + 4];
                return startVal + (endVal - startVal) * ((time - startTime) / (endTime - startTime));
            }
            public float4 GetStepValue4(int frameIndex)
            {
                float4 ret = default;
                ret.x = m_frames[frameIndex + 1];
                ret.y = m_frames[frameIndex + 2];
                ret.z = m_frames[frameIndex + 3];
                ret.w = m_frames[frameIndex + 4];
                return ret;
            }
        }

        [BurstCompile]
        public struct Float1CurveJob : IJob
        {
            public NativeCurve m_curve;
            public float time;
            public float output;

            public void Execute()
            {
                int frameIndex = m_curve.Search(time);
                int curveType = (int)m_curve.m_curves[frameIndex];
                const int LINEAR = 0, STEPPED = 1, BEZIER = 2;
                if(curveType == LINEAR)
                {
                    output = m_curve.GetLinearValue(time, frameIndex);
                }
                else if(curveType == STEPPED)
                {
                    output = m_curve.GetStepValue(frameIndex);
                }
                else
                {
                    int segment = curveType - BEZIER;
                    output = m_curve.GetBezierValue(time, frameIndex, 1, segment);
                }
            }
        }

        [BurstCompile]
        public struct Float2CurveJob : IJob
        {
            public NativeCurve m_curve;
            public float time;
            public float2 output;

            public void Execute()
            {
                int frameIndex = m_curve.Search(time);
                int curveType = (int)m_curve.m_curves[frameIndex];
                const int LINEAR = 0, STEPPED = 1, BEZIER = 2;
                if (curveType == LINEAR)
                {
                    output = m_curve.GetLinearValue2(time, frameIndex);
                }
                else if (curveType == STEPPED)
                {
                    output = m_curve.GetStepValue2(frameIndex);
                }
                else
                {
                    int segment = curveType - BEZIER;
                    output.x = m_curve.GetBezierValue(time, frameIndex, 1, segment);
                    output.y = m_curve.GetBezierValue(time, frameIndex, 2, segment);
                }
            }
        }

        [BurstCompile]
        public struct Float3CurveJob : IJob
        {
            public NativeCurve m_curve;
            public float time;
            public float3 output;

            public void Execute()
            {
                int frameIndex = m_curve.Search(time);
                int curveType = (int)m_curve.m_curves[frameIndex];
                const int LINEAR = 0, STEPPED = 1, BEZIER = 2;
                if (curveType == LINEAR)
                {
                    output = m_curve.GetLinearValue3(time, frameIndex);
                }
                else if (curveType == STEPPED)
                {
                    output = m_curve.GetStepValue3(frameIndex);
                }
                else
                {
                    int segment = curveType - BEZIER;
                    output.x = m_curve.GetBezierValue(time, frameIndex, 1, segment);
                    output.y = m_curve.GetBezierValue(time, frameIndex, 2, segment);
                    output.z = m_curve.GetBezierValue(time, frameIndex, 3, segment);
                }
            }
        }

        [BurstCompile]
        public struct Float4CurveJob : IJob
        {
            public NativeCurve m_curve;
            public float time;
            public float4 output;

            public void Execute()
            {
                int frameIndex = m_curve.Search(time);
                int curveType = (int)m_curve.m_curves[frameIndex];
                const int LINEAR = 0, STEPPED = 1, BEZIER = 2;
                if (curveType == LINEAR)
                {
                    output = m_curve.GetLinearValue4(time, frameIndex);
                }
                else if (curveType == STEPPED)
                {
                    output = m_curve.GetStepValue4(frameIndex);
                }
                else
                {
                    int segment = curveType - BEZIER;
                    output.x = m_curve.GetBezierValue(time, frameIndex, 1, segment);
                    output.y = m_curve.GetBezierValue(time, frameIndex, 2, segment);
                    output.z = m_curve.GetBezierValue(time, frameIndex, 3, segment);
                    output.w = m_curve.GetBezierValue(time, frameIndex, 4, segment);
                }
            }
        }
    }
}
