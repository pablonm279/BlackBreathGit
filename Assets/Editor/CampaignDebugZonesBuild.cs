using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CampaignDebugZonesBuild : IPreprocessBuildWithReport
{
    private const string CampaignScenePath = "Assets/Scenes/ES-Campaña.unity";
    private const string SettingsPath = "Assets/Resources/CampaignDebugZones.json";

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        Scene scene = SceneManager.GetSceneByPath(CampaignScenePath);
        bool openedHere = !scene.IsValid() || !scene.isLoaded;
        if (openedHere)
        {
            scene = EditorSceneManager.OpenScene(CampaignScenePath, OpenSceneMode.Additive);
        }

        try
        {
            CampaignManager campaign = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                campaign = root.GetComponentInChildren<CampaignManager>(true);
                if (campaign != null) break;
            }

            if (campaign == null)
            {
                throw new BuildFailedException("No se encontró CampaignManager en la escena de campaña para copiar los ticks de debug.");
            }

            string json = JsonUtility.ToJson(new CampaignDebugZones
            {
                pasoVientoHelado = campaign.DebugPermiteZona(2),
                nedukazal = campaign.DebugPermiteZona(3)
            });

            if (!File.Exists(SettingsPath) || File.ReadAllText(SettingsPath).Trim() != json)
            {
                File.WriteAllText(SettingsPath, json + "\n");
                AssetDatabase.ImportAsset(SettingsPath, ImportAssetOptions.ForceUpdate);
            }
        }
        finally
        {
            if (openedHere && scene.IsValid())
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }

    [System.Serializable]
    private class CampaignDebugZones
    {
        public bool pasoVientoHelado;
        public bool nedukazal;
    }
}
