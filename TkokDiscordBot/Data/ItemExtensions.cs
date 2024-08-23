using Lucene.Net.Documents;
using Lucene.Net.Index;
using TkokDiscordBot.Entities;

namespace TkokDiscordBot.Data;

public static class ItemExtensions
{
    public static IndexWriter AddItemDocument(this IndexWriter writer, Item item)
    {
        writer.AddDocument(new Document
        {
            // StringField indexes but doesn't tokenize
            new StringField("name",
                item.Name,
                Field.Store.YES),
            new TextField("description",
                item.Description,
                Field.Store.YES)
        });

        return writer;
    }
}