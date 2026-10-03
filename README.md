Bacon.ApiUtilities
==================

Bacon.ApiUtilities is a .NET 10 (net10.0) solution made of two reusable libraries, a sample Web API and the unit tests for each library:

  - Bacon.Utilities       General-purpose .NET helpers, with no ASP.NET Core dependency.
  - Bacon.ApiUtilities    ASP.NET Core building blocks for Web APIs. It builds on Bacon.Utilities.

Source code: https://github.com/bacon-perso/ApiUtilities


Documentation
-------------

Each library has its own readme with installation steps, usage examples and the rules to know:

  - Bacon.Utilities:
    https://github.com/bacon-perso/ApiUtilities/blob/master/Bacon.Utilities/README.md

  - Bacon.ApiUtilities:
    https://github.com/bacon-perso/ApiUtilities/blob/master/Bacon.ApiUtilities/README.md


Projects in the solution
------------------------

1. Bacon.Utilities (library)
   A lightweight collection of helpers:
     - Extensions: date range checks, "is in collection" checks on any value, string masking.
     - Services: cryptography (SHA-256/512 hashing, password generation, AES-256-GCM encryption and decryption) and data conversion (DataTable, DateTime, Base64 and boolean parsing).
     - Validators: passwords, usernames, emails, domains, IP addresses, host names, duplicate detection and stored procedure result checks.

2. Bacon.ApiUtilities (library)
   ASP.NET Core building blocks for Web APIs:
     - A single exception and validation error pipeline. Every failure is returned as a consistent, localized ErrorResult with an internal error code.
     - OpenAPI documentation generation with several documents, hidden endpoints and a choice of Swagger UI or Scalar.
     - Endpoint attributes: document and tag assignment, hidden APIs, documented responses and error codes.
     - Per-caller request rate limiting.
     - Ready-made model validation attributes (required, length, range, email, phone number, URL, UTC date, duplicates, ...).
     - Pagination wrapper and an endpoint authorization pattern.

3. Bacon.ApiUtilities.Samples (runnable Web API)
   A working example of every Bacon.ApiUtilities feature. Run it and open the root URL to browse the generated OpenAPI documents in Swagger UI.

4. Bacon.Utilities.Tests and Bacon.ApiUtilities.Tests (unit tests)
   Tests for the two libraries.


How the projects relate
-----------------------

  Bacon.Utilities  <---  Bacon.ApiUtilities  <---  Bacon.ApiUtilities.Samples

Bacon.ApiUtilities references Bacon.Utilities. For example, EmailFormatAttribute uses Bacon.Utilities.Validators.ValidateEmail. Bacon.Utilities also brings in libphonenumber-csharp, which flows on to Bacon.ApiUtilities for PhoneNumberFormatAttribute.


Getting started
---------------

Reference the project you need:

  <ItemGroup>
    <ProjectReference Include="..\Bacon.Utilities\Bacon.Utilities.csproj" />
    <ProjectReference Include="..\Bacon.ApiUtilities\Bacon.ApiUtilities.csproj" />
  </ItemGroup>

Referencing Bacon.ApiUtilities is enough to get Bacon.Utilities as well.

Build, test and run (from the solution folder):

  dotnet build
  dotnet test
  dotnet run --project Bacon.ApiUtilities.Samples

Note: a few Bacon.Utilities host name tests resolve real domains, so they need network access.

Shared build settings (C# 14, nullable reference types enabled) are defined in Directory.Build.props.
