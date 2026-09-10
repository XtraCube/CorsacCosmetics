/**
 * TypeScript types that exactly mirror the C# BundleManifest, HatManifest, and SpriteData structs.
 * Property names use PascalCase to match the C# JsonSerializer.Deserialize<BundleManifest> expectations.
 * These types are used both for the binary bundle format and the UI state.
 */

/** Maps directly to C# `SpriteData` struct. Size and Offset are relative to start-of-data. */
export interface SpriteData {
	/** Byte length of the sprite PNG in the data section. 0 = not present. */
	Size: number;
	/** Byte offset from the start of the data section. 0 = not present. */
	Offset: number;
}

/** Maps directly to C# `HatManifest` struct. */
export interface HatManifest {
	Name: string;
	MatchPlayerColor: boolean;
	BlocksVisors: boolean;
	InFront: boolean;
	NoBounce: boolean;
	PreviewSprite: SpriteData;
	MainSprite: SpriteData;
	BackSprite: SpriteData;
	ClimbSprite: SpriteData;
	FloorSprite: SpriteData;
	LeftMainSprite: SpriteData;
	LeftBackSprite: SpriteData;
	LeftClimbSprite: SpriteData;
	LeftFloorSprite: SpriteData;
}

/** Maps directly to C# `VisorManifest` struct. */
export interface VisorManifest {
	Name: string;
	MatchPlayerColor: boolean;
	BehindHats: boolean;
	PreviewSprite: SpriteData;
	IdleSprite: SpriteData;
	LeftIdleSprite: SpriteData;
	FloorSprite: SpriteData;
	ClimbSprite: SpriteData;
}

/** Maps directly to C# `NameplateManifest` struct. */
export interface NameplateManifest {
	Name: string;
	PreviewSprite: SpriteData;
	NameplateSprite: SpriteData;
}

/** Bundle version discriminator used by both the editor UI and binary helpers. */
export type BundleVersion = 1 | 2;

/** Default group name used by v2 bundles when the editor flattens all cosmetics into one group. */
export const DEFAULT_BUNDLE_GROUP_NAME = 'Custom Cosmetics';

/** Editor-only group state used to organize cosmetics while editing v2 bundles. */
export interface BundleEditorGroup {
	id: string;
	name: string;
}

/** Maps directly to C# `BundleManifest` struct for Bundle v1. */
export interface BundleManifestV1 {
	Version: 1;
	Hats: HatManifest[];
	Visors: VisorManifest[];
	Nameplates: NameplateManifest[];
}

/** Maps directly to C# `GroupManifest` struct for Bundle v2. */
export interface BundleGroupManifest {
	Name: string;
	Hats: HatManifest[];
	Visors: VisorManifest[];
	Nameplates: NameplateManifest[];
}

/** Maps directly to C# `BundleManifestV2` struct for Bundle v2. */
export interface BundleManifestV2 {
	Version: 2;
	Groups: BundleGroupManifest[];
}

/** Bundle manifest supported by the editor (v1 flat layout or v2 grouped layout). */
export type BundleManifest = BundleManifestV1 | BundleManifestV2;

/** Sprite slot keys in the deterministic order used when building the data section. */
export const SPRITE_SLOTS = [
	'PreviewSprite',
	'MainSprite',
	'BackSprite',
	'ClimbSprite',
	'FloorSprite',
	'LeftMainSprite',
	'LeftBackSprite',
	'LeftClimbSprite',
	'LeftFloorSprite',
] as const;

export type SpriteSlot = (typeof SPRITE_SLOTS)[number];

/** Human-readable labels for each sprite slot shown in the UI. */
export const SPRITE_SLOT_LABELS: Record<SpriteSlot, string> = {
	PreviewSprite: 'Preview',
	MainSprite: 'Main',
	BackSprite: 'Back',
	ClimbSprite: 'Climb',
	FloorSprite: 'Floor',
	LeftMainSprite: 'Left Main',
	LeftBackSprite: 'Left Back',
	LeftClimbSprite: 'Left Climb',
	LeftFloorSprite: 'Left Floor',
};

/** Visor sprite slot keys in deterministic order used when building the data section. */
export const VISOR_SPRITE_SLOTS = [
	'PreviewSprite',
	'IdleSprite',
	'LeftIdleSprite',
	'FloorSprite',
	'ClimbSprite',
] as const;

export type VisorSpriteSlot = (typeof VISOR_SPRITE_SLOTS)[number];

/** Human-readable labels for each visor sprite slot shown in the UI. */
export const VISOR_SPRITE_SLOT_LABELS: Record<VisorSpriteSlot, string> = {
	PreviewSprite: 'Preview',
	IdleSprite: 'Idle',
	LeftIdleSprite: 'Left Idle',
	FloorSprite: 'Floor',
	ClimbSprite: 'Climb',
};

/** Nameplate sprite slot keys in deterministic order used when building the data section. */
export const NAMEPLATE_SPRITE_SLOTS = [
	'NameplateSprite',
] as const;

export type NameplateSpriteSlot = (typeof NAMEPLATE_SPRITE_SLOTS)[number];

/** Human-readable labels for each nameplate sprite slot shown in the UI. */
export const NAMEPLATE_SPRITE_SLOT_LABELS: Record<NameplateSpriteSlot, string> = {
	NameplateSprite: 'Nameplate',
};

/** UI-level hat state, extends HatManifest with in-memory image blobs for each sprite slot. */
export interface HatEntry {
	/** Unique client-side identifier for list key management */
	id: string;
	/** Editor-only group identifier used for v2 bundle grouping */
	groupId: string;
	manifest: HatManifest;
	/** Raw image bytes for each sprite slot (File contents loaded into memory) */
	imageBytes: Partial<Record<SpriteSlot, Uint8Array>>;
	/** Object URLs created for preview rendering — must be revoked on removal */
	previewUrls: Partial<Record<SpriteSlot, string>>;
	/** Original file names for display */
	fileNames: Partial<Record<SpriteSlot, string>>;
}

/** UI-level visor state, extends VisorManifest with in-memory image blobs for each sprite slot. */
export interface VisorEntry {
	/** Unique client-side identifier for list key management */
	id: string;
	/** Editor-only group identifier used for v2 bundle grouping */
	groupId: string;
	manifest: VisorManifest;
	/** Raw image bytes for each sprite slot (File contents loaded into memory) */
	imageBytes: Partial<Record<VisorSpriteSlot, Uint8Array>>;
	/** Object URLs created for preview rendering — must be revoked on removal */
	previewUrls: Partial<Record<VisorSpriteSlot, string>>;
	/** Original file names for display */
	fileNames: Partial<Record<VisorSpriteSlot, string>>;
}

/** UI-level nameplate state, extends NameplateManifest with in-memory image blobs for each sprite slot. */
export interface NameplateEntry {
	/** Unique client-side identifier for list key management */
	id: string;
	/** Editor-only group identifier used for v2 bundle grouping */
	groupId: string;
	manifest: NameplateManifest;
	/** Raw image bytes for each sprite slot (File contents loaded into memory) */
	imageBytes: Partial<Record<NameplateSpriteSlot, Uint8Array>>;
	/** Object URLs created for preview rendering — must be revoked on removal */
	previewUrls: Partial<Record<NameplateSpriteSlot, string>>;
	/** Original file names for display */
	fileNames: Partial<Record<NameplateSpriteSlot, string>>;
}

/** Bundle-level editor state. */
export interface BundleState {
	hats: HatEntry[];
	statusMessage: string;
	statusType: 'info' | 'success' | 'error' | 'warning';
	/** Computed at download time for debug display */
	lastManifestLength: number | null;
	lastDataLength: number | null;
}

/** Parsed result from opening an existing .ccb file. */
export interface OpenResult {
	manifest: BundleManifest;
	/** ArrayBuffer of the full file, used for slicing the data section */
	buffer: ArrayBuffer;
	/** Byte offset where the data section begins (= 16 + manifestLength) */
	dataStart: number;
}

export function createEmptySpriteData(): SpriteData {
	return { Size: 0, Offset: 0 };
}

export function createDefaultHatManifest(): HatManifest {
	return {
		Name: 'Custom Hat',
		MatchPlayerColor: false,
		BlocksVisors: false,
		InFront: true,
		NoBounce: true,
		PreviewSprite: createEmptySpriteData(),
		MainSprite: createEmptySpriteData(),
		BackSprite: createEmptySpriteData(),
		ClimbSprite: createEmptySpriteData(),
		FloorSprite: createEmptySpriteData(),
		LeftMainSprite: createEmptySpriteData(),
		LeftBackSprite: createEmptySpriteData(),
		LeftClimbSprite: createEmptySpriteData(),
		LeftFloorSprite: createEmptySpriteData(),
	};
}

export function createHatEntry(manifest?: HatManifest, groupId = ''): HatEntry {
	return {
		id: crypto.randomUUID(),
		groupId,
		manifest: manifest ?? createDefaultHatManifest(),
		imageBytes: {},
		previewUrls: {},
		fileNames: {},
	};
}

export function createDefaultVisorManifest(): VisorManifest {
	return {
		Name: 'Custom Visor',
		MatchPlayerColor: false,
		BehindHats: false,
		PreviewSprite: createEmptySpriteData(),
		IdleSprite: createEmptySpriteData(),
		LeftIdleSprite: createEmptySpriteData(),
		FloorSprite: createEmptySpriteData(),
		ClimbSprite: createEmptySpriteData(),
	};
}

export function createVisorEntry(manifest?: VisorManifest, groupId = ''): VisorEntry {
	return {
		id: crypto.randomUUID(),
		groupId,
		manifest: manifest ?? createDefaultVisorManifest(),
		imageBytes: {},
		previewUrls: {},
		fileNames: {},
	};
}

export function createDefaultNameplateManifest(): NameplateManifest {
	return {
		Name: 'Custom Nameplate',
		PreviewSprite: createEmptySpriteData(),
		NameplateSprite: createEmptySpriteData(),
	};
}

export function createBundleGroupManifest(
	name: string = DEFAULT_BUNDLE_GROUP_NAME,
	hats: HatManifest[] = [],
	visors: VisorManifest[] = [],
	nameplates: NameplateManifest[] = []
): BundleGroupManifest {
	return {
		Name: name,
		Hats: hats,
		Visors: visors,
		Nameplates: nameplates,
	};
}

export function createBundleEditorGroup(name: string = DEFAULT_BUNDLE_GROUP_NAME): BundleEditorGroup {
	return {
		id: crypto.randomUUID(),
		name,
	};
}

export function createBundleManifest(
	version: BundleVersion,
	hats: HatManifest[] = [],
	visors: VisorManifest[] = [],
	nameplates: NameplateManifest[] = [],
	groups: BundleGroupManifest[] = []
): BundleManifest {
	if (version === 1) {
		return {
			Version: 1,
			Hats: hats,
			Visors: visors,
			Nameplates: nameplates,
		};
	}

	if (groups.length > 0) {
		return {
			Version: 2,
			Groups: groups,
		};
	}

	const hasAnyCosmetics = hats.length > 0 || visors.length > 0 || nameplates.length > 0;
	return {
		Version: 2,
		Groups: hasAnyCosmetics ? [createBundleGroupManifest(DEFAULT_BUNDLE_GROUP_NAME, hats, visors, nameplates)] : [],
	};
}

export function createNameplateEntry(manifest?: NameplateManifest, groupId = ''): NameplateEntry {
	return {
		id: crypto.randomUUID(),
		groupId,
		manifest: manifest ?? createDefaultNameplateManifest(),
		imageBytes: {},
		previewUrls: {},
		fileNames: {},
	};
}
