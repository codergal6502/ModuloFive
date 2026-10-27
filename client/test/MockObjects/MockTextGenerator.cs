namespace MockObjects;

public class MockTextGenerator
{
    private static readonly List<string> ALL_WORDS;
    private static readonly Random RANDOM = new();

    static MockTextGenerator()
    {
        var assembly = typeof(MockTextGenerator).Assembly;
        if (null == assembly)
        {
            throw new InvalidOperationException();
        }

        using Stream? resource = assembly.GetManifestResourceStream("MockObjects.loremipsum.txt");
        if (null == resource)
        {
            throw new InvalidOperationException();
        }

        List<string> allWords = [];

        using StreamReader sr = new(resource);
        while (sr.Peek() >= 0)
        {
            if (sr.ReadLine() is string s)
            {
                allWords.Add(s);
            }
        }

        ALL_WORDS = allWords;
    }

    public static string Generate(int wordCount = 3)
    {
        if (wordCount < 1) wordCount = 3;

        return string.Join(' ', ALL_WORDS.OrderBy(s => RANDOM.Next()).Take(wordCount));
    }
}
