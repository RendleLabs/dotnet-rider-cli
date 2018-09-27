# dotnet-rider-cli

This is a simple .NET Global tool to launch JetBrains Rider from the CLI on Windows.

## Installation

Windows-only for now:

```shell
λ dotnet tool install -g dotnet-rider-cli
```

**Using a Mac or Linux?**
You can enable lauching Rider via command line by going to `Tools > Create Command-line Launcher`.

## Usage

To open the current directory:
```shell
λ rider .
```
To open a solution:
```shell
λ rider Thing.sln
```

## Why?

I install Rider using the JetBrains toolbox, and I generally
use the Nightly builds, so the install location changes
a lot. I don't want to keep updating an alias, so I made this.
It finds all the installations of Rider under
`%LOCALAPPDATA%\JetBrains\Toolbox\apps\Rider` and runs
the most recent version it finds.

Hope somebody else finds it useful.
