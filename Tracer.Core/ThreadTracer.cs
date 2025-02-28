﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Tracer.Format;

namespace Tracer
{
    namespace Core
    {
        public sealed class ThreadTracer
        {
            private readonly Stack<MethodTracer> _callStack;
            private readonly List<MethodTracer> _methodsTracers;

            internal ThreadTracer(int threadId)
            {
                ThreadId = threadId;
                _callStack = new Stack<MethodTracer>();
                _methodsTracers = new List<MethodTracer>();
            }

            [JsonPropertyName(SerializationConfig.JSON_THREAD_ID)]
            public int ThreadId { get; }

            [JsonPropertyName(SerializationConfig.JSON_THREAD_TIME)]
            [JsonConverter(typeof(JsonTimeMsConverter))]
            public long ExecutionTime => _methodsTracers.Select(methodTracer => methodTracer.ExecutionTime).Sum();

            [JsonPropertyName(SerializationConfig.JSON_THREAD_METHODS)]
            public IEnumerable<MethodTracer> MethodsTracers => _methodsTracers;

            internal void StartTrace(MethodBase method)
            {
                var methodTraceResult = new MethodTracer(method);
                if (_callStack.Count == 0)
                    _methodsTracers.Add(methodTraceResult);
                else
                    _callStack.Peek().AddNestedMethod(methodTraceResult);

                _callStack.Push(methodTraceResult);
                methodTraceResult.StartTrace();
            }

            internal void StopTrace()
            {
                if (_callStack.Count == 0)
                    throw new InvalidOperationException("Call stack has no methods to stop tracing.");

                _callStack.Peek().StopTrace();
                _callStack.Pop();
            }
        }
    }
}
