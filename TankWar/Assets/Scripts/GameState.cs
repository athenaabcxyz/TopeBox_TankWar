using System.Collections.Generic;
using System.Diagnostics;
using DG.Tweening;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Topebox.Tankwars
{
    public class GameState : NetworkBehaviour
    {
        public Vector2 Player1Position;
        public Vector2 Player2Position;

        public TextMeshProUGUI TextScore;
        public bool isGameStarted = false;

        public GameConfig Config;
        private Constants.CellType[,] logicMap;
        private Cell[,] displayMap;
        public Cell cellPrefab;
        [SerializeField] GameObject redTurn;
        [SerializeField] GameObject blueTurn;
        public TextMeshProUGUI redScore;
        public TextMeshProUGUI blueScore;
        public TextMeshProUGUI moveText;
        [SerializeField] Button nextMove;
        [SerializeField] Button start;

        public Tank tankPrefab;
        public Tank player1Tank;
        public Tank player2Tank;
        public Transform TankParent;
        public int ScoreRed = 0;
        public int ScoreBlue = 0;

        public int CurrentPlayer = 1;
        public int CurrentTurn = 0;
        public List<Vector2> Player1Moves = new List<Vector2>();
        public List<Vector2> Player2Moves = new List<Vector2>();



        public bool IsGameOver = false;
        [FormerlySerializedAs("IsMoving")] public bool CanMove = false;


        public int SerializeDirection(Constants.Direction direction)
        {
            return (int)direction;
        }
        public Constants.Direction DeSerializeDirection(int direction)
        {
            return (Constants.Direction)direction;
        }

        public int[] SerializeLogicMap(Constants.CellType[,] logicMap)
        {
            int[] currentMap = new int[Config.MapWidth * Config.MapHeight];
            for (int x = 0; x < Config.MapWidth; x++)
            {
                for (int y = 0; y < Config.MapHeight; y++)
                {
                    currentMap[x * Config.MapHeight + y] = (int)logicMap[x, y];
                }
            }

            return currentMap;
        }

        public Constants.CellType[,] DeSerializeLogicMap(int[] logicMap)
        {
            Constants.CellType[,] currentMap = new Constants.CellType[Config.MapWidth, Config.MapHeight];
            for (int x = 0; x < Config.MapWidth; x++)
            {
                for (int y = 0; y < Config.MapHeight; y++)
                {
                    currentMap[x, y] = (Constants.CellType)logicMap[x * Config.MapHeight + y];
                }
            }

            return currentMap;
        }


        public void MoveButtonOnLick()
        {
            if (CurrentPlayer == 1)
            {
                UpdateMoveServerRPC(SerializeDirection(player1Tank.GetNextMove(this, logicMap, player2Tank.CurrentCell)));
            }
            else
            {
                UpdateMoveServerRPC(SerializeDirection(player2Tank.GetNextMove(this, logicMap, player1Tank.CurrentCell)));
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateMoveServerRPC(int direction)
        {
            UpdateMoveClientRPC(direction);
        }

        [ClientRpc]
        public void UpdateMoveClientRPC(int direction)
        {
            UpdateMove(DeSerializeDirection(direction));
        }
        public void UpdateMove(Constants.Direction direction)
        {
            if (!CanMove || IsGameOver)
            {
                Debug.LogError("IsMoving:" + CanMove + " IsGameOver:" + IsGameOver);
                return;
            }


            var winPlayer = CheckGameOver();
            if (winPlayer != Constants.GameResult.PLAYING)
            {
                IsGameOver = true;
                Debug.LogError($" IsGameOver {IsGameOver} Result {winPlayer}");
                switch (winPlayer)
                {
                    case Constants.GameResult.PLAYER1_WIN:
                        TextScore.text = "Game Over Player 1 Win";
                        break;
                    case Constants.GameResult.PLAYER2_WIN:
                        TextScore.text = "Game Over Player 2 Win";
                        break;
                    case Constants.GameResult.DRAW:
                        TextScore.text = "Game Over Draw";
                        break;
                }

                //Export Orginal Map and Moves of players and winner
                return;
            }

            if (player1Tank.PlayerId == CurrentPlayer)
            {
                var hasMoveP1 = HasValidMove(player1Tank.CurrentCell);
                if (hasMoveP1)
                {
                    UpdateMoveForTankClientRPC(1, direction);
                }
                else
                {
                    Debug.Log("No move left for player1 => player2 turn");
                }

                CurrentPlayer = player2Tank.PlayerId;
                redTurn.SetActive(false);
                blueTurn.SetActive(true);
            }
            else if (player2Tank.PlayerId == CurrentPlayer)
            {
                var hasMoveP2 = HasValidMove(player2Tank.CurrentCell);
                if (hasMoveP2)
                {
                    UpdateMoveForTankClientRPC(2, direction);
                }
                else
                {
                    Debug.Log("No move left for player2 => player1 turn");
                }

                CurrentPlayer = player1Tank.PlayerId;
                redTurn.SetActive(true);
                blueTurn.SetActive(false);
            }
        }


        private Constants.GameResult CheckGameOver()
        {
            var hasMoveP1 = HasValidMove(player1Tank.CurrentCell);
            var hasMoveP2 = HasValidMove(player2Tank.CurrentCell);
            if (!hasMoveP1 && hasMoveP2)
            {
                return Constants.GameResult.PLAYER2_WIN;
            }

            if (hasMoveP1 && !hasMoveP2)
            {
                return Constants.GameResult.PLAYER1_WIN;
            }

            if (!hasMoveP1 && !hasMoveP2)
            {
                if (ScoreRed > ScoreBlue)
                    return Constants.GameResult.PLAYER1_WIN;
                if (ScoreRed < ScoreBlue)
                    return Constants.GameResult.PLAYER2_WIN;
                return Constants.GameResult.DRAW;
            }

            return Constants.GameResult.PLAYING; //not over
        }

        private bool HasValidMove(Vector2 currentCell)
        {
            var upCell = GetNextCell(currentCell, Constants.Direction.UP);
            if (IsValidCell(upCell))
            {
                return true;
            }

            var downCell = GetNextCell(currentCell, Constants.Direction.DOWN);
            if (IsValidCell(downCell))
            {
                return true;
            }

            var leftCell = GetNextCell(currentCell, Constants.Direction.LEFT);
            if (IsValidCell(leftCell))
            {
                return true;
            }

            var rightCell = GetNextCell(currentCell, Constants.Direction.RIGHT);
            if (IsValidCell(rightCell))
            {
                return true;
            }

            return false;
        }

        public Vector2 UpdateMoveForTank(Tank currentTank, Tank otherTank, Constants.Direction direction)
        {
            var nextCell = GetNextCell(currentTank.CurrentCell, direction);
            //CheckValidDirection
            if (IsValidCell(nextCell))
            {
                currentTank.SetCurrentCell(nextCell);

                var position = GetPosition(nextCell);
                CanMove = false;
                currentTank.transform.DORotate(GetRotateByDirection(direction), 0.5f);
                currentTank.transform.DOMove(new Vector3(position.x, position.y, 0), 1f).OnComplete(() =>
                {
                    CanMove = true;
                    OccupyPosition(nextCell, currentTank.CurrentTank);
                });
                return nextCell;
            }
            else
            {
                Debug.LogError(
                    $"Your Direction Is Invalid Direction:{direction} CurrentCell:{currentTank.CurrentCell}");
            }

            return new Vector2(-1, -1);
        }

        [ClientRpc]
        public void UpdateMoveForTankClientRPC(int currentPlayer, Constants.Direction direction)
        {
            if (player1Tank.PlayerId == currentPlayer)
            {
                AddMoveServerRPC(UpdateMoveForTank(player1Tank, player2Tank, direction));
            }
            else
                if (player2Tank.PlayerId == currentPlayer)
            {
                AddMoveServerRPC(UpdateMoveForTank(player2Tank, player1Tank, direction));
            }

        }

        [ServerRpc(RequireOwnership = false)]
        public void AddMoveServerRPC(UnityEngine.Vector2 vector2)
        {
            if (player1Tank.PlayerId == CurrentPlayer)
            {
                Player1Moves.Add(vector2);
            }
            else
                if (player2Tank.PlayerId == CurrentPlayer)
            {
                Player2Moves.Add(vector2);
            }
        }

        private Vector3 GetRotateByDirection(Constants.Direction direction)
        {
            switch (direction)
            {
                case Constants.Direction.UP:
                    return new Vector3(0, 0, 180);
                case Constants.Direction.LEFT:
                    return new Vector3(0, 0, 270);
                case Constants.Direction.DOWN:
                    return new Vector3(0, 0, 0);
                case Constants.Direction.RIGHT:
                    return new Vector3(0, 0, 90);
            }

            return Vector3.zero;
        }

        public bool IsValidCell(Vector2 nextCell)
        {
            if (nextCell.x < 0 || nextCell.x >= Config.MapWidth)
                return false;
            if (nextCell.y < 0 || nextCell.y >= Config.MapHeight)
                return false;

            return logicMap[(int)nextCell.x, (int)nextCell.y] == Constants.CellType.EMPTY;
        }

        private void OccupyPosition(Vector2 cell, Constants.TankType tankType)
        {
            if (logicMap[(int)cell.x, (int)cell.y] == Constants.CellType.EMPTY)
            {
                switch (tankType)
                {
                    case Constants.TankType.RED:
                        logicMap[(int)cell.x, (int)cell.y] = Constants.CellType.RED;
                        IncreaseScore(1, Constants.TankType.RED);
                        break;
                    case Constants.TankType.BLUE:
                        logicMap[(int)cell.x, (int)cell.y] = Constants.CellType.BLUE;
                        IncreaseScore(1, Constants.TankType.BLUE);
                        break;
                }

                UpdateMap();
            }
        }



        private void IncreaseScore(int score, Constants.TankType tankType)
        {
            switch (tankType)
            {
                case Constants.TankType.RED:
                    ScoreRed += score;
                    break;
                case Constants.TankType.BLUE:
                    ScoreBlue += score;
                    break;
            }

            UpdateScoreUI();
        }


        public Vector2 GetNextCell(Vector2 currentPosition, Constants.Direction direction)
        {
            switch (direction)
            {
                case Constants.Direction.UP:
                    return new Vector2(currentPosition.x, currentPosition.y - 1);
                case Constants.Direction.LEFT:
                    return new Vector2(currentPosition.x - 1, currentPosition.y);
                case Constants.Direction.DOWN:
                    return new Vector2(currentPosition.x, currentPosition.y + 1);
                case Constants.Direction.RIGHT:
                    return new Vector2(currentPosition.x + 1, currentPosition.y);
            }

            return currentPosition;
        }
        public void Awake()
        {
            logicMap = new Constants.CellType[Config.MapWidth, Config.MapHeight];
            displayMap = new Cell[Config.MapWidth, Config.MapHeight];

        }

        [ServerRpc(RequireOwnership = false)]
        public void SyncMapServerRPC()
        {
            SyncMapClientRPC(SerializeLogicMap(logicMap));
        }

        [ClientRpc]
        public void SyncMapClientRPC(int[] syncMap)
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                logicMap = DeSerializeLogicMap(syncMap);
            }
            UpdateMap();
            InitGame();
        }

        public void StartGame()
        {
            StartGameServerRPC();
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartGameServerRPC()
        {
            StartGameClientRPC();
        }

        [ClientRpc]
        public void StartGameClientRPC()
        {
            DrawMap();
            if (IsOwner)
            {
                GenerateMap();
            }
            else
            {
                SyncMapServerRPC();
            }
            if (!isGameStarted)
                isGameStarted = true;
        }

        public void InitGame()
        {
            player1Tank = CreateTank(Config.Player1Type, 1);
            player1Tank.transform.DORotate(GetRotateByDirection(Constants.Direction.DOWN), 0.1f);

            player2Tank = CreateTank(Config.Player2Type, 2);
            player2Tank.transform.DORotate(GetRotateByDirection(Constants.Direction.UP), 0.1f);
            CanMove = false;
            nextMove.gameObject.SetActive(true);
            start.gameObject.SetActive(false);


            ScoreRed = 0;
            ScoreBlue = 0;
            CurrentPlayer = 1;
            redTurn.SetActive(true);
            blueTurn.SetActive(false);
            CurrentTurn = 0;
            Player1Moves.Clear();
            Player2Moves.Clear();

            player1Tank.SetCurrentCell(Player1Position);
            var position = GetPosition(Player1Position);
            player1Tank.transform.position = position;
            OccupyPosition(Player1Position, player1Tank.CurrentTank);

            player2Tank.SetCurrentCell(Player2Position);
            position = GetPosition(Player2Position);
            player2Tank.transform.position = position;
            OccupyPosition(Player2Position, player2Tank.CurrentTank);

            CanMove = true;
            IsGameOver = false;
        }

        public void Update()
        {
            if (isGameStarted)
            {
                if (CurrentPlayer == 1)
                {
                    redTurn.SetActive(true);
                    blueTurn.SetActive(false);
                    TextScore.SetText("Player 1 turn.");
                }
                else
                {
                    redTurn.SetActive(false);
                    blueTurn.SetActive(true);
                    TextScore.SetText("Player 2 turn.");
                }
                if (IsOwner)
                {
                    if (CurrentPlayer == 1)
                    {
                        nextMove.enabled = true;
                        moveText.SetText("Move");
                    }
                    else
                    {
                        nextMove.enabled = false;
                        moveText.SetText("Waiting");
                    }
                }
                else
                {
                    if (CurrentPlayer == 2)
                    {
                        nextMove.enabled = true;
                        moveText.SetText("Move");
                    }
                    else
                    {
                        nextMove.enabled = false;
                        moveText.SetText("Waiting");
                    }
                }
                redScore.SetText(ScoreRed.ToString());
                blueScore.SetText(ScoreBlue.ToString());
            }


        }

        private void UpdateScoreUI()
        {
            TextScore.text = $"<color=red>{ScoreRed}</color> - <color=blue>{ScoreBlue}</color>";
        }

        private Tank CreateTank(Constants.TankType tankType, int playerId)
        {
            var tank = Instantiate<Tank>(tankPrefab, new Vector3(0, 0, 0), Quaternion.identity,
                TankParent);
            tank.SetType(tankType);
            tank.SetId(playerId);
            return tank;
        }

        public Vector2 GetPosition(Vector2 cellPos)
        {
            return GetPosition(cellPos.x, cellPos.y);
        }

        public Vector2 GetPosition(float cellX, float cellY)
        {
            return new Vector2(cellX, -cellY);
        }


        private void UpdateMap()
        {
            for (int x = 0; x < Config.MapWidth; x++)
            {
                for (int y = 0; y < Config.MapHeight; y++)
                {
                    displayMap[x, y].SetType(logicMap[x, y]);
                }
            }
        }

        private void GenerateMap()
        {
            //generate random symmetric wall
            for (int i = 0; i < Config.WallCount / 2; i++)
            {
                var x = 0;
                var y = 0;
                while (x == 0 && y == 0)
                {
                    x = Random.Range(0, Config.MapWidth / 2);
                    y = Random.Range(0, Config.MapHeight / 2);
                }

                logicMap[x, y] = Constants.CellType.WALL;
                logicMap[Config.MapWidth - x - 1, Config.MapHeight - y - 1] = Constants.CellType.WALL;
            }
        }
        private void DrawMap()
        {
            for (int x = 0; x < Config.MapWidth; x++)
            {
                for (int y = 0; y < Config.MapHeight; y++)
                {
                    if (displayMap[x, y] == null)
                    {
                        var pos = GetPosition(new Vector2(x, y));
                        var cell = Instantiate(cellPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity,
                            transform);
                        displayMap[x, y] = cell;
                    }
                }
            }
        }
    }
}
