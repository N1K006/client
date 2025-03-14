using SFML.System;
using  SFML.Window;
using SFML.Graphics;

namespace client
{
    internal class Grafica
    {
        // Sfondo Schermata Home
        public const int WIDTH = 1023, HEIGHT = 683;
        static IntRect sfondo = new IntRect(0, 0, WIDTH, HEIGHT);
        static Texture Sfondo = new Texture(@"..\..\..\immagini\sfondo.png", sfondo);
        static Sprite SFONDO = new Sprite(Sfondo);

        static public void Disegna(RenderWindow finestra)
        {
            finestra.Draw(SFONDO);
        }
    }
}
