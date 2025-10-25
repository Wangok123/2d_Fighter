using LatServer.Common;

namespace LatServer.Core.Common;

public class ServerStart
{
    public static void Main(string[] args)
    {
        // Initialize the server
        ServerRoot.Instance.Init();

        // Main loop
        while (true)
        {
            // Update the server
            ServerRoot.Instance.Update();
            
            Thread.Sleep(10); // Sleep for a short duration to prevent busy waiting
        }
    }
}