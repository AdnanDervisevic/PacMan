namespace PacManLib.Map
{
    public class Tile
    {
        private TileContent contentCode = TileContent.Empty;

        public TileContent ContentCode
        {
            get { return this.contentCode; }
            set { this.contentCode = value; }
        }
    }
}