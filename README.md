# Hearthsound plugin template (C#)

A starting point for writing a [Hearthsound](https://github.com/hearthsound) plugin
in C#. It compiles to a WebAssembly component that implements the `catalog`
provider interface, and it answers every search with a greeting.

Copy the repository, rename the plugin, and replace the example behavior with
your own.

## Before you start

Change these three things in `manifest.json`:

- `id`: your own reverse-DNS identifier, for example `com.example.my-catalog`.
- `name`: the name users will see.
- `homepage`: the URL of your project.

Then update `HelloCatalog.csproj` (`RootNamespace` and the project file name if
you rename it) and the plugin id in this README.

## Requirements

- The .NET 9 SDK. `global.json` pins the SDK line `9.0.319` and allows newer
  patch releases. Install it from <https://dotnet.microsoft.com/download/dotnet/9.0>.
- Rust 1.96 or newer, only to install the `hsplugin` CLI described below.
  Install it with [rustup](https://rustup.rs).

The first build downloads NuGet packages, including the NativeAOT-LLVM compiler,
from the feeds named in `nuget.config`.

## Build

```console
dotnet build -c Release
```

The build compiles the C# sources to a WebAssembly component through
NativeAOT-LLVM, generates the WIT bindings from `wit/`, and copies the result to
`build/plugin.wasm`. Use `dotnet build`, not `dotnet publish`: the component
target runs publish as part of the build.

## Check, test and pack

Install the plugin CLI from the SDK repository:

```console
cargo install --locked --git https://github.com/hearthsound/plugin-sdk --tag v0.2.1 hearthsound-plugin-cli
```

Then, from the repository root, validate the manifest and the component:

```console
hsplugin check
```

It prints `ok: <id> <version> (provides <layers>)` on success.

Run the catalog conformance suite against the component:

```console
hsplugin test catalog
```

It prints `ok <n>: <check>` for every check it runs and exits 1 if any of them
fails.

Finally, build the archive you install on a server:

```console
hsplugin pack --out-dir dist
```

This writes `dist/<id>-<version>.hsplugin`, which holds the manifest and the
component.

## Install into a server

Install the archive with the server's `plugins` command:

```console
hearthsound plugins install dist/com.example.hello-catalog-0.1.0.hsplugin
```

Installing grants the permissions the manifest requests and enables the plugin.

## Layout

| Path | Contents |
|---|---|
| `HelloCatalog.csproj` | Project file: the WASI target, the WIT binding source, and the copy of the component to `build/plugin.wasm`. |
| `src/HelloCatalog.cs` | The plugin implementation. |
| `wit/` | The WIT package the component implements, copied from the SDK. |
| `manifest.json` | The plugin manifest the server reads. |
| `global.json`, `nuget.config` | The pinned SDK line and the package feeds. |

## License

Licensed under either of Apache License, Version 2.0 or MIT license at your
option.
