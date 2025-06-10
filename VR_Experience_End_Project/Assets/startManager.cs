using UnityEngine;
using UnityEngine.SceneManagement;


public class startManager : MonoBehaviour
{
    public void LoadSceneVR()
    {
        SceneManager.LoadScene("VR_level");
    }

    public void LoadScenePC()
    {
        SceneManager.LoadScene("PC_level");
    }
}
