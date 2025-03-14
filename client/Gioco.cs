using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SFML.System;
using SFML.Window;
using SFML.Graphics;
using static SFML.Window.Keyboard;

namespace client
{
    internal class Gioco
    {
        public RenderWindow finestra;

        int i;
        private object lockStream = new object();
        TcpClient client;
        NetworkStream stream;

        Dictionary<string, Func<string, Task>> actions;

        private object[] playerLocks = new object[2] { new object(), new object() };
        Player[] players = new Player[2];
        Ball ball;

        bool left = false, right = false;
        Key k;

        public Gioco(TcpClient client, RenderWindow finestra)
        {
            this.client = client;
            this.finestra = finestra;

            actions = new Dictionary<string, Func<string, Task>>
            {
                { "pos", pos }
            };

            gioco();
        }
        async Task gioco()
        {
            players[0] = new Player(new Vector2f(0, 0));
            players[1] = new Player(new Vector2f(0, 0));

            finestra.KeyPressed += OnKeyPressed;
            finestra.KeyReleased += OnKeyReleased;

            stream = client.GetStream();
            Task.Run(Read);

            while (finestra.IsOpen)
            {
                finestra.Clear();
                Grafica.Disegna(finestra);
                finestra.Display();
                finestra.DispatchEvents();
                await Task.Delay(10);
            }
        }
        async Task Read()
        {
            try
            {
                {
                    byte[] lunghezzaBytes = new byte[4];
                    lock (lockStream)
                    {
                        int bytesRead = stream.Read(lunghezzaBytes, 0, 4);

                        if (bytesRead == 0) // Client disconnesso
                            return;
                    }

                    int lunghezza = BitConverter.ToInt32(lunghezzaBytes, 0); // Converte i byte in int
                    byte[] index = new byte[lunghezza];

                    // Assicura di leggere tutto il messaggio
                    int letti = 0;
                    while (letti < lunghezza)
                        lock (lockStream)
                        {
                            int read = stream.Read(index, letti, lunghezza - letti);
                            if (read == 0) break; // Disconnessione
                            letti += read;
                        }

                    i = BitConverter.ToInt32(index, 0);
                }

                Task.Run(AggiornaServer);

                while (true)
                {
                    byte[] lunghezzaBytes = new byte[4];
                    lock (lockStream)
                    {
                        int bytesRead = stream.Read(lunghezzaBytes, 0, 4);

                        if (bytesRead == 0) // Client disconnesso
                            break;
                    }

                    int lunghezza = BitConverter.ToInt32(lunghezzaBytes, 0); // Converte i byte in int
                    byte[] dati = new byte[lunghezza];

                    // Assicura di leggere tutto il messaggio
                    int letti = 0;
                    while (letti < lunghezza)
                        lock (lockStream)
                        {
                            int read = stream.Read(dati, letti, lunghezza - letti);
                            if (read == 0) break; // Disconnessione
                            letti += read;
                        }

                    string message = Encoding.UTF8.GetString(dati);
                    Task.Run(() => AvviaAzione(message.Substring(0, 3), message.Substring(3)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore: " + ex.Message);
            }
        }
        async Task AggiornaServer()
        {
            while (true)
            {
                try
                {
                    string posizione = $"mov{i}{k.ToString()}";

                    Write(posizione);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errore nell'aggiornamento dello stato: " + ex.Message);
                }

                await Task.Delay(100);
                Console.WriteLine(players[0].pos + "    " + players[1].pos);
            }
        }
        void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Code == Key.A)
            {
                k = e.Code;
                left = true;
            }
            else if (e.Code == Key.D)
            {
                k = e.Code;
                right = true;
            }
        }
        void OnKeyReleased(object sender, KeyEventArgs e)
        {
            if (e.Code == Key.A)
            {
                left = false;
                if (right)
                    k = Key.D;
                else
                    k = Key.Unknown;
            }
            else if (e.Code == Key.D)
            {
                right = false;
                if (left)
                    k = Key.A;
                else
                    k = Key.Unknown;
            }
        }
        void Write(string messageString)
        {
            try
            {
                byte[] messageByte = Encoding.UTF8.GetBytes(messageString);

                int length = messageByte.Length;
                byte[] lengthByte = BitConverter.GetBytes(length);

                lock (lockStream)
                {
                    stream.Write(lengthByte, 0, 4);
                    stream.Write(messageByte, 0, messageByte.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nell'invio del messaggio {messageString}: " + ex.Message);
            }
        }
        async Task AvviaAzione(string chiave, string parametro)
        {
            if (actions.TryGetValue(chiave, out var action))
                action(parametro);
            else
                Console.WriteLine($"Nessuna azione trovata per {chiave}");
        }
        async Task pos(string s)
        {
            int index = Convert.ToInt32(s[0].ToString());
            lock (playerLocks[index])
            {
                Player p = players[index];
                int x = Convert.ToInt32(s.Substring(1));
                p.pos = new SFML.System.Vector2f(x, p.pos.Y);
            }
        }
    }
}
