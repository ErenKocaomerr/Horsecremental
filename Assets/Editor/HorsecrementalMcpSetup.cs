using System;
using System.Threading.Tasks;
using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

namespace Horsecremental.Editor
{
    internal static class HorsecrementalMcpSetup
    {
        private const string SETUP_KEY = "Horsecremental.McpSetup.V10";
        private const string AUTO_START_KEY = "MCPForUnity.AutoStartOnLoad";
        private const string SERVER_URL = "http://127.0.0.1:8080";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            if (Application.isBatchMode || EditorPrefs.GetBool(SETUP_KEY, false))
            {
                return;
            }

            EditorApplication.delayCall += Configure;
        }

        [MenuItem("Tools/Horsecremental/Configure Unity MCP")]
        private static void Configure()
        {
            EditorConfigurationCache configuration = EditorConfigurationCache.Instance;
            configuration.SetUseHttpTransport(true);
            configuration.SetHttpTransportScope("local");
            configuration.SetHttpBaseUrl(SERVER_URL);
            EditorPrefs.SetBool(AUTO_START_KEY, true);
            EditorPrefs.SetBool(SETUP_KEY, true);
            _ = ConnectAsync();
        }

        private static async Task ConnectAsync()
        {
            try
            {
                if (MCPServiceLocator.Bridge.IsRunning || await MCPServiceLocator.Bridge.StartAsync())
                {
                    Debug.Log("Horsecremental Unity MCP connected at " + SERVER_URL + "/mcp.");
                    return;
                }

                Debug.LogWarning("Unity MCP configured. Open Window > MCP for Unity and select Start Server, then Connect.");
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Unity MCP setup could not connect: " + exception.Message);
            }
        }
    }
}
