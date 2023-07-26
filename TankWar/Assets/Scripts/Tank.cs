using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Topebox.Tankwars
{
    [Serializable]
    public class Tank : MonoBehaviour
    {
        public Constants.TankType CurrentTank = Constants.TankType.RED;
        public SpriteRenderer SpriteRenderer;
        public Sprite RedSprite;
        public Sprite BlueSprite;
        public int PlayerId;
        public Vector2 CurrentCell;

        public void SetType(Constants.TankType tankType)
        {
            CurrentTank = tankType;
            switch (CurrentTank)
            {
                case Constants.TankType.RED:
                    SpriteRenderer.sprite = RedSprite;
                    break;
                case Constants.TankType.BLUE:
                    SpriteRenderer.sprite = BlueSprite;
                    break;
            }
        }

        public void SetId(int playerId)
        {
            PlayerId = playerId;
        }

        public void SetCurrentCell(Vector2 pos)
        {
            CurrentCell = pos;
        }

        public Constants.Direction GetNextMove(GameState game, Constants.CellType[,] logicMap, Vector2 otherPosition)
        {
            var myPosition = CurrentCell;
            var enemyPosition = otherPosition;
            List<Move> predictMoveList = new List<Move>();
            int depth = 0;
            int score = game.CurrentPlayer == 1 ? (game.ScoreRed - game.ScoreBlue) : (game.ScoreBlue - game.ScoreRed);
            Move enemyMove = new Move
            {
                originalDirection = Constants.Direction.NULL,
                depth = -1,
                isMax = false,
                score = score,
                currentPosition = otherPosition,
            };

            var availableMove = new List<Constants.Direction>();
            var upCell = game.GetNextCell(myPosition, Constants.Direction.UP);

            if (game.IsValidCell(upCell))
            {
                Move myMove = new Move
                {
                    originalDirection = Constants.Direction.UP,
                    depth = depth,
                    isMax = true,
                    score = score + 1,
                    currentPosition = upCell,
                };
                predictMoveList.Add(myMove);
                PredictNextMove(ref predictMoveList, enemyMove, myMove, game);
            }

            var downCell = game.GetNextCell(myPosition, Constants.Direction.DOWN);
            if (game.IsValidCell(downCell))
            {
                Move myMove = new Move
                {
                    originalDirection = Constants.Direction.DOWN,
                    depth = depth,
                    isMax = true,
                    score = score + 1,
                    currentPosition = downCell,
                };
                predictMoveList.Add(myMove);

                PredictNextMove(ref predictMoveList, enemyMove, myMove, game);
            }

            var leftCell = game.GetNextCell(myPosition, Constants.Direction.LEFT);
            if (game.IsValidCell(leftCell))
            {
                Move myMove = new Move
                {
                    originalDirection = Constants.Direction.LEFT,
                    depth = depth,
                    isMax = true,
                    score = score + 1,
                    currentPosition = leftCell,
                };
                predictMoveList.Add(myMove);
                PredictNextMove(ref predictMoveList, enemyMove, myMove, game);
            }

            var rightCell = game.GetNextCell(myPosition, Constants.Direction.RIGHT);
            if (game.IsValidCell(rightCell))
            {
                Move myMove = new Move
                {
                    originalDirection = Constants.Direction.RIGHT,
                    depth = depth,
                    isMax = true,
                    score = score + 1,
                    currentPosition = rightCell,
                };
                predictMoveList.Add(myMove);

                PredictNextMove(ref predictMoveList, enemyMove, myMove, game);
            }

            //TODO: Your logic here
            return Minimax(predictMoveList, game); //temp return random move
        }

        void PredictNextMove(ref List<Move> moves, Move previousMove, Move enemies, GameState game)
        {
            Move move = new Move
            {
                depth = enemies.depth + 1,
                isMax = previousMove.isMax,
            };
            if (previousMove.originalDirection == Constants.Direction.NULL)
            {
                move.originalDirection = Constants.Direction.NULL;
            }
            if (move.depth <= 9)
            {


                var upCell = game.GetNextCell(previousMove.currentPosition, Constants.Direction.UP);
                if (game.IsValidCell(upCell))
                {
                    move.currentPosition = upCell;
                    move.score = move.isMax ? enemies.score + 1 : enemies.score - 1;
                    if (previousMove.originalDirection == Constants.Direction.NULL)
                    {
                        move.originalDirection = Constants.Direction.UP;
                    }
                    else
                    {
                        move.originalDirection = previousMove.originalDirection;
                    }
                    bool isOccupied = false;
                    foreach (Move loopMove in moves)
                    {
                        if (loopMove.currentPosition == move.currentPosition && loopMove.depth < move.depth)
                        {
                            isOccupied = true;
                            break;
                        }
                    }
                    if (!isOccupied)
                    {
                        moves.Add(move);
                        PredictNextMove(ref moves, enemies, move, game);
                    }

                }

                var downCell = game.GetNextCell(previousMove.currentPosition, Constants.Direction.DOWN);
                if (game.IsValidCell(downCell))
                {
                    move.currentPosition = downCell;
                    move.score = move.isMax ? enemies.score + 1 : enemies.score - 1;
                    if (previousMove.originalDirection == Constants.Direction.NULL)
                    {
                        move.originalDirection = Constants.Direction.DOWN;
                    }
                    else
                    {
                        move.originalDirection = previousMove.originalDirection;
                    }
                    bool isOccupied = false;
                    foreach (Move loopMove in moves)
                    {
                        if (loopMove.currentPosition == move.currentPosition && loopMove.depth < move.depth)
                        {
                            isOccupied = true;
                            break;
                        }

                    }
                    if (!isOccupied)
                    {
                        moves.Add(move);
                        PredictNextMove(ref moves, enemies, move, game);
                    }
                }

                var leftCell = game.GetNextCell(previousMove.currentPosition, Constants.Direction.LEFT);
                if (game.IsValidCell(leftCell))
                {
                    move.currentPosition = leftCell;
                    move.score = move.isMax ? enemies.score + 1 : enemies.score - 1;
                    if (previousMove.originalDirection == Constants.Direction.NULL)
                    {
                        move.originalDirection = Constants.Direction.LEFT;
                    }
                    else
                    {
                        move.originalDirection = previousMove.originalDirection;
                    }
                    bool isOccupied = false;
                    foreach (Move loopMove in moves)
                    {
                        if (loopMove.currentPosition == move.currentPosition && loopMove.depth < move.depth)
                        {
                            isOccupied = true;
                            break;
                        }
                    }
                    if (!isOccupied)
                    {
                        moves.Add(move);
                        PredictNextMove(ref moves, enemies, move, game);
                    }
                }

                var rightCell = game.GetNextCell(previousMove.currentPosition, Constants.Direction.RIGHT);
                if (game.IsValidCell(rightCell))
                {
                    move.currentPosition = rightCell;
                    move.score = move.isMax ? enemies.score + 1 : enemies.score - 1;
                    if (previousMove.originalDirection == Constants.Direction.NULL)
                    {
                        move.originalDirection = Constants.Direction.RIGHT;
                    }
                    else
                    {
                        move.originalDirection = previousMove.originalDirection;
                    }
                    bool isOccupied = false;
                    foreach (Move loopMove in moves)
                    {
                        if (loopMove.currentPosition == move.currentPosition && loopMove.depth < move.depth)
                        {
                            isOccupied = true;
                            break;
                        }
                    }
                    if (!isOccupied)
                    {
                        moves.Add(move);
                        PredictNextMove(ref moves, enemies, move, game);
                    }
                }
            }
        }

        Constants.Direction Minimax(List<Move> moves, GameState game)
        {
            int maxScore = -999;
            Move maxMove = new Move();
            maxMove.depth = 0;

            foreach (Move move in moves)
            {
                if (move.isMax)
                {
                    if (maxScore == move.score)
                    {
                        if (maxMove.depth < move.depth)
                        {
                            maxScore = move.score;
                            maxMove = move;
                        }
                    }
                    else
                        if (maxScore < move.score)
                    {
                        maxScore = move.score;
                        maxMove = move;
                    }
                }

            }

            return maxMove.originalDirection;
        }

    }
    public struct Move
    {
        public Constants.Direction originalDirection;
        public Vector2 currentPosition;
        public int score;
        public bool isMax;
        public int depth;
    }



    public class MoveAction
    {
        public Move move;
        public MoveAction up { get; set; }
        public MoveAction left { get; set; }
        public MoveAction right { get; set; }
        public MoveAction down { get; set; }

    }
}