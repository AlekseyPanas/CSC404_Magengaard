using UnityEngine;
using UnityEngine.SceneManagement;

/** 
* Used to switch scenes in the menu. Allows injecting waitables which determine when to switch scenes. All waitables
* must fire a "finish" event before this class will proceed with the configured scene change
*/
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
