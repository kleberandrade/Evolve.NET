# Third-party notices

Evolve.NET's original MIT license and copyright notice remain in [LICENSE](../LICENSE).

The core library and runnable examples have no external NuGet dependencies. Tests use xUnit.net v3 (`xunit.v3` 4.0.3, copyright .NET Foundation), licensed under [Apache-2.0](https://github.com/xunit/xunit/blob/main/license.txt), and its transitive Microsoft Testing Platform components. Microsoft Testing Platform is licensed under [MIT](https://github.com/microsoft/testfx/blob/main/LICENSE). Other transitive development dependencies retain their own license notices inside their NuGet packages; exact package versions and hashes are recorded in `Evolve.NET.Tests/packages.lock.json`.

No third-party source files or binaries are vendored into this repository. NuGet package licenses are not replaced by this project's MIT license. If redistributing development tooling or test binaries, retain the notices supplied by those packages.
