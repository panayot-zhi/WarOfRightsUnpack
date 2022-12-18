namespace WarOfRightsUnpack.Utility
{
    public static class ConsoleHelper
    {
        public static void WriteLine()
        {
            Console.WriteLine(string.Empty);
        }

        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        public static void WriteWarning(string message)
        {
            Write(message, null, ConsoleColor.Yellow);
        }

        public static void WriteError(string message)
        {
            Write(message, null, ConsoleColor.DarkRed);
        }

        private static void Write(string message, ConsoleColor? newBackground, ConsoleColor? newForeground)
        {
            // save original colors
            var background = Console.BackgroundColor;
            var foreground = Console.ForegroundColor;

            //Console.Write(Environment.NewLine);

            if (newBackground.HasValue)
            {
                Console.BackgroundColor = newBackground.Value;
            }

            if (newForeground.HasValue)
            {
                Console.ForegroundColor = newForeground.Value;
            }

            Console.WriteLine(message);

            // restore original colors
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;
        }
    }
}
