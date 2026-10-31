using UnityEditor;
using UnityEngine;
using MCPForUnity.Editor.Services;

namespace GuJian.Editor
{
    [InitializeOnLoad]
    public static class ConnectLocalBridge
    {
        private static bool connecting;
        private static double nextAttempt;

        static ConnectLocalBridge()
        {
            Application.runInBackground = true;
            if (!Application.isBatchMode)
                EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            if (connecting || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.timeSinceStartup < nextAttempt) return;
            nextAttempt = EditorApplication.timeSinceStartup + 5;
            if (!MCPServiceLocator.Bridge.IsRunning) Connect();
        }

        private static async void Connect()
        {
            connecting = true;
            try
            {
                if (!MCPServiceLocator.Server.IsLocalHttpServerReachable()) return;
                bool connected = await MCPServiceLocator.Bridge.StartAsync();
                Debug.Log("GUJIAN_MCP_CONNECTED=" + connected);
            }
            finally { connecting = false; }
        }
    }
}
