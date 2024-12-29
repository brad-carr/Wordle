namespace Wordle;

public class Console{
    static readonly Dictionary<string, ConsoleColor> ColorMap = Enum.GetNames<ConsoleColor>().Zip(Enum.GetValues<ConsoleColor>()).ToDictionary(z => z.First.ToLower(), z => z.Second);

    public Console(){
        if (!System.Console.IsOutputRedirected){
            System.Console.Clear();
        }
        ResetColors();
    }
    
    public string? ReadLine(){
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        var input = System.Console.ReadLine();
        ResetColors();
        return input;
    }

    public void WriteLine(string text){
        Write(text);
        System.Console.WriteLine();
    }

    public void Write(string text){
        ResetColors();
        var buf = new System.Text.StringBuilder();
        foreach (var c in text){
            if (c == '$'){
                System.Console.Write(buf.ToString());
                buf.Clear();
            }
            else if (c == '('){
                System.Console.ForegroundColor = ColorMap[buf.ToString()];
                buf.Clear();
            }
            else if (c == ')'){
                System.Console.Write(buf.ToString());
                buf.Clear();
                ResetColors();
            }
            else {
                buf.Append(c);
            }
        }
        System.Console.Write(buf);
    }

    public void ResetColors(){
        System.Console.ForegroundColor = ConsoleColor.Gray;
    }
}

