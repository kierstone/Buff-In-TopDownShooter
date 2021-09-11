using System;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Microsoft.Unity.VisualStudio.Editor.Testing
{
	[InitializeOnLoad]
	internal class TestRunnerApiListener
	{
		private static TestRunnerApi _testRunnerApi;
		private static TestRunnerCallbacks _testRunnerCallbacks;

		static TestRunnerApiListener()
		{
			_testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
			_testRunnerCallbacks = new TestRunnerCallbacks();

			_testRunnerApi.RegisterCallbacks(_testRunnerCallbacks);
		}

		public static void RetrieveTestList(string mode)
		{
			RetrieveTestList((TestMode) Enum.Parse(typeof(TestMode), mode));
		}

		private static void RetrieveTestList(TestMode mode)
		{
			_testRunnerApi.RetrieveTestList(mode, (ta) => _testRunnerCallbacks.TestListRetrieved(mode, ta));
		}

		public static void ExecuteTests(string command)
		{
			// ExecuteTests format:
			// TestMode:FullName

			var index = command.IndexOf(':');
			if (index < 0)
				return;

			var testMode = (TestMode)Enum.Parse(typeof(TestMode), command.Substring(0, index));
			var filter = command.Substring(index + 1);

			ExecuteTests(new Filter() { testMode = testMode, testNames = new string[] { filter } });
		}

		private static void ExecuteTests(Filter filter)
		{
			_testRunnerApi.Execute(new ExecutionSettings(filter));
		}
	}
}
