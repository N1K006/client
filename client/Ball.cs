using SFML.System;

namespace client
{
    internal class Ball
    {
        public Vector2f pos;
        public int r = 5, scale = 1;

        public Ball(Vector2f pos)
        {
            this.pos = pos;
        }
    }
}
