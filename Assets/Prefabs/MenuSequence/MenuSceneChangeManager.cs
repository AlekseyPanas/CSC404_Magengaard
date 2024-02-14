using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneChangeManager : MonoBehaviour
{
    [SerializeField] private AWaitFor[] waitables;
    [SerializeField] private string nextScenePath;
    private int finishedCount = 0;

    void Awake() { 
        foreach (AWaitFor w in waitables) { 
            w.FinishedTaskEvent += async () => { 
                finishedCount++; 
                if (finishedCount >= waitables.Length) {
                    await SceneManager.LoadSceneAsync(nextScenePath);
                }
            }; 
        }
    }
}
