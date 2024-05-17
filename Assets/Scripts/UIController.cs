using System;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Image HealthPrefab;

    public GameObject HealthPanel;
    public GameObject EndPanel;
    public TextMeshProUGUI EndText;
    private int _shownLives = 5;

    public int ShownLives
    {
        get => _shownLives;
        set
        {
            _shownLives = value;
            ChangeShownLiveCount(value);
        }
    }

    public static UIController Instance;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        ShownLives = Health.PlayerHealth.Lives;
        EndPanel.SetActive(false);
        for (int i = 0; i < HealthPanel.transform.childCount; i++)
        {
            Destroy(HealthPanel.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < ShownLives; i++)
        {
            AddLifeUI();
        }
    }

    void ChangeShownLiveCount(int target)
    {
        int currCount = HealthPanel.transform.childCount;
        for (int i = 0; i < Math.Abs(target - currCount); i++)
        {
            if (currCount > target)
            {
                Destroy(HealthPanel.transform.GetChild(i).gameObject);
            }
            else
            {
                AddLifeUI();
            }
        }
    }

    private void AddLifeUI()
    {
        Image image = Instantiate(HealthPrefab, HealthPanel.transform);
        image.enabled = true;
    }

    public void EndGame(bool win)
    {
        if (win)
            EndText.text = "You Win";
        else
            EndText.text = "You Lose";

        EndPanel.SetActive(true);
        GameController.DestroyScripts();
    }

    public void RestartGame()
    {
        GameController.Reset();
        SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}