//#region Assets

/**
 * Provides services for assets.
 */
declare namespace Assets {
    /**
     * Acquires an asset.
     * @param fileName The file name of the asset.
     * @throws If the file does not exist.
     */
    export function getAsset(fileName: string) : AssetHandle;
}

/**
 * References an open asset.
 */
declare interface AssetHandle {
    /**
     * Releases the resources held open with this asset and invalidates this
     * instance.
     */
    dispose() : void;
}

//#endregion

//#region Compressed Archive

declare namespace CompressedArchive {
    /**
     * Expands the specified archive file.
     * @param archive The archive file to expand.
     * @param destination The destination directory to expand the archive to.
     * @param overwrite If set to true, existing files are overwritten.
     */
    function expand(archive: string, destination: string, overwrite?: boolean): void;

    /**
     * Expands the specified archive file.
     * @param archive The archive asset to expand.
     * @param destination The destination directory to expand the archive to.
     * @param overwrite If set to true, existing files are overwritten.
     * @param leaveOpen If set to true, the asset will be left open.
     */
    function expand(asset: AssetHandle, destination: string, overwrite?: boolean, leaveOpen?: boolean): void;
}
//#endregion

//#region Binary

/**
 * Provides executable registration services. This API is available only under
 * the Configuration context.
 */
declare namespace Binary {
    function register(fileName: string, path: string): Promise<void>;
    function register(fileName: string, path: string, envs: EnvironmentVariableDictionary): Promise<void>;
    function unregister(fileName: string): Promise<void>;
}

declare class EnvironmentVariableDictionary implements Map<string, string> {
    clear(): void;
    delete(key: string): boolean;
    forEach(callbackfn: (value: string, key: string, map: Map<string, string>) => void, thisArg?: any): void;
    get(key: string): string | undefined;
    has(key: string): boolean;
    set(key: string, value: string): this;
    size: number;
    entries(): IterableIterator<[string, string]>;
    keys(): IterableIterator<string>;
    values(): IterableIterator<string>;
    [Symbol.iterator](): IterableIterator<[string, string]>;
    [Symbol.toStringTag]: string;
}

//#endregion

//#region Packages

/**
 * Represents a package.
 */
declare interface PackageMeta {}

declare namespace Packages {
    function getLatestInstalledPackage(id: string): PackageMeta | null;

    /**
     * Gets the latest version of a package that is installed.
     * @param id The ID to get.
     * @param traceVirtualProvider If set to true, search and return the provider if package is virtual.
     */
    function getLatestInstalledPackage(id: string, traceVirtualProvider: boolean): PackageMeta | null;

    /**
     * Imports environment from the specified package.
     * @param package The package to import.
     */
    function importEnvironment(package: PackageMeta): void;

    /**
     * Registers a virtual package. This API is only available under Configuration context.
     * @param package The metadata of the virtual package to register.
     */
    function registerVirtual(package: PackageMeta): Promise<void>;

    /**
     * Registers a virtual package with the specified ID and all other metadata equivalent to the
     * current package.
     * This API is only available under Configuration context.
     * @param id The ID of the virtual package to register.
     */
    function registerVirtual(id: string): Promise<void>;
}

//#endregion

//#region Os

/**
 * Provides operating system services. The APIs under this namespace are only
 * accessible under Installation or Configuration context.
 */
declare namespace Os {
    /**
     * Executes an executable with the specified arguments under the current environment.
     * @param file The file to execute.
     * @param arguments The arguments to pass to the executed process.
     */
    function execute(file: string, ...arguments: string[]): number; 

    /**
     * Executes a system command using the operating system shell interpreter. The command will
     * be executed in a new environment.
     * @param command The command to execute.
     * @returns The exit code.
     */
    function system(command: string): number;

    /**
     * Sets an environment variable for the current process.
     * @param key The name of the environment variable to set.
     * @param value The value to set to.
     */
    function setProcessEnv(key: string, value: string): void;

    /**
     * Removes an environment variable from the current process.
     * @param key The name of the environment variable to remove.
     */
    function removeProcessEnv(key: string): void;

    /**
     * Adds an entry to the PATH environment variable of the current process.
     * @param path The path to add.
     */
    function addProcessPath(path: string): void;

    /**
     * Removes an entry from the PATH environment variable of the current process.
     * @param path The path to remove.
     */
    function removeProcessPath(path: string): void;
}

//#endregion

//#region Globals

/**
 * The application base path.
 */
declare const AppBasePath: string;

//#endregion