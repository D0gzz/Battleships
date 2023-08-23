using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tile : MonoBehaviour
{
   [SerializeField] public GameObject hitMark;
   [SerializeField] public GameObject missMark;
   protected bool interactedWith = false;

   public bool InteractedWith { get { return interactedWith; } set { interactedWith = value; } }
   private void Start()
   {
      InitMarks();
   }

   public void InitMarks()
   {
      missMark = gameObject.transform.GetChild(0).gameObject;
      hitMark = gameObject.transform.GetChild(1).gameObject;
   }

   public void ActivateHitMark()
   {
      hitMark.SetActive(true);
   }
   public void ActivateMissMark()
   {
      missMark.SetActive(true);
   }

   public void ResetTile()
   {
      hitMark.SetActive(false);
      missMark.SetActive(false);
   }

   public abstract void ResetTileAppearance();

   public abstract void ActivateShipPlacedTile();

   public abstract void ActivateSunkMark();
}
