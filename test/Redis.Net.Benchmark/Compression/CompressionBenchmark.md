# Benchmark Results 
``` ini

BenchmarkDotNet=v0.12.1, OS=macOS 11.3 (20E232) [Darwin 20.4.0]
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.201
  [Host] : .NET Core 3.1.13 (CoreCLR 4.700.21.11102, CoreFX 4.700.21.11602), X64 RyuJIT DEBUG


```
## Local Redis

|                      Method |     Mean |    Error |   StdDev |     Gen 0 |     Gen 1 |    Gen 2 | Allocated |
|---------------------------- |---------:|---------:|---------:|----------:|----------:|---------:|----------:|
|    WithCompressionBenchmark | 36.86 ms | 0.654 ms | 0.612 ms | 1142.8571 | 1000.0000 | 928.5714 |   4.83 MB |
| WithoutCompressionBenchmark | 24.96 ms | 0.490 ms | 0.545 ms |  375.0000 |  218.7500 | 218.7500 |   2.25 MB |


## Remote Redis
|                      Method |       Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------- |-----------:|---------:|---------:|------:|------:|------:|----------:|
|    WithCompressionBenchmark |   668.2 ms | 13.34 ms | 27.24 ms |     - |     - |     - |   4.88 MB |
| WithoutCompressionBenchmark | 1,387.1 ms | 26.92 ms | 37.74 ms |     - |     - |     - |   2.39 MB |


