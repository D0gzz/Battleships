using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, ShipSunkListener
{
   private Tile[,] physicalTiles;
   private Tile[,] uiTiles;

   [SerializeField] private Board[] virtualBoards;

   private int currentActivePlayer = 0;
   [SerializeField] private TextMeshProUGUI activePlayer;
   [SerializeField] private TextMeshProUGUI enemyShipsAlive;
   [SerializeField] private TextMeshProUGUI enemyShipsSunk;
   [SerializeField] private GameObject endGameScreen;
   [SerializeField] private Button fireButton;
   [SerializeField] private GameObject nextMoveButton;
   [SerializeField] private GameObject previousMoveButton;
   private int currentActiveOpponent = 1;

   private int rows;
   private int cols;

   private (int, int) currentSelectedTilePos = (-1, -1);

   public static GameManager Instance { get; private set; }

   public bool IsGameActive { get; set; }

   public bool InReplay { get; set; }

   private List<MoveData> fieldsFired = new List<MoveData>();

   private List<ShipData> p1ShipsLoaded = new List<ShipData>();
   private List<ShipData> p2ShipsLoaded = new List<ShipData>();

   public Stack<UndoAction> undoStack = new Stack<UndoAction>();

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

      InReplay = PlayerPrefs.GetInt("inReplay", 0) == 0 ? false : true;
   }

   public (int, int) GetCurrentTileSelected()
   {
      return currentSelectedTilePos;
   }

   public void SetCurrentTileSelected(int x, int y)
   {
      currentSelectedTilePos = (x, y);
   }

   private void Start()
   {
      endGameScreen.SetActive(false);
      rows = virtualBoards[0].Rows;
      cols = virtualBoards[0].Cols;
      InitializeSceneBoard();
      InitializeSceneUIBoard();
      EnableBoardInteraction();
      HandleReplayCondition();
      UpdateVisuals();
   }

   private void HandleReplayCondition()
   {
      if (InReplay)
      {
         fireButton.gameObject.SetActive(false);
         LoadRound();
         int replayDuration = PlayerPrefs.GetInt("replayDuration", -1);
         if (replayDuration != -1)
         {
            nextMoveButton.SetActive(false);
            previousMoveButton.SetActive(false);
            ReplayManager.Instance.PlayAutomatic(fieldsFired, replayDuration);
         }
         else
         {
            ReplayManager.Instance.SetMoveData(fieldsFired);
            ReplayManager.Instance.DisEnButt();
         }
      }
      else
      {
         nextMoveButton.SetActive(false);
         previousMoveButton.SetActive(false);
      }
   }



   public List<ShipData> fetchShipData(string name)
   {
      return name == "Board1" ? p1ShipsLoaded : p2ShipsLoaded;
   }

   public void UpdateVisuals()
   {
      activePlayer.text = currentActivePlayer == 0 ? "Player 1" : "Player 2";
      UpdateBoardVisual(virtualBoards[currentActiveOpponent]);
      UpdateUIBoardVisual(virtualBoards[currentActivePlayer]);
   }
   private void InitializeSceneBoard()
   {
      physicalTiles = new Tile[rows, cols];
      GameObject[] objs = GameObject.FindGameObjectsWithTag("Tile");
      for (int i = 0; i < rows; i++)
      {
         for (int j = 0; j < cols; j++)
         {
            physicalTiles[i, j] = objs[i * rows + j].GetComponent<Tile>();
            physicalTiles[i, j].InitMarks();
         }
      }
   }

   private void InitializeSceneUIBoard()
   {
      uiTiles = new Tile[rows, cols];
      GameObject[] objs = GameObject.FindGameObjectsWithTag("CanvasTile");
      for (int i = 0; i < rows; i++)
      {
         for (int j = 0; j < cols; j++)
         {
            uiTiles[i, j] = objs[i * rows + j].GetComponent<Tile>();
            uiTiles[i, j].InitMarks();
         }
      }
   }
   private void UpdateBoardVisual(Board board)
   {
      for (int i = 0; i < rows; i++)
      {
         for (int j = 0; j < cols; j++)
         {
            physicalTiles[i, j].ResetTileAppearance();
            if (board.getBoardTileStatus(i, j) == 'H')
            {
               physicalTiles[i, j].ActivateHitMark();
            }
            else if (board.getBoardTileStatus(i, j) == 'M')
            {
               physicalTiles[i, j].ActivateMissMark();
            }
            else if (board.getBoardTileStatus(i, j) == 'S')
            {
               physicalTiles[i, j].ActivateSunkMark();
            }
         }
      }
   }

   private void UpdateUIBoardVisual(Board board)
   {
      for (int i = 0; i < rows; i++)
      {
         for (int j = 0; j < cols; j++)
         {
            uiTiles[i, j].ResetTileAppearance();
            if (board.getBoardTileStatus(i, j) == 'X')
            {
               uiTiles[i, j].ActivateShipPlacedTile();
            }
            else if (board.getBoardTileStatus(i, j) == 'H')
            {
               uiTiles[i, j].ActivateHitMark();
            }
            else if (board.getBoardTileStatus(i, j) == 'M')
            {
               uiTiles[i, j].ActivateMissMark();
            }
            else if (board.getBoardTileStatus(i, j) == 'S')
            {
               uiTiles[i, j].ActivateSunkMark();
            }
         }
      }
   }

   private void FlipPlayers()
   {
      currentActivePlayer = currentActivePlayer == 0 ? 1 : 0;
      currentActiveOpponent = currentActiveOpponent == 1 ? 0 : 1;
   }

   public void TileClicked(PhysicalTile tile)
   {
      if (currentSelectedTilePos.Item1 != -1)
         physicalTiles[currentSelectedTilePos.Item1, currentSelectedTilePos.Item2].ResetTileAppearance();

      for (int i = 0; i < rows; i++)
      {
         for (int j = 0; j < cols; j++)
         {
            if (physicalTiles[i, j].gameObject == tile.gameObject)
            {
               currentSelectedTilePos = (i, j);
               tile.TileSelected();
               return;
            }
         }
      }
   }

   public void UndoFire()
   {
      if (undoStack.Count > 0)
      {
         UndoAction lastAction = undoStack.Pop();
         virtualBoards[lastAction.CurrentActiveOpponent].BoardField = lastAction.OriginalBoardState;
         virtualBoards[lastAction.CurrentActiveOpponent].PrintBoard();

         if (lastAction.CurrentActiveOpponent != currentActiveOpponent)
         {
            physicalTiles[lastAction.Row, lastAction.Col].InteractedWith = false;
            StartCoroutine(SwitchPlayersReplay());
         }
         else
         {
            if (lastAction.OriginalSymbol == 'S')
            {
               virtualBoards[lastAction.CurrentActiveOpponent].ReverseShot(lastAction.Row, lastAction.Col);
            }
            UpdatePhysicalBoard();
            UpdateVisuals();
         }

         NotificationManager.Instance.PlayNotification("Undo!");
      }
      else
      {
         NotificationManager.Instance.PlayNotification("No more turns to undo!");
      }
   }

   public void Fire()
   {
      int rowSel = currentSelectedTilePos.Item1;
      int colSel = currentSelectedTilePos.Item2;
      UndoAction action = new UndoAction();
      if (!InReplay)
      {
         if (rowSel == -1 && colSel == -1)
         {
            NotificationManager.Instance.PlayNotification("You must first select a field before firing!");
            return;
         }

         fieldsFired.Add(new MoveData
         {
            row = rowSel,
            col = colSel
         });
      }
      else if (PlayerPrefs.GetInt("replayDuration", -1) == -1)
      {
         char[,] destBoard = new char[rows, cols];
         Array.Copy(virtualBoards[currentActiveOpponent].BoardField, destBoard, virtualBoards[currentActiveOpponent].BoardField.Length);

         action.OriginalBoardState = destBoard;
         action.Row = rowSel;
         action.Col = colSel;
         action.CurrentActiveOpponent = currentActiveOpponent;
      }
      virtualBoards[currentActiveOpponent].PrintBoard();
      bool hit = virtualBoards[currentActiveOpponent].HandleShot(rowSel, colSel);
      if (PlayerPrefs.GetInt("replayDuration", -1) == -1)
      {
         action.OriginalSymbol = virtualBoards[currentActiveOpponent].getBoardTileStatus(rowSel, colSel);
         undoStack.Push(action);
      }

      UpdateBoardVisual(virtualBoards[currentActiveOpponent]);
      HandleShotOutcome(hit);
      physicalTiles[rowSel, colSel].InteractedWith = true;
      currentSelectedTilePos = (-1, -1);
   }

   IEnumerator SwitchPlayersReplay()
   {
      yield return new WaitForSeconds(2f);
      FlipPlayers();
      UpdateVisuals();
      shipSunkReact();
   }

   IEnumerator SwitchPlayers()
   {
      DisableBoardInteraction();
      yield return new WaitForSeconds(2f);
      EnableBoardInteraction();
      FlipPlayers();
      UpdatePhysicalBoard();
      UpdateVisuals();
   }

   private void EnableBoardInteraction()
   {
      IsGameActive = true;
      fireButton.enabled = true;
   }

   private void DisableBoardInteraction()
   {
      IsGameActive = false;
      fireButton.enabled = false;
   }

   private void UpdatePhysicalBoard()
   {
      for (int i = 0; i < virtualBoards[currentActiveOpponent].BoardField.GetLength(0); i++)
      {
         for (int j = 0; j < virtualBoards[currentActiveOpponent].BoardField.GetLength(1); j++)
         {
            if (virtualBoards[currentActiveOpponent].getBoardTileStatus(i, j) == 'X' || virtualBoards[currentActiveOpponent].getBoardTileStatus(i, j) == '.')
            {
               physicalTiles[i, j].InteractedWith = false;
            }
            else
            {
               physicalTiles[i, j].InteractedWith = true;
            }
         }
      }
   }

   private void HandleShotOutcome(bool hit)
   {
      if (!hit)
      {
         StartCoroutine(SwitchPlayers());
         NotificationManager.Instance.PlayNotification("Miss!");
      }
      else
      {
         NotificationManager.Instance.PlayNotification("Hit!");
      }
   }

   public void shipSunkReact()
   {
      int amountAliveShips = virtualBoards[currentActiveOpponent].getAmountAliveShips();
      int amountDeadShips = virtualBoards[currentActiveOpponent].ShipCount() - amountAliveShips;
      enemyShipsAlive.text = "Enemy ships alive: " + amountAliveShips;
      enemyShipsSunk.text = "Enemy ships sunk: " + amountDeadShips;
   }

   public void GameOver()
   {
      DisableBoardInteraction();
      endGameScreen.SetActive(true);
      endGameScreen.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Player " + (currentActivePlayer + 1) + " wins";
      if (!InReplay)
      {
         SaveRound();
      }
   }

   public void BackToMenu(){
      SceneManager.LoadScene(0);
   }

   public void SaveRound()
   {
      List<ShipData> p1Ships = new List<ShipData>();


      foreach (Ship ship in virtualBoards[0].GetShips())
      {
         p1Ships.Add(new ShipData
         {
            length = ship.Length,
            orientation = ship.Orientation,
            row = ship.Position.Item1,
            col = ship.Position.Item2
         });
      }

      List<ShipData> p2Ships = new List<ShipData>();
      foreach (Ship ship in virtualBoards[1].GetShips())
      {
         p2Ships.Add(new ShipData
         {
            length = ship.Length,
            orientation = ship.Orientation,
            row = ship.Position.Item1,
            col = ship.Position.Item2
         });
      }

      var saveObject = new SaveData()
      {
         p1ships = p1Ships,
         p2ships = p2Ships,
         moves = fieldsFired
      };
      string saveDirectory = Application.persistentDataPath + "/Replays";
      if (!System.IO.Directory.Exists(saveDirectory))
      {
         System.IO.Directory.CreateDirectory(saveDirectory);
      }
      string json = JsonUtility.ToJson(saveObject);
      string timestamp = System.DateTime.Now.ToString("yyyyMMddHHmmss");
      string filePath = saveDirectory + "/replay_" + timestamp + ".json";
      System.IO.File.WriteAllText(filePath, json);
   }

   public void LoadRound()
   {
      string saveDirectory = Application.persistentDataPath + "/Replays/" + PlayerPrefs.GetString("replay");
      if (System.IO.File.Exists(saveDirectory))
      {
         string json = System.IO.File.ReadAllText(saveDirectory);
         SaveData loadedData = JsonUtility.FromJson<SaveData>(json);
         p1ShipsLoaded = loadedData.p1ships;
         p2ShipsLoaded = loadedData.p2ships;
         fieldsFired = loadedData.moves;
      }
   }

   public class UndoAction
   {
      public char[,] OriginalBoardState { get; set; }
      public int Row { get; set; }
      public int Col { get; set; }
      public int CurrentActiveOpponent { get; set; }

      public char OriginalSymbol { get; set; }
   }
}
