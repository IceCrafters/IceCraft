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