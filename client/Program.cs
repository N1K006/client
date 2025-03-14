using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;
using System.Threading.Tasks;
using SFML.System;
using System.Threading;

class Program
{
    public const int WIDTH = 1023, HEIGHT = 683;
    public static VideoMode SchermoHome = new VideoMode(WIDTH, HEIGHT);
    public static RenderWindow FinestraHome = new RenderWindow(SchermoHome, "Schermata Home");

    // Sfondo Schermata Home
    static IntRect sfondo = new IntRect(0, 0, WIDTH, HEIGHT);
    static Texture Sfondo = new Texture(@"..\..\..\immagini\sfondo.png", sfondo);
    static Sprite SFONDO = new Sprite(Sfondo);


    static void Main()
    {


        //TcpClient client = new TcpClient("192.168.1.110", 5000);
        //NetworkStream stream = client.GetStream();

        // Ciclo principale dell'applicazione
        while (FinestraHome.IsOpen)
        {
            // Gestisci gli eventi
            FinestraHome.DispatchEvents();

            // Pulisci la finestra con un colore (opzionale)
            FinestraHome.Clear();

            // Disegna lo sfondo
            FinestraHome.Draw(SFONDO);

            // Mostra tutto ciò che è stato disegnato
            FinestraHome.Display();
        }

        /*while (true)
        {
            // Invia un messaggio al server
            string message = Console.ReadLine();
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);

            // Riceve la risposta dal server
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Risposta dal server: " + response);
        }
        client.Close();*/
    }

    static void AvviaAzione(string chiave, string parametro)
    {
        Dictionary<string, ParameterizedThreadStart> azioni = new Dictionary<string, ParameterizedThreadStart>
        {
            { "a", new ParameterizedThreadStart(a) }
        };

        if (azioni.ContainsKey(chiave))
        {
            Thread thread = new Thread(azioni[chiave]);
            thread.Start(parametro);
        }
        else
        {
            Console.WriteLine($"Nessuna azione trovata per '{chiave}'");
        }
    }

    public static void a(object b)
    {

    }
}