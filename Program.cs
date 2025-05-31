namespace atp_tp_tetris
{
    internal class Program
    {
        static void Main()
        {
            double x = 0;

            while (true)
            {
                if (Console.KeyAvailable) {
                    var key = Console.ReadKey();
                    Console.WriteLine(key);
                    Console.WriteLine(x);
                }
                x++;
            }
        }
    }
}
