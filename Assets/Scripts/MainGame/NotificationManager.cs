using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class NotificationManager : MonoBehaviour
{
   private TextMeshProUGUI textBox;
   [SerializeField] private int fadeLength;

   public static NotificationManager Instance { get; private set; }
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

      DontDestroyOnLoad(this.gameObject);
   }

   private void Start()
   {
      textBox = GameObject.Find("NotificationField").GetComponent<TextMeshProUGUI>();
   }

   private void OnEnable()
   {
      SceneManager.sceneLoaded += OnSceneLoaded;
   }

   private void OnDisable() {
      SceneManager.sceneLoaded -= OnSceneLoaded;
   }

   private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
   {
      if (scene.isLoaded)
      {
         textBox = GameObject.Find("NotificationField").GetComponent<TextMeshProUGUI>();
      }
   }

   public void PlayNotification(string notification)
   {
      StopAllCoroutines();
      StartCoroutine(FadeNotification(notification));
   }

   private IEnumerator FadeNotification(string notification)
   {
      textBox.text = notification;
      Color c = textBox.color;
      for (float alpha = fadeLength; alpha >= 0; alpha -= 0.01f)
      {
         c.a = alpha / fadeLength;
         textBox.color = c;
         yield return null;
      }
   }
}
