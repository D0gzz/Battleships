using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Ship", menuName = "ScriptableObjects/Ships")]
public class Ship : ScriptableObject
{
   [SerializeField] private string shipName;
   [SerializeField] private int length;
   private bool[] hits;
   private (int, int) pos;
   private int orientation;
   public string ShipName { get { return shipName; } }
   public int Length { get { return length; } }

   public (int, int) Position { get { return pos; } set { pos = value; } }
   public int Orientation { get { return orientation; } set { orientation = value; } }

   public void InitializeShip()
   {
      hits = new bool[length];
   }

   public bool Sunk()
   {
      foreach (bool hit in hits)
      {
         if (!hit)
            return false;
      }
      return true;
   }

   public void SetHit(int pos)
   {
      if (pos < 0 || pos >= hits.Length)
         throw new ArgumentOutOfRangeException();
      hits[pos] = true;
   }

   public void UnsetHit(int pos)
   {
      if (pos < 0 || pos >= hits.Length)
         throw new ArgumentOutOfRangeException();
      hits[pos] = false;
   }
}
