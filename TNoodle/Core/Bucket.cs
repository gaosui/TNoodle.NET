using System;
using System.Collections.Generic;

namespace TNoodle.Core
{
    public class Bucket<H> : IComparable<Bucket<H>>
    {
        private LinkedList<H> contents;
        private int value;

        public Bucket(int value)
        {
            this.value = value;
            this.contents = new LinkedList<H>();
        }

        public int getValue()
        {
            return this.value;
        }

        public H pop()
        {
            var last = contents.Last.Value;
            contents.RemoveLast();
            return last;
        }

        public void push(H element)
        {
            contents.AddLast(element);
        }

        public bool isEmpty()
        {
            return contents.Count == 0;
        }

        public override string ToString()
        {
            return "#: " + value + ": " + contents.ToString();
        }

        public int CompareTo(Bucket<H> other)
        {
            return this.value - other.value;
        }

        public override int GetHashCode()
        {
            return this.value;
        }

        public override bool Equals(object o)
        {
            Bucket<H> other = (Bucket<H>)o;
            return this.value == other.value;
        }
    }
}
