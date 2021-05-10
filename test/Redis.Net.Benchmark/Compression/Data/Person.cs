using System;

namespace Redis.Net.Benchmark.Compression.Data
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public int Age { get; set; }
        public string About { get; set; }
    }
}