using System;
using System.Collections.Generic;

namespace Foruscorp.TrucksTracking.Domain.Trucks
{
    public class TransactionReport
    {
        public Guid Id { get; private set; }
        public string Card { get; private set; }
        public string Group { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public List<TransactionFill> Fills { get; private set; } = new List<TransactionFill>();
        public Dictionary<string, object> Summaries { get; private set; } = new Dictionary<string, object>();

        private TransactionReport() { } // For EF Core

        private TransactionReport(string card, string group)
        {
            Id = Guid.NewGuid();
            Card = card;
            Group = group;
            CreatedAt = DateTime.UtcNow;
        }

        public static TransactionReport CreateNew(string card, string group)
        {
            return new TransactionReport(card, group);
        }

        public void AddFill(TransactionFill fill)
        {
            Fills.Add(fill);
        }

        public void AddSummary(string key, object value)
        {
            Summaries[key] = value;
        }
    }

    public class TransactionFill
    {
        public Guid Id { get; private set; }
        public Guid TransactionReportId { get; private set; }
        public string TranDate { get; private set; }
        public string TranTime { get; private set; }
        public string Invoice { get; private set; }
        public string Unit { get; private set; }
        public string Driver { get; private set; }
        public string Odometer { get; private set; }
        public string Location { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public List<TransactionItem> Items { get; private set; } = new List<TransactionItem>();

        private TransactionFill() { } // For EF Core

        private TransactionFill(
            Guid transactionReportId,
            string tranDate,
            string tranTime,
            string invoice,
            string unit,
            string driver,
            string odometer,
            string location,
            string city,
            string state)
        {
            Id = Guid.NewGuid();
            TransactionReportId = transactionReportId;
            TranDate = tranDate;
            TranTime = tranTime;
            Invoice = invoice;
            Unit = unit;
            Driver = driver;
            Odometer = odometer;
            Location = location;
            City = city;
            State = state;
        }

        public static TransactionFill CreateNew(
            Guid transactionReportId,
            string tranDate,
            string tranTime,
            string invoice,
            string unit,
            string driver,
            string odometer,
            string location,
            string city,
            string state)
        {
            return new TransactionFill(
                transactionReportId,
                tranDate,
                tranTime,
                invoice,
                unit,
                driver,
                odometer,
                location,
                city,
                state);
        }

        public void AddItem(TransactionItem item)
        {
            Items.Add(item);
        }
    }

    public class TransactionItem
    {
        public Guid Id { get; private set; }
        public Guid TransactionFillId { get; private set; }
        public string Type { get; private set; } // e.g., "ULSD" or "DEFD"
        public double UnitPrice { get; private set; }
        public double Quantity { get; private set; }
        public double Amount { get; private set; }
        public string DB { get; private set; }
        public string Currency { get; private set; }

        private TransactionItem() { } // For EF Core

        private TransactionItem(
            Guid transactionFillId,
            string type,
            double unitPrice,
            double quantity,
            double amount,
            string db,
            string currency)
        {
            Id = Guid.NewGuid();
            TransactionFillId = transactionFillId;
            Type = type;
            UnitPrice = unitPrice;
            Quantity = quantity;
            Amount = amount;
            DB = db;
            Currency = currency;
        }

        public static TransactionItem CreateNew(
            Guid transactionFillId,
            string type,
            double unitPrice,
            double quantity,
            double amount,
            string db,
            string currency)
        {
            return new TransactionItem(
                transactionFillId,
                type,
                unitPrice,
                quantity,
                amount,
                db,
                currency);
        }
    }
}
