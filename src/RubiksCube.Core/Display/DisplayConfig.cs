namespace RubiksCube.Core.Display;

public class DisplayConfig
{
    public DisplayFormat Format { get; set; } = DisplayFormat.Unicode;
    public ColorMode ColorMode { get; set; } = ColorMode.Squares;
    public UnicodeSquares Squares { get; set; } = new();
    public AsciiLetters Letters { get; set; } = new();
}

public enum DisplayFormat
{
    ASCII,
    Unicode
}

public enum ColorMode
{
    Letters,   // W R B O G Y
    Squares    // 🔳🟥🟦🟧🟩🟨
}

public class UnicodeSquares
{
    public string White { get; set; } = "🔳";   // U+1F533 White Square Button
    public string Red { get; set; } = "🟥";     // U+1F7E5 Red Square  
    public string Blue { get; set; } = "🟦";    // U+1F7E6 Blue Square
    public string Orange { get; set; } = "🟧";  // U+1F7E7 Orange Square
    public string Green { get; set; } = "🟩";   // U+1F7E9 Green Square
    public string Yellow { get; set; } = "🟨";  // U+1F7E8 Yellow Square
}

public class AsciiLetters
{
    public string White { get; set; } = "W";
    public string Red { get; set; } = "R";
    public string Blue { get; set; } = "B";
    public string Orange { get; set; } = "O";
    public string Green { get; set; } = "G";
    public string Yellow { get; set; } = "Y";
}