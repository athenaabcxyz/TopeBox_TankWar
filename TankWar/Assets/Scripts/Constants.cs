using Unity.Netcode;

namespace Topebox.Tankwars
{
    public static class Constants
    {
        public enum Direction
        {
            UP =1,
            LEFT =2,
            DOWN =3,
            RIGHT =4,
            NULL =0
        }

        public enum CellType
        {
            EMPTY = 0,
            WALL = 1,
            RED = 2,
            BLUE = 3,
        } 
        
        public enum GameResult
        {
            PLAYING,
            DRAW,
            PLAYER1_WIN,
            PLAYER2_WIN,
        }
        
        public enum TankType
        {
            RED,
            BLUE
        }
    }


}