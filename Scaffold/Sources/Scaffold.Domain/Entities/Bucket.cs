namespace Scaffold.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using Scaffold.Domain.Exceptions;

    public class Bucket
    {
        private readonly List<Item> items;

        private int size = 5;

        public Bucket() => this.items = new List<Item>();

        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int Size
        {
            get => this.size;

            set
            {
                if (value < 0)
                {
                    throw new InvalidSizeException(
                        "Size cannot be set to a negative number.");
                }

                if (value < this.items.Count)
                {
                    throw new InvalidSizeException(
                        "Size cannot be set less than the number of items already in the bucket.");
                }

                this.size = value;
            }
        }

        public IReadOnlyCollection<Item> Items => this.items.AsReadOnly();

        public void AddItem(Item item)
        {
            if (this.items.Contains(item))
            {
                return;
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (this.IsFull())
            {
                throw new BucketFullException($"Bucket '{this.Id}' is full. Cannot add Item to Bucket.");
            }

            this.items.Add(item);
            item.Bucket = this;
        }

        public bool IsFull() => this.items.Count >= this.size;

        public void RemoveItem(Item item)
        {
            if (!this.items.Contains(item))
            {
                return;
            }

            this.items.Remove(item);
            item.Bucket = null;
        }
    }
}