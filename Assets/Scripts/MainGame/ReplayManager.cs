using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplayManager : MonoBehaviour
{
   private List<MoveData> moves = new List<MoveData>();
   public static ReplayManager Instance { get; private set; }

   [SerializeField] private Button nextButton;
   [SerializeField] private Button prevButton;

   private int activeMove;
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

   public void SetMoveData(List<MoveData> movedataField)
   {
      moves = movedataField;
   }

   public void PlayAutomatic(List<MoveData> fieldsFired, int replayDuration)
   {
      StartCoroutine(AutomaticReplay(fieldsFired, replayDuration));
   }

   IEnumerator AutomaticReplay(List<MoveData> fieldsFired, int replayDuration)
   {
      foreach (MoveData item in fieldsFired)
      {
         int row = item.row;
         int col = item.col;
         GameManager.Instance.SetCurrentTileSelected(row, col);
         yield return new WaitForSeconds(replayDuration);
         GameManager.Instance.Fire();
      }
   }

   public void PlayNextTurn()
   {
      int row = moves[activeMove].row;
      int col = moves[activeMove].col;
      GameManager.Instance.SetCurrentTileSelected(row, col);
      GameManager.Instance.Fire();
      activeMove++;
      StartCoroutine(DisableEnableButtons());

   }


   public void DisEnButt()
   {
      StartCoroutine(DisableEnableButtons());
   }

   IEnumerator DisableEnableButtons()
   {
      nextButton.enabled = false;
      prevButton.enabled = false;
      yield return new WaitForSeconds(2.2f);
      nextButton.enabled = true;
      prevButton.enabled = true;
   }

   public void UndoTurn()
   {
      if (GameManager.Instance.undoStack.Count > 0)
         activeMove--;
      GameManager.Instance.UndoFire();
      StartCoroutine(DisableEnableButtons());
   }
}
