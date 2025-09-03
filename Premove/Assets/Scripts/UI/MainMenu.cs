using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject[] menues;

    private GameObject activeMenu;

    private void Awake()
    {
        if (menues.Length > 0)
        {
            activeMenu = menues[0];

            foreach(var menu in menues)
            {
                menu.SetActive(false);
            }

            activeMenu.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
    }

    public void OpenMenu(GameObject menu)
    {
        activeMenu.SetActive(false);
        activeMenu = menu;
        activeMenu.SetActive(true);
    }
}
