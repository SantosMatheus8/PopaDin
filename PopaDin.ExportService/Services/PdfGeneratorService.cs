using PopaDin.ExportService.Documents;
using PopaDin.ExportService.Interfaces;
using PopaDin.ExportService.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PopaDin.ExportService.Services;

public class PdfGeneratorService(ILogger<PdfGeneratorService> logger) : IPdfGeneratorService
{
    private const int OperationDeposit = 1;
    private const int OperationOutflow = 0;

    public byte[] GenerateRecordsReport(List<RecordDocument> records, DateTime startDate, DateTime endDate)
    {
        logger.LogInformation("Gerando PDF com {Count} Records", records.Count);

        var totalDeposits = records.Where(r => r.Operation == OperationDeposit).Sum(r => r.Value);
        var totalOutflows = records.Where(r => r.Operation == OperationOutflow).Sum(r => r.Value);
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

    public Stream GenerateRecordsReportStream(List<RecordDocument> records, DateTime startDate, DateTime endDate)
    {
        logger.LogInformation("Gerando PDF stream com {Count} Records", records.Count);

        var totalDeposits = records.Where(r => r.Operation == OperationDeposit).Sum(r => r.Value);
        var totalOutflows = records.Where(r => r.Operation == OperationOutflow).Sum(r => r.Value);
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

        var stream = new MemoryStream();
        document.GeneratePdf(stream);
        stream.Position = 0;
        return stream;
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
            var rowIndex = 0;
            foreach (var record in records)
            {
                var bgColor = rowIndex % 2 == 0
                    ? Colors.White
                    : Colors.Grey.Lighten4;
                rowIndex++;

                var operationType = record.Operation == OperationDeposit ? "Receita" : "Despesa";
                var valueColor = record.Operation == OperationDeposit ? Colors.Green.Darken2 : Colors.Red.Darken2;
                var frequency = FrequencyType.GetDisplayName(record.Frequency);
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
}
