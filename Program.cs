using Humanizer;
using Wordle;

const int WordLength = 5;
var remainingWords   = WordListReader.ReadLines().ToArray();
var console          = new Wordle.Console();
var solution         = Enumerable.Repeat(' ', WordLength).ToArray();
var numAttempts      = 0;
var rand             = new Random(20241229);
var solvedFeedback   = new string('c', WordLength);

while (true){
    numAttempts++;
    
    var suggestion = remainingWords.Length == 1 
        ? remainingWords[0]
        : remainingWords
            .GroupBy(word => word.Distinct().Count())
            .OrderByDescending(g => g.Key)
            .First()
            .RandomElement(rand)!;
        
    console.WriteLine($"Suggestion $magenta({numAttempts}): $green({suggestion.ToUpper()}) - out of $magenta({"possibility".ToQuantity(remainingWords.Length)})");
    
    string? feedback = solvedFeedback;
    while (remainingWords.Length > 1){
        console.Write("Feedback - $yellow([C])orrect $yellow([M])isplaced $yellow([N])o more occurrences? ");
        feedback = console.ReadLine();
        if (feedback == null){
            console.WriteLine("Null feedback; terminating.");
            Environment.Exit(0);
        }
        feedback = feedback.ToLower();
        if (feedback.Length != WordLength){
            console.WriteLine($"invalid feedback; expected {"character".ToQuantity(WordLength)}");
            continue;
        }
        if (!feedback.All(Feedback.IsValid)){
            console.WriteLine("invalid feedback; use only letters $yellow(C), $yellow(M) or $yellow(N)");
            continue;
        }
        break;
    }

    if (feedback == solvedFeedback){
        console.WriteLine($"Solved on the $green({numAttempts.Ordinalize()}) attempt");
        Environment.Exit(0);
    }

    var operations = feedback
        .Zip(suggestion)
        .Select((x, i) => (f:x.First, c:x.Second, i))
        .OrderBy(x => x.f); // ensures processing order 'c' -> 'm' -> 'n'

    foreach (var (f, c, i) in operations){
        switch (f) {
        case Feedback.Correct:
            if (solution[i] == c) continue;
            solution[i] = c;
            remainingWords = remainingWords.Where(w => w[i] == c).ToArray();
            break;
        case Feedback.Misplaced:
            var unsolvedIndexes = Enumerable.Range(0, WordLength).Where(j => j!=i && solution[j] == ' ');
            remainingWords = remainingWords.Where(w => w[i] != c && unsolvedIndexes.Any(u => w[u] == c)).ToArray();
            break;
        case Feedback.NoMoreOccurrences:
            for (var j=0; j<WordLength; j++){
                if (solution[j] == ' '){
                    remainingWords = remainingWords.Where(w => w[j] != c).ToArray();
                }
            }
            break;
        }
    }

    if (remainingWords.Length == 0){
        console.WriteLine("$red(No remaining words, check input)");
        Environment.Exit(1);
    }
}
