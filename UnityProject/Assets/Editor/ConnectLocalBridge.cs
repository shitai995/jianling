using UnityEditor;
using UnityEngine;
using MCPForUnity.Editor.Services;

namespace GuJian.Editor
{
    [InitializeOnLoad]
    public static class ConnectLocalBridge
    {
        static ConnectLocalBridge()
        {
            Application.runInBackground = true;
            if (!Application.isBatchMode)
                EditorApplication.delayCall += Connect;
        }

        private static async void Connect()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += Connect;
                return;
            }
            bool reachable = MCPServiceLocator.Server.IsLocalHttpServerReachable();
            Debug.Log("GUJIAN_MCP_CONNECT reachable=" + reachable);
            if (reachable)
            {
                bool connected = await MCPServiceLocator.Bridge.StartAsync();
                Debug.Log("GUJIAN_MCP_CONNECTED=" + connected);
            }
        }
    }
}
