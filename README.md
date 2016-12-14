## Synopsis

A sprite compiler that targets 16-bit 65816 assembly code on the Apple IIgs computer.  The sprite compiler uses informed search techniques to generate optimal code for whole-sprite rendering.

## Example

The compiler takes a simple masked, sparse byte sequence which are represented by (data, mask, offset) tuples.  During the search, it tracks the state of the 65816 CPU registers in order to find an optimal sequence of operations to generated the sprite data.  The space of possible actions are defined by the subclasses of the CodeSequence class.

Currently, the compiler can only handle short, unmasked sequences, but it does correctly find optimal code sequences.  Here is a sample of the code that the compiler generates 

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

### Data = $11 $22 $22

```
        TCS             ; 2 cycles
        LDA     #$2222  ; 3 cycles
        STA     02,s    ; 5 cycles
        LDA     #$2211  ; 3 cycles
        STA     01,s    ; 5 cycles
; Total Cost = 18 cycles
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
### Data = ($11, 0), ($11, 160), ($11, 320) ###

A simple sprite three lines tall.

```
	TCS             ; 2 cycles
	SEP     #$10    ; 3 cycles
	LDA     #$11    ; 2 cycles
	PHA             ; 3 cycles
	STA     A1,s    ; 4 cycles
	REP     #$10    ; 3 cycles
	TSC             ; 2 cycles
	ADC     #321    ; 3 cycles
	TCS             ; 2 cycles
	SEP     #$10    ; 3 cycles
	LDA     #$11    ; 2 cycles
	PHA             ; 3 cycles
	REP     #$10    ; 3 cycles
; Total Cost = 35 cycles

```

## Limitations ##

The search is quite memory intensive and grows too fast to handle multi-line sprite data yet.  Future versions will
incorporate more aggressive heuristic and Iterative Deepening A-Star search to mitigate the memory usage.

## License

MIT License
