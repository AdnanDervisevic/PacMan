#region File Description
    //////////////////////////////////////////////////////////////////////////
   // TileContent                                                          //
  //                                                                      //
 // Copyright (C) Veritas. All Rights reserved.                          //
//////////////////////////////////////////////////////////////////////////
#endregion

namespace PacManLib.Map
{
    /// <summary>
    /// The different states of a tile.
    /// </summary>
    public enum TileContent
    {
        Path = 0,
        HorizontalWall = 1,
        VerticalWall = 2,
        Corner1 = 3,
        Corner2 = 4,
        Corner3 = 5,
        Corner4 = 6,
        HorizontalLeftStop = 7,
        HorizontalRightStop = 8,
        VerticalTopStop = 9,
        VerticalBottomStop = 10,
        Barrier = 11,
        Ring = 12,
        Dot = 13,
        RingTurn = 14,
        DotTurn = 15,
        Turn = 16,
        Door = 17,
        FruitSpawn = 18,
    }
}