using UnityEngine;
using UnityEngine.SceneManagement;


public class startManager : MonoBehaviour
{
    public void LoadSceneVR()
    {
        SceneManager.LoadScene("VR_level_1");
    }

    public void LoadScenePC()
    {
        SceneManager.LoadScene("PC_level");
    }
}
