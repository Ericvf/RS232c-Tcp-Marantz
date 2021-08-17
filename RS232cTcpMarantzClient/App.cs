using System;
using System.Threading.Tasks;

namespace RS232cTcpMarantz
{
    public class App
    {
        private readonly IRS232cTcpMarantzClient rs232cClient;

        public App(IRS232cTcpMarantzClient rs232CTcpClient)
        {
            this.rs232cClient = rs232CTcpClient;
        }

        public async Task RunAsync(string[] args)
        {
            Console.WriteLine("RS232cTcpMarantz started");

            if (args.Length != 2)
            {
                Console.WriteLine("Please provide a hostname/ipaddress and port.");
                Console.WriteLine("e.g. `RS232cTcpMarantz.exe 192.168.1.3 23`"); // My marantz device uses telnet port 23
                Environment.Exit(1);
            }

            var hostname = args[0];
            var port = Convert.ToInt32(args[1]);
            string result;

            while (true)
            {
                // Start the connection and login
                await rs232cClient.Start(hostname, port);

                // Execute some default commands
                Console.WriteLine("Checking power: ");
                result = await rs232cClient.Get("PW?");
                Console.WriteLine(result);
                Console.WriteLine();

                Console.WriteLine("Turn ZONE2 on");
                result = await rs232cClient.Get("Z2ON");

                Console.WriteLine("Turn ZONE2 to GAME");
                result = await rs232cClient.Get("Z2GAME");

                Console.WriteLine("Turn on receiver");
                result = await rs232cClient.Get("PWON");

                Console.WriteLine("Set Main zone to Bluray");
                result = await rs232cClient.Get("SIBD");

                Console.WriteLine("Paused. Press space to turn off devices");
                Console.ReadLine();

                Console.WriteLine("Set Main zone to TV");
                result = await rs232cClient.Get("SISAT/CBL");

                Console.WriteLine("Turn ZONE2 to TV");
                result = await rs232cClient.Get("SISAT/CBL");

                Console.WriteLine("Turn ZONE2 off");
                result = await rs232cClient.Get("Z2OFF");

                Console.WriteLine("Turn off receiver");
                result = await rs232cClient.Get("PWSTANDBY");

                // REPL
                while (rs232cClient.IsConnected())
                {
                    Console.Write("> ");

                    // Read
                    var command = Console.ReadLine();
                    if (command == "!")
                    {
                        break;
                    }
                    else if (!string.IsNullOrEmpty(command))
                    {
                        // Evaluate
                        var response = await rs232cClient.Get(command!);

                        // Print
                        Console.WriteLine(response);
                    }
                }

                await rs232cClient.Stop();
            }
        }
    }
}