# Globals

## Functions

### author

```ts
function author(name: string, email: string) : AuthorInfo;
```

Creates a new instance of the `AuthorInfo` record representing the specified author information.

### onConfigure

```ts
function onConfigure(fn: (installDir: string) => void);
```

Sets the configuration function, which is called immediately after expanding and preprocessing.

Configuration functions have a `Configuration` context.

### onExpand

```ts
function onExpand(fn: (artefact: string, to: string) => void);
```

Sets the expand function that is called when expanding an artefact.

- `fn`: The function to set as the expand function.
  - `artefact`: The artefact file to expand.
  - `to`: The directory to expand the artefact file to. May or may not be the final installation path.

### onExportEnv

```ts
function onConfigure(fn: (installDir: string) => void);
```

Sets the environment export function, which is called when requested during installation or configuration of other scripts.

Environment exportation functions have a `Configuration` context.

### onUnConfigure

```ts
function onUnConfigure(fn: (installDir: string) => void);
```

Sets the function that reverts the changes made by the configuration function. The function is called when uninstalling and when re-configurating the package.

The specified function have a `Configuration` context.

### setMeta

```ts
function setMeta(meta: PackageMeta) : void;
```

Specifies metadata information for the calling Mashiro script. Must be called at the beginning of the script file, and it is best to assume that any API other than `MetaBuilder` requires it.

- `meta`: The metadata to set.

### onPreprocess

```ts
function onPreprocess(fn: (tempDir: string, to: string) => void) : void;
```

Specifies the preprocess function. It is not required to specify one.

If specifies, the artefact will be expanded to a temporary location to preprocess.

Artefact preprocessing function is recommend to perform actions such as building a source package.

- `fn`: The preprocessor function.
  - `fromDir`: The temporary folder where the artefact was expanded to await preprocessing.
  - `to`: The directory that will be the installation directory.

### onRemove

```ts
function onRemove(fn: (dir: string) => void) : void;
```

Specifies the function that is used to remove the files of an installed package from disk.

- `fn`: The function to set.
  - `dir`: The directory to remove.

### semVer

```ts
function semVer(version: string) : SemVersion;
```

Creates a new instance of `SemVersion` class representing the specified version string.

- `version`: the version string.