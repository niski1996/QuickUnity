using System.Diagnostics;

namespace QuickUnity.Services;

public class VideoProcessor
{
    public async Task ExtractFrame(string videoFilePath, string outputThumbnailPath, int second)
    {
        if (!File.Exists(videoFilePath))
            throw new ApplicationException("Video file does not exist. Please provide a valid path.");

        var ffmpegPath = "ffmpeg"; // Ensure ffmpeg is in PATH
        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments =
                $"-i \"{videoFilePath}\" -ss {second} -vframes 1 \"{outputThumbnailPath}\"", 
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new ApplicationException($"Error extracting frame: {error}");
        }
    }

    public int GetLengthInSeconds(string videoFilePath)
    {
        // Ścieżka do FFmpeg - upewnij się, że poprawnie wskazuje plik wykonywalny ffmpeg
        var ffmpegPath = "ffmpeg";
        
        var arguments = $"-i \"{videoFilePath}\"";
        
        var process = new Process();
        process.StartInfo.FileName = ffmpegPath;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.RedirectStandardError = true; // Wyjście FFmpeg jest na stderr, nie stdout
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();


        var output = process.StandardError.ReadToEnd();

        process.WaitForExit();
        
        var durationStr = "Duration: ";
        var durationIndex = output.IndexOf(durationStr);
        if (durationIndex == -1)
        {
            throw new Exception("Nie można odczytać długości wideo.");
        }

        var time = output.Substring(durationIndex + durationStr.Length, 11); // "hh:mm:ss.xx"
        var duration = TimeSpan.Parse(time.Substring(0, 8)); 

        return (int)duration.TotalSeconds;
    }
}