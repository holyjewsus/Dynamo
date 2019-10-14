using System;
using System.Collections;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using VMDataBridge;
using System.Linq;
using Newtonsoft.Json;

namespace Dynamo.Engine.Profiling
{
    class NodeProfilingData : IDisposable
    {
        NodeModel node = null;

        private DateTime? startTime = null;
        private DateTime? endTime = null;

        internal NodeProfilingData(NodeModel node)
        {
            this.node = node;
            DataBridge.Instance.RegisterCallback(node.GUID.ToString()+ ProfilingSession.profilingID+"BEGIN", RecordEvaluationBegin);
            DataBridge.Instance.RegisterCallback(node.GUID.ToString() + ProfilingSession.profilingID+"END", RecordEvaluationEnd);
        }

        internal void Reset()
        {
            startTime = null;
            endTime = null;
        }

        public void Dispose()
        {
            DataBridge.Instance.UnregisterCallback(node.GUID.ToString() + ProfilingSession.profilingID+"BEGIN");
            DataBridge.Instance.UnregisterCallback(node.GUID.ToString() + ProfilingSession.profilingID+"END");
        }

        private void RecordEvaluationBegin(object data)
        {
            Console.WriteLine($"{nameof(RecordEvaluationBegin)} {node.Name},{prettyPrint(data)}");
                startTime = DateTime.Now;
                node.OnNodeExecutionBegin();

        }

        private void RecordEvaluationEnd(object data)
        {
            Console.WriteLine($"{nameof(RecordEvaluationEnd)} {node.Name},{prettyPrint(data)}");
            endTime = DateTime.Now;
            node.OnNodeExecutionEnd();
        }

        private string prettyPrint(object data)
        {

            if(data is ICollection)
            {
                var items = string.Join(",",
               (data as ICollection).Cast<Object>().Select(x => x.ToString()));
                return $"[{items}]";
            }
            else
            {
                return data.ToString();
            }

        }

        internal TimeSpan? ExecutionTime
        {
            get
            {
                if (!HasPerformanceData())
                    return null;

                return endTime - startTime;
            }
        }

        private bool HasPerformanceData()
        {
            return startTime.HasValue && endTime.HasValue;
        }
    }
}
