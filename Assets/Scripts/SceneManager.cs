using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using SmartDLL;

public class SceneManager : EditorWindow
{
    static public List<GameObject> objects = new List<GameObject>();
    static public string SaveFile = "m_Scene";
    static public SmartFileExplorer fileExplorer = new SmartFileExplorer();

    static string savePath;
    static string loadFile;
    [MenuItem("Window/Scene Loader")]
    static void OpenWindow()
    {
        SceneManager window = (SceneManager)GetWindow(typeof(SceneManager));
        window.minSize = new Vector2(600, 300);
        window.Show();
    }
    private void OnGUI()
    {
        if (GUILayout.Button("Save Scene"))
        {
            Save();
            Debug.Log("Scene Saved");
        }
        if (GUILayout.Button("Load Scene"))
        {
            Load();
            Debug.Log("Scene Loaded");
        }
    }

    public static void Save()
    {
        //save to a certain path
        savePath = Application.dataPath + "/Saved_Scenes/" + SaveFile + ".txt";

        File.WriteAllText(savePath, "");

        objects.Clear();
        //save objects properties in current scene as JASON format
        FileStream fileStream = new FileStream(savePath, FileMode.Create);
        using (StreamWriter writer = new StreamWriter(fileStream))
        {
            foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (obj.GetComponent<MeshFilter>() != null)
                {
                    objects.Add(obj);
                    ObjectsData tempData = new ObjectsData();
                    tempData.type = obj.GetComponent<MeshFilter>().sharedMesh.name;
                    tempData.name = obj.name;
                    tempData.position = obj.transform.position;
                    tempData.scale = obj.transform.localScale;
                    tempData.rotation = obj.transform.rotation.eulerAngles;
                    string data = JsonUtility.ToJson(tempData);
                    writer.Write(data);
                    writer.Write("\n");
                }
            }
        }
    }


    public static void Load()
    {
        //explorer read the file
        string initialDir = Application.dataPath + "/Saved_Scenes/";
        bool restoreDir = true;
        string title = "Select File";
        string defExt = "txt";
        string filter = "txt files (*.txt)|*.txt";
        fileExplorer.OpenExplorer(initialDir, restoreDir, title, defExt, filter);

        if (fileExplorer.resultOK)
        {
            foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                //clean up the current scene
                if (obj.GetComponent<MeshFilter>() != null)
                {
                    DestroyImmediate(obj);

                }
            }
            objects.Clear();

            //go through all file content
            string m_Path = fileExplorer.fileName;
            if (File.Exists(m_Path))
            {
                using (StreamReader reader = new StreamReader(m_Path))
                {
                    loadFile = reader.ReadToEnd();
                }
            }

            //load each objects' content
            string[] newLine = loadFile.Split('\n');
            foreach (string line in newLine)
            {
                if (line.Length != 0)
                {
                    ObjectsData objData = JsonUtility.FromJson<ObjectsData>(line);
                    Debug.Log(line);
                    GameObject gameObject = null;
                    //load objects' type
                    switch (objData.type)
                    {
                        case "Cube":
                            gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            break;

                        case "Cylinder":
                            gameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            break;
                        case "Capsule":
                            gameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                            break;
                        case "Sphere":
                            gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            break;
                        default:
                            break;
                    }
                    //load objects' properties
                    if (gameObject != null)
                    {
                        gameObject.name = objData.name;
                        gameObject.transform.position = objData.position;
                        gameObject.transform.rotation = Quaternion.Euler(objData.rotation);
                        gameObject.transform.localScale = objData.scale;
                    }

                }
            }
        }
    }

}
