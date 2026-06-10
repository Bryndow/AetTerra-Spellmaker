Temporary Readme

### How to use
1. When prompted to open a file, Open an empty .txt file. You can simply create one from the Open File Dialog.
2. Enter a name for the Spell, and enter a sequence in the sequence tab. You'll see the Spellpoint ID to the right. If you mess up, you can press "Clear Sequence" to reset the Sequence.
3. You can then configure the Spell's properties. "Base Name" determines both, wether or not the Spell HAS a base, and what that Base Spell's name is. If left empty, the Spell is independant. "Use Base" will set the Spell to be a Base Spell. "Infinite" will set the spell to be continuously castable. "Use Target" will set the spell to automatically target the closest targetable object. "Drain" configures the mana drain. If "Infinite" is ticked, this will be mana drain/s. "States" will determine in which grounded states the Spell is castable. "Irrelevent" casts the Spell in the same way, regardless of grounded state. "Both Differing" creates conditions for either case. "Ground Only" and "Air Only" explain themselves.
4. Once your Spell is Named, Configured and has a Sequence accompanying it, you can press the "Pass In" button. This will allow you to create another Spell, at which point you start from step 2.
5. If you're done creating Spells, you can press "Write Data". This will fill the save file with new Spells. If you want to change a spell already present, you can replace it by creating a new Spell, with an identical name.
6. The whole point of this software is to create GDScript code. You can therefore select "Export As GDScript", to do just that. Select a folder you would like to create the 3 files in. ***ONLY SPELLS IN THE SAVE FILE WILL BE CONVERTED TO GDSCRIPT***
7. If you want to select a different file, or directory, you can press "Toss File", to reset both.
