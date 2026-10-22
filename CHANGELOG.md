# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- The hello-catalog plugin: a manifest, a C# project that compiles to a
  WebAssembly component for `hearthsound:plugin@0.2.0` with NativeAOT-LLVM,
  and the plugin implementation.
- CI that builds the component, runs `hsplugin check`, `hsplugin test catalog`
  and `hsplugin pack`, and uploads the `.hsplugin` artifact.

### Changed

- The install instructions no longer ask for a separate approval step; installing grants the permissions the manifest requests and enables the plugin.
