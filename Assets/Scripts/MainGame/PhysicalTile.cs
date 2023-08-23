using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalTile : Tile
{
   Ray ray;
   RaycastHit hit;
   void Update()
   {
      ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out hit))
      {
         if (Input.GetMouseButtonDown(0) && hit.collider.gameObject.name == gameObject.name)
         {
            if (!interactedWith && GameManager.Instance.IsGameActive && !GameManager.Instance.InReplay)
            {
               GameManager.Instance.TileClicked(gameObject.GetComponent<PhysicalTile>());
            }
         }
      }
   }

   public override void ActivateShipPlacedTile()
   {
      gameObject.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/ShipOnTileColor");
   }

   public override void ActivateSunkMark()
   {
      gameObject.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/SunkenShipColor");
   }

   public override void ResetTileAppearance()
   {
      gameObject.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/TileColor");
      ResetTile();
   }

   public void TileSelected()
   {
      gameObject.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/TileSelected");
   }
}
