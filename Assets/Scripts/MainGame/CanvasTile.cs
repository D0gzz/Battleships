using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasTile : Tile
{
   public override void ActivateShipPlacedTile()
   {
      gameObject.GetComponent<Image>().color = new Color(0.7062325f, 0f, 1f);
   }

   public override void ActivateSunkMark()
   {
      gameObject.GetComponent<Image>().color = Color.black;
   }

   public override void ResetTileAppearance()
   {
      gameObject.GetComponent<Image>().color = new Color(0.2688679f, 0.9350727f, 1f);
      ResetTile();
   }

}
