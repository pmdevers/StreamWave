# StreamWave AggregateRoot

![Github Release](https://img.shields.io/github/v/release/pmdevers/streamwave)
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/pmdevers/StreamWave/.github%2Fworkflows%2Fbuild-publish.yml)
![GitHub License](https://img.shields.io/github/license/pmdevers/streamwave)
![Github Issues Open](https://img.shields.io/github/issues/pmdevers/streamwave)
![Github Pull Request Open](https://img.shields.io/github/issues-pr/pmdevers/streamwave)
[![Scheduled Code Security Testing](https://github.com/pmdevers/streamwave/actions/workflows/security-analysis.yml/badge.svg?event=schedule)](https://github.com/pmdevers/streamwave/actions/workflows/security-analysis.yml)


An aggregate root designed for a streaming environment, addressing the dual write problem by decoupling the domain model from the event stream.

![Alt text](https://raw.githubusercontent.com/pmdevers/streamwave/master/assets/logo.png "logo")

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Configuration](#configuration)
- [Contributing](#contributing)
- [License](#license)

## Features

- Easy integration

## Installation

To install the package, use the following command in your .NET Core project:

```bash
dotnet add package StreamWave
```

Alternatively, you can add it manually to your `.csproj` file:

```xml
<PackageReference Include="StreamWave" Version="0.1.0" />
```

## Usage

Here are some basic examples of how to use the library:

### Setup

Create your domain objects

```csharp
// domain.cs

public class TestState 
{
    public required Guid Id { get; set; }
    public string? Test { get; set; }
}

```

Add the aggregate to the service collection

```csharp
// program.cs

using StreamWave;
using StreamWave.EntityFramework;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAggregate<TestState, Guid>((id) => new TestState() {  Id = id })
            .WithEntityFramework<TestContext, TestState, Guid>()
            .WithApplier<TestEvent>((s, e) =>
            {
                s.Test = e.Field;
                return s;
            });

```

### Use

```csharp

public async Task HandleAsync(IAggregateManager<TestState, Guid> manager, Guid id)
{
    var aggregate = manager.LoadAsync(id);

    aggregate.Apply(new TestEvent("Update"));

    await manager.SaveAsync(aggregate);
}

```

## Configuration

[TODO]

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue if you encounter any bugs or have feature requests.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Commit your changes (`git commit -am 'Add some fooBar'`)
4. Push to the branch (`git push origin feature/fooBar`)
5. Create a new Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.md) file for details.
