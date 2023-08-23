using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using UnityEngine.UI;

public class SceneSelectionManager : MonoBehaviour
{
   public static SceneSelectionManager Instance;

   [SerializeField] TMP_InputField inputField;
   [SerializeField] GameObject buttonContainer;
   [SerializeField] GameObject saveButtonPrefab;

   private int automaticReplayDuration;

   private int inReplay;
   private void Awake()
   {
      // If there is an instance, and it's not me, delete myself.

      if (Instance != null && Instance != this)
      {
         Destroy(this);
      }
      else
      {
         Instance = this;
      }
   }

   private void Start()
   {
      automaticReplayDuration = -1;
   }
   public void PlayNewGame()
   {
      PlayerPrefs.SetInt("inReplay", inReplay);
      SceneManager.LoadScene(1);
   }

   public void ExitGame()
   {
      Application.Quit();
   }

   public void OpenMainScreen()
   {
      MenuManager.Instance.OpenMenu("Main");
   }

   public void OpenReplaySelection()
   {
      MenuManager.Instance.OpenMenu("ReplaySelection");
   }

   public void OpenAutomaticReplayScreen()
   {
      MenuManager.Instance.OpenMenu("AutomaticReplayMenu");
   }

   public void OpenSelectiveReplay()
   {
      PlayerPrefs.SetInt("replayDuration", -1);
      OpenSaveList();
   }

   public void OpenSaveList()
   {
      MenuManager.Instance.OpenMenu("SaveList");
      string path = Application.persistentDataPath + "/Replays";
      DirectoryInfo d = new DirectoryInfo(path);
      foreach (FileInfo file in d.GetFiles("*.json"))
      {
         GameObject button = Instantiate(saveButtonPrefab, buttonContainer.transform);
         button.GetComponentInChildren<TextMeshProUGUI>().text = file.Name.Split('.')[0];
         button.GetComponent<Button>().onClick.AddListener(() =>
         {
            PlayerPrefs.SetString("replay", file.Name);
            inReplay = 1;
            PlayerPrefs.SetInt("inReplay", inReplay);
            SceneManager.LoadScene(1);
         });
      }
   }

   public void StartAutomaticReplay()
   {
      string input = inputField.text;
      if (int.TryParse(input, out automaticReplayDuration))
      {
         PlayerPrefs.SetInt("replayDuration", automaticReplayDuration);
         OpenSaveList();
      }
      else
      {
         NotificationManager.Instance.PlayNotification("Input must be a number in seconds.");
      }

   }
}
