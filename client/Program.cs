using System;
using System.Net.Sockets;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;
using SFML.System;
using static SFML.Window.Keyboard;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace client
{
    class Program
    {
        public static RenderWindow finestra;

        static int fase = 1;

        static void Main()
        {
            const int WIDTH = 1023, HEIGHT = 683;
            VideoMode Schermo = new VideoMode(WIDTH, HEIGHT);

            finestra = new RenderWindow(Schermo, "Finestra");
            TcpClient client = new TcpClient("127.0.0.1", 5000);
            NetworkStream stream = client.GetStream();

            while (finestra.IsOpen)
            {
                if (fase == 0)
                {
                    //home
                }
                else if (fase == 1)
                {
                    gioco(client, finestra);
                }
            }
        }

        #region timers
        static Timer inviaKey = new Timer(90);
        #endregion

        #region locks
        static private object lockStream = new object();
        static private object[] playerLocks = new object[2] { new object(), new object() };
        #endregion

        static NetworkStream stream;

        static Player[] players = new Player[2];
        static Ball ball;

        static Dictionary<string, Func<string, Task>> actions = new Dictionary<string, Func<string, Task>>();

        static bool left = false, right = false;
        static Key k;

        static int i;

        static void gioco(TcpClient client, RenderWindow finestra)
        {
            finestra.SetFramerateLimit(90);
            actions["pos"] = pos;

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
                System.Threading.Thread.Sleep(10);
            }
        }

        static void Read(out byte[] message)
        {
            byte[] lunghezzaByte = new byte[4]; // Inizializza array
            ReadInternal(out lunghezzaByte, 4);

            if (lunghezzaByte == null || lunghezzaByte.Length < 4) // Controlla errore di lettura
            {
                message = null;
                return;
            }

            int lunghezza = BitConverter.ToInt32(lunghezzaByte, 0); // Converte i byte in int

            Console.WriteLine($"Lunghezza: {lunghezza}");

            if (lunghezza <= 0) // Evita array di lunghezza negativa o zero
            {
                message = null;
                return;
            }

            ReadInternal(out message, lunghezza);

            Console.WriteLine($"Messaggio ricevuto: {BitConverter.ToString(message)}"); // Debug

            void ReadInternal(out byte[] data, int lunghezza)
            {
                data = new byte[lunghezza]; // Ora l'array è inizializzato

                int letti = 0;
                while (letti < lunghezza)
                {
                    int read;
                    lock (lockStream)
                        read = stream.Read(data, letti, 4);
                    if (read == 0) // Disconnessione
                    {
                        data = null;
                        return;
                    }
                    letti += read;
                }
            }
        }


        static async Task Read()
        {
            try
            {
                byte[] messageByte;
                {
                    Read(out messageByte);

                    if (messageByte != null)
                        i = BitConverter.ToInt32(messageByte, 0);
                }

                inviaKey.Start();
                inviaKey.Elapsed += InviaKey_Elapsed;
                inviaKey.AutoReset = true;

                while (true)
                {
                    Read(out messageByte);

                    if (messageByte != null)
                    {
                        string message = Encoding.UTF8.GetString(messageByte);
                        Task.Run(() => AvviaAzione(message.Substring(0, 3), message[3..]));
                    }

                    await Task.Delay(40);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore: " + ex.Message);
            }
        }
        static void OnKeyPressed(object sender, KeyEventArgs e)
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
        static void OnKeyReleased(object sender, KeyEventArgs e)
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
        private static void InviaKey_Elapsed(object sender, ElapsedEventArgs e)
        {
            string message = $"mov{i}{k}";
            Write(message);
        }
        static void Write(string messageString)
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
        static async Task AvviaAzione(string chiave, string parametro)
        {
            if (actions.TryGetValue(chiave, out var action))
                action(parametro);
            else
                Console.WriteLine($"Nessuna azione trovata per {chiave}");
        }
        static async Task pos(string s)
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