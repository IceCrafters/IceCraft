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