# Local upgrade validation

Validated on 2026-11-15 (America/Sao_Paulo), using macOS ARM64, .NET SDK 10.0.404, and runtime 10.0.15. The SDK was installed in `/private/tmp/evolve-dotnet` for this validation; add that directory to `PATH` to reuse it during this session, or install the SDK normally using the README link. This temporary installation does not configure a system-wide SDK.

## Commands and results

Commands below use `dotnet` for the isolated SDK executable:

```sh
dotnet restore Evolve.NET.sln --locked-mode
dotnet build Evolve.NET.sln -c Release --no-restore --disable-build-servers -m:1
dotnet test --solution Evolve.NET.sln -c Release --no-build --no-restore
dotnet run --project Evolve.NET.Samples/Evolve.NET.Sample.Math -c Release --no-build -- 10 200 /private/tmp/evolve-math.tsv
dotnet run --project Evolve.NET.Samples/Evolve.NET.Sample.LevelGenerator -c Release --no-build -- 10 300 /private/tmp/evolve-level.tsv
dotnet list Evolve.NET.sln package --vulnerable --include-transitive
```

- Locked restore succeeded. Release build succeeded with zero warnings and errors.
- All 26 regression test cases passed, with zero skips.
- Math completed generation 200, reached fitness `4294836225`, and wrote 201 TSV rows. Initial best fitness was `4219411849`.
- LevelGenerator completed generation 300, reached fitness `150`, and wrote 301 TSV rows. Initial best fitness was `37`.
- Both complete seed-10 runs were repeated; their TSV output was byte-for-byte identical. Best fitness never decreased across their rows.
- Both examples accepted zero generations and wrote exactly one TSV row. Negative generation arguments returned exit code 1 with usage text.
- The complete C# snippet in the README was extracted into an ignored temporary project, compiled, and executed. It reached a chromosome of sixteen ones with fitness `16`.
- NuGet reported no known vulnerable packages, including transitive dependencies, against its current advisory source. This is an advisory check, not a guarantee against undisclosed vulnerabilities.
- Local documentation links and `git diff --check` passed.

## Validation limits

The workflow is configured for Linux, macOS, and Windows, but hosted CI has not been executed locally or remotely during this upgrade. Only macOS ARM64 execution was verified. The terrain example uses a fitness heuristic and has no physics/playability verification. Randomness is shared process-wide, so reproducibility checks apply to serial runs on the same runtime. No push, package publication, or hosted deployment was performed.
