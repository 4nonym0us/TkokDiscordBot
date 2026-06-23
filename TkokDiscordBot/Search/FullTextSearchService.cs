using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Index;
using Lucene.Net.Index.Extensions;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Entities;
using TkokDiscordBot.Extensions;

namespace TkokDiscordBot.Search;

public class FullTextSearchService : IFullTextSearchService, IDisposable
{
    private readonly IItemsRepository _itemsRepository;

    private readonly string _indexPath = Path.Combine(Environment.CurrentDirectory, "items-index");
    private const int MaxResults = 500;

    private readonly PerFieldAnalyzerWrapper _analyzer;
    private readonly MultiFieldQueryParser _parser;
    private readonly IndexWriter _writer;
    private readonly MMapDirectory _dir;
    private bool _disposed;

    public const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

    public FullTextSearchService(IItemsRepository itemsRepository)
    {
        _itemsRepository = itemsRepository;

        // Configure items index
        _dir = new MMapDirectory(new DirectoryInfo(_indexPath));

        _analyzer = new PerFieldAnalyzerWrapper(
            new EnglishAnalyzer(AppLuceneVersion),
            new Dictionary<string, Analyzer>
            {
                // Custom EdgeNGram analyzer to enable searching `orta` to find loot from `Ortakna`.
                { "source", new EdgeNGramAnalyzer(AppLuceneVersion, 4, 10) }
            });

        _parser = new MultiFieldQueryParser(AppLuceneVersion,
            ["id", "name", "slot", "type", "quality", "source", "level", "class", "description", "special"],
            _analyzer);

        var indexConfig = new IndexWriterConfig(AppLuceneVersion, _analyzer);
        indexConfig.SetOpenMode(OpenMode.CREATE);

        // Populate the index
        _writer = new IndexWriter(_dir, indexConfig);

        var items = _itemsRepository.GetAll();

        foreach (var item in items)
        {
            _writer.AddDocument(item.ToDocument());
        }

        _writer.Commit();
        _writer.Flush(triggerMerge: false, applyAllDeletes: false);
    }

    public IReadOnlyCollection<Item> Search(string queryString)
    {
        var query = _parser.Parse(queryString);

        // Re-use the writer to get real-time updates
        using var reader = _writer.GetReader(applyAllDeletes: true);

        var searcher = new IndexSearcher(reader);
        var hits = searcher.Search(query, MaxResults, Sort.RELEVANCE).ScoreDocs;

        var items = _itemsRepository.GetAll();
        var docIds = hits.Select(h => int.Parse(searcher.Doc(h.Doc).Get("id"))).ToList();
        if (!docIds.Any())
        {
            return [];
        }

        return items.Where(i => docIds.Contains(i.Id)).ToList();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _writer.Dispose();
            _analyzer.Dispose();
            _dir.Dispose();
        }

        _disposed = true;
    }
}