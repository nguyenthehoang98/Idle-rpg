using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace _TDS.Tests.PlayMode
{
    [SetUpFixture]
    public sealed class RuntimeLogGuard
    {
        private readonly List<string> failures = new List<string>();

        [OneTimeSetUp]
        public void Start()
        {
            Application.logMessageReceived += OnLog;
        }

        [OneTimeTearDown]
        public void Stop()
        {
            Application.logMessageReceived -= OnLog;
            if (failures.Count > 0)
            {
                Assert.Fail(string.Join("\n", failures));
            }
        }

        private void OnLog(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception ||
                (type == LogType.Warning && message.Contains("referenced script (Unknown)")))
            {
                failures.Add($"[{type}] {message}");
            }
        }
    }
}
