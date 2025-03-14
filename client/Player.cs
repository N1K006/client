using SFML.System;

namespace client
{
    internal class Player
    {
        public Vector2f pos;
        public int width = 100, scale = 1;

        public Player(Vector2f pos)
        {
            this.pos = pos;
        }
    }
}
