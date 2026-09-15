using System.Xml.Linq;
using System.Xml.XPath;
using Sonban.Common.Dto;

namespace Sonban.Api.Classes;

public static class BookHelper {
    public static XElement LoadBookWithoutNs(string bookPath) {
        XElement book;
        //book = XDocument.Parse(File.ReadAllText(bookPath), LoadOptions.PreserveWhitespace).Root;
        //book = ReadXDocumentWithInvalidCharacters(bookPath).Root;
        using (Stream file = File.OpenRead(bookPath)) {
            book = XElement.Load(file, LoadOptions.PreserveWhitespace);
        }

        XNamespace ns = "";
        foreach (var el in book.DescendantsAndSelf()) {
            el.Name = ns.GetName(el.Name.LocalName);
            var atList = el.Attributes().ToList();
            el.Attributes().Remove();
            foreach (var at in atList)
                el.Add(new XAttribute(ns.GetName(at.Name.LocalName), at.Value));
        }

        book = new XElement("book", book.Elements("description"), book.Elements("body"), book.Elements("binary"));
        return book;
    }

    //public static void SaveCover(string inputFile, string outputFile) {
    //    var book = LoadBookWithoutNs(inputFile);
    //    if (book == null) return;
    //    var coverImage = book.Descendants("coverpage").Elements("image").FirstOrDefault();
    //    if (coverImage != null) {
    //        var coverPage = (string)coverImage.Attribute("href");
    //        if (!string.IsNullOrWhiteSpace(coverPage)) {
    //            var node = book.XPathSelectElement($"descendant::binary[@id='{coverPage.Replace("#", "")}']");
    //            if (node != null) {
    //                File.WriteAllBytes(outputFile, Convert.FromBase64String(node.Value));
    //                return;
    //            }
    //        }
    //    }

    //    foreach (var binEl in book.Elements("binary")) {
    //        File.WriteAllBytes(outputFile, Convert.FromBase64String(binEl.Value));
    //        return;
    //    }
    //}

    private static string Value(IEnumerable<XElement> source, string defaultResult = null) {
        var value = source.Select(element => element.Value).FirstOrDefault();
        if (value == null || String.IsNullOrEmpty(value.Trim()))
            return defaultResult;
        return value.Trim();
    }

    public static BookDetailsDto GetBookDetails(string inputFilePath) {

        var result = new BookDetailsDto();
        var book = LoadBookWithoutNs(inputFilePath);

        result.Name = Value(book.Elements("description").Elements("title-info").Elements("book-title"), "");

        result.Authors = new List<AuthorDto>();
        var authors = book.Elements("description").Elements("title-info").Elements("author");
        foreach (var ai in authors) {
            var authorFirstname = $"{Value(ai.Elements("first-name"))} {Value(ai.Elements("middle-name"))}".Trim();
            var authorLastname = Value(ai.Elements("last-name"))?.Trim();

            if (!string.IsNullOrWhiteSpace(authorFirstname) || !string.IsNullOrWhiteSpace(authorLastname)) {
                result.Authors.Add(new AuthorDto { FirstName = authorFirstname, LastName = authorLastname });
            }
        }

        var desc = book.XPathSelectElement("descendant::annotation");
        if (desc != null) {
            result.Description = desc.Value?.Replace("\\n", "").Trim();
        }
        //else {
        //    //no annotation - get short part from body
        //    var body = book.XPathSelectElement("descendant::body");
        //    if (body != null) {
        //        result.Description = Regex.Replace(body.Value.Trim().Shorten(1024).Replace("\n", "<br/>"), @"\s+", " ")
        //          .Replace("<br/> <br/> ", "<br/>");
        //    }
        //}

        return result;
    }
}