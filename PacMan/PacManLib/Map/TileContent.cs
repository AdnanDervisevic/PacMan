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
        WoodBarrier = 11,
        ghostBarrier = 12,
        Ring = 13,
        Dot = 14,
        RingTurn = 15,
        DotTurn = 16,
        Turn = 17,
        Door = 18
    }
}