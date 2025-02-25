using UnityEngine;
using UnityEngine.SceneManagement;

public class portal : MonoBehaviour
{
    //Set this field in the Inspector for each portal you want to assign
    public int sceneBuildIndex;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            //Loads the scene specified in the inspector
            SceneManager.LoadScene(sceneBuildIndex);
        }
    }
}
