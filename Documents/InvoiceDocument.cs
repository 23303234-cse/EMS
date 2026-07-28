using EMS.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EMS.Documents
{
    public class InvoiceDocument : IDocument
    {
        private readonly Order _order;

        public InvoiceDocument(Order order)
        {
            _order = order;
        }

        public DocumentMetadata GetMetadata()
        {
            return DocumentMetadata.Default;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header().Element(ComposeHeader);

                page.Content().Element(ComposeContent);

                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("EMS Shopping System")
                    .FontSize(24)
                    .Bold();

                column.Item().AlignCenter().Text("INVOICE")
                    .FontSize(18)
                    .SemiBold();

                column.Item().PaddingTop(20);

                column.Item().Text($"Invoice No : INV-{_order.Id}");
                column.Item().Text($"Customer : {_order.UserEmail}");
                column.Item().Text($"Order Date : {_order.OrderDate:dd MMM yyyy}");
                column.Item().Text($"Payment Method : {_order.PaymentMethod}");
                column.Item().Text($"Payment Status : {_order.PaymentStatus}");
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingTop(20).Column(column =>
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Border(1).Padding(5).Text("Product").Bold();

                        header.Cell().Border(1).Padding(5)
                            .AlignCenter()
                            .Text("Qty").Bold();

                        header.Cell().Border(1).Padding(5)
                            .AlignRight()
                            .Text("Price").Bold();

                        header.Cell().Border(1).Padding(5)
                            .AlignRight()
                            .Text("Total").Bold();
                    });

                    foreach (var item in _order.Items)
                    {
                        table.Cell().Border(1).Padding(5)
                            .Text(item.ProductName);

                        table.Cell().Border(1).Padding(5)
                            .AlignCenter()
                            .Text(item.Quantity.ToString());

                        table.Cell().Border(1).Padding(5)
                            .AlignRight()
                            .Text($"৳ {item.Price:N2}");

                        table.Cell().Border(1).Padding(5)
                            .AlignRight()
                            .Text($"৳ {item.Total:N2}");
                    }
                });

                column.Item().PaddingTop(20);

                column.Item().AlignRight().Text(text =>
                {
                    text.Span("Grand Total: ")
                        .Bold()
                        .FontSize(14);

                    text.Span($"৳ {_order.TotalAmount:N2}")
                        .Bold()
                        .FontSize(16)
                        .FontColor(Colors.Green.Darken2);
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().LineHorizontal(1);

                column.Item().PaddingTop(10)
                    .AlignCenter()
                    .Text("Thank you for shopping with EMS Shopping System.")
                    .FontSize(12);

                column.Item()
                    .AlignCenter()
                    .Text("This is a computer generated invoice.")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);

                column.Item()
                    .AlignCenter()
                    .Text("support@ems.com")
                    .FontSize(10);

                column.Item()
                    .AlignCenter()
                    .Text("www.emsshopping.com")
                    .FontSize(10);
            });
        }
    }
}