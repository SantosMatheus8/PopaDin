using PopaDin.ExportService.Documents;
using PopaDin.ExportService.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PopaDin.ExportService.Services;

public class PdfGeneratorService(ILogger<PdfGeneratorService> logger) : IPdfGeneratorService
{
    public byte[] GenerateRecordsReport(List<RecordDocument> records, DateTime startDate, DateTime endDate)
    {
        logger.LogInformation("Gerando PDF com {Count} Records", records.Count);

        var totalDeposits = records.Where(r => r.Operation == 1).Sum(r => r.Value);
        var totalOutflows = records.Where(r => r.Operation == 0).Sum(r => r.Value);
        var balance = totalDeposits - totalOutflows;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(40);
                page.MarginVertical(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(header => ComposeHeader(header, startDate, endDate));
                page.Content().Element(content => ComposeContent(content, records, totalDeposits, totalOutflows, balance));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, DateTime startDate, DateTime endDate)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text("PopaDin - Relatório de entradas e saidas")
                .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);

            column.Item().AlignCenter().Text($"Período: {startDate:dd/MM/yyyy} a {endDate:dd/MM/yyyy}")
                .FontSize(12).FontColor(Colors.Grey.Darken1);

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    }

    private static void ComposeContent(IContainer container, List<RecordDocument> records,
        decimal totalDeposits, decimal totalOutflows, decimal balance)
    {
        container.Column(column =>
        {
            column.Spacing(10);

            // Resumo
            column.Item().Element(c => ComposeSummary(c, totalDeposits, totalOutflows, balance));

            // Tabela de Records
            column.Item().Element(c => ComposeTable(c, records));
        });
    }

    private static void ComposeSummary(IContainer container, decimal totalDeposits, decimal totalOutflows, decimal balance)
    {
        container.Background(Colors.Grey.Lighten4).Padding(15).Column(column =>
        {
            column.Item().Text("Resumo").FontSize(14).Bold();
            column.Item().PaddingTop(5);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Total de Receitas").FontSize(10).FontColor(Colors.Grey.Darken1);
                    c.Item().Text($"R$ {totalDeposits:N2}").FontSize(13).Bold().FontColor(Colors.Green.Darken2);
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Total de Despesas").FontSize(10).FontColor(Colors.Grey.Darken1);
                    c.Item().Text($"R$ {totalOutflows:N2}").FontSize(13).Bold().FontColor(Colors.Red.Darken2);
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Saldo").FontSize(10).FontColor(Colors.Grey.Darken1);
                    var balanceColor = balance >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;
                    c.Item().Text($"R$ {balance:N2}").FontSize(13).Bold().FontColor(balanceColor);
                });
            });
        });
    }

    private static void ComposeTable(IContainer container, List<RecordDocument> records)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(80);  // Data
                columns.RelativeColumn(2);   // Valor
                columns.RelativeColumn(2);   // Tipo
                columns.RelativeColumn(2);   // Frequência
                columns.RelativeColumn(3);   // Tags
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                    .Text("Data").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                    .Text("Valor").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                    .Text("Tipo").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                    .Text("Frequência").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                    .Text("Tags").FontColor(Colors.White).Bold();
            });

            // Rows
            foreach (var record in records)
            {
                var bgColor = records.IndexOf(record) % 2 == 0
                    ? Colors.White
                    : Colors.Grey.Lighten4;

                var operationType = record.Operation == 1 ? "Receita" : "Despesa";
                var valueColor = record.Operation == 1 ? Colors.Green.Darken2 : Colors.Red.Darken2;
                var frequency = GetFrequencyName(record.Frequency);
                var tags = string.Join(", ", record.Tags.Select(t => t.Name));

                var displayDate = record.ReferenceDate ?? record.CreatedAt;
                table.Cell().Background(bgColor).Padding(5)
                    .Text(displayDate.ToString("dd/MM/yyyy"));
                table.Cell().Background(bgColor).Padding(5)
                    .Text($"R$ {record.Value:N2}").FontColor(valueColor);
                table.Cell().Background(bgColor).Padding(5)
                    .Text(operationType);
                table.Cell().Background(bgColor).Padding(5)
                    .Text(frequency);
                table.Cell().Background(bgColor).Padding(5)
                    .Text(string.IsNullOrEmpty(tags) ? "-" : tags);
            }
        });
    }

    private static string GetFrequencyName(int frequency) => frequency switch
    {
        0 => "Mensal",
        1 => "Bimestral",
        2 => "Trimestral",
        3 => "Semestral",
        4 => "Anual",
        _ => "-"
    };
}
