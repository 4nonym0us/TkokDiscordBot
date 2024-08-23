using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace TkokDiscordBot.Search;

public class EdgeNGramAnalyzer : Analyzer
{
    private readonly LuceneVersion _version;
    private readonly int _minGram;
    private readonly int _maxGram;

    public EdgeNGramAnalyzer(LuceneVersion version, int minGram = 2, int maxGram = 8)
    {
        _version = version;
        _minGram = minGram;
        _maxGram = maxGram;
    }

    protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
    {
        // Splits words at punctuation characters, removing punctuation.
        // Splits words at hyphens, unless there's a number in the token...
        var tokenizer = new StandardTokenizer(_version, reader);

        TokenStream filter = new StandardFilter(_version, tokenizer);

        // Normalizes token text to lower case.
        filter = new LowerCaseFilter(_version, filter);

        // Removes stop words from a token stream.
        filter = new StopFilter(_version, filter, StopAnalyzer.ENGLISH_STOP_WORDS_SET);

        filter = new EdgeNGramTokenFilter(_version, filter, _minGram, _maxGram);

        return new TokenStreamComponents(tokenizer, filter);
    }
}