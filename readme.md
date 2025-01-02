# Lazyripent 2
A command line tool for mass ripenting.

This tool allows you to mass edit .map, .bsp, or .ent files.
Comes with a basic syntax to set up selectors and actions. See [Rule file syntax](#rule-file-syntax) and [Rule file cheatsheet](#rule-file-cheatsheet) sections. If you need a more advanced tool, please check out [Solokiller's MapUpgrader tool](https://github.com/twhl-community/HalfLife.UnifiedSdk-CSharp/tree/master)

This tool will attempt to automatically upgrade the following keyvalues:
```
angle yaw -> angles pitch=0 yaw roll=0
```

Unlike Lazyripent1 this does not require any external ripent executables, and is perfectly capable of processing bsp files on its own.

## Usage
This also prints when the application is ran without any options
```
Usage: lazyripent [options]

.ent files produced by this program are most likely incompatible with other ripent tools.

options:
-i arg  or --input arg           input may be a .rule file, .map file, .ent file, .bsp file, or a folder containing .map, .ent, or .bsp files
-o arg  or --output arg          output may be a .map file, .ent file, .bsp file, or a folder
-s arg  or --strip arg           strip output file(s) from default values as assigned in the .fgd file
-ee     or --export-ent-only     export .ent file(s) from .bsp file(s) only instead of applying rules
-ie     or --import-ent-only     import .ent file(s) to .bsp file(s) only instead of applying rules
-ss     or --strip-only          strip output file(s) only instead of applying rules
-u      or --unattended          automatically confirm changes instead of asking before applying them
-v      or --verbose             produce more verbose output
-w      or --warnings-as-fatal   treat all warnings as fatal and stop the program

examples:
lazyripent -i ./fixspawns.rule -i ./broken_bsp -o ./fixed_bsp
        apply fixspawns.rule ruleset to folder "broken_bsp" and write results out to folder "fixed_bsp"
lazyripent -ee -i ./map1.bsp -i ./map2.bsp -o ./ents
        export .ent files from both bsp files and write them out to folder "ents"
lazyripent -ss -s ./halflife.fgd -i ./map1.map -o ./map1.map -u
        strip map1.map file from fgd default keyvalue entries and write it back to the same file in unattended mode
        Note: fgd default entries may not match the game's code, so this may break the level
```

## Rule file syntax
This is a quick basic runthrough of the syntax.

A .rule file can be made up of one or more "rule blocks" indicated by the left and right braces. The rule syntax now ignores whitespace and newlines. a rule block must contain at least one or more selectors, actions, or both depending on the situation.

A basic rule block may look like this:
```
{
	match classname monster_scientist
	replace health 1000
}
```
This rule block would attempt to find all entities with the key `classname` of value `monster_scientist` (as defined by `match key value`) and replace their `health` key with the value `1000` (as defined by `replace key value`)

In some situations where the health key does not exist, the program will error out. In such cases you have to define your rule blocks with more precision like so:
```
{
	match classname monster_scientist
	have health
	replace health 1000
}
```
The addition of `have key` makes this rule block find all entities with the key `classname` of value `monster_scientist` which also have a key `health`.

Both of these keywords have a negated counterpart `dont-match` and `dont-have`, which will select entities that dont match the given key value combo, or dont have a given key.

Note: normally if your keys and values only contain letters, dashes (in the middle for strings, in the beginning for numerics), or underscores you are not required to surround it in quotes, however if you wish to use any sort of special character, whitespace, or variables you will need to surround it in quotes.

You may also limit the maps the rule block affects by adding a prefix keyword `map`:
```  
map my_awesome_map
{
	match classname monster_scientist
	have health
	replace health 1000
}  
```
This keyword can take multiple maps separated by whitespace.

Sometimes you may need to adjust spawnflags (bitflag fields) without replacing the entire value, that is to say you can clear or set bits without affecting the rest. This will require some level of understanding of how bitfields or spawnflags in goldsrc work.

Bit-set and bit-clear both take in a special type of value called bit-values as indicated by `b` followed by six zeros or ones. These values are binary, and are read from right to left, rightmost being `1`, and leftmost being `32`, with a total of `63` when all bits are set to one.
```
{
	match classname monster_scientist
	bit-clear spawnflags b000101
}
```
This rule block would clear the flags "Wait till seen" and "Monster clip" from `spawnflags`

And as you would guess:
```
{
	match classname monster_scientist
	bit-set spawnflags b000101
}
```
This rule block would set the flags "Wait till seen" and "Monster clip" from `spawnflags`

Also another new addition to Lazyripent2 is the ability to do basic arithmetic inside rule blocks:
```
{
	match classname monster_scientist
	add health 100
}
```
This rule block would add 100 to the existing value of `health`

Rule blocks are executed from top to bottom, so you can abuse this to do more slightly involved operations if necessary.

The way you refer to entity's existing keys has changed also:
```
{
	match clasname monster_scientist
	replace armor "{health}"
}
```
This rule block would take the value `armor` and place it in `health`

Storing values into global variables is also now possible:
```
{
	match classname monster_scientist
	store health scientist-health
}
{
	match classname monster_barney
	replace health "{global.scientist-health}"
}
```
This rule block would transfer the `health` from all `monster_scientist`s to all `monster_barney`s.

A global variable can be used at any point in the rule file after it has been stored at least once. Global variables are not shared between rule files.

Basic string concatenation is now possible:
```
{
	match classname monster_scientist
	replace displayname "{displayname} (really really angry)"
}
```

When `new-entity` is used a rule block is not required to use any selectors, however when used in conjunction with a selector the new-entity will loop for each matched entity, and all keys when referred to in values will point to the matched entity. In the case `remove-entity` is used, no actions are allowed to happen.

## Rule file cheatsheet
```
#this is a comment line
#entity variables are accessed through {key}
#global variables are accessed through {global.variable}

map name ...
{
	match key value
	dont-match key value
	have key
	dont-have key
	
	replace key value
	remove key
	new key value
	rename key key
	bit-set key b000000
	bit-clear key b000000
	add key value
	sub key value
	mult key value
	div key value
	store key variable
	new-entity classname
	remove-entity
}
```
