using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
   public static MenuManager Instance;

   [SerializeField] Menu[] menus;


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

   public void OpenMenu(string menuName)
   {
      for (int i = 0; i < menus.Length; i++)
      {
         if (menus[i].menuName == menuName)
            menus[i].Open();
         else if (menus[i].open)
            CloseMenu(menus[i]);
      }
   }
   public void CloseMenu(Menu menu)
   {
      menu.Close();
   }
}
