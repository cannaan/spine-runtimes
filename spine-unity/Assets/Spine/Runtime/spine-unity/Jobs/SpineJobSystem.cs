using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Jobs;

using UnityEngine;

namespace Spine.Unity
{
    public class SpineJobSystem : MonoBehaviour
    {
        private List<JobHandle> m_jobHandles = new List<JobHandle>();
        private NativeArray<JobHandle> m_jobHandleNativeArray;

        public void ScheduleJobs()
        {
            if(m_jobHandleNativeArray.IsCreated)
            {
                Debug.LogWarning("Spine jobs are running. can not reschedule jobs again.");
                return;
            }
            if(m_jobHandles.Count > 0)
            {
                m_jobHandleNativeArray = new NativeArray<JobHandle>(m_jobHandles.Count, Allocator.TempJob);
                for(int i = 0;i < m_jobHandles.Count;++i)
                {
                    m_jobHandleNativeArray[i] = m_jobHandles[i];
                }
            }
        }

        public void Wait()
        {
            if(m_jobHandleNativeArray.IsCreated)
            {
                JobHandle.CompleteAll(m_jobHandleNativeArray);
                m_jobHandleNativeArray.Dispose();
            }
        }

        private static SpineJobSystem s_inst;
        public static SpineJobSystem instance
        {
            get
            {
                if (s_inst == null)
                {
                    GameObject go = new GameObject("[SpineJobSystem]");
                    DontDestroyOnLoad(go);
                    s_inst = go.AddComponent<SpineJobSystem>();
                }
                return s_inst;
            }
        }
    }
}
