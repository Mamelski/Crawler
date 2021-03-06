﻿namespace Crawler.DiscOperations
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Crawler.Model;

    /// <summary>
    /// The document saver.
    /// </summary>
    public class Disc
    {
        /// <summary>
        /// The domain.
        /// </summary>
        private readonly Uri domain;

        /// <summary>
        /// The domain folder.
        /// </summary>
        private readonly string domainFolder;

        /// <summary>
        /// The windows illegal chars.
        /// </summary>
        private readonly char[] windowsIllegalChars = @"\/:".ToCharArray();

        /// <summary>
        /// Initializes a new instance of the <see cref="Disc"/> class.
        /// </summary>
        /// <param name="domain">
        /// The domain.
        /// </param>
        public Disc(Uri domain)
        {
            this.domain = domain;
            var illegalChars = Path.GetInvalidPathChars();
            this.domainFolder = new string(domain.ToString().Where(c => !illegalChars.Contains(c) && !this.windowsIllegalChars.Contains(c)).ToArray());
        }

        /// <summary>
        /// The save document.
        /// </summary>
        /// <param name="uri">
        /// The uri.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        public async Task SaveDocument(Uri uri, string content)
        {
            var filename = this.domain.MakeRelativeUri(uri).ToString();

            var index = filename.LastIndexOfAny(@"\/".ToCharArray());

            var lastPart = (index >= 0) ? filename.Substring(index) : filename;

            if (!lastPart.Contains('.'))
            {
                if (filename.Length > 0 && filename.Last() != '/')
                {
                    filename += '/';
                }

                filename += "index.html";
            }

            filename = Path.Combine(this.domainFolder, filename);

            var file = new FileInfo(filename);
            file.Directory.Create();

            File.WriteAllText(file.FullName, content);
        }

        public void SaveReport(Graph graph)
        {
            var illegalChars = Path.GetInvalidPathChars();
            var filename = new string(this.domain.ToString().Where(c => !illegalChars.Contains(c) && !this.windowsIllegalChars.Contains(c)).ToArray());

            this.SaveReport(graph, filename);
            this.SavePaths(graph, filename);
            this.SaveIn(graph,filename);
            this.SaveOut(graph, filename);
        }

        private void SaveIn(Graph graph, string filename)
        {
            using (var sw = new StreamWriter($"{filename}-int.txt"))
            {
                foreach (var node in graph.Neighborhood)
                {
                    sw.WriteLine($"{node.Value.InDegree}");
                }
            }
        }

        private void SaveOut(Graph graph, string filename)
        {
            using (var sw = new StreamWriter($"{filename}-out.txt"))
            {
                foreach (var node in graph.Neighborhood)
                {
                    sw.WriteLine($"{node.Value.OutDegree}");
                }
            }
        }

        private void SaveReport(Graph graph, string filename)
        {
            using (var reportSw = new StreamWriter($"{filename}-report.txt"))
            {
                // Graph info
                reportSw.WriteLine("GRAF");
                reportSw.WriteLine($"Liczba wierzchołków: {graph.NumberOfNodes}");
                reportSw.WriteLine($"Liczba krawędzi: {graph.NumberOfEdges}");
                reportSw.WriteLine($"Średnia odległość: {graph.GetAverageDistance()}");
                reportSw.WriteLine($"Średnica grafu: {graph.GetDiameter()}");
                reportSw.WriteLine($"Promień grafu: {graph.GetRadius()}");
                reportSw.WriteLine($"Średni stopień wejściowy: {graph.AverageInDegree}");
                reportSw.WriteLine($"Średni stopień wyjściowy: {graph.AverageOutDegree}");
                reportSw.WriteLine($"Średni PageRank: {graph.AveragePageRank}");
                reportSw.WriteLine($"Iteracje PageRank: {graph.Iterations}");
                reportSw.WriteLine($"Czas analizy: {graph.AnalyzeTime}");
                reportSw.WriteLine($"Czas Floyda: {graph.FloydTime}");

                // TODO pagerank, invalid
                reportSw.WriteLine("\n");

                // Node info
                foreach (var node in graph.Neighborhood)
                {
                    reportSw.WriteLine($"Wierzchołek: {node.Key}");
                    reportSw.WriteLine($"\tInDegree: {node.Value.InDegree}");
                    reportSw.WriteLine($"\tOutDegree: {node.Value.OutDegree}");
                    reportSw.WriteLine($"\tPageRank: {node.Value.PageRank}");
                    reportSw.WriteLine("\tSąsiedzi:");
                    foreach (var subnode in node.Value.Neighbours)
                    {
                        reportSw.WriteLine("\t\t" + subnode.Uri);
                    }
                }
            }
        }

        private void SavePaths(Graph graph, string filename)
        {
            using (var sw = new StreamWriter($"{filename}-paths.txt"))
            {
                // Graph info
                sw.WriteLine("Najkrótsze ścieżki:");

                // Node info
                foreach (var node in graph.Neighborhood)
                {
                    sw.WriteLine($"Wierzchołek: {node.Key}");
                    foreach (var neighbour in node.Value.ShortestPaths)
                    {
                        sw.WriteLine($"\tŚcieżka do {neighbour.Key}");
                        foreach (var step in neighbour.Value)
                        {
                            sw.WriteLine($"\t\t{step.Uri}");
                        }
                    }
                }
            }
        }
    }
}
