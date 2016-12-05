## Synopsis

A sprite compiler that targets 16-bit 65816 assembly code on the Apple IIgs computer.  The sprite compiler uses informed search techniques to generate optimal code for whole-sprite rendering.

## Example

The compiler takes a simple masked, sparse byte sequence which are represented by (data, mask, offset) tuples.  During the search, it tracked the state of the 65816 CPU registers in order to find an optimal sequence if operations to generated the sprite data.  The space of possible actions are defined by the subclasses of the CodeSequence class.

Currently, the compile can only handle short, unmasked sequences, but it does correctly find the optimal code sequences.  Here is a sample of the code that the compiler generates 

### Data = $11 ###

```
	TCS       ; 2 cycles
	SEP	#$10  ; 3 cycles
	LDA	#$11  ; 2 cycles
	STA	00,s  ; 4 cycles
	REP	#$10  ; 3 cycles
; Total Cost = 14 cycles
```

### Data = $11 $22 ###

```
	TCS         ; 2 cycles
	LDA	#$2211  ; 3 cycles
	STA	00,s    ; 5 cycles
; Total Cost = 10 cycles
```

### Data = $11 $22 $11 $22 ###

```
	TCS         ; 2 cycles
	LDA	#$2211  ; 3 cycles
	STA	00,s    ; 5 cycles
	STA	02,s    ; 5 cycles
; Total Cost = 15 cycles
```

### Data = $11 $22 $33 $44 $55 $66 ###

```
	ADC	#5     ; 3 cycles
	TCS        ; 2 cycles
	PEA	$6655  ; 5 cycles
	PEA	$4433  ; 5 cycles
	PEA	$2211  ; 5 cycles
; Total Cost = 20 cycles
```

### Data = $11 $22 $11 $22 $11 $22 $11 $22 ###

```
	ADC	#7      ; 3 cycles
	TCS         ; 2 cycles
	LDA	#$2211  ; 3 cycles
	PHA         ; 4 cycles
	PHA         ; 4 cycles
	PHA         ; 4 cycles
	PHA         ; 4 cycles
; Total Cost = 24 cycles
```

## License

MIT License
