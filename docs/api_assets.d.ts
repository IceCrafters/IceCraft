/**
 * Provides services for assets.
 */
export namespace Assets {
    /**
     * Acquires an asset.
     * @param fileName The file name of the asset.
     * @throws If the file does not exist.
     */
    export function getAsset(fileName: string) : AssetHandle;
}

/**
 * An opaque interface for referencing an open asset.
 */
export interface AssetHandle {
    /**
     * Releases the resources held open with this asset and invalidates this
     * instance.
     */
    dispose() : void;
}