# CharacterArt.md — The Black Breath

## Purpose

This file defines the visual-generation rules for character sprites and character pose variants in The Black Breath.

Its purpose is to keep generated character art consistent with the existing game, compatible with Unity, and visually stable when switching between poses during gameplay.

The original character sprite is always the canonical source.

---

## 1. General art direction

The Black Breath uses a dark-fantasy, grim and grounded visual style that sits between painterly illustration and graphic inked art.

Character art should feel:

- dark fantasy;
- worn and lived-in;
- painterly surfaces organized through clear graphic shapes;
- moderate, variable contour accents rather than uniformly heavy outlines;
- strong, clearly defined directional shadows with restrained soft transitions where useful;
- pronounced light-and-shadow contrast without reducing every material to flat cel shading;
- large readable value groups supported by selective texture and detail;
- readable at gameplay scale;
- grounded anatomy with limited graphic exaggeration for pose and silhouette readability;
- detailed enough to feel handcrafted, but simplified enough to avoid visual noise;
- physically plausible within the game's fantasy setting.

Avoid:

- glossy or plastic rendering;
- mobile-game aesthetics;
- cartoon exaggeration;
- fully photorealistic rendering or material micro-detail;
- uniformly thick comic-book outlines;
- completely flat two-tone cel shading;
- soft airbrushed rendering without clear edge definition;
- excessive fantasy ornamentation;
- generic AI-fantasy details;
- unnecessary glowing effects;
- overdesigned armor or weapons;
- drastic stylistic reinterpretations;
- sepia filters.

Never use a sepia filter.

Existing Black Breath character assets have priority over any generic style assumption.

### 1.1 Painterly / graphic balance

The target is a controlled midpoint between the existing painterly character art and a strongly inked dark-fantasy illustration. It should not become a full comic-book or pure cel-shaded style.

Use the graphic influence mainly to improve:

- silhouette clarity;
- separation between overlapping limbs, clothing and equipment;
- decisive directional lighting;
- strong cast and form shadows;
- readable facial planes and expressions;
- controlled shape simplification at gameplay scale.

Retain painterly handling mainly in:

- skin, cloth, leather and metal transitions;
- subtle hue variation inside large color areas;
- selected wear, scratches and material texture;
- softer edge changes away from focal areas;
- restrained secondary detail.

As a practical balance, use approximately `3–5` principal value groups rather than a strict two-tone light/shadow split. Keep the hardest and darkest contour accents around the outer silhouette, face, hands, weapon and important overlaps. Interior linework should be thinner, broken or partially replaced by value contrast.

Shadows should be strong and usually hard-edged, but not every shadow edge should be equally sharp. Reserve limited softer transitions for rounded anatomy, cloth folds, atmospheric depth and material changes.

---

## 2. Output requirements

Unless explicitly requested otherwise:

- output must be PNG;
- the final exported sprite canvas must be exactly `1536 × 1536` pixels;
- every pose in every character set must use that same canvas size;
- background must be transparent;
- preserve alpha transparency;
- no scenery;
- no floor;
- no environmental background;
- no decorative vignette;
- no frame;
- no text;
- no watermark;
- no artificial square background;
- never automatically crop the canvas to the visible character bounds;
- keep enough transparent padding to avoid clipping weapons, capes, limbs or hair;
- preserve a sprite composition suitable for use directly in Unity.

The resulting sprite should be usable without requiring background removal.

### 2.1 Standard canvas, ground line and pivot

All final character sprites must be normalized to the following layout:

- canvas size: exactly `1536 × 1536` pixels;
- coordinate reference: bottom-left origin;
- horizontal anchor: `x = 768` pixels;
- ground line: `y = 64` pixels above the bottom edge;
- Unity custom pivot: `X = 0.5`, `Y = 0.041667`;
- minimum preferred transparent safety margin: approximately `64` pixels on the top and sides whenever the pose allows it.

For grounded humanoids and creatures, the lowest supporting foot, paw, hoof, root or equivalent contact point should rest on the `y = 64` ground line.

The principal lower-body / foot anchor should remain aligned as closely as possible with `x = 768`. A wide action may extend to either side, but the character must not be recentered around the full visible silhouette if doing so would move the gameplay anchor.

For hovering or flying units, define an invisible gameplay ground point at `(768, 64)` and preserve the same vertical body offset from that point across every pose.

For a normal standing humanoid Base pose, aim for an approximate visible body height of `1180–1240` pixels from the ground line to the top of the head. This leaves additional room for raised weapons and extended action poses. Unusually large or small creatures may use a different body height, but every pose of that exact unit must use the same scale.

If a generated action does not fit inside the standard canvas at the established character scale, regenerate or recompose the pose. Do not shrink only that pose to make it fit.

---

## 3. Canonical character rule

Whenever a pose variant is requested, the original sprite of that exact character is the primary visual reference.

Do not recreate the character from textual description when the original sprite is available.

Before generating a new pose:

1. inspect the original character sprite;
2. inspect other approved poses of the same character, if they exist;
3. inspect nearby Black Breath character references only when necessary to understand the rendering style;
4. preserve the identity of the original above everything else.

The character must remain immediately recognizable between variants.

---

## 4. Elements that must remain consistent

Across all pose variants, preserve as closely as possible:

- face;
- apparent age;
- body proportions;
- body build;
- hairstyle;
- hair color;
- skin tone;
- clothing;
- armor;
- weapon;
- weapon size;
- accessories;
- colors;
- materials;
- silhouette identity;
- camera angle;
- perspective;
- rendering technique;
- level of detail;
- overall contrast;
- sprite scale;
- framing.

Do not invent, remove or replace equipment unless explicitly requested.

Do not redesign the character simply because another interpretation could look more dramatic.

---

## 5. Pose set

The standard character pose set contains five visual states.

### 5.1 Base / Idle

The canonical neutral combat pose.

It should communicate the character's identity and normal battlefield stance without implying an immediate action.

Use this pose as the principal reference for all other variants.

Avoid excessive motion.

---

### 5.2 Movement

A movement pose used while the unit changes position on the tactical grid.

The pose should:

- clearly suggest displacement;
- use a pronounced, immediately readable stride or weight shift that clearly distinguishes movement from the base pose;
- preserve the character's silhouette and equipment;
- remain grounded;
- avoid exaggerated sprinting unless appropriate for that character;
- work visually during a short sprite transition.

Movement should feel like battlefield repositioning rather than a long-distance running animation.

Keep the body anchor and scale close to the base pose.

Keep the head, torso and lower body oriented in a coherent direction. Avoid poses where the body faces one side while the head turns unnaturally toward the opposite side.

---

### 5.3 Melee Attack

A clear physical attack pose for melee actions.

The pose must reflect:

- the character's actual weapon;
- the way that weapon would plausibly be used;
- the character's physical style;
- the class fantasy already established by the game.

The action should be forceful and readable at gameplay scale.

Avoid:

- changing weapons;
- oversized motion arcs baked into the sprite unless explicitly requested;
- extreme poses that radically change the sprite footprint;
- cinematic camera-angle changes.

The pose should look like a strong moment within an attack, not a completely different illustration.

---

### 5.4 Alert / Reaction

This is a shared pose used for two gameplay situations:

- the unit is currently active and ready to act;
- the unit is currently being targeted or attacked.

The pose must therefore be intentionally ambiguous between offensive readiness and defensive awareness.

It should communicate:

- alertness;
- tension;
- readiness;
- awareness of immediate danger;
- focus.

Good visual cues include:

- slightly raised guard;
- more engaged posture;
- shifted body weight;
- weapon held ready;
- shoulders or torso slightly more tense;
- focused attention.

Avoid:

- obvious pain;
- visible injury;
- full recoil;
- a completed block;
- a full attack wind-up;
- falling backward;
- exaggerated surprise;
- visual effects implying that damage has already occurred.

The character should look like something is about to happen right now.

This pose must work equally well when the character is acting and when an enemy is acting against them.

---

### 5.5 Ability Pose

A characteristic action pose used when performing a special ability.

The exact body language depends on the character and ability type.

The pose should:

- reflect the character's class identity;
- remain compatible with the character's real equipment;
- feel distinct from the melee attack pose;
- communicate deliberate use of a special technique.

Do not invent:

- new weapons;
- new equipment;
- floating objects;
- spell effects;
- magical circles;
- particles;
- extra props

unless explicitly requested.

The default request is for the character pose itself, not for the VFX.

---

## 6. Sprite-to-sprite stability

Pose variants are intended to switch during gameplay.

They must therefore remain visually aligned.

Preserve exactly the same:

- `1536 × 1536` canvas dimensions;
- final Unity pivot: `X = 0.5`, `Y = 0.041667`;
- ground line at `y = 64` pixels;
- unit-specific scale established by the approved Base pose.

Preserve as closely as the action allows:

- visible character height;
- body center relative to the gameplay anchor;
- principal foot / lower-body anchor near `x = 768`;
- camera distance;
- transparent padding;
- overall visual mass.

Avoid visual jumps where the character appears to:

- teleport several pixels;
- suddenly become taller or shorter;
- change body width dramatically;
- move far left or right inside the canvas;
- change camera distance;
- change perspective.

When some displacement is necessary for an action pose, keep it controlled.

Do not independently resize or recenter each pose according to its visible bounds. The transparent canvas is a fixed coordinate space, not a framing guide. A weapon, arm or cape may extend farther in an action pose while the ground point and character scale remain stable.

The goal is not pixel-perfect skeletal animation, but the sprites should feel like different poses of the same physical character standing in the same battlefield space.

---

## 7. Reference hierarchy

When multiple visual references are available, use this priority:

1. original sprite of the exact character;
2. approved alternate poses of the exact character;
3. characters of the same class or visual family;
4. other approved Black Breath character sprites;
5. textual description.

Never allow a secondary reference to override the identity of the original character.

If one image is provided as a style reference and another as the character reference:

- character reference defines identity, clothing, proportions and equipment;
- style reference only informs rendering treatment.

---

## 8. Editing vs regeneration

When creating a pose variant from an existing character:

prefer image editing / transformation of the original sprite over recreating the character from scratch.

The goal is pose variation, not reinterpretation.

Regeneration from scratch should only be used when editing cannot produce a usable pose.

---

## 9. Generation workflow

For every new character pose:

1. identify the source character;
2. inspect the canonical sprite;
3. determine which of the five pose types is being requested;
4. preserve character identity and equipment;
5. generate the new pose;
6. verify transparent background;
7. establish the approved Base pose scale for the unit;
8. normalize the output to the exact `1536 × 1536` canvas without cropping visible content;
9. apply the same unit scale used by the Base pose;
10. align the supporting contact point to the ground line at `y = 64` and the principal lower-body anchor near `x = 768`;
11. verify the Unity pivot will be `X = 0.5`, `Y = 0.041667`;
12. compare scale, body placement and anchor against the approved Base pose and other variants;
13. check for accidental redesigns;
14. check weapon and clothing consistency;
15. save as a separate variant.

Canvas normalization is a required post-generation step. Do not rely on the image generator alone to produce exact dimensions or pixel alignment.

When producing a complete pose set, approve and normalize the Base pose first. Derive one unit-specific scale from that Base pose and reuse it unchanged for Move, Melee, Alert and Ability. Correct pose composition or regenerate an overflowing pose instead of reducing only that pose.

If background removal is performed manually, preserve the full `1536 × 1536` canvas during cleanup. Do not trim transparent borders. Recheck the ground line and anchor after background removal.

Never overwrite the canonical source sprite automatically.

---

## 10. Naming convention

When possible, use clear suffixes for generated variants.

Suggested convention:

- `_Base`
- `_Move`
- `_Melee`
- `_Alert`
- `_Ability`

Example:

`Purificadora_Olivia_Base.png`

`Purificadora_Olivia_Move.png`

`Purificadora_Olivia_Melee.png`

`Purificadora_Olivia_Alert.png`

`Purificadora_Olivia_Ability.png`

Do not invent a new naming convention when the project folder already uses an established one.

---

## 11. Staging

New AI-generated character poses should initially be saved in a staging location.

Do not automatically replace production sprites.

Recommended structure:

```text
Assets/
├── Art/
│   └── Characters/
└── _AI_GENERATED/
    └── Characters/
```

Production placement should happen only after the generated pose has been reviewed.

---

## 12. Final quality checklist

Before accepting a generated pose, verify:

- Is this unmistakably the same character?
- Is the face consistent?
- Is the equipment identical?
- Is the weapon correct?
- Are the proportions consistent?
- Is the camera angle consistent?
- Is the sprite scale compatible with the original?
- Is the final canvas exactly `1536 × 1536` pixels?
- Is the supporting contact point on the `y = 64` ground line?
- Is the principal lower-body anchor still near `x = 768`?
- Is the Unity pivot set to `X = 0.5`, `Y = 0.041667`?
- Was the same Base-derived scale used for every pose of this unit?
- Is every extended limb, weapon, cape or accessory fully inside the canvas with transparent safety padding?
- Does the pose clearly communicate its intended gameplay state?
- Does the Alert / Reaction pose remain ambiguous enough to work both when active and when attacked?
- Is there any accidental background?
- Is transparency preserved?
- Is there any sepia tint?
- Does the rendering remain between painterly and graphic rather than becoming fully photorealistic or fully cel-shaded?
- Are the shadows strong and readable without making every edge equally hard?
- Are contour accents selective and variable rather than uniformly thick?
- Did the generator add unnecessary props, VFX or ornamentation?
- Would switching to this sprite in Unity feel natural?

If any answer is unsatisfactory, revise the variant rather than accepting a visually impressive but inconsistent result.
