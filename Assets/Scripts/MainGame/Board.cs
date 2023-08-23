using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Board : MonoBehaviour
{
   private const int rows = 10;
   private const int cols = 10;

   public int Rows { get { return rows; } }
   public int Cols { get { return cols; } }
   private char[,] board;

   public char[,] BoardField { get { return board; } set { board = value; } }

   [SerializeField] private List<Ship> ships;
   private List<ShipSunkListener> listeners;
   System.Random rand;

   private void Awake()
   {
      rand = new System.Random();
      InitializeBoard();
   }

   private void Start()
   {
      listeners = new List<ShipSunkListener>();
      listeners.Add(GameManager.Instance);

      StartCoroutine(DelayedLoad());
      fireListeners();
   }

   IEnumerator DelayedLoad()
   {
      yield return new WaitForSeconds(1f);
      ShipPlacementInitialization();
   }
   private void ShipPlacementInitialization()
   {
      if (!(PlayerPrefs.GetInt("inReplay", 0) == 0 ? false : true))
      {
         foreach (Ship ship in ships)
         {
            ship.InitializeShip();
            PlaceShip(ship);
         }
      }
      else
      {
         PlaceShipsReplay();
      }

      PrintBoard();
      GameManager.Instance.UpdateVisuals();
   }
   private void InitializeBoard()
   {
      board = new char[rows, cols];
      for (int i = 0; i < rows; i++)
      {
         for (int j = 0; j < cols; j++)
         {
            board[i, j] = '.';
         }
      }
   }

   public void PlaceShipsReplay()
   {
      List<ShipData> shipPlacement = GameManager.Instance.fetchShipData(gameObject.name);
      for (int i = 0; i < shipPlacement.Count; i++)
      {
         int orientation = shipPlacement[i].orientation;
         int length = shipPlacement[i].length;
         int row = shipPlacement[i].row;
         int col = shipPlacement[i].col;
         if (orientation == 0) // horizontal
         {
            for (int j = col; j < col + length; j++)
            {
               board[row, j] = 'X';
            }
         }
         else // vertical
         {
            for (int j = row; j < row + length; j++)
            {
               board[j, col] = 'X';
            }
         }

         ships[i].Position = (row, col);
         ships[i].Orientation = orientation;
         ships[i].InitializeShip();
      }
   }

   private void PlaceShip(Ship ship)
   {
      int length = ship.Length;
      while (true)
      {
         int orientation = rand.Next(2);
         int row = rand.Next(10);
         int col = rand.Next(10);

         if (CanPlaceShip(row, col, length, orientation))
         {
            if (orientation == 0) // horizontal
            {
               for (int i = col; i < col + length; i++)
               {
                  board[row, i] = 'X';
               }
            }
            else // vertical
            {
               for (int i = row; i < row + length; i++)
               {
                  board[i, col] = 'X';
               }
            }

            ship.Position = (row, col);
            ship.Orientation = orientation;
            break;
         }
      }
   }

   public void ReverseShot(int row, int col)
   {
      foreach (Ship ship in ships)
      {
         int shipRow = ship.Position.Item1;
         int shipCol = ship.Position.Item2;
         if (ship.Length == 1)
         {
            if (row == shipRow && col == shipCol)
            {
               ship.UnsetHit(0);
               fireListeners();
            }
         }
         else
         {
            if (ship.Orientation == 0 && row == shipRow && col >= shipCol && col < shipCol + ship.Length)
            {
               ship.UnsetHit(col - shipCol);
               fireListeners();
            }
            else if (ship.Orientation == 1 && col == shipCol && row >= shipRow && row < shipRow + ship.Length)
            {
               ship.UnsetHit(row - shipRow);
               fireListeners();
            }
         }
      }
   }

   public bool HandleShot(int row, int col)
   {
      if (board[row, col] == 'X')
      {
         foreach (Ship ship in ships)
         {
            if (!ship.Sunk())
            {
               int shipRow = ship.Position.Item1;
               int shipCol = ship.Position.Item2;
               if (ship.Length == 1)
               {
                  if (row == shipRow && col == shipCol)
                  {
                     ship.SetHit(0);
                     if (ship.Sunk())
                     {
                        board[row, col] = 'S';
                        fireListeners();
                        CheckWinCondition();
                     }
                     return true;
                  }
               }
               else
               {
                  //orientation horizontal 0, vertical 1
                  if (ship.Orientation == 0 && row == shipRow && col >= shipCol && col < shipCol + ship.Length)
                  {
                     ship.SetHit(col - shipCol);
                     board[row, col] = 'H';
                     if (ship.Sunk())
                     {
                        for (int i = shipCol; i < shipCol + ship.Length; i++)
                        {
                           board[row, i] = 'S';
                           fireListeners();
                           CheckWinCondition();
                        }
                     }
                     return true;
                  }
                  else if (ship.Orientation == 1 && col == shipCol && row >= shipRow && row < shipRow + ship.Length)
                  {
                     ship.SetHit(row - shipRow);
                     board[row, col] = 'H'; // Hit

                     if (ship.Sunk())
                     {
                        for (int i = shipRow; i < shipRow + ship.Length; i++)
                        {
                           board[i, col] = 'S';
                           fireListeners();
                           CheckWinCondition();
                        }
                     }
                     return true;
                  }
               }
            }
         }
      }
      else if (board[row, col] != 'H') // Avoid marking an already hit position
      {
         board[row, col] = 'M'; // Miss
      }

      return false;
   }

   private bool CanPlaceShip(int row, int col, int length, int orientation)
   {
      if (orientation == 0) // horizontal
      {
         if (col + length > 10)
            return false;

         for (int i = col; i < col + length; i++)
         {
            if (board[row, i] == 'X')
            {
               return false;
            }

            for (int r = Math.Max(0, row - 1); r <= Math.Min(9, row + 1); r++)
            {
               for (int c = Math.Max(0, i - 1); c <= Math.Min(9, i + 1); c++)
               {
                  if (board[r, c] == 'X')
                  {
                     return false;
                  }
               }
            }
         }

         return true;
      }
      else // vertical
      {
         if (row + length > 10)
            return false;

         for (int i = row; i < row + length; i++)
         {
            if (board[i, col] == 'X')
            {
               return false;
            }

            for (int r = Math.Max(0, i - 1); r <= Math.Min(9, i + 1); r++)
            {
               for (int c = Math.Max(0, col - 1); c <= Math.Min(9, col + 1); c++)
               {
                  if (board[r, c] == 'X')
                  {
                     return false;
                  }
               }
            }
         }

         return true;
      }
   }

   public void PrintBoard()
   {
      string output = "";
      for (int i = 0; i < 10; i++)
      {

         for (int j = 0; j < 10; j++)
         {

            output += board[i, j] + " ";
         }
         output += "\n";
      }
   }

   public char getBoardTileStatus(int i, int j)
   {
      if (i > rows || i < 0 || j > cols || j < 0)
      {
         throw new ArgumentOutOfRangeException();
      }
      return board[i, j];
   }

   public int getAmountAliveShips()
   {
      int i = 0;
      foreach (Ship ship in ships)
      {
         if (!ship.Sunk())
            i++;
      }

      return i;
   }

   public int ShipCount()
   {
      return ships.Count;
   }

   private void CheckWinCondition()
   {
      foreach (Ship ship in ships)
      {
         if (!ship.Sunk())
            return;
      }

      GameManager.Instance.GameOver();
   }

   private void fireListeners()
   {
      foreach (ShipSunkListener listener in listeners)
      {
         listener.shipSunkReact();
      }
   }

   public List<Ship> GetShips()
   {
      return ships;
   }

   public Ship GetShip(int index)
   {
      return ships[index];
   }

}
