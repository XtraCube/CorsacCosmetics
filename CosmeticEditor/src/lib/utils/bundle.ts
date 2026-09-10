/**
 * bundle.ts — Binary read/write utilities for the .ccb (Corsac Cosmetics Bundle) format.
 *
 * Binary layout (all fields little-endian):
 *   Offset  0: uint32  Magic          = 0x434F5253 ("CORS" in ASCII)
	 *   Offset  4: uint16  Version        = 1 (flat manifest) or 2 (grouped manifest)
 *   Offset  6: uint16  Flags          = 0 (reserved)
 *   Offset  8: uint32  ManifestLength = byte length of the UTF-8 JSON manifest
 *   Offset 12: uint32  DataLength     = total bytes of all appended image data
 *   --- end of 16-byte header ---
 *   Offset 16: [ManifestLength bytes] UTF-8 JSON matching BundleManifest v1 or v2 shape
 *   Offset 16+ManifestLength: [DataLength bytes] raw concatenated PNG bytes
 *
 * Sprite Offset values in the manifest are relative to the start of the data section
 * (i.e., byte position 16 + ManifestLength in the file).
 *
 * This module is pure browser code — no Node/server APIs used.
 */

import type {
	BundleManifest,
	BundleManifestV1,
	BundleManifestV2,
	BundleVersion,
	BundleEditorGroup,
	HatEntry,
	HatManifest,
	VisorEntry,
	VisorManifest,
	NameplateEntry,
	NameplateManifest,
	SpriteData,
	SpriteSlot,
} from '$lib/types';
import {
	DEFAULT_BUNDLE_GROUP_NAME,
	SPRITE_SLOTS,
	VISOR_SPRITE_SLOTS,
	NAMEPLATE_SPRITE_SLOTS,
	createBundleManifest,
	createBundleEditorGroup,
	createEmptySpriteData,
} from '$lib/types';

// ---------------------------------------------------------------------------
// Constants
// ---------------------------------------------------------------------------

export const BUNDLE_MAGIC = 0x434f5253; // 'CORS' in ASCII
export const BUNDLE_VERSION = 1;
export const BUNDLE_VERSION_V2 = 2;
export const SUPPORTED_BUNDLE_VERSION = BUNDLE_VERSION_V2;
export const HEADER_SIZE = 16;
export const MAX_BUNDLE_SIZE_BYTES = 50 * 1024 * 1024; // 50 MB warning threshold

export const BUNDLE_VERSION_DESCRIPTIONS: Record<BundleVersion, string> = {
	1: 'Corsac v0.1.0',
	2: 'Corsac v2',
};

export function describeBundleVersion(version: BundleVersion): string {
	return `Bundle v${version} (${BUNDLE_VERSION_DESCRIPTIONS[version]})`;
}

// ---------------------------------------------------------------------------
// Header read/write (little-endian DataView operations)
// ---------------------------------------------------------------------------

export interface BundleHeader {
	magic: number;
	version: number;
	flags: number;
	manifestLength: number;
	dataLength: number;
}

/**
 * Write the 16-byte bundle header into a new ArrayBuffer.
 * All fields are written little-endian to match C# BinaryPrimitives.WriteUInt*LittleEndian.
 */
export function writeHeader(manifestLength: number, dataLength: number, version: BundleVersion = BUNDLE_VERSION): ArrayBuffer {
	const buf = new ArrayBuffer(HEADER_SIZE);
	const view = new DataView(buf);
	// Offset  0: uint32 Magic
	view.setUint32(0, BUNDLE_MAGIC, true);
	// Offset  4: uint16 Version
	view.setUint16(4, version, true);
	// Offset  6: uint16 Flags (reserved = 0)
	view.setUint16(6, 0, true);
	// Offset  8: uint32 ManifestLength
	view.setUint32(8, manifestLength, true);
	// Offset 12: uint32 DataLength
	view.setUint32(12, dataLength, true);
	return buf;
}

/**
 * Read and parse the 16-byte header from an ArrayBuffer.
 * Throws a descriptive error if the buffer is too short.
 */
export function readHeader(buffer: ArrayBuffer): BundleHeader {
	if (buffer.byteLength < HEADER_SIZE) {
		throw new Error(`File too short: expected at least ${HEADER_SIZE} bytes, got ${buffer.byteLength}.`);
	}
	const view = new DataView(buffer, 0, HEADER_SIZE);
	return {
		magic: view.getUint32(0, true),
		version: view.getUint16(4, true),
		flags: view.getUint16(6, true),
		manifestLength: view.getUint32(8, true),
		dataLength: view.getUint32(12, true),
	};
}

/**
 * Validate header magic and version. Returns an error string or null on success.
 */
export function validateHeader(header: BundleHeader): string | null {
	if (header.magic !== BUNDLE_MAGIC) {
		const got = '0x' + header.magic.toString(16).toUpperCase().padStart(8, '0');
		return `Invalid magic number: expected 0x434F5253 ("CORS"), got ${got}. This is not a valid .ccb file.`;
	}
	if (header.version === 0 || header.version > SUPPORTED_BUNDLE_VERSION) {
		return `Unsupported bundle version: ${header.version}. This editor supports Bundle v1 and Bundle v2.`;
	}
	return null;
}

// ---------------------------------------------------------------------------
// UTF-8 encode / decode
// ---------------------------------------------------------------------------

const encoder = new TextEncoder();
const decoder = new TextDecoder('utf-8');

export function encodeUtf8(str: string): Uint8Array {
	return encoder.encode(str);
}

export function decodeUtf8(bytes: ArrayBuffer | ArrayBufferView): string {
	return decoder.decode(bytes);
}

// ---------------------------------------------------------------------------
// Manifest serialization
// ---------------------------------------------------------------------------

/**
 * Serialize a BundleManifest to a UTF-8 JSON string.
 * Property names stay in PascalCase to match the C# JsonSerializer expectations.
 * Uses JSON.stringify with stable ordering of sprite fields.
 */
export function serializeManifest(manifest: BundleManifest): Uint8Array {
	const json = JSON.stringify(manifest);
	return encodeUtf8(json);
}

/**
 * Deserialize a UTF-8 JSON byte range into a BundleManifest.
 */
export function deserializeManifest<TManifest extends BundleManifest = BundleManifest>(buffer: ArrayBuffer, offset: number, length: number): TManifest {
	const slice = buffer.slice(offset, offset + length);
	const json = decodeUtf8(slice);
	return JSON.parse(json) as TManifest;
}

// ---------------------------------------------------------------------------
// Bundle assembly (Download .ccb)
// ---------------------------------------------------------------------------

export interface AssembledBundle {
	blob: Blob;
	manifestLength: number;
	dataLength: number;
	warnings: string[];
}

function areBytesEqual(a: Uint8Array | Buffer, b: Uint8Array | Buffer): boolean {
	if (a === b) return true;
	if (a.byteLength !== b.byteLength) return false;

	for (let i = 0; i < a.byteLength; i++) {
		if (a[i] !== b[i]) return false;
	}
	return true;
}

/**
 * Assemble a complete .ccb bundle from lists of HatEntry and VisorEntry objects.
 *
 * Algorithm:
 *  1. Collect all image bytes per hat per sprite slot, then per visor per sprite slot,
 *     in deterministic order (SPRITE_SLOTS for hats, VISOR_SPRITE_SLOTS for visors).
 *  2. Assign each sprite an Offset (relative to start-of-data) and Size.
 *     If no image, Size=0 and Offset=0.
 *  3. Build the BundleManifest JSON.
 *  4. Compute ManifestLength and DataLength.
 *  5. Write 16-byte header.
 *  6. Concatenate: header + manifestBytes + dataBytes → Blob.
 */
export function assembleBundle(
	hats: HatEntry[],
	visors: VisorEntry[] = [],
	nameplates: NameplateEntry[] = [],
	bundleVersion: BundleVersion = BUNDLE_VERSION,
	groups: BundleEditorGroup[] = []
): AssembledBundle {
	const warnings: string[] = [];

	// --- Step 1 & 2: collect image bytes, compute offsets ---
	const dataParts: Uint8Array[] = [];
	let runningOffset = 0;

	function processCosmetics(items: any[], typeName: string, slotsArray: string[]) {
		return items.map((item) => {
			const manifest = { ...item.manifest };

			if (!manifest.Name || manifest.Name.trim() === '') {
				warnings.push(`A ${typeName.toLowerCase()} has an empty name — it will default to "Custom ${typeName}".`);
				manifest.Name = `Custom ${typeName}`;
			}
			const localSpriteCache: { bytes: Uint8Array | Buffer; spriteData: SpriteData }[] = [];

			for (const slot of slotsArray) {
				const bytes = item.imageBytes[slot];

				if (bytes && bytes.byteLength > 0) {
					const cached = localSpriteCache.find(entry => areBytesEqual(entry.bytes, bytes));

					if (cached) {
						manifest[slot] = { Size: cached.spriteData.Size, Offset: cached.spriteData.Offset } as SpriteData;
					} else {
						const spriteData = { Size: bytes.byteLength, Offset: runningOffset } as SpriteData;

						localSpriteCache.push({ bytes, spriteData });
						dataParts.push(bytes);
						runningOffset += bytes.byteLength;

						manifest[slot] = spriteData;
					}
				} else {
					manifest[slot] = createEmptySpriteData();
				}
			}

			return { manifest, groupId: item.groupId };
		});
	}

	const resolvedHats = processCosmetics(hats, 'Hat', SPRITE_SLOTS);
	const resolvedVisors = processCosmetics(visors, 'Visor', VISOR_SPRITE_SLOTS);
	const resolvedNameplates = processCosmetics(nameplates, 'Nameplate', NAMEPLATE_SPRITE_SLOTS);

	const dataLength = runningOffset;
	const resolvedGroups = normalizeEditorGroups(groups, resolvedHats, resolvedVisors, resolvedNameplates);
	warnings.push(...collectGroupWarnings(resolvedGroups));

	// --- Step 3: build manifest ---
	const manifest: BundleManifest = createBundleManifest(
		bundleVersion,
		resolvedHats.map((entry) => entry.manifest),
		resolvedVisors.map((entry) => entry.manifest),
		resolvedNameplates.map((entry) => entry.manifest),
		bundleVersion === 2
			? resolvedGroups.map((group) => ({
				Name: group.name,
				Hats: group.hats.map((entry) => entry.manifest),
				Visors: group.visors.map((entry) => entry.manifest),
				Nameplates: group.nameplates.map((entry) => entry.manifest),
			}))
			: []
	);

	// --- Step 4: serialize manifest ---
	const manifestBytes = serializeManifest(manifest);
	const manifestLength = manifestBytes.byteLength;

	// --- Step 5: write header ---
	const headerBuffer = writeHeader(manifestLength, dataLength, bundleVersion);

	// --- Step 6: size warning ---
	const totalSize = HEADER_SIZE + manifestLength + dataLength;
	if (totalSize > MAX_BUNDLE_SIZE_BYTES) {
		warnings.push(`Bundle size is ${(totalSize / 1024 / 1024).toFixed(1)} MB, which exceeds the 50 MB warning threshold.`);
	}

	// --- Assemble Blob ---
	const parts: BlobPart[] = [headerBuffer as ArrayBuffer, manifestBytes.buffer as ArrayBuffer];
	for (const part of dataParts) {
		parts.push(part.buffer as ArrayBuffer);
	}
	const blob = new Blob(parts, { type: 'application/octet-stream' });

	return { blob, manifestLength, dataLength, warnings };
}

function normalizeEditorGroups(
	groups: BundleEditorGroup[],
	hats: Array<{ manifest: HatManifest; groupId: string }>,
	visors: Array<{ manifest: VisorManifest; groupId: string }>,
	nameplates: Array<{ manifest: NameplateManifest; groupId: string }>
): Array<{
	id: string;
	name: string;
	hats: Array<{ manifest: HatManifest; groupId: string }>;
	visors: Array<{ manifest: VisorManifest; groupId: string }>;
	nameplates: Array<{ manifest: NameplateManifest; groupId: string }>;
}> {
	const sourceGroups = groups.length > 0 ? groups : [createBundleEditorGroup(DEFAULT_BUNDLE_GROUP_NAME)];
	const primaryGroupId = sourceGroups[0].id;
	const groupIds = new Set(sourceGroups.map((group) => group.id));

	const pickGroupId = (groupId: string) => (groupIds.has(groupId) ? groupId : primaryGroupId);

	return sourceGroups.map((group) => ({
		id: group.id,
		name: group.name || DEFAULT_BUNDLE_GROUP_NAME,
		hats: hats.filter((entry) => pickGroupId(entry.groupId) === group.id),
		visors: visors.filter((entry) => pickGroupId(entry.groupId) === group.id),
		nameplates: nameplates.filter((entry) => pickGroupId(entry.groupId) === group.id),
	}));
}

function collectGroupWarnings(
	groups: Array<{
		name: string;
		hats: Array<{ manifest: HatManifest }>;
		visors: Array<{ manifest: VisorManifest }>;
		nameplates: Array<{ manifest: NameplateManifest }>;
	}>
): string[] {
	const warnings: string[] = [];
	const groupNameCounts = new Map<string, number>();
	for (const group of groups) {
		groupNameCounts.set(group.name, (groupNameCounts.get(group.name) ?? 0) + 1);

		const hatNameCounts = new Map<string, number>();
		for (const hat of group.hats) hatNameCounts.set(hat.manifest.Name, (hatNameCounts.get(hat.manifest.Name) ?? 0) + 1);
		for (const [name, count] of hatNameCounts) {
			if (count > 1) warnings.push(`Duplicate hat name "${name}" found ${count} times in group "${group.name}".`);
		}

		const visorNameCounts = new Map<string, number>();
		for (const visor of group.visors) visorNameCounts.set(visor.manifest.Name, (visorNameCounts.get(visor.manifest.Name) ?? 0) + 1);
		for (const [name, count] of visorNameCounts) {
			if (count > 1) warnings.push(`Duplicate visor name "${name}" found ${count} times in group "${group.name}".`);
		}

		const nameplateNameCounts = new Map<string, number>();
		for (const nameplate of group.nameplates) nameplateNameCounts.set(nameplate.manifest.Name, (nameplateNameCounts.get(nameplate.manifest.Name) ?? 0) + 1);
		for (const [name, count] of nameplateNameCounts) {
			if (count > 1) warnings.push(`Duplicate nameplate name "${name}" found ${count} times in group "${group.name}".`);
		}
	}

	for (const [name, count] of groupNameCounts) {
		if (count > 1) warnings.push(`Duplicate group name "${name}" found ${count} times.`);
	}

	return warnings;
}

// ---------------------------------------------------------------------------
// Bundle parsing (Open .ccb)
// ---------------------------------------------------------------------------

export interface ParsedHat {
	manifest: HatManifest;
	groupId: string;
	/** Per-slot image bytes, sliced from the file buffer */
	imageBytes: Partial<Record<(typeof SPRITE_SLOTS)[number], Uint8Array>>;
}

export interface ParsedVisor {
	manifest: VisorManifest;
	groupId: string;
	/** Per-slot image bytes, sliced from the file buffer */
	imageBytes: Partial<Record<(typeof VISOR_SPRITE_SLOTS)[number], Uint8Array>>;
}

export interface ParsedNameplate {
	manifest: NameplateManifest;
	groupId: string;
	/** Per-slot image bytes, sliced from the file buffer */
	imageBytes: Partial<Record<(typeof NAMEPLATE_SPRITE_SLOTS)[number], Uint8Array>>;
}

export interface ParsedBundle {
	version: BundleVersion;
	manifest: BundleManifest;
	groups: BundleEditorGroup[];
	hats: ParsedHat[];
	visors: ParsedVisor[];
	nameplates: ParsedNameplate[];
	warnings: string[];
}

/**
 * Parse a complete .ccb file from an ArrayBuffer.
 *
 * The data section starts at byte (HEADER_SIZE + manifestLength).
 * Each sprite's Offset in the manifest is relative to that data section start.
 */
export function parseBundle(buffer: ArrayBuffer): ParsedBundle {
	const warnings: string[] = [];

	// --- Read header ---
	const header = readHeader(buffer);
	const headerError = validateHeader(header);
	if (headerError) throw new Error(headerError);
	const version = header.version as BundleVersion;

	const minExpectedSize = HEADER_SIZE + header.manifestLength + header.dataLength;
	if (buffer.byteLength < minExpectedSize) {
		throw new Error(
			`File appears truncated: header claims ${minExpectedSize} bytes but file is ${buffer.byteLength} bytes.`
		);
	}

	// --- Parse manifest and extract sprite bytes ---
	const dataStart = HEADER_SIZE + header.manifestLength;
	if (version === BUNDLE_VERSION) {
		const manifest = deserializeManifest<BundleManifestV1>(buffer, HEADER_SIZE, header.manifestLength);
		if (!manifest.Hats || !Array.isArray(manifest.Hats)) {
			throw new Error('Manifest is missing the "Hats" array.');
		}
		if (manifest.Version !== version) {
			warnings.push(`Manifest Version field is ${manifest.Version}; expected 1.`);
		}

		const group = createBundleEditorGroup(DEFAULT_BUNDLE_GROUP_NAME);
		const hats: ParsedHat[] = manifest.Hats.map((hatManifest) => ({ ...extractHatBytes(hatManifest, buffer, dataStart, warnings), groupId: group.id }));
		const visors: ParsedVisor[] = (manifest.Visors ?? []).map((visorManifest) => ({ ...extractVisorBytes(visorManifest, buffer, dataStart, warnings), groupId: group.id }));
		const nameplates: ParsedNameplate[] = (manifest.Nameplates ?? []).map((nameplateManifest) => ({ ...extractNameplateBytes(nameplateManifest, buffer, dataStart, warnings), groupId: group.id }));

		return { version, manifest, groups: [group], hats, visors, nameplates, warnings };
	}

	const manifest = deserializeManifest<BundleManifestV2>(buffer, HEADER_SIZE, header.manifestLength);
	if (!manifest.Groups || !Array.isArray(manifest.Groups)) {
		throw new Error('Manifest is missing the "Groups" array.');
	}
	if (manifest.Version !== version) {
		warnings.push(`Manifest Version field is ${manifest.Version}; expected 2.`);
	}
	if (manifest.Groups.length > 1) {
		warnings.push(`Bundle v2 contains ${manifest.Groups.length} groups.`);
	}

	const groups = manifest.Groups.map((group) => createBundleEditorGroup(group.Name || DEFAULT_BUNDLE_GROUP_NAME));
	const hats: ParsedHat[] = [];
	const visors: ParsedVisor[] = [];
	const nameplates: ParsedNameplate[] = [];
	for (let index = 0; index < manifest.Groups.length; index++) {
		const groupManifest = manifest.Groups[index];
		const groupId = groups[index].id;
		for (const hatManifest of groupManifest.Hats ?? []) {
			hats.push({ ...extractHatBytes(hatManifest, buffer, dataStart, warnings), groupId });
		}
		for (const visorManifest of groupManifest.Visors ?? []) {
			visors.push({ ...extractVisorBytes(visorManifest, buffer, dataStart, warnings), groupId });
		}
		for (const nameplateManifest of groupManifest.Nameplates ?? []) {
			nameplates.push({ ...extractNameplateBytes(nameplateManifest, buffer, dataStart, warnings), groupId });
		}
	}

	return { version, manifest, groups, hats, visors, nameplates, warnings };
}

function extractHatBytes(
	hatManifest: HatManifest,
	buffer: ArrayBuffer,
	dataStart: number,
	warnings: string[]
): ParsedHat {
	const imageBytes: Partial<Record<(typeof SPRITE_SLOTS)[number], Uint8Array>> = {};

	for (const slot of SPRITE_SLOTS) {
		const sprite: SpriteData = hatManifest[slot] ?? { Size: 0, Offset: 0 };
		if (sprite.Size > 0) {
			const start = dataStart + sprite.Offset;
			const end = start + sprite.Size;
			if (end > buffer.byteLength) {
				warnings.push(
					`Hat "${hatManifest.Name}" sprite ${slot}: data range [${start}, ${end}) exceeds file size ${buffer.byteLength}. Skipping.`
				);
			} else {
				imageBytes[slot] = new Uint8Array(buffer.slice(start, end));
			}
		}
	}

	return { manifest: hatManifest, groupId: '', imageBytes };
}

function extractVisorBytes(
	visorManifest: VisorManifest,
	buffer: ArrayBuffer,
	dataStart: number,
	warnings: string[]
): ParsedVisor {
	const imageBytes: Partial<Record<(typeof VISOR_SPRITE_SLOTS)[number], Uint8Array>> = {};

	for (const slot of VISOR_SPRITE_SLOTS) {
		const sprite: SpriteData = visorManifest[slot] ?? { Size: 0, Offset: 0 };
		if (sprite.Size > 0) {
			const start = dataStart + sprite.Offset;
			const end = start + sprite.Size;
			if (end > buffer.byteLength) {
				warnings.push(
					`Visor "${visorManifest.Name}" sprite ${slot}: data range [${start}, ${end}) exceeds file size ${buffer.byteLength}. Skipping.`
				);
			} else {
				imageBytes[slot] = new Uint8Array(buffer.slice(start, end));
			}
		}
	}

	return { manifest: visorManifest, groupId: '', imageBytes };
}

function extractNameplateBytes(
	nameplateManifest: NameplateManifest,
	buffer: ArrayBuffer,
	dataStart: number,
	warnings: string[]
): ParsedNameplate {
	const imageBytes: Partial<Record<(typeof NAMEPLATE_SPRITE_SLOTS)[number], Uint8Array>> = {};

	for (const slot of NAMEPLATE_SPRITE_SLOTS) {
		const sprite: SpriteData = nameplateManifest[slot] ?? { Size: 0, Offset: 0 };
		if (sprite.Size > 0) {
			const start = dataStart + sprite.Offset;
			const end = start + sprite.Size;
			if (end > buffer.byteLength) {
				warnings.push(
					`Nameplate "${nameplateManifest.Name}" sprite ${slot}: data range [${start}, ${end}) exceeds file size ${buffer.byteLength}. Skipping.`
				);
			} else {
				imageBytes[slot] = new Uint8Array(buffer.slice(start, end));
			}
		}
	}

	return { manifest: nameplateManifest, groupId: '', imageBytes };
}

// ---------------------------------------------------------------------------
// File download helper
// ---------------------------------------------------------------------------

/**
 * Trigger a browser download of the given Blob as the specified filename.
 */
export function triggerDownload(blob: Blob, filename: string): void {
	const url = URL.createObjectURL(blob);
	const a = document.createElement('a');
	a.href = url;
	a.download = filename;
	a.style.display = 'none';
	document.body.appendChild(a);
	a.click();
	document.body.removeChild(a);
	// Revoke after a short delay to allow the download to start
	setTimeout(() => URL.revokeObjectURL(url), 5000);
}

// ---------------------------------------------------------------------------
// Preview URL helpers
// ---------------------------------------------------------------------------

/**
 * Create an object URL from raw image bytes. Caller is responsible for revoking it.
 */
export function createPreviewUrl(bytes: Uint8Array): string {
	const blob = new Blob([bytes.buffer as ArrayBuffer], { type: 'image/png' });
	return URL.createObjectURL(blob);
}
