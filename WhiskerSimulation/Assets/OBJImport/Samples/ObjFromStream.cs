using Dummiesman;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections; // Add this using directive

public class ObjFromStream : MonoBehaviour {
    void Start () {
        StartCoroutine(LoadObjFromURL("https://people.sc.fsu.edu/~jburkardt/data/obj/lamp.obj"));
    }

    private IEnumerator LoadObjFromURL(string url) {
        using (UnityWebRequest www = UnityWebRequest.Get(url)) {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.LogError("Failed to download file: " + www.error);
            } else {
                //create stream and load
                var textStream = new MemoryStream(Encoding.UTF8.GetBytes(www.downloadHandler.text));
                var loadedObj = new OBJLoader().Load(textStream);
            }
        }
    }
}
