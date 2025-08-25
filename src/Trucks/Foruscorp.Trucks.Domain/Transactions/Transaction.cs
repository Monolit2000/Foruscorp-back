using Foruscorp.BuildingBlocks.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Domain.Transactions
{
    public class Transaction : Entity, IAggregateRoot
    {
        public Guid Id { get; private set; }
        public string Card { get; private set; }
        public string Group { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsProcessed { get; private set; } = false;
        public List<Fill> Fills { get; private set; }

        [NotMapped]
        public Dictionary<string, object> Summaries { get; private set; }

        private Transaction()
        {
            Fills = new List<Fill>();
            Summaries = new Dictionary<string, object>();
        }

        public Transaction(string card, string group)
        {
            Id = Guid.NewGuid();
            Card = card;
            Group = group;
            CreatedAt = DateTime.UtcNow;
            Fills = new List<Fill>();
            Summaries = new Dictionary<string, object>();
        }

        public void AddFill(Fill fill)
        {
            if (fill == null)
                throw new ArgumentNullException(nameof(fill));

            Fills.Add(fill);
        }

        public void AddSummary(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Summary key cannot be empty", nameof(key));

            Summaries[key] = value;
        }

        public static Transaction CreateNew(string card, string group)
        {
            if (string.IsNullOrWhiteSpace(card))
                throw new ArgumentException("Card cannot be empty", nameof(card));

            return new Transaction(card, group);
        }
    }

    public class Fill : Entity
    {
        public Guid Id { get; private set; }
        public string TranDate { get; private set; }
        public string TranTime { get; private set; }
        public string Invoice { get; private set; }
        public string Unit { get; private set; }
        public string Driver { get; private set; }
        public string Odometer { get; private set; }
        public string Location { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public virtual List<Item> Items { get; private set; }

        private Fill()
        {
            Items = new List<Item>();
        }

        public Fill(string tranDate, string tranTime, string invoice, string unit, string driver,
                   string odometer, string location, string city, string state)
        {
            Id = Guid.NewGuid();
            TranDate = tranDate;
            TranTime = tranTime;
            Invoice = invoice;
            Unit = unit;
            Driver = driver;
            Odometer = odometer;
            Location = location;
            City = city;
            State = state;
            CreatedAt = DateTime.UtcNow;
            Items = new List<Item>();
        }


        public static Fill CreateNew(string tranDate, string tranTime, string invoice, string unit, 
            string driver, string odometer, string location, string city, string state)
        {
            return new Fill(tranDate, tranTime, invoice, unit, driver, odometer, location, city, state);
        }


        public void AddItem(Item item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            Items.Add(item);
        }

        public string GetUnitNumber()
        {
            if (string.IsNullOrWhiteSpace(Unit))
                return string.Empty;

            var match = Regex.Match(Unit, @"\d+");
            return match.Success ? match.Value : string.Empty;
        }

        public DateTime GetTransactionDateTime()
        {
            var dateTimeStr = $"{TranDate} {TranTime}";

            if (!DateTime.TryParse(dateTimeStr, out var result))
            {
                throw new FormatException($"Cannot parse TranDate and TranTime to DateTime: '{dateTimeStr}'");
            }

            // Задаём, что это время в часовом поясе США, например Eastern Time
            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            // Конвертируем в UTC
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(result, easternZone);

            return result;
        }


    }

    public class Item : Entity
    {
        public Guid Id { get; private set; }
        public string Type { get; private set; } // e.g., "ULSD" or "DEFD"
        public double UnitPrice { get; private set; }
        public double Quantity { get; private set; }
        public double Amount { get; private set; }
        public string DB { get; private set; }
        public string Currency { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Item() { }

        public Item(string type, double unitPrice, double quantity, double amount, string db, string currency)
        {
            Id = Guid.NewGuid();
            Type = type;
            UnitPrice = unitPrice;
            Quantity = quantity;
            Amount = amount;
            DB = db;
            Currency = currency;
            CreatedAt = DateTime.UtcNow;
        }

        public static Item CreateNew(string type, double unitPrice, double quantity, double amount, string db, string currency)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Item type cannot be empty", nameof(type));

            return new Item(type, unitPrice, quantity, amount, db, currency);
        }
    }
}
