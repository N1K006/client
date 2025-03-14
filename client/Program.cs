using System;
using System.Net.Sockets;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;
using System.Threading;
using client;

class Program
{
    public const int WIDTH = 1023, HEIGHT = 683;
    public static VideoMode Schermo = new VideoMode(WIDTH, HEIGHT);
    public static RenderWindow finestra = new RenderWindow(Schermo, "Finestra");

    static int fase = 1;


    static void Main()
    {
        TcpClient client = new TcpClient("127.0.0.1", 5000);
        NetworkStream stream = client.GetStream();

        while (finestra.IsOpen)
        {
            if (fase == 0)
            {
                //home
            }
            else if(fase == 1)
            {
                new Gioco(client, finestra);
            }


            //finestra.Clear();
            //finestra.Display();
            //finestra.DispatchEvents();

            //finestra.Draw(SFONDO);
        }

        //while (true)
        //{
        //    Invia un messaggio al server
        //    string message = Console.ReadLine();
        //    byte[] data = Encoding.UTF8.GetBytes(message);
        //    stream.Write(data, 0, data.Length);

        //    Riceve la risposta dal server
        //    byte[] buffer = new byte[1024];
        //    int bytesRead = stream.Read(buffer, 0, buffer.Length);
        //    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        //    Console.WriteLine("Risposta dal server: " + response);
        //}
        //client.Close();
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