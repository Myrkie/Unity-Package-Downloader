using System.Text;

namespace Unity_package_downloader
{
    public class DownloadProgressUi(string taskName)
    {
        private static readonly char[] ProgressCharacters =
        [
            '⠀',
            '⡀',
            '⣀',
            '⣄',
            '⣤',
            '⣦',
            '⣶',
            '⣷',
            '⣿' 
        ];
        private const int MaxProgressCharacters = 15;
        private const string Purple = "\e[38;2;136;1;154m";
        private const string Reset = "\e[0m";
        private readonly int _progressRow = Console.CursorTop;
        private readonly int _progressColumn = Console.CursorLeft;

        public void DrawProgress(long downloadedBytes, long totalBytes)
        {
            Console.SetCursorPosition(_progressColumn, _progressRow);
            if (totalBytes <= 0)
            {
                Console.Write($"{Purple}[⣀⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀]{Reset} {taskName}");
                return;
            }

            double progress = Math.Clamp((double)downloadedBytes / totalBytes, 0.0, 1.0);
            double position = progress * MaxProgressCharacters;
            int fullCharacters = (int)Math.Floor(position);
            double partial = position - fullCharacters;
            var builder = new StringBuilder(MaxProgressCharacters);
            for (int i = 0; i < fullCharacters; i++)
            {
                builder.Append('⣿');
            }

            if (fullCharacters < MaxProgressCharacters)
            {
                int partialIndex = (int)Math.Floor(partial * (ProgressCharacters.Length - 1));
                partialIndex = Math.Clamp(partialIndex, 0, ProgressCharacters.Length - 1);
                builder.Append(ProgressCharacters[partialIndex]);
            }

            while (builder.Length < MaxProgressCharacters)
            {
                builder.Append('⠀');
            }

            Console.Write($"{Purple}[{builder}]{Reset} {taskName}");
        }

        public void Complete()
        {
            Console.SetCursorPosition(_progressColumn + MaxProgressCharacters + 2, _progressRow);
            Console.Write($" Completed {taskName}");
            Console.WriteLine();
        }
    }
}